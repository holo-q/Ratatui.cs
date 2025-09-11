# Batching and Spans

High throughput in Ratatui.cs comes from sending many styled runs in one interop call. The batching helpers do the heavy lifting without allocations on hot paths.

Core idea
- Pack many (utf8, style) runs into a single contiguous buffer and a flat `FfiSpan[]`, then call the native batch API once. For nested shapes (lines/cells/rows), build flat arrays of `FfiLineSpans` and `FfiRowCellsLines` that reference the same utf8 buffer.

API surface
- `Batching.WithFfiSpans(ReadOnlySpan<SpanRun> runs, Action<IntPtr,UIntPtr> call)`
- `Batching.WithFfiLineSpans(ReadOnlySpan<ReadOnlyMemory<SpanRun>> lines, Action<IntPtr,UIntPtr> call)`
- `Batching.WithFfiRowsCellsLines(ReadOnlySpan<Row> rows, Action<IntPtr,UIntPtr> call)`

Widget shortcuts (thin wrappers over the above)
- Paragraph: `AppendSpans`, `AppendLineSpans`
- List: `AppendItem(ReadOnlySpan<SpanRun>)`, `AppendItems(ReadOnlySpan<ReadOnlyMemory<SpanRun>>)`
- Tabs: `AddTitleSpans`, `SetTitlesSpans(ReadOnlySpan<ReadOnlyMemory<SpanRun>>)`
- Table: `AppendRow(ReadOnlySpan<SpanRun>)`, `AppendRows(ReadOnlySpan<Row>)`, `Headers(ReadOnlySpan<SpanRun>)`

Performance notes
- Small batches use `stackalloc`; larger batches rent from `ArrayPool<T>` and return immediately after the call.
- No persistent allocations; pointers remain valid only for the call duration.
- For frequently reused content (e.g., static headers), cache your UTF‑8 bytes and SpanRuns to avoid repeated encodes.

Related
- Headless frame wrappers: dump styles/cells to validate expected styling.
- Widgets quick guide: see widgets.md for Table/Tabs/Chart specifics.

