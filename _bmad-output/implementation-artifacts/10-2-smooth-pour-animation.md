# Story 10.2: Smooth Pour Animation

Status: done

## Story

As a player,
I want to see the liquid level smoothly rise and fall during a pour,
so that pouring feels satisfying and fluid rather than slot-by-slot.

## Acceptance Criteria

1. **Smooth drain** — Source bottle's liquid level smoothly decreases via fill amount lerp (not instant slot removal)
2. **Smooth fill** — Target bottle's liquid level smoothly increases synchronized with source drain
3. **Dynamic tilt** — Source bottle tilt angle scales with pour count (more liquid = more tilt)
4. **Band-aware animation** — Fill amounts animate per color band, not per slot; when a band drains, next band starts
5. **PourAnimator refactored** — Internal animation changed from slot-by-slot `SetSlotVisible` to `LiquidMaterialController.SetFillAmount` lerps; static class pattern preserved
6. **Timing preserved** — Total pour duration remains ~0.5s (lift 0.15s, tilt 0.12s, pour lerp, return 0.2s)
7. **Visual headroom respected** — Fill amounts respect `_MaxVisualFill` during animation
8. **Final Refresh** — After animation completes, `Refresh()` snaps to final data-driven state

## Tasks / Subtasks

- [x] Task 1: Refactor PourAnimator for smooth fills (AC: 1, 2, 5, 6)
  - [x]1.1 Replace Phase 3 (slot-by-slot transfer) with a smooth fill lerp phase
  - [x]1.2 Source: lerp fill amounts from current to post-pour values over pour duration
  - [x]1.3 Target: lerp fill amounts from current to post-pour values (synchronized with source)
  - [x]1.4 Access LiquidMaterialController via `source.LiquidController` / `target.LiquidController` (property added in 10-1)
  - [x]1.5 Use `LiquidController.SetFillAmount()` for per-frame updates during lerp
  - [x]1.6 Remove calls to `SetSlotVisible()` and `SetSlotColorAndShow()` from PourAnimator
  - [x]1.7 Remove stub `SetSlotVisible()` and `SetSlotColorAndShow()` methods from BottleContainerView (no longer needed)
  - [x]1.8 Keep lift, tilt, return phases unchanged

- [x] Task 2: Dynamic tilt angle (AC: 3)
  - [x]2.1 Scale `TiltAngle` based on `pourCount / source.SlotCount` ratio
  - [x]2.2 More liquid poured = steeper tilt (range: 15° to 35°)

- [x] Task 3: Band-aware fill calculation (AC: 4, 7)
  - [x]3.1 Pre-compute source band state before and after pour
  - [x]3.2 Pre-compute target band state before and after pour
  - [x]3.3 Lerp each band's fill amount independently from pre to post state
  - [x]3.4 All fill heights scaled by `_MaxVisualFill`

- [x] Task 4: Validate (AC: 8)
  - [x]4.1 Verify final Refresh() after animation snaps to correct data-driven state
  - [x]4.2 Test with 1-unit, 2-unit, and max-unit pours
  - [x]4.3 Verify input remains blocked during animation

## Dev Notes

### What you're changing

Current PourAnimator Phase 3 does slot-by-slot instant transfers:
```csharp
source.SetSlotVisible(srcSlot, false);     // instant hide
target.SetSlotColorAndShow(tgtSlot, color); // instant show
```

New approach: smooth lerp of fill amounts via `LiquidMaterialController`:
```csharp
// Over pourDuration, lerp source fills from pre-pour to post-pour
// Simultaneously lerp target fills from pre-pour to post-pour
```

### Dependencies

- **Requires Story 10-1** — LiquidMaterialController, shader, and `LiquidController` property must exist
- PourAnimator is a static class — accesses LiquidMaterialController via `BottleContainerView.LiquidController` property

### Accessing LiquidMaterialController from static PourAnimator

PourAnimator receives `BottleContainerView` params. Access the controller via the public property added in 10-1:
```csharp
// In PourAnimator.Animate():
var srcController = source.LiquidController;
var tgtController = target.LiquidController;
// Use srcController.SetFillAmount() for per-frame lerps
```

### Cleanup: remove stub methods

Story 10-1 kept `SetSlotVisible()` and `SetSlotColorAndShow()` as stubs for PourAnimator compatibility. After this story refactors PourAnimator to use `LiquidController` directly, **remove both stub methods** from BottleContainerView.

### What MUST NOT change

- PourAnimator remains a static class with `Animate()` IEnumerator
- Lift, tilt, return phases timing unchanged
- `onMidPour` and `onComplete` callbacks preserved
- Input blocking during animation preserved

### References

- [Source: _bmad-output/epics.md#Epic 10] — Story 10.2 specification
- [Source: Scripts/Game/Puzzle/PourAnimator.cs] — Current implementation to refactor

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Replaced Phase 3 slot-by-slot transfer with smooth fill lerp (0.25s PourLerpDuration)
- Dynamic tilt: 15°-35° based on pourCount/slotCount ratio
- Band-aware: pre-computes source/target band states before & after pour, lerps each independently
- ComputeBandsAfterSourcePour: removes top slots, clamps runs to remaining filled
- ComputeBandsAfterTargetPour: adds pourCount of pourColor, merges with top band if same color
- Removed SetSlotVisible/SetSlotColorAndShow stubs from BottleContainerView
- Final Refresh() snaps to data-driven state after animation
- Lift, tilt, return phases timing unchanged; onMidPour/onComplete callbacks preserved

### File List

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PourAnimator.cs` — complete Phase 3 rewrite with smooth fills, dynamic tilt, band-aware lerp
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleContainerView.cs` — removed SetSlotVisible/SetSlotColorAndShow stubs

### Change Log

- 2026-03-23: Story implementation complete. PourAnimator refactored, stubs removed.
