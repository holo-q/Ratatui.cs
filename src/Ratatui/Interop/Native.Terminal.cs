using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_init_terminal", CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr RatatuiInitTerminal();

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_clear", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTerminalClear(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiTerminalFree(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_size", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalSize(out ushort width, out ushort height);

    [DllImport(LibraryName, EntryPoint = "ratatui_next_event", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiNextEvent(ulong timeoutMs, out FfiEvent ev);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_enable_raw", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalEnableRaw(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_disable_raw", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDisableRaw(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_enter_alt", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalEnterAlt(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_leave_alt", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalLeaveAlt(IntPtr term);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_set_cursor_position", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalSetCursorPosition(IntPtr term, ushort x, ushort y);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_get_cursor_position", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalGetCursorPosition(IntPtr term, out ushort x, out ushort y);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_show_cursor", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalShowCursor(IntPtr term, [MarshalAs(UnmanagedType.I1)] bool show);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_set_viewport_area", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalSetViewportArea(IntPtr term, FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_get_viewport_area", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalGetViewportArea(IntPtr term, out FfiRect rect);
}
