using System;
using System.Linq;

namespace Ratatui;

public sealed class BarChart : IDisposable
{
    private readonly BarChartHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public BarChart()
    {
        var ptr = Interop.Native.RatatuiBarChartNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create BarChart");
        _handle = BarChartHandle.FromRaw(ptr);
    }

    public BarChart Values(params ulong[] values)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiBarChartSetValues(_handle.DangerousGetHandle(), values, (UIntPtr)(values?.LongLength ?? 0));
        return this;
    }

    public BarChart Values(ReadOnlySpan<ulong> values)
    {
        EnsureNotDisposed();
        if (values.IsEmpty) return this;
        // P/Invoke takes ulong[]; copy via ArrayPool to minimize allocations.
        var arr = System.Buffers.ArrayPool<ulong>.Shared.Rent(values.Length);
        try
        {
            values.CopyTo(arr);
            Interop.Native.RatatuiBarChartSetValues(_handle.DangerousGetHandle(), arr, (UIntPtr)values.Length);
        }
        finally
        {
            System.Buffers.ArrayPool<ulong>.Shared.Return(arr);
        }
        return this;
    }

    public BarChart Labels(params string[] labels)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", labels ?? Array.Empty<string>());
        Interop.Native.RatatuiBarChartSetLabels(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    // UTF-8 labels path can use spans in future.

    public BarChart Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiBarChartSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    // UTF-8 title path can use spans in future.

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(BarChart)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
