# Story 1.4: Pour Color Validation

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to only be able to pour onto matching colors or empty containers,
so that the puzzle has clear rules and I can plan my sorting strategy.

## Acceptance Criteria

1. **Matching color required** — A pour is only allowed if the target container's top color matches the source container's top color
2. **Empty target always valid** — Pouring into a completely empty container is always allowed regardless of source color
3. **Mismatched color rejected** — If the target's top color differs from the source's top color, the pour does not execute
4. **No state change on rejection** — When a pour is rejected, neither container's data changes
5. **Player stays selected** — When a pour is rejected due to color mismatch, the source container remains selected (player can try a different target)
6. **Invalid pour is silent** — No error message or visual feedback on rejected pour (GDD: "forgiving" input)

## Tasks / Subtasks

- [x] Task 1: Add color validation to PuzzleEngine (AC: 1, 2, 3, 4)
  - [x] 1.1 Added `CanPour()` — checks source not empty, target not full, target empty OR top colors match
  - [x] 1.2 ExecutePour now calls CanPour as gate — returns false if validation fails
  - [x] 1.3 Removed inline IsEmpty/IsFull from ExecutePour body — consolidated into CanPour

- [x] Task 2: Update GameplayManager for rejected pour behavior (AC: 5, 6)
  - [x] 2.1 AttemptPour only deselects on success — failed pour keeps source selected
  - [x] 2.2 No feedback on rejection — silent per GDD

- [x] Task 3: Write EditMode tests for color validation (AC: all)
  - [x] 3.1 CanPour matching colors → true
  - [x] 3.2 CanPour empty target → true
  - [x] 3.3 CanPour mismatched colors → false
  - [x] 3.4 ExecutePour mismatched → false, no state change (verified all 4 values)
  - [x] 3.5 ExecutePour matching → succeeds
  - [x] 3.6 CanPour empty source → false
  - [x] 3.7 CanPour full target → false

## Dev Notes

### Previous Story Intelligence (Story 1.3)

**Current PuzzleEngine.ExecutePour:**
```csharp
public static bool ExecutePour(PuzzleState state, int sourceIndex, int targetIndex)
{
    var source = state.GetContainer(sourceIndex);
    var target = state.GetContainer(targetIndex);
    if (source.IsEmpty()) return false;
    if (target.IsFull()) return false;
    var color = source.RemoveTop();
    target.AddToTop(color);
    return true;
}
```
The inline empty/full checks need to move into the new `CanPour` method. ExecutePour calls CanPour first, then executes.

**Current GameplayManager.AttemptPour:**
```csharp
private void AttemptPour(int sourceIndex, int targetIndex)
{
    bool success = PuzzleEngine.ExecutePour(_currentPuzzle, sourceIndex, targetIndex);
    if (success) { _moveCount++; refresh views; log; }
    DeselectCurrent();  // ← always deselects
}
```
Change: only deselect on success. On failure, keep source selected.

**Key ContainerData methods available:**
- `GetTopColor()` — returns topmost non-empty color, or None if empty
- `IsEmpty()` — true if all slots are None
- `IsFull()` — true if no slots are None

### Architecture Patterns

**CanPour as validation gate:**
```csharp
public static bool CanPour(PuzzleState state, int sourceIndex, int targetIndex)
{
    var source = state.GetContainer(sourceIndex);
    var target = state.GetContainer(targetIndex);

    if (source.IsEmpty()) return false;
    if (target.IsFull()) return false;
    if (target.IsEmpty()) return true;  // always valid
    return source.GetTopColor() == target.GetTopColor();  // must match
}
```
This consolidates ALL pour validation. Story 1.5 (slot validation) is already covered by `target.IsFull()` — that story will just verify and test this.

### Scope Boundaries — DO NOT IMPLEMENT

- NO deselect by tapping same container or empty space (Story 1.6)
- NO win condition (Story 1.7)
- NO undo (Story 1.8)
- NO visual feedback for invalid pours — just silent rejection

### Cross-Story Dependencies

- Story 1.5 (slot validation) — already covered by IsFull() in CanPour. Story 1.5 will verify this and may add tests but shouldn't need code changes.
- Story 1.7 (win condition) will call IsAllSorted() after successful pours
- Story 1.8 (undo) will save state before ExecutePour

### References

- [Source: _bmad-output/gdd.md#Game Mechanics] — "Can only pour onto a matching color", "Can always pour into an empty container"
- [Source: _bmad-output/gdd.md#Input Feel] — "Forgiving: Tapping an invalid target does nothing"
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — "Pour validation must check: matching color OR empty target, AND available slot"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — no runtime errors encountered.

### Completion Notes List

- Added PuzzleEngine.CanPour() consolidating all validation: empty source, full target, empty target bypass, color matching
- ExecutePour now delegates to CanPour before executing — clean separation of validation and execution
- GameplayManager.AttemptPour changed: only deselects on success, keeps source selected on failure
- 7 new tests added to PuzzleEngineTests.cs covering all CanPour paths and ExecutePour with color validation

### File List

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleEngine.cs` — added CanPour, refactored ExecutePour to use it
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — AttemptPour only deselects on success
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/PuzzleEngineTests.cs` — added 7 color validation tests
