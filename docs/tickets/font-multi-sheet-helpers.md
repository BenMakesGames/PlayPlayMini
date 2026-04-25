# Font multi-sheet foundation helpers

## Context
**Current behavior**: `Font` (at `BenMakesGames.PlayPlayMini\Model\Font.cs`) is a `sealed record Font(List<FontSheet> Sheets)` whose only multi-sheet logic is an inlined `foreach (var sheet in Sheets)` lookup inside `ComputeWidth`. Higher-level draw/measure code (`GraphicsManager.Text.cs`, `WrapText`, span overloads, outline text) still operates on a single `FontSheet` only — there is no shared "which sheet draws this char?" primitive, and no concept of a multi-sheet line height.

**New behavior**: `Font` exposes the foundation helpers all multi-sheet draw paths will route through: `TryGetSheet(char, out FontSheet?)` for per-character sheet lookup, an init-only `LineHeight` property (max of `CharacterHeight + VerticalSpacing` across sheets) computed once at construction, and `ComputeHeight(string)` mirroring the existing `ComputeWidth` line-counting and trailing-spacing convention. `ComputeWidth` is refactored to call `TryGetSheet` instead of inlining the lookup. Behavior of `ComputeWidth` is unchanged.

## Scope
### In scope
Additive changes to the single file `BenMakesGames.PlayPlayMini\Model\Font.cs`. New public surface: `TryGetSheet`, `LineHeight`, `ComputeHeight`. Internal refactor of `ComputeWidth` to delegate to `TryGetSheet`.

### Out of scope
- Migrating any callers (`GraphicsManager.Text.cs`, `WrapText`, span overloads, outline text, `MeasureText`, etc.) to use the new helpers. That belongs to follow-up tickets.
- Updating tests in `BenMakesGames.PlayPlayMini.Tests\WordWrapTests.cs` that still call the old single-sheet `Font` ctor — those are now constructing `FontSheet` directly anyway, which is fine, but any leftover stale `new Font(null!, 1, 1, 0, 0, ' ')` shapes are not blocking and are addressed in a future ticket.
- No new `FontSheet` API.

## Relevant Docs & Anchors
- **Code anchors**:
  - `BenMakesGames.PlayPlayMini\Model\Font.cs` — current state; `ComputeWidth` is the style + trailing-spacing reference.
  - `BenMakesGames.PlayPlayMini\Model\FontSheet.cs` — `LastCharacter = (char)(FirstCharacter + Columns * Rows)` is **one past** the last drawable char. Inclusion test must be `c >= FirstCharacter && c < LastCharacter`. Mirror what `ComputeWidth` already does.
  - `BenMakesGames.PlayPlayMini\Services\GraphicsManager.Text.cs` — existing `\r`/`\n` handling: `\r` is ignored (skipped/`continue`); `\n` advances Y by `CharacterHeight + VerticalSpacing`; in some draw paths `\r` and `\n` are both treated as line breaks (`is '\r' or '\n'`). The convention to mirror for `ComputeHeight` is **`ComputeWidth`'s convention** (skip `\r`, line break only on `\n`) since we are extending the same record's API surface.
- **Trailing-spacing convention**: `ComputeWidth` subtracts the last `HorizontalSpacing` per line so the rightmost edge is the glyph edge, not the inter-glyph gap. `ComputeHeight` mirrors this by subtracting one `VerticalSpacing` (the spacing of the sheet that drew the last line — see Open Decisions for the simplification).
- **User-feedback memory**: `feedback_docblocks_for_consumers.md` — public API gets `/// <summary>` XML doc comments, not `//`. The doc website is generated from these.

## Constraints & Gotchas
- **Hot path / zero allocations.** Many consumers will call draw/measure paths at 60fps; everything added here is reachable from those paths transitively. Direct `foreach` only — no LINQ (`Max()`/`Any()`/etc. allocate enumerators on `List<T>` in the general case and force delegate captures for selectors), no `Func<>`/`Action<>`, no `Lazy<T>` with a closure, no `params` arrays, no string allocations.
- **`FontSheet` is a sealed record class** — `out FontSheet?` is a reference out-param; no boxing concern.
- **Init-only computation in record body.** Records support the `public int LineHeight { get; } = ComputeLineHeight(Sheets);` pattern with a `private static` helper. Compute once; never recompute per call.
- **Public API → XML doc comments** (`/// <summary>`/`/// <param>`/`/// <returns>`), per project convention. The doc website is generated from these. No bare `//` on consumer-facing members.
- **No backwards-compat shims.** Plain additive change to `Font`.
- **`Sheets` is a `List<FontSheet>`** — iterating with `foreach` over `List<T>` uses the struct enumerator and does not allocate. Keep it that way.

## Open Decisions
1. **Trailing `VerticalSpacing` source for `ComputeHeight`.** Spec calls for "`lineCount * LineHeight - lastVerticalSpacing` style result, mirroring `ComputeWidth`." `LineHeight` already bakes in the largest `CharacterHeight + VerticalSpacing` across sheets, so the natural simplification is: total height = `lineCount * LineHeight - maxVerticalSpacing`, where `maxVerticalSpacing` is the `VerticalSpacing` of the sheet contributing to `LineHeight`. Default: pre-compute and store the `VerticalSpacing` of the tallest-line sheet alongside `LineHeight` (one extra init-only int) and subtract it once at the end. This keeps `ComputeHeight` allocation-free and avoids per-call sheet lookup. Implementer may instead subtract a constant derived differently if equivalent and clearer.
2. **Empty-text behavior of `ComputeHeight`.** `ComputeWidth` returns `0` for empty input. Default: `ComputeHeight("")` returns `0` as well (no lines, nothing to draw). Non-empty input with no `\n` is one line; height = `LineHeight - trailingVerticalSpacing`.
3. **Lone `\r` in `ComputeHeight`.** Mirror `ComputeWidth`: skip `\r`, treat only `\n` as a line break (do not also count `\r` as a break, do not collapse `\r\n` into two). `\r\n` ends up incrementing the line count once, via the `\n`.
4. **Helper method naming.** `ComputeLineHeight` for the private static initializer is a reasonable default; implementer may pick something else if it reads better alongside neighboring code.

## Acceptance Criteria
- [ ] `Font.TryGetSheet(char c, out FontSheet? sheet)` exists as a public instance method, returns `bool`. Returns `true` and sets `sheet` to the first `FontSheet` in `Sheets` where `c >= sheet.FirstCharacter && c < sheet.LastCharacter`; otherwise returns `false` and sets `sheet` to `null`. Implementation uses a direct `foreach` over `Sheets` — no LINQ, no allocations, no delegates.
- [ ] `Font.LineHeight` exists as a public init-only `int` property, initialized via the record body initializer pattern (e.g., `public int LineHeight { get; } = ComputeLineHeight(Sheets);`) so it is computed once at construction. Value equals the maximum of `sheet.CharacterHeight + sheet.VerticalSpacing` across all entries in `Sheets`. Initializer uses `foreach`, not LINQ `Max()`.
- [ ] `Font.ComputeHeight(string text)` exists as a public instance method returning `int`. Returns `0` for empty input. Otherwise returns `lineCount * LineHeight - <trailingVerticalSpacing>` where `lineCount` is `1 + (count of '\n' chars in text)`, `\r` is skipped, and `<trailingVerticalSpacing>` matches the trailing-spacing convention of `ComputeWidth` (one `VerticalSpacing` worth, see Open Decisions #1). No allocations on the hot path.
- [ ] `Font.ComputeWidth` no longer contains the inlined `foreach (var sheet in Sheets)` lookup; instead it calls `TryGetSheet` and uses the out-param sheet to update `lineWidth` / `lastSpacing`. The control-flow for `\r`, `\n`, end-of-string finalization, and the trailing-spacing subtraction is unchanged. Existing `WordWrapTests` still pass without modification.
- [ ] All four members (`TryGetSheet`, `LineHeight`, `ComputeHeight`, refactored `ComputeWidth`) carry `/// <summary>` XML doc comments — including `<param>` and `<returns>` where appropriate — describing intent for consumers of the public NuGet API.
- [ ] No new `using` for `System.Linq` is added to the file.

## Implementation
### 1. Add `TryGetSheet`
Single source of truth for "which sheet draws this char?" — every higher-level multi-sheet draw/measure path will call into it. In `Font.cs`, add a public instance method `TryGetSheet(char c, out FontSheet? sheet)` returning `bool`. Use a direct `foreach` over `Sheets`, mirroring the inclusion test currently inlined in `ComputeWidth` (`c >= sheet.FirstCharacter && c < sheet.LastCharacter`). On match: assign to `sheet`, return `true`. After loop: assign `null`, return `false`. Add XML doc comments explaining intent and the "first match wins" / "ordering matters" semantics so consumers configuring multi-sheet fonts (e.g., Latin + CJK) understand sheet order in `Sheets`.

### 2. Add `LineHeight` (and trailing-spacing companion if needed)
Tallest-glyph sheet must drive `\n` advance so shorter-glyph sheets don't clip taller ones. Add a public init-only `int LineHeight` property to `Font`'s record body, initialized via a private `static int ComputeLineHeight(List<FontSheet> sheets)` helper that `foreach`-scans for the maximum `CharacterHeight + VerticalSpacing`. Per Open Decisions #1, also pre-compute and store the `VerticalSpacing` contribution of that same tallest sheet (e.g., a parallel `private int TrailingVerticalSpacing { get; }` or fold into a single helper that returns both). Choose whichever shape reads cleanly; both are init-only, both computed once, both `foreach`-based. Empty-`Sheets` case: `LineHeight` and any trailing-spacing companion both `0` (the `Font(FontSheet)` convenience ctor and any caller-supplied list always have at least one sheet in practice, but defend against empty for safety with no allocation).

### 3. Add `ComputeHeight(string text)`
Mirror `ComputeWidth`'s shape: `for` loop over `text`, increment a line counter on `\n`, skip `\r`, ignore everything else (line height does not depend on which characters are on each line — it is `LineHeight` regardless). Empty input returns `0`. Otherwise return `lineCount * LineHeight - trailingVerticalSpacing` (per Open Decisions #1). XML doc comments mirroring the style of the existing `ComputeWidth` doc.

### 4. Refactor `ComputeWidth` to use `TryGetSheet`
Replace the inlined `foreach (var sheet in Sheets) { if (c >= sheet.FirstCharacter && c < sheet.LastCharacter) { ... break; } }` block with a `if (TryGetSheet(c, out var sheet)) { lineWidth += sheet!.CharacterWidth + sheet.HorizontalSpacing; lastSpacing = sheet.HorizontalSpacing; }`. Preserve the existing `\r` skip, `\n` line-finalization, end-of-string finalization, and trailing-spacing subtraction unchanged. Existing XML doc on `ComputeWidth` stays (light wording polish is fine but not required).

## Test Plan
- [ ] `dotnet build` from repo root (or solution file) — clean build, no new warnings.
- [ ] `dotnet test` — `BenMakesGames.PlayPlayMini.Tests` passes. Note: `WordWrapTests.cs` constructs `FontSheet` directly via `new FontSheet(null!, 1, 1, 0, 0, ' ')` — it does not exercise `Font` and so should be unaffected. If pre-existing test failures unrelated to this change appear (e.g., a stale `new Font(null!, 1, 1, 0, 0, ' ')` shape elsewhere), flag in the implementation note as not blocking — a future ticket will address.
- [ ] Manual eyeball: open `Font.cs` and confirm `using System.Linq;` is **not** present, and that the only loops introduced are `foreach`/`for` over `Sheets` or `text` directly — no `.Max(...)`, `.Any(...)`, lambda expressions, or `Func<>`/`Action<>`.
- [ ] Manual eyeball: each new public member has a `/// <summary>` block; `TryGetSheet` documents `<param>` and `<returns>`; `ComputeHeight` documents `<param>` and `<returns>` like `ComputeWidth`.
