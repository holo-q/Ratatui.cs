Ratatui.cs — C# bindings for Ratatui (Rust TUI)

![CI](https://github.com/holo-q/Ratatui.cs/actions/workflows/ci.yml/badge.svg)
[![Guard](https://github.com/holo-q/Ratatui.cs/actions/workflows/guard.yml/badge.svg)](https://github.com/holo-q/Ratatui.cs/actions/workflows/guard.yml)
[![GitHub Release](https://img.shields.io/github/v/release/holo-q/Ratatui.cs?logo=github)](https://github.com/holo-q/Ratatui.cs/releases)
[![NuGet](https://img.shields.io/nuget/v/Ratatui.cs.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Ratatui.cs)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ratatui.cs.svg?logo=nuget)](https://www.nuget.org/packages/Ratatui.cs)

What this is
- Native Rust cdylib (`native/ratatui_ffi`) with a stable C ABI over Ratatui.
- Idiomatic .NET library (`src/Ratatui`) with SafeHandles and builders.
- Headless snapshot rendering for CI and smoke tests (no terminal required).

Supported today
- Widgets: Paragraph, List (stateful selection), Table (stateful selection), Gauge, Tabs, BarChart, Sparkline, Scrollbar.
- Events: Keyboard, Mouse (down/up/drag/move/scroll + mods), Resize.
- Rendering: Draw into rects, batched DrawFrame for multiple widgets.
- Testing: Headless renderers for each widget + composite frames; event injection for deterministic flows.
- Ergonomics: Vec2i swizzles (x/y/xy/yx/pos), Rect x/y/w/h aliases; simple C# layout split helpers.

Prereqs
- Rust stable + Cargo
- .NET SDK 9.0+

Local dev quickstart
1) Build native
   - `cd native/ratatui_ffi && cargo build` (or `cargo build --release`)
2) Run examples
   - Interactive: `dotnet run --project examples/Hello/Hello.csproj`
   - Headless snapshots: `dotnet run --project examples/Smoke/Smoke.csproj`
   - Composite headless: `dotnet run --project examples/SmokeFrame/SmokeFrame.csproj`
   - If you see DllNotFoundException, point the loader to your native build:
     - `export RATATUI_FFI_DIR=$(pwd)/native/ratatui_ffi/target/debug` (or `.../release`)
     - Or set `RATATUI_FFI_PATH` to the full path of the built library (`libratatui_ffi.so`/`.dylib` or `ratatui_ffi.dll`).

Using in your app (source)
- Add a ProjectReference to `src/Ratatui/Ratatui.csproj`.
- Ensure the native library is discoverable (Resolver searches `runtimes/<rid>/native/` and common dev `target/` paths). You can override with `RATATUI_FFI_DIR` env var.

Using via NuGet (CI-packaged)
- This repo includes a GitHub Actions workflow that builds the native library for:
  - linux-x64, win-x64, osx-x64, osx-arm64
- It then packs a NuGet with RID-specific native assets under `runtimes/<rid>/native/`.
- After a successful run, download the `.nupkg` from the workflow artifacts or releases and reference it in your project.

Publishing and feeds
- GitHub Packages (default): the CI publishes on tags (vX.Y.Z) and on main/master to `https://nuget.pkg.github.com/holo-q/index.json`.
  - Add the source (once):
    - `dotnet nuget add source https://nuget.pkg.github.com/holo-q/index.json -n holo-q -u <github-username> -p <github-personal-access-token> --store-password-in-clear-text`
  - Install: `dotnet add package Ratatui.cs --version <x.y.z>`
- nuget.org (optional): if `NUGET_API_KEY` is set in repo secrets, tags publish to nuget.org as well.

API sketch
```csharp
using Ratatui;

using var term = new Terminal();
using var p = new Paragraph("Hello from C#").Title("Demo");
var (w,h) = term.Size();
term.Draw(p, new Rect(0,0,w,h));

// Batched frame
term.DrawFrame(
  DrawCommand.Paragraph(p, new Rect(0,0,w/2,h)),
  DrawCommand.Gauge(new Gauge().Ratio(0.42f).Title("Load"), new Rect(w/2,0,w/2,3))
);
```

Headless snapshots (CI-friendly)
```csharp
using Ratatui.Testing;
using var table = new Table().Title("T").Headers("A","B").AppendRow("1","2");
var snapshot = Headless.RenderTable(30, 6, table);
Console.WriteLine(snapshot);
```

CI: Build native + pack NuGet
- Workflow: `.github/workflows/ci.yml`
- Jobs:
  - Build native per RID (linux-x64, win-x64, osx-x64, osx-arm64) and upload artifacts.
  - Pack job downloads all artifacts, copies them into `src/Ratatui/runtimes/<rid>/native/`, and runs `dotnet pack`.
  - Outputs a `.nupkg` under `artifacts/`; publishes to GitHub Packages on tags and main; optionally to nuget.org on tags.

Notes
- Native library name: `ratatui_ffi` (`libratatui_ffi.so`/`.dylib` or `ratatui_ffi.dll`).
- Loader search order: application base, `runtimes/<rid>/native/`, dev `native/ratatui_ffi/target/{debug,release}`, or `RATATUI_FFI_DIR`.
- Layout: simple C# helpers in `Ratatui.Layout`. For full parity, use Rect math or add your own helpers as needed.
