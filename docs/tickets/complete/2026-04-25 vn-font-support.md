# VN project Font support

## Context
**Current behavior**: Code in `BenMakesGames.PlayPlayMini.VN` reads single-sheet metrics (`CharacterWidth`, `CharacterHeight`, `HorizontalSpacing`, `VerticalSpacing`) directly off `Font` instances obtained from `graphics.Fonts[VNSettings.DialogFont]`. Those properties no longer exist on `Font` after the i18n split — they live on `FontSheet`. The VN project does not compile against the current `Font` model.

**New behavior**: VN code measures and lays out dialog/choice/button UI using the multi-sheet-aware `Font` helpers added by `font-multi-sheet-helpers`: `LineHeight` for per-line advance, `MaxCharacterHeight` for single-line glyph height, and `ComputeWidth(text)` for text-width math. The VN project compiles cleanly and visual layout for single-sheet fonts is unchanged from the pre-split behavior.

## Prerequisites
- `font-multi-sheet-helpers` — adds `Font.LineHeight`, `Font.MaxCharacterHeight`, `Font.ComputeWidth`, `Font.ComputeHeight`, `Font.TryGetSheet`. (Already complete.)
- `graphicsmanager-font-text-overloads` — adds `GraphicsManager.DrawText(Font, ...)` overloads that the VN code's existing `graphics.DrawText(font, ...)` call sites bind against. (Already complete.)

## Scope
### In scope
Mechanical migration across four VN files:
- `BenMakesGames.PlayPlayMini.VN\Extensions\GraphicsManagerExtensions.cs` (`DrawCharacterDialog`)
- `BenMakesGames.PlayPlayMini.VN\GameStates\MakeChoice.cs`
- `BenMakesGames.PlayPlayMini.VN\Model\SceneController.cs` (the `AnimatedDialog.Draw` body)
- `BenMakesGames.PlayPlayMini.VN\Model\Buttons\TextButton.cs`

Each replaces stale `font.CharacterHeight` / `font.VerticalSpacing` / `font.CharacterWidth` / `font.HorizontalSpacing` reads on a `Font` instance with the appropriate `Font` helper.

### Out of scope
- Modifying `BenMakesGames.PlayPlayMini.GraphicsExtensions` callers (separate follow-up; flagged in `graphicsmanager-font-text-overloads.md` Learnings).
- Modifying `BenMakesGames.PlayPlayMini.Performance` benchmarks.
- Public API/signature changes to `TextButton`, `SceneController`, or any VN type.
- Adding new `Font` helpers — all three needed (`LineHeight`, `MaxCharacterHeight`, `ComputeWidth`) already exist.
- Refactoring `AnimatedDialog`'s `WrapText` / line-grouping logic — it already takes a `Font` via the `WrapText(string, Font, int)` extension from `wraptext-font-overload`.

## Relevant Docs & Anchors
- **Code anchors**:
  - `BenMakesGames.PlayPlayMini\Model\Font.cs` — definitions + XML docs for `LineHeight`, `MaxCharacterHeight`, `ComputeWidth`. `LineHeight` and `MaxCharacterHeight` are init-only (zero per-call cost); `ComputeWidth` walks chars zero-alloc.
  - `BenMakesGames.PlayPlayMini\Model\FontSheet.cs` — old home of `CharacterWidth`/`CharacterHeight`/`HorizontalSpacing`/`VerticalSpacing`. Reading these off a `FontSheet` is fine; reading them off a `Font` is the bug being fixed.
  - `graphics.Fonts[name]` returns `Font` (multi-sheet wrapper), per `GraphicsManager` — every `var font = graphics.Fonts[VNSettings.DialogFont];` site in the VN project is a `Font`.
- **Analogue tickets**:
  - `docs/tickets/complete/2026-04-25 graphicsmanager-font-text-overloads.md` — sibling caller-migration with the same property-mapping pattern at the framework's draw-method layer.
  - `docs/tickets/complete/2026-04-25 wraptext-font-overload.md` — sibling `Font` overload that this ticket's `AnimatedDialog` already consumes via `WrapText(font, ...)`.

## Constraints & Gotchas
- **VN draw paths run every frame.** All replacements use init-only properties or zero-alloc walks; do not introduce per-frame allocations or LINQ.
- **Public API of `TextButton`, `SceneController`, `MakeChoice`, and the `GraphicsManagerExtensions` extension methods is unchanged.** No new parameters, no renamed types — only the bodies are touched.
- **XML doc comments are preserved.** No public-API doc surface is added or removed by this ticket; existing `/// <inheritdoc/>` and any other doc comments stay as-is.
- **No back-compat shims.** Fix in place; do not add `Font.CharacterHeight` style aliases on `Font`.

## Property mapping (load-bearing)

| Old expression on a `Font` instance | New expression | Why |
|---|---|---|
| `font.CharacterHeight + font.VerticalSpacing` (per-line vertical advance) | `font.LineHeight` | Encapsulates multi-sheet "tallest line + gap"; single value precomputed at `Font` construction. |
| `font.CharacterHeight` alone (single-line glyph height) | `font.MaxCharacterHeight` | Tallest glyph height with no inter-line spacing — correct for single-line UI rectangles and text-baseline math. |
| `text.Length * font.CharacterWidth + (text.Length - 1) * font.HorizontalSpacing` (or any handwritten width math) | `font.ComputeWidth(text)` | Handles per-character sheet widths in mixed-sheet fonts; matches `GraphicsManager.DrawText`'s advance behavior. |

## Open Decisions
1. **Speaker-label background height in `DrawCharacterDialog`** — the current code uses `font.CharacterHeight` for both the speaker-label rectangle height and the speaker-text Y offset. Swap to `font.MaxCharacterHeight`. If a project ever defines a multi-sheet `DialogFont` where the speaker name uses glyphs from a sheet shorter than the tallest, `MaxCharacterHeight` will produce a slightly oversized label background — that is the intended trade-off for single-line layout consistency. Default: `MaxCharacterHeight`. Implementer may revisit if visual testing shows a clear issue.
2. **Trailing-spacing in multi-line dialog box height (`Lines * LineHeight + 7`)** — the old expression `Lines * (CharacterHeight + VerticalSpacing) + 7` over-counts by one trailing `VerticalSpacing` when `Lines >= 1` (last line has no following gap). The straight `Lines * font.LineHeight + 7` swap preserves the existing convention exactly (still over-counts by one trailing spacing); `Font.ComputeHeight` does *not* over-count. Default: keep the literal `Lines * font.LineHeight + 7` swap so visual layout for single-sheet fonts is byte-for-byte identical to before. Do *not* "fix" the over-count here — it is the existing visual contract.
3. **`TextButton` width math when `label` includes `\n`** — `font.ComputeWidth` handles `\n` (returns the widest line); the old hand-rolled `text.Length * CharacterWidth + ...` did not. Default: trust `ComputeWidth`. If any caller passes multi-line button labels (none observed in this ticket's scope), behavior changes from "compute width assuming single line" to "compute widest line width" — strictly better.

## Acceptance Criteria
- [ ] `BenMakesGames.PlayPlayMini.VN` project compiles cleanly (`dotnet build` succeeds, no new warnings).
- [ ] No file in the VN project reads `CharacterWidth`, `CharacterHeight`, `HorizontalSpacing`, or `VerticalSpacing` off a `Font` instance. (Reads off a `FontSheet` are still fine; this ticket only touches `Font` access.)
- [ ] Every per-line vertical-advance computation in the four target files uses `font.LineHeight`.
- [ ] Every single-line glyph-height computation (speaker-label rectangles, button height) uses `font.MaxCharacterHeight`.
- [ ] Every text-width computation (button width, speaker-name length) uses `font.ComputeWidth(text)`.
- [ ] Public type signatures for `TextButton` (constructor + properties), `SceneController`, `AnimatedDialog`, `MakeChoice`, `MakeChoiceConfig`, `Choice`, and the `GraphicsManagerExtensions.DrawCharacterDialog` / `DrawSpriteFlippedWithOutline` extension methods are byte-for-byte unchanged.
- [ ] No new `using System.Linq` imports added by this change. No new heap allocations on draw paths (no LINQ, no `string.Split`, no `params` arrays in the touched bodies).

## Implementation

### 1. `Extensions\GraphicsManagerExtensions.cs` — `DrawCharacterDialog`
Why: this is the most-visited single function in the VN draw path; both per-line and single-line metric mistakes live here.

Where: the body of `DrawCharacterDialog`, after `var font = graphics.Fonts[VNSettings.DialogFont];`.

What:
- Replace the `dialogHeight` calculation that uses `font.CharacterHeight + font.VerticalSpacing` with `dialogLines * font.LineHeight + 7`.
- Every other `font.CharacterHeight` reference in the function (used for speaker-label background-rectangle heights, the small-rectangle stack above the dialog box, and the speaker-text Y offset `graphics.Height - dialogHeight - font.CharacterHeight + 2`) becomes `font.MaxCharacterHeight`. These positions are all computing single-line glyph dimensions for the speaker-name label (per Open Decision #1).
- The `font.ComputeWidth(speaker.Name)` call already uses the right helper — no change.
- The two `graphics.DrawText(font, ...)` calls are already `Font`-typed (resolved by the prereq `graphicsmanager-font-text-overloads` ticket) — no change.

### 2. `GameStates\MakeChoice.cs` — `CreateChoiceButtons`
Why: vertical centering and per-row Y stride for the choice list.

Where: the body of `CreateChoiceButtons`, after `var font = Graphics.Fonts[VNSettings.DialogFont];`.

What:
- The `y` initial value: replace `(Graphics.Height - choices.Count * (font.CharacterHeight + font.VerticalSpacing + 4)) / 2` with `(Graphics.Height - choices.Count * (font.LineHeight + 4)) / 2`. The `+ 4` padding stays outside `LineHeight`.
- The per-row `y + i * (font.CharacterHeight + font.VerticalSpacing + 6)` becomes `y + i * (font.LineHeight + 6)`. Same logic — `+ 6` padding stays outside `LineHeight`.
- `font.ComputeWidth(c.Title)` in the `choices.Max(...)` LINQ expression already uses the right helper — no change. (LINQ `.Max()` here is on construction, not the per-frame draw path; leave it.)

### 3. `Model\SceneController.cs` — `AnimatedDialog.Draw`
Why: non-speaking dialog boxes (`Thinking`, `None`, `NoneInverted`) compute their own `dialogHeight` here, paralleling `DrawCharacterDialog`'s.

Where: inside `AnimatedDialog.Draw(GraphicsManager graphics, int xOffset)`, the line `var dialogHeight = Lines * (font.CharacterHeight + font.VerticalSpacing) + 7;` after `var font = graphics.Fonts[VNSettings.DialogFont];`.

What: replace with `var dialogHeight = Lines * font.LineHeight + 7;`. No other `Font.CharacterHeight`/`VerticalSpacing` reads in this method (the `+5` text Y offsets are literals against `dialogHeight` — leave them).

The `WrapText(graphics.Fonts[VNSettings.DialogFont], ...)` call in the `AnimatedDialog` ctor already binds to the `Font` overload from `wraptext-font-overload` — no change.

### 4. `Model\Buttons\TextButton.cs` — constructor
Why: button width and height math.

Where: the `TextButton` constructor body.

What:
- `Width = Math.Max(minWidth ?? 0, font.ComputeWidth(label) + 8);` already uses `ComputeWidth` — no change.
- `Height = font.CharacterHeight + 4;` becomes `Height = font.MaxCharacterHeight + 4;` (single-line button, per Open Decision #1's pattern).
- The `Draw` method's `graphics.DrawText(Font, X + 4, Y + 2, Label, textColor);` already binds to the `Font` overload — no change.

### 5. Self-check
- Grep the four touched files for `font.CharacterHeight`, `font.VerticalSpacing`, `font.CharacterWidth`, `font.HorizontalSpacing` (where `font` is a `Font` local) — should be zero hits after the change.
- Confirm no `using System.Linq` was added.
- Confirm no public signatures changed.

## Test Plan
- [ ] `dotnet build` of `BenMakesGames.PlayPlayMini.VN.csproj` (and the full solution) is clean — no errors, no new warnings.
- [ ] `dotnet test` passes (no VN-specific tests exist, but ensure no regression in `BenMakesGames.PlayPlayMini.Tests`).
- [ ] Manual: run a VN sample game with a single-sheet `DialogFont`. Verify visually:
  - Dialog box height (in `Speaking`, `Thinking`, `None`, `NoneInverted` styles) is unchanged.
  - Speaker-label background rectangle height + position is unchanged.
  - Choice list (when entering a `MakeChoice` mini-game) renders centered vertically and rows have consistent spacing identical to pre-split behavior.
  - `TextButton` height and label position unchanged.
- [ ] Spot-check: with a multi-sheet `DialogFont` (if one is constructable in the sample), confirm tall glyphs are not clipped between dialog lines (this is the `LineHeight` payoff).

## Learnings

### Architectural decisions
- **Open Decision #1 (speaker-label height: `MaxCharacterHeight`)**: applied. Hoisted to a single `var speakerHeight = font.MaxCharacterHeight;` local to dedupe seven reads of the same value across the speaker-label rectangles + text Y offset. One hoist beats seven property reads, even if `MaxCharacterHeight` is init-only — readability + intent.
- **Open Decision #2 (`Lines * font.LineHeight + 7` literal swap)**: kept the existing trailing-spacing over-count to preserve byte-for-byte single-sheet visual layout. Did not switch to `ComputeHeight`.
- **Open Decision #3 (`TextButton` ComputeWidth handles `\n`)**: trusted the helper. No multi-line button labels in scope.

### Scope held
- Public surfaces of `TextButton`, `SceneController`, `MakeChoice`/`MakeChoiceConfig`/`Choice`, and the two `GraphicsManagerExtensions` extension methods unchanged.
- No `using System.Linq` added (none of the touched files needed it; existing usage in `MakeChoice` already had it).
- Build of `BenMakesGames.PlayPlayMini.VN.csproj`: **0 errors**, 236 pre-existing CS1591 doc warnings (unrelated to this ticket).

### Interesting tidbits
- `DrawCharacterDialog` had seven literal `font.CharacterHeight` reads — same value re-read from a property each time. Hoisting to `speakerHeight` is the same shape as the UI elements ticket's `var font = UI.Font;` pattern (Open Decision #1 there). Same idiom across two tickets, same day — pattern worth noting in repo `docs/`.
- The `LineHeight` over-count for `Lines >= 1` (one trailing `VerticalSpacing` past the last line) is **intentional visual contract** — the spec preserves it. `Font.ComputeHeight` does NOT over-count, so any future migration to `ComputeHeight` here would shift dialog box height by one spacing unit. Document, don't fix.

### Rejected alternatives
- Moving the dialog-height calc to `Font.ComputeHeight(text)` — would change the over-count contract and silently shift visuals. Out of scope per Open Decision #2.
- Adding a `speakerHeight` field on `Character` or caching at the extension method level — `MaxCharacterHeight` is init-only on `Font`, no caching needed beyond the local hoist.

### Related areas affected
- None outside the four target files. The `BenMakesGames.PlayPlayMini.GraphicsExtensions` and `.Performance` projects still need their own follow-up tickets per the parent migration.
