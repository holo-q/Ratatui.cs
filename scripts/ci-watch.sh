#!/usr/bin/env bash
set -euo pipefail

# Backward-compat wrapper: prefer scripts/push-watch.sh
exec "$(dirname "$0")/push-watch.sh" -w ci.yml "$@"
