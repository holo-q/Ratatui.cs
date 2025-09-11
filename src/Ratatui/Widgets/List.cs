using System;

namespace Ratatui;

public sealed class List : IDisposable
{
    private readonly ListHandle _handle;
    private bool _disposed;

    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public List()
    {
        var ptr = Interop.Native.RatatuiListNew();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create List");
        _handle = ListHandle.FromRaw(ptr);
    }

    public List Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public List AppendItem(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListAppendItem(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
        return this;
    }

    // Zero-alloc overload from UTF-8 bytes (e.g. "text"u8)
    public unsafe List AppendItem(ReadOnlySpan<byte> utf8, Style? style = null)
    {
        EnsureNotDisposed();
        var sty = style ?? default;
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var ffiSpans = stackalloc Interop.Native.FfiSpan[1];
        ffiSpans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = sty.ToFfi() };
        Interop.Native.RatatuiListAppendItemSpans(_handle.DangerousGetHandle(), (IntPtr)ffiSpans, (UIntPtr)1);
        return this;
    }

    // Append a single list item composed of styled spans (UTF-8), allocation-light.
    public List AppendItem(ReadOnlySpan<Batching.SpanRun> runs)
    {
        EnsureNotDisposed();
        if (runs.IsEmpty) return this;
        Batching.WithFfiSpans(runs, (spans, len) =>
        {
            Interop.Native.RatatuiListAppendItemSpans(_handle.DangerousGetHandle(), spans, len);
        });
        return this;
    }

    public List Selected(int index)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListSetSelected(_handle.DangerousGetHandle(), index);
        return this;
    }

    public List HighlightStyle(Style style)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListSetHighlightStyle(_handle.DangerousGetHandle(), style.ToFfi());
        return this;
    }

    public List HighlightSymbol(string? symbol)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiListSetHighlightSymbol(_handle.DangerousGetHandle(), symbol);
        return this;
    }

    // Append multiple list items where each item is a line of styled spans.
    public List AppendItems(ReadOnlySpan<ReadOnlyMemory<Batching.SpanRun>> items)
    {
        EnsureNotDisposed();
        if (items.IsEmpty) return this;
        Batching.WithFfiLineSpans(items, (lines, len) =>
        {
            Interop.Native.RatatuiListAppendItemsSpans(_handle.DangerousGetHandle(), lines, len);
        });
        return this;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(List));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}
