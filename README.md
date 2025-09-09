Ratatui.cs — C# bindings for Ratatui

Overview
- Native Rust cdylib (`native/ratatui_ffi`) exposes a small, stable C ABI over Ratatui.
- .NET library (`src/Ratatui`) provides SafeHandle-based wrappers and idiomatic API.
- Example (`examples/Hello`) renders a titled paragraph.

Build prerequisites
- Rust toolchain (stable) and Cargo.
- .NET SDK 9.0+.

Build and run (dev)
1) Build native library
   - `cd native/ratatui_ffi && cargo build`
2) Run example (resolver auto-loads the native lib from target/)
   - `dotnet run --project examples/Hello/Hello.csproj`

Notes
- The native library name is `ratatui_ffi` (`libratatui_ffi.so`/`.dylib` or `ratatui_ffi.dll`).
- The C# loader searches common dev paths; you can override with `RATATUI_FFI_DIR` env var (point it to the directory containing the native library).
- Current surface: Terminal init/restore, Paragraph creation, simple draw. Next: layout, styles, lists, tables, events, and batched frames.

