# Story 10.8: Pour Visual Fixes

Status: done

## Story

As a player,
I want to see liquid decrease smoothly in the source bottle during a pour with correct fill levels and no visual glitches,
so that the pour animation looks polished and trustworthy — I can follow exactly how much liquid is moving.

## Acceptance Criteria

1. **Source liquid decreases smoothly**: During the pour animation, the source bottle's liquid level must visually decrease at the same rate the target bottle's liquid increases. Both must be synchronized and smooth.

2. **No extra-slot visual bug**: During the pour animation, the target bottle must NEVER show more filled slots than the correct amount. Specifically:
   - If source has 1 slot filled and pours 1 slot to target (which has 1 slot), the target must show exactly 2 slots during and after animation — NOT 3 slots during animation
   - The bug is that band computation appears to double-count the poured liquid during the animation lerp

3. **Enforce 75–80% visual fill cap**: When a bottle is completely full (all slots filled), the liquid must render to a maximum of 75–80% of the bottle sprite height — NEVER 100%. This must hold true:
   - During pour animation (as target fills up)
   - After pour animation completes
   - On level load for pre-filled bottles
   - The `_MaxVisualFill` shader property (currently 0.80) must be respected at ALL times

4. **Last slot pour respects visual cap**: When pouring the final slot that completes a bottle, the fill animation must stop at the `_MaxVisualFill` cap (80%), not animate to 100% of bottle height.

5. **Fill synchronization**: Source decrease and target increase must be perfectly synchronized — at any frame during the animation, `sourceFillLost + targetFillGained = totalPourAmount` (conservation of liquid).

## Tasks / Subtasks

- [x] Task 1: Debug and fix source liquid not decreasing smoothly (AC: #1, #5)
  - [x] 1.1 Root cause identified: ExecutePour mutates ContainerData BEFORE PourAnimator reads it — srcBandsBefore = post-pour state, so lerp shows no change
  - [x] 1.2 Verified ComputeBandsAfterSourcePour() logic is correct — issue was input data, not computation
  - [x] 1.3 Verified SetFillAmount() propagation is correct — material instance and shader array updates work
  - [x] 1.4 Confirmed no other caller resets source fills during animation (ResetVisualState only resets position/scale/dim)
  - [x] 1.5 Fix: Added sourceDataSnapshot/targetDataSnapshot parameters to PourAnimator.Animate(); GameplayManager captures Clone() before ExecutePour

- [x] Task 2: Fix extra-slot visual bug in target bottle (AC: #2)
  - [x] 2.1 Confirmed ComputeBandsAfterTargetPour() adds pourCount on top of existing bands — correct logic but wrong input data
  - [x] 2.2 Confirmed: ComputeBands(target.Data) reads post-pour data (ExecutePour already ran), so tgtBandsBefore = post-pour state
  - [x] 2.3 Root cause confirmed: ExecutePour at GameplayManager:507 mutates data, then PourAnimator at :63-64 reads already-mutated Data, then ComputeBandsAfterTargetPour adds pourCount AGAIN = double-counting
  - [x] 2.4 Fix: PourAnimator.Animate() now accepts pre-pour ContainerData snapshots and uses them for all band computations
  - [x] 2.5 GameplayManager captures source.Clone() and target.Clone() before ExecutePour and passes to PourAnimator

- [x] Task 3: Enforce visual fill cap during animations (AC: #3, #4)
  - [x] 3.1 Verified _MaxVisualFill = 0.80 is set in LiquidMaterialController.Initialize() (line 42)
  - [x] 3.2 Confirmed band fill sums cannot exceed 1.0 with correct pre-pour snapshot data (max is slotCount/slotCount = 1.0)
  - [x] 3.3 Added ClampFillSum() safety clamp in ComputeBandsAfterTargetPour to prevent rounding overflow
  - [x] 3.4 Verified shader LiquidFill.shader:118 multiplies each band by _MaxVisualFill — visual cap at 80% confirmed
  - [x] 3.5 Added SetupSourceColors() to re-apply pre-pour band colors from snapshot, ensuring shader state consistency

- [x] Task 4: Remove debug logging after fixes confirmed
  - [x] 4.1 No debug logging was added — root cause was clear from code analysis, fix was direct
  - [x] 4.2 Visual verification requires Unity play mode — test scenarios documented in Dev Notes

## Dev Notes

### Critical Bug Analysis

**Extra-slot bug (Issue #4 from user report):**
The most likely root cause is a **data timing issue**. Looking at the animation flow:

1. `GameplayManager` calls `ExecutePour()` which modifies `ContainerData` (adds slots to target, removes from source)
2. `GameplayManager` then starts `PourAnimator.Animate()` coroutine
3. `PourAnimator.Animate()` calls `ComputeBands(target.Data)` for `tgtBandsBefore`
4. But `target.Data` ALREADY has the poured slots because `ExecutePour` ran first!
5. Then `ComputeBandsAfterTargetPour()` adds `pourCount` AGAIN

**Fix approach**: Either:
- (A) Run animation BEFORE `ExecutePour`, using predicted states, then apply game logic in `onMidPour`
- (B) Pass pre-pour data snapshots to `PourAnimator` instead of reading live `Data`

Option (B) is safer — take snapshots of source/target `ContainerData` before `ExecutePour`, pass to `PourAnimator`.

**Source not decreasing smoothly:**
May be the same root cause — if `source.Data` already has slots removed, `srcBandsBefore` is already the post-pour state, so the lerp from before→after shows no change.

### Key Files to Modify

| File | What to Change |
|------|----------------|
| `Assets/Scripts/Game/Puzzle/PourAnimator.cs` | Fix band computation to use pre-pour snapshots, clamp fill sums |
| `Assets/Scripts/Game/Puzzle/GameplayManager.cs` | Pass pre-pour data snapshots to PourAnimator, or reorder ExecutePour timing |
| `Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs` | Verify SetFillAmount propagation |
| `Assets/Art/Shaders/LiquidFill.shader` | Audit _MaxVisualFill multiplication in frag function |

### Shader Fill Cap Verification

The shader property `_MaxVisualFill` (0.80) should multiply all band fill heights. Verify in `LiquidFill.shader`:
```hlsl
// Expected: totalFill = sum(bandFills) * _MaxVisualFill
// So even if bands sum to 1.0, visual height caps at 80%
```

### Testing Scenarios

1. Pour 1 slot from source (2 filled) to target (1 filled) → target shows 2 during AND after animation
2. Pour 2 slots from source (3 filled) to target (2 filled) → target shows 4, source shows 1
3. Pour last slot to fill target completely → visual fill stops at 80%, not 100%
4. Pour from full bottle (4/4) → source decrease must be visible from very first frame

### Project Structure Notes

- Alignment: ContainerData is the game's source of truth. Visual state must be derived from it but animation needs pre-pour snapshots.
- `PourAnimator` is static — snapshots can be passed as parameters or computed inside `Animate()` before any data changes.

### References

- [Source: Assets/Scripts/Game/Puzzle/PourAnimator.cs:61-70] — Band computation (before/after)
- [Source: Assets/Scripts/Game/Puzzle/PourAnimator.cs:197-228] — ComputeBandsAfterSourcePour
- [Source: Assets/Scripts/Game/Puzzle/PourAnimator.cs:233-274] — ComputeBandsAfterTargetPour
- [Source: Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs:42] — _MaxVisualFill = 0.80
- [Source: Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs:104-109] — SetFillAmount
- [Source: Assets/Art/Shaders/LiquidFill.shader] — Shader fill cap logic

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — no debug logging needed; root cause identified through code analysis.

### Completion Notes List

- Root cause for BOTH source-not-decreasing and extra-slot bugs was the same: data timing. `ExecutePour` mutates `ContainerData` before `PourAnimator` reads it. Fix: pass pre-pour `ContainerData.Clone()` snapshots to `PourAnimator.Animate()`.
- Added `SetupSourceColors()` to re-apply source band colors from snapshot, preventing stale shader state after data mutation.
- Added `ClampFillSum()` safety clamp in `ComputeBandsAfterTargetPour` to prevent visual overflow from floating-point accumulation.
- Shader `_MaxVisualFill = 0.80` was already correctly applied — no shader changes needed.
- `LiquidMaterialController.SetFillAmount()` propagation verified correct — issue was upstream data, not shader communication.

### Change Log

- 2026-03-23: Implemented pre-pour snapshot fix for pour animation visual bugs (Tasks 1-4)
- 2026-03-24: Post-completion code review (Epic 10 cross-story) — 2 High, 6 Medium, 8 Low fixes:
  - Removed dead _WobbleZ shader property and all C# references
  - Extracted fill compensation magic numbers to named constants (MinFillRatioForTilt, MaxTiltCompensation)
  - Fixed wobble coroutine leak: added StopWobble() in LiquidMaterialController.OnDestroy()
  - Fixed PourStreamVFX material leak: clear LineRenderer reference before destroy
  - Added PourStreamVFX cleanup in GameplayManager.OnDestroy() to prevent orphaned GameObjects
  - Added null validation guard at top of PourAnimator.Animate()
  - Batched SetFillAmount GPU uploads: 12/frame → 2/frame via FlushFills()
  - Added 2 missing test cases (empty target pour, fill sum clamping)
  - Fixed stale comments (tilt curve values, BaseTiltStrength, symmetric ratios, shader tiltY docs)

### File List

- `Assets/Scripts/Game/Puzzle/PourAnimator.cs` — Modified: snapshots, named constants, null guard, FlushFills batching, comment fixes
- `Assets/Scripts/Game/Puzzle/GameplayManager.cs` — Modified: snapshot capture, PourStreamVFX cleanup in OnDestroy
- `Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs` — Modified: removed WobbleZ, added FlushFills(), StopWobble in OnDestroy
- `Assets/Art/Shaders/LiquidFill.shader` — Modified: removed _WobbleZ, additive tilt, updated comments
- `Assets/Scripts/Game/Puzzle/PourStreamVFX.cs` — Modified: material leak fix in OnDestroy
- `Assets/Scripts/Tests/EditMode/PourAnimatorBandTests.cs` — Modified: added empty target and fill sum tests
- `Assembly-CSharp.csproj` — Modified: added shader reference
