using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_list_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiListNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_list_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListFree(IntPtr list);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_item", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItem(IntPtr list, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_item_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItemSpans(IntPtr list, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_append_items_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListAppendItemsSpans(IntPtr list, IntPtr lines, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_reserve_items", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListReserveItems(IntPtr list, UIntPtr additional);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_direction", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetDirection(IntPtr list, uint direction);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_highlight_spacing", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetHighlightSpacing(IntPtr list, uint spacing);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_scroll_offset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetScrollOffset(IntPtr list, ushort offset);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetSelected(IntPtr list, int selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetHighlightStyle(IntPtr list, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_highlight_symbol", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetHighlightSymbol(IntPtr list, [MarshalAs(UnmanagedType.LPUTF8Str)] string? symbol);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockTitle(IntPtr list, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockTitleSpans(IntPtr list, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockAdv(IntPtr list, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListSetBlockTitleAlignment(IntPtr list, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_list_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawListIn(IntPtr term, IntPtr list, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_list", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderList(ushort width, ushort height, IntPtr list, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiListStateNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListStateFree(IntPtr state);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListStateSetSelected(IntPtr state, int selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_list_state_set_offset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiListStateSetOffset(IntPtr state, UIntPtr offset);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_list_state_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawListStateIn(IntPtr term, IntPtr list, FfiRect rect, IntPtr state);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_list_state", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderListState(ushort width, ushort height, IntPtr list, IntPtr state, out IntPtr utf8Text);
}
