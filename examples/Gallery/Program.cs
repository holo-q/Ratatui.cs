using System;
using System.Threading;
using System.Threading.Tasks;
using Ratatui;

class Program
{
    static async Task<int> Main()
    {
        using var term = new Terminal();
        term.Clear();

        using var para = new Paragraph("Ratatui.cs ")
            .AppendSpan("KINO", new Style(fg: Color.LightMagenta, bold: true))
            .AppendSpan(" mode", new Style(italic: true))
            .NewLine()
            .AppendSpan("Align & Wrap", new Style(underline: true))
            .Align(Alignment.Center)
            .Wrap();

        using var list = new List()
            .Title("List")
            .AppendItem("Alpha")
            .AppendItem("Beta")
            .AppendItem("Gamma")
            .Selected(1);

        using var table = new Table()
            .Title("Table")
            .Headers("A", "B", "C")
            .AppendRow("1","2","3")
            .AppendRow("4","5","6")
            .ColumnPercents(34,33,33)
            .Selected(0)
            .HighlightSymbol("> ");

        using var chart = new Chart()
            .Title("Chart")
            .Axes("x","y").AxesBounds(0,2,0,3)
            .Line("L1", (ReadOnlySpan<(double,double)>)new (double,double)[] { (0.0,1.0), (1.0,2.5), (2.0,1.2) });

        var (w,h) = term.Size();
        var rect = new Rect(0,0,w,Math.Max(20,h));
        var left = new Rect(rect.X, rect.Y, rect.Width/2, rect.Height/2);
        var right = new Rect(rect.X + rect.Width/2, rect.Y, rect.Width - rect.Width/2, rect.Height/2);
        var bottom = new Rect(rect.X, rect.Y + rect.Height/2, rect.Width, rect.Height - rect.Height/2);

        // Demonstrate buffered frame rendering
        term.PushFrame();
        term.Draw(para, left);
        term.Draw(list, right);
        term.Draw(table, new Rect(bottom.X, bottom.Y, bottom.Width/2, bottom.Height));
        term.Draw(chart, new Rect(bottom.X + bottom.Width/2, bottom.Y, bottom.Width - bottom.Width/2, bottom.Height));
        term.PopFrame();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
        await term.RunAsync(evt =>
        {
            if (evt.Kind == EventKind.Key)
            {
                if (evt.Key.Char == (uint)'q' || evt.Key.Char == (uint)'Q') cts.Cancel();
            }
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(50), cts.Token);

        return 0;
    }
}
