using System;
using System.Linq;

namespace Ratatui;

public sealed class Table : IDisposable
{
    private readonly TableHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Table()
    {
        var ptr = Interop.Native.RatatuiTableNew();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create Table");
        _handle = TableHandle.FromRaw(ptr);
    }

    public Table Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public Table Headers(params string[] cells)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", cells ?? Array.Empty<string>());
        Interop.Native.RatatuiTableSetHeaders(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    public Table AppendRow(params string[] cells)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", cells ?? Array.Empty<string>());
        Interop.Native.RatatuiTableAppendRow(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Table));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}

