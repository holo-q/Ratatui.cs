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

    public unsafe Gauge Label(ReadOnlySpan<byte> utf8)
    {
        EnsureNotDisposed();
        if (utf8.IsEmpty)
        {
            Interop.Native.RatatuiGaugeSetLabel(_handle.DangerousGetHandle(), null);
            return this;
        }
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = default };
        Interop.Native.RatatuiGaugeSetLabelSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1);
        return this;
    }

    // UTF-8 label path can use spans in future.

    public Gauge Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiGaugeSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe Gauge Title(ReadOnlySpan<byte> utf8, bool border = true)
    {
        EnsureNotDisposed();
        if (utf8.IsEmpty)
        {
            Interop.Native.RatatuiGaugeSetBlockTitle(_handle.DangerousGetHandle(), null, border);
            return this;
        }
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = default };
        Interop.Native.RatatuiGaugeSetBlockTitleSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1, border);
        return this;
    }

    // UTF-8 title path can use spans in future.

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Gauge)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
