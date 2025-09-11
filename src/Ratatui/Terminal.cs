using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ratatui;

public sealed class Terminal : IDisposable
{
    private readonly TerminalHandle _handle;
    private bool _disposed;
    private bool _raw;
    private bool _alt;
    private bool _cursorShown = true;

    public Terminal()
    {
        // Force static ctor of Native to run and set resolver
        var _ = typeof(Interop.Native);

        var ptr = Interop.Native.RatatuiInitTerminal();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to initialize Ratatui terminal");
        _handle = TerminalHandle.FromRaw(ptr);
    }

    public void Clear()
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTerminalClear(_handle.DangerousGetHandle());
    }

    // Ergonomic toggles (RAII-safe): Raw mode, Alt screen, Cursor visibility
    public Terminal Raw(bool on = true)
    {
        EnsureNotDisposed();
        var ok = on ? Interop.Native.RatatuiTerminalEnableRaw(_handle.DangerousGetHandle())
                    : Interop.Native.RatatuiTerminalDisableRaw(_handle.DangerousGetHandle());
        if (ok) _raw = on;
        return this;
    }

    public Terminal AltScreen(bool on = true)
    {
        EnsureNotDisposed();
        var ok = on ? Interop.Native.RatatuiTerminalEnterAlt(_handle.DangerousGetHandle())
                    : Interop.Native.RatatuiTerminalLeaveAlt(_handle.DangerousGetHandle());
        if (ok) _alt = on;
        return this;
    }

    public Terminal ShowCursor(bool show = true)
    {
        EnsureNotDisposed();
        if (Interop.Native.RatatuiTerminalShowCursor(_handle.DangerousGetHandle(), show))
            _cursorShown = show;
        return this;
    }

    public (ushort X, ushort Y) GetCursor()
    {
        EnsureNotDisposed();
        if (!Interop.Native.RatatuiTerminalGetCursorPosition(_handle.DangerousGetHandle(), out var x, out var y))
            throw new InvalidOperationException("GetCursor failed");
        return (x, y);
    }

    public Terminal SetCursor(ushort x, ushort y)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTerminalSetCursorPosition(_handle.DangerousGetHandle(), x, y);
        return this;
    }

    public Rect Viewport
    {
        get
        {
            EnsureNotDisposed();
            if (!Interop.Native.RatatuiTerminalGetViewportArea(_handle.DangerousGetHandle(), out var r))
                return default;
            return new Rect(r.X, r.Y, r.Width, r.Height);
        }
        set
        {
            EnsureNotDisposed();
            var r = new Interop.Native.FfiRect { X = (ushort)value.X, Y = (ushort)value.Y, Width = (ushort)value.Width, Height = (ushort)value.Height };
            Interop.Native.RatatuiTerminalSetViewportArea(_handle.DangerousGetHandle(), r);
        }
    }

    public void Draw(Paragraph paragraph)
    {
        EnsureNotDisposed();
        if (paragraph is null) throw new ArgumentNullException(nameof(paragraph));
        var ok = Interop.Native.RatatuiTerminalDrawParagraph(_handle.DangerousGetHandle(), paragraph.DangerousHandle);
        if (!ok)
            throw new InvalidOperationException("Draw failed");
    }

    public (int Width, int Height) Size()
    {
        EnsureNotDisposed();
        if (!Interop.Native.RatatuiTerminalSize(out var w, out var h))
            throw new InvalidOperationException("Failed to get terminal size");
        return (w, h);
    }

    public void Draw(Paragraph paragraph, Rect rect)
    {
        EnsureNotDisposed();
        if (paragraph is null) throw new ArgumentNullException(nameof(paragraph));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawParagraphIn(_handle.DangerousGetHandle(), paragraph.DangerousHandle, r);
        if (!ok)
            throw new InvalidOperationException("DrawIn failed");
    }

    public void Draw(Paragraph paragraph, Vec2i pos, Vec2i size)
        => Draw(paragraph, Rect.From(pos, size));

    public void Draw(List list, Rect rect)
    {
        EnsureNotDisposed();
        if (list is null) throw new ArgumentNullException(nameof(list));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawListIn(_handle.DangerousGetHandle(), list.DangerousHandle, r);
        if (!ok)
            throw new InvalidOperationException("DrawList failed");
    }

    public void Draw(List list, Vec2i pos, Vec2i size)
        => Draw(list, Rect.From(pos, size));

    public void Draw(Table table, Rect rect)
    {
        EnsureNotDisposed();
        if (table is null) throw new ArgumentNullException(nameof(table));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawTableIn(_handle.DangerousGetHandle(), table.DangerousHandle, r);
        if (!ok)
            throw new InvalidOperationException("DrawTable failed");
    }

    public void Draw(Table table, Vec2i pos, Vec2i size)
        => Draw(table, Rect.From(pos, size));

    public void Draw(Gauge gauge, Rect rect)
    {
        EnsureNotDisposed();
        if (gauge is null) throw new ArgumentNullException(nameof(gauge));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawGaugeIn(_handle.DangerousGetHandle(), gauge.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawGauge failed");
    }

    public void Draw(Gauge gauge, Vec2i pos, Vec2i size)
        => Draw(gauge, Rect.From(pos, size));

    public void Draw(Tabs tabs, Rect rect)
    {
        EnsureNotDisposed();
        if (tabs is null) throw new ArgumentNullException(nameof(tabs));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawTabsIn(_handle.DangerousGetHandle(), tabs.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawTabs failed");
    }

    public void Draw(Tabs tabs, Vec2i pos, Vec2i size)
        => Draw(tabs, Rect.From(pos, size));

    public void Draw(BarChart chart, Rect rect)
    {
        EnsureNotDisposed();
        if (chart is null) throw new ArgumentNullException(nameof(chart));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawBarChartIn(_handle.DangerousGetHandle(), chart.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawBarChart failed");
    }

    public void Draw(BarChart chart, Vec2i pos, Vec2i size)
        => Draw(chart, Rect.From(pos, size));

    public void Draw(Sparkline spark, Rect rect)
    {
        EnsureNotDisposed();
        if (spark is null) throw new ArgumentNullException(nameof(spark));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawSparklineIn(_handle.DangerousGetHandle(), spark.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawSparkline failed");
    }

    public void Draw(Sparkline spark, Vec2i pos, Vec2i size)
        => Draw(spark, Rect.From(pos, size));

    public void Draw(Scrollbar sb, Rect rect)
    {
        EnsureNotDisposed();
        if (sb is null) throw new ArgumentNullException(nameof(sb));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawScrollbarIn(_handle.DangerousGetHandle(), sb.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawScrollbar failed");
    }

    public void Draw(Scrollbar sb, Vec2i pos, Vec2i size)
        => Draw(sb, Rect.From(pos, size));

    public void Draw(Chart chart, Rect rect)
    {
        EnsureNotDisposed();
        if (chart is null) throw new ArgumentNullException(nameof(chart));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawChartIn(_handle.DangerousGetHandle(), chart.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawChart failed");
    }

    public void Draw(Chart chart, Vec2i pos, Vec2i size)
        => Draw(chart, Rect.From(pos, size));

    public void Draw(LineGauge g, Rect rect)
    {
        EnsureNotDisposed();
        if (g is null) throw new ArgumentNullException(nameof(g));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawLineGaugeIn(_handle.DangerousGetHandle(), g.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawLineGauge failed");
    }

    public void Draw(LineGauge g, Vec2i pos, Vec2i size)
        => Draw(g, Rect.From(pos, size));

    public void Draw(Canvas c, Rect rect)
    {
        EnsureNotDisposed();
        if (c is null) throw new ArgumentNullException(nameof(c));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawCanvasIn(_handle.DangerousGetHandle(), c.DangerousHandle, r);
        if (!ok) throw new InvalidOperationException("DrawCanvas failed");
    }

    public void Draw(Canvas c, Vec2i pos, Vec2i size)
        => Draw(c, Rect.From(pos, size));

    public bool NextEvent(TimeSpan timeout, out Event ev)
    {
        EnsureNotDisposed();
        if (Interop.Native.RatatuiNextEvent((ulong)timeout.TotalMilliseconds, out var fe))
        {
            ev = Event.FromFfi(fe);
            return ev.Kind != EventKind.None;
        }
        ev = default;
        return false;
    }

    public void DrawFrame(params DrawCommand[] commands)
    {
        EnsureNotDisposed();
        var ffi = DrawCommand.ToFfi(commands);
        var ok = Interop.Native.RatatuiTerminalDrawFrame(_handle.DangerousGetHandle(), ffi, (UIntPtr)ffi.Length);
        if (!ok) throw new InvalidOperationException("DrawFrame failed");
    }

    public void DrawFrame(ReadOnlySpan<DrawCommand> commands)
    {
        EnsureNotDisposed();
        var ffi = DrawCommand.ToFfi(commands);
        var ok = Interop.Native.RatatuiTerminalDrawFrame(_handle.DangerousGetHandle(), ffi, (UIntPtr)ffi.Length);
        if (!ok) throw new InvalidOperationException("DrawFrame failed");
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Terminal));
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            if (!_cursorShown) Interop.Native.RatatuiTerminalShowCursor(_handle.DangerousGetHandle(), true);
            if (_alt) Interop.Native.RatatuiTerminalLeaveAlt(_handle.DangerousGetHandle());
            if (_raw) Interop.Native.RatatuiTerminalDisableRaw(_handle.DangerousGetHandle());
        }
        finally
        {
            _handle.Dispose();
        }
        _disposed = true;
    }

    public async IAsyncEnumerable<Event> Events(TimeSpan? pollInterval = null, [EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(50);
        while (!cancellationToken.IsCancellationRequested)
        {
            if (NextEvent(interval, out var ev) && ev.Kind != EventKind.None)
            {
                yield return ev;
            }
            else
            {
                await System.Threading.Tasks.Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public System.Threading.Tasks.Task RunAsync(Func<Event, System.Threading.Tasks.Task> handler, TimeSpan? pollInterval = null, System.Threading.CancellationToken cancellationToken = default)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        return System.Threading.Tasks.Task.Run(async () =>
        {
            await foreach (var ev in Events(pollInterval, cancellationToken).ConfigureAwait(false))
            {
                await handler(ev).ConfigureAwait(false);
            }
        }, cancellationToken);
    }
    public void Draw(List list, Rect rect, ListState state)
    {
        EnsureNotDisposed();
        if (list is null) throw new ArgumentNullException(nameof(list));
        if (state is null) throw new ArgumentNullException(nameof(state));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawListStateIn(_handle.DangerousGetHandle(), list.DangerousHandle, r, state.DangerousHandle);
        if (!ok) throw new InvalidOperationException("DrawListState failed");
    }

    public void Draw(Table table, Rect rect, TableState state)
    {
        EnsureNotDisposed();
        if (table is null) throw new ArgumentNullException(nameof(table));
        if (state is null) throw new ArgumentNullException(nameof(state));
        var r = new Interop.Native.FfiRect { X = (ushort)rect.X, Y = (ushort)rect.Y, Width = (ushort)rect.Width, Height = (ushort)rect.Height };
        var ok = Interop.Native.RatatuiTerminalDrawTableStateIn(_handle.DangerousGetHandle(), table.DangerousHandle, r, state.DangerousHandle);
        if (!ok) throw new InvalidOperationException("DrawTableState failed");
    }
}
