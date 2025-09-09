#!/usr/bin/env bash
set -euo pipefail

# Fail CI if build artifacts or native outputs are tracked in the diff.

paths=(
  'native/ratatui_ffi/target'
  'src/Ratatui/bin'
  'src/Ratatui/obj'
  'src/Ratatui/runtimes'
  'examples/*/bin'
  'examples/*/obj'
)

bad=0
for p in "${paths[@]}"; do
  if git ls-files -z "$p" 2>/dev/null | grep -qz .; then
    echo "Tracked files found under: $p" >&2
    bad=1
  fi
done

if [ $bad -ne 0 ]; then
  echo "Build artifacts should not be committed. See .gitignore." >&2
  exit 1
fi

echo "No build artifacts tracked."

