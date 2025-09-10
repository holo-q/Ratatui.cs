using System;
using System.Collections.Generic;

namespace Ratatui;

public sealed class Chart : IDisposable
{
    private readonly ChartHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Chart()
    {
        var ptr = Interop.Native.RatatuiChartNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Chart");
        _handle = ChartHandle.FromRaw(ptr);
    }

    public Chart Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    // UTF-8 title path can use spans in future.

    public Chart Axes(string? xTitle = null, string? yTitle = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetAxesTitles(_handle.DangerousGetHandle(), xTitle, yTitle);
        return this;
    }

    public Chart AxesBounds(double xMin, double xMax, double yMin, double yMax)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetBounds(_handle.DangerousGetHandle(), xMin, xMax, yMin, yMax);
        return this;
    }

    public Chart Line(string name, IReadOnlyList<(double x, double y)> points, Style? style = null)
    {
        EnsureNotDisposed();
        if (points == null || points.Count == 0) return this;
        var count = points.Count;
        double[] flat = System.Buffers.ArrayPool<double>.Shared.Rent(count * 2);
        try
        {
            for (int i = 0; i < count; i++) { flat[i*2] = points[i].x; flat[i*2+1] = points[i].y; }
            Interop.Native.RatatuiChartAddLine(_handle.DangerousGetHandle(), name, flat, (UIntPtr)count, (style ?? default).ToFfi());
        }
        finally
        {
            System.Buffers.ArrayPool<double>.Shared.Return(flat);
        }
        return this;
    }

    public Chart Line(string name, ReadOnlySpan<(double x, double y)> points, Style? style = null)
    {
        EnsureNotDisposed();
        if (points.IsEmpty) return this;
        var count = points.Length;
        if (count * 2 <= 256)
        {
            Span<double> flat = stackalloc double[count * 2];
            for (int i = 0; i < count; i++) { flat[i*2] = points[i].x; flat[i*2+1] = points[i].y; }
            Interop.Native.RatatuiChartAddLine(_handle.DangerousGetHandle(), name, flat.ToArray(), (UIntPtr)count, (style ?? default).ToFfi());
        }
        else
        {
            double[] array = System.Buffers.ArrayPool<double>.Shared.Rent(count * 2);
            try
            {
                for (int i = 0; i < count; i++) { array[i*2] = points[i].x; array[i*2+1] = points[i].y; }
                Interop.Native.RatatuiChartAddLine(_handle.DangerousGetHandle(), name, array, (UIntPtr)count, (style ?? default).ToFfi());
            }
            finally
            {
                System.Buffers.ArrayPool<double>.Shared.Return(array);
            }
        }
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Chart)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
