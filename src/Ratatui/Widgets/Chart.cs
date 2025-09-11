using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

    public enum DatasetKind : uint { Line = 0, Bar = 1, Scatter = 2 }

    public Chart Dataset(string name, IReadOnlyList<(double x, double y)> points, Style? style = null, DatasetKind kind = DatasetKind.Line)
    {
        EnsureNotDisposed();
        if (points == null || points.Count == 0) return this;
        var count = points.Count;
        double[] flat = System.Buffers.ArrayPool<double>.Shared.Rent(count * 2);
        try
        {
            for (int i = 0; i < count; i++) { flat[i*2] = points[i].x; flat[i*2+1] = points[i].y; }
            Interop.Native.RatatuiChartAddDatasetWithType(_handle.DangerousGetHandle(), name, flat, (UIntPtr)count, (style ?? default).ToFfi(), (uint)kind);
        }
        finally
        {
            System.Buffers.ArrayPool<double>.Shared.Return(flat);
        }
        return this;
    }

    public Chart Dataset(string name, ReadOnlySpan<(double x, double y)> points, Style? style = null, DatasetKind kind = DatasetKind.Line)
    {
        EnsureNotDisposed();
        if (points.IsEmpty) return this;
        var count = points.Length;
        if (count * 2 <= 256)
        {
            Span<double> flat = stackalloc double[count * 2];
            for (int i = 0; i < count; i++) { flat[i*2] = points[i].x; flat[i*2+1] = points[i].y; }
            Interop.Native.RatatuiChartAddDatasetWithType(_handle.DangerousGetHandle(), name, flat.ToArray(), (UIntPtr)count, (style ?? default).ToFfi(), (uint)kind);
        }
        else
        {
            double[] array = System.Buffers.ArrayPool<double>.Shared.Rent(count * 2);
            try
            {
                for (int i = 0; i < count; i++) { array[i*2] = points[i].x; array[i*2+1] = points[i].y; }
                Interop.Native.RatatuiChartAddDatasetWithType(_handle.DangerousGetHandle(), name, array, (UIntPtr)count, (style ?? default).ToFfi(), (uint)kind);
            }
            finally
            {
                System.Buffers.ArrayPool<double>.Shared.Return(array);
            }
        }
        return this;
    }

    // Bulk datasets append (single FFI call). Copies names to UTF-8 + points to double[] for the call duration.
    public Chart Datasets(params (string name, (double x, double y)[] points, Style? style, DatasetKind kind)[] datasets)
    {
        EnsureNotDisposed();
        if (datasets == null || datasets.Length == 0) return this;
        var specs = new Interop.Native.FfiChartDatasetSpec[datasets.Length];
        var handles = new List<GCHandle>(datasets.Length * 2);
        try
        {
            for (int i = 0; i < datasets.Length; i++)
            {
                var (name, pts, sty, kind) = datasets[i];
                // Name as UTF-8 with NUL
                var nameUtf8 = System.Text.Encoding.UTF8.GetBytes(name ?? string.Empty + "\0");
                var nameHandle = GCHandle.Alloc(nameUtf8, GCHandleType.Pinned);
                handles.Add(nameHandle);

                // Flatten points
                var len = pts?.Length ?? 0;
                double[] flat = len == 0 ? Array.Empty<double>() : new double[len * 2];
                for (int p = 0; p < len; p++) { flat[p*2] = pts![p].x; flat[p*2+1] = pts![p].y; }
                var ptsHandle = GCHandle.Alloc(flat, GCHandleType.Pinned);
                handles.Add(ptsHandle);

                specs[i] = new Interop.Native.FfiChartDatasetSpec
                {
                    NameUtf8 = nameHandle.AddrOfPinnedObject(),
                    PointsXY = ptsHandle.AddrOfPinnedObject(),
                    LenPairs = (UIntPtr)len,
                    Style = (sty ?? default).ToFfi(),
                    Kind = (uint)kind,
                };
            }

            Interop.Native.RatatuiChartAddDatasets(_handle.DangerousGetHandle(), specs, (UIntPtr)specs.Length);
        }
        finally
        {
            foreach (var h in handles)
            {
                if (h.IsAllocated) h.Free();
            }
        }
        return this;
    }

    public Chart XLabels(ReadOnlySpan<ReadOnlyMemory<Batching.SpanRun>> labels)
    {
        EnsureNotDisposed();
        if (labels.IsEmpty)
        {
            // Passing empty clears labels; FFI treats null appropriately if len==0
            Batching.WithFfiLineSpans(labels, (lines, len) =>
            {
                Interop.Native.RatatuiChartSetXLabelsSpans(_handle.DangerousGetHandle(), lines, len);
            });
            return this;
        }
        Batching.WithFfiLineSpans(labels, (lines, len) =>
        {
            Interop.Native.RatatuiChartSetXLabelsSpans(_handle.DangerousGetHandle(), lines, len);
        });
        return this;
    }

    public Chart YLabels(ReadOnlySpan<ReadOnlyMemory<Batching.SpanRun>> labels)
    {
        EnsureNotDisposed();
        Batching.WithFfiLineSpans(labels, (lines, len) =>
        {
            Interop.Native.RatatuiChartSetYLabelsSpans(_handle.DangerousGetHandle(), lines, len);
        });
        return this;
    }

    public Chart AxisStyles(Style xStyle, Style yStyle)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetAxisStyles(_handle.DangerousGetHandle(), xStyle.ToFfi(), yStyle.ToFfi());
        return this;
    }

    public Chart LabelsAlignment(Alignment xAlign, Alignment yAlign)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetLabelsAlignment(_handle.DangerousGetHandle(), (uint)xAlign, (uint)yAlign);
        return this;
    }

    public enum LegendPosition : uint
    {
        Right = 4,
        Top = 1,
        Bottom = 2,
        Left = 3,
        TopLeft = 5,
        TopRight = 6,
        BottomLeft = 7,
        BottomRight = 8,
    }

    public Chart Legend(LegendPosition pos)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetLegendPosition(_handle.DangerousGetHandle(), (uint)pos);
        return this;
    }

    public Chart Style(Style s)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiChartSetStyle(_handle.DangerousGetHandle(), s.ToFfi());
        return this;
    }

    public enum HiddenConstraintKind : uint { Length = 0, Percentage = 1, Min = 2 }

    public Chart HiddenLegendConstraints((HiddenConstraintKind kind, ushort value) first,
                                         (HiddenConstraintKind kind, ushort value) second)
    {
        EnsureNotDisposed();
        uint[] kinds = [(uint)first.kind, (uint)second.kind];
        ushort[] values = [first.value, second.value];
        Interop.Native.RatatuiChartSetHiddenLegendConstraints(_handle.DangerousGetHandle(), kinds, values);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Chart)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
