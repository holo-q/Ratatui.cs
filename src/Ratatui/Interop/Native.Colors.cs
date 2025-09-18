using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_color_rgb", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint RatatuiColorRgb(byte r, byte g, byte b);

    [DllImport(LibraryName, EntryPoint = "ratatui_color_indexed", CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint RatatuiColorIndexed(byte index);
}
