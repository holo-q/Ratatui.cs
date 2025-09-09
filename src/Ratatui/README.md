# Ratatui.cs

C# bindings for Ratatui (Rust TUI). Provides core widgets, input events, frame batching, and headless rendering for tests. Native code is shipped per-RID and loaded automatically.

Getting started

- Install: `dotnet add package Ratatui.cs`
- Example:

```
using Ratatui;

using var term = new Terminal();
term.Clear();

using var para = new Paragraph("Hello from Ratatui.cs!").Title("Demo");
term.DrawIn(para, new Rect(0, 0, 40, 5));
```

Headless testing

- Render any widget or a composed frame to a string image for snapshot tests:

```
var ascii = Headless.Render(new Rect(0,0,20,5), DrawCommand.Paragraph(para, new Rect(0,0,20,5)));
```

Native loading

- The package includes native libraries under `runtimes/<rid>/native`.
- If needed, override with env vars:
  - `RATATUI_FFI_DIR` (directory containing the native lib)
  - `RATATUI_FFI_PATH` (full path to the native lib)

Links

- Repo: https://github.com/holo-q/Ratatui.cs
- FFI (Rust): https://github.com/holo-q/ratatui-ffi
