# Story 10.7: Pour Movement Overhaul

Status: done

## Story

As a player,
I want to see the source bottle physically move above the target bottle and tilt progressively based on liquid depth during a pour,
so that the pouring animation looks realistic and satisfying — like a real person tilting a bottle to pour liquid.

## Acceptance Criteria

1. **Source bottle moves horizontally to target**: After lifting, the source bottle must translate horizontally to position itself directly above (or near-above) the target bottle before tilting to pour.

2. **Multi-row movement support**: When the source and target bottles are on different rows (2-row layout), the source bottle must move vertically between rows to reach the target. For example:
   - Bottom row → top row: source lifts and moves up+over
   - Top row → bottom row: source lifts and moves down+over
   - Same row: source lifts and moves horizontally only

3. **Progressive tilt based on liquid depth**: The tilt angle must increase progressively during the pour based on which layer of liquid is being drained:
   - **Top layer only** (bottle nearly full): small tilt ~15°–25°
   - **Middle layers**: moderate tilt ~40°–60°
   - **Bottom layers**: steep tilt ~70°–100°+
   - The tilt ANIMATES continuously during the pour — as liquid drains, the bottle tilts further to "chase" the receding liquid level

4. **Tilt follows source fill level, not just pour count**: If 3 layers are being poured from a 4-slot bottle, tilt starts at ~40° (first layer near top) and progresses to ~90°+ as it reaches the bottom layer. The tilt angle at any moment corresponds to the current remaining fill level in the source.

5. **Smooth movement and return**: All movements (lift, horizontal translate, tilt, return) use eased interpolation. After pour completes, the source bottle smoothly returns to its original position via a natural-looking path (not teleport).

6. **No collision/overlap during movement**: The moving source bottle must not visually clip through other bottles during its travel path. Consider lifting above other bottles before horizontal movement.

## Tasks / Subtasks

- [x] Task 1: Refactor PourAnimator phases to add horizontal movement (AC: #1, #5)
  - [x] 1.1 Phase 2 added: LerpPosition from lifted pos to hover above target (targetLocalPos.x, liftY)
  - [x] 1.2 Hover position calculated: max Y of source/target + bottleWorldHeight * 0.6 + HoverClearance (0.5 units)
  - [x] 1.3 EaseInOutCubic easing added for horizontal movement (new overload of LerpPosition with Func<float,float> param)
  - [x] 1.4 MoveDuration = 0.18f constant added

- [x] Task 2: Add multi-row awareness (AC: #2, #6)
  - [x] 2.1 Lift height uses `Mathf.Max(originalPos.y, targetLocalPos.y)` — automatically handles different-row bottles
  - [x] 2.2 Lift goes above BOTH rows by using max Y + bottleHeight * 0.6 + clearance
  - [x] 2.3 Same-row case: max Y is same for both, horizontal slide at lift height
  - [x] 2.4 Uses localPosition (bottle positions relative to board) — no need for BottleLayout struct directly
  - [x] 2.5 HoverClearance (0.5 units) + 60% bottle height ensures no clipping even with tilt

- [x] Task 3: Replace fixed tilt with progressive depth-based tilt (AC: #3, #4)
  - [x] 3.1 Removed MinTiltAngle/MaxTiltAngle (15°/35°); replaced with TiltAtFull (15°) and TiltAtEmpty (105°)
  - [x] 3.2 FillRatioToTiltAngle() maps fill ratio to angle: 1.0→15°, 0.75→~30°, 0.5→~55°, 0.25→~80°, 0.0→105°
  - [x] 3.3 Each frame during Phase 3: currentFillRatio = Lerp(prePourRatio, postPourRatio, t) drives tilt
  - [x] 3.4 Quadratic-blended curve (60% quadratic, 40% linear) for natural acceleration at lower fills
  - [x] 3.5 sourceTf.localRotation updated every frame during pour loop (not just once)
  - [x] 3.6 Tilt direction computed from dx = target.x - source.x, applied as Z rotation * direction

- [x] Task 4: Update return phase for new movement (AC: #5)
  - [x] 4.1 Phase 4: un-tilt (30%) → horizontal move back to original X (35%) → descend to rest (35%)
  - [x] 4.2 Three-step return with EaseInOutCubic for horizontal travel
  - [x] 4.3 ReturnDuration = 0.25f, split proportionally across three sub-phases

- [x] Task 5: Adjust timing constants
  - [x] 5.1 Total animation: ~0.12 + 0.18 + (0.25-0.4) + 0.04 + 0.25 = ~0.84-0.99s (snappy)
  - [x] 5.2 Constants: LiftDuration=0.12, MoveDuration=0.18, BasePourDuration=0.25 + 0.05/slot, ReturnDuration=0.25

## Dev Notes

### Critical Architecture Requirements

- **PourAnimator is a static class** — no MonoBehaviour, no instance fields. All state must be local to the coroutine or passed as parameters.
- **All positions use `localPosition`** relative to the board transform. The `BottleBoardView` parent positions the board; bottles are children.
- **BottleLayout.Positions** array contains the rest positions for all bottles. Index matches bottle index.
- The hover height must account for bottle scale (from `BottleLayout.Scale`).

### Key Files to Modify

| File | What to Change |
|------|----------------|
| `Assets/Scripts/Game/Puzzle/PourAnimator.cs` | Main refactor: add movement phases, progressive tilt, new return path |
| `Assets/Scripts/Game/Puzzle/GameplayManager.cs` | May need to pass layout info (positions, scale) to PourAnimator |

### Current Animation Flow (to be refactored)

```
Phase 1: Lift (+0.4Y, 0.15s) — KEEP but adjust height
Phase 2: Tilt (fixed angle, 0.12s) — REPLACE with progressive tilt during pour
Phase 3: Pour lerp (0.25s) — KEEP but add tilt animation alongside fill lerps
Phase 4: Return (0.2s) — REPLACE with full return path
```

### New Animation Flow

```
Phase 1: Lift source above other bottles (0.12s)
Phase 2: Move horizontally to above target (0.18s, row-aware)
Phase 3: Pour — simultaneous tilt progression + fill lerps + stream (0.25–0.4s)
Phase 4: Return — un-tilt → travel back → descend to rest (0.25s)
```

### Magic Sort Reference Behavior

In Magic Sort, the source bottle:
- Lifts and travels to hover above the target
- Tilts progressively — nearly inverted (~100°+) for bottom-layer pours
- The tilt angle directly corresponds to how deep the liquid being poured is
- Returns smoothly to its rest position after pouring

### Project Structure Notes

- Alignment with unified project structure: PourAnimator stays static, receives all needed context via parameters
- If layout data is needed, pass `BottleLayout` struct or just the positions array
- No new files expected — this is a refactor of `PourAnimator.cs`

### References

- [Source: Assets/Scripts/Game/Puzzle/PourAnimator.cs] — Current 4-phase animation
- [Source: Assets/Scripts/Game/Layout/ResponsiveLayoutManager.cs] — Row layout calculation, positions
- [Source: Assets/Scripts/Game/Layout/BottleLayout.cs] — Layout struct with Positions, Scale, RowCount
- [Source: Assets/Scripts/Game/Puzzle/BottleContainerView.cs] — Bottle transform hierarchy

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Complete rewrite of PourAnimator.Animate() with 4 new phases: Lift → Move to target → Progressive tilt+pour → Return
- Removed unused `sourceTopIndex`/`targetFirstEmpty` params (flagged in 10.8 code review)
- Added `bottleWorldHeight` param for clearance calculation
- Progressive tilt: FillRatioToTiltAngle() with quadratic-blended curve (15°→105° range)
- Multi-row aware: lift height uses max(source.y, target.y) + bottle height clearance
- EaseInOutCubic added for horizontal movement (smoother than EaseOutCubic for travel)
- Pour duration scales with pourCount: 0.25s base + 0.05s per slot poured
- Return phase: 3-step (un-tilt → horizontal return → descend)
- All band computation and snapshot logic preserved from Story 10.8

### Change Log

- 2026-03-23: Complete PourAnimator refactor — movement, progressive tilt, return path (Tasks 1-5)

### File List

- `Assets/Scripts/Game/Puzzle/PourAnimator.cs` — Rewritten: new 4-phase animation, progressive tilt, horizontal movement, multi-row support, removed unused params
- `Assets/Scripts/Game/Puzzle/GameplayManager.cs` — Modified: removed sourceTopIndex/targetFirstEmpty, added bottleWorldHeight calculation, updated Animate() call
