using Ratatui;

public static class ListTableChartRecipe
{
    public static void Demo(Terminal term)
    {
        using var list = new List()
            .Title("Items")
            .Items("Alpha", "Beta", "Gamma")
            .Selected(1);

        using var table = new Table()
            .Title("Data")
            .Headers("A", "B")
            .AppendRow("1", "2")
            .AppendRow("3", "4")
            .Selected(0)
            .HighlightSymbol("→ ");

        using var chart = new Chart()
            .Title("Chart")
            .Axes("x", "y")
            .Line("L1", new [] { (0.0, 1.0), (1.0, 2.0), (2.0, 1.5) });

        term.Draw(list, new Rect(0,0,20,8));
        term.Draw(table, new Rect(20,0,22,8));
        term.Draw(chart, new Rect(0,8,42,8));
    }
}

