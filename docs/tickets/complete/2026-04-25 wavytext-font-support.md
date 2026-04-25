# WavyText Font Support

## Context
**Current behavior**: `BenMakesGames.PlayPlayMini.GraphicsExtensions\WavyText.cs` provides four public `DrawWavyText` overloads (full-screen-centered, y-positioned-x-centered, free-positioned, plus a white-color delegate for each). Three of them (the colored ones) read `font.CharacterWidth`, `font.HorizontalSpacing`, and `font.CharacterHeight` directly off the `Font` returned by `graphics.Fonts[fontName]`. Those properties no longer live on `Font` (which is now a `List<FontSheet> Sheets` wrapper); they only exist on `FontSheet`. The file produces 7 build errors and is the last consumer holding the cascade back from a clean build.

**New behavior**: All `DrawWavyText` overloads measure and step using the multi-sheet `Font` helpers (`ComputeWidth`, `MaxCharacterHeight`, `TryGetSheet`). The per-character wave loop advances `currentX` by each char's own sheet width + spacing, so mixed-sheet text waves correctly without misalignment. A single-sheet fast path keeps the common case allocation-free and dispatch-free. All four public signatures and their XML doc comments are preserved.

## Prerequisites
- `font-multi-sheet-helpers` — adds `Font.TryGetSheet`, `Font.MaxCharacterHeight`, `Font.ComputeWidth`. All used directly here.
- `graphicsmanager-font-text-overloads` — adds `Font`-typed `DrawText` overloads on `GraphicsManager`. Not strictly required for this file (the wave loop dispatches per-char via `FontSheet`), but landing first keeps any incidental `graphics.DrawText(font, …)` paths in the file resolving cleanly.

## Scope
### In scope
- Fix the three colored `DrawWavyText` overloads in `BenMakesGames.PlayPlayMini.GraphicsExtensions\WavyText.cs` to use `Font` helpers instead of stale `FontSheet` properties accessed off `Font`.
- Add a single-sheet fast path to each colored overload (full-screen-centered, y-positioned, free-positioned).
- Multi-sheet per-character wave loop using `TryGetSheet` for x-advance and per-char draw.

### Out of scope
- Modifying the three white-color delegate overloads — they already forward to the colored variants and stay byte-identical.
- Changing the wave math (`Math.Cos(... * 8 + i / 3.0) * 1.95`) — visual behavior must be unchanged for single-sheet fonts.
- Adding `Font`- or `FontSheet`-typed public `DrawWavyText` overloads. The public API stays string-keyed only.
- Backwards-compat shims for the removed `Font.CharacterWidth`/`CharacterHeight`/`HorizontalSpacing` properties.

## Relevant Docs & Anchors
- **Code anchors**:
  - `BenMakesGames.PlayPlayMini.GraphicsExtensions\WavyText.cs` — file under repair; uses C# 14 `extension(GraphicsManager graphics) { ... }` block syntax.
  - `BenMakesGames.PlayPlayMini\Model\Font.cs` — `TryGetSheet`, `MaxCharacterHeight`, `ComputeWidth`, `Sheets` list.
  - `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs` — per-character `DrawText(FontSheet, int, int, char, Color)` overload used by the multi-sheet wave loop; also the `DrawText(Font, …)` overloads' single-sheet-fast-path pattern (`if (font.Sheets.Count == 1) return DrawText(font.Sheets[0], …);`) to mirror.
- **Analogue tickets**:
  - `docs/tickets/complete/2026-04-25 graphicsextensions-font-overloads.md` — same migration shape (Font/FontSheet split in an extension file using the `extension(GraphicsManager graphics)` block); its Learnings section even flags WavyText as the leftover. Mirror its single-sheet-fast-path + multi-sheet `TryGetSheet` loop pattern.

## Constraints & Gotchas
- **Public NuGet API**: every public method needs preserved XML doc comments — the project generates a doc website from them. `// ` comments are insufficient. Existing tags (`<summary>`, `<param>` for each parameter) stay in place; do not reword unless a parameter's role actually changed.
- **Zero allocations on the 60fps draw path**: no LINQ, no closures, no `params`, no array materialization. The wave loop is a plain `for (var i = 0; i < text.Length; i++)`. `font` is hoisted once outside the loop.
- **Match the file's `extension(GraphicsManager graphics) { ... }` block style** — new code stays inside the existing block, not as separate `this GraphicsManager graphics` extension methods.
- **`Font.TryGetSheet` returns `out FontSheet?`**: when `true`, use the null-forgiving `sheet!` to forward to `DrawText(FontSheet, …)`, mirroring the core `GraphicsManager.Text.cs` style.
- **Unmapped char semantics** must match `DrawText(Font, …)`: when `TryGetSheet` returns `false`, do not draw and do not advance `currentX`. Same as the core `Font`-typed text path.
- **Three white-color delegates** (`DrawWavyText(string, GameTime, string)`, `DrawWavyText(string, int, GameTime, string)`, `DrawWavyText(string, int, int, GameTime, string)`) keep their bodies — they only delegate to the colored variants.

## Open Decisions
1. **Per-char dispatch in the wave loop** — call `graphics.DrawText(sheet!, currentX, currentY, text[i], color)` (the existing `FontSheet` per-char overload, sheet already resolved) versus `graphics.DrawText(fontName, currentX, currentY, text[i], color)` (string-keyed, re-resolves the dictionary every char). Default: `FontSheet` per-char overload after `TryGetSheet`. Implementer judgment if `Font` per-char overload reads cleaner — but do not re-resolve `Fonts[fontName]` in the inner loop.
2. **Single-sheet fast path body shape** — keep the original fixed-stride math against `font.Sheets[0]` (`graphics.DrawText(fontName, x + i * (sheet.CharacterWidth + sheet.HorizontalSpacing), y + yOffset, text[i], color)`), or hoist the sheet and call `graphics.DrawText(sheet, …, text[i], color)` to skip the per-char dictionary lookup. Default: hoist the sheet (faster), but either is acceptable as long as the visual output and wave timing are identical to the pre-migration single-sheet behavior.

## Acceptance Criteria
- [ ] `WavyText.cs` compiles cleanly — no remaining references to `font.CharacterWidth`, `font.CharacterHeight`, or `font.HorizontalSpacing` where `font` is a `Font` (the type returned by `graphics.Fonts[fontName]`). The 7 build errors are resolved.
- [ ] `DrawWavyText(string fontName, GameTime, string text, Color)` (full-screen centered) computes `x` from `font.ComputeWidth(text)` and `y` from `font.MaxCharacterHeight` (single-line height, NOT `LineHeight` — no inter-line gap wanted for vertical centering of a one-line wave).
- [ ] `DrawWavyText(string fontName, int y, GameTime, string text, Color)` (y-fixed, x-centered) computes `x` from `font.ComputeWidth(text)`. The `y` parameter passes through unchanged.
- [ ] `DrawWavyText(string fontName, int x, int y, GameTime, string text, Color)` (free-positioned) has a single-sheet fast path: when `font.Sheets.Count == 1` it iterates the string with fixed-stride x-advance against `font.Sheets[0]` and produces visual output identical to the pre-migration body.
- [ ] Same overload's multi-sheet path tracks `currentX` starting at `x`; per char calls `font.TryGetSheet(text[i], out var sheet)` — on hit draws via the `FontSheet` per-char `DrawText` overload at `(currentX, y + yOffset)` and advances `currentX += sheet!.CharacterWidth + sheet.HorizontalSpacing`; on miss does nothing (no draw, no advance).
- [ ] Wave math (`Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8 + i / 3.0) * 1.95` cast to `int`) is byte-identical to the pre-migration formula. The wave index `i` is the loop-counter into `text` (not a per-sheet index), preserving cross-character phase.
- [ ] `font` is hoisted once outside any per-character loop in each colored overload — no `graphics.Fonts[fontName]` dictionary lookup inside the wave loop.
- [ ] All four public `DrawWavyText` overload signatures (the three colored + one of the three white-color delegates is `(string, GameTime, string)`, the other two are `(string, int, GameTime, string)` and `(string, int, int, GameTime, string)`) exist with unchanged signatures and unchanged XML doc comments.
- [ ] No allocations in any colored overload's hot path — no LINQ, no closures, no `params`, no array materialization, no boxing.
- [ ] File still uses the `extension(GraphicsManager graphics) { ... }` block; no new top-level `this GraphicsManager graphics` extension methods introduced.

## Implementation

### 1. Re-read the post-prereq state of `Font.cs` and `GraphicsManager.Text.cs`
Confirm `Font.TryGetSheet`, `Font.MaxCharacterHeight`, `Font.ComputeWidth` exist and have the shapes assumed here, and that `GraphicsManager.DrawText(FontSheet, int, int, char, Color)` (the per-char overload used in the multi-sheet wave loop) is unchanged. Note the single-sheet fast-path idiom in `GraphicsManager.Text.cs` (`if (font.Sheets.Count == 1) return DrawText(font.Sheets[0], …);`) so the new overloads' fast paths read the same way.

### 2. Fix the full-screen-centered colored overload
In `DrawWavyText(string fontName, GameTime gameTime, string text, Color color)`: hoist `var font = graphics.Fonts[fontName]`. Replace `x` calculation with `(graphics.Width - font.ComputeWidth(text)) / 2`. Replace `y` calculation with `(graphics.Height - font.MaxCharacterHeight) / 2`. Body still ends in `graphics.DrawWavyText(fontName, x, y, gameTime, text, color)` (the free-positioned overload handles per-char dispatch). XML docs unchanged.

### 3. Fix the y-fixed-x-centered colored overload
In `DrawWavyText(string fontName, int y, GameTime gameTime, string text, Color color)`: hoist `var font = graphics.Fonts[fontName]`. Replace `x` calculation with `(graphics.Width - font.ComputeWidth(text)) / 2`. Body still ends in `graphics.DrawWavyText(fontName, x, y, gameTime, text, color)`. XML docs unchanged.

### 4. Fix the free-positioned colored overload — the heart of the change
In `DrawWavyText(string fontName, int x, int y, GameTime gameTime, string text, Color color)`:

- Hoist `var font = graphics.Fonts[fontName]` once (it is already there, just ensure no inner loop re-fetches).
- **Single-sheet fast path**: if `font.Sheets.Count == 1`, hoist `var sheet = font.Sheets[0]` and run the existing fixed-stride loop — `for (var i = 0; i < text.Length; i++)` computing `yOffset` exactly as today (`(int)(Math.Cos(gameTime.TotalGameTime.TotalSeconds * 8 + i / 3.0) * 1.95)`), drawing via `graphics.DrawText(sheet, x + i * (sheet.CharacterWidth + sheet.HorizontalSpacing), y + yOffset, text[i], color)`. Return after the loop.
- **Multi-sheet path**: track `var currentX = x;` outside the loop. `for (var i = 0; i < text.Length; i++)`: compute `yOffset` as today; call `font.TryGetSheet(text[i], out var sheet)`; on hit, draw via `graphics.DrawText(sheet!, currentX, y + yOffset, text[i], color)` and advance `currentX += sheet!.CharacterWidth + sheet.HorizontalSpacing`; on miss, skip (no draw, no advance), matching `DrawText(Font, …)` semantics for unmapped chars.

XML doc comment unchanged from the existing `<summary>` + 6 `<param>` tags.

### 5. Confirm the three white-color delegate overloads stay byte-identical
The three `DrawWavyText(...)` overloads without a `Color` parameter just call `graphics.DrawWavyText(..., Color.White)`. They have no `font.CharacterWidth` access — leave them alone. Verify by inspection.

### 6. Build and smoke-test
Run `dotnet build` from the solution root. The 7 errors in `WavyText.cs` should be gone, unblocking the entire solution. No new warnings expected.

## Test Plan
- [ ] `dotnet build` from the solution root — clean, zero new warnings on `WavyText.cs`. All previously cascading build errors elsewhere in the solution that depended on this file should also clear.
- [ ] `dotnet test` — all existing tests pass (no test changes expected; `WavyText` has no direct tests).
- [ ] Manual smoke (single-sheet font): in a sample game, draw `graphics.DrawWavyText("MainFont", gameTime, "Hello!", Color.White)`, the 2-arg fixed-y variant, and the 3-arg fixed-position variant — output is visually identical to pre-migration (same wave amplitude, same phase, same kerning).
- [ ] Manual smoke (single-sheet, full-screen centering): the centered overload places the wave's vertical midline at the screen midline (within 1 px) and centers the string horizontally — confirms `MaxCharacterHeight` and `ComputeWidth` give correct measurements.
- [ ] Manual smoke (multi-sheet font, if available): build a `Font` from two `FontSheet`s with disjoint ranges; draw mixed-range text via the free-positioned overload — characters from each range render at the correct x with no kerning glitches; an unmapped char is skipped (cursor does not advance, nothing drawn).

## Learnings

### Architectural decisions
- **Open Decision 1 (per-char dispatch in multi-sheet wave loop)** resolved to the default: call `graphics.DrawText(sheet!, currentX, y + yOffset, text[i], color)` (the `FontSheet` per-char overload) after `font.TryGetSheet`. The sheet is already in hand from the lookup — re-resolving via `graphics.DrawText(fontName, …)` would re-hit the dictionary every character. Mirrors the core `GraphicsManager.DrawText(Font, …, string, …)` body in `GraphicsManager.Text.cs`.
- **Open Decision 2 (single-sheet fast-path body shape)** resolved to the "hoist the sheet" variant: `var sheet = font.Sheets[0];` outside the loop, then `graphics.DrawText(sheet, x + i * (sheet.CharacterWidth + sheet.HorizontalSpacing), y + yOffset, text[i], color)` per char. Same fixed-stride math as the pre-migration body, but skips the per-char `Fonts[fontName]` dictionary lookup. Visual output and wave timing are bitwise identical to the old single-sheet behavior.
- **Centered overloads delegate to free-positioned, not duplicating the wave loop**: they only compute `x`/`y` and forward to `graphics.DrawWavyText(fontName, x, y, gameTime, text, color)`. So both centered overloads do go through the dictionary one extra time on the inner call, but only once per draw call (not per char) — acceptable, and keeps the wave loop in one place.

### Problems encountered
- None. The migration was mechanical once the prereq helpers (`TryGetSheet`, `MaxCharacterHeight`, `ComputeWidth`) were in place. The build error count for `WavyText.cs` dropped from 7 to 0 with no cascade fixes needed elsewhere in `GraphicsExtensions`.

### Interesting tidbits
- `MaxCharacterHeight` (NOT `LineHeight`) is the right vertical-centering height for a single-line wave: `LineHeight` includes the inter-line gap (`VerticalSpacing`), which would offset the centered text downward by that gap on multi-sheet fonts.
- `Font.TryGetSheet` returns `out FontSheet?` — even on `true`, the out-param is typed nullable, so `sheet!` null-forgiving is needed when forwarding. Same idiom as the core `GraphicsManager.Text.cs` body.
- `font.Sheets.Count == 1` fast-path return is a meaningful optimization here because the wave loop hits this every frame for animated text — the dictionary-lookup elision is per-character, not per-call.

### Workarounds / limitations
- None required. The single-sheet fast path is an optimization, not a workaround — multi-sheet fonts work correctly through the slower path.

### Related areas affected
- `BenMakesGames.PlayPlayMini.GraphicsExtensions` project now builds clean (0 errors). The remaining 17 build errors in the solution are all in `BenMakesGames.PlayPlayMini.VN` (tracked by `vn-font-support.md`) and are independent of this fix.

### Rejected alternatives
- **Re-resolve the sheet via `graphics.DrawText(fontName, …)` in the multi-sheet wave loop**: would re-hit the `Fonts` dictionary on every character. Rejected per Open Decision 1 default.
- **Drop the single-sheet fast path and route everything through `TryGetSheet`**: simpler code, but adds a per-character branch + nullable out-param dance to the hot path that 99% of callers (single-sheet fonts) don't need. Rejected — the fast path is one tiny `if` and pays for itself.
- **Use `LineHeight` for vertical centering in the screen-centered overload**: would add the inter-line gap to the height, biasing the centered single-line wave downward by `VerticalSpacing` pixels on multi-sheet fonts. Rejected — `MaxCharacterHeight` is the correct measurement for single-line vertical centering, and the ticket spelled this out as an Acceptance Criterion.
