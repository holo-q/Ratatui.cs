using Ratatui;
using Ratatui.Testing;

using var p = new Paragraph("Frame test")
    .Title("P", true)
    .AppendLine("Left", new Style(fg: Color.LightGreen));

using var list = new List()
    .Title("L", true)
    .AppendItem("One")
    .AppendItem("Two");

using var table = new Table()
    .Title("T", true)
    .Headers("A","B")
    .AppendRow("1","2");

using var gauge = new Gauge().Title("G", true).Label("50%").Ratio(0.5f);
using var tabs = new Tabs().Title("Tabs", true).Titles("Home","Edit","View").Selected(1);
using var bars = new BarChart().Title("Bars", true).Labels("A","B","C","D").Values(1,3,2,5);
using var spark = new Sparkline().Title("Spark", true).Values(1,2,3,4,5,4,3,2,1);

var cmds = new []{
    DrawCommand.Paragraph(p, new Rect(0,0,20,5)),
    DrawCommand.List(list, new Rect(20,0,20,5)),
    DrawCommand.Table(table, new Rect(0,5,18,5)),
    DrawCommand.Gauge(gauge, new Rect(18,5,11,2)),
    DrawCommand.Sparkline(spark, new Rect(29,5,11,2)),
    DrawCommand.Tabs(tabs, new Rect(18,7,22,1)),
    DrawCommand.BarChart(bars, new Rect(29,6,11,4)),
};

var snapshot = Headless.RenderFrame(40, 10, cmds);
Console.WriteLine("=== Frame Snapshot ===\n" + snapshot + "\n======================");
