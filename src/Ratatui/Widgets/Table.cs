using System;
using System.Linq;

namespace Ratatui;

public sealed class Table : IDisposable
{
    private readonly TableHandle _handle;
    private bool _disposed;
    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Table()
    {
        var ptr = Interop.Native.RatatuiTableNew();
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create Table");
        _handle = TableHandle.FromRaw(ptr);
    }

    public Table Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public Table Headers(params string[] cells)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", cells ?? Array.Empty<string>());
        Interop.Native.RatatuiTableSetHeaders(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    // Headers defined as styled spans (one header cell as a single line of spans)
    public Table Headers(ReadOnlySpan<Batching.SpanRun> runs)
    {
        EnsureNotDisposed();
        if (runs.IsEmpty) return this;
        Batching.WithFfiSpans(runs, (spans, len) =>
        {
            Interop.Native.RatatuiTableSetHeadersSpans(_handle.DangerousGetHandle(), spans, len);
        });
        return this;
    }

    public Table AppendRow(params string[] cells)
    {
        EnsureNotDisposed();
        var tsv = string.Join("\t", cells ?? Array.Empty<string>());
        Interop.Native.RatatuiTableAppendRow(_handle.DangerousGetHandle(), tsv);
        return this;
    }

    // Append a row as a single line of styled spans (one cell).
    public Table AppendRow(ReadOnlySpan<Batching.SpanRun> runs)
    {
        EnsureNotDisposed();
        if (runs.IsEmpty) return this;
        Batching.WithFfiSpans(runs, (spans, len) =>
        {
            Interop.Native.RatatuiTableAppendRowSpans(_handle.DangerousGetHandle(), spans, len);
        });
        return this;
    }

    // Append multiple rows with multi-line cells using nested spans batching.
    public Table AppendRows(ReadOnlySpan<Batching.Row> rows)
    {
        EnsureNotDisposed();
        if (rows.IsEmpty) return this;
        Batching.WithFfiRowsCellsLines(rows, (pRows, len) =>
        {
            Interop.Native.RatatuiTableAppendRowsCellsLines(_handle.DangerousGetHandle(), pRows, len);
        });
        return this;
    }

    public Table ColumnPercents(params ushort[] percents)
    {
        EnsureNotDisposed();
        var vals = percents ?? Array.Empty<ushort>();
        if (vals.Length == 0) return this;
        Interop.Native.RatatuiTableSetWidthsPercentages(_handle.DangerousGetHandle(), vals, (UIntPtr)vals.Length);
        return this;
    }

    public Table Selected(int index)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetSelected(_handle.DangerousGetHandle(), index);
        return this;
    }

    public Table RowHighlightStyle(Style style)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetRowHighlightStyle(_handle.DangerousGetHandle(), style.ToFfi());
        return this;
    }

    public Table HighlightSymbol(string? symbol)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetHighlightSymbol(_handle.DangerousGetHandle(), symbol);
        return this;
    }

    // Width helpers
    public Table WidthsPercentages(ReadOnlySpan<ushort> widths)
    {
        EnsureNotDisposed();
        if (widths.IsEmpty) return this;
        var temp = new ushort[widths.Length];
        widths.CopyTo(temp);
        Interop.Native.RatatuiTableSetWidthsPercentages(_handle.DangerousGetHandle(), temp, (UIntPtr)temp.Length);
        return this;
    }

    public enum TableWidthKind : uint { Length = 0, Percentage = 1, Min = 2 }

    public Table Widths(ReadOnlySpan<(TableWidthKind kind, ushort value)> specs)
    {
        EnsureNotDisposed();
        if (specs.IsEmpty) return this;
        var kinds = new uint[specs.Length];
        var vals = new ushort[specs.Length];
        for (int i = 0; i < specs.Length; i++) { kinds[i] = (uint)specs[i].kind; vals[i] = specs[i].value; }
        Interop.Native.RatatuiTableSetWidths(_handle.DangerousGetHandle(), kinds, vals, (UIntPtr)specs.Length);
        return this;
    }

    public Table ColumnSpacing(ushort spacing)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetColumnSpacing(_handle.DangerousGetHandle(), spacing);
        return this;
    }

    public Table RowHeight(ushort height)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetRowHeight(_handle.DangerousGetHandle(), height);
        return this;
    }

    public Table HeaderStyle(Style s)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetHeaderStyle(_handle.DangerousGetHandle(), s.ToFfi());
        return this;
    }

    public Table ColumnHighlightStyle(Style s)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetColumnHighlightStyle(_handle.DangerousGetHandle(), s.ToFfi());
        return this;
    }

    public Table CellHighlightStyle(Style s)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetCellHighlightStyle(_handle.DangerousGetHandle(), s.ToFfi());
        return this;
    }

    public Table HighlightSpacing(uint spacing)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiTableSetHighlightSpacing(_handle.DangerousGetHandle(), spacing);
        return this;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Table));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}
