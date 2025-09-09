using System;

namespace Ratatui.Testing;

public static class Inject
{
    public static void KeyChar(char c, bool ctrl = false, bool alt = false, bool shift = false)
    {
        byte mods = 0;
        if (shift) mods |= 0x1;
        if (alt) mods |= 0x2;
        if (ctrl) mods |= 0x4;
        Interop.Native.RatatuiInjectKey((uint)0 /*Char*/, (uint)c, mods);
    }

    public static void Key(uint code, uint ch = 0, byte mods = 0)
        => Interop.Native.RatatuiInjectKey(code, ch, mods);

    public static void Resize(ushort width, ushort height)
        => Interop.Native.RatatuiInjectResize(width, height);

    public static void Mouse(MouseKind kind, MouseButton btn, ushort x, ushort y, byte mods = 0)
        => Interop.Native.RatatuiInjectMouse((uint)kind, (uint)btn, x, y, mods);
}
