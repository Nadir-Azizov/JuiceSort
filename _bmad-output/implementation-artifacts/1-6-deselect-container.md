# Story 1.6: Deselect Container

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to deselect by tapping the same container or empty space,
so that I can change my mind about which container to pour from.

## Acceptance Criteria

1. **Tap same container deselects** — Tapping the currently selected container deselects it (highlight removed, no selection active)
2. **Tap empty space deselects** — Tapping an area that is not a container deselects the current selection
3. **Visual feedback** — Container returns to idle appearance (gray) after deselection
4. **Selection state cleared** — SelectedContainerIndex returns to -1 after deselection
5. **No action on double-deselect** — If nothing is selected and player taps empty space, nothing happens

## Tasks / Subtasks

- [x] Task 1: Handle tap-same-container deselect (AC: 1, 3, 4)
  - [x] 1.1 Changed `else if (_selectedContainerIndex == index)` body to call `DeselectCurrent()`
  - [x] 1.2 One-line change in GameplayManager

- [x] Task 2: Handle tap-empty-space deselect (AC: 2, 3, 4, 5)
  - [x] 2.1 Created BackgroundTapCatcher with IPointerClickHandler
  - [x] 2.2 Full-screen transparent Image created in PuzzleBoardView before containers (renders behind)
  - [x] 2.3 BackgroundTapCatcher.OnTapped → PuzzleBoardView.OnBackgroundTapped → GameplayManager.DeselectCurrent()
  - [x] 2.4 Containers render on top — container taps take priority

- [x] Task 3: Write tests (AC: all)
  - [x] 3.1 TapSameContainer_Deselects — selected → tap same → -1
  - [x] 3.2 BackgroundTap_Deselects — selected → background → -1
  - [x] 3.3 BackgroundTap_NothingSelected_NoChange — -1 stays -1

## Dev Notes

### Previous Story Intelligence

**Current GameplayManager.OnContainerTapped has a placeholder:**
```csharp
else if (_selectedContainerIndex == index)
{
    // Tapping same container — do nothing (Story 1.6 handles deselect)
}
```
Change the comment body to `DeselectCurrent();`

**DeselectCurrent() already exists** and works correctly — calls `ContainerView.Deselect()` which restores IdleColor and sets state to Idle.

**For empty-space tap detection:**
- PuzzleBoardView already has a Canvas with GraphicRaycaster
- Add a full-screen background Image (transparent, raycastTarget=true) behind the ContainerArea
- Add IPointerClickHandler to a BackgroundTapCatcher component on that Image
- Wire it to a new `OnBackgroundTapped` event on PuzzleBoardView → GameplayManager.DeselectCurrent()
- Container taps take priority because containers are rendered on top of the background

### Scope Boundaries — DO NOT IMPLEMENT

- NO win condition (Story 1.7)
- NO undo (Story 1.8)
- NO level restart (Story 1.9)

### References

- [Source: _bmad-output/gdd.md#Controls and Input] — "Deselect: Tap selected container again or tap empty space"
- [Source: _bmad-output/gdd.md#Input Feel] — "Forgiving: Tapping an invalid target does nothing"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Tap-same deselect: one-line change in GameplayManager.OnContainerTapped
- Background tap: created BackgroundTapCatcher (IPointerClickHandler) with full-screen transparent Image, created before containers so it renders behind
- Event chain: BackgroundTapCatcher.OnTapped → PuzzleBoardView.OnBackgroundTapped → GameplayManager.OnBackgroundTapped → DeselectCurrent()
- 3 deselect tests added to ContainerSelectionTests.cs

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BackgroundTapCatcher.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — tap-same deselect + background tap handler
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleBoardView.cs` — OnBackgroundTapped event, CreateBackgroundTapCatcher()
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ContainerSelectionTests.cs` — 3 deselect tests
