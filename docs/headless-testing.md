# Headless Testing

Render widgets and frames to strings or structured cells without touching the console. Deterministic and CI‑friendly.

Common helpers
- `Headless.RenderParagraph(width,height, paragraph)`
- `Headless.RenderList(width,height, list)` / `Headless.RenderListState(width,height, list, state)`
- `Headless.RenderTable(width,height, table)`
- `Headless.RenderGauge/Tabs/BarChart/Sparkline/Scrollbar/Chart(..., widget)`
- `Headless.RenderFrame(width,height, params DrawCommand[])`
- `Headless.RenderFrameStyles(width,height, ...)` and `RenderFrameStylesEx(...)`
- `Headless.RenderFrameCells(width,height, ...)` → returns `CellInfo[] { ch, fg, bg, mods }`
- `Headless.RenderClear(width,height)`

Tips
- Use styles_ex or structured cells to assert detailed styling; plain text is best for quick snapshots.
- Normalize line endings in your test harness for cross‑platform snapshots.
- For complex frames, prefer a handful of “golden” tests over lots of small overlapping ones.

Related
- Batching: build larger frames efficiently — see batching-and-spans.md
- Ergonomics: RAII terminal patterns and App runners — see terminal-and-app.md

