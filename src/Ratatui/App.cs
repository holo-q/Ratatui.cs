using System;

namespace Ratatui;

/// <summary>
/// Minimal application runner that manages Terminal RAII (raw, alt screen, cursor) and dispatches events.
/// Performance-neutral: does not hide or intercept draw calls.
/// </summary>
public static class App
{
    /// <summary>
    /// Runs an event loop with RAII terminal setup. The handler returns false to exit.
    /// </summary>
    public static void Run(Func<Event, bool> handler, TimeSpan? pollInterval = null)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        using var term = new Terminal().Raw(true).AltScreen(true).ShowCursor(false);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(50);
        while (true)
        {
            if (term.NextEvent(interval, out var ev))
            {
                if (!handler(ev)) break;
            }
        }
    }
}

