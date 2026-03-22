# Story 10.3: Pour Stream VFX

Status: done

## Story

As a player,
I want to see a flowing liquid stream between bottles during a pour,
so that liquid visibly moves from one bottle to another.

## Acceptance Criteria

1. **Visible stream** — A colored liquid stream connects source bottle mouth to target bottle mouth during pour
2. **Color-matched** — Stream color matches the liquid being poured (from ThemeConfig drink colors)
3. **Animated flow** — Stream appears when pour starts, flows during transfer, disappears when pour ends
4. **Arc trajectory** — Stream follows a natural arc (gravity-influenced curve, not a straight line)
5. **Stream width** — Thin stream (0.05-0.1 world units) that feels like liquid flowing
6. **PourStreamVFX component** — New MonoBehaviour using LineRenderer or particle system for stream rendering
7. **Integration with PourAnimator** — Stream starts at tilt phase, ends before return phase
8. **Performance** — Single LineRenderer or lightweight particle system; no per-frame allocations

## Tasks / Subtasks

- [x] Task 1: Create PourStreamVFX component (AC: 1, 2, 5, 6)
  - [x]1.1 Create `Scripts/Game/Puzzle/PourStreamVFX.cs` — MonoBehaviour
  - [x]1.2 Use LineRenderer with 8-12 points along a bezier/catenary curve
  - [x]1.3 Method: `StartStream(Vector3 sourcePos, Vector3 targetPos, Color color)` — shows stream
  - [x]1.4 Method: `StopStream()` — fades and hides stream
  - [x]1.5 Stream width: start 0.08, end 0.05 (tapers)
  - [x]1.6 Material: simple unlit with alpha, color-tinted

- [x] Task 2: Arc trajectory (AC: 4)
  - [x]2.1 Calculate bezier control point above midpoint for natural arc
  - [x]2.2 Update curve points each frame while stream is active (source/target may move during animation)
  - [x]2.3 Gravity-influenced: control point offset upward from midpoint

- [x] Task 3: Integrate with PourAnimator (AC: 3, 7)
  - [x]3.1 Add `PourStreamVFX pourStream` parameter to `PourAnimator.Animate()` (nullable — pass null to skip stream)
  - [x]3.2 GameplayManager creates one PourStreamVFX instance at level start, reuses across pours
  - [x]3.3 GameplayManager passes the instance when calling `StartCoroutine(PourAnimator.Animate(..., pourStream, ...))`
  - [x]3.4 PourAnimator calls `pourStream.StartStream()` at beginning of pour lerp phase
  - [x]3.5 PourAnimator updates stream positions each frame during pour
  - [x]3.6 PourAnimator calls `pourStream.StopStream()` before return phase begins
  - [x]3.7 Stream source position: near top of source bottle (tilted position)
  - [x]3.8 Stream target position: near top of target bottle

- [x] Task 4: Validate (AC: 8)
  - [x]4.1 Verify no per-frame allocations (profile in Unity)
  - [x]4.2 Test with bottles at various distances and positions
  - [x]4.3 Verify stream cleans up properly (deactivated after pour, reused next pour)
  - [x]4.4 Verify passing null for pourStream skips stream rendering (backward compatible)

## Dev Notes

### Dependencies

- **Requires Story 10-2** — Integrated into refactored PourAnimator
- LineRenderer approach recommended over particle system for precise control

### PourAnimator is static — can't create MonoBehaviours

PourAnimator is a static class. It CANNOT call `Instantiate()`, `AddComponent()`, or create GameObjects. The PourStreamVFX instance must be pre-created and passed in:

```csharp
// GameplayManager creates once:
_pourStream = PourStreamVFX.Create();

// GameplayManager passes to PourAnimator:
StartCoroutine(PourAnimator.Animate(
    source, target, pourCount, pourColor,
    sourceTopIndex, targetFirstEmpty,
    _pourStream,  // <-- pre-created instance
    onMidPour, onComplete));
```

Only one pour happens at a time, so a single reusable instance is sufficient. Activate on StartStream, deactivate on StopStream.

### Architecture

- PourStreamVFX: `Scripts/Game/Puzzle/PourStreamVFX.cs`
- Single reusable instance — managed by GameplayManager
- LineRenderer with gradient material for smooth appearance

### References

- [Source: _bmad-output/epics.md#Epic 10] — Story 10.3 specification
- [Source: _bmad-output/game-architecture.md#Liquid Shader System] — PourStreamVFX component

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- PourStreamVFX: LineRenderer with 10-point quadratic bezier arc, pre-allocated point array
- StartStream/UpdatePositions/StopStream API with fade-out coroutine
- Arc height 0.3 units above midpoint for natural gravity-influenced curve
- Stream width tapers 0.08→0.05, uses Sprites/Default material with color tint
- PourAnimator: added `PourStreamVFX pourStream` parameter (nullable for backward compat)
- Stream starts after tilt, updates each frame during lerp, stops before return phase
- GameplayManager: creates single PourStreamVFX instance on first level load, reuses across pours

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PourStreamVFX.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PourAnimator.cs` — added pourStream parameter, start/update/stop calls
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — creates _pourStream, passes to Animate

### Change Log

- 2026-03-23: Story implementation complete.
