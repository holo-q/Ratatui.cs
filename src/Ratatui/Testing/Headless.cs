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
}
