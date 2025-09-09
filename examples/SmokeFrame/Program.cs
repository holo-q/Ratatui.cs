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

var cmds = new []{
    DrawCommand.Paragraph(p, new Rect(0,0,20,5)),
    DrawCommand.List(list, new Rect(20,0,20,5)),
    DrawCommand.Table(table, new Rect(0,5,40,5)),
};

var snapshot = Headless.RenderFrame(40, 10, cmds);
Console.WriteLine("=== Frame Snapshot ===\n" + snapshot + "\n======================");
