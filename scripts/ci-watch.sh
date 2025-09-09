#!/usr/bin/env bash
set -euo pipefail

# ci-watch.sh — Push (optional), then attach to the latest run of a workflow
# and block until completion. On failure, print failing job logs.

usage() {
  cat <<EOF
Usage: $0 [-w workflow_name_or_file] [--no-push] [--branch <name>] [--ref <sha>]

Defaults:
  -w ci.yml
  --branch: current branch
  --ref:    current HEAD sha

Examples:
  $0                # push current branch, watch latest ci.yml run for HEAD
  $0 --no-push      # do not push, just attach to the latest run for HEAD
  $0 -w snapshots-on-ci.yml --no-push
EOF
}

workflow="ci.yml"
do_push=1
branch="$(git rev-parse --abbrev-ref HEAD)"
sha="$(git rev-parse HEAD)"

while [[ $# -gt 0 ]]; do
  case "$1" in
    -w|--workflow) workflow="$2"; shift 2;;
    --no-push) do_push=0; shift;;
    --branch) branch="$2"; shift 2;;
    --ref) sha="$2"; shift 2;;
    -h|--help) usage; exit 0;;
    *) echo "Unknown arg: $1" >&2; usage; exit 2;;
  esac
done

if [[ $do_push -eq 1 ]]; then
  echo "[ci-watch] Pushing branch $branch..."
  git push origin "$branch"
fi

echo "[ci-watch] Waiting for workflow '$workflow' run for sha $sha..."
run_id=""
for i in {1..60}; do
  run_id=$(gh run list --workflow "$workflow" --limit 20 --json databaseId,headSha -q \
    "map(select(.headSha == \"$sha\")) | .[0].databaseId" 2>/dev/null || true)
  [[ -n "${run_id:-}" && "$run_id" != "null" ]] && break
  sleep 2
done

if [[ -z "${run_id:-}" || "$run_id" == "null" ]]; then
  echo "[ci-watch] No run found for $sha on workflow $workflow (timed out)." >&2
  exit 1
fi

echo "[ci-watch] Attaching to run: $run_id"
gh run watch "$run_id" --exit-status

# Summarize jobs and print logs for failures
echo "[ci-watch] Run complete. Summarizing jobs..."
json=$(gh run view "$run_id" --json conclusion,status,jobs)
echo "$json" | jq -r '.jobs[] | "- " + .name + ": " + (.conclusion // .status)'

fail_ids=$(echo "$json" | jq -r '.jobs[] | select(.conclusion=="failure" or .conclusion=="cancelled" or .conclusion=="timed_out") | .databaseId')
if [[ -n "${fail_ids:-}" ]]; then
  echo "[ci-watch] Printing logs for failing jobs..."
  while read -r jid; do
    [[ -z "$jid" ]] && continue
    echo "===== JOB $jid ====="
    gh run view "$run_id" --job "$jid" --log || true
    echo "===== END JOB $jid ====="
  done <<< "$fail_ids"
else
  echo "[ci-watch] All jobs succeeded."
fi

