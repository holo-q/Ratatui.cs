using System;

namespace Ratatui;

public sealed class LineGauge : IDisposable
{
    private readonly LineGaugeHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public LineGauge()
    {
        var ptr = Interop.Native.RatatuiLineGaugeNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create LineGauge");
        _handle = LineGaugeHandle.FromRaw(ptr);
    }

    public LineGauge Ratio(float value)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiLineGaugeSetRatio(_handle.DangerousGetHandle(), value);
        return this;
    }

    public LineGauge Label(string? text)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiLineGaugeSetLabel(_handle.DangerousGetHandle(), text);
        return this;
    }

    public LineGauge Style(Style s)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiLineGaugeSetStyle(_handle.DangerousGetHandle(), s.ToFfi());
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(LineGauge)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}

