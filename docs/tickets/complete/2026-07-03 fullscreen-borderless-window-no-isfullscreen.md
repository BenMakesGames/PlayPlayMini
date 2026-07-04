# Fullscreen path: borderless-window sized to display, no IsFullScreen

## Context

**Current behavior**: `GraphicsManager` treats "fullscreen" as `Graphics.IsFullScreen = true` with `Graphics.HardwareModeSwitch = false`. On Windows this routes through SDL2's `SDL_WINDOW_FULLSCREEN_DESKTOP`, producing a borderless window sized to the display. Windows detects "borderless window covering entire display" and applies Fullscreen Optimizations (FSO) / Multi-Plane Overlay (MPO) — trying to hand the app exclusive-like scanout. On some GPU/driver combos (older or driver-dependent negotiation, HDR, G-Sync, external overlays), that DWM/MPO handshake stutters for many seconds: repeated black flashes, whole-system input lag, before the driver settles on a scanout path. Reproduced on a friend's PC (~15–20 s of black flicker + slow mouse after picking the max-zoom radio, which auto-flips `IsFullScreen = true` via `SetZoom`).

**New behavior**: Neither `SetZoom` nor `SetFullscreen` sets `Graphics.IsFullScreen = true` on Windows any more. The "fullscreen" path becomes: `Game.Window.IsBorderless = true`, `Game.Window.Position = (0, 0)`, back buffer sized to `Adapter.CurrentDisplayMode` width/height, `Graphics.IsFullScreen = false`, `ApplyChanges()`. Result on-screen is visually identical (a chromeless window filling the whole display) but Windows classifies it as a plain borderless window rather than a fullscreen candidate — FSO/MPO negotiation is skipped, and the transition is a normal window-style change. Leaving the fullscreen path restores `Window.IsBorderless = false` and the previous windowed size before `ApplyChanges`. The public `FullScreen` property retains its current semantic ("the window fills the display").

## Scope

### In scope

- `BenMakesGames.PlayPlayMini\Services\GraphicsManager.cs` — `Initialize`, `SetZoom`, `SetFullscreen`. Rework the three call sites that touch `Graphics.IsFullScreen`. Restore-to-windowed path when leaving fullscreen.
- Public `FullScreen` property semantics unchanged; internal implementation swaps to borderless.

### Out of scope

- Exclusive fullscreen (`HardwareModeSwitch = true`) — deliberately not offered. Alt+Tab / overlay / DPI-scale-switch friction, worse for streamers, not worth the marginal snappiness.
- User-facing exclusive-vs-borderless toggle. YAGNI; borderless is the right default for every current consumer.
- Multi-monitor picker (which display to fill). Current behavior picks `GraphicsDevice.Adapter.CurrentDisplayMode`, i.e. primary. Keep as-is; add later if a consumer needs it.
- `IsFixedTimeStep` / vsync default changes. Considered as adjacent fixes for the same symptom and rejected — they address tearing / frame-rate cap, not FSO/MPO black-flicker.
- Per-exe Windows compatibility shim (registry / manifest to disable FSO). Out of PlayPlayMini's remit; ship the fix in code.
- Astromino-side changes. This ticket lives entirely in PlayPlayMini; Astromino picks up the fix on the next framework bump.

## Relevant Docs & Anchors

- **Primary file**: `BenMakesGames.PlayPlayMini\Services\GraphicsManager.cs` — `Initialize`, `SetZoom`, `SetFullscreen`, and the `FullScreen` property. All three methods currently set `Graphics.IsFullScreen = FullScreen` and call `ApplyChanges()`.
- **Auto-flip site**: in `SetZoom`, the line that flips `FullScreen = Zoom * Width == Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width && Zoom * Height == Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;` — this is how consumers reach fullscreen without ever calling `SetFullscreen`, and it's the path the Astromino Settings max-zoom radio hits.
- **Window access**: `GraphicsManager` already holds a `Game Game` field (via `SetGame`). `Game.Window.IsBorderless`, `Game.Window.Position`, `Game.Window.AllowUserResizing` are the SDL2-backed knobs to use.
- **SDL2 backing note**: MonoGame's DesktopGL Windows port uses SDL2. `Window.IsBorderless = true` maps to `SDL_SetWindowBordered(window, SDL_FALSE)`; combined with an explicit backbuffer at display size and `IsFullScreen = false`, Windows does not classify the app as fullscreen for FSO purposes.

## Constraints & Gotchas

- **`Graphics.IsFullScreen` must be `false` in every path this ticket touches.** The whole point of the change is to stop Windows classifying the window as fullscreen. Setting `IsFullScreen = true` anywhere (even briefly, then setting `false` again) re-triggers FSO negotiation on the intermediate `ApplyChanges`.
- **`FullScreen` property (public) stays.** Public API contract: `FullScreen == true` means "the window covers the whole display", regardless of underlying mechanism. Consumers reading this property must keep working. Only the *implementation* changes.
- **Restore path matters.** Leaving fullscreen (e.g., `SetFullscreen(false)`, or `SetZoom` to a smaller zoom that no longer matches the display) must set `Window.IsBorderless = false` and reposition the window somewhere sane (default: don't reposition; SDL2 keeps the window at its last position, but a `Window.Position` at (0,0) from the fullscreen path may leave the restored windowed view flush to the top-left corner of the display — worth confirming during manual testing).
- **Property set ordering matters on SDL2.** `Window.IsBorderless` and `Window.Position` are settable on the `Window` directly and take effect immediately (they don't wait for `ApplyChanges`). `Graphics.PreferredBackBuffer*` need `ApplyChanges()` to apply. Set the `Window.*` knobs *before* `ApplyChanges` to avoid an intermediate frame with borders + display-size backbuffer.
- **`HardwareModeSwitch = false`** stays set in `Initialize` — it's a belt-and-suspenders belt in the new world (since we never flip `IsFullScreen = true`) but removing it now would risk a future consumer flipping `IsFullScreen` back on and getting exclusive fullscreen unexpectedly.
- **Cross-platform**: SDL2's borderless-window semantics apply on macOS and Linux too. The FSO/MPO problem is Windows-specific, but the fix (borderless + display-size backbuffer) produces the same visual result on every platform, so no per-OS branching is needed.
- **`SynchronizeWithVerticalRetrace = false`** currently re-assigned in `SetZoom` and `SetFullscreen`. Unrelated to this fix; do not touch here. (If a separate ticket wants to reconsider the vsync default, it lives on its own.)

## Open Decisions

1. **Restore-window position on leaving fullscreen** — leave `Window.Position` at whatever the fullscreen path set it to (probably `(0, 0)`), vs. recenter on the display, vs. remember pre-fullscreen position and restore it. Default: leave it. Recentering is one line if it feels wrong under manual testing; remembering-and-restoring is more state than warranted for a rare transition.
2. **`Window.AllowUserResizing` under borderless-fullscreen** — irrelevant while borderless (no grab handles), but confirm it isn't accidentally toggled. Default: don't touch it.
3. **Whether `FullScreen` flip on the auto-flip path in `SetZoom` should be gated behind a config knob** — right now, `SetZoom(maxZoom)` auto-enters fullscreen if `Zoom * Width == display.Width && Zoom * Height == display.Height`. Surprising UX (see [[project_astromino_concept]] consumer commentary), but this ticket is scoped to *how* fullscreen is entered, not *when*. Default: preserve the existing auto-flip trigger. Split off if it becomes a real complaint.

## Acceptance Criteria

- [ ] No code path in `GraphicsManager` sets `Graphics.IsFullScreen = true`. Grep the file: zero hits.
- [ ] The fullscreen path (`SetFullscreen(true)`, or `SetZoom` hitting the auto-flip trigger) results in: `Window.IsBorderless == true`, back buffer size == `Adapter.CurrentDisplayMode` width × height, window positioned at `(0, 0)` of the primary display, `Graphics.IsFullScreen == false`.
- [ ] The windowed path (`SetFullscreen(false)`, or `SetZoom` to a value not matching display) results in: `Window.IsBorderless == false`, back buffer size == `Zoom * Width` × `Zoom * Height`, `Graphics.IsFullScreen == false`.
- [ ] Public `FullScreen` property still reflects "window covers the whole display": `true` after entering the fullscreen path, `false` after leaving it. Callers reading this property see no behavior change.
- [ ] `Initialize` no longer relies on `Graphics.IsFullScreen = FullScreen` for the initial windowed state (which is `FullScreen == false` at startup and thus already a no-op for the fullscreen path). It still calls `ApplyChanges()`.
- [ ] Public method signatures of `SetZoom`, `SetFullscreen`, `MaxZoom` are unchanged.
- [ ] `dotnet build` is clean against the existing warning baseline; `dotnet test` passes.

## Implementation

### 1. Read the three call sites

Open `GraphicsManager.cs`. Read `Initialize`, `SetZoom`, `SetFullscreen`, and the `FullScreen` property. Note that all three methods share the same three lines: set backbuffer W/H, set `IsFullScreen`, call `ApplyChanges`. The rework replaces the middle line with a `Window.IsBorderless` toggle on the fullscreen branch.

### 2. Add a private helper for the graphics-apply block

Extract the shared "set backbuffer, toggle borderless, apply" work into a private helper — something like `ApplyWindowMode(int desiredWidth, int desiredHeight, bool coverDisplay)`. Body: set `Window.IsBorderless = coverDisplay`, set `Window.Position = coverDisplay ? Point.Zero : Window.Position` (don't clobber user-moved window position when going back to windowed), set `Graphics.PreferredBackBufferWidth/Height` to `desiredWidth`/`desiredHeight`, set `Graphics.IsFullScreen = false` (always), call `Graphics.ApplyChanges()`. Order matters — see Constraints.

### 3. Rework `Initialize`

Replace the existing `Graphics.IsFullScreen = FullScreen; Graphics.ApplyChanges();` pair with a call to the new helper: `ApplyWindowMode(Width * Zoom, Height * Zoom, coverDisplay: FullScreen)`. At startup `FullScreen` is `false`, so this is a windowed apply — but wire it through the same helper so there's one path.

### 4. Rework `SetZoom`

Keep the existing zoom-clamp and auto-flip trigger (`FullScreen = Zoom * Width == display.W && Zoom * Height == display.H`). Replace the trailing three-line `Graphics.*` block with a call to `ApplyWindowMode(Zoom * Width, Zoom * Height, coverDisplay: FullScreen)`. Drop the `Graphics.SynchronizeWithVerticalRetrace = false` re-assignment only if it feels dead — otherwise leave it (out of scope, and a redundant same-value write is harmless).

### 5. Rework `SetFullscreen`

Keep the existing branch that computes `desiredWidth`, `desiredHeight`, and (for the fullscreen branch) recomputes `Zoom`. Replace the trailing three-line `Graphics.*` block with a call to `ApplyWindowMode(desiredWidth, desiredHeight, coverDisplay: FullScreen)`.

### 6. Confirm no residual `Graphics.IsFullScreen = true` write

Grep the file for `IsFullScreen`. Only the assignments in the helper (`= false`) should remain. If any `= true` snuck through, remove it.

### 7. Manual verification on Ben's dev box

Launch Astromino (or a PlayPlayMini sample). Enter Settings → Window Size → max zoom. Confirm the window becomes chromeless and covers the display without a black-flicker episode. Alt+Tab away and back — confirm the window restores without needing to be re-selected. Drop back to a smaller zoom — confirm borders return and the window sizes down cleanly. Move the window before entering fullscreen, exit fullscreen, confirm restored position is reasonable (see Open Decision 1).

## Test Plan

- [ ] `dotnet build BenMakesGames.PlayPlayMini\BenMakesGames.PlayPlayMini.csproj` clean against baseline warnings.
- [ ] `dotnet test` passes.
- [ ] Grep `GraphicsManager.cs` for `IsFullScreen` — every hit is either `= false` (in the helper) or a read of the public `FullScreen` property. No `= true` assignments to `Graphics.IsFullScreen`.
- [ ] Launch Astromino, Settings → Window Size → max zoom → confirm smooth transition to chromeless full-display window, no black flicker.
- [ ] From the max-zoom state, drop back to a smaller zoom → confirm borders return, window sizes down.
- [ ] Alt+Tab out and back while in the max-zoom state → confirm the window remains on-top-restorable, no DWM stall.
- [ ] Multi-monitor spot-check if hardware available: on a machine with two displays, confirm the fullscreen fills the primary display and doesn't stretch across both.
- [ ] Regression: from Astromino's Startup loading screen (before Settings is reachable), confirm windowed launch is unchanged — same window size, same borders as before.
- [ ] Verify with the friend's PC (the reporter): repeat the max-zoom repro → confirm the 15–20 s black-flicker episode does not occur.

## Learnings

### Architectural decisions

- **Single `ApplyWindowMode(desiredWidth, desiredHeight, coverDisplay)` helper.** All three call sites (`Initialize`, `SetZoom`, `SetFullscreen`) previously duplicated the same three-line `PreferredBackBuffer* / IsFullScreen / ApplyChanges` block. Extracting the block collapses the "windowed vs fullscreen mechanism" decision into one place, so `Graphics.IsFullScreen = false` is guaranteed everywhere without every caller needing to remember.
- **`Graphics.SynchronizeWithVerticalRetrace = false` left where it was** in `SetZoom`/`SetFullscreen` (redundant re-assignment of the value already set in `Initialize`). Out of scope for this ticket; harmless.
- **Open Decision 1 (restore-window position on leaving fullscreen)** — initially resolved as ticket's default (don't touch position on leave). Manual testing immediately showed the bug the ticket flagged: after fullscreen set position to `(0,0)`, dropping back to a smaller zoom left `Window.Position == (0,0)`, but `Window.Position` on SDL2 refers to the client area — so the title bar rendered above the top of the display and the window was un-draggable. Tried tracking a `_wasCoveringDisplay` field to recenter only on the covering→windowed transition, but that added state to guard against edge cases (future "start fullscreen" configs, monitor-count changes leaving remembered positions off-screen) that the "not clever" approach avoids. Final resolution: helper always centers the window on the windowed path (`GraphicsAdapter.DefaultAdapter.CurrentDisplayMode`, works pre-first-`ApplyChanges` too), and always parks it at `Point.Zero` on the cover path. Trade-off accepted: `SetZoom` calls that stay windowed will re-center over any user-dragged position — rare in practice, and preferable to remembering-state hazards.
- **Open Decision 2 (`AllowUserResizing`)** — untouched, per default.
- **Open Decision 3 (auto-flip trigger in `SetZoom`)** — preserved, per default. This ticket is about *how* fullscreen is entered, not *when*.

### Interesting tidbits

- `Game.Window.IsBorderless` and `Game.Window.Position` on MonoGame DesktopGL/SDL2 apply immediately when set — no `ApplyChanges()` needed. `Graphics.PreferredBackBuffer*` do need `ApplyChanges()`. Setting the `Window.*` knobs before `ApplyChanges` avoids a one-frame flash where borders + display-size backbuffer would coexist.
- `HardwareModeSwitch = false` is retained even though it's now dead-code belt-and-suspenders (we never flip `IsFullScreen = true`). Removing it would let a future consumer flip `IsFullScreen` back on and accidentally get exclusive fullscreen. Cheap insurance.

### Verification gaps

Manual/hardware test-plan items were not exercised by me — they require launching Astromino on a display, and one (the reporter's PC repro) requires a specific machine. Build clean, `dotnet test` (20 passed), and grep verification of `IsFullScreen` all pass. Ben will need to run the manual Astromino checks (max-zoom transition, alt+tab, drop-back-to-smaller, primary-display fill on multi-monitor, windowed launch regression) and re-run the friend's PC repro before shipping.

### Rejected alternatives

- **Storing pre-fullscreen `Window.Position` for restore.** Not worth the extra state field for a rare transition; SDL2 already remembers a reasonable position most of the time.
- **Cross-platform branching (`OperatingSystem.IsWindows()`).** Not needed — borderless-window + display-size backbuffer produces the same visual result on macOS/Linux, and there's no FSO/MPO analogue to work around.
