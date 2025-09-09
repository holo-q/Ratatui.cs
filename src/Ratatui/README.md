Ratatui.cs — .NET/C# Terminal UI (TUI) powered by Rust Ratatui

[![NuGet](https://img.shields.io/nuget/v/Ratatui.cs.svg)](https://www.nuget.org/packages/Ratatui.cs)
[![CI](https://github.com/holo-q/Ratatui.cs/actions/workflows/ci.yml/badge.svg)](https://github.com/holo-q/Ratatui.cs/actions/workflows/ci.yml)
[![FFI crate](https://img.shields.io/crates/v/ratatui_ffi.svg)](https://crates.io/crates/ratatui_ffi)

High-performance, cross-platform Terminal UI for .NET and C#. Exposes the Rust Ratatui engine over FFI with a friendly, idiomatic C# API. Build rich console apps with widgets, layout, keyboard/mouse events, batched frame rendering, and headless snapshot testing.

Features

- Core widgets: Paragraph, List, Table, Gauge, Tabs, BarChart, Sparkline, Chart
- Terminal helpers: init/clear, draw-in-rect, batched frame drawing
- Events: key, mouse, resize; injection for automation/tests
- Styles: colors + modifiers; simple layout helpers and rect math
- Headless rendering: snapshot widgets or full frames to ASCII
- Cross-platform native loading: robust resolver + env var overrides

Install

- `dotnet add package Ratatui.cs`

Quickstart

Interactive draw-in-rect

```
using Ratatui;

using var term = new Terminal();
term.Clear();

using var para = new Paragraph("Hello from Ratatui.cs!\nThis is Ratatui via Rust FFI.")
    .Title("Demo");

term.DrawIn(para, new Rect(2, 1, 44, 6));
```

Headless snapshot (tests)

```
using Ratatui;

using var para = new Paragraph("Snapshot me").Title("Test");
var ascii = Headless.Render(
    new Rect(0, 0, 24, 5),
    DrawCommand.Paragraph(para, new Rect(0, 0, 24, 5))
);
// Assert on `ascii` or save to file for golden tests
```

Batched frame (multiple widgets)

```
using var list = new List().Title("Items").Items("A", "B", "C");
using var chart = new Chart().Title("Line").Line("L1", new [] { (0.0,1.0), (1.0,2.0) });

var rect = new Rect(0, 0, 60, 12);
var left = new Rect(0, 0, 30, rect.Height);
var right = new Rect(30, 0, 30, rect.Height);

var ascii = Headless.Render(
    rect,
    DrawCommand.List(list, left),
    DrawCommand.Chart(chart, right)
);
```

Events

```
await foreach (var evt in term.Events())
{
    if (evt is KeyEvent k && k.Code == KeyCode.Esc) break;
}
```

Native loading

- Ships native libs under `runtimes/<rid>/native`.
- Resolver also searches local dev build outputs and supports overrides:
  - `RATATUI_FFI_DIR` = directory containing the native library
  - `RATATUI_FFI_PATH` = full path to the library file

Supported RIDs

- linux-x64, win-x64, osx-x64, osx-arm64

Troubleshooting

- DllNotFoundException: set `RATATUI_FFI_DIR` to where your native lib lives, or ensure your app deploys `runtimes/<rid>/native/*ratatui_ffi*` alongside your binaries.
- Headless rendering returns UTF-8 text; ensure your asserts treat it as UTF-8.

Links

- Repo: https://github.com/holo-q/Ratatui.cs
- FFI (Rust): https://github.com/holo-q/ratatui-ffi
- Ratatui (upstream): https://github.com/ratatui-org/ratatui

License

MIT
