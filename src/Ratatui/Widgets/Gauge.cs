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

    public Gauge Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiGaugeSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe Gauge Title(ReadOnlySpan<byte> titleUtf8, bool border = true)
    {
        EnsureNotDisposed();
        fixed (byte* p = titleUtf8)
        {
            Interop.Native.RatatuiGaugeSetBlockTitleBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)titleUtf8.Length, border);
        }
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Gauge)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
