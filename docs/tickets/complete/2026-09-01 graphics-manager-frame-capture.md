# GraphicsManager one-shot frame capture

## Context

**Current behavior**: A game that wants to save the fully-composed frame to disk has no supported way to reach the post-`BackbufferPostProcessChain` result. `GameStateManager.Draw` calls `Graphics.BeginDraw()`, then runs `CurrentState.Draw` + every `IServiceDraw.Draw` inside a `WithShader((Effect?)null)` scope, then calls `Graphics.EndDraw()` — which is where `BackbufferPostProcessChain` runs. In `EndDraw`'s chain path, the last entry's shader draws straight to the actual backbuffer (`SetRenderTarget(null)`), so no render target ever holds the composited output; on the fast path (empty chain) the composited image is just the native `RenderTarget` point-upscaled to the backbuffer, and again nothing persistent exists. An `IServiceDraw` (e.g. a screenshot service) runs *inside* the outer `WithShader` scope — before `EndDraw` — so even if it grabs the currently-bound target, it gets the pre-upscale native `RenderTarget` and misses `BackbufferPostProcessChain` entirely. A game that wants a "what the player actually sees" screenshot today has to hand-replicate the whole backbuffer chain in its own code, or read back the presented backbuffer with `GraphicsDevice.GetBackBufferData` (fragile across backends, monitor-space dimensions).

**New behavior**: `GraphicsManager` exposes a one-shot capture request — `RequestFrameCapture(RenderTarget2D target)` — that arms a single-frame capture. Inside `EndDraw`, if a capture is armed, PPM writes the fully-composited frame (post-`BackbufferPostProcessChain`, at `Width * Zoom` × `Height * Zoom`) into the caller-supplied render target *in addition to* the normal backbuffer blit, then clears the arm flag. The caller reads the target after the frame is presented (i.e. from the next frame's `Draw` or later) and does whatever it wants with the pixels — `SaveAsPng`, hand it to a shader, whatever. Empty chain and populated chain both honor the request; an unarmed frame costs nothing new. Anything added to `BackbufferPostProcessChain` in the future (bloom, CRT, curvature, a hypothetical letterbox pass) automatically appears in captures with no per-effect wiring.

## Prerequisites

- `BackbufferPostProcessChain` support in `EndDraw` (shipped in 8.4.0 — the pipeline this ticket taps into).

## Scope

### In scope

- `BenMakesGames.PlayPlayMini\Services\GraphicsManager.cs` — a new public `RequestFrameCapture(RenderTarget2D target)` method that validates the target and stores it in an internal one-shot field. `EndDraw` consumes and clears the field, writing the composited frame into the target on both the fast path and the chain path.
- XML doc on `RequestFrameCapture` spelling out: one-shot semantics (consumed once, then cleared), when the target's pixels become readable (after the frame that armed the capture presents — earliest from the next frame's `Draw`), the required target dimensions and surface format, that empty and populated chains both honor it, and the "caller owns the RT" contract (PPM never disposes it).
- `UPGRADE.md` — short section under the current in-flight version's heading covering the new API, a one-line example of the two-frame arm/read pattern, and the "captures reflect the composited frame including `BackbufferPostProcessChain`" guarantee.

### Out of scope

- Any changes to `PostProcessChain` (native-resolution chain) — this ticket captures the final composited output, which is downstream of both chains, so tapping `EndDraw`'s exit is the right place regardless of which chain is populated.
- Multi-frame capture, video capture, ring buffers, or a "capture every frame" mode. YAGNI — a one-shot flag is enough for screenshot flows; a game that wants continuous capture can re-arm each frame from its own `IServiceDraw` and eat the cost knowingly.
- Managing the capture RT for the caller. Allocation, disposal, and reuse are the caller's business. The API takes a `RenderTarget2D` reference and stores it; PPM never disposes it, never resizes it, never reallocates it.
- Backbuffer readback via `GraphicsDevice.GetBackBufferData`. Considered and rejected — brittle across MonoGame backends, and monitor-space pixel dimensions can drift from `Width * Zoom` under windowed/letterbox/DPI variations. The `EndDraw`-side blit uses the exact game-space dimensions PPM controls.
- Consumer-side changes. Any game that adopts this (e.g. Astromino's dev-tool screenshot sweep) is a separate follow-up in its own repo.
- A same-frame synchronous read of the capture. Not supported — `EndDraw` runs *after* every `IServiceDraw.Draw` on the arming frame, so the target is populated only after those services have already returned. Reading synchronously would require an extra "post-EndDraw" hook that this ticket does not add. The two-frame arm/read pattern is documented instead.

## Relevant Docs & Anchors

- **The pipeline this hooks into**: `GraphicsManager.EndDraw` — the two branches (empty-chain fast path and chain path) are what the capture logic tees into. Read the current body top-to-bottom before changing anything; the chain path's ping-pong `intermediateA` / `intermediateB` logic and the "last entry draws to `null`" branch are exactly the hand-offs the capture code has to slot into.
- **Related ticket for the surrounding pipeline**: `docs\tickets\complete\2026-08-28 graphics-manager-post-process-chain.md` — established `BackbufferPostProcessChain` and the "empty chain has no per-frame cost" precedent this ticket preserves; also demonstrates the "add a section to `UPGRADE.md` under the in-flight version's heading" cadence.
- **XML doc style for `BackbufferPostProcessChain`**: the `BackbufferPostProcessChain` property's `<remarks>` on `GraphicsManager` — same level of detail (paragraph on semantics, paragraph on cost, paragraph on the invariant guarantee, paragraph on caller responsibilities) is the target for `RequestFrameCapture`'s docs.

## Constraints & Gotchas

- **`EndDraw` is `internal`, not overridable.** The capture arm has to be a separate public method (or property) — the two-branch capture write goes inside `EndDraw`. No external caller can call `EndDraw` directly, so no need to change its signature.
- **Empty-chain fast path must stay allocation-free when no capture is armed.** The current fast path is a `SetRenderTarget(null)` + `SpriteBatch.Begin/Draw/End` triple. When no capture is armed, do exactly what it does today — no branch, no field read, no allocation. Only when `_pendingCaptureTarget is not null` does the fast path do extra work.
- **Chain path must stay allocation-free when no capture is armed.** Same story — the pool-acquired intermediates and per-entry blits stay as-is. The extra "also render to the capture target" step is gated behind `_pendingCaptureTarget is not null`.
- **The capture RT's dimensions must match `Width * Zoom` × `Height * Zoom`.** If they don't, `EndDraw` would either silently scale-blit (visually wrong) or throw mid-frame (hard to diagnose). Validate at `RequestFrameCapture` time and throw an `ArgumentException` with a message naming the expected dimensions vs. the target's actual dimensions — the caller finds out immediately, not one frame later.
- **The capture RT must be a `SurfaceFormat.Color` render target with `RenderTargetUsage.PreserveContents`** (matching PPM's own `RenderTarget` allocation and the pool RTs after the rc8 fix). `Format` mismatch is safe to check at request-time. `Usage` matters if the caller intends to hold the RT across multiple frames or read it after other rebinds; validate at request-time too, or document as a caller responsibility — see Open Decisions.
- **Same-frame read is unsupported.** `EndDraw` runs *after* every `IServiceDraw.Draw` on the arming frame, so an `IServiceDraw` that both arms and reads in the same `Draw` gets stale (or zero) pixels. Document this loudly on `RequestFrameCapture` — read on frame N+1's `Draw` or later.
- **Capture RT vs. layer pool: never mix.** A caller who somehow got hold of a pool-acquired layer RT (they can't today — `AcquireLayerRenderTarget` is `internal`) and passed it here would poison the pool. Not a real concern given the visibility, but worth a one-line internal comment in `EndDraw` that the capture write happens *after* the last blit and the caller-owned RT stays outside the pool.
- **Graphics device reset (window resize, fullscreen toggle) can lose RT contents.** The capture RT is caller-owned; if the caller keeps it across a device reset, `RenderTargetUsage.PreserveContents` on their RT protects it, but any capture that was in-flight when the reset fired may be lost. Not our problem to defend against — document it as a caller consideration if it fits, or leave it entirely.
- **`SpriteBatch` state at capture time.** On both paths the capture write is another `SpriteBatch.Begin/Draw/End` triple — same discipline as the existing blits, no leaked state.

## Open Decisions

1. **Method name** — `RequestFrameCapture(RenderTarget2D)` vs. `CaptureNextFrameTo(RenderTarget2D)` vs. a settable `NextFrameCaptureTarget` property. Default: `RequestFrameCapture` (matches the "arm-then-consume" verb style, distinct from a persistent property, no confusion about whether re-assigning replaces or queues). Change to `CaptureNextFrameTo` if it reads more naturally next to `EndDraw` and friends.
2. **`RenderTargetUsage` validation** — throw at request-time if the caller's RT isn't `RenderTargetUsage.PreserveContents`, or document it as a caller responsibility and leave the check off the hot path (`EndDraw` doesn't validate). Default: don't validate — the failure mode (contents lost after another rebind) is caller-visible and documented, and the value is inspectable on `RenderTarget2D.RenderTargetUsage` if the caller wants to guard. Adds no per-request or per-frame cost.
3. **What happens if `RequestFrameCapture` is called twice before `EndDraw` consumes** — silently replace the pending target (last-writer-wins), or throw. Default: silently replace. The one-shot flag is a simple "capture the next frame" contract; two arms in one frame is almost certainly a bug in the caller, but throwing would surprise a reasonable "always arm from Input, don't check state" flow. Consider throwing if a real caller ever hits it and finds the silent overwrite confusing.
4. **Whether the fast path uses two blits (native → backbuffer, native → capture) or one intermediate (native → capture, capture → backbuffer)** — see Implementation step 3. Default: two blits — the fast path stays trivially close to today's code (one extra blit gated on the null-check), no new RT hop, no allocation.
5. **Where to place the capture write on the chain path** — inline as a second blit inside the last-entry branch, or a small helper on `GraphicsManager`. Default: inline. The write is three lines (`SetRenderTarget(capture)`, `SpriteBatch.Draw(source, ...)`, wrapped in `Begin/End`), and the last-entry branch is already the natural home for it.

## Acceptance Criteria

- [ ] `GraphicsManager` exposes a public `void RequestFrameCapture(RenderTarget2D target)` method (or the named-per-Open-Decision-1 equivalent) that stores the target for one-shot consumption by the next `EndDraw` call.
- [ ] `RequestFrameCapture` throws `ArgumentNullException` for a `null` target, and `ArgumentException` for a target whose `Width`, `Height`, or `Format` do not match `Width * Zoom`, `Height * Zoom`, `SurfaceFormat.Color` respectively. The exception message names the expected value(s) and the target's actual value(s).
- [ ] With no capture armed, both `EndDraw` branches (empty chain, non-empty chain) behave byte-identically to today — no additional field reads on the hot path beyond a single `is not null` check on the pending target, no additional allocation, no additional blit. Verifiable by code inspection.
- [ ] With a capture armed and `BackbufferPostProcessChain.Count == 0`, after `EndDraw` returns, the capture target contains the point-upscaled native `RenderTarget` at `Width * Zoom` × `Height * Zoom` — the same pixels the fast path writes to the backbuffer.
- [ ] With a capture armed and `BackbufferPostProcessChain.Count > 0`, after `EndDraw` returns, the capture target contains the output of the last chain entry's shader — the same pixels the chain path writes to the backbuffer.
- [ ] The pending capture target is cleared before `EndDraw` returns, whether or not the write completed successfully. Two consecutive frames — arm, present, don't arm, present — leave the capture target holding only the first frame's contents.
- [ ] XML doc on `RequestFrameCapture` covers: one-shot semantics (armed, consumed, cleared per frame); when the target's pixels become readable (after the arming frame presents — earliest from the next frame's `Draw`); required target dimensions and format; behavior when both chain and empty-chain paths run; caller-owns-the-RT contract (PPM never disposes it, allocates it, or resizes it); and the "same-frame read is unsupported" gotcha.
- [ ] `UPGRADE.md` gains a section under the current in-flight version's heading — API name, two-frame arm/read pattern, and the "captures include `BackbufferPostProcessChain`" guarantee. Short — one or two paragraphs plus a small snippet.
- [ ] All existing PPM tests pass. Any new tests added (see Implementation step 5) pass.
- [ ] `dotnet build` is clean across the solution — no new warnings, no errors.

## Implementation

### 1. Re-read `EndDraw` and the two branches

Open `GraphicsManager.cs`. Read the current `EndDraw` in full — both the `chain.Count == 0` fast path (three-line `SetRenderTarget(null)` + `SpriteBatch.Begin/Draw/End`) and the chain path's `intermediateA` / `intermediateB` ping-pong, including the last-entry branch that sets `destination = null` to draw straight to the backbuffer. Note where the caller-facing "final image is now committed" moment sits in each branch — that's where the capture write slots in. On the fast path it's a second blit tacked on. On the chain path it's the same last-entry blit, done a second time with `destination = captureTarget` instead of `null` (or done first, then re-blitted to `null`).

### 2. Add the pending-capture field and the public arm method

Adjacent to `RenderTarget` (or grouped with the other pool/scope private fields near the bottom of the field block): `private RenderTarget2D? _pendingCaptureTarget;`. Public method above `BeginDraw` (or grouped with the other public draw-time API):

```csharp
public void RequestFrameCapture(RenderTarget2D target) { ... }
```

Validate: null → `ArgumentNullException(nameof(target))`; dimension or format mismatch → `ArgumentException` with a message like `"Capture target must be {expectedW}x{expectedH} SurfaceFormat.Color, got {actualW}x{actualH} {actualFormat}."`. Then assign: `_pendingCaptureTarget = target;` (silently replacing any prior arm — see Open Decision 3). Write the XML doc per Acceptance Criteria — one paragraph on one-shot semantics, one on when to read, one on validation, one on the caller-owns-the-RT contract, one on the same-frame-read gotcha.

### 3. Tee the fast path

Inside `EndDraw`'s `chain.Count == 0` branch, keep the existing three lines exactly as-is. After the existing `SpriteBatch.End()`, add:

```csharp
if (_pendingCaptureTarget is not null)
{
    // ... blit RenderTarget into _pendingCaptureTarget at scaledRect ...
    _pendingCaptureTarget = null;
}
```

The blit mirrors the existing one: `SetRenderTarget(_pendingCaptureTarget)`, `SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp)`, `SpriteBatch.Draw(RenderTarget, scaledRect, Color.White)`, `SpriteBatch.End()`. Two blits total on an armed fast-path frame (native → backbuffer, native → capture); zero extra work on an unarmed frame beyond the null check. Local field, no allocation, no property read chain.

### 4. Tee the chain path

Inside `EndDraw`'s chain loop, the last-entry branch currently sets `destination = null`. Two-target variant: after the last-entry shaded blit lands on the backbuffer, if `_pendingCaptureTarget is not null`, do one more shaded blit of `source` (the input to the last entry) into `_pendingCaptureTarget` using the same shader and configure call. Clear `_pendingCaptureTarget` after the write. Reuses the shader effect already fetched (`shader`), the same `entry.Configure` call (which the shader currently retains from the backbuffer pass — no need to reconfigure unless the shader has per-draw internal state; if it does, call `Configure` again).

Alternative — draw the last entry to `_pendingCaptureTarget` first, then a plain unshaded `PointClamp` blit from `_pendingCaptureTarget` to the backbuffer. Uses one shaded blit instead of two, at the cost of one extra unshaded blit and slightly re-ordered work. Both are correct; pick whichever reads more clearly in-context. Zero extra work on an unarmed frame either way.

Whichever variant is chosen, clear `_pendingCaptureTarget = null` before returning from the chain path — Acceptance Criteria requires the one-shot to clear regardless of which branch consumed it, so both branches must reset it.

### 5. Tests

The existing test project (`BenMakesGames.PlayPlayMini.Tests`) has no coverage of the draw path — `GameStateManager` derives from MonoGame's `Game`, which requires a real window and `GraphicsDevice` to construct, and `EndDraw` needs the `SpriteBatch` and a bound render target to do anything meaningful. The 2026-08-28 chain ticket faced the same limit and skipped unit tests; do the same here for the `EndDraw` side.

The one testable surface without a `GraphicsDevice` is `RequestFrameCapture`'s validation branches — but each branch reads `Width * Zoom` off `GraphicsManager`, which in turn needs `Initialize` to have run, which needs a `Game`. If a fake construction path is feasible (e.g. bypass `Initialize` by setting the fields via reflection or a test-only setter), add three tests: null target throws `ArgumentNullException`; wrong dimensions throw `ArgumentException` with a message including "expected"; wrong format throws `ArgumentException` with a message including "SurfaceFormat.Color". If not feasible without a large test-only carve-out, skip and rely on the sample-game manual test in the Test Plan.

### 6. Update `UPGRADE.md`

Under the existing "Upgrading from 8.3.x to 8.4.0" heading (or under a new "Upgrading from 8.4.x to 8.5.0" heading — whichever matches the version this ships in; check `BenMakesGames.PlayPlayMini.csproj` before writing):

- One paragraph on the new `RequestFrameCapture` API — one-shot, consumed by the next `EndDraw`, target dimensions must be `Width * Zoom` × `Height * Zoom` and `SurfaceFormat.Color`.
- One paragraph on the arm/read pattern — arm on frame N, read the target on frame N+1's `Draw` or later (`EndDraw` runs after every `IServiceDraw.Draw`, so same-frame reads see stale pixels).
- One paragraph on the guarantee — captures reflect the composited frame including everything on `BackbufferPostProcessChain`, so a game that adds bloom / CRT / any future backbuffer effect gets them in screenshots automatically.
- One small snippet showing an `IServiceDraw` that arms on one frame, saves on the next, using a member field for the RT and a small `bool` for the "pending save" state.

### 7. Self-check

Re-read `EndDraw` in full after the changes. Verify:

- Fast path with no capture armed: unchanged code before the added `if` block; the `if` block short-circuits on `null` with no work.
- Fast path with capture armed: two blits total, in the order the ticket specifies; `_pendingCaptureTarget = null` before return.
- Chain path with no capture armed: unchanged.
- Chain path with capture armed: the last-entry blit lands on both the backbuffer and the capture target (via whichever of the two variants was picked); `_pendingCaptureTarget = null` before return.
- Both branches clear the field. A single arm consumed exactly once, next frame's `EndDraw` sees `null`.

## Test Plan

- [ ] `dotnet build` succeeds with no new warnings or errors.
- [ ] `dotnet test` passes for every test project in the solution.
- [ ] Manual: in a sample game with `BackbufferPostProcessChain` empty, add an `IServiceDraw` that allocates a `Width * Zoom` × `Height * Zoom` `RenderTarget2D` in `Initialize`, arms `RequestFrameCapture` on the first frame's `Draw`, and on the second frame's `Draw` calls `SaveAsPng` on the RT to disk. Confirm the resulting PNG matches the on-screen image byte-for-byte (or as close as `SaveAsPng`'s PNG encoding allows) at the same dimensions.
- [ ] Manual: with the same setup, populate `BackbufferPostProcessChain` with a single test shader (e.g. a tint-red pass). Re-run the arm/save flow. Confirm the saved PNG shows the tinted output — i.e. the capture reflects the post-`BackbufferPostProcessChain` frame, not the pre-chain native `RenderTarget`.
- [ ] Manual: populate `BackbufferPostProcessChain` with two entries (tint-red then tint-blue). Confirm the saved PNG is blue-tinted (the last entry's output — chain order = apply order, same as the on-screen result).
- [ ] Manual: arm the capture, present the frame, do *not* re-arm, present another frame, then inspect the RT. Confirm it still holds only the first frame's pixels — the one-shot cleared, the second frame didn't overwrite.
- [ ] Manual: `RequestFrameCapture(null)` → `ArgumentNullException`. `RequestFrameCapture(rt)` with wrong dimensions → `ArgumentException` with a message naming expected vs. actual. `RequestFrameCapture(rt)` with `SurfaceFormat.HalfVector4` (or any non-`Color`) → `ArgumentException` with a message naming `SurfaceFormat.Color`. All three throws happen synchronously from the request call, not on the next `EndDraw`.
- [ ] Manual: with no capture armed for 30+ seconds in a sample game under Rider's allocation profiler, confirm `GraphicsManager.EndDraw`'s per-frame allocations are unchanged from a build with the ticket's changes reverted.

## Learnings

### Architectural decisions

- **All five Open Decisions resolved to their listed defaults.** (1) Method name is `RequestFrameCapture` — "arm-then-consume" verb style, and distinct from a persistent property so re-assignment semantics are unambiguous. (2) `RenderTargetUsage` is not validated at request-time: the failure mode (contents lost after another rebind) is caller-visible via `RenderTarget2D.RenderTargetUsage`, and skipping the check keeps the request-time path narrow. (3) Double-arm silently replaces (last-writer-wins) — the assignment already does this with no extra code; adding a throw would surprise a reasonable "always arm from Input, don't check state" flow. (4) Fast path uses two blits (native → backbuffer, native → capture) when armed — trivially close to today's code, one extra blit gated on the null-check. (5) Chain path uses Variant B: on the last entry, when armed, `destination = _pendingCaptureTarget` (instead of `null`), do the shaded blit into the capture RT, then one unshaded PointClamp blit from the capture RT to the actual backbuffer. That way the on-screen pixels and the captured pixels come from the *same* shader invocation, not two — so any shader with non-deterministic-looking state (a `Time` uniform, a per-invocation random) still captures byte-identically.
- **The clear-the-arm-in-both-branches Acceptance Criterion turned into two `_pendingCaptureTarget = null` writes**, one at the end of the fast-path armed block and one after the chain-path `if (_pendingCaptureTarget is not null)` block. Simpler than a `try/finally` at the top of `EndDraw` because each branch already has a natural exit point and neither can throw between the check and the clear (both blit sequences are `Begin`/`Draw`/`End` triples over caller-owned or field-owned state).
- **The capture RT never goes through `AcquireLayerRenderTarget`.** The field is caller-owned, allocated by them, sized and formatted to match the game window's scaled dimensions. `EndDraw` treats it as a plain `SetRenderTarget` target, no pool interaction. This preserves the "layer pool is internal" contract established by the 2026-08-28 chain ticket — a caller who hands us a pool RT (they can't today) couldn't poison the pool because we never release it back.

### Interesting tidbits

- **Variant B's "one shaded blit + one unshaded blit" is a strict pixel-fidelity win over Variant A ("two shaded blits").** Both variants are correct for shaders whose output is purely a function of source-texture pixels + configured uniforms. But a shader that samples anything else (a monotonically-changing `Time` uniform captured at draw time; a per-invocation pseudo-random driven by a `SV_VertexID`-derived seed; texture sampling near a `NPOT` boundary where rounding differs across passes) would render two slightly different frames under Variant A, and the screenshot would drift from what the player sees. Variant B guarantees both destinations come from the same shader run.
- **The fast-path armed cost is two `SpriteBatch.Begin/Draw/End` triples with identical parameters, drawing the same source RT to the same destination rectangle.** No shader indirection, no configuration. This is the cheapest possible on-armed cost — the same work a "duplicate the last blit" naïve implementation would produce.
- **The chain-path unshaded finishing blit** (capture RT → backbuffer at `scaledRect`, `PointClamp`, `Opaque`) mirrors the empty-chain fast path's normal blit exactly, just with a different source texture. That symmetry is why Variant B reads cleanly in place: the unshaded finisher looks like something already documented in the file, not novel machinery.
- **`ArgumentNullException.ThrowIfNull(target)` collapses the null branch to one line** and matches the style used elsewhere in modern .NET code, though this is the first place in `GraphicsManager` to use it — the older null checks in the file predate .NET 6's helper.

### Workarounds / limitations

- **No unit tests were added.** The 2026-08-28 chain ticket faced the same limitation: `GameStateManager` needs a real `GraphicsDevice` to construct, and every interesting `EndDraw` assertion requires a live draw. `RequestFrameCapture`'s three throw branches could in principle be tested against a `GraphicsManager` whose `Width`/`Height`/`Zoom` were reflection-set without running `Initialize`, but the reflection carve-out is more maintenance than the checks are worth — the throw messages are simple string formatting, verified by manual test items in the Test Plan.
- **Manual Test Plan items 3–7 were not run in this pass.** They require an interactive sample game with a tint-shader-populated `BackbufferPostProcessChain` and Rider's allocation profiler; consumer verification will happen when Astromino's `ScreenshotSweep` adopts the API in a follow-up ticket. Same posture as the 2026-08-28 chain ticket's manual items.

### Rejected alternatives

- **Variant A (two shaded blits on the chain path)** — see the tidbits above. Correct for most shaders, but breaks pixel fidelity for any shader with non-source-only state. Variant B costs one extra unshaded blit to eliminate that entire class of bug.
- **Reading back the backbuffer via `GraphicsDevice.GetBackBufferData<Color>`** — considered up front, called out in the ticket's Out-of-Scope list. Brittle across MonoGame backends, and monitor-space pixel dimensions can drift from `Width * Zoom` under windowed/letterbox/DPI variations. The `EndDraw`-side blit uses the exact game-space dimensions PPM controls.
- **Continuous "capture every frame" mode via a settable `NextFrameCaptureTarget` property** — YAGNI, and out of scope. A game that wants continuous capture can re-arm each frame from its own `IServiceDraw` and pay the cost knowingly. Building the property in now would prejudge the question of whether it should persist across arm consumption.
- **Throwing on double-arm** (Open Decision 3 alternative) — would surprise a reasonable caller who arms unconditionally from Input without state-checking. The silent-replace default matches the "one-shot request that expresses intent for the next frame" mental model.
- **A `try/finally` at the top of `EndDraw` to guarantee `_pendingCaptureTarget = null` even on throw** — unnecessary. Each blit sequence is a `Begin`/`Draw`/`End` triple over state PPM controls; no user code runs between the null check and the clear.

### Related areas affected

- `UPGRADE.md` gained a "New: `RequestFrameCapture` for post-composite frame capture" section under the existing 8.3.x → 8.4.0 heading (this ships in a later 8.4.0 rc, still on the same major/minor). Includes the two-frame arm/read pattern and a full `IServiceDraw` example.
- Every rc-train package version bumped rc9 → rc10 in one pass, per the user's instruction and the [[reference_ppm_local_feed_publish]] procedure: `PlayPlayMini` (8.4.0), `PlayPlayMini.GraphicsExtensions` (8.4.0), `PlayPlayMini.BeepBoop` (0.16.0), `PlayPlayMini.NAudio` (0.19.0), `PlayPlayMini.VN` (1.4.0). `PlayPlayMini.UI` (7.1.0 stable) and `PlayPlayMini.Performance` (no `Version`) untouched.
- Follow-up in Astromino's repo: `ScreenshotSweep` should stop hand-replicating the point-upscale in `SaveCurrentFrame` and switch to the arm/read pattern. That's a separate ticket in that repo — not in this PPM ticket's scope.

