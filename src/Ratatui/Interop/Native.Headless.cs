using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrame(ushort width, ushort height, [In] FfiDrawCmd[] commands, UIntPtr len, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame_styles", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrameStyles(ushort width, ushort height, [In] FfiDrawCmd[] commands, UIntPtr len, out IntPtr utf8Text);

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame_styles_ex", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrameStylesEx(ushort width, ushort height, [In] FfiDrawCmd[] commands, UIntPtr len, out IntPtr utf8Text);

    [StructLayout(LayoutKind.Sequential)]
    internal struct FfiCellInfo { public uint Ch, Fg, Bg; public ushort Mods; }

    [DllImport(LibraryName, EntryPoint = "ratatui_headless_render_frame_cells", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool RatatuiHeadlessRenderFrameCells(ushort width, ushort height, [In] FfiDrawCmd[] commands, UIntPtr len, IntPtr outCells, UIntPtr cap);
}
