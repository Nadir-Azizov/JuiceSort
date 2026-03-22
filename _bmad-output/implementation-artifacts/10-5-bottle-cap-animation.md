# Story 10.5: Bottle Cap Animation

Status: done

## Story

As a player,
I want to see a cork/cap close on a bottle when it becomes fully sorted,
so that completion feels celebratory and final.

## Acceptance Criteria

1. **Cap appears on completion** — A cork/cap sprite drops onto the bottle mouth when it becomes fully sorted
2. **Drop + bounce** — Cap drops from above with a bounce ease (overshoot then settle)
3. **Confetti burst** — Small particle burst around the bottle on cap landing
4. **Timing** — Cap animation starts after existing completion shimmer finishes
5. **Cap stays** — Cap remains visible on completed bottles for the rest of the level
6. **Cap removed on undo** — If completion is undone, cap disappears
7. **Procedural cap sprite** — Cap is a simple rounded rectangle, colored to match bottle frame

## Tasks / Subtasks

- [x] Task 1: Create BottleCapAnimation component (AC: 1, 7)
  - [x]1.1 Create `Scripts/Game/Puzzle/BottleCapAnimation.cs` — MonoBehaviour
  - [x]1.2 Create procedural cap sprite (rounded rectangle, ~1/4 bottle width)
  - [x]1.3 Position cap at bottle mouth (top of bottle sprite, above liquid headroom)
  - [x]1.4 Cap initially hidden (disabled SpriteRenderer)

- [x] Task 2: Drop + bounce animation (AC: 2, 4)
  - [x]2.1 Method: `PlayCapClose(Action onComplete)` — coroutine
  - [x]2.2 Cap starts above bottle (0.5 units up), drops to mouth position
  - [x]2.3 EaseOutBounce easing for satisfying landing feel
  - [x]2.4 Duration: 0.3s drop + bounce
  - [x]2.5 Trigger AFTER existing CompletionShimmer finishes (chain callbacks)

- [x] Task 3: Confetti burst (AC: 3)
  - [x]3.1 Spawn 6-10 small colored particles on cap landing
  - [x]3.2 Particles radiate outward with gravity, fade over 0.5s
  - [x]3.3 Colors from ThemeConfig drink palette (match bottle's liquid color)
  - [x]3.4 Cleanup: destroy particles after fade

- [x] Task 4: Integration (AC: 4, 5, 6)
  - [x]4.1 Wire into BottleContainerView.PlayCompletionEffect → shimmer → cap close
  - [x]4.2 Cap stays visible after animation (SpriteRenderer enabled)
  - [x]4.3 On ResetVisualState() or undo: hide cap (disable SpriteRenderer)

- [x] Task 5: Validate
  - [x]5.1 Verify cap appears after shimmer on sorted bottle
  - [x]5.2 Verify cap disappears on undo
  - [x]5.3 Verify cap position respects visual headroom (sits above liquid)

## Dev Notes

### Dependencies

- **Requires Story 10-1** — Visual headroom provides natural space for cap placement
- Existing CompletionShimmer callback chain used for timing

### Cap position

Cap sits at the bottle mouth — above the `_MaxVisualFill` liquid level. The 20% headroom from Story 10-1 provides natural space for the cap.

### Architecture

- BottleCapAnimation: `Scripts/Game/Puzzle/BottleCapAnimation.cs`
- Attached as child of BottleContainerView (like CompletionShimmer and GlassSparkle)
- Created in BottleContainerView.Create()

### References

- [Source: _bmad-output/epics.md#Epic 10] — Story 10.5 specification
- [Source: _bmad-output/visual-direction-tropical-fresh.md#Bottle Cap/Cork Close]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- BottleCapAnimation: procedural cap sprite, EaseOutBounce drop (0.3s), 8-particle confetti burst
- Cap positioned at bottle mouth (spriteHeight * 0.42) — in the 20% headroom zone
- Cap color: cork brown (0.55, 0.35, 0.2)
- Confetti uses bottle's top liquid color, radiates outward with gravity, fades over 0.5s
- Chained into PlayCompletionEffect: shimmer+pulse → cap drop → onComplete
- HideCap() called from ResetVisualState and Refresh undo detection
- Cap cached sprite shared across all bottles

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleCapAnimation.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleContainerView.cs` — added _cap field, Create() instantiation, PlayCompletionEffect chain, ResetVisualState/undo hide

### Change Log

- 2026-03-23: Story implementation complete.
