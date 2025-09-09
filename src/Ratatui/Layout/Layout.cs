using System;
using System.Collections.Generic;

namespace Ratatui.Layout;

public enum ConstraintKind { Length, Percentage }

public readonly struct Constraint
{
    public ConstraintKind Kind { get; }
    public int Value { get; }
    private Constraint(ConstraintKind kind, int value) { Kind = kind; Value = value; }
    public static Constraint Length(int cells) => new(ConstraintKind.Length, cells);
    public static Constraint Percentage(int pct) => new(ConstraintKind.Percentage, pct);
}

public static class Layout
{
    public static IReadOnlyList<Rect> SplitVertical(Rect area, ReadOnlySpan<Constraint> constraints, int gap = 0, int margin = 0)
    {
        var inner = new Rect(area.X + margin, area.Y + margin, area.Width - 2*margin, area.Height - 2*margin);
        int fixedSum = 0; int pctSum = 0;
        foreach (var c in constraints) { if (c.Kind == ConstraintKind.Length) fixedSum += c.Value; else pctSum += c.Value; }
        var remaining = Math.Max(0, inner.Width - fixedSum - Math.Max(0, constraints.Length - 1) * gap);
        var result = new List<Rect>(constraints.Length);
        int cursorX = inner.X;
        foreach (var c in constraints)
        {
            int w = c.Kind == ConstraintKind.Length ? c.Value : (int)Math.Floor(remaining * (c.Value / (double)Math.Max(1, pctSum)));
            result.Add(new Rect(cursorX, inner.Y, w, inner.Height));
            cursorX += w + gap;
        }
        return result;
    }

    public static IReadOnlyList<Rect> SplitHorizontal(Rect area, ReadOnlySpan<Constraint> constraints, int gap = 0, int margin = 0)
    {
        var inner = new Rect(area.X + margin, area.Y + margin, area.Width - 2*margin, area.Height - 2*margin);
        int fixedSum = 0; int pctSum = 0;
        foreach (var c in constraints) { if (c.Kind == ConstraintKind.Length) fixedSum += c.Value; else pctSum += c.Value; }
        var remaining = Math.Max(0, inner.Height - fixedSum - Math.Max(0, constraints.Length - 1) * gap);
        var result = new List<Rect>(constraints.Length);
        int cursorY = inner.Y;
        foreach (var c in constraints)
        {
            int h = c.Kind == ConstraintKind.Length ? c.Value : (int)Math.Floor(remaining * (c.Value / (double)Math.Max(1, pctSum)));
            result.Add(new Rect(inner.X, cursorY, inner.Width, h));
            cursorY += h + gap;
        }
        return result;
    }
}

