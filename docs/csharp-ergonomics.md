# Ratatui.cs — C# Ergonomics and Performance Guide

This guide highlights the C#-only conveniences layered on top of the 1:1 FFI. The goal is to make Ratatui.cs pleasant to use while preserving the original Ratatui performance profile: zero or near‑zero allocations on hot paths, explicit ownership, and predictable behavior.

- Interop layer: flat C ABI, explicit lifetimes, no surprises.
- C# layer: SafeHandles, fluent APIs, Span‑friendly batching, RAII for terminal toggles, uniform BlockAdv borders/padding/title alignment, and headless testing helpers.

## Performance Essentials

- Favor Span/ReadOnlySpan overloads and batched methods to minimize allocations and interop calls.
- Value types for small data: `Rect`, `Style`, `Padding`, `BlockAdv` are structs; pass by `in` where hot.
- Builders/batching avoid per‑item P/Invoke costs. Use a single call to send many spans/lines/rows.
- ArrayPool and stackalloc: wrappers use stackalloc for small bursts and ArrayPool for larger buffers.
- SafeHandle + IDisposable ensure native lifetimes are released deterministically without tracking.

## Terminal RAII (Raw/Alt/Cursor/Viewport)

```csharp
using var term = new Terminal()
    .Raw(true)
    .AltScreen(true)
    .ShowCursor(false);

// draw...
var (w, h) = term.Size();
term.Viewport = new Rect(0, 0, w, h);
term.SetCursor(0, 0);
// terminal state automatically unwinds on Dispose()
```

- `Raw(on)`, `AltScreen(on)`, `ShowCursor(show)`, `SetCursor/GetCursor()`, and `Viewport { get; set; }` map directly to native toggles.
- Dispose restores cursor → leaves alt → disables raw (in that order).

## BlockAdv (Borders, Padding, Title Alignment)

`BlockAdv` provides a single, uniform way to set borders/padding/title alignment across widgets with zero extra allocations.

```csharp
var adv = new BlockAdv(
    Borders.All,
    BorderType.Plain,
    Padding.All(1),
    Alignment.Center);

para.WithBlock(adv);
list.WithBlock(adv);
table.WithBlock(adv);
tabs.WithBlock(adv);
gauge.WithBlock(adv);
barchart.WithBlock(adv);
sparkline.WithBlock(adv);
scrollbar.WithBlock(adv);
```

- Internally calls native `*_set_block_adv` + `*_set_block_title_alignment`.
- Use span‑based title setters for per‑character styling when needed.

## Styles and Colors

- `Style` struct uses named colors and modifier flags for zero‑cost encoding.
- For RGB/indexed colors, you can call the native helpers and plug raw values into FFI when needed:

```csharp
var rgb = Interop.Native.RatatuiColorRgb(0x33, 0x66, 0x99);
var idx = Interop.Native.RatatuiColorIndexed(123);
```

## Batching and Spans (Lists, Paragraphs, Tables, Tabs)

- Prefer batching: append many items/lines/spans in one call to reduce interop overhead.
- Span‑based helpers allow passing pre‑encoded UTF‑8 text as `ReadOnlySpan<byte>` without allocating strings.
- Tables support multi‑line cells and full rows batching via `FfiRowCellsLines` shape.

Tip: When building larger batched structures, consider precomputing UTF‑8 into pooled buffers (ArrayPool) and reusing them across frames for peak performance.

## Layout Helpers

- Use managed `Layout` helpers for common splits (rows/columns/even splits), or call native `layout_split(_ex|_ex2)` for precise control (spacing, per‑side margins, ratios).

```csharp
var parent = new Rect(0, 0, 120, 30);
var (left, right) = parent.SplitVertical(60);
```

## Headless Snapshot Testing

- Deterministic, allocation‑light snapshot rendering for widgets and composite frames.

```csharp
using Ratatui.Testing;
var ascii = Headless.RenderFrame(80, 24,
    DrawCommand.Paragraph(para, new Rect(0,0,40,5)),
    DrawCommand.Table(table, new Rect(0,6,80,10))
);
```

- Also available: widget‑level helpers (`RenderParagraph`, `RenderList`, `RenderTable`, `RenderGauge`, `RenderTabs`, `RenderBarChart`, `RenderSparkline`, `RenderScrollbar`, `RenderChart`).
- For style introspection or cell dumps, use native `headless_render_frame_styles(_ex)` and `headless_render_frame_cells` via the interop layer.

## Object Model and Lifetimes

- Every native widget has a SafeHandle and a small disposable wrapper class; dispose patterns are consistent and cheap.
- Avoid long‑lived unmanaged text; prefer passing spans or strings each call; the engine copies as needed.

## When to Drop to Native

- The C# layer aims to be zero‑cost; if you need something not yet exposed, use `Interop.Native` directly. If a pattern turns out to be common, open an issue/PR — we’ll add a first‑class wrapper and keep it allocation‑free.

## Roadmap (ergonomics with zero overhead)

- Span‑first batch marshaling helpers for Paragraph/List/Tabs/Table (spans → `FfiSpan`/`FfiLineSpans` via stackalloc/ArrayPool).
- Canvas/LineGauge wrappers mirroring the rest of the API surface.
- Optional source generator for `DllImport` parity driven by the introspector JSON.

---

Questions or ideas? Open a discussion/issue and we’ll iterate — carefully, with performance as the first constraint.

