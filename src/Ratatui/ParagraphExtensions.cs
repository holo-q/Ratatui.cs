using System;

namespace Ratatui;

// Lightweight ergonomic aliases over Paragraph. Keeps core thin; zero new state.
public static class ParagraphExtensions
{
    // Line/Span verb aliases
    public static Paragraph Line(this Paragraph p, string text, Style? style = null)
        => p.AppendLine(text, style);

    public static Paragraph Span(this Paragraph p, string text, Style? style = null)
        => p.AppendSpan(text, style);

    // Span-first batching aliases
    public static Paragraph Spans(this Paragraph p, ReadOnlySpan<Batching.SpanRun> runs)
        => p.AppendSpans(runs);

    public static Paragraph Line(this Paragraph p, ReadOnlySpan<Batching.SpanRun> runs)
        => p.AppendLineSpans(runs);

    // Convenience newline count overload
    public static Paragraph NewLine(this Paragraph p, int count)
    {
        for (int i = 0; i < count; i++) p.NewLine();
        return p;
    }

    // Block sugar
    public static Paragraph Block(this Paragraph p)
        => p.WithBlock(BlockAdv.Default);

    public static Paragraph Block(this Paragraph p, in BlockAdv adv)
        => p.WithBlock(adv);

    public static Paragraph Bordered(this Paragraph p)
        => p.WithBlock(BlockAdv.Default);

    // Tiny niceties
    public static Paragraph Space(this Paragraph p)
        => p.AppendSpan(" ");

    // Zero-alloc overloads that accept UTF-8 bytes (e.g., "text"u8)
    public static unsafe Paragraph SpanUtf8(this Paragraph p, ReadOnlySpan<byte> utf8, Style? style = null)
    {
        var sty = style ?? default;
        // Ensure NUL-terminated buffer on stack
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        // Build single-span array on stack and invoke FFI directly
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = sty.ToFfi() };
        Interop.Native.RatatuiParagraphAppendSpans(p.DangerousHandle, (IntPtr)spans, (UIntPtr)1);
        return p;
    }

    public static unsafe Paragraph LineUtf8(this Paragraph p, ReadOnlySpan<byte> utf8, Style? style = null)
    {
        var sty = style ?? default;
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = sty.ToFfi() };
        Interop.Native.RatatuiParagraphAppendLineSpans(p.DangerousHandle, (IntPtr)spans, (UIntPtr)1);
        return p;
    }
}
