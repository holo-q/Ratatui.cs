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

    public ref struct SpanRun
    {
        public ReadOnlySpan<byte> Text;
        public Style Style;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanRun(ReadOnlySpan<byte> text, Style style)
        {
            Text = text;
            Style = style;
        }
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
            Span<int> offsets = runs.Length <= 128 ? stackalloc int[runs.Length] : new int[runs.Length];
            for (int i = 0; i < runs.Length; i++)
            {
                var src = runs[i].Text;
                if (!src.IsEmpty)
                {
                    src.CopyTo(buffer.Slice(offset));
                    offset += src.Length;
                }
                buffer[offset++] = 0; // NUL
                offsets[i] = offset; // store end position (start is previous end minus length)
            }

            // Build FfiSpan array in place via stackalloc
            Span<Interop.Native.FfiSpan> spans = runs.Length <= 128 ? stackalloc Interop.Native.FfiSpan[runs.Length] : new Interop.Native.FfiSpan[runs.Length];

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
}

