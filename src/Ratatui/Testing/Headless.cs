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
}
