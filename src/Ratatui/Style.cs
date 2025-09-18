namespace Ratatui;

public enum Color
{
    Reset = 0,
    Black, Red, Green, Yellow, Blue, Magenta, Cyan, Gray,
    DarkGray, LightRed, LightGreen, LightYellow, LightBlue, LightMagenta, LightCyan, White,
}

public readonly struct Style
{
    public Color? Fg { get; }
    public Color? Bg { get; }
    public bool Bold { get; }
    public bool Italic { get; }
    public bool Underline { get; }
    public bool Dim { get; }
    public bool Crossed { get; }
    public bool Reversed { get; }
    public bool RapidBlink { get; }
    public bool SlowBlink { get; }

    public Style(Color? fg = null, Color? bg = null, bool bold = false, bool italic = false, bool underline = false, bool dim = false, bool crossed = false, bool reversed = false, bool rapidBlink = false, bool slowBlink = false)
    {
        Fg = fg; Bg = bg; Bold = bold; Italic = italic; Underline = underline; Dim = dim; Crossed = crossed; Reversed = reversed; RapidBlink = rapidBlink; SlowBlink = slowBlink;
    }

    public static StyledSpan operator |(Style style, string text)
        => new StyledSpan(text, style);

    public static StyledSpan operator |(string text, Style style)
        => new StyledSpan(text, style);

    internal Interop.Native.FfiStyle ToFfi()
    {
        var mods = Interop.Native.FfiStyleMods.None;
        if (Bold) mods |= Interop.Native.FfiStyleMods.Bold;
        if (Italic) mods |= Interop.Native.FfiStyleMods.Italic;
        if (Underline) mods |= Interop.Native.FfiStyleMods.Underline;
        if (Dim) mods |= Interop.Native.FfiStyleMods.Dim;
        if (Crossed) mods |= Interop.Native.FfiStyleMods.Crossed;
        if (Reversed) mods |= Interop.Native.FfiStyleMods.Reversed;
        if (RapidBlink) mods |= Interop.Native.FfiStyleMods.RapidBlink;
        if (SlowBlink) mods |= Interop.Native.FfiStyleMods.SlowBlink;
        return new Interop.Native.FfiStyle
        {
            Fg = (uint)(Fg.HasValue ? (Interop.Native.FfiColor)Fg.Value : Interop.Native.FfiColor.Reset),
            Bg = (uint)(Bg.HasValue ? (Interop.Native.FfiColor)Bg.Value : Interop.Native.FfiColor.Reset),
            Mods = (ushort)mods,
        };
    }
}
