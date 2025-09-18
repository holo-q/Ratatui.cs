using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Ratatui;

/// <summary>
/// Thin wrapper that forwards to <see cref="Terminal"/> while disposing widget handles
/// immediately after they are drawn. Keeps the hot path in <see cref="Terminal"/> branch-free
/// but lets call sites inline "new Paragraph(...)" without leaking native memory.
/// </summary>
public sealed class AutoDisposingTerminal : IDisposable
{
    private readonly Terminal _inner;
    private readonly bool _ownsTerminal;
    private readonly Stack<List<IDisposable>> _pendingFrames = new();
    private bool _disposed;
    private int _frameDepth;

    public AutoDisposingTerminal()
        : this(new Terminal(), ownsTerminal: true)
    {
    }

    public AutoDisposingTerminal(Terminal inner, bool ownsTerminal = false)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _ownsTerminal = ownsTerminal;
    }

    public Terminal Inner => _inner;

    public AutoDisposingTerminal Clear()
    {
        EnsureNotDisposed();
        _inner.Clear();
        return this;
    }

    public AutoDisposingTerminal Raw(bool on = true)
    {
        EnsureNotDisposed();
        _inner.Raw(on);
        return this;
    }

    public AutoDisposingTerminal AltScreen(bool on = true)
    {
        EnsureNotDisposed();
        _inner.AltScreen(on);
        return this;
    }

    public AutoDisposingTerminal ShowCursor(bool show = true)
    {
        EnsureNotDisposed();
        _inner.ShowCursor(show);
        return this;
    }

    public (ushort X, ushort Y) GetCursor()
    {
        EnsureNotDisposed();
        return _inner.GetCursor();
    }

    public AutoDisposingTerminal SetCursor(ushort x, ushort y)
    {
        EnsureNotDisposed();
        _inner.SetCursor(x, y);
        return this;
    }

    public Rect Viewport
    {
        get
        {
            EnsureNotDisposed();
            return _inner.Viewport;
        }
        set
        {
            EnsureNotDisposed();
            _inner.Viewport = value;
        }
    }

    public (int w, int h) Size()
    {
        EnsureNotDisposed();
        return _inner.Size();
    }

    public void Draw(Paragraph paragraph)
    {
        EnsureNotDisposed();
        if (paragraph is null) throw new ArgumentNullException(nameof(paragraph));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(paragraph);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(paragraph, framePending && succeeded);
        }
    }

    public void Draw(Paragraph paragraph, Rect rect)
    {
        EnsureNotDisposed();
        if (paragraph is null) throw new ArgumentNullException(nameof(paragraph));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(paragraph, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(paragraph, framePending && succeeded);
        }
    }

    public void Draw(Paragraph paragraph, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (paragraph is null) throw new ArgumentNullException(nameof(paragraph));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(paragraph, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(paragraph, framePending && succeeded);
        }
    }

    public void Draw(List list, Rect rect)
    {
        EnsureNotDisposed();
        if (list is null) throw new ArgumentNullException(nameof(list));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(list, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(list, framePending && succeeded);
        }
    }

    public void Draw(List list, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (list is null) throw new ArgumentNullException(nameof(list));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(list, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(list, framePending && succeeded);
        }
    }

    public void Draw(List list, Rect rect, ListState state)
    {
        EnsureNotDisposed();
        if (list is null) throw new ArgumentNullException(nameof(list));
        if (state is null) throw new ArgumentNullException(nameof(state));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(list, rect, state);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(list, framePending && succeeded);
        }
    }

    public void Draw(Table table, Rect rect)
    {
        EnsureNotDisposed();
        if (table is null) throw new ArgumentNullException(nameof(table));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(table, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(table, framePending && succeeded);
        }
    }

    public void Draw(Table table, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (table is null) throw new ArgumentNullException(nameof(table));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(table, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(table, framePending && succeeded);
        }
    }

    public void Draw(Table table, Rect rect, TableState state)
    {
        EnsureNotDisposed();
        if (table is null) throw new ArgumentNullException(nameof(table));
        if (state is null) throw new ArgumentNullException(nameof(state));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(table, rect, state);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(table, framePending && succeeded);
        }
    }

    public void Draw(Gauge gauge, Rect rect)
    {
        EnsureNotDisposed();
        if (gauge is null) throw new ArgumentNullException(nameof(gauge));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(gauge, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(gauge, framePending && succeeded);
        }
    }

    public void Draw(Gauge gauge, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (gauge is null) throw new ArgumentNullException(nameof(gauge));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(gauge, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(gauge, framePending && succeeded);
        }
    }

    public void Draw(Tabs tabs, Rect rect)
    {
        EnsureNotDisposed();
        if (tabs is null) throw new ArgumentNullException(nameof(tabs));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(tabs, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(tabs, framePending && succeeded);
        }
    }

    public void Draw(Tabs tabs, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (tabs is null) throw new ArgumentNullException(nameof(tabs));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(tabs, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(tabs, framePending && succeeded);
        }
    }

    public void Draw(BarChart chart, Rect rect)
    {
        EnsureNotDisposed();
        if (chart is null) throw new ArgumentNullException(nameof(chart));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(chart, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(chart, framePending && succeeded);
        }
    }

    public void Draw(BarChart chart, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (chart is null) throw new ArgumentNullException(nameof(chart));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(chart, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(chart, framePending && succeeded);
        }
    }

    public void Draw(Sparkline spark, Rect rect)
    {
        EnsureNotDisposed();
        if (spark is null) throw new ArgumentNullException(nameof(spark));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(spark, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(spark, framePending && succeeded);
        }
    }

    public void Draw(Sparkline spark, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (spark is null) throw new ArgumentNullException(nameof(spark));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(spark, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(spark, framePending && succeeded);
        }
    }

    public void Draw(Scrollbar sb, Rect rect)
    {
        EnsureNotDisposed();
        if (sb is null) throw new ArgumentNullException(nameof(sb));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(sb, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(sb, framePending && succeeded);
        }
    }

    public void Draw(Scrollbar sb, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (sb is null) throw new ArgumentNullException(nameof(sb));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(sb, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(sb, framePending && succeeded);
        }
    }

    public void Draw(Chart chart, Rect rect)
    {
        EnsureNotDisposed();
        if (chart is null) throw new ArgumentNullException(nameof(chart));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(chart, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(chart, framePending && succeeded);
        }
    }

    public void Draw(Chart chart, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (chart is null) throw new ArgumentNullException(nameof(chart));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(chart, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(chart, framePending && succeeded);
        }
    }

    public void Draw(LineGauge gauge, Rect rect)
    {
        EnsureNotDisposed();
        if (gauge is null) throw new ArgumentNullException(nameof(gauge));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(gauge, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(gauge, framePending && succeeded);
        }
    }

    public void Draw(LineGauge gauge, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (gauge is null) throw new ArgumentNullException(nameof(gauge));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(gauge, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(gauge, framePending && succeeded);
        }
    }

    public void Draw(Canvas canvas, Rect rect)
    {
        EnsureNotDisposed();
        if (canvas is null) throw new ArgumentNullException(nameof(canvas));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(canvas, rect);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(canvas, framePending && succeeded);
        }
    }

    public void Draw(Canvas canvas, Vec2i pos, Vec2i size)
    {
        EnsureNotDisposed();
        if (canvas is null) throw new ArgumentNullException(nameof(canvas));
        var framePending = _frameDepth > 0;
        var succeeded = false;
        try
        {
            _inner.Draw(canvas, pos, size);
            succeeded = true;
        }
        finally
        {
            DisposeAfterDraw(canvas, framePending && succeeded);
        }
    }

    public bool NextEvent(TimeSpan timeout, out Event ev)
    {
        EnsureNotDisposed();
        return _inner.NextEvent(timeout, out ev);
    }

    public void DrawFrame(params DrawCommand[] commands)
    {
        EnsureNotDisposed();
        _inner.DrawFrame(commands);
    }

    public void DrawFrame(ReadOnlySpan<DrawCommand> commands)
    {
        EnsureNotDisposed();
        _inner.DrawFrame(commands);
    }

    public IDisposable BeginFrame()
    {
        PushFrame();
        return new FrameScope(this);
    }

    public void PushFrame()
    {
        EnsureNotDisposed();
        _inner.PushFrame();
        _frameDepth++;
        _pendingFrames.Push(new List<IDisposable>());
    }

    public void PopFrame()
    {
        EnsureNotDisposed();
        if (_frameDepth == 0)
            throw new InvalidOperationException("PopFrame called with no frame in progress");

        var disposables = _pendingFrames.Pop();
        _frameDepth--;
        try
        {
            _inner.PopFrame();
        }
        finally
        {
            if (_frameDepth == 0)
            {
                DisposeAll(disposables);
            }
            else if (disposables.Count > 0)
            {
                _pendingFrames.Peek().AddRange(disposables);
            }
        }
    }

    public IAsyncEnumerable<Event> Events(TimeSpan? pollInterval = null, System.Threading.CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return _inner.Events(pollInterval, cancellationToken);
    }

    public System.Threading.Tasks.Task RunAsync(Func<Event, System.Threading.Tasks.Task> handler, TimeSpan? pollInterval = null, System.Threading.CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return _inner.RunAsync(handler, pollInterval, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            while (_frameDepth > 0 && _pendingFrames.Count > 0)
            {
                _frameDepth--;
                var frame = _pendingFrames.Pop();
                DisposeAll(frame);
            }
            _frameDepth = 0;
            _pendingFrames.Clear();
        }
        finally
        {
            if (_ownsTerminal)
            {
                _inner.Dispose();
            }
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(AutoDisposingTerminal));
    }

    private void DisposeAfterDraw(IDisposable widget, bool defer)
    {
        if (widget is null) return;
        if (defer && _frameDepth > 0 && _pendingFrames.Count > 0)
        {
            _pendingFrames.Peek().Add(widget);
            return;
        }
        widget.Dispose();
    }

    private static void DisposeAll(List<IDisposable> disposables)
    {
        if (disposables.Count == 0) return;
        List<Exception>? errors = null;
        foreach (var d in disposables)
        {
            try
            {
                d.Dispose();
            }
            catch (Exception ex)
            {
                errors ??= new List<Exception>();
                errors.Add(ex);
            }
        }

        if (errors is not null)
        {
            if (errors.Count == 1)
            {
                ExceptionDispatchInfo.Capture(errors[0]).Throw();
            }
            throw new AggregateException(errors);
        }
    }

    private sealed class FrameScope : IDisposable
    {
        private AutoDisposingTerminal? _owner;

        public FrameScope(AutoDisposingTerminal owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            var owner = _owner;
            if (owner is null) return;
            _owner = null;
            owner.PopFrame();
        }
    }
}
