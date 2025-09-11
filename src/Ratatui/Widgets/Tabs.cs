using System;

namespace Ratatui;

public sealed class Tabs : IDisposable
{
    private readonly TabsHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Tabs()
    {
        var ptr = Interop.Native.RatatuiTabsNew();
        if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Tabs");
        _handle = TabsHandle.FromRaw(ptr);
    }

    public Tabs Titles(params string[] titles)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", titles ?? Array.Empty<string>());
        Interop.Native.RatatuiTabsSetTitles(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    // Set a single title (tab) using styled UTF-8 spans.
    public Tabs AddTitleSpans(ReadOnlySpan<Batching.SpanRun> runs)
    {
        EnsureNotDisposed();
        if (runs.IsEmpty) return this;
        Batching.WithFfiSpans(runs, (spans, len) =>
        {
            Interop.Native.RatatuiTabsAddTitleSpans(_handle.DangerousGetHandle(), spans, len);
        });
        return this;
    }

    // Replace all tab titles using lines of styled spans (one line per tab title).
    public Tabs SetTitlesSpans(ReadOnlySpan<ReadOnlyMemory<Batching.SpanRun>> titles)
    {
        EnsureNotDisposed();
        if (titles.IsEmpty) { Interop.Native.RatatuiTabsClearTitles(_handle.DangerousGetHandle()); return this; }
        Batching.WithFfiLineSpans(titles, (lines, len) =>
        {
            Interop.Native.RatatuiTabsSetTitlesSpans(_handle.DangerousGetHandle(), lines, len);
        });
        return this;
    }

    public Tabs Selected(int index)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetSelected(_handle.DangerousGetHandle(), (ushort)Math.Max(0, index));
        return this;
    }

    public Tabs Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    // UTF-8 title path can use spans: use AddTitleSpans/SetTitlesSpans.

    public Tabs Divider(string divider)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetDivider(_handle.DangerousGetHandle(), divider);
        return this;
    }

    // Zero-alloc divider from UTF-8 bytes
    public unsafe Tabs Divider(ReadOnlySpan<byte> utf8)
    {
        EnsureNotDisposed();
        if (utf8.IsEmpty) { Interop.Native.RatatuiTabsSetDivider(_handle.DangerousGetHandle(), string.Empty); return this; }
        var buf = stackalloc byte[utf8.Length + 1];
        utf8.CopyTo(new Span<byte>(buf, utf8.Length));
        buf[utf8.Length] = 0;
        var spans = stackalloc Interop.Native.FfiSpan[1];
        spans[0] = new Interop.Native.FfiSpan { TextUtf8 = (IntPtr)buf, Style = default };
        Interop.Native.RatatuiTabsSetDividerSpans(_handle.DangerousGetHandle(), (IntPtr)spans, (UIntPtr)1);
        return this;
    }

    public Tabs Styles(Style unselected, Style selected, Style divider)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetStyles(_handle.DangerousGetHandle(), unselected.ToFfi(), selected.ToFfi(), divider.ToFfi());
        return this;
    }

    public Tabs TitleAlignment(Alignment align)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTabsSetBlockTitleAlignment(_handle.DangerousGetHandle(), (uint)align);
        return this;
    }

    private void EnsureNotDisposed() { if (_disposed) throw new ObjectDisposedException(nameof(Tabs)); }
    public void Dispose() { if (_disposed) return; _handle.Dispose(); _disposed = true; }
}
