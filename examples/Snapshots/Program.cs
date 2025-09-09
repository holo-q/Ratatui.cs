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

        // Paragraph styled spans
        using (var p = new Paragraph("Ratatui.cs ")
            .AppendSpan("snap", new Style(fg: Color.LightCyan, italic: true))
            .NewLine()
            .AppendSpan("wrap & center", new Style(underline: true))
            .Align(Alignment.Center)
            .Wrap())
        {
            var txt = Headless.RenderParagraph(32, 6, p);
            File.WriteAllText("artifacts/snapshots/paragraph.txt", txt, Encoding.UTF8);
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
            File.WriteAllText("artifacts/snapshots/table.txt", txt, Encoding.UTF8);
        }

        // Chart simple
        using (var ch = new Chart().Title("Chart").Axes("x","y").AxesBounds(0, 2, 0, 3)
            .Line("L1", (ReadOnlySpan<(double,double)>)new (double,double)[] { (0,1), (1,2.5), (2,1.2) }))
        {
            var txt = Headless.RenderChart(32, 10, ch);
            File.WriteAllText("artifacts/snapshots/chart.txt", txt, Encoding.UTF8);
        }

        return 0;
    }
}
