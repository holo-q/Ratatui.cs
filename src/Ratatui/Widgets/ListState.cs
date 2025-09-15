using System;

namespace Ratatui;

public sealed class ListState : IDisposable
{
    private readonly ListStateHandle _handle;
    private bool _disposed;
    private int? _selected;
    private int _offset;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();
    internal int? SelectedIndex => _selected;
    internal int OffsetValue => _offset;

    public ListState()
    {
        var ptr = Interop.Native.RatatuiListStateNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create ListState");
        _handle = ListStateHandle.FromRaw(ptr);
    }

    public ListState Selected(int index)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListStateSetSelected(_handle.DangerousGetHandle(), index);
        _selected = index;
        return this;
    }

    public ListState Offset(int offset)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListStateSetOffset(_handle.DangerousGetHandle(), (UIntPtr)Math.Max(0, offset));
        _offset = Math.Max(0, offset);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(ListState)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
