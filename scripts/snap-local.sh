#!/usr/bin/env bash
set -euo pipefail

# snap-local.sh — Generate local snapshots (ASCII + PNG) without CI
# - Builds native library
# - Drops it into the Snapshots sample runtimes/<rid>/native
# - Runs the Snapshots example to write artifacts/snapshots/*.txt
# - Converts to PNG via scripts/ascii2png.sh

RID=""
case "$(uname -s)" in
  Linux*)   arch=$(uname -m); RID=$([[ "$arch" == "aarch64" || "$arch" == "arm64" ]] && echo linux-arm64 || echo linux-x64);;
  Darwin*)  arch=$(uname -m); RID=$([[ "$arch" == "arm64" ]] && echo osx-arm64 || echo osx-x64);;
  MINGW*|MSYS*|CYGWIN*) arch=$(uname -m); RID=$([[ "$arch" == "aarch64" || "$arch" == "arm64" ]] && echo win-arm64 || echo win-x64);;
  *) echo "Unsupported OS" >&2; exit 1;;
esac

echo "[snap-local] Building native (release)…"
pushd native/ratatui_ffi >/dev/null
cargo build --release
popd >/dev/null

mkdir -p examples/Snapshots/runtimes/$RID/native
case "$RID" in
  linux-*) cp native/ratatui_ffi/target/release/libratatui_ffi.so examples/Snapshots/runtimes/$RID/native/ ;;
  osx-*)   cp native/ratatui_ffi/target/release/libratatui_ffi.dylib examples/Snapshots/runtimes/$RID/native/ ;;
  win-*)   cp native/ratatui_ffi/target/release/ratatui_ffi.dll examples/Snapshots/runtimes/$RID/native/ ;;
esac

echo "[snap-local] Running Snapshots example…"
dotnet run --project examples/Snapshots/Snapshots.csproj -c Release

echo "[snap-local] Converting to PNG…"
./scripts/ascii2png.sh artifacts/snapshots artifacts/snapshots

echo "[snap-local] Done. Open artifacts/snapshots in your image viewer."

