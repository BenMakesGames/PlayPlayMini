# UI elements: route layout math through new `Font` API

## Context
**Current behavior**: The five UI elements in `BenMakesGames.PlayPlayMini.UI\UIElements\` (`Button`, `Label`, `LabelWithIcon`, `Checkbox`, `RangeSelect`) compute their `Width`/`Height` and centering offsets by reading `UI.Font.CharacterWidth`, `UI.Font.CharacterHeight`, and `UI.Font.HorizontalSpacing` directly. After the i18n refactor, those properties live on `FontSheet` — `UI.Font` returns a `Font` (see `UIService.Font` and `UIService.GetFont()`, both `Graphics.Fonts[GetTheme().FontName]` lookups returning `Font`). The five files no longer compile.

**New behavior**: Every per-string width calculation in those five elements goes through `Font.ComputeWidth(string)`, and every single-line height calculation goes through `Font.MaxCharacterHeight`. The elements compile cleanly, render identically for single-sheet fonts, and pick up correct measurement automatically when a multi-sheet (e.g., Latin + CJK) font is themed in. Public `Width`/`Height`/`Draw` signatures are unchanged.

## Prerequisites
- `font-multi-sheet-helpers` — adds `Font.MaxCharacterHeight`, `Font.LineHeight`, `Font.ComputeWidth(string)`, `Font.ComputeHeight(string)`, `Font.TryGetSheet(char, out FontSheet?)`. (The `MaxCharacterHeight` and `ComputeWidth` members are the two this ticket consumes.)
- `graphicsmanager-font-text-overloads` — already-merged sibling ticket that gave `GraphicsManager` `DrawText(Font, ...)` overloads. The five elements already call `UI.Graphics.DrawText(UI.Font, ...)`; with that ticket landed those calls bind correctly. Listed here so the implementer treats build-clean as a real gate rather than a goal still in flight.

If filenames have drifted by implementation time, grep `docs/tickets/complete/` for `MaxCharacterHeight` and `DrawText(Font` respectively.

## Scope
### In scope
Edits to five files in `BenMakesGames.PlayPlayMini.UI\UIElements\`:
- `Button.cs`
- `Label.cs`
- `LabelWithIcon.cs`
- `Checkbox.cs`
- `RangeSelect.cs`

Each file's `Width` getter, `Height` getter, and `Draw` body are inspected and any reference to `Font.CharacterWidth` / `Font.CharacterHeight` / `Font.HorizontalSpacing` is rewritten in terms of `Font.ComputeWidth(text)` and `Font.MaxCharacterHeight`.

### Out of scope
- Other `UIElements` files (`Image.cs`, `Canvas.cs`, `Window.cs`, `UIContainer.cs`, `IUIElement.cs`) — none read those font members today; verify in passing but do not edit speculatively.
- `UIService.cs`, themes, and any non-UI projects (`.GraphicsExtensions`, `.VN`, `.Performance`) — those have their own follow-up tickets.
- New unit tests. No automated visual layout test harness exists for these elements; the test plan is build-clean + manual eyeball.
- Caching `Font` instances or `ComputeWidth` results at the element level (see Constraints).
- Any change to the public surface of these elements: ctor signatures, `Width`/`Height` types, `Draw` parameters all stay byte-for-byte.

## Relevant Docs & Anchors
- **Code anchors**:
  - `BenMakesGames.PlayPlayMini.UI\Services\UIService.cs` — `public Font Font => Graphics.Fonts[GetTheme().FontName];` and the parallel `public Font GetFont() => ...`. Both return `Font`. Both do a dictionary lookup *and* a theme lookup per call.
  - `BenMakesGames.PlayPlayMini\Model\Font.cs` — read `MaxCharacterHeight` (init-only, "tallest glyph, no spacing"), `LineHeight` (init-only, includes inter-line spacing), and `ComputeWidth(string)` (handles `\r`/`\n` and per-char `TryGetSheet` lookup, allocation-free).
  - `BenMakesGames.PlayPlayMini\Model\FontSheet.cs` — confirms `CharacterWidth` / `CharacterHeight` / `HorizontalSpacing` / `VerticalSpacing` now live here, not on `Font`.
- **Analogue tickets**:
  - `docs/tickets/complete/2026-04-25 graphicsmanager-font-text-overloads.md` — same single-vs-multi-sheet design and the same "single-sheet fast path is internal to the helper, callers don't branch" idiom this ticket relies on.
  - `docs/tickets/complete/2026-04-25 wraptext-font-overload.md` — for stylistic precedent of "swap to the `Font` API at the call site, leave the existing helper untouched."

## Constraints & Gotchas
- **`UI.Font` is a per-frame, per-element property doing a `Dictionary<string, Font>` lookup keyed off a `Theme` lookup.** Re-reading it three or four times in one `Draw` call multiplies that work. Hoist to a local within `Draw` and any `Width`/`Height` getter that reads it more than once. Surface this as a default in Open Decisions, not a hard requirement — taste call.
- **Allocation budget**: `ComputeWidth(string)` walks `text.Length` chars. The previous arithmetic was a single multiply. Both are O(n) and zero-alloc. Acceptable per spec; do not introduce per-element caching here. If a hot-path consumer surfaces later, that's a caching ticket above this layer, not these five files.
- **Public API of the five elements is frozen.** `Width`/`Height` remain `int` properties; `Draw(int, int, GameTime)` keeps its signature. Ctors unchanged. No new fields exposed.
- **XML doc comments unchanged.** None of these elements currently document `Width`/`Height`/`Draw` for consumers — leave it that way; this ticket is not a doc pass.
- **`MaxCharacterHeight` vs `LineHeight`**: `MaxCharacterHeight` is "tallest glyph, no inter-line spacing" — exactly what single-line UI layout wants. `LineHeight = max(CharacterHeight + VerticalSpacing)` and over-estimates a single-line element by one inter-line gap. **Use `MaxCharacterHeight` everywhere** in this ticket; never `LineHeight`. (This is the Open Decision #2 default — if a reading uncovers an element where the old `CharacterHeight` was deliberately the inter-line spacing, raise it; the expected answer is still `MaxCharacterHeight`.)
- **`RangeSelect.Draw` calls `UI.GetFont()`** rather than `UI.Font`. Both resolve to the same `Font`; the inconsistency is pre-existing and not load-bearing. Pick one and stay consistent within the file (default: `UI.Font`, matching the other four elements), but do not chase this style across the codebase as part of this ticket.

## Open Decisions
1. **Hoist `UI.Font` to a local before multiple uses.** Default: yes, in any `Draw` body that reads it 2+ times and in `Width`/`Height` getters that compose multiple reads (e.g. `LabelWithIcon.Width` reads it twice in one expression). Skip the hoist when there's a single read — extra local hurts readability without a perf win.
2. **Single-line height: `MaxCharacterHeight` vs `LineHeight`.** Default: always `MaxCharacterHeight` for these five elements. Re-read each height expression and confirm the old code was using `CharacterHeight` (single-glyph) not `CharacterHeight + VerticalSpacing` (line advance). Spec confirms `MaxCharacterHeight` is correct in every case — this open decision exists only as a checkpoint during implementation.
3. **`RangeSelect.Draw` `UI.GetFont()` vs `UI.Font`.** Default: switch to `UI.Font` for consistency with the other four elements (same target, fewer redundant theme lookups since you'll be hoisting anyway). If implementer prefers leaving `GetFont()` for diff minimalism, that's also fine — the call resolves identically.

## Acceptance Criteria
- [ ] No reference to `Font.CharacterWidth`, `Font.CharacterHeight`, `Font.HorizontalSpacing`, or `Font.VerticalSpacing` remains in any of the five files (these members no longer exist on `Font`; the names now live on `FontSheet`). Verifiable by `grep -nE "\.(CharacterWidth|CharacterHeight|HorizontalSpacing|VerticalSpacing)" BenMakesGames.PlayPlayMini.UI/UIElements/{Button,Label,LabelWithIcon,Checkbox,RangeSelect}.cs` returning no hits.
- [ ] Every per-string width calculation in those files routes through `Font.ComputeWidth(text)`. The pattern `Text.Length * font.CharacterWidth + (Text.Length - 1) * font.HorizontalSpacing` (and its `Label.Length` variant) does not appear in any of the five files post-edit.
- [ ] Every single-line vertical sizing in those files uses `Font.MaxCharacterHeight` — not `LineHeight`, not `CharacterHeight` (which no longer exists).
- [ ] Public surface of all five elements is unchanged: ctor parameter lists, `Width`/`Height` types and access levels, `Draw(int, int, GameTime)` signature.
- [ ] `Button.Width` is `ForcedWidth ?? (6 + UI.Font.ComputeWidth(Label))` (or equivalent with a hoisted local).
- [ ] `Button.Draw` centers the label using `UI.Font.ComputeWidth(Label)` (or a hoisted local), not the old length-times-width arithmetic.
- [ ] `Label.Width` is `ForcedWidth ?? UI.Font.ComputeWidth(Text)`; `Label.Height` is `UI.Font.MaxCharacterHeight`; `Label.Draw` uses `ComputeWidth(Text)` for centering.
- [ ] `LabelWithIcon.Width` uses `UI.Font.ComputeWidth(Text)` plus the existing `+ 1 + 2 + SpriteRectangle.Width` (or whatever the pre-edit constant offset is — keep it identical); `LabelWithIcon.Height` is `Math.Max(SpriteRectangle.Height, UI.Font.MaxCharacterHeight)`; `LabelWithIcon.Draw`'s vertical centering uses `MaxCharacterHeight`.
- [ ] `Checkbox.Width` is `CheckBox.SpriteWidth + 1 + UI.Font.ComputeWidth(Label)`; `Checkbox.Height` is `Math.Max(CheckBox.SpriteHeight, UI.Font.MaxCharacterHeight)`.
- [ ] `RangeSelect.Height` is `Math.Max(UI.Graphics.SpriteSheets[UI.GetTheme().ButtonSpriteSheetName].SpriteHeight, UI.Font.MaxCharacterHeight)`.
- [ ] `dotnet build` of `BenMakesGames.PlayPlayMini.UI` (and the solution as far as previously-compiling projects are concerned) is clean — no new warnings, and the previously-broken `BenMakesGames.PlayPlayMini.UI` project now compiles.
- [ ] No new `using` directives are added beyond what the file already has — `Font.ComputeWidth` and `Font.MaxCharacterHeight` live in `BenMakesGames.PlayPlayMini.Model`, which `Label`, `Checkbox`, `LabelWithIcon`, and `RangeSelect` already import transitively or directly. Verify and only add a `using` if compilation requires it.

## Implementation
### 1. Re-read each of the five files end-to-end
Before editing, open each of `Button.cs`, `Label.cs`, `LabelWithIcon.cs`, `Checkbox.cs`, `RangeSelect.cs` in full. Note every `UI.Font.<member>` call site — `Width`, `Height`, `Draw`, and any helpers. Confirm the spec's enumeration of which expressions need swapping matches what's actually in the file (drift is possible if any element has been touched since the spec was drafted).

### 2. `Button.cs`
`Width`: replace the `Label.Length * UI.Font.CharacterWidth + (Label.Length - 1) * UI.Font.HorizontalSpacing` term inside the `ForcedWidth ??` expression with `UI.Font.ComputeWidth(Label)`. The leading `6 +` (left-edge padding) and the `ForcedWidth ??` short-circuit stay. `Height` already uses sprite metrics, not font — leave alone. `Draw`: the label-centering math (`X + (Width - (Label.Length * UI.Font.CharacterWidth + (Label.Length - 1) * UI.Font.HorizontalSpacing)) / 2 + xOffset`) collapses to `X + (Width - UI.Font.ComputeWidth(Label)) / 2 + xOffset`. `Draw` reads `UI.Font` once for `ComputeWidth` and once again inside `DrawText(UI.Font, ...)` — hoist `var font = UI.Font;` at the top of `Draw` per Open Decision #1.

### 3. `Label.cs`
`Width`: `ForcedWidth ?? UI.Font.ComputeWidth(Text)`. `Height`: `UI.Font.MaxCharacterHeight`. `Draw`: same centering swap as Button — `(Width - UI.Font.ComputeWidth(Text)) / 2`. Hoist `UI.Font` to a local in `Draw` (used twice).

### 4. `LabelWithIcon.cs`
`Width`: replace the length-arithmetic prefix with `UI.Font.ComputeWidth(Text)`; the `+ 1 + 2 + SpriteRectangle.Width` tail is unchanged (those constants are layout, not font, math). `Height`: change `UI.Font.CharacterHeight` to `UI.Font.MaxCharacterHeight` inside the `Math.Max`. `Draw`: the vertical-centering term `(SpriteRectangle.Height - UI.Font.CharacterHeight) / 2` becomes `(SpriteRectangle.Height - UI.Font.MaxCharacterHeight) / 2`. Hoist `UI.Font` in `Draw` if it ends up being read twice (once for the centering calc, once via `DrawText(UI.Font, ...)`).

### 5. `Checkbox.cs`
`Width`: `CheckBox.SpriteWidth + 1 + UI.Font.ComputeWidth(Label)`. `Height`: replace `UI.Font.CharacterHeight` inside the `Math.Max(CheckBox.SpriteHeight, ...)` with `UI.Font.MaxCharacterHeight`. `Draw` does not currently read font width/height for layout (it just calls `DrawText`) — leave alone.

### 6. `RangeSelect.cs`
`Height`: replace `UI.Font.CharacterHeight` inside the `Math.Max` with `UI.Font.MaxCharacterHeight`. `Draw` calls `UI.GetFont()` rather than `UI.Font`; pick one (default `UI.Font` per Open Decision #3) and stay consistent within the file. No width math swap needed — `Width` is set from a ctor parameter, not computed from text. The `Decrement`/`Increment` `Button` instances inherit their fix automatically from step 2.

### 7. Verify no other `UIElements` files reference the dead members
Spot-check `Image.cs`, `Canvas.cs`, `Window.cs`, `UIContainer.cs`, `IUIElement.cs` with a grep for `Font.CharacterWidth`/`CharacterHeight`/`HorizontalSpacing`. If any hits surface (spec says no), pull them into this ticket's edit set; otherwise note in the implementation summary that the scope held.

### 8. Build and verify
`dotnet build` from the repo root. The `BenMakesGames.PlayPlayMini.UI` project should now compile clean. Other projects (`.GraphicsExtensions`, `.VN`, `.Performance`, `.Tests`) may still have their own pre-existing failures (separate follow-up tickets); confirm they are unchanged in number and shape, not introduced or amplified by these edits.

## Test Plan
- [ ] `dotnet build` from repo root: `BenMakesGames.PlayPlayMini.UI` compiles clean with no new warnings. Pre-existing failures in other projects are unchanged.
- [ ] `grep -nE "\.(CharacterWidth|CharacterHeight|HorizontalSpacing)" BenMakesGames.PlayPlayMini.UI/UIElements/*.cs` returns no hits in the five touched files.
- [ ] `dotnet test` passes (no UI-element tests today, but confirm no regression in `BenMakesGames.PlayPlayMini.Tests`).
- [ ] Manual: launch a sample game/scene that uses each of `Button`, `Label`, `LabelWithIcon`, `Checkbox`, `RangeSelect` with the default single-sheet themed font. Confirm visual layout is pixel-identical to pre-edit: button widths, label centering, checkbox alignment, range-select arrow positioning, label-with-icon vertical centering all unchanged.
- [ ] Manual (optional, only if a multi-sheet `Font` is easy to build in a sample): swap the themed font to a multi-sheet font containing characters that span sheets in a `Button` or `Label`. Confirm `Width` now expands correctly to fit the wider glyphs (would have been wrong with the old `Label.Length * CharacterWidth` math).

## Learnings

### Architectural decisions
- **Open Decision #1 (hoist `UI.Font`)**: hoisted in `Button.Draw`, `Label.Draw`, `LabelWithIcon.Draw` (each reads twice). Skipped in `Checkbox.Draw` and `RangeSelect.Draw` — single read, no benefit. Width/Height getters left un-hoisted: each reads `UI.Font` exactly once now (the `ComputeWidth(...)` plus `MaxCharacterHeight` cases are in separate getters).
- **Open Decision #2 (`MaxCharacterHeight` vs `LineHeight`)**: `MaxCharacterHeight` everywhere, as default. All five existing height expressions used the old `Font.CharacterHeight` (single-glyph), confirming the spec.
- **Open Decision #3 (`UI.GetFont()` vs `UI.Font` in `RangeSelect.Draw`)**: switched to `UI.Font` for consistency with the other four elements.

### Scope held
- Public surface unchanged — ctor params, `Width`/`Height` types/access, `Draw(int, int, GameTime)` signatures all byte-for-byte.
- No new `using` directives needed; `Font` was already in scope in every file via existing imports.
- Spot-check of `Image.cs`, `Canvas.cs`, `Window.cs`, `UIContainer.cs`, `IUIElement.cs`: zero hits for the dead members. Scope held at 5 files.

### Problems encountered
- `BenMakesGames.PlayPlayMini.UI` build still fails with one pre-existing error in `UIService.cs:78` (`Cursor.Draw(gameTime)` — `MouseManager.Draw` signature changed to accept `AbstractGameState?` instead of `GameTime`). Unrelated to this ticket; codebase is mid-recovery and that is a separate follow-up.

### Interesting tidbits
- `Font.ComputeWidth` collapses two callsite expressions per element into one method call and gains correct multi-sheet measurement for free. The `Label.Length * CharacterWidth + (Label.Length - 1) * HorizontalSpacing` pattern was duplicated across 4 of the 5 files — perfect candidate for the helper that already existed.
- `RangeSelect` had two long-standing inconsistencies (`UI.GetFont()` vs `UI.Font`, and the `_enabled` field next to all-property style). Only the font lookup was touched per ticket scope.

### Rejected alternatives
- Caching the `Font` instance on the element (e.g. lazy field) — explicitly out of scope per Constraints. If profiling later shows the per-frame dictionary lookup matters, that's a caching ticket above this layer.
- Hoisting `UI.Font` inside `Width`/`Height` getters — each ended up with a single `UI.Font` read after the rewrite, so the local would be pure noise.
