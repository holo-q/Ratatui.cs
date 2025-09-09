using System;

namespace Ratatui;

public sealed class Sparkline : IDisposable
{
    private readonly SparklineHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Sparkline()
    {
        var ptr = Interop.Native.RatatuiSparklineNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Sparkline");
        _handle = SparklineHandle.FromRaw(ptr);
    }

    public Sparkline Values(params ulong[] values)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiSparklineSetValues(_handle.DangerousGetHandle(), values, (UIntPtr)(values?.LongLength ?? 0));
        return this;
    }

    public Sparkline Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiSparklineSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Sparkline)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}

