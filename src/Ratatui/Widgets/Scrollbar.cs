using System;

namespace Ratatui;

public enum ScrollbarOrient { Vertical = 0, Horizontal = 1 }

public sealed class Scrollbar : IDisposable
{
    private readonly ScrollbarHandle _handle;
    private bool _disposed;
    private ScrollbarOrient _orient;
    private ushort _position;
    private ushort _contentLen;
    private ushort _viewportLen;
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
        _orient = orient; _position = (ushort)position; _contentLen = (ushort)contentLength; _viewportLen = (ushort)viewportLength;
        Interop.Native.RatatuiScrollbarConfigure(_handle.DangerousGetHandle(), (uint)_orient, _position, _contentLen, _viewportLen);
        return this;
    }

    public Scrollbar Orientation(ScrollbarOrient orient) { return Configure(orient, _position, _contentLen, _viewportLen); }
    public Scrollbar Position(int pos) { return Configure(_orient, pos, _contentLen, _viewportLen); }
    public Scrollbar ContentLength(int len) { return Configure(_orient, _position, len, _viewportLen); }
    public Scrollbar ViewportLength(int len) { return Configure(_orient, _position, _contentLen, len); }

    public enum Side : uint { Left = 0, Right = 1, Top = 2, Bottom = 3 }
    public Scrollbar OrientationSide(Side side)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiScrollbarSetOrientationSide(_handle.DangerousGetHandle(), (uint)side);
        return this;
    }

    public Scrollbar Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiScrollbarSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public Scrollbar TitleAlignment(Alignment align)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiScrollbarSetBlockTitleAlignment(_handle.DangerousGetHandle(), (uint)align);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Scrollbar)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
