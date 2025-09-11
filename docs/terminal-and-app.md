# Terminal RAII and App Runners

Terminal
- RAII toggles: `Raw(true)`, `AltScreen(true)`, `ShowCursor(false)`; safe unwind in `Dispose()` (cursor → leave alt → disable raw).
- Cursor helpers: `GetCursor()`, `SetCursor(x,y)`; `Viewport { get; set; }`.
- Batched draw: `DrawFrame(ReadOnlySpan<DrawCommand>)` and widget `Draw(widget, rect)` overloads.
- Async events: `await foreach (var ev in term.Events(interval, ct)) { ... }`

App
- `App.Run(Func<Event,bool> handler, TimeSpan? poll = null)`
- `App.RunAsync(Func<Event,Task<bool>> handler, TimeSpan? poll = null, CancellationToken ct = default)`
- `App.RunAsync(Func<Event,Task> handler, TimeSpan? poll = null, CancellationToken ct = default)`

Notes
- The App runners are minimal and performance‑neutral; they don’t hide draw calls or keep state.
- Prefer composing your own loop if you need custom scheduling, timers, or IO.

Related
- Headless testing: render frames/widgets offscreen — see headless-testing.md
- Widgets and batching: table/list/tabs/chart helpers — see widgets.md and batching-and-spans.md

