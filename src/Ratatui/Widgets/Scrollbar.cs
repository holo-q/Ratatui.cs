using System;

namespace Ratatui;

public enum ScrollbarOrient { Vertical = 0, Horizontal = 1 }

public sealed class Scrollbar : IDisposable
{
    private readonly ScrollbarHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Scrollbar()
    {
        var ptr = Interop.Native.RatatuiScrollbarNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Scrollbar");
        _handle = ScrollbarHandle.FromRaw(ptr);
    }

    public Scrollbar Configure(ScrollbarOrient orient, int position, int contentLength, int viewportLength)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiScrollbarConfigure(_handle.DangerousGetHandle(), (uint)orient, (ushort)position, (ushort)contentLength, (ushort)viewportLength);
        return this;
    }

    public Scrollbar Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiScrollbarSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe Scrollbar Title(ReadOnlySpan<byte> titleUtf8, bool border = true)
    {
        EnsureNotDisposed();
        fixed (byte* p = titleUtf8)
        {
            Interop.Native.RatatuiScrollbarSetBlockTitleBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)titleUtf8.Length, border);
        }
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Scrollbar)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
