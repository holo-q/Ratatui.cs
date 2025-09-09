namespace Ratatui;

public readonly record struct Rect(int X, int Y, int Width, int Height)
{
    public static Rect FromSize(int width, int height) => new(0, 0, width, height);
    public Rect SplitLeft(int w) => new(X, Y, w, Height);
    public Rect SplitRight(int w) => new(X + Width - w, Y, w, Height);
    public (Rect Left, Rect Right) SplitVertical(int leftWidth) => (SplitLeft(leftWidth), new Rect(X + leftWidth, Y, Width - leftWidth, Height));
    public (Rect Top, Rect Bottom) SplitHorizontal(int topHeight) => (new Rect(X, Y, Width, topHeight), new Rect(X, Y + topHeight, Width, Height - topHeight));

    // Vector-friendly aliases and helpers
    public int x => X;
    public int y => Y;
    public int w => Width;
    public int h => Height;
    public Vec2i pos => new(X, Y);
    public Vec2i xy => new(X, Y);
    public Vec2i size => new(Width, Height);

    public static Rect From(Vec2i pos, Vec2i size) => new(pos.X, pos.Y, size.X, size.Y);
    public Rect WithPos(Vec2i p) => new(p.X, p.Y, Width, Height);
    public Rect WithSize(Vec2i s) => new(X, Y, s.X, s.Y);
    public bool Contains(Vec2i p) => p.X >= X && p.Y >= Y && p.X < X + Width && p.Y < Y + Height;
}

public enum EventKind { None = 0, Key = 1, Resize = 2 }

public readonly struct KeyEvent(uint code, uint ch, byte mods)
{
    public uint Code { get; } = code;
    public uint Char { get; } = ch;
    public bool Shift => (mods & 0x1) != 0;
    public bool Alt => (mods & 0x2) != 0;
    public bool Ctrl => (mods & 0x4) != 0;
}

public readonly struct Event
{
    public EventKind Kind { get; init; }
    public KeyEvent Key { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    internal static Event FromFfi(Interop.Native.FfiEvent fe)
    {
        return new Event
        {
            Kind = (EventKind)fe.Kind,
            Key = new KeyEvent(fe.Key.Code, fe.Key.Ch, fe.Key.Mods),
            Width = fe.Width,
            Height = fe.Height,
        };
    }
}
