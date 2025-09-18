using Ratatui;

public static class ParagraphRecipe
{
    public static void Demo(Terminal term)
    {
        using var p = new Paragraph("Ratatui.cs ")
            .AppendSpan("styled", new Style(fg: Color.LightCyan, italic: true))
            .NewLine()
            .AppendSpan("wrap and align", new Style(underline: true))
            .Block(Block.Default
                .Borders(Borders.All)
                .BorderType(BorderType.Rounded)
                .Title("Para"))
            .Align(Alignment.Center)
            .Wrap();
        term.Draw(p, new Rect(0,0,30,5));
    }
}
