using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_new", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiParagraphNew([MarshalAs(UnmanagedType.LPUTF8Str)] string text);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_new_empty", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiParagraphNewEmpty();

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_line", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendLine(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_append_span", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphAppendSpan(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, FfiStyle style);

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

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetAlignment(IntPtr para, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_wrap", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetWrap(IntPtr para, [MarshalAs(UnmanagedType.I1)] bool trim);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_scroll", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetScroll(IntPtr para, ushort x, ushort y);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_style", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetStyle(IntPtr para, FfiStyle style);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_title", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockTitle(IntPtr para, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_title_spans", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockTitleSpans(IntPtr para, IntPtr spans, UIntPtr len, [MarshalAs(UnmanagedType.I1)] bool showBorder);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_adv", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockAdv(IntPtr para, byte bordersBits, uint borderType, ushort padL, ushort padT, ushort padR, ushort padB, IntPtr titleSpans, UIntPtr titleLen);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_set_block_title_alignment", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphSetBlockTitleAlignment(IntPtr para, uint align);

    [DllImport(LibraryName, EntryPoint = "ratatui_paragraph_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiParagraphFree(IntPtr para);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_paragraph", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawParagraph(IntPtr term, IntPtr para);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_paragraph_in", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawParagraphIn(IntPtr term, IntPtr para, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_paragraph", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderParagraph(ushort width, ushort height, IntPtr para, out IntPtr utf8Text);
}
