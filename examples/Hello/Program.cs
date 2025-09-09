using Ratatui;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var term = new Terminal();
using var p = new Paragraph("Hello Ratatui from C#!\n\nPress Ctrl+C to exit...")
    .Title("Ratatui.cs", border: true)
    .AppendLine("", new Style())
    .AppendLine("Bold text example", new Style(fg: Color.LightGreen, bold: true))
    .AppendLine("Underlined text", new Style(fg: Color.LightBlue, underline: true));

using var list = new List()
    .Title("Things", border: true)
    .AppendItem("Apples", new Style(fg: Color.Yellow))
    .AppendItem("Bananas", new Style(fg: Color.LightYellow))
    .AppendItem("Cherries", new Style(fg: Color.Red))
    .AppendItem("Dates", new Style(fg: Color.Magenta));

using var table = new Table()
    .Title("Scores", border: true)
    .Headers("Name", "Score")
    .AppendRow("Alice", "42")
    .AppendRow("Bob", "1337")
    .AppendRow("Carol", "9001");

term.Clear();

var (w, h) = term.Size();
var full = Rect.FromSize(w, h);
var (left, right) = full.SplitVertical(w/2);
var (topRight, bottomRight) = right.SplitHorizontal(h/2);
Vec2i center = (w/2, h/2);

term.Draw(p, left);
term.Draw(list, topRight);
term.Draw(table, bottomRight);

// Simple event loop: exit on Ctrl+C
while (true)
{
    if (term.NextEvent(TimeSpan.FromMilliseconds(250), out var ev))
    {
        if (ev.Kind == EventKind.Key && ev.Key.Ctrl && (ev.Key.Char == 'c' || ev.Key.Char == 'C'))
            break;

        if (ev.Kind == EventKind.Resize)
        {
            (w, h) = term.Size();
            full = Rect.FromSize(w, h);
            (left, right) = full.SplitVertical(w/2);
            (topRight, bottomRight) = right.SplitHorizontal(h/2);
            center = (w/2, h/2);
            term.Draw(p, left);
            term.Draw(list, topRight);
            term.Draw(table, bottomRight);
        }
    }
}
