using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiCanvasNew(double xMin, double xMax, double yMin, double yMax);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasFree(IntPtr canvas);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_bounds", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBounds(IntPtr canvas, double x1, double y1, double x2, double y2);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_background_color", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBackgroundColor(IntPtr canvas, uint color);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_marker", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetMarker(IntPtr canvas, uint marker);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockTitle(IntPtr canvas, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockTitleSpans(IntPtr canvas, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockAdv(IntPtr canvas, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasSetBlockTitleAlignment(IntPtr canvas, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_add_line", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasAddLine(IntPtr canvas, double x1, double y1, double x2, double y2, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_add_rect", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasAddRect(IntPtr canvas, double x, double y, double width, double height, FfiStyle style, [MarshalAs(UnmanagedType.I1)] bool filled);

    [DllImport(LibraryName, EntryPoint = "ratatui_canvas_add_points", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiCanvasAddPoints(IntPtr canvas, double[] pointsXY, UIntPtr lenPairs, FfiStyle style, uint marker);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_canvas_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawCanvasIn(IntPtr term, IntPtr canvas, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_canvas", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderCanvas(ushort width, ushort height, IntPtr canvas, out IntPtr utf8Text);
}
