using System;
using System.Runtime.InteropServices;

namespace Ratatui;

public static class Extensions
{
    // List sugar
    public static List Item(this List l, ReadOnlySpan<byte> utf8, Style? style = null)
        => l.AppendItem(utf8, style);

    // Gauge / LineGauge sugar
    public static Gauge LabelUtf8(this Gauge g, ReadOnlySpan<byte> utf8)
        => g.Label(utf8);
    public static LineGauge LabelUtf8(this LineGauge g, ReadOnlySpan<byte> utf8)
        => g.Label(utf8);

    // Tabs sugar
    public static Tabs DividerUtf8(this Tabs t, ReadOnlySpan<byte> utf8)
        => t.Divider(utf8);

    // BarChart sugar
    public static BarChart LabelsUtf8(this BarChart b, ReadOnlySpan<byte> tsvUtf8)
        => b.Labels(tsvUtf8);

    // Table sugar
    public static Table Header(this Table t, ReadOnlySpan<byte> utf8, Style? style = null)
    {
        var run = new Batching.SpanRun(utf8.ToArray(), style ?? default);
        var span = MemoryMarshal.CreateReadOnlySpan(ref run, 1);
        return t.Headers(span);
    }
}
