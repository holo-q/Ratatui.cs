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
            var _ = RatatuiFfiVersion();
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiVersion { public uint Abi; public ushort ApiMajor; public ushort ApiMinor; public ushort ApiPatch; }

    [DllImport(LibraryName, EntryPoint = "ratatui_ffi_version", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FfiVersion RatatuiFfiVersion();

    private const uint ExpectedAbi = 1;
    private static void ValidateVersion()
    {
        var v = RatatuiFfiVersion();
        if (v.Abi != ExpectedAbi)
        {
            throw new DllNotFoundException($"ratatui_ffi ABI mismatch: expected {ExpectedAbi}, got {v.Abi}. Make sure the native library matches this package.");
        }
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

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_newline", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphNewLine(IntPtr para);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_span", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendSpan(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_span_bytes", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendSpanBytes(IntPtr para, IntPtr bytes, UIntPtr len, FfiStyle style);

    // List
    [DllImport(LibraryName, EntryPoint = "ratatui_list_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiListNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_list_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListFree(IntPtr lst);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_item", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItem(IntPtr lst, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_item_bytes", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItemBytes(IntPtr lst, IntPtr bytes, UIntPtr len, FfiStyle style);

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

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_headers_bytes", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeadersBytes(IntPtr tbl, IntPtr bytes, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRow(IntPtr tbl, [MarshalAs(UnmanagedType.LPUTF8Str)] string rowTsv);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row_bytes", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowBytes(IntPtr tbl, IntPtr bytes, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_column_percents", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetColumnPercents(IntPtr tbl, ushort[] percents, UIntPtr len);

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
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_sparkline_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawSparklineIn(IntPtr term, IntPtr s, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_sparkline", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderSparkline(ushort width, ushort height, IntPtr s, out IntPtr utf8Text);

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
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_axes_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetAxesBounds(IntPtr c, double xMin, double xMax, double yMin, double yMax);
    [DllImport(LibraryName, EntryPoint = "ratatui_chart_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiChartSetBlockTitle(IntPtr c, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);
    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_chart_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawChartIn(IntPtr term, IntPtr c, FfiRect rect);
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_chart", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderChart(ushort width, ushort height, IntPtr c, out IntPtr utf8Text);
}
