# Pool ShaderScope and SceneShaderScope; remove allocating foreach in Draw

## Context

**Current behavior**: `GraphicsManager.WithShader` and `GraphicsManager.WithSceneShader` each `new` a `ShaderScope` or `SceneShaderScope` (both reference types) every call. `GameStateManager.Draw` itself allocates one `ShaderScope` per frame via its top-level `using (Graphics.WithShader((Effect?)null))`, and any user `Draw` body that uses these helpers adds more. Rider's profiler flags `GameStateManager.Draw` and `SceneShaderScope.Dispose` as the top-two hotspots in a running PlayPlayMini game — the allocation traffic is showing up at 60fps. Separately, `GameStateManager.Draw` iterates `ServiceWatcher.DrawnServices` (an `IReadOnlyList<IServiceDraw>`) with `foreach`, which boxes an `IEnumerator<T>` every frame; the sibling `Input` / `Update` methods already use `for` loops with comments noting the allocation reason.

**New behavior**: `ShaderScope` and `SceneShaderScope` are pooled per-`GraphicsManager`. `WithShader` / `WithSceneShader` pop a reusable instance from a stack (or `new` one if the pool is empty) and call an `Initialize(...)` method to set per-use state. `Dispose` clears the per-use refs (so pooled instances do not pin shaders or captured-lambda closures), returns the instance to the pool, and otherwise behaves identically to the current Dispose. After warm-up the steady-state allocation count for both scope types is zero. Public API is unchanged: both methods still return `IDisposable`. `GameStateManager.Draw`'s `foreach` over `DrawnServices` is converted to a `for` loop matching the existing `Input` / `Update` style, with the same `// for loop, instead of foreach, reduces allocations` comment.

## Scope

### In scope

- `BenMakesGames.PlayPlayMini\Services\GraphicsManager.cs` — pool fields, factory methods (`WithShader`, `WithSceneShader`), and the two scope classes (`ShaderScope`, `SceneShaderScope`).
- `BenMakesGames.PlayPlayMini\GameStateManager.cs` — convert `foreach` over `DrawnServices` in `Draw` to a `for` loop.
- Inline XML doc updates only where pooling changes observable behavior (e.g., a one-line caveat that the `IDisposable` instance must not outlive its `using` block — already implied, but worth stating now that lifetime matters).

### Out of scope

- Converting scopes to `ref struct` or otherwise changing the public API surface (return type stays `IDisposable`). Considered and rejected: `CurrentBatchScope` / `CurrentLayerScope` are class fields; making the scopes ref structs would force a separate parent-tracking redesign for marginal gain over pooling.
- Reducing allocations inside the user's `configure: e => e.Parameters[...].SetValue(myVar)` lambda. That closure is allocated by the caller and pooling cannot fix it. Capture-free patterns (static lambdas, pre-cached delegates) are a separate documentation follow-up.
- Anything in `GraphicsManager.EndDraw` (Rider's #3). The body has no managed allocations; the cost there is MonoGame `SpriteBatch.Begin/End/Draw` internals and the unavoidable composite-to-screen.
- Pool size limits, eviction, or `IDisposable.Dispose` on the pool itself. The scopes hold no unmanaged resources — the `RenderTarget2D` they use already has its own pool with a `Dispose` path in `GraphicsManager.UnloadContent`.

## Relevant Docs & Anchors

- **Primary file**: `BenMakesGames.PlayPlayMini\Services\GraphicsManager.cs` — read the `IBatchScope` interface, both scope classes (`ShaderScope`, `SceneShaderScope`), and the existing `_layerRenderTargetPool` (`Stack<RenderTarget2D>`) for the pooling style to mirror, including `AcquireLayerRenderTarget` / `ReleaseLayerRenderTarget` and the `UnloadContent` drain loop.
- **Caller**: `BenMakesGames.PlayPlayMini\GameStateManager.cs` — `Draw` (line ~160) for the `using` block + the `foreach` to convert; `Input` / `Update` for the `for`-loop pattern + comment to mirror.
- **Sibling pool**: `_layerRenderTargetPool` in the same file is the existing precedent — same `Stack<T>` shape, same internal `Acquire`/`Release` method pair. Match its naming convention (`_shaderScopePool`, `AcquireShaderScope`, `ReleaseShaderScope`, etc.) and visibility (private field, internal methods).

## Constraints & Gotchas

- **No thread safety required.** `GraphicsManager` is used from MonoGame's `Draw` callback, which is single-threaded. The existing `_layerRenderTargetPool` is also a plain `Stack<>` with no locking — match that.
- **Clear per-use refs in Dispose before returning to the pool.** A pooled `ShaderScope` that still holds a reference to the user's `Effect` or captured-lambda `Action<Effect>` will pin those objects in GC until the scope is reused. Set `Shader`, `ShaderConfigureAction`, `PreviousScope` (and for `SceneShaderScope`, also `PreviousLayerScope`, `PreviousRenderTarget`, `LayerRenderTarget`) back to `null`/`default` as part of Dispose, before pushing onto the pool.
- **Lifetime contract: dispose-once, in-scope.** Pooling makes double-dispose actively dangerous (the same instance would land in the pool twice and could be handed to two concurrent `using` blocks). The compiler-emitted `using` pattern guarantees single-dispose; just don't introduce manual `IDisposable` capture-and-reuse anywhere. No code change needed — but worth a one-line XML doc remark on `WithShader` / `WithSceneShader` so consumers don't capture the return value and dispose it twice.
- **The pool must survive `UnloadContent`.** The existing `UnloadContent` drains `_layerRenderTargetPool` because each `RenderTarget2D` is unmanaged. Scope objects are pure managed; they need no drain. Do not add one (it would just be noise).
- **Field mutability.** Current scope fields are `readonly`. Pooling requires them to be assigned in `Initialize`, so they become non-`readonly` private fields. That is fine (these are `internal sealed class`es) — the `readonly` was incidental, not load-bearing.

## Open Decisions

1. **Pool storage type** — `Stack<T>` (LIFO, matches `_layerRenderTargetPool`) vs. `ConcurrentStack<T>` vs. a plain singleton field. Default: `Stack<T>`, matching the existing pool. Single-threaded use means no concurrent variant; keeping a singleton "spare" instead of a stack would break under nested `WithShader` calls (which do happen — `GameStateManager.Draw`'s outer `WithShader` plus any inner one in a state's `Draw`).
2. **Pool initial capacity** — pre-allocate at construction vs. lazy. Default: lazy (`new Stack<>()`, no pre-fill). Warm-up cost is one allocation per nesting level reached, paid on the first few frames; not worth the constructor complexity to pre-fill.
3. **Refactor the constructor + `Initialize` shape** — could keep one ctor that takes graphics-only and call `Initialize` from inside `WithShader`, or fold `Initialize` into `Acquire`. Default: separate `Initialize` method on the scope, called from `WithShader` after `Acquire`. Cleaner blame for the per-use state than burying it in a private `Acquire` helper, and mirrors how `_layerRenderTargetPool` already separates `Acquire` (pool concern) from "what to do with the thing once acquired" (caller concern).

## Acceptance Criteria

- [ ] `GraphicsManager` holds two private fields, one per scope type, of `Stack<ShaderScope>` and `Stack<SceneShaderScope>` (or equivalent — see Open Decisions).
- [ ] `WithShader(Effect?, Action<Effect>?)` and `WithShader(string, Action<Effect>?)` no longer contain a `new ShaderScope(...)` expression on the steady-state path; they pop from the pool and `new` only when the pool is empty.
- [ ] `WithSceneShader(Effect?, Action<Effect>?)` and `WithSceneShader(string, Action<Effect>?)` likewise no longer `new` a `SceneShaderScope` on the steady-state path.
- [ ] Both scope classes expose a single constructor that takes only the `GraphicsManager` (no per-use state). Per-use state (`Shader`, `ShaderConfigureAction`, `PreviousScope`, plus `SceneShaderScope`'s `PreviousLayerScope` / `PreviousRenderTarget` / `LayerRenderTarget`) is set by an `Initialize(...)` method called immediately after acquiring the instance from the pool.
- [ ] Both scope classes' `Dispose` methods set every per-use field back to `null`/`default` and push the instance onto its pool — in that order, after the existing batch-end / render-target-restore / parent-scope-restore work completes.
- [ ] `Initialize` performs the same setup work the current constructors do (capture previous scope/layer/render-target, switch render target for `SceneShaderScope`, end previous batch if needed, call `BeginBatch`).
- [ ] `GameStateManager.Draw` iterates `ServiceWatcher.DrawnServices` with a `for` loop and the same `// for loop, instead of foreach, reduces allocations` comment used in `Input` / `Update`.
- [ ] `WithShader` and `WithSceneShader` XML doc comments include a one-line remark that the returned `IDisposable` must be disposed exactly once (e.g., via `using`) and not stored beyond its scope.
- [ ] Public method signatures of `WithShader` / `WithSceneShader` (and the string-name overloads) are unchanged: same parameter types, same return type `IDisposable`.
- [ ] `dotnet build` is clean (no new warnings, no errors) and `dotnet test` passes.

## Implementation

### 1. Re-read the existing scope code

Open `GraphicsManager.cs`. Read `IBatchScope`, `ShaderScope`, `SceneShaderScope`, and the existing `_layerRenderTargetPool` block (`AcquireLayerRenderTarget` / `ReleaseLayerRenderTarget` / the `UnloadContent` drain). The scope-pool work mirrors the render-target pool's shape exactly — same `Stack<T>`, same internal `Acquire`/`Release` pair, same private-field visibility. The only difference: scope objects are managed-only, so no `UnloadContent` drain.

### 2. Add scope pools to `GraphicsManager`

Place adjacent to `_layerRenderTargetPool`. Two private readonly fields, one per scope type, each a `Stack<>` of the concrete scope class. Add internal `Acquire{ShaderScope|SceneShaderScope}()` methods that pop-or-`new` (passing `this` to the constructor), and internal `Release{ShaderScope|SceneShaderScope}(scope)` methods that push.

### 3. Convert `ShaderScope` constructor → ctor + Initialize

Reduce the constructor to `(GraphicsManager graphics)` — store the back-reference only. Move the rest of the existing constructor body (capture `PreviousScope`, end the previous batch, call `BeginBatch`) into a new `Initialize(Effect? shader, Action<Effect>? configure)` method. Mark per-use fields (`Shader`, `ShaderConfigureAction`, `PreviousScope`) as plain private — no `readonly` (they're now reassigned across acquisitions).

### 4. Update `ShaderScope.Dispose` to pool itself

Keep the existing batch-end / parent-restore work as-is. After `Graphics.CurrentBatchScope = null` (or the parent-rebegin path) finishes, null out `Shader`, `ShaderConfigureAction`, `PreviousScope`, then call `Graphics.ReleaseShaderScope(this)`. The release call is the last statement in `Dispose`.

### 5. Convert `SceneShaderScope` constructor → ctor + Initialize

Same shape as step 3, but the `Initialize` body covers more state: capture `PreviousScope`, `PreviousLayerScope`, `PreviousRenderTarget`; end the previous batch if any; acquire a layer render target via the existing `AcquireLayerRenderTarget`; switch to it and clear; set `Graphics.CurrentLayerScope = this`; call `BeginBatch`. The `LayerRenderTarget` property currently has a `{ get; }` init-only setter — change to a private settable backing so `Initialize` can assign per-use, OR keep it init-only and store the layer RT in a private field that the existing code paths read. Either works; pick the lower-churn path.

### 6. Update `SceneShaderScope.Dispose` to pool itself

Keep the existing composite-and-restore body intact (end batch, restore render target, run shader configure, begin a new batch with the shader, draw the layer, end, release the layer RT, restore parent batch). After that work finishes, null out every per-use field (`Shader`, `ShaderConfigureAction`, `PreviousScope`, `PreviousLayerScope`, `PreviousRenderTarget`, and the layer-RT field), then call `Graphics.ReleaseSceneShaderScope(this)` as the last statement.

### 7. Wire `WithShader` / `WithSceneShader` to the pool

Change each `new ShaderScope(this, shader, configure)` call to `var scope = AcquireShaderScope(); scope.Initialize(shader, configure); return scope;` (and likewise for `SceneShaderScope`). Both string-name overloads route through their `Effect?` siblings already; just confirm — no change needed at those call sites if the `Effect?`-typed methods are updated.

### 8. Add XML doc remark on lifetime

On all four public `WithShader` / `WithSceneShader` overloads, append a `<remarks>` (or a sentence to the existing one) noting the returned `IDisposable` must be disposed exactly once and not retained beyond its `using` scope. One sentence; do not over-document.

### 9. Convert `GameStateManager.Draw`'s `foreach` to `for`

Mirror the `Input` / `Update` pattern: `// ReSharper disable once ForCanBeConvertedToForeach` + `// for loop, instead of foreach, reduces allocations` + `for (var i = 0; i < ServiceWatcher.DrawnServices.Count; i++) ServiceWatcher.DrawnServices[i].Draw(gameTime);`.

### 10. Self-check: zero per-frame scope allocations after warm-up

Re-read `WithShader`, `WithSceneShader`, `Acquire*`, and both `Dispose` bodies. Confirm: on the steady-state path (pool non-empty), nothing allocates — no `new`, no LINQ, no closures. The per-use lambda the *caller* passes to `configure` is still allocated by the caller; that's out of scope (see Out of scope).

## Test Plan

- [ ] `dotnet build` clean across the whole solution — no new warnings, no errors.
- [ ] `dotnet test` passes.
- [ ] Manual: launch a PlayPlayMini sample game (the simplest available — anything that hits `Draw`). Run for 30+ seconds.
- [ ] Manual: under Rider's allocation profiler, confirm `ShaderScope` and `SceneShaderScope` instance counts stop growing after the first few frames (the pool fills, then no further allocations).
- [ ] Manual: under Rider's CPU profiler, confirm `GameStateManager.Draw` and `SceneShaderScope.Dispose` are no longer the top-two hotspots, OR — if they still appear high — confirm the cost is now genuine work (SpriteBatch state changes, GPU composite) and not allocation/GC.
- [ ] Manual: in a sample scene, nest `using (Graphics.WithSceneShader(...))` inside another `using (Graphics.WithSceneShader(...))` — confirm the inner scope composites into the outer layer and the outer composites to the framebuffer (no regression of the existing nesting behavior).
- [ ] Manual: in a sample scene, use `Graphics.WithShader(myShader, e => e.Parameters["X"].SetValue(value))` for two consecutive frames — confirm the shader continues to apply correctly (i.e., the pooled instance does not retain stale per-use state across acquisitions).
- [ ] Spot-check `_layerRenderTargetPool`'s `UnloadContent` drain still runs and disposes its render targets (regression check that the new pools didn't accidentally break the existing one).

## Learnings

### Architectural decisions

- **Open Decisions all resolved to the listed defaults.** `Stack<T>` over `ConcurrentStack<T>` (no concurrent use), lazy pool fill (no constructor pre-warm), and a separate `Initialize(...)` method on each scope rather than folding init into `Acquire`. The `Acquire`/`Initialize` split mirrors how `_layerRenderTargetPool` separates pool concern (`AcquireLayerRenderTarget`) from caller concern (the call site that does whatever it needs with the acquired RT). It reads more linearly than burying setup in a private `Acquire` helper.
- **`LayerRenderTarget` stayed a property**, just with `{ get; private set; } = null!;` instead of init-only-in-ctor. Lower churn than introducing a private backing field — the existing read sites (`PreviousLayerScope?.LayerRenderTarget`, `Graphics.SpriteBatch.Draw(LayerRenderTarget, …)`) still work unchanged.
- **`PreviousRenderTarget` became nullable** so `Dispose` can null it out before pooling. Between `Initialize` and `Dispose` it is always non-null; the nullability is purely a "cleared while in pool" annotation.

### Interesting tidbits

- Both scope `Dispose` methods pool *after* the existing batch-end / parent-restore work, then null out per-use refs, then call `Release{Shader|SceneShader}Scope(this)` as the last statement. Order matters: the per-use refs are read during the batch-restore work (`PreviousScope.BeginBatch()`), so they have to stay alive until that completes.
- Pre-existing `CS1570` "undefined entity 'nbsp'" warnings exist in unrelated files (`StringExtensions.cs`, `GameState.cs`, `MouseManager.cs`, `GameStateManagerBuilder.cs`). The "no new warnings" check is against that baseline, not zero.
- The repo has no `.sln` file — build per-project (`dotnet build BenMakesGames.PlayPlayMini\BenMakesGames.PlayPlayMini.csproj`) or per-test-project. The Tests project transitively builds the core lib.

### Rejected alternatives

- **Pre-allocating the pools at construction time** — the warm-up cost is one allocation per nesting level reached on the first few frames. Not worth the constructor complexity to pre-fill a `Stack<>` with N spares whose count would be guesswork anyway.
- **`ConcurrentStack<T>`** — `GraphicsManager` is single-threaded by MonoGame's `Draw` callback contract. The existing `_layerRenderTargetPool` is also a plain `Stack<>` — match precedent.
- **Singleton spare instead of a stack** — would break under nested `WithShader` calls, which do happen (`GameStateManager.Draw`'s outer wrapper plus any inner one in a state's `Draw`).
- **Converting scopes to `ref struct`** — would force a separate parent-tracking redesign because `CurrentBatchScope` / `CurrentLayerScope` are class fields. Out of scope and out of proportion to pooling's gain.

### Related areas affected

- `_layerRenderTargetPool` and its `UnloadContent` drain are unchanged. By code inspection, `UnloadContent` still pops & disposes render targets. The new scope pools intentionally have no drain — scope objects hold no unmanaged resources.

### Verification not run from this environment

The Test Plan's Manual items (Rider allocation/CPU profiler runs, nested `WithSceneShader` visual check, multi-frame shader-parameter check) require launching a sample game interactively. They are not yet exercised by this implementation pass — needs user verification.
