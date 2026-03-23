# Story 10.9: Pour Stream Rework

Status: done

## Story

As a player,
I want to see the liquid stream originate from the tilted bottle's mouth opening and adapt its length based on the target's water level,
so that the pour looks physically correct — liquid flows from the bottle opening and the stream naturally reaches the target's current fill level.

## Acceptance Criteria

1. **Stream originates from bottle mouth/opening**: The pour stream must start from the actual opening of the source bottle (the top edge/lip), not from a fixed offset. As the bottle tilts, the stream origin point must track the bottle's mouth position accurately.

2. **Stream origin follows tilt**: As the source bottle progressively tilts during the pour (Story 10.7), the stream origin must continuously update to stay at the bottle's mouth. At 15° tilt the mouth is near the top; at 90°+ tilt the mouth is to the side/below.

3. **No stream clipping through bottle**: The stream must NOT visually pass through the bottle body. It must start from the opening/lip — there should be no visible gap between the bottle edge and the stream start.

4. **Stream length adapts to target water level**: The stream endpoint must target the current liquid surface in the target bottle, NOT a fixed position:
   - If target has 3/4 slots filled: stream is short (liquid surface is high)
   - If target is empty: stream is long (liquid surface is at the bottom)
   - During animation: stream endpoint rises as the target fills up (stream gets shorter over time)

5. **Stream arc adjusts with length**: Shorter streams should have a tighter arc; longer streams should have a wider, more gravity-influenced arc. The `ArcHeight` should scale proportionally to stream length.

6. **Stream width and appearance**: Stream should taper naturally from source to target. Width should be proportional to pour rate (more liquid = wider stream). Color must match the liquid being poured.

## Tasks / Subtasks

- [x] Task 1: Calculate accurate bottle mouth position (AC: #1, #2, #3)
  - [x] 1.1 MouthOffsetRatio = 0.42 from pivot center — `Vector3(0, spriteHeight * 0.42, 0)` in local space
  - [x] 1.2 `GetBottleMouthWorldPos(Transform, float spriteHeight, float bottleScale)` added to PourAnimator
  - [x] 1.3 Uses `bottleTf.TransformPoint(localMouth)` — automatically rotates with tilt at any angle
  - [x] 1.4 TransformPoint handles all angles correctly by design — tested implicitly via progressive tilt (15°–105°)

- [x] Task 2: Calculate target liquid surface position (AC: #4)
  - [x] 2.1 `surfaceLocalY = spriteHeight * BottleBottomRatio + fillRatio * MaxVisualFill * spriteHeight + SurfaceLandingOffset`
  - [x] 2.2 Each frame: `currentTgtFillRatio = Lerp(tgtPreFillRatio, tgtPostFillRatio, t)` drives surface height
  - [x] 2.3 Uses `targetTf.TransformPoint(Vector3(0, surfaceLocalY, 0))` for world position
  - [x] 2.4 SurfaceLandingOffset = 0.05 units above computed surface

- [x] Task 3: Refactor PourStreamVFX for dynamic endpoints (AC: #1–#5)
  - [x] 3.1 `UpdatePositions()` signature unchanged — arc adapts internally via `UpdateCurve()`
  - [x] 3.2 Dynamic arc: `arcHeight = Lerp(MinArcHeight=0.1, MaxArcHeight=0.5, streamLength/MaxExpectedLength=4.0)`
  - [x] 3.3 Bezier control point uses dynamic arc height each frame
  - [x] 3.4 GravityBias = 0.15 shifts control point toward source — `mid = Lerp(start, end, 0.5 - 0.15)`

- [x] Task 4: Integrate with PourAnimator (AC: #1–#4)
  - [x] 4.1 `GetBottleMouthWorldPos()` called every frame in Phase 3 loop for streamSourcePos
  - [x] 4.2 `GetTargetSurfaceWorldPos()` called every frame with lerped tgtFillRatio for streamTargetPos
  - [x] 4.3 Both positions passed to `pourStream.UpdatePositions()` each frame
  - [x] 4.4 Stream starts AFTER initial tilt (after LerpRotation to startTiltRot) — correct ordering
  - [x] 4.5 Stream stops before Phase 4 return — `pourStream.StopStream()` called before un-tilt

- [x] Task 5: Stream width scaling (AC: #6)
  - [x] 5.1 `widthScale = Lerp(0.7, 1.3, pourRatio)` in `StartStream()` — more liquid = wider stream
  - [x] 5.2 BaseStartWidth = 0.08, BaseEndWidth = 0.05 — natural taper preserved
  - [x] 5.3 Color passed via `StartStream()` — `ThemeConfig.GetDrinkColor(pourColor)` — verified preserved

## Dev Notes

### Bottle Mouth Position Calculation

The bottle sprite has a SpriteMask that defines the bottle shape. The "mouth" is the top opening. In the current setup:
- `BottleContainerView` uses `_liquidRenderer` positioned as child
- Bottle sprite height is used for layout calculations
- The 80% visual fill means the bottle mouth area is roughly at Y = `spriteHeight * 0.42` from the bottle's pivot (center)
- When bottle tilts, `Transform.TransformPoint()` automatically rotates this offset

```csharp
// Example: get mouth world position
Vector3 localMouth = new Vector3(0f, bottleSpriteHeight * 0.42f * scale, 0f);
Vector3 worldMouth = sourceTf.TransformPoint(localMouth);
```

### Dependency on Story 10.7

This story depends on **10.7 (Pour Movement Overhaul)** because:
- The source bottle's position and tilt change continuously during pour (10.7 adds progressive tilt)
- The mouth position calculation must work with the progressive tilt angles (15°–105°)
- Can be developed in parallel but integration testing requires 10.7

### Current Stream Issues (from user report)

The current stream uses fixed offsets:
```csharp
// Current (broken):
streamSourcePos = sourceTf.position + new Vector3(0f, LiftHeight * 0.5f, 0f);  // NOT the mouth!
streamTargetPos = targetTf.position + new Vector3(0f, LiftHeight * 0.3f, 0f);  // NOT the surface!
```

This means:
- Stream starts from center of bottle, not the opening — appears to flow through the glass
- Stream always targets the same height regardless of target fill level
- Arc doesn't adapt to distance

### Key Files to Modify

| File | What to Change |
|------|----------------|
| `Assets/Scripts/Game/Puzzle/PourStreamVFX.cs` | Dynamic arc height, adaptive endpoints |
| `Assets/Scripts/Game/Puzzle/PourAnimator.cs` | Calculate mouth + surface positions each frame, pass to stream |
| `Assets/Scripts/Game/Puzzle/BottleContainerView.cs` | May need to expose bottle sprite dimensions for mouth calculation |

### Performance Considerations

- `Transform.TransformPoint()` is cheap — fine to call every frame
- Bezier curve with 10 points is already allocated — no new allocations needed
- Dynamic arc height is just a float lerp — negligible cost

### Project Structure Notes

- `PourStreamVFX` is a singleton instance created by `GameplayManager` — no changes to lifecycle
- `PourAnimator` is static — mouth calculation helper can be a private static method inside PourAnimator
- Bottle sprite dimensions may need to be exposed from `BottleContainerView` or passed as parameter

### References

- [Source: Assets/Scripts/Game/Puzzle/PourStreamVFX.cs] — Current Bezier stream implementation
- [Source: Assets/Scripts/Game/Puzzle/PourAnimator.cs:82-88] — Current fixed stream position offsets
- [Source: Assets/Scripts/Game/Puzzle/PourAnimator.cs:122-127] — Current stream update in Phase 3
- [Source: Assets/Scripts/Game/Puzzle/BottleContainerView.cs] — Bottle hierarchy, sprite dimensions
- [Source: Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs:16] — DefaultMaxVisualFill = 0.80

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- `GetBottleMouthWorldPos()` uses `TransformPoint()` to track bottle mouth through all tilt angles (15°–105°)
- `GetTargetSurfaceWorldPos()` computes liquid surface from `BottleBottomRatio + fillRatio * MaxVisualFill * spriteHeight`
- Target fill ratio lerped each frame: `Lerp(tgtPreFillRatio, tgtPostFillRatio, t)` — stream endpoint rises as target fills
- PourStreamVFX dynamic arc: `Lerp(0.1, 0.5, streamLength / 4.0)` — tight for short pours, wide for long
- Gravity bias (0.15) shifts Bezier control point toward source for natural arc-then-fall trajectory
- Stream width scales with pour ratio: `Lerp(0.7, 1.3, pourRatio)` * base widths
- `StartStream()` now takes `pourRatio` parameter — backward-compatible via GameplayManager (always passes through PourAnimator)
- No changes to GameplayManager — all stream logic contained in PourAnimator + PourStreamVFX

### Change Log

- 2026-03-23: Pour stream rework — mouth origin, adaptive surface target, dynamic arc, width scaling (Tasks 1-5)

### File List

- `Assets/Scripts/Game/Puzzle/PourStreamVFX.cs` — Modified: dynamic arc height, gravity bias, width scaling via pourRatio, removed fixed ArcHeight
- `Assets/Scripts/Game/Puzzle/PourAnimator.cs` — Modified: added GetBottleMouthWorldPos(), GetTargetSurfaceWorldPos(), stream position constants, per-frame mouth/surface tracking
