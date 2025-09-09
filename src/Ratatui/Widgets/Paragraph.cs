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

    public Paragraph Title(string? title, bool border = true)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphSetBlockTitle(_handle.DangerousGetHandle(), title, border);
        return this;
    }

    public Paragraph AppendLine(string text, Style? style = null)
    {
        EnsureNotDisposed();
        Interop.Native.RatatuiParagraphAppendLine(_handle.DangerousGetHandle(), text, (style ?? default).ToFfi());
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
