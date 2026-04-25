# GraphicsManager.Text.cs: Add Font overloads + fix broken string-name overloads

## Context

**Current behavior**: `GraphicsManager.Text.cs` exposes text drawing/measuring methods that take `FontSheet` (the original single-bitmap font model). The `string fontName` convenience overloads in the same file currently route to those `FontSheet`-typed methods via `Fonts[fontName]` — but `Fonts` is `IReadOnlyDictionary<string, Font>` (not `FontSheet`). This is a **type mismatch bug** post-`Font`/`FontSheet` split: the string-name overloads cannot bind to the `FontSheet` parameter without an implicit conversion that does not exist. (Confirmed by reading `GraphicsManager.cs` and `Model/Font.cs` — there is no `implicit operator FontSheet`.)

**New behavior**: For each public `FontSheet`-based text method in `GraphicsManager.Text.cs`, a sibling `Font`-based overload exists. The `string fontName` overloads route to the new `Font` overloads (correct types). Multi-sheet `Font`s render correctly with per-character sheet selection; single-sheet `Font`s take a fast path that delegates to the existing tight `FontSheet` method. All existing `FontSheet`-typed public methods remain unchanged for backward compatibility — consumers migrate at their own pace.

## Prerequisites

- `font-multi-sheet-helpers` (separate ticket) — adds these to `Font`: `bool TryGetSheet(char c, out FontSheet? sheet)`, `int LineHeight` (init-only, precomputed), `int ComputeHeight(string text)`. `Font.ComputeWidth(string)` already exists.
- `wraptext-font-overload` (separate ticket) — adds `WrapText(this string text, Font font, int maxWidth)` extension to `BenMakesGames.PlayPlayMini\Extensions\StringExtensions.cs` to mirror the existing `WrapText(this string, FontSheet, int)`.

If those filenames have drifted by implementation time, grep `docs/tickets/` and `docs/tickets/complete/` for `TryGetSheet`, `LineHeight`, and `WrapText` respectively.

## Scope

### In scope

- Add `Font`-typed public overloads alongside every existing `FontSheet`-typed public method in `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs`.
- Fix existing `string fontName` overloads in the same file to delegate to the new `Font` overloads.
- XML doc comments on every new public overload (this project generates a doc website from them — `//` comments do not surface).

### Out of scope

- Modifying any existing `FontSheet`-typed public methods. Additive change only.
- Migrating callers across the rest of the solution (e.g., `BenMakesGames.PlayPlayMini.GraphicsExtensions`, `.VN`, `.Performance`) to the new overloads — separate follow-ups.
- Changing the private `FontRectangle(FontSheet, int)` helper or `DrawTexture` calls.
- New unit tests for visual rendering — no automated visual test infra here.

## Relevant Docs & Anchors

- **Primary file**: `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs` — read end-to-end. Note the private `FontRectangle(FontSheet, int)` helper, the `DrawTexture(...)` calls, the `\r`/`\n` handling pattern (existing `FontSheet` `DrawText` treats `\r` and `\n` identically: each advances a line), and the `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attributes on the thin `string fontName` delegating overloads.
- **Model**: `BenMakesGames.PlayPlayMini\Model\Font.cs` and `Model\FontSheet.cs` for the data model. `Font` is a `record` wrapping `List<FontSheet> Sheets` and gets the prerequisite-ticket helpers.
- **Word wrap**: `BenMakesGames.PlayPlayMini\Extensions\StringExtensions.cs` — current `WrapText(string, FontSheet, int)` shape; the prerequisite ticket adds the `Font` overload that `DrawTextWithWordWrap(Font,...)` and `ComputeDimensionsWithWordWrap(Font,...)` will call.
- **Texture draw helper**: `DrawTexture(Texture2D, int, int, Rectangle, Color)` lives in `GraphicsManager.Textures.cs`. Same call signature used by existing `FontSheet` `DrawText`.

## Constraints & Gotchas

- **No allocations on the 60fps hot path.** No LINQ, no closures, no delegates, no shared "walker" abstraction. Tight `for`/`foreach` over `string`/`char`. The user has explicitly rejected an iterator/walker that would unify single- and multi-sheet paths.
- **Single-sheet fast path required**: every multi-sheet-capable overload must short-circuit when `font.Sheets.Count == 1` by delegating to the existing `FontSheet`-typed method. Most consumers will have one sheet; this preserves JIT-inlined tight loops and zero added overhead.
- **Public NuGet API**: keep all existing `FontSheet` public methods byte-for-byte unchanged. Additive only.
- **XML docs feed a doc website** (per user memory): every new public overload needs `/// <summary>`, `/// <param>`, `/// <returns>` mirroring existing methods in the file. Do not use `//` comments to explain public API.
- **`[MethodImpl(MethodImplOptions.AggressiveInlining)]`**: apply to thin delegating overloads (string-name → `Font`, single-sheet fast path → `FontSheet`) where the existing file already uses it. Do not apply to the multi-sheet loop bodies.
- **`\r`/`\n` semantics**: existing `FontSheet` `DrawText` treats `\r` and `\n` identically — each resets `x` and advances `y`. The multi-sheet path must match this (do not silently skip `\r`). `Font.ComputeWidth` differs (it skips `\r` and only advances on `\n`); for the draw path, mirror the existing draw semantics, not `ComputeWidth`'s.

## Open Decisions

1. **Glyph fallback when no sheet matches a char** — existing `FontSheet` `DrawText` only draws when `character >= font.FirstCharacter` and otherwise silently no-ops (no advance). Default for multi-sheet path: same — if `TryGetSheet` returns false, skip the char without advancing `x`. Implementer may revisit if it produces visibly wrong output during manual testing.
2. **`\r\n` pair handling** — existing draw treats `\r` and `\n` independently (so `\r\n` produces two line advances). Default: preserve existing buggy-but-stable behavior. Do not "fix" it as part of this ticket.
3. **Width advance on multi-sheet path** — use the matched sheet's `CharacterWidth + HorizontalSpacing`. If implementer finds a cleaner shape (e.g., reusing a `Font`-level helper added by the prereq ticket), prefer that, but do not introduce per-char allocations.

## Acceptance Criteria

- [ ] Every public `FontSheet`-based method in `GraphicsManager.Text.cs` has a sibling `Font`-typed overload with the same parameter shape (replacing the leading `FontSheet font` with `Font font`):
  - `DrawText(Font, int x, int y, string text, Color color)` returning `(int x, int y)`
  - `DrawText(Font, int x, int y, char character, Color color)` returning `(int x, int y)`
  - `DrawTextWithWordWrap(Font, int x, int y, int maxWidth, string text, Color color)` returning the same tuple shape as the `FontSheet` sibling
  - `ComputeDimensionsWithWordWrap(Font, int maxWidth, string text)` returning `(int Width, int Height)`
  - `PretendDrawText(Font, int x, int y, string text)` returning the same tuple shape as the `FontSheet` sibling
- [ ] Each new `Font` overload begins with a `font.Sheets.Count == 1` fast path that delegates to the existing `FontSheet`-typed sibling using `font.Sheets[0]`.
- [ ] Every public `string fontName` overload in the file (`DrawText` ×2, `DrawTextWithWordWrap`, `ComputeDimensionsWithWordWrap`, `PretendDrawText`) routes to the corresponding `Font` overload — no `string fontName` overload passes a `Font` value into a `FontSheet`-typed parameter.
- [ ] The string-name → `Font` delegating overloads carry `[MethodImpl(MethodImplOptions.AggressiveInlining)]` matching the style already used in the file.
- [ ] Every new public overload has XML doc comments (`<summary>`, `<param>`, `<returns>`) mirroring the surrounding style.
- [ ] All existing `FontSheet`-typed public methods in `GraphicsManager.Text.cs` are unchanged (signatures and bodies).
- [ ] The multi-sheet draw path introduces no per-character heap allocations (no LINQ, no delegate captures, no `params` arrays, no string concatenation).
- [ ] `DrawTextWithWordWrap(Font,...)` and `ComputeDimensionsWithWordWrap(Font,...)` call the `Font`-typed `WrapText` extension from the prerequisite ticket — not the `FontSheet` overload via `font.Sheets[0]`.
- [ ] Solution compiles cleanly (no new warnings) and `dotnet test` passes.

## Implementation

### 1. Re-read the existing file end-to-end

Open `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs`. Note (a) the private `FontRectangle(FontSheet, int)` helper, (b) the `\r`/`\n` advance pattern, (c) which overloads use `[MethodImpl(MethodImplOptions.AggressiveInlining)]`, (d) the tuple return shape `(int x, int y)` vs. `(int, int)`. Mirror these conventions exactly.

### 2. Verify prerequisite surface on `Font`

Confirm `Font.TryGetSheet`, `Font.LineHeight`, `Font.ComputeHeight`, `Font.ComputeWidth` are present. Confirm `WrapText(string, Font, int)` exists in `Extensions\StringExtensions.cs`. If a prereq is missing, stop and surface — do not stub.

### 3. Add `DrawText(Font, int, int, string, Color)` overload

Place adjacent to the existing `DrawText(FontSheet, int, int, string, Color)`. Begin with the `Sheets.Count == 1` fast path delegating to the `FontSheet` sibling. Multi-sheet path: tight `for` over `text`, mirroring the existing `FontSheet` loop's `\r`/`\n` handling but advancing `y += font.LineHeight` (no `Max`, no recompute) on newline. For a printable char, call `font.TryGetSheet(c, out var sheet)`; on hit, compute the glyph rect via `FontRectangle(sheet, c - sheet.FirstCharacter)` and `DrawTexture(sheet.Texture, ...)`, then advance `x += sheet.CharacterWidth + sheet.HorizontalSpacing`. On miss, skip without advancing (mirrors existing `character >= font.FirstCharacter` gate).

### 4. Add `DrawText(Font, int, int, char, Color)` overload

Place adjacent to the `FontSheet` char overload. Same fast path. Multi-sheet path is the per-char body of step 3 specialized for one character. Return the `(x, y)` tuple in the same shape as the `FontSheet` sibling.

### 5. Add `DrawTextWithWordWrap(Font, int, int, int, string, Color)` overload

Place adjacent to the `FontSheet` sibling. Single-sheet fast path delegates to `FontSheet` sibling. Multi-sheet: call `text.WrapText(font, maxWidth)` (the prereq `Font`-typed extension), then call the new `DrawText(Font, ...)` from step 3. Carry `[MethodImpl(MethodImplOptions.AggressiveInlining)]` if the `FontSheet` sibling does.

### 6. Add `ComputeDimensionsWithWordWrap(Font, int, string)` overload

Place adjacent to the `FontSheet` sibling. Single-sheet fast path delegates. Multi-sheet: use `text.WrapText(font, maxWidth)` to get the wrapped string, then derive width via `font.ComputeWidth(wrapped)` and height via `font.ComputeHeight(wrapped)` (both from the prereq ticket). Return `(Width, Height)`.

### 7. Add `PretendDrawText(Font, int, int, string)` overload

Place adjacent to the `FontSheet` sibling. Single-sheet fast path delegates. Multi-sheet: tight `foreach` over `text` mirroring the existing `FontSheet` `PretendDrawText` body — on `\r` or `\n` reset `x` and advance `y += font.LineHeight`; on a printable char that resolves via `TryGetSheet`, advance `x += sheet.CharacterWidth + sheet.HorizontalSpacing`; on miss, skip. Return the `(currentX, currentY)` tuple. (Kept for parity even though `Font.ComputeWidth`/`ComputeHeight` supersede it; consumers may rely on the tuple shape.)

### 8. Fix every `string fontName` overload

Re-route each existing string-name overload (`DrawText` ×2, `DrawTextWithWordWrap`, `ComputeDimensionsWithWordWrap`, `PretendDrawText`) to call the new `Font` overload — not the `FontSheet` overload. `Fonts[fontName]` returns `Font`, so the call sites compile cleanly. Keep `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on these one-line delegators.

### 9. Self-check: no allocations on multi-sheet path

Read each new multi-sheet body. Confirm: no LINQ, no `string.Split`, no closures, no `params`, no `List<>`/`StringBuilder` per call beyond what the existing `FontSheet` path already incurs (e.g., `WrapText` already builds a string — that's pre-existing). The per-character draw loop must do zero heap work.

## Test Plan

- [ ] `dotnet build` clean across the entire solution — no new warnings, no errors. (Confirms the type-mismatch bug in string-name overloads is fixed and new overloads compile.)
- [ ] `dotnet test` passes (existing `WordWrapTests` and any others).
- [ ] Manual: in a sample game/scene, call `DrawText(font, ...)` where `font` has a single `FontSheet` — visually identical to the previous `DrawText(fontSheet, ...)` rendering.
- [ ] Manual: call `DrawText(fontName, ...)` (string-name overload) and confirm it now compiles and renders correctly (regression-of-bug check).
- [ ] Manual: render a multi-line string containing `\n` with a `Font` whose `Sheets.Count > 1` (mock by constructing one in a test scene if needed) — confirm characters route to the correct sheet and lines advance by `font.LineHeight`.
- [ ] Spot-check that no caller in `BenMakesGames.PlayPlayMini.GraphicsExtensions`, `.VN`, `.Performance`, `.Tests` regressed (those still call `FontSheet` overloads; they should compile and behave identically since those methods are unchanged).
