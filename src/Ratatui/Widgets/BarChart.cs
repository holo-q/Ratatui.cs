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

    public BarChart Labels(params string[] labels)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", labels ?? Array.Empty<string>());
        Interop.Native.RatatuiBarChartSetLabels(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    public unsafe BarChart Labels(ReadOnlySpan<byte> labelsTsvUtf8)
    {
        EnsureNotDisposed();
        fixed (byte* p = labelsTsvUtf8)
        {
            Interop.Native.RatatuiBarChartSetLabelsBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)labelsTsvUtf8.Length);
        }
        return this;
    }

    public BarChart Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiBarChartSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe BarChart Title(ReadOnlySpan<byte> titleUtf8, bool border = true)
    {
        EnsureNotDisposed();
        fixed (byte* p = titleUtf8)
        {
            Interop.Native.RatatuiBarChartSetBlockTitleBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)titleUtf8.Length, border);
        }
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(BarChart)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
