#!/usr/bin/env bash
set -euo pipefail

# Rewrites history to remove build outputs and vendor cache without losing logical history.
# Requires: git-filter-repo (https://github.com/newren/git-filter-repo)

if ! command -v git-filter-repo >/dev/null 2>&1; then
  echo "git-filter-repo not found. Install via: pip install git-filter-repo" >&2
  exit 1
fi

echo "This will rewrite history. Ensure you have a backup remote or clone." >&2
read -p "Type 'yes' to continue: " ans
if [ "${ans:-}" != "yes" ]; then
  echo "Aborted." >&2
  exit 1
fi

# Paths to drop across all commits
git filter-repo --force --invert-paths \
  --path native/ratatui_ffi/target \
  --path-glob "examples/*/bin" \
  --path-glob "examples/*/obj" \
  --path src/Ratatui/bin \
  --path src/Ratatui/obj \
  --path src/Ratatui/runtimes/ \
  --path-glob "**/.DS_Store" \
  --path-glob "**/Thumbs.db"

echo "Running aggressive GC..."
git gc --prune=now --aggressive

echo "Done. Force-push your branch and tags when ready:"
echo "  git push --force --tags origin "+$(git branch --show-current)

