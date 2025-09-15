# Terminal RAII and App Runners

Terminal
- RAII toggles: `Raw(true)`, `AltScreen(true)`, `ShowCursor(false)`; safe unwind in `Dispose()` (cursor → leave alt → disable raw).
- Cursor helpers: `GetCursor()`, `SetCursor(x,y)`; `Viewport { get; set; }`.
- Batched draw: `PushFrame()`/`PopFrame()` to buffer `Draw(...)` calls, or `DrawFrame(ReadOnlySpan<DrawCommand>)` with explicit `DrawCommand`s.
- Async events: `await foreach (var ev in term.Events(interval, ct)) { ... }`

App
- `App.Run(Func<Event,bool> handler, TimeSpan? poll = null)`
- `App.RunAsync(Func<Event,Task<bool>> handler, TimeSpan? poll = null, CancellationToken ct = default)`
- `App.RunAsync(Func<Event,Task> handler, TimeSpan? poll = null, CancellationToken ct = default)`

Notes
- The App runners are minimal and performance‑neutral; they don’t hide draw calls or keep state.
- Prefer composing your own loop if you need custom scheduling, timers, or IO.
- Frame buffering: `PushFrame()`/`PopFrame()` supports up to 32 nested levels. Each `PopFrame()` flushes only the commands recorded since its matching `PushFrame()`.

Examples
- Using PushFrame/PopFrame
  ```csharp
  using var term = new Terminal();
  var (w,h) = term.Size();
  using var p = new Paragraph("Hello").Title("Demo");
  using var g = new Gauge().Ratio(0.42f).Title("Load");
  term.PushFrame();
  term.Draw(p, new Rect(0,0,w/2,h));
  term.Draw(g, new Rect(w/2,0,w/2,3));
  term.PopFrame();
  ```

Related
- Headless testing: render frames/widgets offscreen — see headless-testing.md
- Widgets and batching: table/list/tabs/chart helpers — see widgets.md and batching-and-spans.md
