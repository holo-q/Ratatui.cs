using System;

namespace Ratatui;

public sealed class TableState : IDisposable
{
    private readonly TableStateHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public TableState()
    {
        var ptr = Interop.Native.RatatuiTableStateNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create TableState");
        _handle = TableStateHandle.FromRaw(ptr);
    }

    public TableState Selected(int index)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableStateSetSelected(_handle.DangerousGetHandle(), index);
        return this;
    }

    public TableState Offset(int offset)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableStateSetOffset(_handle.DangerousGetHandle(), (UIntPtr)Math.Max(0, offset));
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(TableState)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}

