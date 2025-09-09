using System;
using System.Collections.Generic;
using System.Linq;

namespace Ratatui;

public readonly struct DrawCommand
{
    internal readonly Interop.Native.FfiDrawCmd Ffi;

    private DrawCommand(Interop.Native.FfiWidgetKind kind, IntPtr handle, Rect rect)
    {
        Ffi = new Interop.Native.FfiDrawCmd
        {
            Kind = (uint)kind,
            Handle = handle,
            Rect = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height },
        };
    }

    public static DrawCommand Paragraph(Paragraph p, Rect rect) => new(Interop.Native.FfiWidgetKind.Paragraph, p.DangerousHandle, rect);
    public static DrawCommand List(List l, Rect rect) => new(Interop.Native.FfiWidgetKind.List, l.DangerousHandle, rect);
    public static DrawCommand Table(Table t, Rect rect) => new(Interop.Native.FfiWidgetKind.Table, t.DangerousHandle, rect);
    public static DrawCommand Gauge(Gauge g, Rect rect) => new(Interop.Native.FfiWidgetKind.Gauge, g.DangerousHandle, rect);
    public static DrawCommand Tabs(Tabs t, Rect rect) => new(Interop.Native.FfiWidgetKind.Tabs, t.DangerousHandle, rect);

    internal static Interop.Native.FfiDrawCmd[] ToFfi(ReadOnlySpan<DrawCommand> cmds)
    {
        var arr = new Interop.Native.FfiDrawCmd[cmds.Length];
        for (int i = 0; i < cmds.Length; i++) arr[i] = cmds[i].Ffi;
        return arr;
    }

    internal static Interop.Native.FfiDrawCmd[] ToFfi(params DrawCommand[] cmds) => ToFfi((ReadOnlySpan<DrawCommand>)cmds);
}
