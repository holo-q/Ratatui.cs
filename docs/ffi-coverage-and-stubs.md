# FFI Coverage, Suggestions, and Stubs

Ratatui.cs enforces 100% parity with the native ratatui_ffi exports at build time and provides helpers to speed up upgrades.

What runs on build
- Coverage check: scans native/ratatui-ffi/src for `#[no_mangle] extern "C" fn ratatui_*` names and diffs against our C# DllImports (EntryPoint names). The build fails on missing or stale.
- Suggestions (comments): writes a `Native.Suggestions.g.cs` file (comments only) under `obj/...` listing missing/stale, plus template `[DllImport]` stubs.
- Stubs (compile‑ready): on .NET 9 builds, the tool can write `Native.Generated.g.cs` with minimal partial interop stubs so you can compile instantly while you refine signatures. Our hand‑written Native.cs stays the source of truth for optimized signatures.

CLI equivalents (useful locally)
- Quick coverage: `dotnet run --project tools/BindingCoverage/BindingCoverage.csproj -- --assembly src/Ratatui/bin/Debug/net9.0/Ratatui.dll --project-dir src/Ratatui`
- Emit suggestions: add `--emit-suggestions obj/Debug/net9.0/Native.Suggestions.g.cs`
- Emit stub file: add `--emit-generated obj/Debug/net9.0/Native.Generated.g.cs`

FAQ
- Why both suggestions and stubs? Suggestions are human‑readable. Stubs unblock compilation on day‑zero FFI changes; you replace them with precise signatures shortly after. Coverage still fails if anything is missing or stale in the final DllImports.
- Where are the “real” DllImports? `src/Ratatui/Interop/Native.cs` (marked `partial` to accommodate generated stubs). We keep them hand‑written and optimized (correct marshalling, arrays vs IntPtr, `[MarshalAs(I1)]` for bool, `UIntPtr` for size_t, etc.).

Notes
- The coverage tool operates on source (Rust and C#) — it doesn’t need to load the native library.
- You can override the loader via `RATATUI_FFI_DIR` or `RATATUI_FFI_PATH` when running examples or tests.

