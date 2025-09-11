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
    public static Paragraph SpanUtf8(this Paragraph p, ReadOnlySpan<byte> utf8, Style? style = null)
        => p.AppendSpan(utf8, style);

    public static Paragraph LineUtf8(this Paragraph p, ReadOnlySpan<byte> utf8, Style? style = null)
        => p.AppendLine(utf8, style);
}
