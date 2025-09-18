using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiTabsNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsFree(IntPtr tabs);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_titles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetTitles(IntPtr tabs, [MarshalAs(UnmanagedType.LPUTF8Str)] string titlesTsv);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_add_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsAddTitleSpans(IntPtr tabs, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_clear_titles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsClearTitles(IntPtr tabs);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_titles_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetTitlesSpans(IntPtr tabs, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_divider", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetDivider(IntPtr tabs, [MarshalAs(UnmanagedType.LPUTF8Str)] string dividerUtf8);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_divider_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetDividerSpans(IntPtr tabs, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_styles", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetStyles(IntPtr tabs, FfiStyle normal, FfiStyle selected, FfiStyle divider);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetSelected(IntPtr tabs, ushort selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockTitle(IntPtr tabs, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockTitleSpans(IntPtr tabs, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockAdv(IntPtr tabs, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_tabs_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTabsSetBlockTitleAlignment(IntPtr tabs, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_tabs_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawTabsIn(IntPtr term, IntPtr tabs, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_tabs", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderTabs(ushort width, ushort height, IntPtr tabs, out IntPtr utf8Text);
}
