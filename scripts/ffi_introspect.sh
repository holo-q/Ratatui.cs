#!/usr/bin/env bash
set -euo pipefail

# Forwarder to the vendored FFI crate's introspection helper.
# Usage:
#   scripts/ffi_introspect.sh            # pretty text output
#   scripts/ffi_introspect.sh --json     # JSON export list

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
FFI_DIR="$ROOT_DIR/native/ratatui-ffi"
SCRIPT="$FFI_DIR/scripts/ffi_introspect.sh"

if [[ ! -f "$SCRIPT" ]]; then
  echo "ffi_introspect script not found at: $SCRIPT" >&2
  exit 1
fi

exec "$SCRIPT" "$@"

