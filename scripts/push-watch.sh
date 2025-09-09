#!/usr/bin/env bash
set -euo pipefail

# push-watch.sh — Augment `git push` by waiting for GitHub Actions to finish.
#
# What it does
# - Optionally pushes the current branch to a remote.
# - Detects the matching workflow run for the pushed commit.
# - Streams high‑level status until completion and exits with CI status.
# - On failure, prints failing job logs and (optionally) reruns failed jobs.
#
# This is an augmentation of `git push` (not a replacement). It’s safe to use
# anywhere you’d use `git push` when you also want CI feedback in the terminal.
#
# Quick setup (optional):
#   git config alias.pushw '!scripts/push-watch.sh -- '
# Then run:
#   git pushw         # push + watch default workflow
#   git pushw --no-push  # only watch the current HEAD’s run
#
# Requirements: gh, git, jq in PATH; repo hosted on GitHub.

usage() {
  cat <<EOF
Usage: push-watch.sh [options]

Options:
  -w, --workflow <name|file>   Workflow to watch (auto-detect by default)
  -r, --remote <name>          Git remote to push (default: origin)
  -b, --branch <name>          Branch to push (default: current)
      --repo <owner/name>      Override repository to watch (default: current)
      --no-push                Do not push; just attach to HEAD run
      --open                   Open the run in the browser as it starts
      --rerun-failed           If CI fails, rerun failed jobs once
      --poll <secs>            Polling interval for status (default: 3)
  -h, --help                   Show help

Examples:
  push-watch.sh                       # push current branch, auto-detect workflow
  push-watch.sh -w ci.yml --no-push   # attach to latest run for HEAD
  push-watch.sh -w build.yml -r upstream -b main

Auto-detect rules:
  - Prefer .github/workflows/ci.yml
  - Otherwise first of: ci-*.yml, build*.yml, test*.yml
EOF
}

remote="origin"
branch="$(git rev-parse --abbrev-ref HEAD)"
sha="$(git rev-parse HEAD)"
repo_override=""
workflow=""
do_push=1
open_web=0
rerun_failed=0
poll=3

while [[ $# -gt 0 ]]; do
  case "$1" in
    -w|--workflow) workflow="$2"; shift 2;;
    -r|--remote) remote="$2"; shift 2;;
    -b|--branch) branch="$2"; shift 2;;
    --repo) repo_override="$2"; shift 2;;
    --no-push) do_push=0; shift;;
    --open) open_web=1; shift;;
    --rerun-failed) rerun_failed=1; shift;;
    --poll) poll="$2"; shift 2;;
    -h|--help) usage; exit 0;;
    *) echo "Unknown arg: $1" >&2; usage; exit 2;;
  esac
done

# Determine repo
repo="$(gh repo view --json nameWithOwner -q .nameWithOwner 2>/dev/null || true)"
if [[ -n "$repo_override" ]]; then repo="$repo_override"; fi
if [[ -z "$repo" ]]; then
  echo "[push-watch] Not a GitHub repo (gh repo view failed)." >&2
  exit 1
fi

# Auto-detect workflow file/name if not provided
if [[ -z "$workflow" ]]; then
  candidates=(
    ".github/workflows/ci.yml"
    $(ls .github/workflows/ci-*.yml 2>/dev/null || true)
    $(ls .github/workflows/build*.yml 2>/dev/null || true)
    $(ls .github/workflows/test*.yml 2>/dev/null || true)
  )
  for c in "${candidates[@]}"; do
    [[ -f "$c" ]] && workflow="$(basename "$c")" && break
  done
fi
if [[ -z "$workflow" ]]; then
  echo "[push-watch] Could not auto-detect a workflow; specify with -w." >&2
  exit 1
fi

if [[ $do_push -eq 1 ]]; then
  echo "[push-watch] git push $remote $branch (repo: $repo)"
  git push "$remote" "$branch"
  sha="$(git rev-parse HEAD)"
else
  echo "[push-watch] No push; watching HEAD $sha on $repo"
fi

echo "[push-watch] Waiting for workflow '$workflow' run for $sha ..."

# Find run ID for this SHA
run_id=""
for i in {1..120}; do
  run_id=$(GH_REPO="$repo" gh run list --workflow "$workflow" --limit 30 \
           --json databaseId,headSha -q "map(select(.headSha == \"$sha\")) | .[0].databaseId" 2>/dev/null || true)
  [[ -n "${run_id:-}" && "$run_id" != "null" ]] && break
  sleep "$poll"
done
if [[ -z "${run_id:-}" || "$run_id" == "null" ]]; then
  echo "[push-watch] Timed out waiting for run of $workflow on $sha." >&2
  exit 1
fi

run_url="$(GH_REPO="$repo" gh run view "$run_id" --json url -q .url)"
echo "[push-watch] Attached to $run_url"
if [[ $open_web -eq 1 ]]; then
  GH_REPO="$repo" gh run view "$run_id" --web >/dev/null 2>&1 || true
fi

# Watch until completion (gh will exit nonzero on failure)
set +e
GH_REPO="$repo" gh run watch "$run_id"
rc=$?
set -e

# Summarize jobs and print logs for failures
conclusion="$(GH_REPO="$repo" gh run view "$run_id" --json conclusion -q .conclusion)"
jobs_json="$(GH_REPO="$repo" gh run view "$run_id" --json jobs)"
echo "[push-watch] Conclusion: ${conclusion:-unknown}"
echo "$jobs_json" | jq -r '.jobs[] | "- " + .name + ": " + (.conclusion // .status)'

if [[ "$conclusion" != "success" ]]; then
  echo "[push-watch] Printing logs for failing jobs..."
  echo "$jobs_json" | jq -r '.jobs[] | select(.conclusion!="success") | .databaseId' | while read -r jid; do
    [[ -z "$jid" ]] && continue
    echo "===== JOB $jid ====="
    GH_REPO="$repo" gh run view "$run_id" --job "$jid" --log || true
    echo "===== END JOB $jid ====="
  done
  if [[ $rerun_failed -eq 1 ]]; then
    echo "[push-watch] Rerunning failed jobs..."
    GH_REPO="$repo" gh run rerun "$run_id" --failed || true
  fi
fi

exit $rc

