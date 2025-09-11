using System;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>
    /// Runs an async event loop with RAII terminal setup. The handler returns false to exit.
    /// </summary>
    public static async Task RunAsync(Func<Event, Task<bool>> handler, TimeSpan? pollInterval = null, CancellationToken cancellationToken = default)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        using var term = new Terminal().Raw(true).AltScreen(true).ShowCursor(false);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(50);
        await foreach (var ev in term.Events(interval, cancellationToken).ConfigureAwait(false))
        {
            if (!await handler(ev).ConfigureAwait(false)) break;
        }
    }

    /// <summary>
    /// Runs an async event loop until cancellation is requested. The handler processes each event.
    /// </summary>
    public static async Task RunAsync(Func<Event, Task> handler, TimeSpan? pollInterval = null, CancellationToken cancellationToken = default)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        using var term = new Terminal().Raw(true).AltScreen(true).ShowCursor(false);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(50);
        await foreach (var ev in term.Events(interval, cancellationToken).ConfigureAwait(false))
        {
            await handler(ev).ConfigureAwait(false);
        }
    }
}
