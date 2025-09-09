using System;
using System.IO;
using System.Text;
using Ratatui;
using Ratatui.Testing;

class Program
{
    static int Main()
    {
        Directory.CreateDirectory("artifacts/snapshots");
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        // Paragraph styled spans
        using (var p = new Paragraph("Ratatui.cs ")
            .AppendSpan("snap", new Style(fg: Color.LightCyan, italic: true))
            .NewLine()
            .AppendSpan("wrap & center", new Style(underline: true))
            .Align(Alignment.Center)
            .Wrap())
        {
            var txt = Headless.RenderParagraph(32, 6, p);
            File.WriteAllText("artifacts/snapshots/paragraph.txt", txt, utf8NoBom);
        }

        // Table with column percents
        using (var tbl = new Table()
            .Title("Table")
            .Headers("A", "B", "C")
            .AppendRow("1", "2", "3")
            .AppendRow("4", "5", "6")
            .ColumnPercents(34, 33, 33)
            .Selected(1))
        {
            var txt = Headless.RenderTable(30, 8, tbl);
            File.WriteAllText("artifacts/snapshots/table.txt", txt, utf8NoBom);
        }

        // Chart simple
        using (var ch = new Chart().Title("Chart").Axes("x","y").AxesBounds(0, 2, 0, 3)
            .Line("L1", (ReadOnlySpan<(double,double)>)new (double,double)[] { (0,1), (1,2.5), (2,1.2) }))
        {
            var txt = Headless.RenderChart(32, 10, ch);
            File.WriteAllText("artifacts/snapshots/chart.txt", txt, utf8NoBom);
        }

        // Tabs snapshot
        using (var tabs = new Tabs().Title("Tabs").Titles("One", "Two", "Three").Selected(1))
        {
            var txt = Headless.RenderTabs(32, 3, tabs);
            File.WriteAllText("artifacts/snapshots/tabs.txt", txt, utf8NoBom);
        }

        // Gauge snapshot
        using (var gauge = new Gauge().Title("Gauge").Label("42% ").Ratio(0.42f))
        {
            var txt = Headless.RenderGauge(32, 3, gauge);
            File.WriteAllText("artifacts/snapshots/gauge.txt", txt, utf8NoBom);
        }

        // Combined frame (mini dashboard)
        using (var para2 = new Paragraph("Dashboard").Title("Header").Align(Alignment.Center))
        using (var list = new List().Title("List").AppendItem("Alpha").AppendItem("Beta").AppendItem("Gamma").Selected(2))
        using (var table2 = new Table().Title("Table").Headers("A","B").AppendRow("1","2").AppendRow("3","4").Selected(1))
        using (var chart2 = new Chart().Title("Chart").Axes("x","y").Line("L1", (ReadOnlySpan<(double,double)>)new (double,double)[]{ (0,0.5),(1,1.0),(2,0.8) }))
        using (var gauge2 = new Gauge().Title("CPU").Label("72% ").Ratio(0.72f))
        using (var tabs2 = new Tabs().Titles("Home","Stats","Logs").Selected(0))
        {
            int W = 64, H = 20;
            var top = new Rect(0,0,W,3);
            var body = new Rect(0,3,W,H-3);
            var left = new Rect(0,3,W/2,H-3);
            var right = new Rect(W/2,3,W - W/2,H-3);
            var halfLeft = new Rect(0,3,W/2,(H-3)/2);
            var halfLeft2 = new Rect(0,3+(H-3)/2,W/2,(H-3) - (H-3)/2);
            var halfRight = new Rect(W/2,3,(W - W/2),(H-3)/2);
            var halfRight2 = new Rect(W/2,3+(H-3)/2,(W - W/2),(H-3) - (H-3)/2);

            var frameTxt = Headless.RenderFrame(
                W, H,
                DrawCommand.Paragraph(para2, top),
                DrawCommand.Tabs(tabs2, new Rect(0,3,W,1)),
                DrawCommand.List(list, halfLeft),
                DrawCommand.Table(table2, halfLeft2),
                DrawCommand.Gauge(gauge2, halfRight),
                DrawCommand.Chart(chart2, halfRight2)
            );
            File.WriteAllText("artifacts/snapshots/combined.txt", frameTxt, utf8NoBom);
        }

        return 0;
    }
}
