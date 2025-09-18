using System;
using System.Runtime.CompilerServices;

namespace Ratatui;

public static class Symbols
{
    private static ushort[]? _brailleDotsFlat;

    public static ReadOnlySpan<ushort> BrailleDotsFlat
    {
        get
        {
            if (_brailleDotsFlat is { Length: > 0 }) return _brailleDotsFlat;
            var slice = Interop.Native.RatatuiSymbolsGetBrailleDotsFlat();
            if (slice.Ptr == IntPtr.Zero || slice.Len == UIntPtr.Zero)
            {
                _brailleDotsFlat = Array.Empty<ushort>();
                return _brailleDotsFlat;
            }
            int len = checked((int)slice.Len);
            var result = new ushort[len];
            unsafe
            {
                Buffer.MemoryCopy((void*)slice.Ptr, Unsafe.AsPointer(ref result[0]), len * sizeof(ushort), len * sizeof(ushort));
            }
            _brailleDotsFlat = result;
            return result;
        }
    }
}
