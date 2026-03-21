# Story 1.2: Container Selection

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to tap a container to select it (visual highlight),
so that I can indicate which container I want to pour from.

## Acceptance Criteria

1. **Tap to select** — Tapping a non-empty container selects it and shows a visual highlight (e.g., outline color change, scale pulse, or glow)
2. **Only one selected** — Only one container can be selected at a time; selecting a new container deselects the previous one
3. **Empty container ignored** — Tapping an empty container does nothing (no selection, no error feedback)
4. **Immediate feedback** — Selection highlight appears instantly on tap with no perceptible delay
5. **Touch input** — Selection uses Unity Input System for touch/click detection on the container's UI element
6. **Selection state accessible** — The currently selected container index (or null/none) is readable by other systems for future pour logic (Story 1.3)

## Tasks / Subtasks

- [x] Task 1: Add ContainerState enum and selection state to ContainerView (AC: 1, 2, 6)
  - [x] 1.1 Created `Scripts/Game/Puzzle/ContainerState.cs` — enum with Idle, Selected, Pouring, Receiving
  - [x] 1.2 Added `_state` field to ContainerView with `Select()` and `Deselect()` methods that change state and call `UpdateHighlight()`
  - [x] 1.3 Added `IsSelected`, `ContainerIndex`, `State`, and `Data` properties to ContainerView

- [x] Task 2: Implement selection visual highlight (AC: 1, 4)
  - [x] 2.1 Selection changes container body Image color: Idle=gray(0.7,0.7,0.7,0.5), Selected=yellow(1.0,0.9,0.4,0.8)
  - [x] 2.2 Deselect() restores IdleColor via UpdateHighlight()
  - [x] 2.3 Immediate color swap — no animation

- [x] Task 3: Implement touch input handler for container taps (AC: 1, 3, 5)
  - [x] 3.1 Added `IPointerClickHandler` directly to ContainerView (no separate TouchInputHandler needed — simpler)
  - [x] 3.2 ContainerView fires `OnTapped` event → PuzzleBoardView relays via `OnContainerTapped` → GameplayManager handles logic
  - [x] 3.3 Empty container check in `GameplayManager.OnContainerTapped()` using `ContainerData.IsEmpty()`

- [x] Task 4: Implement selection logic in GameplayManager (AC: 2, 3, 6)
  - [x] 4.1 Added `_selectedContainerIndex` field (int, -1 = none)
  - [x] 4.2 Implemented OnContainerTapped: empty → ignore, different non-empty → deselect previous + select new
  - [x] 4.3 Exposed `SelectedContainerIndex` property

- [x] Task 5: Wire up PuzzleBoardView to expose container tap events (AC: 5)
  - [x] 5.1 IPointerClickHandler on ContainerView fires OnTapped(containerIndex)
  - [x] 5.2 PuzzleBoardView subscribes to each ContainerView.OnTapped and relays via OnContainerTapped event
  - [x] 5.3 GameplayManager subscribes to `_boardView.OnContainerTapped` in LoadTestPuzzle()

- [x] Task 6: Write EditMode tests (AC: all)
  - [x] 6.1 Created ContainerSelectionTests.cs — 9 tests: select non-empty, select empty ignored, deselect, sequential selection, enum validation, empty-after-non-empty keeps selection
  - [x] 6.2 Tests verify SelectedContainerIndex tracking through selection sequences

## Dev Notes

### Previous Story Intelligence (Story 1.1)

**Key files to modify:**
- `ContainerView.cs` — add state management (ContainerState enum), Select/Deselect methods, visual highlight, IPointerClickHandler
- `PuzzleBoardView.cs` — add OnContainerTapped event, pass container index
- `GameplayManager.cs` — add selection logic, track selected container index

**Key files created in 1.1 (DO NOT recreate):**
- `ContainerData.cs` — has `IsEmpty()` method needed for empty container check
- `DrinkColorMap.cs` — color mapping, not touched by this story
- `PuzzleBoardView.cs` — has `_containerViews` array and `GetContainerView(int)` method
- `GameplayManager.cs` — has `_currentPuzzle` (PuzzleState) and `_boardView` (PuzzleBoardView)

**Patterns established in 1.1:**
- Containers created programmatically via `ContainerView.Create()` factory
- ContainerView has `_data` (ContainerData reference) and `_slotViews` array
- Container body uses `Image` component — can change color for highlight
- PuzzleBoardView creates Canvas with `GraphicRaycaster` (already set up for UI input)
- All UI is uGUI-based (Canvas, Image, RectTransform)

### Architecture Patterns — MUST FOLLOW

**Container State Pattern (from architecture doc):**
```csharp
public enum ContainerState
{
    Idle,
    Selected,
    Pouring,    // stub for Story 1.3
    Receiving   // stub for Story 1.3
}
```
Use enum + switch — NEVER string-based state names.

**Input Handling:**
- Use `IPointerClickHandler` interface on `ContainerView` — this works with Unity UI's EventSystem and the existing `GraphicRaycaster` on the Canvas
- Do NOT use `Input.GetMouseButtonDown()` or raw touch APIs — use the EventSystem
- Invalid taps do nothing — no error feedback to player (GDD: "Forgiving: Tapping an invalid target does nothing")

**Selection Visual:**
- Keep it simple and immediate — change the container body Image color
- Idle: `Color(0.7f, 0.7f, 0.7f, 0.5f)` (current gray)
- Selected: `Color(1.0f, 0.9f, 0.4f, 0.8f)` (bright yellow highlight)
- No animation for now — just swap the color on the Image component
- Future stories may add scale/glow effects

**Communication Pattern:**
- Container tap detection: `IPointerClickHandler` on `ContainerView` (internal within Puzzle system)
- Container → Board: C# event (internal communication, not cross-system)
- Board → GameplayManager: C# event callback (internal within gameplay)
- Do NOT use SO Event Channels for this — it's internal to the Puzzle system

### Project Structure Notes

**New file:**
- `Scripts/Game/Puzzle/ContainerState.cs` — enum only
- `Scripts/Game/Input/TouchInputHandler.cs` — may be folded into ContainerView via IPointerClickHandler instead of separate file (dev agent's choice based on simplicity)

**Modified files:**
- `Scripts/Game/Puzzle/ContainerView.cs` — add state, highlight, IPointerClickHandler
- `Scripts/Game/Puzzle/PuzzleBoardView.cs` — add OnContainerTapped event
- `Scripts/Game/Puzzle/GameplayManager.cs` — add selection tracking and logic

**New test file:**
- `Scripts/Tests/EditMode/ContainerSelectionTests.cs`

### Anti-Patterns — DO NOT

- Do NOT use `FindObjectOfType()` or `GameObject.Find()` to find containers
- Do NOT add `using UnityEngine.InputSystem` for raw input — use EventSystem + IPointerClickHandler
- Do NOT add pour logic (Story 1.3) — only selection
- Do NOT add deselect-by-tapping-empty-space (Story 1.6) — only basic deselect when selecting a different container
- Do NOT add animation or tweening for the highlight — just immediate color swap
- Do NOT create a separate InputManager service — touch handling is internal to the Puzzle UI system

### Scope Boundaries — DO NOT IMPLEMENT

- NO pour mechanic (Story 1.3) — selecting a second container just selects it, doesn't pour
- NO deselect by tapping same container again or empty space (Story 1.6) — basic deselect only when selecting a different container
- NO pour validation (Stories 1.4, 1.5)
- NO win condition (Story 1.7)
- NO undo (Story 1.8)
- NO level restart (Story 1.9)

### Cross-Story Dependencies

- Story 1.3 (pour) will use `SelectedContainerIndex` to determine source container, then call pour logic when a second container is tapped
- Story 1.6 (deselect) will extend `OnContainerTapped` to handle tapping the same container or empty space
- The `ContainerState` enum's `Pouring` and `Receiving` values will be used in Story 1.3

### References

- [Source: _bmad-output/game-architecture.md#State Patterns] — ContainerState enum definition
- [Source: _bmad-output/game-architecture.md#Communication Patterns] — C# events for internal communication
- [Source: _bmad-output/game-architecture.md#Consistency Rules] — enum states, no string-based states
- [Source: _bmad-output/gdd.md#Controls and Input] — tap to select, immediate feedback, forgiving input
- [Source: _bmad-output/project-context.md#Engine-Specific Rules] — Awake for self-init, Start for cross-refs
- [Source: _bmad-output/project-context.md#Performance Rules] — cache GetComponent in Awake, no allocations in hot paths
- [Source: 1-1-container-rendering.md#Dev Agent Record] — ContainerView structure, PuzzleBoardView with GraphicRaycaster

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — no runtime errors encountered.

### Completion Notes List

- Created ContainerState enum (Idle, Selected, Pouring, Receiving stubs)
- Extended ContainerView with: state management, Select()/Deselect() methods, IPointerClickHandler for tap detection, OnTapped event, visual highlight (immediate color swap: gray↔yellow)
- Extended PuzzleBoardView: passes container indices during creation, subscribes to each ContainerView.OnTapped, relays via OnContainerTapped event
- Extended GameplayManager: tracks _selectedContainerIndex (-1=none), OnContainerTapped handler with empty-container guard, DeselectCurrent/SelectContainer helpers
- Chose IPointerClickHandler on ContainerView instead of separate TouchInputHandler — simpler, leverages existing GraphicRaycaster
- ContainerView.Create() now accepts containerIndex parameter
- 10 EditMode tests covering selection logic and ContainerState enum

### Senior Developer Review (AI)

**Review Date:** 2026-03-20
**Review Outcome:** Changes Requested → Fixed
**Findings:** 1 High, 3 Medium, 1 Low — all High/Medium fixed

**Action Items (all resolved):**
- [x] [HIGH] Rewrote ContainerSelectionTests — tests now replicate actual OnContainerTapped logic instead of testing integer assignments
- [x] [MEDIUM] Added EventSystem creation in PuzzleBoardView.CreateCanvas() — required for IPointerClickHandler
- [x] [MEDIUM] Added OnDestroy() to PuzzleBoardView — unsubscribes from ContainerView.OnTapped events
- [x] [MEDIUM] Replaced FindObjectsByType in BootLoader with static `_initialized` flag

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerState.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ContainerSelectionTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerView.cs` — added state, highlight, IPointerClickHandler, OnTapped event, ContainerIndex
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleBoardView.cs` — added OnContainerTapped event, container index passing, ContainerCount property
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — added selection logic, SelectedContainerIndex, OnContainerTapped handler
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — replaced FindObjectsByType with static flag (review fix)
