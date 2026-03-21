# Story 8.1: Completion Shimmer Ripple

Status: done

## Story

As a player,
I want to see a shimmer ripple across a container when it becomes fully sorted,
so that I get satisfying visual feedback that a bottle is complete.

## Acceptance Criteria

1. **Shimmer trigger** — When the final pour completes and a container becomes fully sorted (all slots same color), a shimmer animation plays on that container
2. **Shimmer visual** — A white semi-transparent highlight sweeps diagonally across the glass from bottom-left to top-right
3. **Shimmer timing** — The sweep takes 0.4s, followed by a 0.2s fade-out (0.6s total)
4. **Gold pulse** — After the shimmer sweep, the bottle frame briefly pulses to gold (0.15s) then settles to a subtle completed tint
5. **Masked to bottle** — The shimmer overlay is clipped to the bottle shape using the existing SpriteMask
6. **Before win check** — The shimmer plays after pour animation completes but before the level-complete screen appears. If this pour completes the level, the win-condition check waits for the shimmer to finish
7. **Multiple completions** — If a pour completes multiple containers (edge case), each plays the shimmer simultaneously
8. **No input blocking** — The shimmer is visual-only and does not block player input (unless it's the final bottle triggering level complete)

## Tasks / Subtasks

- [x] Task 1: Create CompletionShimmer component (AC: 2, 3, 5)
  - [x] 1.1 Create `Scripts/Game/Puzzle/CompletionShimmer.cs` MonoBehaviour
  - [x] 1.2 Add a child SpriteRenderer for the shimmer highlight (white diagonal gradient sprite)
  - [x] 1.3 Implement `PlayShimmer()` coroutine — sweep position from left to right over 0.4s (Linear), then fade alpha to 0 over 0.2s (EaseOutCubic)
  - [x] 1.4 Mask the shimmer sprite using the existing bottle SpriteMask so it clips to the glass shape

- [x] Task 2: Add gold pulse to bottle frame (AC: 4)
  - [x] 2.1 In `BottleContainerView`, add `AnimateCompletionPulse()` coroutine
  - [x] 2.2 Lerp frame color to gold `(1f, 0.88f, 0.3f, 1f)` over 0.15s, then to a subtle completed tint `(0.9f, 0.95f, 1f, 0.95f)` over 0.2s
  - [x] 2.3 Store the completed tint as the new rest color so deselect doesn't revert it

- [x] Task 3: Integrate shimmer into pour completion flow (AC: 1, 6, 7)
  - [x] 3.1 In `GameplayManager`, after pour animation `onComplete`, check which containers are newly sorted
  - [x] 3.2 For each newly sorted container, call `CompletionShimmer.PlayShimmer()` and `AnimateCompletionPulse()`
  - [x] 3.3 If level is complete (all sorted), wait for shimmer coroutines to finish before calling `OnLevelComplete()`
  - [x] 3.4 If level is not complete, allow input immediately (AC: 8)

- [x] Task 4: Create shimmer sprite asset (AC: 2)
  - [x] 4.1 Create a simple white diagonal gradient sprite for the shimmer highlight
  - [x] 4.2 Place in `Resources/Effects/` or similar

## Dev Notes

### Key Files to Modify
- `BottleContainerView.cs` — add `AnimateCompletionPulse()` coroutine and completed tint state
- `GameplayManager.cs` — modify the `onComplete` callback in `AttemptPour()` to detect newly sorted containers and trigger shimmer before win check
- `PourAnimator.cs` — no changes needed (shimmer is triggered after pour completes)

### Key Architecture
- `PourAnimator.Animate()` fires `onComplete` callback → `GameplayManager` checks `_currentPuzzle.IsAllSorted()`
- Container sorting check: `ContainerData.IsSorted()` method
- Bottle visuals use SpriteMask-based rendering — shimmer sprite should use the same mask
- Easing functions `EaseOutCubic` already exist in `BottleContainerView.cs` — reuse them

### Constraints
- No external tween libraries — pure coroutines with manual Lerp + easing
- Shimmer must not interfere with `_isAnimating` flag (which is for pour blocking only)
