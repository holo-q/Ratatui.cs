using System;

namespace Ratatui;

[Flags]
public enum Borders : byte
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8,
    All = Left | Right | Top | Bottom,
}

public enum BorderType : uint { Plain = 0, Thick = 1, Double = 2 }

public readonly record struct Padding(ushort Left, ushort Top, ushort Right, ushort Bottom)
{
    public static Padding All(ushort v) => new(v, v, v, v);
}

public readonly record struct BlockAdv(Borders Borders, BorderType BorderType, Padding Pad, Alignment TitleAlignment)
{
    public static BlockAdv Default => new(Borders.All, BorderType.Plain, Padding.All(0), Alignment.Left);
}

public static class BlockAdvExtensions
{
    public static Paragraph WithBlock(this Paragraph p, in BlockAdv adv)
    {
        Interop.Native.RatatuiParagraphSetBlockAdv(p.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiParagraphSetBlockTitleAlignment(p.DangerousHandle, (uint)adv.TitleAlignment);
        return p;
    }

    public static List WithBlock(this List l, in BlockAdv adv)
    {
        Interop.Native.RatatuiListSetBlockAdv(l.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiListSetBlockTitleAlignment(l.DangerousHandle, (uint)adv.TitleAlignment);
        return l;
    }

    public static Table WithBlock(this Table t, in BlockAdv adv)
    {
        Interop.Native.RatatuiTableSetBlockAdv(t.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiTableSetBlockTitleAlignment(t.DangerousHandle, (uint)adv.TitleAlignment);
        return t;
    }

    public static Tabs WithBlock(this Tabs t, in BlockAdv adv)
    {
        Interop.Native.RatatuiTabsSetBlockAdv(t.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiTabsSetBlockTitleAlignment(t.DangerousHandle, (uint)adv.TitleAlignment);
        return t;
    }

    public static Gauge WithBlock(this Gauge g, in BlockAdv adv)
    {
        Interop.Native.RatatuiGaugeSetBlockAdv(g.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiGaugeSetBlockTitleAlignment(g.DangerousHandle, (uint)adv.TitleAlignment);
        return g;
    }

    public static BarChart WithBlock(this BarChart b, in BlockAdv adv)
    {
        Interop.Native.RatatuiBarChartSetBlockAdv(b.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        return b;
    }

    public static Sparkline WithBlock(this Sparkline s, in BlockAdv adv)
    {
        Interop.Native.RatatuiSparklineSetBlockAdv(s.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        return s;
    }

    public static Scrollbar WithBlock(this Scrollbar s, in BlockAdv adv)
    {
        Interop.Native.RatatuiScrollbarSetBlockAdv(s.DangerousHandle, (byte)adv.Borders, (uint)adv.BorderType,
            adv.Pad.Left, adv.Pad.Top, adv.Pad.Right, adv.Pad.Bottom, IntPtr.Zero, UIntPtr.Zero);
        Interop.Native.RatatuiScrollbarSetBlockTitleAlignment(s.DangerousHandle, (uint)adv.TitleAlignment);
        return s;
    }
}
