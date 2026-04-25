# GraphicsExtensions Font Overloads

## Context
**Current behavior**: `BenMakesGames.PlayPlayMini.GraphicsExtensions\DrawTextWithSpans.cs` and `TextWithOutline.cs` provide span/array text-drawing extensions on `GraphicsManager` keyed by `FontSheet` (single-sheet) or by `string` font-name. The string-name span overloads (`DrawText(string fontName, …, ReadOnlySpan<char>, …)`, `DrawText(string fontName, …, Span<char>, …)`, `DrawTextWithOutline(string fontName, …, ReadOnlySpan<char>, …)`) call `graphics.Fonts[fontName]` — which now returns the new multi-sheet `Font` record, not `FontSheet` — so they currently rely on a `FontSheet` overload that no longer matches the argument type. The latent type mismatch is unblocked once the core `GraphicsManager.Text.cs` ticket lands `Font` overloads, and this ticket then adds the corresponding span/outline `Font` overloads to keep extension parity.

**New behavior**: Each existing `FontSheet`-based span/array/outline extension has a sibling `Font` overload with identical signature shape. The string-name overloads naturally resolve through the new `Font` overloads (since `Fonts[fontName]` returns `Font`). All existing `FontSheet` overloads remain unchanged for back-compat. Multi-sheet `Font` arguments dispatch per-character to the correct sheet via `Font.TryGetSheet`; single-sheet `Font` arguments take a fast path that delegates to the existing `FontSheet` body.

## Prerequisites
- `font-multi-sheet-helpers` — adds `Font.TryGetSheet(char, out FontSheet)`, `Font.LineHeight`, `Font.ComputeHeight(...)` helpers used by the multi-sheet path.
- `graphicsmanager-font-text-overloads` — adds `Font` variants of the core `GraphicsManager.DrawText(...)` methods (string, single-char, span). New extensions in this ticket forward to those core overloads.

## Scope
### In scope
- New `Font` overloads in `BenMakesGames.PlayPlayMini.GraphicsExtensions\DrawTextWithSpans.cs` for `ReadOnlySpan<char>` and `Span<char>` text.
- New `Font` overloads in `BenMakesGames.PlayPlayMini.GraphicsExtensions\TextWithOutline.cs` for `string` and `ReadOnlySpan<char>` text.
- Verifying the existing `Font` + `char[]` overload in `DrawTextWithSpans.cs` routes through a `Font` span overload (not a `FontSheet` one).
- Confirming the latent `Fonts[fontName]` → `Font` argument in the string-name span/outline overloads now resolves cleanly to the new `Font` overloads.

### Out of scope
- Word-wrapping `Font` overloads (`DrawTextWithWordWrap`, `ComputeDimensionsWithWordWrap`) — covered by other tickets.
- Modifying or removing any existing `FontSheet` overload.
- Changes to the core `GraphicsManager` partials beyond what the prerequisite ticket delivers.

## Relevant Docs & Anchors
- **Code anchors**:
  - `BenMakesGames.PlayPlayMini.GraphicsExtensions\DrawTextWithSpans.cs` — pattern to mirror, including the `extension(GraphicsManager graphics) { ... }` C# extension-block syntax used throughout the file.
  - `BenMakesGames.PlayPlayMini.GraphicsExtensions\TextWithOutline.cs` — the 4-offset + 1-fill outline pattern.
  - `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs` — the `FontSheet` per-character `DrawText(FontSheet, int, int, char, Color)` body to draw individual chars in the multi-sheet loop, plus the new `Font` overloads added by the prereq ticket that the new extensions will forward to (for `TextWithOutline`).
  - `BenMakesGames.PlayPlayMini\Model\Font.cs` — `Sheets` list, `TryGetSheet`, `LineHeight` (added by prereq).
  - `BenMakesGames.PlayPlayMini\Model\FontSheet.cs` — `CharacterHeight`, `VerticalSpacing` already used in newline reset logic.

## Constraints & Gotchas
- **Public NuGet API**: every new public overload needs XML doc comments (the project generates a docs site from them; `// ` comments are not sufficient).
- **Zero-allocation contract**: span overloads must not introduce LINQ, closures, params arrays, or array materialization. Multi-sheet path uses a `for` loop over the span and a stack-local `(int x, int y)` tuple, mirroring the existing `FontSheet` body.
- **Match neighboring style**: existing extensions use the C# 14 `extension(GraphicsManager graphics) { ... }` block. New overloads go inside the same block, not as separate `this GraphicsManager graphics` extension methods.
- **`[MethodImpl(MethodImplOptions.AggressiveInlining)]`**: existing `DrawText(string fontName, …)` thin pass-throughs in `DrawTextWithSpans.cs` do *not* use it; existing thin pass-throughs in `GraphicsManager.Text.cs` (e.g. `DrawText(string fontName, …, string, …)`) *do*. Match each file's local convention — the extensions file currently omits it, so omit it on the new thin pass-throughs there unless the file's pattern changes.
- **Single-sheet fast path**: when `font.Sheets.Count == 1`, delegate directly to the existing `DrawText(FontSheet, …)` overload with `font.Sheets[0]` — avoids the per-char `TryGetSheet` overhead for the common case and exactly matches the old single-sheet behavior.

## Open Decisions
1. **Multi-sheet newline advance** — should newline advance by `font.LineHeight` (the prereq's helper, presumably max sheet height + spacing) or by the current line's tallest sheet? Default: `font.LineHeight`, matching the core `GraphicsManager.Text.cs` `Font` overloads added by the prereq ticket. Re-check after prereq lands; mirror its choice.
2. **Per-char draw inside multi-sheet loop** — call `graphics.DrawText(sheet, position.x, position.y, c, color)` (existing core `FontSheet` per-char overload) or call `graphics.DrawText(font, position.x, position.y, c, color)` (new core `Font` per-char overload from prereq). Default: the `FontSheet` per-char overload after `TryGetSheet`, since we already have the sheet in hand and avoid re-resolving — same pattern the prereq's core `Font` span overload should use.

## Acceptance Criteria
- [ ] `DrawTextWithSpans.cs` contains a public `DrawText(Font font, int x, int y, ReadOnlySpan<char> text, Color color)` overload returning `(int x, int y)`, with XML doc comments matching the neighboring `FontSheet` overload's style (`<summary>`, `<param>` for each parameter, `<returns>`).
- [ ] `DrawTextWithSpans.cs` contains a public `DrawText(Font font, int x, int y, Span<char> text, Color color)` overload returning `(int x, int y)`, with XML doc comments.
- [ ] Both new `DrawText(Font, …)` span overloads have a single-sheet fast path: `if (font.Sheets.Count == 1) return graphics.DrawText(font.Sheets[0], x, y, text, color);` before any multi-sheet work.
- [ ] Both new `DrawText(Font, …)` span overloads' multi-sheet path is a `for` loop over the span: on `\r`/`\n` reset `position` to `(x, position.y + font.LineHeight)`; otherwise resolve the sheet via `font.TryGetSheet(c, out var sheet)` and draw via `graphics.DrawText(sheet, position.x, position.y, c, color)`. No allocations, no LINQ.
- [ ] The existing `DrawText(Font font, int x, int y, char[] text, Color color)` overload routes to a `Font` span overload (i.e. its body resolves to `graphics.DrawText(font, x, y, text.AsSpan(), color)` and the call binds to the new `DrawText(Font, …, ReadOnlySpan<char>, …)`, not to `DrawText(FontSheet, …)`).
- [ ] `TextWithOutline.cs` contains a public `DrawTextWithOutline(Font font, int x, int y, string text, Color fillColor, Color outlineColor)` overload with XML doc comments mirroring the existing `string fontName` variant's tag set.
- [ ] `TextWithOutline.cs` contains a public `DrawTextWithOutline(Font font, int x, int y, ReadOnlySpan<char> text, Color fillColor, Color outlineColor)` overload with XML doc comments mirroring the existing `FontSheet` variant.
- [ ] Both new `DrawTextWithOutline(Font, …)` overloads use the same 5-call pattern as the existing `FontSheet` outline body (4 offsets `+1`/`-1` in each axis with `outlineColor`, then center with `fillColor`), forwarding to `graphics.DrawText(font, …)` (the core `Font` overload added by the prereq).
- [ ] Existing `DrawTextWithOutline(string fontName, …, ReadOnlySpan<char>, …)` body still reads `graphics.DrawTextWithOutline(graphics.Fonts[fontName], x, y, text, fillColor, outlineColor)` and resolves to the new `Font` overload (no `Font` → `FontSheet` argument-type mismatch).
- [ ] All existing `FontSheet` overloads in both files are byte-identical to pre-ticket state (signatures, bodies, doc comments).
- [ ] Solution builds clean (`dotnet build`) with zero new warnings on the two changed files; the latent compile error from the string-name overloads passing a `Font` to a `FontSheet`-typed parameter is resolved.

## Implementation

### 1. Read the post-prereq state of `GraphicsManager.Text.cs`
Before editing the extension files, open `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs` and confirm the prereq ticket has landed `DrawText(Font, …, ReadOnlySpan<char>, …)`, `DrawText(Font, …, string, …)`, and `DrawText(Font, …, char, …)`. Note the multi-sheet loop pattern it uses (newline advance, per-char dispatch) so the new extension overloads' multi-sheet bodies stay consistent with the core.

### 2. Add `Font` span overloads to `DrawTextWithSpans.cs`
Inside the existing `extension(GraphicsManager graphics) { ... }` block, add two new public methods alongside the existing `DrawText(FontSheet, …, ReadOnlySpan<char>, …)` and `DrawText(FontSheet, …, Span<char>, …)`:

- `(int x, int y) DrawText(Font font, int x, int y, ReadOnlySpan<char> text, Color color)`
- `(int x, int y) DrawText(Font font, int x, int y, Span<char> text, Color color)`

Body for each: single-sheet fast path delegating to `graphics.DrawText(font.Sheets[0], x, y, text, color)`; otherwise a `for (var i = 0; i < text.Length; i++)` loop maintaining a `(int x, int y) position` tuple — on `'\r' or '\n'` reset `position = (x, position.y + font.LineHeight)`; otherwise `if (font.TryGetSheet(text[i], out var sheet)) position = graphics.DrawText(sheet, position.x, position.y, text[i], color);`. Return `position` at end. Mirror the existing `FontSheet` body's variable naming and structure.

XML doc comments: copy the neighboring `FontSheet` overload's `<summary>` and parameter shape; rename the `<param name="fontSheet">` slot to `<param name="font">`. The two `Span<char>` and `ReadOnlySpan<char>` overloads can share doc text (they already do for the `FontSheet` versions).

### 3. Verify `Font` + `char[]` overload routes through the `Font` span overload
The existing `public (int x, int y) DrawText(Font font, int x, int y, char[] text, Color color) => graphics.DrawText(font, x, y, text.AsSpan(), color);` is already in place. After step 2, confirm `text.AsSpan()` (which yields `Span<char>`) binds to the new `DrawText(Font, …, Span<char>, …)` overload — *not* to a `FontSheet` overload via implicit conversion (there is none). No code change expected; this is a verification step. If overload resolution accidentally prefers the `ReadOnlySpan<char>` overload, that's still correct behavior — both Font span overloads have identical semantics.

### 4. Add `Font` overloads to `TextWithOutline.cs`
Inside the existing `extension(GraphicsManager graphics) { ... }` block, add two new public methods. Place each next to its `string fontName` / `FontSheet` sibling for readability:

- `void DrawTextWithOutline(Font font, int x, int y, string text, Color fillColor, Color outlineColor)`
- `void DrawTextWithOutline(Font font, int x, int y, ReadOnlySpan<char> text, Color fillColor, Color outlineColor)`

Each body: 4 outline draws at `(x, y+1)`, `(x, y-1)`, `(x-1, y)`, `(x+1, y)` with `outlineColor`, then 1 fill draw at `(x, y)` with `fillColor` — all forwarded to `graphics.DrawText(font, …, text, …)`. Identical structure to the existing `DrawTextWithOutline(FontSheet, …, ReadOnlySpan<char>, …)` body, just with `Font` instead of `FontSheet`. The `string text` variant forwards to the core `DrawText(Font, …, string, …)` overload added by the prereq; the `ReadOnlySpan<char>` variant forwards to the new `DrawText(Font, …, ReadOnlySpan<char>, …)` overload from step 2.

XML doc comments: mirror the existing outline overloads — `<summary>` describing outline draw, `<param>` for each parameter (`font`, `x`, `y`, `text`, `fillColor`, `outlineColor`).

### 5. Confirm string-name outline routing
The existing `DrawTextWithOutline(string fontName, int x, int y, ReadOnlySpan<char> text, …)` body reads `graphics.DrawTextWithOutline(graphics.Fonts[fontName], x, y, text, fillColor, outlineColor)`. After step 4, this resolves to the new `DrawTextWithOutline(Font, …, ReadOnlySpan<char>, …)` overload (since `Fonts[fontName]` is `Font`). No body change needed — verify by build.

Same check for the string-name `DrawTextWithOutline(string fontName, …, string text, …)` if it likewise indirects through the dictionary; current source has its 5 calls go through `graphics.DrawText(fontName, …)` directly (string-keyed), so it's untouched.

### 6. Build and verify
Run `dotnet build` from the solution root. No new warnings, no errors. The previously-latent type mismatch (`Font` arg to `FontSheet` parameter in the string-name span/outline overloads) is now fixed by overload resolution picking up the new `Font` overloads.

## Test Plan
- [ ] `dotnet build` from the solution root — clean, zero new warnings on `DrawTextWithSpans.cs` and `TextWithOutline.cs`. This should also resolve any pre-existing compile error in those files caused by `Fonts[fontName]` returning `Font` against `FontSheet`-typed parameters.
- [ ] `dotnet test` — all existing tests pass (no test changes expected; the new overloads have no tests yet, but neither do existing `FontSheet` ones).
- [ ] Manual smoke (single-sheet font): in a sample game, build a `Font` from a single `FontSheet` and call each new overload (`DrawText(font, …, ReadOnlySpan<char>, …)`, `DrawText(font, …, Span<char>, …)`, `DrawTextWithOutline(font, …, string, …)`, `DrawTextWithOutline(font, …, ReadOnlySpan<char>, …)`) — output is visually identical to the equivalent `FontSheet` calls (single-sheet fast path).
- [ ] Manual smoke (multi-sheet font, if a multi-sheet font is available): build a `Font` from two `FontSheet`s covering disjoint character ranges (e.g. ASCII + extended Latin); render mixed text — characters from each range render via the correct sheet, newlines advance by `font.LineHeight`.
- [ ] Manual smoke (string-name overload regression): a call to `graphics.DrawText("MainFont", x, y, someSpan, color)` (where `MainFont` is registered in `Fonts`) compiles and renders correctly — confirms the latent `Font` vs `FontSheet` mismatch is resolved.
- [ ] Manual smoke (outline regression): a call to `graphics.DrawTextWithOutline("MainFont", x, y, someSpan, fillColor, outlineColor)` compiles and renders correctly with outline.

## Learnings

### Architectural decisions
- **Open Decision 1 (newline advance)** resolved to `font.LineHeight`, matching the core `GraphicsManager.DrawText(Font, …, string, …)` pattern from the prereq ticket. Consistency between core and extension multi-sheet bodies keeps cursor results identical regardless of which entry point a caller picks.
- **Open Decision 2 (per-char dispatch)** resolved to call `graphics.DrawText(sheet, position.x, position.y, c, color)` (the existing `FontSheet` per-char overload) after `TryGetSheet` resolves the sheet — matches the core `Font`-typed `DrawText` body in `GraphicsManager.Text.cs` line 88-92. Avoids re-resolving the sheet that was just resolved.
- **Outline forwards to core `DrawText(Font, …)` not to extension `DrawText(Font, …)`**: the `string`-text outline overload forwards to the core `GraphicsManager.DrawText(Font, …, string, …)` (added by prereq); the `ReadOnlySpan<char>`-text outline forwards to the new extension `DrawText(Font, …, ReadOnlySpan<char>, …)` from this ticket. Both outline overloads do the 5-call pattern — there is no need for the outline body to know whether it crosses an extension/core boundary.

### Problems encountered
- **Pre-existing latent compile errors in `WavyText.cs`** outside this ticket's scope: `WavyText.cs` lines 26, 27, 52, 84 access `font.CharacterWidth`/`HorizontalSpacing`/`CharacterHeight` on the result of `graphics.Fonts[fontName]`, which is `Font` (record) — those properties live on `FontSheet`, not `Font`. Same migration shape as the spans/outline files but the WavyText helpers measure horizontally based on a single character width, which assumes a single uniform sheet. A follow-up ticket should decide: (a) require single-sheet fonts for `DrawWavyText` (delegate via `font.Sheets[0]`), or (b) measure with `font.ComputeWidth` per character via `TryGetSheet`. Not fixed here.

### Interesting tidbits
- C# 14 `extension(GraphicsManager graphics) { ... }` extension blocks let multiple overloads share a single receiver binding without per-method `this GraphicsManager graphics` parameters. New overloads must go inside the existing block to keep style consistent.
- The `Font + char[]` overload (line 121-122) needed no edit: its body is `text.AsSpan()` which now binds to the new `DrawText(Font, …, ReadOnlySpan<char>, …)` overload (or `Span<char>` — both have identical multi-sheet semantics, so overload resolution either way is correct).
- `Font.TryGetSheet` returns a nullable `FontSheet?` even when `true` — `sheet!` null-forgiving operator needed when forwarding to the per-char `DrawText(FontSheet, …)` overload, mirroring the core `GraphicsManager.Text.cs` style.

### Related areas affected
- `BenMakesGames.PlayPlayMini.GraphicsExtensions\WavyText.cs` — same Font/FontSheet migration needed; flagged above for a follow-up ticket.

### Rejected alternatives
- **Per-char draw via core `DrawText(Font, …, char, …)`** — would re-resolve the sheet inside the per-char call after we already resolved it via `TryGetSheet`. Rejected: redundant work in the inner loop.
- **Newline advance via current line's tallest sheet** — required tracking max-height-this-line across the loop. Rejected: inconsistent with core `Font` overloads' behavior, and `Font.LineHeight` is a sufficient pre-computed worst-case.
