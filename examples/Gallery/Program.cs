using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ratatui;
using Ratatui.Layout;

class Program
{
    static async Task Main()
    {
        using var term = new Terminal();
        term.Clear();

        var running = true;
        var tick = 0;
        var selected = 0;
        var tabs = new Tabs().Titles("Overview", "Metrics", "Table");
        var list = new List().Title("Items");
        foreach (var s in Enumerable.Range(1, 20).Select(i => $"Item {i}")) list.AppendItem(s);
        var gauge = new Gauge().Title("Load").Ratio(0.10f);
        static ulong[] Spark(int t)
        {
            return Enumerable.Range(0, 40)
                .Select(i => (int)Math.Round(((Math.Sin((i + t) / 4.0) + 1.0) / 2.0) * 100))
                .Select(v => (ulong)Math.Clamp(v, 0, 100))
                .ToArray();
        }
        var spark = new Sparkline().Values(Spark(0));
        var table = new Table().Title("Data").Headers("A", "B");
        for (int i = 0; i < 10; i++) table.AppendRow($"{i}", $"{i*i}");

        using var cts = new CancellationTokenSource();
        var evTask = term.RunAsync(async ev =>
        {
            if (ev.Kind == EventKind.Key)
            {
                if (ev.Key.Code == 27) // Esc
                {
                    running = false; cts.Cancel();
                }
                else if (ev.Key.Code == (uint)ConsoleKey.LeftArrow)
                {
                    selected = Math.Max(0, selected - 1);
                }
                else if (ev.Key.Code == (uint)ConsoleKey.RightArrow)
                {
                    selected = Math.Min(2, selected + 1);
                }
            }
            await Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(16), cts.Token);

        while (running)
        {
            var (w, h) = term.Size();
            var area = new Rect(0, 0, w, h);
            var rows = Layout.SplitHorizontal(area, stackalloc Constraint[]
            {
                Constraint.Length(3),
                Constraint.Percentage(70),
                Constraint.Percentage(30)
            }, gap: 1, margin: 0);

            tabs.Selected(selected);
            gauge.Ratio((float)((Math.Sin(tick/12.0) + 1.0) / 2.0));
            spark.Values(Spark(tick));

            // Upper header row
            term.DrawFrame(
                DrawCommand.Tabs(tabs, rows[0])
            );

            // Middle content row split 2 columns
            var cols = Layout.SplitVertical(rows[1], stackalloc Constraint[] { Constraint.Percentage(40), Constraint.Percentage(60) }, gap: 1);
            term.DrawFrame(
                DrawCommand.List(list, cols[0]),
                DrawCommand.Table(table, cols[1])
            );

            // Bottom row: gauge + sparkline
            var bottom = Layout.SplitVertical(rows[2], stackalloc Constraint[] { Constraint.Length(rows[2].Width/3), Constraint.Percentage(100) }, gap: 1);
            term.DrawFrame(
                DrawCommand.Gauge(gauge, bottom[0]),
                DrawCommand.Sparkline(spark, bottom[1])
            );

            tick++;
            await Task.Delay(33);
        }

        cts.Cancel();
        try { await evTask; } catch { /* ignore */ }
    }
}
