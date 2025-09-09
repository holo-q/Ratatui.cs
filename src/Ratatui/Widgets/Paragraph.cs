using System;

namespace Ratatui;

public sealed class Paragraph : IDisposable
{
    private readonly ParagraphHandle _handle;
    private bool _disposed;

    internal IntPtr DangerousHandle => _handle.DangerousGetHandle();

    public Paragraph(string text)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        var ptr = Interop.Native.RatatuiParagraphNew(text);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create Paragraph");
        _handle = ParagraphHandle.FromRaw(ptr);
    }

    public unsafe Paragraph(ReadOnlySpan<byte> utf8Text)
    {
        fixed (byte* p = utf8Text)
        {
            var ptr = Interop.Native.RatatuiParagraphNewBytes((IntPtr)p, (UIntPtr)utf8Text.Length);
            if (ptr == IntPtr.Zero) throw new InvalidOperationException("Failed to create Paragraph");
            _handle = ParagraphHandle.FromRaw(ptr);
        }
    }

    public Paragraph Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public unsafe Paragraph Title(ReadOnlySpan<byte> titleUtf8, bool border = true)
    {
        EnsureNotDisposed();
        fixed (byte* p = titleUtf8)
        {
            Interop.Native.RatatuiParagraphSetBlockTitleBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)titleUtf8.Length, border);
        }
        return this;
    }

    public Paragraph AppendLine(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphAppendLine(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
        return this;
    }

    public unsafe Paragraph AppendLine(ReadOnlySpan<byte> utf8, Style? style = null)
    {
        EnsureNotDisposed();
        fixed (byte* p = utf8)
        {
            Interop.Native.RatatuiParagraphAppendLineBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)utf8.Length, (style ?? default).ToFfi());
        }
        return this;
    }

    public Paragraph AppendSpan(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphAppendSpan(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
        return this;
    }

    public unsafe Paragraph AppendSpan(ReadOnlySpan<byte> utf8, Style? style = null)
    {
        EnsureNotDisposed();
        fixed (byte* p = utf8)
        {
            Interop.Native.RatatuiParagraphAppendSpanBytes(_handle.DangerousGetHandle(), (IntPtr)p, (UIntPtr)utf8.Length, (style ?? default).ToFfi());
        }
        return this;
    }

    public Paragraph NewLine()
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphNewLine(_handle.DangerousGetHandle());
        return this;
    }

    public Paragraph Align(Alignment alignment)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetAlignment(_handle.DangerousGetHandle(), (uint)alignment);
        return this;
    }

    public Paragraph Wrap(bool trim = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetWrap(_handle.DangerousGetHandle(), trim);
        return this;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Paragraph));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _handle.Dispose();
        _disposed = true;
    }
}

public enum Alignment : uint
{
    Left = 0,
    Center = 1,
    Right = 2,
}
