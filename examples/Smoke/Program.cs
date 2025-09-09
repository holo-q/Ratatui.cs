using Ratatui;
using Ratatui.Testing;

// Build a paragraph with styles
using var p = new Paragraph("Hello headless!")
    .Title("Smoke", border: true)
    .AppendLine("", new Style())
    .AppendLine("Styled", new Style(fg: Color.LightGreen, bold: true));

var snapshot = Headless.RenderParagraph(30, 6, p);
Console.WriteLine("=== Headless Snapshot ===\n" + snapshot + "\n==========================");
