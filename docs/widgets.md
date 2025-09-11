# Widgets Quick Guide

This page highlights the most useful, zero‑cost helpers per widget, with links to batching where relevant.

Table
- Widths: `WidthsPercentages(span)`, `Widths(ReadOnlySpan<(kind,value)>)`, presets `WidthsEven(columns)`, `WidthsAbsolute(span)`, `WidthsMin(span)`.
- Styles: `HeaderStyle`, `ColumnHighlightStyle`, `CellHighlightStyle`, `HighlightSpacing`.
- Batching: `Headers(spans)`, `AppendRow(spans)`, `AppendRows(rows→cells→lines)`.
- Stateful draw: `Terminal.Draw(table, rect, TableState)`.

Tabs
- Titles via spans: `AddTitleSpans(spans)`, `SetTitlesSpans(lines)`.
- Styles: `Styles(unselected, selected, divider)`, `Divider(string)`, `TitleAlignment(Alignment)`.

Chart
- Datasets: `Line(name, points, style)`, `Dataset(name, points, style, kind)`.
- Bulk datasets: `Datasets((name, points[], style, kind)...)` (single FFI call).
- Labels via spans: `XLabels(lines)`, `YLabels(lines)`.
- Axis: `AxisStyles(xStyle, yStyle)`, `LabelsAlignment(xAlign, yAlign)`, `Legend(pos)`, `Style(style)`, `HiddenLegendConstraints(...)`.

Scrollbar
- Configure: `Orientation`, `Position`, `ContentLength`, `ViewportLength`.
- Orientation side and title alignment: `OrientationSide(Side)`, `TitleAlignment(Alignment)`.
- UX helpers: `LineUp/Down`, `PageUp/Down`, `ScrollToStart/End`, `EnsureVisible(index)`.

Canvas / LineGauge
- Canvas: `Bounds`, `Background`, `Marker`, `Title`, `WithBlock(adv)`, `AddLine/Rect/Points`.
- LineGauge: `Ratio`, `Label`, `Style`, and draw via `Terminal.Draw`.

List / Paragraph
- List: append styled items with `AppendItem(spans)`; batch multiple via `AppendItems(lines)`; stateful draw via `Terminal.Draw(list, rect, ListState)`.
- Paragraph: `AppendSpans` and `AppendLineSpans` for styled content with no extra allocations.

Related
- Batching details and patterns — see batching-and-spans.md
- Terminal RAII and App runners — see terminal-and-app.md

