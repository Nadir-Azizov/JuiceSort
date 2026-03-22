# Story 11.5: Extra Bottle Re-Layout Animation

Status: done

## Story

As a player,
I want all bottles to smoothly rearrange when an extra bottle is added mid-level,
so that the new bottle integrates naturally without covering the screen or breaking the layout.

## Priority

MEDIUM — currently the extra bottle appears at an inconsistent position and scale, sometimes covering gameplay.

## Acceptance Criteria

1. **Re-layout on add** — When an extra bottle is added, ALL bottle positions and scales are recalculated using `ResponsiveLayoutManager` (same logic as level start)
2. **Smooth animation** — Existing bottles animate to their new positions over 0.3s (not instant snap)
3. **Pop-in effect** — The new bottle appears with a scale-from-zero animation (0 → full scale over 0.2s, EaseOutBack)
4. **Correct scale** — After re-layout, all bottles (old + new) share the same uniform scale
5. **Row transition** — If adding a bottle crosses the row threshold (6 → 7), bottles smoothly transition from 1-row to 2-row layout
6. **No overlap** — During and after animation, no bottles overlap with each other or with HUD bars
7. **Input blocked** — Player input is blocked during the re-layout animation (same `_isAnimating` pattern as pour)
8. **Up to 2 extra bottles** — Works correctly for both the 1st and 2nd extra bottle addition

## Architect Decisions

- **Uses same `ResponsiveLayoutManager.CalculateLayout()`** from Story 11.1 — one algorithm for all cases
- **Row distribution on re-layout:** Top row gets extra on odd count (same as initial layout)
- **Input blocking:** Uses existing `_isAnimating` pattern on GameplayManager

## Tasks / Subtasks

- [x] Task 1: Add re-layout method to BottleBoardView (AC: 1, 4, 5)
  - [x] 1.1 Create `BottleBoardView.RecalculateLayout()` private method
  - [x] 1.2 Call `ResponsiveLayoutManager.CalculateLayout()` with updated bottle count
  - [x] 1.3 Return the new `BottleLayout` (positions + scale) for animation

- [x] Task 2: Animate existing bottles to new positions (AC: 2, 6)
  - [x] 2.1 Create `AnimateRelayout(BottleLayout, BottleContainerView, Action)` coroutine in BottleBoardView
  - [x] 2.2 For each existing bottle, Lerp `localPosition` from current to new target over 0.3s (EaseOutCubic)
  - [x] 2.3 For each existing bottle, Lerp `localScale` from current to new uniform scale over 0.3s
  - [x] 2.4 Run all bottle animations concurrently (single while loop iterating all bottles)

- [x] Task 3: Pop-in animation for new bottle (AC: 3)
  - [x] 3.1 Create new `BottleContainerView` at its target position but with scale = 0
  - [x] 3.2 After existing bottles finish moving (0.3s), animate new bottle scale from 0 to target scale over 0.2s (EaseOutBack)
  - [x] 3.3 Total animation time: 0.3s relayout + 0.2s pop-in = 0.5s

- [x] Task 4: Refactor AddContainerView flow (AC: 1, 7, 8)
  - [x] 4.1 Modify `BottleBoardView.AddContainerView()` to use RecalculateLayout + AnimateRelayout flow
  - [x] 4.2 Old hardcoded spacing/scale logic was already removed in Story 11.1
  - [x] 4.3 `_isAnimating` set true before animation, false in onComplete callback
  - [x] 4.4 `AddContainerView()` accepts `Action onComplete` parameter

- [x] Task 5: Integration with GameplayManager (AC: 7)
  - [x] 5.1 `RequestExtraBottle()` sets `_isAnimating = true` + disables sparkles before calling AddContainerView
  - [x] 5.2 `onComplete` callback re-enables input (`_isAnimating = false`) and sparkles
  - [x] 5.3 Test with both 1st and 2nd extra bottle additions in same level (manual — requires Unity Editor)

## Dev Agent Record

### File List
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleBoardView.cs` — added `RecalculateLayout()`, `AnimateRelayout()` coroutine, refactored `AddContainerView()` with `onComplete` callback, added easing functions
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — updated `RequestExtraBottle()` to block input during animation and re-enable in onComplete

### Change Log
- 2026-03-22: Implemented animated re-layout with coroutine-based Phase 1 (existing bottle reposition 0.3s) + Phase 2 (new bottle pop-in 0.2s). Input blocked via `_isAnimating` pattern.
- 2026-03-22: Code review passed. Known cosmetic note: new bottle's collider/sparkle exists at scale=0 during Phase 1 — no functional impact since `_isAnimating` blocks all input.

## Dev Notes

### Key Files to Modify
- **MODIFY:** `BottleBoardView.cs` — add `RecalculateLayout()`, `AnimateRelayout()`, refactor `AddContainerView()`
- **MODIFY:** `GameplayManager.cs` — update `RequestExtraBottle()` to use animated flow with input blocking
- **DEPENDS ON:** `ResponsiveLayoutManager.cs` from Story 11.1 (must be implemented first)

### Current Extra Bottle Code (to replace)
- `BottleBoardView.AddContainerView()`: hardcoded `bottleSpacing = 1.2f/0.9f`, `scale = 0.8f/0.6f`
- No animation — bottle appears instantly at calculated position
- Doesn't recalculate positions of existing bottles

### Architecture
- Uses the same `ResponsiveLayoutManager.CalculateLayout()` from Story 11.1 — one layout algorithm for all cases
- Animation follows existing coroutine patterns (same as pour animation, selection animation)
- `_isAnimating` flag on GameplayManager blocks all input during animation (existing pattern)
- Easing functions reused from `BottleContainerView.cs` (`EaseOutCubic`, `EaseOutBack`)

### Constraints
- No external tween libraries — pure coroutines with Lerp + easing
- Must preserve existing `PuzzleEngine.AddExtraContainer()` data logic (only visual layer changes)
- Must work with current sprite-based rendering (shader refactor is Epic 10)
- Story 11.1 (ResponsiveLayoutManager) MUST be complete before this story can be implemented
