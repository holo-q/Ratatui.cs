using System;
using System.Runtime.InteropServices;

namespace Ratatui.Testing;

public static class Headless
{
    public static string RenderParagraph(int width, int height, Paragraph paragraph)
    {
        if (paragraph is null) throw new ArgumentNullException(nameof(paragraph));
        var ok = Interop.Native.RatatuiHeadlessRenderParagraph((ushort)width, (ushort)height, paragraph.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless render failed");
        try
        {
            return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
        }
        finally
        {
            Interop.Native.RatatuiStringFree(ptr);
        }
    }

    public static string RenderList(int width, int height, List list)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));
        var ok = Interop.Native.RatatuiHeadlessRenderList((ushort)width, (ushort)height, list.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless list render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderTable(int width, int height, Table table)
    {
        if (table is null) throw new ArgumentNullException(nameof(table));
        var ok = Interop.Native.RatatuiHeadlessRenderTable((ushort)width, (ushort)height, table.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless table render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderFrame(int width, int height, params DrawCommand[] commands)
    {
        var ffi = DrawCommand.ToFfi(commands);
        var ok = Interop.Native.RatatuiHeadlessRenderFrame((ushort)width, (ushort)height, ffi, (UIntPtr)ffi.Length, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless frame render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderGauge(int width, int height, Gauge g)
    {
        if (g is null) throw new ArgumentNullException(nameof(g));
        var ok = Interop.Native.RatatuiHeadlessRenderGauge((ushort)width, (ushort)height, g.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless gauge render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderTabs(int width, int height, Tabs t)
    {
        if (t is null) throw new ArgumentNullException(nameof(t));
        var ok = Interop.Native.RatatuiHeadlessRenderTabs((ushort)width, (ushort)height, t.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless tabs render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderBarChart(int width, int height, BarChart b)
    {
        if (b is null) throw new ArgumentNullException(nameof(b));
        var ok = Interop.Native.RatatuiHeadlessRenderBarChart((ushort)width, (ushort)height, b.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless bar chart render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderSparkline(int width, int height, Sparkline s)
    {
        if (s is null) throw new ArgumentNullException(nameof(s));
        var ok = Interop.Native.RatatuiHeadlessRenderSparkline((ushort)width, (ushort)height, s.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless sparkline render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderScrollbar(int width, int height, Scrollbar s)
    {
        if (s is null) throw new ArgumentNullException(nameof(s));
        var ok = Interop.Native.RatatuiHeadlessRenderScrollbar((ushort)width, (ushort)height, s.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless scrollbar render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderChart(int width, int height, Chart c)
    {
        if (c is null) throw new ArgumentNullException(nameof(c));
        var ok = Interop.Native.RatatuiHeadlessRenderChart((ushort)width, (ushort)height, c.DangerousHandle, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless chart render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderFrameStyles(int width, int height, params DrawCommand[] commands)
    {
        var ffi = DrawCommand.ToFfi(commands);
        var ok = Interop.Native.RatatuiHeadlessRenderFrameStyles((ushort)width, (ushort)height, ffi, (UIntPtr)ffi.Length, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless frame styles render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public static string RenderFrameStylesEx(int width, int height, params DrawCommand[] commands)
    {
        var ffi = DrawCommand.ToFfi(commands);
        var ok = Interop.Native.RatatuiHeadlessRenderFrameStylesEx((ushort)width, (ushort)height, ffi, (UIntPtr)ffi.Length, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless frame styles_ex render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }

    public readonly struct CellInfo
    {
        public readonly uint Ch;
        public readonly uint Fg;
        public readonly uint Bg;
        public readonly ushort Mods;
        public CellInfo(uint ch, uint fg, uint bg, ushort mods) { Ch = ch; Fg = fg; Bg = bg; Mods = mods; }
    }

    public static unsafe CellInfo[] RenderFrameCells(int width, int height, params DrawCommand[] commands)
    {
        if (width <= 0 || height <= 0) return Array.Empty<CellInfo>();
        var ffi = DrawCommand.ToFfi(commands);
        var cap = checked(width * height);
        var cells = new Interop.Native.FfiCellInfo[cap];
        fixed (Interop.Native.FfiCellInfo* p = cells)
        {
            var ok = Interop.Native.RatatuiHeadlessRenderFrameCells((ushort)width, (ushort)height, ffi, (UIntPtr)ffi.Length, (IntPtr)p, (UIntPtr)cap);
            if (!ok) throw new InvalidOperationException("Headless frame cells render failed");
        }
        var outArr = new CellInfo[cap];
        for (int i = 0; i < cap; i++)
        {
            outArr[i] = new CellInfo(cells[i].Ch, cells[i].Fg, cells[i].Bg, cells[i].Mods);
        }
        return outArr;
    }

    public static string RenderClear(int width, int height)
    {
        var ok = Interop.Native.RatatuiHeadlessRenderClear((ushort)width, (ushort)height, out var ptr);
        if (!ok || ptr == IntPtr.Zero) throw new InvalidOperationException("Headless clear render failed");
        try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
        finally { Interop.Native.RatatuiStringFree(ptr); }
    }
}
