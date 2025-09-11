using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace Ratatui.Interop;

internal static class Native
{
    private const string LibraryName = "ratatui_ffi";

    // Register the resolver as early as possible. Also exposed for ModuleInitializer.
    internal static void EnsureResolver()
    {
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(Native).Assembly, Resolve);
            // Trigger load and version check once
            RatatuiFfiVersion(out _, out _, out _);
            ValidateVersion();
        }
        catch
        {
            // Ignore if already set for this assembly
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        // Always ensure resolver is hooked when first used.
        // Safe to call repeatedly due to try/catch in EnsureResolver.
        EnsureResolver();
        if (!string.Equals(libraryName, LibraryName, StringComparison.Ordinal))
            return IntPtr.Zero;

        // Allow override via env var (directory containing the native library)
        var envDir = Environment.GetEnvironmentVariable("RATATUI_FFI_DIR");
        if (!string.IsNullOrEmpty(envDir))
        {
            var candidate = Path.Combine(envDir, GetPlatformLibraryFileName(LibraryName));
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle))
                return handle;
        }
        // Optional direct file override
        var envPath = Environment.GetEnvironmentVariable("RATATUI_FFI_PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            var full = Path.GetFullPath(envPath);
            if (File.Exists(full) && NativeLibrary.TryLoad(full, out var handle))
                return handle;
        }

        // Search common dev output locations relative to the managed assembly.
        var baseDir = AppContext.BaseDirectory;
        var fileName = GetPlatformLibraryFileName(LibraryName);
        var candidates = new List<string>();
        // Standard locations
        candidates.Add(Path.Combine(baseDir, fileName));
        candidates.Add(Path.Combine(baseDir, "runtimes", GetRid(), "native", fileName));
        // Try all known rid folders if specific rid didn't match
        var runtimesDir = Path.Combine(baseDir, "runtimes");
        if (Directory.Exists(runtimesDir))
        {
            foreach (var ridDir in Directory.EnumerateDirectories(runtimesDir))
            {
                var cand = Path.Combine(ridDir, "native", fileName);
                candidates.Add(cand);
            }
        }
        // Dev builds from the repo: climb up to 6 levels just in case
        for (int up = 3; up <= 6; up++)
        {
            var ups = new string[up];
            Array.Fill(ups, "..");
            var debugPath = Path.Combine(new[] { baseDir }.Concat(ups).Concat(new[] { "native", "ratatui_ffi", "target", "debug", fileName }).ToArray());
            var releasePath = Path.Combine(new[] { baseDir }.Concat(ups).Concat(new[] { "native", "ratatui_ffi", "target", "release", fileName }).ToArray());
            candidates.Add(debugPath);
            candidates.Add(releasePath);
        }

        foreach (var path in candidates)
        {
            var full = Path.GetFullPath(path);
            if (File.Exists(full) && NativeLibrary.TryLoad(full, out var handle))
                return handle;
        }

        return IntPtr.Zero; // fall back to default loader resolution
    }

    private static string GetPlatformLibraryFileName(string baseName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return baseName + ".dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "lib" + baseName + ".dylib";
        return "lib" + baseName + ".so";
    }

    private static string GetRid()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "win-arm64" : (Environment.Is64BitProcess ? "win-x64" : "win-x86");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
        return "linux-x64";
    }

    [DllImport(LibraryName, EntryPoint = "ratatui_ffi_version", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiFfiVersion(out uint major, out uint minor, out uint patch);

    [DllImport(LibraryName, EntryPoint = "ratatui_ffi_feature_bits", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint RatatuiFfiFeatureBits();

    private static void ValidateVersion()
    {
        if (!RatatuiFfiVersion(out var maj, out var min, out var pat))
        {
            throw new DllNotFoundException("ratatui_ffi version query failed; ensure compatible native library present.");
        }
        // Optionally, enforce a minimum version if needed in the future.
        _ = maj; _ = min; _ = pat;
    }

    [DllImport(LibraryName, EntryPoint = "ratatui_init_terminal", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiInitTerminal();

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_clear", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTerminalClear(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTerminalFree(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiParagraphNew([MarshalAs(UnmanagedType.LPUTF8Str)] string text);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockTitle(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetAlignment(IntPtr para, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_wrap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetWrap(IntPtr para, [MarshalAs(UnmanagedType.I1)] bool trim);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphFree(IntPtr para);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_paragraph", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawParagraph(IntPtr term, IntPtr para);

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiRect { public ushort X, Y, Width, Height; }

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_paragraph_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawParagraphIn(IntPtr term, IntPtr para, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_size", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalSize(out ushort width, out ushort height);

    internal enum FfiEventKind : uint { None = 0, Key = 1, Resize = 2, Mouse = 3 }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiKeyEvent { public uint Code; public uint Ch; public byte Mods; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiEvent { public uint Kind; public FfiKeyEvent Key; public ushort Width; public ushort Height; public ushort MouseX; public ushort MouseY; public uint MouseKind; public uint MouseBtn; public byte MouseMods; }

    [DllImport(LibraryName, EntryPoint = "ratatui_next_event", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiNextEvent(ulong timeoutMs, out FfiEvent ev);

    // Styles
    internal enum FfiColor : uint { Reset = 0, Black, Red, Green, Yellow, Blue, Magenta, Cyan, Gray, DarkGray, LightRed, LightGreen, LightYellow, LightBlue, LightMagenta, LightCyan, White }
    [Flags]
    internal enum FfiStyleMods : ushort { None = 0, Bold = 1<<0, Italic = 1<<1, Underline = 1<<2, Dim = 1<<3, Crossed = 1<<4, Reversed = 1<<5, RapidBlink = 1<<6, SlowBlink = 1<<7 }
    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiStyle { public uint Fg; public uint Bg; public ushort Mods; }

    // Paragraph content builders
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_line", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendLine(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);


    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_span", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendSpan(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);


    // ===== Helpers =====
    [DllImport(LibraryName, EntryPoint = "ratatui_color_rgb", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint RatatuiColorRgb(byte r, byte g, byte b);
    [DllImport(LibraryName, EntryPoint = "ratatui_color_indexed", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint RatatuiColorIndexed(byte index);

    // ===== Layout =====
    [DllImport(LibraryName, EntryPoint = "ratatui_layout_split", CallingConvention = CallingConvention.Cdecl)]
    internal static extern UIntPtr RatatuiLayoutSplit(ushort width, ushort height, uint dir,
        IntPtr kinds, IntPtr values, UIntPtr len,
        ushort ml, ushort mt, ushort mr, ushort mb,
        IntPtr outRects, UIntPtr outCap);
    [DllImport(LibraryName, EntryPoint = "ratatui_layout_split_ex", CallingConvention = CallingConvention.Cdecl)]
    internal static extern UIntPtr RatatuiLayoutSplitEx(ushort width, ushort height, uint dir,
        IntPtr kinds, IntPtr values, UIntPtr len,
        ushort spacing, ushort ml, ushort mt, ushort mr, ushort mb,
        IntPtr outRects, UIntPtr outCap);
    [DllImport(LibraryName, EntryPoint = "ratatui_layout_split_ex2", CallingConvention = CallingConvention.Cdecl)]
    internal static extern UIntPtr RatatuiLayoutSplitEx2(ushort width, ushort height, uint dir,
        IntPtr kinds, IntPtr valuesA, IntPtr valuesB, UIntPtr len,
        ushort spacing, ushort ml, ushort mt, ushort mr, ushort mb,
        IntPtr outRects, UIntPtr outCap);

    // ===== Headless Frame =====
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrame(ushort width, ushort height, IntPtr cmds, UIntPtr len, out IntPtr utf8Text);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame_styles", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrameStyles(ushort width, ushort height, IntPtr cmds, UIntPtr len, out IntPtr utf8Text);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame_styles_ex", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrameStylesEx(ushort width, ushort height, IntPtr cmds, UIntPtr len, out IntPtr utf8Text);
    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiCellInfo { public uint Ch, Fg, Bg; public ushort Mods; }
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame_cells", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrameCells(ushort width, ushort height, IntPtr cmds, UIntPtr len, IntPtr outCells, UIntPtr cap);
    // ratatui_string_free already declared earlier in file

    // ===== Terminal toggles / cursor / viewport =====
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_enable_raw", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalEnableRaw(IntPtr term);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_disable_raw", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDisableRaw(IntPtr term);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_enter_alt", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalEnterAlt(IntPtr term);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_leave_alt", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalLeaveAlt(IntPtr term);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_set_cursor_position", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalSetCursorPosition(IntPtr term, ushort x, ushort y);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_get_cursor_position", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalGetCursorPosition(IntPtr term, out ushort x, out ushort y);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_show_cursor", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalShowCursor(IntPtr term, [MarshalAs(UnmanagedType.I1)] bool show);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_set_viewport_area", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalSetViewportArea(IntPtr term, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_get_viewport_area", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalGetViewportArea(IntPtr term, out FfiRect rect);

    // ===== Paragraph extended/batched =====
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_new_empty", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiParagraphNewEmpty();
    [StructLayout(LayoutKind.Sequential)] internal struct FfiSpan { public IntPtr TextUtf8; public FfiStyle Style; }
    [StructLayout(LayoutKind.Sequential)] internal struct FfiLineSpans { public IntPtr Spans; public UIntPtr Len; }
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendSpans(IntPtr para, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_line_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendLineSpans(IntPtr para, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_lines_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendLinesSpans(IntPtr para, IntPtr lines, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_line_break", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphLineBreak(IntPtr para);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_reserve_lines", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphReserveLines(IntPtr para, UIntPtr additional);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetStyle(IntPtr para, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_scroll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetScroll(IntPtr para, ushort x, ushort y);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockAdv(IntPtr para, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockTitleAlignment(IntPtr para, uint align);

    // ===== List and ListState =====
    [DllImport(LibraryName, EntryPoint = "ratatui_list_reserve_items", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListReserveItems(IntPtr lst, UIntPtr additional);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_item_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItemSpans(IntPtr lst, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_items_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItemsSpans(IntPtr lst, IntPtr lines, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_direction", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetDirection(IntPtr lst, uint direction);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_highlight_spacing", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetHighlightSpacing(IntPtr lst, uint spacing);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_scroll_offset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetScrollOffset(IntPtr lst, ushort offset);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockAdv(IntPtr lst, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockTitleAlignment(IntPtr lst, uint align);
    // ListState
    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiListStateNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListStateFree(IntPtr st);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListStateSetSelected(IntPtr st, int selected);
    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_set_offset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListStateSetOffset(IntPtr st, UIntPtr offset);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_list_state_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawListStateIn(IntPtr term, IntPtr lst, FfiRect rect, IntPtr state);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_list_state", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderListState(ushort width, ushort height, IntPtr lst, IntPtr state, out IntPtr utf8Text);

    // ===== Table + TableState =====
    [StructLayout(LayoutKind.Sequential)] internal struct FfiCellLines { public IntPtr Lines; public UIntPtr Len; }
    [StructLayout(LayoutKind.Sequential)] internal struct FfiRowCellsLines { public IntPtr Cells; public UIntPtr Len; }
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_headers_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeadersSpans(IntPtr tbl, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowSpans(IntPtr tbl, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row_cells_lines", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowCellsLines(IntPtr tbl, IntPtr cells, UIntPtr cellCount);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_rows_cells_lines", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowsCellsLines(IntPtr tbl, IntPtr rows, UIntPtr rowCount);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_reserve_rows", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableReserveRows(IntPtr tbl, UIntPtr additional);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_widths_percentages", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetWidthsPercentages(IntPtr tbl, ushort[] widths, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_widths", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetWidths(IntPtr tbl, uint[] kinds, ushort[] values, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_column_spacing", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetColumnSpacing(IntPtr tbl, ushort spacing);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_row_height", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetRowHeight(IntPtr tbl, ushort height);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_header_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeaderStyle(IntPtr tbl, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_column_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetColumnHighlightStyle(IntPtr tbl, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_cell_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetCellHighlightStyle(IntPtr tbl, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_highlight_spacing", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHighlightSpacing(IntPtr tbl, uint spacing);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockAdv(IntPtr tbl, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockTitleAlignment(IntPtr tbl, uint align);
    // TableState
    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiTableStateNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableStateFree(IntPtr st);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableStateSetSelected(IntPtr st, int selected);
    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_set_offset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableStateSetOffset(IntPtr st, UIntPtr offset);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_table_state_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawTableStateIn(IntPtr term, IntPtr tbl, FfiRect rect, IntPtr state);

    // ===== Tabs =====
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_add_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsAddTitleSpans(IntPtr t, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_clear_titles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsClearTitles(IntPtr t);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_titles_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetTitlesSpans(IntPtr t, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_divider", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetDivider(IntPtr t, [MarshalAs(UnmanagedType.LPUTF8Str)] string dividerUtf8);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetStyles(IntPtr t, FfiStyle normal, FfiStyle selected, FfiStyle divider);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockAdv(IntPtr t, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockTitleAlignment(IntPtr t, uint align);

    // ===== Gauge / LineGauge / BarChart =====
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetStyles(IntPtr g, FfiStyle gauge, FfiStyle label);
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockAdv(IntPtr g, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockTitleAlignment(IntPtr g, uint align);
    // LineGauge
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiLineGaugeNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeFree(IntPtr g);
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_ratio", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetRatio(IntPtr g, float ratio);
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_label", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetLabel(IntPtr g, [MarshalAs(UnmanagedType.LPUTF8Str)] string? label);
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetStyle(IntPtr g, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockAdv(IntPtr g, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockTitle(IntPtr g, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockTitleAlignment(IntPtr g, uint align);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_linegauge_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawLineGaugeIn(IntPtr term, IntPtr g, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_linegauge", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderLineGauge(ushort width, ushort height, IntPtr g, out IntPtr utf8Text);
    // BarChart extras
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_bar_width", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBarWidth(IntPtr b, ushort width);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_bar_gap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBarGap(IntPtr b, ushort gap);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetStyles(IntPtr b, FfiStyle value, FfiStyle label, FfiStyle bar);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBlockAdv(IntPtr b, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBlockTitleAlignment(IntPtr b, uint align);

    // ===== Scrollbar =====
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_orientation_side", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetOrientationSide(IntPtr s, uint side);
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockAdv(IntPtr s, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockTitleAlignment(IntPtr s, uint align);

    // ===== Canvas =====
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiCanvasNew(double xMin, double xMax, double yMin, double yMax);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasFree(IntPtr c);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBounds(IntPtr c, double xMin, double xMax, double yMin, double yMax);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_background_color", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBackgroundColor(IntPtr c, uint color);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockTitle(IntPtr c, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockAdv(IntPtr c, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockTitleAlignment(IntPtr c, uint align);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_marker", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetMarker(IntPtr c, uint marker);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_add_line", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasAddLine(IntPtr c, double x1, double y1, double x2, double y2, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_add_rect", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasAddRect(IntPtr c, double x, double y, double w, double h, FfiStyle style, [MarshalAs(UnmanagedType.I1)] bool filled);
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_add_points", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasAddPoints(IntPtr c, double[] pointsXY, UIntPtr lenPairs, FfiStyle style, uint marker);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_canvas_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawCanvasIn(IntPtr term, IntPtr c, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_canvas", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderCanvas(ushort width, ushort height, IntPtr c, out IntPtr utf8Text);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_clear", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderClear(ushort width, ushort height, out IntPtr utf8Text);

    // ===== Clear / Logo =====
    [DllImport(LibraryName, EntryPoint = "ratatui_clear_in", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiClearIn(FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_ratatuilogo_draw_in", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLogoDrawIn(FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_ratatuilogo_draw_sized_in", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLogoDrawSizedIn(FfiRect rect, ushort width, ushort height);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_ratatuilogo", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderLogo(ushort width, ushort height, out IntPtr utf8Text);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_ratatuilogo_sized", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderLogoSized(ushort width, ushort height, ushort logoWidth, ushort logoHeight, out IntPtr utf8Text);

    // ===== Chart (extended) =====
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_reserve_datasets", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartReserveDatasets(IntPtr c, UIntPtr additional);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_add_datasets", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartAddDatasets(IntPtr c, double[] pointsXY, UIntPtr lenPairs, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_add_dataset_with_type", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartAddDatasetWithType(IntPtr c, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, double[] pointsXY, UIntPtr lenPairs, FfiStyle style, uint dtype);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_x_labels_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetXLabelsSpans(IntPtr c, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_y_labels_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetYLabelsSpans(IntPtr c, IntPtr spans, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_axis_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetAxisStyles(IntPtr c, FfiStyle x, FfiStyle y);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_labels_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetLabelsAlignment(IntPtr c, uint alignX, uint alignY);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBounds(IntPtr c, double xMin, double xMax, double yMin, double yMax);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetStyle(IntPtr c, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_legend_position", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetLegendPosition(IntPtr c, uint pos);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_hidden_legend_constraints", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetHiddenLegendConstraints(IntPtr c, uint[] kinds2, ushort[] values2);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockAdv(IntPtr c, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockTitleAlignment(IntPtr c, uint align);

    // List
    [DllImport(LibraryName, EntryPoint = "ratatui_list_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiListNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_list_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListFree(IntPtr lst);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_item", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItem(IntPtr lst, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);


    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockTitle(IntPtr lst, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_list_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawListIn(IntPtr term, IntPtr lst, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetSelected(IntPtr lst, int selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetHighlightStyle(IntPtr lst, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_highlight_symbol", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetHighlightSymbol(IntPtr lst, [MarshalAs(UnmanagedType.LPUTF8Str)] string? symbol);

    // Table (simple tab-separated API)
    [DllImport(LibraryName, EntryPoint = "ratatui_table_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiTableNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_table_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableFree(IntPtr tbl);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_headers", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeaders(IntPtr tbl, [MarshalAs(UnmanagedType.LPUTF8Str)] string headersTsv);


    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRow(IntPtr tbl, [MarshalAs(UnmanagedType.LPUTF8Str)] string rowTsv);



    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockTitle(IntPtr tbl, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_table_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawTableIn(IntPtr term, IntPtr tbl, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetSelected(IntPtr tbl, int selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_row_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetRowHighlightStyle(IntPtr tbl, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_highlight_symbol", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHighlightSymbol(IntPtr tbl, [MarshalAs(UnmanagedType.LPUTF8Str)] string? symbol);

    // Headless rendering
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_paragraph", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderParagraph(ushort width, ushort height, IntPtr para, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_string_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiStringFree(IntPtr ptr);

    // Headless list/table and composite frame
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_list", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderList(ushort width, ushort height, IntPtr list, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_table", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderTable(ushort width, ushort height, IntPtr table, out IntPtr utf8Text);

    internal enum FfiWidgetKind : uint { Paragraph = 1, List = 2, Table = 3, Gauge = 4, Tabs = 5, BarChart = 6, Sparkline = 7, Chart = 8, Scrollbar = 9 }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiDrawCmd { public uint Kind; public IntPtr Handle; public FfiRect Rect; }

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrame(ushort width, ushort height, [In] FfiDrawCmd[] commands, UIntPtr len, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_frame", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawFrame(IntPtr term, [In] FfiDrawCmd[] commands, UIntPtr len);

    // Event injection
    [DllImport(LibraryName, EntryPoint = "ratatui_inject_key", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiInjectKey(uint code, uint ch, byte mods);

    [DllImport(LibraryName, EntryPoint = "ratatui_inject_resize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiInjectResize(ushort width, ushort height);

    internal enum FfiMouseKind : uint { Down = 1, Up = 2, Drag = 3, Moved = 4, ScrollUp = 5, ScrollDown = 6 }
    internal enum FfiMouseButton : uint { None = 0, Left = 1, Right = 2, Middle = 3 }

    [DllImport(LibraryName, EntryPoint = "ratatui_inject_mouse", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiInjectMouse(uint kind, uint btn, ushort x, ushort y, byte mods);

    // Gauge
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiGaugeNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeFree(IntPtr g);
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_ratio", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetRatio(IntPtr g, float ratio);
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_label", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetLabel(IntPtr g, [MarshalAs(UnmanagedType.LPUTF8Str)] string? label);
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockTitle(IntPtr g, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_gauge_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawGaugeIn(IntPtr term, IntPtr g, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_gauge", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderGauge(ushort width, ushort height, IntPtr g, out IntPtr utf8Text);

    // Tabs
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiTabsNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsFree(IntPtr t);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_titles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetTitles(IntPtr t, [MarshalAs(UnmanagedType.LPUTF8Str)] string tsv);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetSelected(IntPtr t, ushort selected);
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockTitle(IntPtr t, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    // removed: tabs_set_block_title_bytes (use spans/adv APIs)
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_tabs_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawTabsIn(IntPtr term, IntPtr t, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_tabs", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderTabs(ushort width, ushort height, IntPtr t, out IntPtr utf8Text);

    // BarChart
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiBarChartNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartFree(IntPtr b);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_values", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetValues(IntPtr b, ulong[] values, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_labels", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetLabels(IntPtr b, [MarshalAs(UnmanagedType.LPUTF8Str)] string tsv);
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBlockTitle(IntPtr b, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    // removed: barchart bytes variants
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_barchart_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawBarChartIn(IntPtr term, IntPtr b, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_barchart", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderBarChart(ushort width, ushort height, IntPtr b, out IntPtr utf8Text);

    // Sparkline
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiSparklineNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineFree(IntPtr s);
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_values", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetValues(IntPtr s, ulong[] values, UIntPtr len);
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetBlockTitle(IntPtr s, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    // removed: sparkline bytes variant
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetBlockAdv(IntPtr s, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetBlockTitleAlignment(IntPtr s, uint align);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_sparkline_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawSparklineIn(IntPtr term, IntPtr s, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_sparkline", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderSparkline(ushort width, ushort height, IntPtr s, out IntPtr utf8Text);
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_max", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetMax(IntPtr s, ulong max);
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetStyle(IntPtr s, FfiStyle style);

    // Scrollbar
    internal enum FfiScrollbarOrient : uint { Vertical = 0, Horizontal = 1 }
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiScrollbarNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarFree(IntPtr s);
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_configure", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarConfigure(IntPtr s, uint orient, ushort position, ushort contentLen, ushort viewportLen);
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockTitle(IntPtr s, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    // removed: scrollbar bytes variant
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_scrollbar_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawScrollbarIn(IntPtr term, IntPtr s, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_scrollbar", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderScrollbar(ushort width, ushort height, IntPtr s, out IntPtr utf8Text);

    // Chart
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiChartNew();
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartFree(IntPtr c);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_add_line", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartAddLine(IntPtr c, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, double[] pointsXY, UIntPtr lenPairs, FfiStyle style);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_axes_titles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetAxesTitles(IntPtr c, [MarshalAs(UnmanagedType.LPUTF8Str)] string? xTitle, [MarshalAs(UnmanagedType.LPUTF8Str)] string? yTitle);
    // removed: chart_set_axes_bounds (use chart_set_bounds)
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockTitle(IntPtr c, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    // removed: chart bytes variant

    // removed: tabs titles bytes variant
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_chart_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawChartIn(IntPtr term, IntPtr c, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_chart", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderChart(ushort width, ushort height, IntPtr c, out IntPtr utf8Text);
}
