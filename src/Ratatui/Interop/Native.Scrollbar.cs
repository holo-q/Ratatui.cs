using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiScrollbarNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarFree(IntPtr scrollbar);

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_configure", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarConfigure(IntPtr scrollbar, uint orient, ushort position, ushort contentLength, ushort viewportLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_orientation_side", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetOrientationSide(IntPtr scrollbar, uint side);

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockTitle(IntPtr scrollbar, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockTitleSpans(IntPtr scrollbar, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockAdv(IntPtr scrollbar, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_scrollbar_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiScrollbarSetBlockTitleAlignment(IntPtr scrollbar, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_scrollbar_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawScrollbarIn(IntPtr term, IntPtr scrollbar, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_scrollbar", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderScrollbar(ushort width, ushort height, IntPtr scrollbar, out IntPtr utf8Text);
}
