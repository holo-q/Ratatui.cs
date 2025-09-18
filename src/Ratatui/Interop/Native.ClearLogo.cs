using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_clear_in", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiClearIn(FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_clear", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderClear(ushort width, ushort height, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_ratatuilogo_draw_in", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLogoDrawIn(FfiRect rect);

    [DllImport(LibraryName, EntryPoint = "ratatui_ratatuilogo_draw_sized_in", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void RatatuiLogoDrawSizedIn(FfiRect rect, ushort width, ushort height);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_ratatuilogo", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderLogo(ushort width, ushort height, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_ratatuilogo_sized", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderLogoSized(ushort width, ushort height, ushort logoWidth, ushort logoHeight, out IntPtr utf8Text);
}
