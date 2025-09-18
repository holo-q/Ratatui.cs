using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_string_free", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiStringFree(IntPtr ptr);

    [DllImport(LibraryName, EntryPoint = "ratatui_terminal_draw_frame", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiTerminalDrawFrame(IntPtr term, [In] FfiDrawCmd[] commands, UIntPtr len);

    [DllImport(LibraryName, EntryPoint = "ratatui_inject_key", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiInjectKey(uint code, uint ch, byte mods);

    [DllImport(LibraryName, EntryPoint = "ratatui_inject_resize", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiInjectResize(ushort width, ushort height);

    [DllImport(LibraryName, EntryPoint = "ratatui_inject_mouse", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiInjectMouse(uint kind, uint btn, ushort x, ushort y, byte mods);
}
