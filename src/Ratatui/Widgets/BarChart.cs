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

    // UTF-8 TSV labels (allocates a string due to FFI signature)
    public unsafe BarChart Labels(ReadOnlySpan<byte> tsvUtf8)
    {
        EnsureNotDisposed();
        if (tsvUtf8.IsEmpty)
        {
            Interop.Native.RatatuiBarChartSetLabels(_handle.DangerousGetHandle(), string.Empty);
            return this;
        }
        // Interpret TSV as multiple labels; build single-line spans per label (no styles).
        int count = 1; for (int i = 0; i < tsvUtf8.Length; i++) if (tsvUtf8[i] == (byte) '\t') count++;
        Span<Interop.Native.FfiSpan> spanBuf = count <= 64 ? stackalloc Interop.Native.FfiSpan[count] : new Interop.Native.FfiSpan[count];
        Span<Interop.Native.FfiLineSpans> lines = count <= 64 ? stackalloc Interop.Native.FfiLineSpans[count] : new Interop.Native.FfiLineSpans[count];
        // Make a NUL-terminated copy per label slice inside one temporary buffer? Simpler: allocate per label pointer into separate small stackallocs.
        // For simplicity and zero heap allocs, we duplicate into small per-label stacks in a loop.
        int start = 0, idx = 0;
        fixed (Interop.Native.FfiSpan* pSpanBuf = spanBuf)
        {
            for (int i = 0; i <= tsvUtf8.Length; i++)
            {
                bool end = (i == tsvUtf8.Length) || (tsvUtf8[i] == (byte) '\t');
                if (!end) continue;
                int len = i - start;
                var buf = stackalloc byte[len + 1];
                if (len > 0) tsvUtf8.Slice(start, len).CopyTo(new Span<byte>(buf, len));
                buf[len] = 0;
                spanBuf[idx] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = default };
                lines[idx] = new Interop.Native.FfiLineSpans { Spans = (IntPtr)(pSpanBuf + idx), Len = (UIntPtr)1 };
                idx++; start = i + 1;
            }
            fixed (Interop.Native.FfiLineSpans* pLines = lines)
            {
                Interop.Native.RatatuiBarChartSetLabelsSpans(_handle.DangerousGetHandle(), (IntPtr)pLines, (UIntPtr)idx);
            }
        }
        return this;
    }

    public BarChart Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiBarChartSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe BarChart Title(ReadOnlySpan<byte> utf8, bool border = true)
    {
        EnsureNotDisposed();
        if (utf8.IsEmpty)
        {
            Interop.Native.RatatuiBarChartSetBlockTitle(_handle.DangerousGetHandle(), null, border);
            return this;
        }
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = default };
        Interop.Native.RatatuiBarChartSetBlockTitleSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1, border);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(BarChart)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
