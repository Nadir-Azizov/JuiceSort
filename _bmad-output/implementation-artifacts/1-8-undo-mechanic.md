# Story 1.8: Undo Mechanic

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to undo my last pour (limited uses),
so that I can recover from mistakes without restarting the entire level.

## Acceptance Criteria

1. **Undo reverses last pour** — Pressing undo restores the puzzle to the state before the most recent pour
2. **Limited undo count** — Undo is limited to 3 uses per level (GameConstants.MaxUndo)
3. **Undo counter visible** — Remaining undo count is displayed on screen (placeholder UI)
4. **Undo unavailable when empty** — When no undos remain or no pours have been made, undo does nothing
5. **Move counter decrements** — Undo reduces the move count by 1
6. **Visuals refresh** — All container visuals update after undo to reflect restored state
7. **Multiple sequential undos** — Player can undo multiple times up to the limit (reverting multiple pours)

## Tasks / Subtasks

- [x] Task 1: Create UndoStack data structure (AC: 1, 2, 7)
  - [x] 1.1 Created UndoStack.cs with circular buffer
  - [x] 1.2 Constructor takes capacity (MaxUndo = 3)
  - [x] 1.3 Push: overwrites oldest when full (silent drop)
  - [x] 1.4 Pop: returns most recent or null
  - [x] 1.5 Count property
  - [x] 1.6 Clear removes all
  - [x] 1.7 Array + head pointer, no allocations

- [x] Task 2: Integrate undo into GameplayManager (AC: 1, 2, 4, 5, 6)
  - [x] 2.1 _undoStack initialized with MaxUndo capacity
  - [x] 2.2 AttemptPour pushes Clone before pour (pops on failure)
  - [x] 2.3 Undo() pops, replaces _currentPuzzle, rebinds views, decrements moveCount
  - [x] 2.4 Guard: empty stack → no-op
  - [x] 2.5 UndoRemaining property exposed

- [x] Task 3: Add placeholder undo button (AC: 3)
  - [x] 3.1 CreateUndoButton() with "Undo (X)" text
  - [x] 3.2 Button.onClick wired to Undo()
  - [x] 3.3 UpdateUndoButton() called after pour and undo
  - [x] 3.4 Placeholder — Epic 4 replaces

- [x] Task 4: Write EditMode tests (AC: all)
  - [x] 4.1 UndoStackTests: push/pop, capacity overflow, clear, empty pop, LIFO order (6 tests)
  - [x] 4.2 Undo restores previous state (pour then undo)
  - [x] 4.3 Undo decrements move counter
  - [x] 4.4 Undo empty → no change
  - [x] 4.5 3 pours, 3 undos → back to original
  - [x] 4.6 Overflow: 4 pours capacity 3 → oldest lost

## Dev Notes

### Previous Story Intelligence

**PuzzleState.Clone() already exists** — creates a deep copy of all containers. Implemented in Story 1.1.

**GameConstants.MaxUndo = 3** — already defined in Story 1.1.

**Current AttemptPour flow:**
```csharp
private void AttemptPour(int sourceIndex, int targetIndex)
{
    bool success = PuzzleEngine.ExecutePour(_currentPuzzle, sourceIndex, targetIndex);
    if (success)
    {
        _moveCount++;
        refresh views;
        DeselectCurrent();
    }
}
```
Add BEFORE ExecutePour: `_undoStack.Push(_currentPuzzle.Clone());`

**Undo flow:**
```
Undo():
  if stack empty → return
  _currentPuzzle = _undoStack.Pop()
  _moveCount--
  refresh ALL container views
  update undo button text
```

### Architecture — UndoStack Design

Per project-context.md: "Undo stack is fixed at 3 — when full, oldest snapshot drops silently (no error)"

Use a circular buffer approach:
```csharp
private readonly PuzzleState[] _buffer;
private int _head;  // next write position
private int _count; // current items
```
Push overwrites oldest when full. Pop returns most recent. No allocations during gameplay — just reference swaps.

### Scope Boundaries — DO NOT IMPLEMENT

- NO undo button in proper HUD (Epic 4, Story 4.4) — just a placeholder button
- NO difficulty-scaled undo counts (Epic 2) — hardcoded to MaxUndo=3
- NO undo animation — just instant state restore

### References

- [Source: _bmad-output/gdd.md#Game Mechanics] — "Undo: Goes back one pour at a time, limited uses"
- [Source: _bmad-output/game-architecture.md#Architectural Decisions] — "Immutable state snapshots (fixed stack of 3) for undo"
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — "Undo stack is fixed at 3 — when full, oldest snapshot drops silently"
- [Source: _bmad-output/project-context.md#Testing Rules] — "UndoStack — push, pop, overflow behavior"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- UndoStack: circular buffer with head pointer, silently drops oldest on overflow, no allocations
- GameplayManager: push Clone before pour, pop on failure, Undo() replaces state and rebinds views
- Added ContainerView.SetData() for rebinding after undo/restart
- Added PuzzleBoardView.RebindPuzzle() to rebind all views to new state
- Placeholder undo button with count display, updates on pour and undo
- 11 EditMode tests: 6 UndoStack unit tests + 5 integration tests (restore, decrement, empty, sequential, overflow)

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/UndoStack.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/UndoStackTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — undo integration, placeholder button
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerView.cs` — added SetData() for rebinding
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleBoardView.cs` — added RebindPuzzle()
