using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Ratatui;

/// <summary>
/// Span-first batching helpers for marshaling styled UTF-8 spans into FFI structures
/// with minimal allocations (stackalloc for small payloads, ArrayPool for larger).
/// </summary>
public static class Batching
{
    public const int StackThresholdBytes = 2048;
    public const int StackThresholdRuns = 128;
    public const int StackThresholdLines = 128;
    public const int StackThresholdCells = 128;
    public const int StackThresholdRows = 256;

    public readonly struct SpanRun
    {
        public ReadOnlyMemory<byte> Text { get; }
        public Style Style { get; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanRun(ReadOnlyMemory<byte> text, Style style)
        { Text = text; Style = style; }
    }

    public readonly struct Line
    {
        public ReadOnlyMemory<SpanRun> Runs { get; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Line(ReadOnlyMemory<SpanRun> runs) { Runs = runs; }
    }

    public readonly struct Cell
    {
        public ReadOnlyMemory<Line> Lines { get; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cell(ReadOnlyMemory<Line> lines) { Lines = lines; }
    }

    public readonly struct Row
    {
        public ReadOnlyMemory<Cell> Cells { get; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Row(ReadOnlyMemory<Cell> cells) { Cells = cells; }
    }

    /// <summary>
    /// Packs a sequence of (utf8, style) runs into a single contiguous buffer with NUL terminators
    /// and calls the provided action with a pointer to an array of FfiSpan entries.
    /// The lifetime of the pointers is limited to the duration of <paramref name="invoke"/>.
    /// </summary>
    public static unsafe void WithFfiSpans(ReadOnlySpan<SpanRun> runs, Action<IntPtr, UIntPtr> invoke)
    {
        if (runs.Length == 0) { invoke(IntPtr.Zero, UIntPtr.Zero); return; }

        // Compute total size with NUL terminators per run.
        int total = 0;
        for (int i = 0; i < runs.Length; i++) total += runs[i].Text.Length + 1;

        byte[]? rented = null;
        Span<byte> buffer = total <= StackThresholdBytes ? stackalloc byte[total] : (rented = ArrayPool<byte>.Shared.Rent(total));
        try
        {
            int offset = 0;
            Span<int> offsets = runs.Length <= StackThresholdRuns ? stackalloc int[runs.Length] : new int[runs.Length];
            for (int i = 0; i < runs.Length; i++)
            {
                var src = runs[i].Text.Span;
                if (!src.IsEmpty)
                {
                    src.CopyTo(buffer.Slice(offset));
                    offset += src.Length;
                }
                buffer[offset++] = 0; // NUL
                offsets[i] = offset; // store end position (start is previous end minus length)
            }

            // Build FfiSpan array in place via stackalloc
            Span<Interop.Native.FfiSpan> spans = runs.Length <= StackThresholdRuns ? stackalloc Interop.Native.FfiSpan[runs.Length] : new Interop.Native.FfiSpan[runs.Length];

            fixed (byte* bufPtr = buffer)
            {
                int cursor = 0;
                for (int i = 0; i < runs.Length; i++)
                {
                    int end = offsets[i];
                    int start = cursor;
                    spans[i] = new Interop.Native.FfiSpan
                    {
                        TextUtf8 = (IntPtr)(bufPtr + start),
                        Style = runs[i].Style.ToFfi(),
                    };
                    cursor = end;
                }

                fixed (Interop.Native.FfiSpan* pSpans = spans)
                {
                    invoke((IntPtr)pSpans, (UIntPtr)runs.Length);
                }
            }
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Builds FfiLineSpans for multiple lines where each line is a sequence of (utf8, style) runs.
    /// Calls the provided action with a pointer to an array of FfiLineSpans.
    /// </summary>
    public static unsafe void WithFfiLineSpans(ReadOnlySpan<ReadOnlyMemory<SpanRun>> lines, Action<IntPtr, UIntPtr> invoke)
    {
        if (lines.Length == 0) { invoke(IntPtr.Zero, UIntPtr.Zero); return; }

        // Count total runs and total bytes (with NUL terminators)
        int totalRuns = 0;
        int totalBytes = 0;
        var runsPerLine = lines.Length <= StackThresholdLines ? stackalloc int[lines.Length] : new int[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Span;
            runsPerLine[i] = line.Length;
            totalRuns += line.Length;
            for (int j = 0; j < line.Length; j++) totalBytes += line[j].Text.Length + 1;
        }

        byte[]? rented = null;
        Span<byte> buffer = totalBytes <= StackThresholdBytes ? stackalloc byte[totalBytes] : (rented = ArrayPool<byte>.Shared.Rent(totalBytes));
        var spansArr = totalRuns <= StackThresholdRuns ? stackalloc Interop.Native.FfiSpan[totalRuns] : new Interop.Native.FfiSpan[totalRuns];
        var lineSpansArr = lines.Length <= StackThresholdLines ? stackalloc Interop.Native.FfiLineSpans[lines.Length] : new Interop.Native.FfiLineSpans[lines.Length];
        try
        {
            int bufOff = 0;
            int spanOff = 0;
            fixed (byte* pBuf = buffer)
            fixed (Interop.Native.FfiSpan* pSpans = spansArr)
            fixed (Interop.Native.FfiLineSpans* pLineSpans = lineSpansArr)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Span;
                    if (line.Length == 0)
                    {
                        pLineSpans[i] = new Interop.Native.FfiLineSpans { Spans = IntPtr.Zero, Len = UIntPtr.Zero };
                        continue;
                    }
                    int startSpanIndex = spanOff;
                    for (int j = 0; j < line.Length; j++)
                    {
                        var src = line[j].Text.Span;
                        if (!src.IsEmpty)
                        {
                            src.CopyTo(buffer.Slice(bufOff));
                            bufOff += src.Length;
                        }
                        buffer[bufOff++] = 0;
                        pSpans[spanOff++] = new Interop.Native.FfiSpan
                        {
                            TextUtf8 = (IntPtr)(pBuf + (bufOff - (src.Length + 1))),
                            Style = line[j].Style.ToFfi(),
                        };
                    }
                    pLineSpans[i] = new Interop.Native.FfiLineSpans
                    {
                        Spans = (IntPtr)(pSpans + startSpanIndex),
                        Len = (UIntPtr)(spanOff - startSpanIndex)
                    };
                }

                invoke((IntPtr)pLineSpans, (UIntPtr)lines.Length);
            }
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Builds FfiRowCellsLines for multiple table rows (cells -> lines -> spans).
    /// Efficient: a single contiguous UTF-8 buffer and flat arrays pinned for the call.
    /// </summary>
    public static unsafe void WithFfiRowsCellsLines(ReadOnlySpan<Row> rows, Action<IntPtr, UIntPtr> invoke)
    {
        if (rows.Length == 0) { invoke(IntPtr.Zero, UIntPtr.Zero); return; }

        int totalRuns = 0, totalLines = 0, totalCells = 0, totalBytes = 0;
        for (int r = 0; r < rows.Length; r++)
        {
            var cells = rows[r].Cells.Span;
            totalCells += cells.Length;
            for (int c = 0; c < cells.Length; c++)
            {
                var lines = cells[c].Lines.Span;
                totalLines += lines.Length;
                for (int l = 0; l < lines.Length; l++)
                {
                    var runs = lines[l].Runs.Span;
                    totalRuns += runs.Length;
                    for (int i = 0; i < runs.Length; i++) totalBytes += runs[i].Text.Length + 1;
                }
            }
        }

        byte[]? rented = null;
        Span<byte> buffer = totalBytes <= StackThresholdBytes ? stackalloc byte[totalBytes] : (rented = ArrayPool<byte>.Shared.Rent(totalBytes));
        var spansArr = totalRuns <= StackThresholdRuns ? stackalloc Interop.Native.FfiSpan[totalRuns] : new Interop.Native.FfiSpan[totalRuns];
        var lineSpansArr = totalLines <= StackThresholdLines ? stackalloc Interop.Native.FfiLineSpans[totalLines] : new Interop.Native.FfiLineSpans[totalLines];
        var cellLinesArr = totalCells <= StackThresholdCells ? stackalloc Interop.Native.FfiCellLines[totalCells] : new Interop.Native.FfiCellLines[totalCells];
        var rowsArr = rows.Length <= StackThresholdRows ? stackalloc Interop.Native.FfiRowCellsLines[rows.Length] : new Interop.Native.FfiRowCellsLines[rows.Length];

        try
        {
            int bufOff = 0, spanOff = 0, lineOff = 0, cellOff = 0;
            fixed (byte* pBuf = buffer)
            fixed (Interop.Native.FfiSpan* pSpans = spansArr)
            fixed (Interop.Native.FfiLineSpans* pLineSpans = lineSpansArr)
            fixed (Interop.Native.FfiCellLines* pCellLines = cellLinesArr)
            fixed (Interop.Native.FfiRowCellsLines* pRows = rowsArr)
            {
                for (int r = 0; r < rows.Length; r++)
                {
                    var cells = rows[r].Cells.Span;
                    int cellStart = cellOff;
                    for (int c = 0; c < cells.Length; c++)
                    {
                        var lines = cells[c].Lines.Span;
                        int lineStart = lineOff;
                        for (int l = 0; l < lines.Length; l++)
                        {
                            var runs = lines[l].Runs.Span;
                            int spanStart = spanOff;
                            for (int i = 0; i < runs.Length; i++)
                            {
                                var src = runs[i].Text.Span;
                                if (!src.IsEmpty)
                                {
                                    src.CopyTo(buffer.Slice(bufOff));
                                    bufOff += src.Length;
                                }
                                buffer[bufOff++] = 0;
                                pSpans[spanOff++] = new Interop.Native.FfiSpan
                                {
                                    TextUtf8 = (IntPtr)(pBuf + (bufOff - (src.Length + 1))),
                                    Style = runs[i].Style.ToFfi(),
                                };
                            }
                            pLineSpans[lineOff++] = new Interop.Native.FfiLineSpans
                            {
                                Spans = runs.Length == 0 ? IntPtr.Zero : (IntPtr)(pSpans + spanStart),
                                Len = (UIntPtr)(spanOff - spanStart)
                            };
                        }
                        pCellLines[cellOff++] = new Interop.Native.FfiCellLines
                        {
                            Lines = lines.Length == 0 ? IntPtr.Zero : (IntPtr)(pLineSpans + lineStart),
                            Len = (UIntPtr)(lineOff - lineStart)
                        };
                    }
                    pRows[r] = new Interop.Native.FfiRowCellsLines
                    {
                        Cells = cells.Length == 0 ? IntPtr.Zero : (IntPtr)(pCellLines + cellStart),
                        Len = (UIntPtr)(cellOff - cellStart)
                    };
                }

                invoke((IntPtr)pRows, (UIntPtr)rows.Length);
            }
        }
        finally
        {
            if (rented is not null) ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
