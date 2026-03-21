# Story 1.7: Win Condition

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to see the level complete when all containers are sorted,
so that I know I've solved the puzzle and can feel accomplished.

## Acceptance Criteria

1. **Win detection after each pour** — After every successful pour, the system checks if all containers are sorted
2. **Win condition correct** — Level is complete when every container holds only one color or is empty (uses existing `PuzzleState.IsAllSorted()`)
3. **Win feedback displayed** — When win is detected, a visible "Level Complete" message appears on screen
4. **Gameplay paused on win** — After win detection, further container taps are ignored (no more pours or selections)
5. **Move count shown** — The final move count is displayed alongside the win message
6. **Closed bottle** — A bottle that is completely full with the same color (sorted and non-empty) is "closed": it cannot be selected, cannot be poured from, and cannot be poured into. Tapping a closed bottle does nothing.
7. **Closed bottle visual** — Closed bottles have a distinct visual state (e.g., dimmed frame, lid/cap, or reduced opacity) so the player can see they are locked.

## Tasks / Subtasks

- [x] Task 1: Add win check after successful pour (AC: 1, 2)
  - [x] 1.1 Added `_currentPuzzle.IsAllSorted()` check after successful pour in AttemptPour
  - [x] 1.2 Calls `OnLevelComplete()` when all sorted
  - [x] 1.3 No data model changes needed

- [x] Task 2: Implement gameplay pause on win (AC: 4)
  - [x] 2.1 Added `_isLevelComplete` flag and `IsLevelComplete` property
  - [x] 2.2 Early return in OnContainerTapped and OnBackgroundTapped if level complete
  - [x] 2.3 Flag set in OnLevelComplete()

- [x] Task 3: Display win message (AC: 3, 5)
  - [x] 3.1 ShowWinOverlay() creates dark semi-transparent background + centered text
  - [x] 3.2 Uses Unity UI Text with built-in font (LegacyRuntime.ttf)
  - [x] 3.3 Placeholder — Epic 4 replaces
  - [x] 3.4 Debug.Log with move count

- [x] Task 5: Closed bottle logic (AC: 6)
  - [x] 5.1 Added `IsCompleted()` to `ContainerData` — returns `!IsEmpty() && IsSorted()`
  - [x] 5.2 In `GameplayManager.OnContainerTapped`, early return if `containerData.IsCompleted()`
  - [x] 5.3 In `PuzzleEngine.CanPour`, reject pour if `source.IsCompleted()`
  - [x] 5.4 Target already blocked by `IsFull()` check — IsCompleted implies IsFull
  - [x] 5.5 In re-select logic, skip re-select if `tappedData.IsCompleted()`

- [x] Task 6: Closed bottle visual (AC: 7)
  - [x] 6.1 Added `ContainerState.Completed` to the enum
  - [x] 6.2 In `Refresh()`, completed bottles get dimmed liquid (70% RGB) and green-tinted frame
  - [x] 6.3 `OnMouseDown` returns early if `_state == ContainerState.Completed`
  - [x] 6.4 Undo can un-complete a bottle — Refresh resets state back to Idle when no longer completed

- [x] Task 4: Write tests (AC: 1, 2, 4)
  - [x] 4.1 IsAllSorted solved → true
  - [x] 4.2 IsAllSorted unsolved → false
  - [x] 4.3 Solve simple 2-color puzzle via pours → IsAllSorted becomes true
  - [x] 4.4 LevelComplete flag blocks further interaction

## Dev Notes

### Previous Story Intelligence

**PuzzleState.IsAllSorted() already exists** — implemented in Story 1.1, tested in PuzzleStateTests. It checks every container's `IsSorted()` (all same color or empty).

**GameplayManager.AttemptPour currently:**
```csharp
if (success)
{
    _moveCount++;
    refresh views;
    DeselectCurrent();
    Debug.Log(...);
}
```
Add after deselect: `if (_currentPuzzle.IsAllSorted()) OnLevelComplete();`

**For the win UI overlay:**
- PuzzleBoardView already creates a Canvas — the win overlay can be a sibling panel on the same Canvas, or a new overlay Canvas
- Keep it simple: a dark semi-transparent background + white "Level Complete!" text + "Moves: X"
- This is temporary — Epic 4 replaces it with a proper screen

### Scope Boundaries — DO NOT IMPLEMENT

- NO star rating calculation (Epic 3)
- NO "next level" button (Epic 4)
- NO replay option (Epic 3)
- NO celebration animation or sound (Epics 5, 6)
- NO proper LevelCompleteScreen UI (Epic 4, Story 4.5)

### References

- [Source: _bmad-output/gdd.md#Win/Loss Conditions] — "Level Win: All drinks sorted into matching containers"
- [Source: _bmad-output/game-architecture.md#State Patterns] — GameFlowState.LevelComplete
- [Source: _bmad-output/epics.md#Epic 1] — "Win condition detection (all containers sorted)"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Win detection: IsAllSorted() called after every successful pour in AttemptPour
- Gameplay pause: _isLevelComplete flag blocks all taps (containers + background)
- Placeholder win UI: dark overlay + "Level Complete! Moves: X" using Unity UI Text
- 4 tests including solving a 2-color puzzle through pours and verifying level complete flag blocks interaction

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/WinConditionTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — win detection, _isLevelComplete flag, ShowWinOverlay()
