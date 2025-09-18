using System;
using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_layout_split", CallingConvention = CallingConvention.Cdecl)]
    internal static extern UIntPtr RatatuiLayoutSplit(ushort width, ushort height, uint dir,
        IntPtr kinds, IntPtr values, UIntPtr len,
        ushort marginL, ushort marginT, ushort marginR, ushort marginB,
        IntPtr outRects, UIntPtr outCap);

    [DllImport(LibraryName, EntryPoint = "ratatui_layout_split_ex", CallingConvention = CallingConvention.Cdecl)]
    internal static extern UIntPtr RatatuiLayoutSplitEx(ushort width, ushort height, uint dir,
        IntPtr kinds, IntPtr values, UIntPtr len,
        ushort spacing, ushort marginL, ushort marginT, ushort marginR, ushort marginB,
        IntPtr outRects, UIntPtr outCap);

    [DllImport(LibraryName, EntryPoint = "ratatui_layout_split_ex2", CallingConvention = CallingConvention.Cdecl)]
    internal static extern UIntPtr RatatuiLayoutSplitEx2(ushort width, ushort height, uint dir,
        IntPtr kinds, IntPtr valuesA, IntPtr valuesB, UIntPtr len,
        ushort spacing, ushort marginL, ushort marginT, ushort marginR, ushort marginB,
        IntPtr outRects, UIntPtr outCap);
}
