using Ratatui;

public static class ParagraphRecipe
{
    public static void Demo(Terminal term)
    {
        using var p = new Paragraph("Ratatui.cs\nwrap and align").Title("Para");
        term.Draw(p, new Rect(0,0,30,5));
    }
}

