using System;

namespace Ratatui;

public sealed class Paragraph : IDisposable
{
    private readonly ParagraphHandle _handle;
    private bool _disposed;

    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Paragraph(string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        var ptr = Interop.Native.RatatuiParagraphNew(text);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create Paragraph");
        _handle = ParagraphHandle.FromRaw(ptr);
    }

    // Zero-allocation constructor from UTF-8 bytes (e.g. "text"u8)
    public unsafe Paragraph(ReadOnlySpan<byte> utf8)
    {
        var ptr = Interop.Native.RatatuiParagraphNewEmpty();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create Paragraph");
        _handle = ParagraphHandle.FromRaw(ptr);
        if (!utf8.IsEmpty)
        {
            var buf = stackalloc byte[utf8.Length + 1];
            utf8.CopyTo(new Span<byte>(buf, utf8.Length));
            buf[utf8.Length] = 0;
            var spans = stackalloc Interop.Native.FfiSpan[1];
            spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = default };
            Interop.Native.RatatuiParagraphAppendLineSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1);
        }
    }

    public Paragraph Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    // UTF-8 title overload can be restored using spans+adv APIs if needed.

    public Paragraph AppendLine(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphAppendLine(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
        return this;
    }

    // Zero-allocation overload from UTF-8 bytes (e.g. "text"u8)
    public unsafe Paragraph AppendLine(ReadOnlySpan<byte> utf8, Style? style = null)
    {
        EnsureNotDisposed();
        var sty = style ?? default;
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = sty.ToFfi() };
        Interop.Native.RatatuiParagraphAppendLineSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1);
        return this;
    }

    // Span-based overload can be reintroduced via AppendLineSpans.

    public Paragraph AppendSpan(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphAppendSpan(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
        return this;
    }

    // Zero-allocation overload from UTF-8 bytes (e.g. "text"u8)
    public unsafe Paragraph AppendSpan(ReadOnlySpan<byte> utf8, Style? style = null)
    {
        EnsureNotDisposed();
        var sty = style ?? default;
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = sty.ToFfi() };
        Interop.Native.RatatuiParagraphAppendSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1);
        return this;
    }

    // Batch append styled UTF-8 spans without allocations beyond the batch buffer.
    public Paragraph AppendSpans(ReadOnlySpan<Batching.SpanRun> runs)
    {
        EnsureNotDisposed();
        if (runs.IsEmpty) return this;
        Batching.WithFfiSpans(runs, (spans, len) =>
        {
            Interop.Native.RatatuiParagraphAppendSpans(_handle.DangerousGetHandle(), spans, len);
        });
        return this;
    }

    public Paragraph AppendLineSpans(ReadOnlySpan<Batching.SpanRun> runs)
    {
        EnsureNotDisposed();
        if (runs.IsEmpty)
        {
            // Force a line break if no content specified
            Interop.Native.RatatuiParagraphLineBreak(_handle.DangerousGetHandle());
            return this;
        }
        Batching.WithFfiSpans(runs, (spans, len) =>
        {
            Interop.Native.RatatuiParagraphAppendLineSpans(_handle.DangerousGetHandle(), spans, len);
        });
        return this;
    }

    public Paragraph NewLine()
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphLineBreak(_handle.DangerousGetHandle());
        return this;
    }

    public Paragraph Align(Alignment alignment)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetAlignment(_handle.DangerousGetHandle(), (uint)alignment);
        return this;
    }

    public Paragraph Wrap(bool trim = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetWrap(_handle.DangerousGetHandle(), trim);
        return this;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Paragraph));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}

public enum Alignment : uint
{
    Left = 0,
    Center = 1,
    Right = 2,
}
