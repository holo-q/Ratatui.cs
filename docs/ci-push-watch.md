Push + Watch CI (git push augmentation)

Overview
- push-watch.sh wraps a normal `git push` and attaches to the corresponding GitHub Actions run for the pushed commit. It blocks until CI completes and surfaces failures inline (including failing job logs).

Why
- Save the context switch: you push, then see CI succeed/fail without tab-hopping or polling.
- Fail-fast: failing job logs are printed right away.

Install
- Put `scripts/push-watch.sh` on your PATH (e.g., `~/bin`), or keep it in this repo.
- Optional git alias:
  - `git config alias.pushw '!scripts/push-watch.sh -- '`
  - Then use `git pushw` in place of `git push` when you want CI feedback.

Usage
- `scripts/push-watch.sh` (auto-detects workflow, pushes current branch)
- `scripts/push-watch.sh --no-push` (only attaches to HEAD’s run)
- `scripts/push-watch.sh -w ci.yml -r origin -b main`

Options
- `-w, --workflow <name|file>`: Workflow name/file to watch. Auto-detect prefers `.github/workflows/ci.yml` then `ci-*.yml`, `build*.yml`, `test*.yml`.
- `-r, --remote <name>`: Remote to push (default `origin`).
- `-b, --branch <name>`: Branch to push (default current branch).
- `--repo <owner/name>`: Target repo to watch (default current).
- `--no-push`: Attaches without pushing.
- `--open`: Opens the run in a browser tab.
- `--rerun-failed`: If CI fails, reruns failed jobs once.
- `--poll <secs>`: Polling interval for run discovery (default 3s).

What command it augments
- It augments `git push` by adding “wait for CI and print results”. It does not replace `git push` — continue using `git push` normally; use `git pushw` when you want CI feedback inline.

Requirements
- gh CLI (logged in), jq, git.

