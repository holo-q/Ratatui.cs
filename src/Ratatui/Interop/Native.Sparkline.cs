using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiSparklineNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineFree(IntPtr sparkline);

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_values", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetValues(IntPtr sparkline, ulong[] values, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_max", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetMax(IntPtr sparkline, ulong max);

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetStyle(IntPtr sparkline, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetBlockTitle(IntPtr sparkline, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetBlockTitleSpans(IntPtr sparkline, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_sparkline_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiSparklineSetBlockAdv(IntPtr sparkline, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_sparkline_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawSparklineIn(IntPtr term, IntPtr sparkline, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_sparkline", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderSparkline(ushort width, ushort height, IntPtr sparkline, out IntPtr utf8Text);
}
