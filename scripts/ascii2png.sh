#!/usr/bin/env bash
set -euo pipefail

SRC_DIR=${1:-artifacts/snapshots}
OUT_DIR=${2:-artifacts/snapshots}
FONT=${FONT:-DejaVu-Sans-Mono}
POINT=${POINT:-16}
COLOR=${COLOR:-white}
BG=${BG:-black}

shopt -s nullglob
for f in "${SRC_DIR}"/*.txt; do
  base=$(basename "$f" .txt)
  out="${OUT_DIR}/${base}.png"
  echo "[ascii2png] $f -> $out"
  # Read content, strip CRs and any UTF-8 BOM, escape quotes, pass inline
  content=$(tr -d '\r' < "$f" | sed $'1s/^\xEF\xBB\xBF//' | sed 's/"/\\"/g')
  convert -font "$FONT" -pointsize "$POINT" -fill "$COLOR" -background "$BG" label:"$content" "$out"
done
