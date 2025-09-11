using System;
using Ratatui;
using Ratatui.Testing;

static class Parity
{
    static int Failures = 0;

    static void Check(string name, string a, string b)
    {
        if (a == b)
        {
            Console.WriteLine($"[PASS] {name}");
        }
        else
        {
            Console.WriteLine($"[FAIL] {name}");
            Console.WriteLine("-- A --\n" + a);
            Console.WriteLine("-- B --\n" + b);
            Failures++;
        }
    }

    public static int Run()
    {
        // Paragraph title parity
        using (var p1 = new Paragraph("hello").Title("Title", border: true))
        using (var p2 = new Paragraph("hello"u8))
        {
            var title = "Title"u8;
            var spans = stackalloc Batching.SpanRun[1];
            spans[0] = new Batching.SpanRun(title, default);
            // Title via spans using block title spans setter
            Interop.Native.RatatuiParagraphSetBlockTitleSpans(p2.DangerousHandle, (IntPtr)(&spans[0]), (UIntPtr)1, true);
            var a = Headless.RenderParagraph(20, 3, p1);
            var b = Headless.RenderParagraph(20, 3, p2);
            Check("Paragraph.Title string vs spans", a, b);
        }

        // Gauge label + title parity
        using (var g1 = new Gauge().Ratio(0.5f).Label("Load 50%").Title("Gauge", true))
        using (var g2 = new Gauge().Ratio(0.5f).Label("Load 50%"u8).Title("Gauge"u8, true))
        {
            var a = Headless.RenderGauge(20, 3, g1);
            var b = Headless.RenderGauge(20, 3, g2);
            Check("Gauge.Label/Title string vs spans", a, b);
        }

        // LineGauge label parity
        using (var lg1 = new LineGauge().Ratio(0.42f).Label("42%"))
        using (var lg2 = new LineGauge().Ratio(0.42f).Label("42%"u8))
        {
            var a = Headless.RenderLineGauge(20, 1, lg1);
            var b = Headless.RenderLineGauge(20, 1, lg2);
            Check("LineGauge.Label string vs spans", a, b);
        }

        // Tabs divider parity (plain)
        using (var t1 = new Tabs().Titles("A","B").Divider("|"))
        using (var t2 = new Tabs().Titles("A","B").Divider("|"u8))
        {
            var a = Headless.RenderTabs(20, 3, t1);
            var b = Headless.RenderTabs(20, 3, t2);
            Check("Tabs.Divider string vs spans (plain)", a, b);
        }

        // Table title parity
        using (var tb1 = new Table().Title("T", true).Headers("A","B").AppendRow("1","2"))
        using (var tb2 = new Table().Title("T"u8, true).Headers("A","B").AppendRow("1","2"))
        {
            var a = Headless.RenderTable(20, 5, tb1);
            var b = Headless.RenderTable(20, 5, tb2);
            Check("Table.Title string vs spans", a, b);
        }

        // BarChart title + labels parity (labels text-only)
        using (var bc1 = new BarChart().Values(3,4,5).Labels("A\tB\tC").Title("Bars", true))
        using (var bc2 = new BarChart().Values(3,4,5).Labels("A\tB\tC"u8).Title("Bars"u8, true))
        {
            var a = Headless.RenderBarChart(20, 7, bc1);
            var b = Headless.RenderBarChart(20, 7, bc2);
            Check("BarChart.Labels/Title string vs spans", a, b);
        }

        // Sparkline title parity
        using (var sp1 = new Sparkline().Values([1,2,3,2,1]).Title("S", true))
        using (var sp2 = new Sparkline().Values([1,2,3,2,1]).Title("S"u8, true))
        {
            var a = Headless.RenderSparkline(20, 3, sp1);
            var b = Headless.RenderSparkline(20, 3, sp2);
            Check("Sparkline.Title string vs spans", a, b);
        }

        // Scrollbar title parity
        using (var sb1 = new Scrollbar().Title("Scroll", true))
        using (var sb2 = new Scrollbar().Title("Scroll"u8, true))
        {
            var a = Headless.RenderScrollbar(20, 3, sb1);
            var b = Headless.RenderScrollbar(20, 3, sb2);
            Check("Scrollbar.Title string vs spans", a, b);
        }

        // Canvas title parity
        using (var cv1 = new Canvas().Title("C", true).Bounds(0,10,0,10))
        using (var cv2 = new Canvas().Title("C"u8, true).Bounds(0,10,0,10))
        {
            var a = Headless.RenderCanvas(20, 5, cv1);
            var b = Headless.RenderCanvas(20, 5, cv2);
            Check("Canvas.Title string vs spans", a, b);
        }

        Console.WriteLine(Failures == 0 ? "All parity checks passed." : $"Failures: {Failures}");
        return Failures == 0 ? 0 : 1;
    }
}

return Parity.Run();

