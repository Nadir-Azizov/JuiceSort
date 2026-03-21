# Story 8.2: Screen Transitions

Status: done

## Story

As a player,
I want smooth crossfade transitions between all screens,
so that navigation feels polished and premium rather than jarring instant swaps.

## Acceptance Criteria

1. **Crossfade with slide** — Transitioning between screens uses a crossfade (alpha 1→0 outgoing, 0→1 incoming) combined with a slight vertical slide (20px up for outgoing, 20px below to rest for incoming)
2. **Duration** — Total transition is 0.6s: 0.3s fade-out + 0.3s fade-in
3. **Easing** — EaseInOutCubic for both fade and slide
4. **All screen transitions** — Applies to: Main Menu ↔ Roadmap ↔ Gameplay ↔ Level Complete
5. **Overlay transitions** — Level Complete overlay uses fade-in only (0.3s), no slide
6. **Input blocked** — All input is blocked during transition to prevent double-taps or navigation conflicts
7. **First screen** — The initial screen on app launch appears without transition (instant)

## Tasks / Subtasks

- [x] Task 1: Add transition animation to ScreenManager (AC: 1, 2, 3, 6)
  - [x] 1.1 Add `CanvasGroup` component to each registered screen for alpha control
  - [x] 1.2 Implement `TransitionCoroutine(GameObject outgoing, GameObject incoming)` — fades out + slides up outgoing, then fades in + slides down incoming
  - [x] 1.3 Add `_isTransitioning` flag to block input during transition
  - [x] 1.4 Use EaseInOutCubic easing: `t < 0.5 ? 4*t*t*t : 1 - pow(-2*t+2, 3)/2`

- [x] Task 2: Update TransitionTo() method (AC: 4, 7)
  - [x] 2.1 Modify `ScreenManager.TransitionTo()` to use the new transition coroutine instead of instant show/hide
  - [x] 2.2 Skip transition on first screen load (no outgoing screen)
  - [x] 2.3 Ensure screen GameObjects are activated before fade-in starts

- [x] Task 3: Update overlay transitions (AC: 5)
  - [x] 3.1 Modify `ShowOverlay()` to fade in the overlay CanvasGroup over 0.3s
  - [x] 3.2 Modify `HideOverlay()` to fade out over 0.3s
  - [x] 3.3 No vertical slide for overlays — just alpha

- [x] Task 4: Wire up input blocking (AC: 6)
  - [x] 4.1 Disable raycasting on all CanvasGroups during transition (`blocksRaycasts = false`)
  - [x] 4.2 Re-enable raycasting on the incoming screen after transition completes

## Dev Notes

### Key Files to Modify
- `ScreenManager.cs` — main file: add CanvasGroup management, transition coroutine, input blocking
- Screen classes (`MainMenuScreen.cs`, `RoadmapScreen.cs`, `LevelCompleteScreen.cs`, `StarGateScreen.cs`) — may need CanvasGroup added to their GameObjects

### Key Architecture
- `ScreenManager.TransitionTo(GameFlowState)` is the main navigation method
- `ScreenManager.ShowOverlay(GameFlowState)` / `HideOverlay()` for Level Complete overlay
- Each screen registers with `RegisterScreen()` which maps `GameFlowState → GameObject`
- Navigation flow: MainMenu → Roadmap → Playing → LevelComplete (overlay) → Roadmap or next level

### Constraints
- No external tween libraries — pure coroutines with manual Lerp + easing
- CanvasGroup.alpha controls fade, RectTransform.anchoredPosition controls slide
- Must handle edge case of rapid navigation (transition already in progress)
