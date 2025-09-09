using System;
using Ratatui;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            using var term = new Terminal();
            using var para = new Paragraph("Hello AOT").Title("AOT");
            term.Draw(para, new Rect(0, 0, 20, 3));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
