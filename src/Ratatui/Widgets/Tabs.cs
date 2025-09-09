using System;

namespace Ratatui;

public sealed class Tabs : IDisposable
{
    private readonly TabsHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Tabs()
    {
        var ptr = Interop.Native.RatatuiTabsNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Tabs");
        _handle = TabsHandle.FromRaw(ptr);
    }

    public Tabs Titles(params string[] titles)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", titles ?? Array.Empty<string>());
        Interop.Native.RatatuiTabsSetTitles(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    public Tabs Selected(int index)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetSelected(_handle.DangerousGetHandle(), (ushort)Math.Max(0, index));
        return this;
    }

    public Tabs Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Tabs)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}

