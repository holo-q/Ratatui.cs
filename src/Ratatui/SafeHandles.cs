using System;
using System.Runtime.InteropServices;

namespace Ratatui;

public sealed class TerminalHandle : SafeHandle
{
    public TerminalHandle() : base(IntPtr.Zero, ownsHandle: true) {}

    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);

    internal static TerminalHandle FromRaw(IntPtr ptr)
    {
        var h = new TerminalHandle();
        h.SetHandle(ptr);
        return h;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Interop.Native.RatatuiTerminalFree(handle);
        }
        return true;
    }
}

public sealed class ParagraphHandle : SafeHandle
{
    public ParagraphHandle() : base(IntPtr.Zero, ownsHandle: true) {}

    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);

    internal static ParagraphHandle FromRaw(IntPtr ptr)
    {
        var h = new ParagraphHandle();
        h.SetHandle(ptr);
        return h;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Interop.Native.RatatuiParagraphFree(handle);
        }
        return true;
    }
}

public sealed class ListHandle : SafeHandle
{
    public ListHandle() : base(IntPtr.Zero, ownsHandle: true) {}

    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);

    internal static ListHandle FromRaw(IntPtr ptr)
    {
        var h = new ListHandle();
        h.SetHandle(ptr);
        return h;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Interop.Native.RatatuiListFree(handle);
        }
        return true;
    }
}

public sealed class TableHandle : SafeHandle
{
    public TableHandle() : base(IntPtr.Zero, ownsHandle: true) {}

    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);

    internal static TableHandle FromRaw(IntPtr ptr)
    {
        var h = new TableHandle();
        h.SetHandle(ptr);
        return h;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Interop.Native.RatatuiTableFree(handle);
        }
        return true;
    }
}

public sealed class GaugeHandle : SafeHandle
{
    public GaugeHandle() : base(IntPtr.Zero, ownsHandle: true) {}
    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);
    internal static GaugeHandle FromRaw(IntPtr ptr) { var h = new GaugeHandle(); h.SetHandle(ptr); return h; }
    protected override bool ReleaseHandle() { if (!IsInvalid) Interop.Native.RatatuiGaugeFree(handle); return true; }
}

public sealed class TabsHandle : SafeHandle
{
    public TabsHandle() : base(IntPtr.Zero, ownsHandle: true) {}
    public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);
    internal static TabsHandle FromRaw(IntPtr ptr) { var h = new TabsHandle(); h.SetHandle(ptr); return h; }
    protected override bool ReleaseHandle() { if (!IsInvalid) Interop.Native.RatatuiTabsFree(handle); return true; }
}
