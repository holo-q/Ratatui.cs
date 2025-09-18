namespace Ratatui;

public readonly record struct Rect(int X, int Y, int Width, int Height)
{
    public static Rect                    FromSize(int        width, int height) => new(0, 0, width, height);
    public        Rect                    SplitLeft(int       w)         => this with { Width = w };
    public        Rect                    SplitRight(int      w)         => this with { X = X + Width - w, Width = w };
    public        (Rect Left, Rect Right) SplitVertical(int   leftWidth) => (SplitLeft(leftWidth), this with { X = X + leftWidth, Width = Width - leftWidth });
    public        (Rect Top, Rect Bottom) SplitHorizontal(int topHeight) => (this with { Height = topHeight }, this with { Y = Y + topHeight, Height = Height - topHeight });

    // Vector-friendly aliases and helpers
    public int x => X;
    public int y => Y;
    public int w => Width;
    public int h => Height;
    public Vec2i pos => new(X, Y);
    public Vec2i xy => new(X, Y);
    public Vec2i size => new(Width, Height);

    public static Rect From(Vec2i     pos, Vec2i size) => new(pos.X, pos.Y, size.X, size.Y);
    public        Rect WithPos(Vec2i  p) => this with { X = p.X, Y = p.Y };
    public        Rect WithSize(Vec2i s) => this with { Width = s.X, Height = s.Y };
    public        bool Contains(Vec2i p) => p.X >= X && p.Y >= Y && p.X < X + Width && p.Y < Y + Height;
}

public enum EventKind { None = 0, Key = 1, Resize = 2, Mouse = 3 }

public readonly struct KeyEvent(uint code, uint ch, byte mods)
{
    public uint Code { get; } = code;
    public uint Char { get; } = ch;
    public bool Shift => (mods & 0x1) != 0;
    public bool Alt => (mods & 0x2) != 0;
    public bool Ctrl => (mods & 0x4) != 0;
    public KeyCode CodeEnum => (KeyCode)Code;
}

public readonly struct Event
{
    public EventKind Kind { get; init; }
    public KeyEvent Key { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public MouseEvent Mouse { get; init; }

    internal static Event FromFfi(Interop.Native.FfiEvent fe)
    {
        return new Event
        {
            Kind = (EventKind)fe.Kind,
            Key = new KeyEvent(fe.Key.Code, fe.Key.Ch, fe.Key.Mods),
            Width = fe.Width,
            Height = fe.Height,
            Mouse = MouseEvent.FromFfi(fe),
        };
    }
}

public enum KeyCode : uint
{
    Char = 0,
    Enter = 1,
    Left = 2,
    Right = 3,
    Up = 4,
    Down = 5,
    Esc = 6,
    Backspace = 7,
    Tab = 8,
    Delete = 9,
    Home = 10,
    End = 11,
    PageUp = 12,
    PageDown = 13,
    Insert = 14,
    F1 = 100, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
}

public enum MouseKind { Down = 1, Up = 2, Drag = 3, Moved = 4, ScrollUp = 5, ScrollDown = 6 }
public enum MouseButton { None = 0, Left = 1, Right = 2, Middle = 3 }

public readonly struct MouseEvent
{
    public MouseKind Kind { get; }
    public MouseButton Button { get; }
    public int X { get; }
    public int Y { get; }
    public bool Shift { get; }
    public bool Alt { get; }
    public bool Ctrl { get; }

    private MouseEvent(MouseKind kind, MouseButton button, int x, int y, bool shift, bool alt, bool ctrl)
    { Kind = kind; Button = button; X = x; Y = y; Shift = shift; Alt = alt; Ctrl = ctrl; }

    internal static MouseEvent FromFfi(Interop.Native.FfiEvent fe)
    {
        var mods = fe.MouseMods;
        return new MouseEvent(
            (MouseKind)fe.MouseKind,
            (MouseButton)fe.MouseBtn,
            fe.MouseX,
            fe.MouseY,
            (mods & 0x1) != 0,
            (mods & 0x2) != 0,
            (mods & 0x4) != 0
        );
    }
}
