using System;
using System.Buffers;

namespace Ratatui;

public sealed class Canvas : IDisposable
{
    private readonly CanvasHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Canvas(double xMin = 0, double xMax = 1, double yMin = 0, double yMax = 1)
    {
        var ptr = Interop.Native.RatatuiCanvasNew(xMin, xMax, yMin, yMax);
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Canvas");
        _handle = CanvasHandle.FromRaw(ptr);
    }

    public Canvas Bounds(double xMin, double xMax, double yMin, double yMax)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasSetBounds(_handle.DangerousGetHandle(), xMin, xMax, yMin, yMax);
        return this;
    }

    public Canvas Background(Color color)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasSetBackgroundColor(_handle.DangerousGetHandle(), (uint)color);
        return this;
    }

    public Canvas Marker(uint marker)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasSetMarker(_handle.DangerousGetHandle(), marker);
        return this;
    }

    public Canvas Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public Canvas WithBlock(in BlockAdv adv)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasSetBlockAdv(_handle.DangerousGetHandle(), (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiCanvasSetBlockTitleAlignment(_handle.DangerousGetHandle(), (uint)adv.TitleAlignment);
        return this;
    }

    public Canvas AddLine(double x1, double y1, double x2, double y2, Style style)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasAddLine(_handle.DangerousGetHandle(), x1, y1, x2, y2, style.ToFfi());
        return this;
    }

    public Canvas AddRect(double x, double y, double w, double h, Style style, bool filled = false)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiCanvasAddRect(_handle.DangerousGetHandle(), x, y, w, h, style.ToFfi(), filled);
        return this;
    }

    public Canvas AddPoints(ReadOnlySpan<(double x, double y)> points, Style style, uint? marker = null)
    {
        EnsureNotDisposed();
        if (points.IsEmpty) return this;
        // Flatten to XY pairs; Pool for larger sets
        var len = points.Length * 2;
        if (len <= 256)
        {
            Span<double> flat = stackalloc double[len];
            for (int i = 0; i < points.Length; i++) { flat[i*2] = points[i].x; flat[i*2+1] = points[i].y; }
            Interop.Native.RatatuiCanvasAddPoints(_handle.DangerousGetHandle(), flat.ToArray(), (UIntPtr)points.Length, style.ToFfi(), marker ?? 0);
        }
        else
        {
            var arr = ArrayPool<double>.Shared.Rent(len);
            try
            {
                for (int i = 0; i < points.Length; i++) { arr[i*2] = points[i].x; arr[i*2+1] = points[i].y; }
                Interop.Native.RatatuiCanvasAddPoints(_handle.DangerousGetHandle(), arr, (UIntPtr)points.Length, style.ToFfi(), marker ?? 0);
            }
            finally { ArrayPool<double>.Shared.Return(arr); }
        }
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Canvas)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
