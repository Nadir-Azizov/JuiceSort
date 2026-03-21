# Story 1.9: Level Restart

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to restart the level at any time,
so that I can try a different approach when I'm stuck without penalty.

## Acceptance Criteria

1. **Restart resets puzzle** — The puzzle returns to its initial state (same configuration as when the level started)
2. **Move counter resets** — Move count goes back to 0
3. **Undo stack clears** — All undo snapshots are cleared on restart
4. **Selection cleared** — Any active container selection is removed
5. **Visuals refresh** — All container visuals update to show the reset state
6. **Level complete state cleared** — If the level was completed, restart allows playing again
7. **Restart button visible** — A restart button is available on screen during gameplay (placeholder UI)
8. **No penalty** — Restart is free and unlimited (GDD: "no lives system, no penalty beyond replaying")

## Tasks / Subtasks

- [x] Task 1: Store initial puzzle state for reset (AC: 1)
  - [x] 1.1 Added `_initialPuzzle` field
  - [x] 1.2 `_initialPuzzle = _currentPuzzle.Clone()` in LoadTestPuzzle

- [x] Task 2: Implement restart logic (AC: 1, 2, 3, 4, 5, 6)
  - [x] 2.1 Added `RestartLevel()` public method
  - [x] 2.2 Clones _initialPuzzle for fresh state
  - [x] 2.3 Resets _moveCount = 0
  - [x] 2.4 Clears undo stack
  - [x] 2.5 Deselects current
  - [x] 2.6 Resets _isLevelComplete = false
  - [x] 2.7 Calls RebindPuzzle (already exists from Story 1.8)
  - [x] 2.8 Destroys win overlay if present

- [x] Task 3: Add placeholder restart button (AC: 7)
  - [x] 3.1 CreateRestartButton() with "Restart" text
  - [x] 3.2 Button.onClick wired to RestartLevel()
  - [x] 3.3 Placeholder — Epic 4 replaces

- [x] Task 4: Write tests (AC: all)
  - [x] 4.1 Restart restores initial state
  - [x] 4.2 Move counter resets
  - [x] 4.3 Undo stack clears
  - [x] 4.4 Restart after level complete allows replay
  - [x] 4.5 Multiple restarts produce fresh independent states
  - [x] 4.6 Initial state not mutated by gameplay

## Dev Notes

### Previous Story Intelligence

**PuzzleState.Clone() exists** — deep copies all containers. Used for undo stack, now also for restart.

**Key challenge: Re-binding views to new data.**
After restart, `_currentPuzzle` is a completely new PuzzleState object (clone of initial). The ContainerViews still reference the OLD ContainerData objects. Need a way to rebind:

Option A: PuzzleBoardView.RefreshAll(PuzzleState) — re-calls Initialize on each ContainerView with new data
Option B: ContainerView.SetData(ContainerData) — replaces the internal _data reference and calls Refresh()

**Option B is simpler** — add a `SetData(ContainerData)` method to ContainerView that replaces `_data` and calls `Refresh()`. PuzzleBoardView gets a `RebindPuzzle(PuzzleState)` method that calls `SetData` on each view.

**Dependencies on previous stories:**
- Story 1.7: `_isLevelComplete` flag — must be reset on restart
- Story 1.8: `_undoStack` — must be cleared on restart
- Story 1.3: `_moveCount` — must be reset
- Story 1.2: selection — must be cleared

### Scope Boundaries — DO NOT IMPLEMENT

- NO proper restart button in HUD (Epic 4)
- NO "are you sure?" confirmation dialog
- NO restart animation

### References

- [Source: _bmad-output/gdd.md#Failure Recovery] — "Restart: player restarts from beginning, no lives system, no penalty"
- [Source: _bmad-output/gdd.md#Controls and Input] — "Restart: Tap restart button, Menu/UI button"
- [Source: _bmad-output/epics.md#Epic 1] — "Level restart" in scope

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- RestartLevel() resets all state: puzzle (from initial clone), moveCount, undoStack, selection, isLevelComplete
- _initialPuzzle stored at load time — cloned on each restart to ensure independence
- Win overlay destroyed on restart so player sees the puzzle again
- Placeholder restart button (dark red, top-right) wired to RestartLevel()
- 6 EditMode tests covering state restoration, counter reset, undo clear, post-complete restart, multiple restarts, initial immutability

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/RestartTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — _initialPuzzle, RestartLevel(), CreateRestartButton(), win overlay tracked for cleanup
