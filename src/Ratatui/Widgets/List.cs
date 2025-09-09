using System;

namespace Ratatui;

public sealed class List : IDisposable
{
    private readonly ListHandle _handle;
    private bool _disposed;

    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public List()
    {
        var ptr = Interop.Native.RatatuiListNew();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create List");
        _handle = ListHandle.FromRaw(ptr);
    }

    public List Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public List AppendItem(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListAppendItem(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
        return this;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(List));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}

