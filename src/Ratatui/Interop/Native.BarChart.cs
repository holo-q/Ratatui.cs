using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiBarChartNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartFree(IntPtr chart);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_values", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetValues(IntPtr chart, ulong[] values, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_labels", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetLabels(IntPtr chart, [MarshalAs(UnmanagedType.LPUTF8Str)] string labelsTsv);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_labels_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetLabelsSpans(IntPtr chart, IntPtr lines, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_bar_width", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBarWidth(IntPtr chart, ushort width);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_bar_gap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBarGap(IntPtr chart, ushort gap);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetStyles(IntPtr chart, FfiStyle valueStyle, FfiStyle labelStyle, FfiStyle barStyle);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBlockTitle(IntPtr chart, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBlockTitleSpans(IntPtr chart, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_barchart_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiBarChartSetBlockAdv(IntPtr chart, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_barchart_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawBarChartIn(IntPtr term, IntPtr chart, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_barchart", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderBarChart(ushort width, ushort height, IntPtr chart, out IntPtr utf8Text);
}
