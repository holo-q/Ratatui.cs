using System.Runtime.InteropServices;

namespace Ratatui.Interop;

internal static partial class Native
{
    [DllImport(LibraryName, EntryPoint = "ratatui_symbols_get_braille_dots_flat", CallingConvention = CallingConvention.Cdecl)]
    internal static extern FfiU16Slice RatatuiSymbolsGetBrailleDotsFlat();
}
