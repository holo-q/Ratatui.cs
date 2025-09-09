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

    public Sparkline Values(ReadOnlySpan<ulong> values)
    {
        EnsureNotDisposed();
        if (values.IsEmpty) return this;
        var arr = System.Buffers.ArrayPool<ulong>.Shared.Rent(values.Length);
        try
        {
            values.CopyTo(arr);
            Interop.Native.RatatuiSparklineSetValues(_handle.DangerousGetHandle(), arr, (UIntPtr)values.Length);
        }
        finally
        {
            System.Buffers.ArrayPool<ulong>.Shared.Return(arr);
        }
        return this;
    }

    public Sparkline Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiSparklineSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe Sparkline Title(ReadOnlySpan<byte> titleUtf8, bool border = true)
    {
        EnsureNotDisposed();
        fixed (byte* p = titleUtf8)
        {
            Interop.Native.RatatuiSparklineSetBlockTitleBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)titleUtf8.Length, border);
        }
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Sparkline)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
