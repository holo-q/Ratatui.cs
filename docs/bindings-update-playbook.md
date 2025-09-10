# Bindings Update Playbook (C#, Python, TypeScript)

This document gives clear, practical steps for updating/creating bindings for ratatui_ffi in three languages. It is designed for parallel execution by separate agents. Follow the steps in order; each section includes type mappings, function coverage, examples, and validation.

Key goals
- Full functional parity: expose everything reachable via FFI (widgets, layout, style, terminal, canvas, batching, headless).
- Stable interop: flat C ABI, simple structs/enums, explicit memory ownership.
- Automated validation: snapshot renders (text + styles + cells) and symbol link-through checks using the introspector JSON.

Library names
- Linux: `libratatui_ffi.so`
- macOS: `libratatui_ffi.dylib`
- Windows: `ratatui_ffi.dll`

Feature/Version detection
- `ratatui_ffi_version(out_major, out_minor, out_patch)` → crate version.
- `ratatui_ffi_feature_bits()` → bitmask (SCROLLBAR, CANVAS, STYLE_DUMP_EX, BATCH_TABLE_ROWS, BATCH_LIST_ITEMS, COLOR_HELPERS, AXIS_LABELS).

Introspection for coverage
- Text: `scripts/ffi_introspect.sh`
- JSON: `cargo run --quiet --bin ffi_introspect -- --json`
  - Use this list to ensure every export has a matching binding entry.

Types you must map
- `FfiRect { u16 x, y, width, height }`
- `FfiStyle { u32 fg, bg; u16 mods }` (colors: 0=reset; named=1..16; RGB=0x80000000|R<<16|G<<8|B; Indexed=0x40000000|idx)
- `FfiSpan { const char* text_utf8; FfiStyle style }`
- `FfiLineSpans { const FfiSpan* spans; size_t len }` (used for batching titles/items/lines)
- `FfiCellLines { const FfiLineSpans* lines; size_t len }` (for multi-line table cells)
- `FfiRowCellsLines { const FfiCellLines* cells; size_t len }` (for batched table rows)
- `FfiCellInfo { u32 ch, fg, bg; u16 mods }` (headless cell dump)
- Enum-like integers:
  - Alignment: 0=Left,1=Center,2=Right
  - Direction (layout): 0=Vertical,1=Horizontal
  - Borders bitflags: LEFT=1, RIGHT=2, TOP=4, BOTTOM=8 (combine)
  - BorderType: 0=Plain,1=Thick,2=Double
  - List HighlightSpacing, Tabs styles (integers passed via setters)

Memory ownership
- Strings returned from FFI (e.g., headless text) must be freed via `ratatui_string_free`.
- Handles from `*_new()` must be freed via `*_free()`.
- Headless buffers you allocate (e.g., `FfiCellInfo[]`) are owned by the caller.

High‑value APIs to cover (non‑exhaustive; derive full list from introspector JSON)
- Terminal: init/free/clear; draw widgets in rect; batched `ratatui_terminal_draw_frame`; raw/alt toggles; cursor get/set/show; size; `ratatui_next_event`.
- Paragraph: new/new_empty; append spans/lines; base style, align, wrap(trim), scroll; block title/_adv; title alignment.
- List/Table/Tabs: items/headers/rows/titles; span-based variants; highlight/selection; state structs (ListState/TableState); block adv; title alignment; widths/spacing/row height.
- Gauge/LineGauge/BarChart/Sparkline/Chart/Scrollbar/Clear/Logo/Canvas: create/configure/draw/headless; styles; block adv + title alignment; chart datasets/labels/bounds/styles/legend; scrollbar orientation/side.
- Layout: `layout_split`, `layout_split_ex` (spacing + per-side margins), `layout_split_ex2` (adds `Constraint::Ratio`).
- Headless: render single widgets; frame render; style dumps (compact + EX); structured cell dump (`ratatui_headless_render_frame_cells`).
- Helpers: color helpers (rgb/indexed); reserve helpers; batching appends.

Validation strategy (all languages)
1) Link‑through check
   - Load the library; dynamically query the JSON from `ffi_introspect` and assert your binding has entries for every export name (allow known platform-specific omissions like nm/naming if needed).
2) Snapshot tests
   - Compose a representative frame: Paragraph/List/Table/Tabs/Gauge/Chart/Scrollbar/Canvas with block titles, alignment, per-span styling, ratios layout.
   - Use `ratatui_headless_render_frame` for text, and `ratatui_headless_render_frame_styles_ex` + `ratatui_headless_render_frame_cells` for styles.
   - Compare against golden outputs (allow platform EOL normalization).

---

## C# Binding Agent

Interop style: P/Invoke with `DllImport` and `StructLayout(LayoutKind.Sequential)`. Use `SafeHandle` or `IntPtr` wrappers for widget handles. Provide a loader that resolves the correct library name per OS.

Minimal interop structs
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct FfiRect { public ushort x, y, width, height; }

[StructLayout(LayoutKind.Sequential)]
public struct FfiStyle { public uint fg, bg; public ushort mods; }

[StructLayout(LayoutKind.Sequential)]
public struct FfiSpan { public IntPtr text_utf8; public FfiStyle style; }

[StructLayout(LayoutKind.Sequential)]
public struct FfiLineSpans { public IntPtr spans; public UIntPtr len; }

[StructLayout(LayoutKind.Sequential)]
public struct FfiCellInfo { public uint ch, fg, bg; public ushort mods; }
```

Key imports (sample; expand from JSON)
```csharp
static class Native {
  const string Lib = "ratatui_ffi"; // resolver maps to .dll/.so/.dylib

  [DllImport(Lib)] public static extern IntPtr ratatui_init_terminal();
  [DllImport(Lib)] public static extern void   ratatui_terminal_free(IntPtr t);
  [DllImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
  public static extern bool ratatui_terminal_draw_frame(IntPtr t, IntPtr cmds, UIntPtr len);

  [DllImport(Lib)] public static extern IntPtr ratatui_paragraph_new_empty();
  [DllImport(Lib)] public static extern void   ratatui_paragraph_free(IntPtr p);
  [DllImport(Lib)] public static extern void   ratatui_paragraph_append_spans(IntPtr p, IntPtr spans, UIntPtr len);
  [DllImport(Lib)] public static extern void   ratatui_paragraph_set_alignment(IntPtr p, uint align);
  [DllImport(Lib)] public static extern void   ratatui_paragraph_set_block_title_alignment(IntPtr p, uint align);

  [DllImport(Lib)] public static extern void   ratatui_layout_split_ex2(ushort w, ushort h, uint dir,
                                 IntPtr kinds, IntPtr valsA, IntPtr valsB, UIntPtr len,
                                 ushort spacing, ushort ml, ushort mt, ushort mr, ushort mb,
                                 IntPtr outRects, UIntPtr cap);

  [DllImport(Lib)] [return: MarshalAs(UnmanagedType.I1)]
  public static extern bool ratatui_headless_render_frame_cells(ushort w, ushort h,
                                IntPtr cmds, UIntPtr len, IntPtr outCells, UIntPtr cap);

  [DllImport(Lib)] public static extern void   ratatui_string_free(IntPtr s);
}
```

Notes
- For `char*` inputs, pin UTF-8 strings (e.g., `Encoding.UTF8.GetBytes(...)` + `fixed`) and pass pointers.
- For out strings from headless APIs, marshal `IntPtr` to UTF-8 string, then call `ratatui_string_free`.
- Build helpers for color encoding (mirror the FFI bit layout) or call `ratatui_color_rgb/indexed`.
- Create a thin OO layer that owns IntPtrs and arranges frame commands.

Validation
- Implement a test rendering composing multiple widgets and run text, styles_ex, and cells snapshot comparisons.
- Add a small check that compares your DllImport set with `ffi_introspect --json` function names.

---

## Python Binding Agent

Interop style: `ctypes` (or `cffi`). Provide a loader resolving platform library names and paths. Ensure UTF-8 handling and lifetime management for strings.

ctypes types
```python
from ctypes import *

class FfiRect(Structure):
    _fields_ = [("x", c_ushort), ("y", c_ushort), ("width", c_ushort), ("height", c_ushort)]

class FfiStyle(Structure):
    _fields_ = [("fg", c_uint), ("bg", c_uint), ("mods", c_ushort)]

class FfiSpan(Structure):
    _fields_ = [("text_utf8", c_char_p), ("style", FfiStyle)]

class FfiLineSpans(Structure):
    _fields_ = [("spans", POINTER(FfiSpan)), ("len", c_size_t)]

class FfiCellInfo(Structure):
    _fields_ = [("ch", c_uint), ("fg", c_uint), ("bg", c_uint), ("mods", c_ushort)]
```

Function prototypes (sample)
```python
lib = CDLL("libratatui_ffi.so")  # resolve per OS

lib.ratatui_init_terminal.restype = c_void_p
lib.ratatui_terminal_free.argtypes = [c_void_p]

lib.ratatui_paragraph_new_empty.restype = c_void_p
lib.ratatui_paragraph_free.argtypes = [c_void_p]
lib.ratatui_paragraph_append_spans.argtypes = [c_void_p, POINTER(FfiSpan), c_size_t]
lib.ratatui_paragraph_set_alignment.argtypes = [c_void_p, c_uint]
lib.ratatui_paragraph_set_block_title_alignment.argtypes = [c_void_p, c_uint]

lib.ratatui_layout_split_ex2.argtypes = [c_ushort,c_ushort,c_uint,
    POINTER(c_uint),POINTER(c_ushort),POINTER(c_ushort),c_size_t,
    c_ushort,c_ushort,c_ushort,c_ushort,c_ushort,
    POINTER(FfiRect),c_size_t]

lib.ratatui_headless_render_frame_cells.argtypes = [c_ushort,c_ushort,c_void_p,c_size_t,POINTER(FfiCellInfo),c_size_t]
lib.ratatui_headless_render_frame_cells.restype  = c_size_t

lib.ratatui_string_free.argtypes = [c_char_p]
```

Notes
- Use `create_unicode_buffer(...).value.encode("utf-8")` for text or manually build `c_char_p` from `bytes`.
- When receiving a `char* out`, wrap it as `c_char_p`, decode, then call `ratatui_string_free` with the original pointer.
- Batch helpers: construct arrays of `FfiSpan`/`FfiLineSpans` and pass pointers.

Validation
- Compose a frame and compare text, styles_ex, and cells against goldens.
- Compare function coverage: parse names from `ffi_introspect --json` and ensure every name has a callable in your module.

---

## TypeScript Binding Agent

Interop style: `ffi-napi` + `ref-napi` + `ref-struct-di` (or N-API if you prefer compiled addons). Show both shapes but implement with ffi-napi for speed.

Types (ffi-napi/ref)
```ts
import ref from 'ref-napi';
import StructDi from 'ref-struct-di';
const Struct = StructDi(ref);

const ushort = ref.types.uint16;
const uint   = ref.types.uint32;
const size_t = ref.types.size_t;
const voidPtr = ref.refType(ref.types.void);

export const FfiRect = Struct({ x: ushort, y: ushort, width: ushort, height: ushort });
export const FfiStyle = Struct({ fg: uint, bg: uint, mods: ref.types.uint16 });
export const FfiSpan  = Struct({ text_utf8: ref.types.CString, style: FfiStyle });
export const FfiLineSpans = Struct({ spans: ref.refType(FfiSpan), len: size_t });
export const FfiCellInfo  = Struct({ ch: uint, fg: uint, bg: uint, mods: ref.types.uint16 });
```

Functions (sample)
```ts
import ffi from 'ffi-napi';
const lib = ffi.Library(process.platform === 'win32' ? 'ratatui_ffi' : 'libratatui_ffi', {
  ratatui_init_terminal: [ voidPtr, [] ],
  ratatui_terminal_free: [ 'void', [ voidPtr ] ],
  ratatui_paragraph_new_empty: [ voidPtr, [] ],
  ratatui_paragraph_free: [ 'void', [ voidPtr ] ],
  ratatui_paragraph_append_spans: [ 'void', [ voidPtr, ref.refType(FfiSpan), size_t ] ],
  ratatui_paragraph_set_alignment: [ 'void', [ voidPtr, 'uint' ] ],
  ratatui_paragraph_set_block_title_alignment: [ 'void', [ voidPtr, 'uint' ] ],
  ratatui_layout_split_ex2: [ 'void', [ 'uint16','uint16','uint',
    ref.refType('uint'), ref.refType('uint16'), ref.refType('uint16'), size_t,
    'uint16','uint16','uint16','uint16','uint16', ref.refType(FfiRect), size_t ] ],
  ratatui_headless_render_frame_cells: [ 'size_t', [ 'uint16','uint16', voidPtr, size_t, ref.refType(FfiCellInfo), size_t ] ],
  ratatui_string_free: [ 'void', [ ref.types.CString ] ],
});
```

Notes
- Encode strings to UTF-8 (`Buffer.from(str, 'utf8')`) for `CString` inputs; ensure lifetime until the call returns.
- For outputs, if using APIs returning text via `char**`, you’ll need a helper wrapper in JS to read the pointer from a Buffer (or add small native glue). Prefer the structured cells dump to avoid manual `char**` shenanigans.
- Consider shipping prebuilt binaries and loading per-platform.

Validation
- Implement the standard frame snapshot with text + styles_ex + cells.
- Compare export names from `ffi_introspect --json` to your registered functions.

---

## Quick Checklist (for all agents)

- Load correct library per OS.
- Map core structs/enums exactly (sequential layout, correct widths).
- Implement color helpers or call FFI helpers.
- Cover batch APIs (paragraph lines, list items, table rows, datasets) and reserve helpers where relevant.
- Use `ratatui_ffi_version` and `ratatui_ffi_feature_bits` to adapt if needed.
- Snapshot validate: headless text + styles_ex + cells.
- Diff binding symbol names with `ffi_introspect --json` and report unknown/missing.

Happy hacking!

---

Notes for this repository
- The FFI crate is vendored as a submodule at `native/ratatui-ffi`. Its helper scripts live under `native/ratatui-ffi/scripts`. For convenience, a wrapper `scripts/ffi_introspect.sh` is provided at the root scripts directory.
- The current C# interop lives under `src/Ratatui/Interop/Native.cs` and adjacent files. When upgrading the bindings, use the introspector JSON to verify coverage and add any newly exported functions.

