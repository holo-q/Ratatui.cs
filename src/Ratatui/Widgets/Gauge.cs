using System;

namespace Ratatui;

public sealed class Gauge : IDisposable
{
    private readonly GaugeHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Gauge()
    {
        var ptr = Interop.Native.RatatuiGaugeNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Gauge");
        _handle = GaugeHandle.FromRaw(ptr);
    }

    public Gauge Ratio(float value)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiGaugeSetRatio(_handle.DangerousGetHandle(), value);
        return this;
    }

    public Gauge Label(string? text)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiGaugeSetLabel(_handle.DangerousGetHandle(), text);
        return this;
    }

    public Gauge Label(ReadOnlySpan<byte> utf8)
    {
        EnsureNotDisposed();
        // FFI expects LPUTF8Str; create a transient string as there is no spans variant.
        Interop.Native.RatatuiGaugeSetLabel(_handle.DangerousGetHandle(), utf8.IsEmpty ? null : System.Text.Encoding.UTF8.GetString(utf8));
        return this;
    }

    // UTF-8 label path can use spans in future.

    public Gauge Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiGaugeSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public Gauge Title(ReadOnlySpan<byte> utf8, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiGaugeSetBlockTitle(_handle.DangerousGetHandle(), utf8.IsEmpty ? null : System.Text.Encoding.UTF8.GetString(utf8), border);
        return this;
    }

    // UTF-8 title path can use spans in future.

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Gauge)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
