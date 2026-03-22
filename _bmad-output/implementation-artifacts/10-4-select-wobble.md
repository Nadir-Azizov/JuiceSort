# Story 10.4: Select Wobble

Status: done

## Story

As a player,
I want to see the liquid wobble/slosh when I select or deselect a bottle,
so that the liquid feels alive and reactive to my touch.

## Acceptance Criteria

1. **Wobble on select** — Liquid surface oscillates when bottle is selected (damped sine wave)
2. **Wobble on deselect** — Liquid surface oscillates when bottle is deselected (smaller amplitude)
3. **Wobble on pour receive** — Target bottle wobbles when receiving liquid
4. **Damped oscillation** — Wobble starts strong and decays to zero over ~0.5s
5. **Shader-driven** — Wobble uses `_WobbleX` / `_WobbleZ` shader properties (already added in 10-1)
6. **Coroutine-driven** — Impulse triggers coroutine that drives wobble parameters to zero; NOT Update loop
7. **Natural feel** — Wobble frequency ~3-5Hz, amplitude 0.02-0.05, decay factor ~5.0

## Tasks / Subtasks

- [x] Task 1: Add wobble logic to LiquidMaterialController (AC: 5, 6, 7)
  - [x]1.1 Method: `TriggerWobble(float amplitude)` — starts damped oscillation coroutine
  - [x]1.2 Coroutine: `sin(time * frequency) * amplitude * e^(-decay * time)` → sets `_WobbleX`
  - [x]1.3 Default params: frequency=4Hz, amplitude=0.03, decay=5.0, duration=0.5s
  - [x]1.4 Stop previous wobble coroutine before starting new one
  - [x]1.5 At end: snap `_WobbleX` and `_WobbleZ` to 0

- [x] Task 2: Wire wobble triggers (AC: 1, 2, 3)
  - [x]2.1 BottleContainerView.Select() → `_liquidController.TriggerWobble(0.04f)` (strong)
  - [x]2.2 BottleContainerView.Deselect() → `_liquidController.TriggerWobble(0.02f)` (gentle)
  - [x]2.3 PourAnimator: trigger wobble on target bottle after receiving liquid

- [x] Task 3: Update shader to use wobble (AC: 5)
  - [x]3.1 In LiquidFill.shader: offset fill boundary Y position by `_WobbleX * sin(x * waveFreq)`
  - [x]3.2 Creates visible liquid surface deformation

- [x] Task 4: Validate (AC: 4)
  - [x]4.1 Verify wobble decays to zero (no permanent offset)
  - [x]4.2 Verify rapid select/deselect doesn't stack wobbles
  - [x]4.3 Verify wobble looks natural at different fill levels

## Dev Notes

### Dependencies

- **Requires Story 10-1** — `_WobbleX`, `_WobbleZ` shader properties must exist
- LiquidMaterialController must exist

### Damped oscillation formula

```csharp
float wobble = Mathf.Sin(elapsed * frequency * 2f * Mathf.PI)
             * amplitude
             * Mathf.Exp(-decay * elapsed);
_material.SetFloat("_WobbleX", wobble);
```

### What MUST NOT change

- Select/Deselect lift+scale animation unchanged (wobble adds to it)
- Game logic timing unchanged

### References

- [Source: _bmad-output/epics.md#Epic 10] — Story 10.4 specification
- [Source: _bmad-output/game-architecture.md#Liquid Shader System] — WobbleX/WobbleZ parameters

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- TriggerWobble(amplitude) added to LiquidMaterialController — damped sine wave coroutine
- Params: frequency=4Hz, decay=5.0, duration=0.5s — snaps to zero at end
- Stops previous wobble before starting new one (no stacking)
- Select → wobble 0.04 (strong), Deselect → wobble 0.02 (gentle), Pour receive → wobble 0.03
- Shader already has _WobbleX displacement from 10-1 — no shader changes needed
- Task 3 (shader update) was already done in 10-1: `_WobbleX * sin(x * 6.2832)` offsets Y sampling

### File List

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs` — added TriggerWobble, AnimateWobble coroutine
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleContainerView.cs` — Select/Deselect trigger wobble
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PourAnimator.cs` — target wobble after pour

### Change Log

- 2026-03-23: Story implementation complete.
