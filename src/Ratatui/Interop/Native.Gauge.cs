using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiGaugeNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeFree(IntPtr gauge);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_ratio", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetRatio(IntPtr gauge, float ratio);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_label", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetLabel(IntPtr gauge, [MarshalAs(UnmanagedType.LPUTF8Str)] string? text);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_label_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetLabelSpans(IntPtr gauge, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetStyles(IntPtr gauge, FfiStyle gaugeStyle, FfiStyle labelStyle);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockTitle(IntPtr gauge, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockTitleSpans(IntPtr gauge, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockAdv(IntPtr gauge, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_gauge_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiGaugeSetBlockTitleAlignment(IntPtr gauge, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_gauge_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawGaugeIn(IntPtr term, IntPtr gauge, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_gauge", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderGauge(ushort width, ushort height, IntPtr gauge, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiLineGaugeNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeFree(IntPtr gauge);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_ratio", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetRatio(IntPtr gauge, float ratio);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_label", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetLabel(IntPtr gauge, [MarshalAs(UnmanagedType.LPUTF8Str)] string? text);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_label_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetLabelSpans(IntPtr gauge, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetStyle(IntPtr gauge, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockTitle(IntPtr gauge, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockTitleSpans(IntPtr gauge, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockAdv(IntPtr gauge, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_linegauge_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLineGaugeSetBlockTitleAlignment(IntPtr gauge, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_linegauge_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawLineGaugeIn(IntPtr term, IntPtr gauge, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_linegauge", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderLineGauge(ushort width, ushort height, IntPtr gauge, out IntPtr utf8Text);
}
