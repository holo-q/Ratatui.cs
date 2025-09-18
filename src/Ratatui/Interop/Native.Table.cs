using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_table_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiTableNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_table_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableFree(IntPtr table);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_headers", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeaders(IntPtr table, [MarshalAs(UnmanagedType.LPUTF8Str)] string headersTsv);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_headers_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeadersSpans(IntPtr table, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRow(IntPtr table, [MarshalAs(UnmanagedType.LPUTF8Str)] string rowTsv);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowSpans(IntPtr table, IntPtr spans, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_row_cells_lines", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowCellsLines(IntPtr table, IntPtr cells, UIntPtr cellCount);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_append_rows_cells_lines", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableAppendRowsCellsLines(IntPtr table, IntPtr rows, UIntPtr rowCount);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_reserve_rows", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableReserveRows(IntPtr table, UIntPtr additional);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_widths_percentages", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetWidthsPercentages(IntPtr table, ushort[] widths, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_widths", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetWidths(IntPtr table, uint[] kinds, ushort[] values, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_column_spacing", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetColumnSpacing(IntPtr table, ushort spacing);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_row_height", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetRowHeight(IntPtr table, ushort height);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_header_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHeaderStyle(IntPtr table, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_column_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetColumnHighlightStyle(IntPtr table, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_cell_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetCellHighlightStyle(IntPtr table, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_highlight_spacing", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHighlightSpacing(IntPtr table, uint spacing);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_highlight_symbol", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetHighlightSymbol(IntPtr table, [MarshalAs(UnmanagedType.LPUTF8Str)] string? symbol);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockTitle(IntPtr table, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockTitleSpans(IntPtr table, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockAdv(IntPtr table, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetBlockTitleAlignment(IntPtr table, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetSelected(IntPtr table, int selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_set_row_highlight_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableSetRowHighlightStyle(IntPtr table, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_table_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawTableIn(IntPtr term, IntPtr table, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_table", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderTable(ushort width, ushort height, IntPtr table, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiTableStateNew();

    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableStateFree(IntPtr state);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_set_selected", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableStateSetSelected(IntPtr state, int selected);

    [DllImport(LibraryName, EntryPoint = "ratatui_table_state_set_offset", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTableStateSetOffset(IntPtr state, UIntPtr offset);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_table_state_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawTableStateIn(IntPtr term, IntPtr table, FfiRect rect, IntPtr state);

}
