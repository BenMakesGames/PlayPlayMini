# GraphicsManager PostProcessChain hook

## Context

**Current behavior**: `GraphicsManager.WithSceneShader(...)` (four overloads) is the only way to apply a post-process shader — a scoped `IDisposable` opened by the code doing the drawing. There is no framework-level hook to wrap *every* frame's state + service draws through a shader, so a game that wants an always-on post-process effect must either (a) open the `WithSceneShader` in every game state's `Draw`, (b) provide an abstract base class that all states inherit from, or (c) hack around the missing hook. All three push boilerplate onto the game and silently regress when a new state is added without the wrap.

**New behavior**: `GraphicsManager` exposes an ordered `PostProcessChain` — a list of `PostProcessEntry` value records (shader name + optional per-frame `Configure` callback). `GameStateManager.Draw` iterates the chain and opens nested `WithSceneShader` scopes around `CurrentState.Draw` + every `IServiceDraw.Draw`, so a game can register a post-process pipeline once at startup and every frame runs through it — including new states added later, no opt-in required. Chain order is "apply in order 0 to N-1" (index 0 runs first on the raw scene; last entry runs last, its output goes to the framebuffer). When the chain is empty, `Draw` behaves byte-identically to today (no extra render-target hop, no allocation). Steady-state allocation is zero per frame.

## Scope

### In scope

- `BenMakesGames.PlayPlayMini\Model\PostProcessEntry.cs` — new `readonly record struct` with `ShaderName` (string) and `Configure` (`Action<Effect>?`) properties, in the same namespace as the other `IAsset`-adjacent model types.
- `BenMakesGames.PlayPlayMini\Services\GraphicsManager.cs` — new public `PostProcessChain` property (backing `List<PostProcessEntry>` exposed as `IList<PostProcessEntry>`), initialized to an empty list.
- `BenMakesGames.PlayPlayMini\GameStateManager.cs` — `Draw` iterates `Graphics.PostProcessChain` and opens N nested `WithSceneShader` scopes around the existing `CurrentState.Draw` + `IServiceDraw.Draw` block. Empty chain path stays byte-identical to today.
- XML docs on `PostProcessChain` and `PostProcessEntry` explaining chain order semantics ("apply in order — index 0 runs first, last entry runs last"), the "empty list = no extra RT hop" contract, and the caller's responsibility to cache `Configure` lambdas as fields to keep per-frame allocation at zero.

### Out of scope

- Any specific shaders — the game supplies its own `.fx` files and registers them via existing `PixelShaderMeta`.
- Mutating `PostProcessChain` from inside `Draw` (add/remove during a frame's own iteration). Not supported, not defended against — same posture as any list-iterating framework code. Games should mutate the chain from Input/Update callbacks or Program.cs, not from Draw.
- Thread safety. `GraphicsManager` is single-threaded per MonoGame contract; the chain follows suit.
- Concurrent chains for split-screen / multi-camera. Not a use case today; would be a separate design.
- A fluent builder on `GameStateManagerBuilder` for pre-registering the chain. Games can populate `Graphics.PostProcessChain` post-construction (e.g., in `Startup.LoadContent` after shaders are loaded — the chain stores names, not `Effect` refs, so timing is flexible). Adding a builder now would prejudge whether registration belongs at build time vs. runtime.
- Changing the `WithSceneShader` API surface. This ticket consumes it as-is.
- Post-process on the final backbuffer blit in `GraphicsManager.EndDraw`. Unshaded point-clamp zoom-scale blit stays as-is; the chain lives *before* that blit, inside `GraphicsManager.RenderTarget`.

## Relevant Docs & Anchors

- **Sibling scope helper**: `GraphicsManager.WithSceneShader(string, Action<Effect>?)` — the string overload the new hook consumes. Read its docstring for the "nesting is supported" clause the chain semantics rely on.
- **Existing pattern for chain iteration**: `GameStateManager.Input` / `Update` / `Draw` already use `for` loops (not `foreach`) over `ServiceWatcher.InputServices` / etc., with an inline comment noting the allocation reason. The chain iteration should mirror that style verbatim.
- **Pooling precedent**: `docs\tickets\complete\2026-05-07 pool-shader-scopes.md` — the ticket that established the "zero per-frame alloc in `GameStateManager.Draw`" bar and pooled the underlying `SceneShaderScope`. This ticket sits on top of that pooling — every entry opens a pooled scope, so N-deep chains at 60+ FPS stay allocation-free.
- **Upgrade note precedent**: `BenMakesGames.PlayPlayMini\UPGRADE.md` — the 8.1→8.2 section documented the `SetPostProcessingPixelShader` → `WithSceneShader` migration. This ticket adds a *new* capability rather than replacing one; still worth adding an upgrade note explaining when a game should reach for `PostProcessChain` vs. per-state `WithSceneShader` (short version: chain for always-on / cross-state effects; `WithSceneShader` for state-specific or event-driven effects).

## Constraints & Gotchas

- **Zero allocation per frame on steady state.** The chain is a `List<PostProcessEntry>` iterated by `for` loop; `PostProcessEntry` is a `readonly record struct`; `WithSceneShader` scopes are already pooled. The only per-frame heap alloc risk is the caller's `Configure` lambda — that's the game's responsibility (cache as a field, don't `_ => _.Parameters[...]...` inline every frame). The XML doc must state this so consumers know.
- **Empty-chain path stays byte-identical to today's `Draw`.** Skip the wrap entirely (no `WithSceneShader(null)` blend-isolation layer, which would silently add a per-frame RT round-trip). Explicit `Count == 0` short-circuit.
- **Chain iteration order = apply order.** Chain `[Bloom, CRT, Vignette]` should apply Bloom first, then CRT on top, then Vignette last. Because nested `using (WithSceneShader(...))` composes innermost-shader-first at Dispose time, the recursive helper must open scopes from *last index to first*: `chain[N-1]` opens outermost, `chain[0]` opens innermost, draws land in `chain[0]`'s layer. On Dispose unwinding, `chain[0]` shader runs first (composites into `chain[1]`'s layer), and so on. Getting this direction wrong is the single most likely bug — the ticket's `DrawSceneThroughChain` prose spells it out.
- **Recursion depth == chain length.** Typically 1–3 in real games, worst case maybe 5. Comfortably within stack. Iterative alternatives (an `ArrayPool<IDisposable>` rented buffer) are noisier for no benefit at these depths.
- **Chain mutation during Draw is undefined.** No explicit guard; the game is expected to mutate the chain from Input/Update callbacks or Program.cs. Same posture as `ServiceWatcher.DrawnServices` — the framework does not defend against mid-frame list mutation.
- **`WithSceneShader(string)` throws if the shader is not loaded.** If a game populates the chain before `GraphicsManager.LoadContent` fires, or misspells a shader name, the first `Draw` throws. This is fine — same behavior as today's `WithSceneShader(string)` calls. Games should populate the chain in `Startup.LoadContent` (post-shader-load) or ensure the shader is `PreLoaded`.

## Open Decisions

1. **Backing type of `PostProcessChain`** — plain `List<PostProcessEntry>` exposed as `IList<PostProcessEntry>` vs. a custom `PostProcessChain` collection type with `Add`/`Remove`/`Insert`/`Clear`. Default: plain `List<T>` exposed as `IList<T>`. `List<T>`'s `Count` + indexer are exactly what the `for` loop needs; a custom type would add API surface without pulling weight until we know what the game wants (priority-based ordering? per-entry enable flag?). Revisit when a real game asks for it.
2. **Iteration approach in `GameStateManager.Draw`** — recursive helper vs. iterative acquire/dispose with an `ArrayPool<IDisposable>` rented buffer. Default: recursive. Chain depths of 1–3 don't stress the stack, and the recursive form reads cleaner than pool bookkeeping.
3. **Where to place `PostProcessEntry`** — same file as `GraphicsManager.cs`, alongside the existing `PixelShaderMeta`/`FontMeta`/etc. in the `Model` namespace, or its own file. Default: its own file in `BenMakesGames.PlayPlayMini\Model\` matching the sibling asset records' file-per-record convention.

## Acceptance Criteria

- [ ] `BenMakesGames.PlayPlayMini\Model\PostProcessEntry.cs` exists and declares `public readonly record struct PostProcessEntry(string ShaderName, Action<Effect>? Configure = null)`.
- [ ] `GraphicsManager` exposes a public `PostProcessChain` property of declared type `IList<PostProcessEntry>`, backed by a `new List<PostProcessEntry>()` initialized inline (never null).
- [ ] With `PostProcessChain.Count == 0`, `GameStateManager.Draw` behaves byte-identically to today (no additional `WithSceneShader` wrap, no additional render-target hop, no additional allocation). Verifiable by code inspection — an explicit `Count == 0` short-circuit branch calls the same three-line block as today.
- [ ] With `PostProcessChain.Count > 0`, `GameStateManager.Draw` opens `PostProcessChain.Count` nested `WithSceneShader(entry.ShaderName, entry.Configure)` scopes around the existing `CurrentState.Draw(gameTime)` + `IServiceDraw.Draw(gameTime)` block, in the order such that `PostProcessChain[0]`'s shader runs first at Dispose time and `PostProcessChain[Count-1]`'s shader runs last (its output composites to the previous target).
- [ ] Chain iteration in `GameStateManager.Draw` uses a `for` loop (or the recursive helper it drives), never `foreach`, and includes the same `// for loop, instead of foreach, reduces allocations` comment (or an equivalent one for the recursive helper) used elsewhere in `GameStateManager`.
- [ ] XML doc on `PostProcessChain` states: (a) chain order = apply order (index 0 first, last entry last); (b) empty chain has no per-frame cost; (c) `Configure` lambdas should be cached as fields by the caller to keep per-frame allocation at zero.
- [ ] XML doc on `PostProcessEntry` mirrors the same order-and-caching notes in one sentence and points at `PostProcessChain` for the longer explanation.
- [ ] `UPGRADE.md` gains a short section on when to reach for `PostProcessChain` (always-on / cross-state) vs. `WithSceneShader` (state-specific / event-driven).
- [ ] All existing PPM tests pass. New unit test coverage (in whichever test project already covers `GameStateManager.Draw` or `GraphicsManager`): empty-chain no-op path, single-entry wrap, multi-entry ordering. Tests can assert on side-effects observable through a fake `IServiceDraw` or through instrumenting a test shader that records invocation order.
- [ ] `dotnet build` is clean across the solution — no new warnings, no errors.

## Implementation

### 1. Re-read `WithSceneShader` and the current `GameStateManager.Draw`

Open `GraphicsManager.cs` — read all four `WithSceneShader` overloads and the `SceneShaderScope` class (roughly the second half of the file). Confirm the "inner scope composites into outer layer, outer composites into framebuffer" semantics documented on the string overload. Open `GameStateManager.cs` — read the current `Draw` body (the `using (Graphics.WithShader((Effect?)null)) { base.Draw; CurrentState.Draw; for (…) DrawnServices[i].Draw; }` block) and the `for`-loop precedent + comment already in `Input` / `Update`. This ticket's iteration matches that comment style verbatim.

### 2. Introduce `PostProcessEntry`

Create `BenMakesGames.PlayPlayMini\Model\PostProcessEntry.cs`. Declare a `public readonly record struct PostProcessEntry(string ShaderName, Action<Effect>? Configure = null)` in the `BenMakesGames.PlayPlayMini.Model` namespace (matching sibling asset records' namespace). XML doc: one short paragraph stating this is one entry in a `GraphicsManager.PostProcessChain`, name is looked up in `GraphicsManager.PixelShaders`, and `Configure` should be a cached delegate field (not a per-frame lambda) to keep allocation at zero on the hot path.

### 3. Add `PostProcessChain` to `GraphicsManager`

Adjacent to other public properties near the top of `GraphicsManager`. `public IList<PostProcessEntry> PostProcessChain { get; } = new List<PostProcessEntry>();` — declared as `IList<T>` (games only need `Add`/`Remove`/`Clear`/indexer) but backed by `List<T>` so iteration is `for (i = 0; i < .Count; i++)` with no enumerator boxing. XML doc mirrors the caching-and-order guidance from step 2; the doc on `PostProcessEntry` points here for the longer explanation.

### 4. Rewire `GameStateManager.Draw` to iterate the chain

Two-branch structure. Extract the current inner block (`base.Draw; CurrentState.Draw; for(…) DrawnServices[i].Draw;`) into a small private method `DrawSceneCore(GameTime gameTime)` — pure refactor, no behavior change on its own.

Add a private recursive helper on `GameStateManager`:

```csharp
private void DrawSceneThroughChain(GameTime gameTime, int index)
{
    if (index < 0)
    {
        DrawSceneCore(gameTime);
        return;
    }

    var entry = Graphics.PostProcessChain[index];
    using (Graphics.WithSceneShader(entry.ShaderName, entry.Configure))
        DrawSceneThroughChain(gameTime, index - 1);
}
```

Draw becomes:

```csharp
protected override void Draw(GameTime gameTime)
{
    Graphics.BeginDraw();

    using (Graphics.WithShader((Effect?)null))
    {
        var chain = Graphics.PostProcessChain;
        if (chain.Count == 0)
            DrawSceneCore(gameTime);
        else
            DrawSceneThroughChain(gameTime, chain.Count - 1);
    }

    Graphics.EndDraw();
}
```

The `chain.Count == 0` short-circuit is the "byte-identical to today" path. The `chain.Count - 1` starting index is the ordering fix — opening outermost-to-innermost from last index down means `chain[0]`'s scope is innermost, so `chain[0]`'s shader runs first on Dispose unwinding. Verify by walking a `[A, B, C]` example against the ticket's Constraints & Gotchas "chain iteration order" note.

Do not iterate the chain with `foreach`. Add a one-line comment above the recursive helper (or the `chain.Count - 1` call site) mirroring `Input`/`Update`'s existing "reduces allocations" comment.

### 5. Add tests

In the same test project that already covers `GameStateManager.Draw` (or, failing that, the project that covers `GraphicsManager`), add three tests:

- Empty-chain path: with `PostProcessChain.Count == 0`, verify no `WithSceneShader` scope is opened. Simplest assertion: use a fake `IServiceDraw` whose `Draw` records the currently-bound render target; confirm it's `Graphics.RenderTarget` (the framebuffer-sized main RT), not a pool layer.
- Single-entry wrap: with one entry `PostProcessChain.Add(new("TestShader"))`, verify the fake service draws into a pool layer RT (not the main RT), and the `TestShader` effect is invoked at Dispose time. Use a test-only shader that increments a counter in its `configure` callback, or a fake `Effect` if the framework allows.
- Multi-entry ordering: with three entries, verify `configure` callbacks fire in reverse index order at Dispose time (`chain[0].Configure` runs first — i.e., its scope is innermost — because that's the shader whose composite hits `chain[1]`'s layer first). This is the acceptance criterion "chain order = apply order (index 0 first)" made testable.

If none of the existing test projects cover `GameStateManager.Draw` (this is possible — a lot of the draw path is untestable without a GraphicsDevice), skip the tests and note it in the PR body; the `.Count == 0` short-circuit and recursion direction are simple enough to review by inspection.

### 6. Update `UPGRADE.md`

Append a short section (three or four paragraphs) titled roughly "8.x → 8.y — PostProcessChain for always-on post-process effects":

- One paragraph on the new API surface (`PostProcessChain` + `PostProcessEntry`).
- One paragraph on when to reach for it: always-on effects that should apply to every game state uniformly (bloom, CRT filter, colorblind simulation, screen-shake-across-transitions). Contrast with `WithSceneShader`: state-specific or event-driven effects (line-clear ripples, per-boss visual effects) still open their own scope inside the state's `Draw`.
- One paragraph on allocation: chain iteration and `WithSceneShader` scopes are alloc-free; the game's `Configure` lambda is the only source of per-frame alloc and should be cached as a field.
- One short code snippet showing chain registration in the game's startup.

### 7. Self-check

Re-read `GameStateManager.Draw`, `DrawSceneCore`, `DrawSceneThroughChain`, and the `PostProcessChain` initialization. Verify:

- Empty chain: exactly the same code path as today (no allocation, no extra RT).
- Non-empty chain: no `foreach`, no closures over `gameTime` (it's passed as a param), no LINQ, no per-entry allocation.
- Recursion depth exactly equals `chain.Count` for any non-empty chain.
- `chain[0]`'s shader runs first at Dispose time by walking through with a two-entry chain on paper.

## Test Plan

- [ ] `dotnet build` succeeds with no new warnings or errors.
- [ ] `dotnet test` passes for every test project in the solution (Tests + any others).
- [ ] Manual: launch a PlayPlayMini sample game with `PostProcessChain` unpopulated. Confirm rendering is visually identical to a build of the same commit with the ticket's changes reverted. Use a screenshot diff if unsure.
- [ ] Manual: in a sample game, populate `Graphics.PostProcessChain` in `LoadContent` with a single throwaway shader (e.g., a tint-red pixel shader). Confirm the whole scene renders red-tinted every frame, including menu overlays and any `IServiceDraw` output.
- [ ] Manual: populate the chain with two entries — a tint-red shader and a tint-blue shader in that order. Confirm the tint-blue shader wins (it's the last entry, composites last, its output goes to the framebuffer). Swap the order — tint-red now wins. Confirms chain order = apply order.
- [ ] Manual: with the chain populated, open a per-state `WithSceneShader` inside the sample game state's `Draw`. Confirm both effects compose correctly (state-local shader applies first — innermost — then the chain wraps around it).
- [ ] Manual: under Rider's allocation profiler, run a chain-populated sample game for 30+ seconds after warm-up. Confirm `PostProcessEntry` and `SceneShaderScope` instance counts are stable (no growth) and per-frame allocations from `GameStateManager.Draw` are zero.

## Learnings

### Architectural decisions

- **All three Open Decisions resolved to their listed defaults.** (1) `PostProcessChain` is a plain `List<PostProcessEntry>` exposed as `IList<PostProcessEntry>` — `Count` + indexer are exactly what the walk needs, and a custom collection type would add API surface before we know whether games want priority ordering or per-entry enable flags. (2) The walk is the recursive `DrawSceneThroughChain(gameTime, index)`; at realistic depths (1–3) the recursion is cheaper to read than `ArrayPool<IDisposable>` bookkeeping and equally allocation-free. (3) `PostProcessEntry` lives in its own file under `Model\`, matching the file-per-record convention of `PixelShaderMeta`, `FontMeta`, and friends.
- **Method names follow the ticket verbatim** — `DrawSceneCore` (the extracted, unchanged three-part draw block) and `DrawSceneThroughChain` (the recursive scope-opener). The extraction is a pure refactor; the `base.Draw` / `CurrentState.Draw` / `DrawnServices` loop moved wholesale, comment included.
- **The "reduces allocations" note moved with the block it annotates.** The `// ReSharper disable once ForCanBeConvertedToForeach` + `// for loop, instead of foreach, reduces allocations` pair travelled into `DrawSceneCore` alongside the `DrawnServices` loop. The chain walk got its own equivalent note in `DrawSceneThroughChain`'s `<remarks>`, explaining why recursion (not `foreach`, not a rented buffer) was chosen.

### Interesting tidbits

- **The ordering argument, walked on paper for `[A, B, C]`:** `Draw` calls `DrawSceneThroughChain(gt, 2)`, so C's scope opens outermost, then B, then A innermost; `DrawSceneCore` draws into A's layer RT. Unwinding, A disposes first and composites A's layer through shader A into `PreviousRenderTarget`, which is B's layer; B composites into C's layer; C composites into `Graphics.RenderTarget`. Net: A applies first, C applies last, C's output reaches the framebuffer. Chain order == apply order, as specified.
- **The outer `WithShader((Effect?)null)` doesn't interfere with the chain's target math.** `WithShader` produces a `ShaderScope`, not a `SceneShaderScope`, so it never touches `GraphicsManager.CurrentLayerScope`. The outermost chain entry therefore still sees `CurrentLayerScope == null` and resolves `PreviousRenderTarget` to `Graphics.RenderTarget` — exactly as it would with no wrapper at all.
- **A null `Configure` needs no guard at the call site.** `SceneShaderScope.Dispose` already checks `Shader is not null && ShaderConfigureAction is not null` before invoking, so `new PostProcessEntry("Bloom")` (no configure delegate) just works.
- **`sed` in this repo's Git Bash strips `\r` on read**, so `sed -n 'N,Mp' file | od -c` misleadingly reports LF-only endings on a CRLF file. Use `tail -n +N | head -n M | od -c` (or `head -c`) to inspect real line endings. All source files here are CRLF, and most `Model\*.cs` files carry a UTF-8 BOM; `PostProcessEntry.cs` was written to match.
- **Warning baseline is 533, not zero.** The solution builds with 533 pre-existing `CS1591`/`CS1570` warnings (mostly in the VN project). "No new warnings" was verified by grepping the build output for the touched files and the new one — no new entries appeared, and every `cref` in the new XML docs resolved (no `CS1574`).

### Workarounds / limitations

- **No unit tests were added.** The ticket's Implementation step 5 anticipated this: the test project (`BenMakesGames.PlayPlayMini.Tests`, xunit + Shouldly, one `WordWrapTests` file) has no coverage of the draw path, and can't get any — `GameStateManager` derives from MonoGame's `Game`, whose construction needs a real window and `GraphicsDevice`, and the three interesting assertions (empty-chain no-op, single-entry wrap, multi-entry ordering) all require a live draw. The remaining testable surface (chain is non-null and empty on construction) is too close to testing the compiler to be worth the maintenance. The `Count == 0` short-circuit and the recursion direction were verified by inspection instead, with the `[A, B, C]` walk above.
- The Test Plan's Manual items — sample-game visual checks (unpopulated chain identical to before, single tint shader, two-shader ordering, composition with a per-state `WithSceneShader`) and the Rider allocation-profiler run — need an interactive launch and were **not** exercised in this pass. Same posture as the pooling ticket that preceded this one; they need user verification.

### Rejected alternatives

- **Wrapping the chain in an always-on `WithSceneShader(null)` blend-isolation layer** to unify the two branches — rejected, and explicitly so: it would add a per-frame render-target round-trip to every game that never touches the chain. The `Count == 0` short-circuit exists precisely to avoid that.
- **An `ArrayPool<IDisposable>` rented buffer with an iterative acquire/dispose loop** — equivalent allocation behavior, noticeably more bookkeeping (and a `try`/`finally` to keep dispose correct on throw) at chain depths that never stress the stack.
- **A fluent builder on `GameStateManagerBuilder` for pre-registering the chain** — out of scope per the ticket. Because entries store shader *names* rather than `Effect` references, registration timing is already flexible; adding a builder now would prejudge build-time vs. runtime registration.
- **Guarding against mid-`Draw` chain mutation** — deliberately not done, matching `ServiceWatcher.DrawnServices`, which the framework also iterates without defending against mutation.

### Related areas affected

- `UPGRADE.md` gained a new top section, "Upgrading from 8.3.x to 8.4.0" (the previous top was 8.1.x → 8.2.0; `v8.3.0` shipped with no upgrade notes). It covers the API, when to prefer the chain over `WithSceneShader`, the `Configure`-caching rule, and a registration snippet.
- No `docs\` reference file was updated — `docs\` currently contains only `tickets\` and `tickets\complete\`, with no reference-doc tree to distil into.
- `SceneShaderScope` and the scope/render-target pools are consumed unchanged; this ticket adds no new pooling and rides entirely on the pooling established by `2026-05-07 pool-shader-scopes.md`.

### Follow-up bug fix (rc7 → rc8)

- **Nested bounded `WithSceneShader` inside a full-scene one wiped the outer layer.** Surfaced by Astromino's `bloom-post-process` playtest: a `ButtonFrame.DrawPlasmaHighlight` (which opens `WithSceneShader("SubtractivePlasma", bounds, ...)`) inside a `PostProcessChain`-wrapped Draw dropped every draw call issued *before* the plasma scope. Cause: `AcquireLayerRenderTarget` allocated pool RTs with the default `RenderTargetUsage.DiscardContents`. The framebuffer RT already uses `PreserveContents` for exactly this reason; layer RTs needed the same. Fix: `AcquireLayerRenderTarget` now allocates with `usage: RenderTargetUsage.PreserveContents` (matching the framebuffer allocation). Shipped in 8.4.0-rc8. Latent since bounded `WithSceneShader` landed — nothing exercised the nested pattern until PostProcessChain made it easy to hit.
