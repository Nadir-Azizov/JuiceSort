# Story 1.3: Pour Mechanic

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to tap another container to pour liquid,
so that I can move drinks between containers to sort them.

## Acceptance Criteria

1. **Pour execution** — When a source container is selected and a different container is tapped, all consecutive same-color units from the top of the source are moved to the target, limited by available empty slots
2. **Multi-unit pour** — Each pour moves all consecutive same-color layers from the top in one tap (e.g., 2 same-color on top + 2 empty slots in target = 2 units poured)
3. **Target receives liquid** — The poured color appears in the lowest empty slot of the target container
4. **Visuals update** — Both source and target container visuals refresh immediately after the pour
5. **Deselect after pour** — The source container is deselected after a successful pour
6. **Move counter** — A move counter tracks total pours executed (for future star rating in Epic 3)
7. **Pour into empty container** — Pouring into a completely empty container works (color goes to bottom slot)

## Tasks / Subtasks

- [x] Task 1: Add pour data operations to ContainerData (AC: 1, 2, 3, 7)
  - [x] 1.1 Added `RemoveTop()` — finds topmost non-empty via GetTopIndex(), sets to None, returns color
  - [x] 1.2 Added `AddToTop(DrinkColor)` — finds lowest empty via GetFirstEmptyIndex(), sets color, returns bool
  - [x] 1.3 Added `GetFirstEmptyIndex()` — scans bottom-up for first None slot

- [x] Task 2: Create PuzzleEngine with pour logic (AC: 1, 2)
  - [x] 2.1 Created `PuzzleEngine.cs` — static class, no MonoBehaviour, no Unity deps
  - [x] 2.2 Implemented `ExecutePour(state, sourceIndex, targetIndex)` — RemoveTop + AddToTop, returns bool
  - [x] 2.3 No color matching validation — per scope boundaries

- [x] Task 3: Integrate pour into GameplayManager tap flow (AC: 1, 4, 5)
  - [x] 3.1 Rewrote OnContainerTapped: no selection → select non-empty; source selected + different tap → AttemptPour
  - [x] 3.2 After successful pour → refresh both views, deselect
  - [x] 3.3 After failed pour → deselect only (no visual change)
  - [x] 3.4 Tapping same as selected → does nothing

- [x] Task 4: Add move counter (AC: 6)
  - [x] 4.1 Added `_moveCount` field, incremented on successful pour
  - [x] 4.2 Exposed `MoveCount` property
  - [x] 4.3 Debug.Log with move count after each pour

- [x] Task 5: Pour animation (AC: 4)
  - [x] 5.1 Created `PourAnimator.cs` — static class with `IEnumerator Animate()` coroutine
  - [x] 5.2 Phase 1: Lift source bottle 0.4 units (EaseOutCubic, 0.15s)
  - [x] 5.3 Phase 2: Tilt source 25° toward target (EaseOutCubic, 0.12s)
  - [x] 5.4 Phase 3: Slot-by-slot liquid transfer — hide from source, show on target (0.08s per slot)
  - [x] 5.5 Phase 4: Return source to original position (untilt + drop, 0.2s)
  - [x] 5.6 `_isAnimating` flag blocks all input during animation (tap, undo, restart, back, extra bottle)
  - [x] 5.7 Pre-mutation capture: sourceTopIndex and targetFirstEmpty passed as params to avoid stale data
  - [x] 5.8 `DestroyBoard()` resets `_isAnimating` to prevent stuck input lock

- [x] Task 6: Write EditMode tests for pour logic (AC: all)
  - [x] 5.1 Created PuzzleEngineTests.cs — 17 tests covering ExecutePour, RemoveTop, AddToTop, GetFirstEmptyIndex
  - [x] 5.2 Test pour into empty container — verified color at index 0
  - [x] 5.3 Test pour from empty source — returns false, no change
  - [x] 5.4 Test pour to full target — returns false, no change
  - [x] 5.5 Tested RemoveTop/AddToTop independently (7 tests)
  - [x] 5.6 Test move counter logic — increments only on success

## Dev Notes

### Previous Story Intelligence (Stories 1.1, 1.2)

**Key files to modify:**
- `ContainerData.cs` — add RemoveTop(), AddToTop(), GetFirstEmptyIndex()
- `GameplayManager.cs` — change OnContainerTapped to execute pour when source selected, add move counter

**Key files from previous stories (DO NOT recreate):**
- `ContainerView.cs` — has Refresh() method to update visuals after pour, Select()/Deselect() for state
- `PuzzleBoardView.cs` — has GetContainerView(int) to access views for refresh
- `ContainerData.cs` — has GetTopColor(), GetTopIndex(), IsEmpty(), IsFull(), SetSlot()
- `PuzzleState.cs` — has GetContainer(int), Clone()

**Current GameplayManager.OnContainerTapped flow:**
```csharp
public void OnContainerTapped(int index)
{
    var containerData = _currentPuzzle.GetContainer(index);
    if (containerData.IsEmpty()) return;
    if (_selectedContainerIndex != index)
    {
        DeselectCurrent();
        SelectContainer(index);
    }
}
```
This needs to change: when `_selectedContainerIndex >= 0` and `index != _selectedContainerIndex`, execute pour instead of re-selecting.

**New flow:**
```
OnContainerTapped(index):
  if no source selected:
    if container not empty → select it
  else (source IS selected):
    if index == source → do nothing (Story 1.6)
    else → execute pour(source, index), deselect, refresh
```

**Code review learnings from Story 1.2:**
- Tests must exercise actual logic, not just integer assignments
- EventSystem is now created in PuzzleBoardView
- PuzzleBoardView has OnDestroy cleanup for events
- BootLoader uses static flag instead of FindObjectsByType

### Architecture Patterns — MUST FOLLOW

**PuzzleEngine as static class (pure logic):**
- No MonoBehaviour, no Unity dependencies
- All methods take PuzzleState + indices as parameters
- Returns success/failure bool
- Testable in EditMode without scene

**ContainerData slot model (bottom-up indexing):**
- Index 0 = bottom slot, index N-1 = top slot
- RemoveTop: find topmost non-empty (scan down from top), set to None
- AddToTop: find lowest empty (scan up from bottom), set color
- Pour = RemoveTop(source) + AddToTop(target)

**Refresh pattern:**
- After data changes, call `ContainerView.Refresh()` on affected containers
- Refresh reads from ContainerData and updates SlotView colors
- No need to recreate UI — just update colors

### Scope Boundaries — DO NOT IMPLEMENT

- NO color matching validation (Story 1.4) — pour executes regardless of color match
- NO slot availability validation beyond basic full check (Story 1.5)
- NO deselect by tapping same container (Story 1.6) — tapping selected does nothing
- NO deselect by tapping empty space (Story 1.6)
- NO win condition detection (Story 1.7)
- NO undo (Story 1.8)
- NO pour animation (placeholder — just instant visual update)
- NO sound effects (Epic 6)

### Cross-Story Dependencies

- Story 1.4 will add: before ExecutePour, check `source.GetTopColor() == target.GetTopColor() || target.IsEmpty()`
- Story 1.5 will add: before ExecutePour, check `!target.IsFull()`
- Story 1.6 will add: tapping selected container deselects it
- Story 1.7 will add: after each pour, check `PuzzleState.IsAllSorted()` for win
- Story 1.8 will add: before each pour, push `PuzzleState.Clone()` to undo stack

### Anti-Patterns — DO NOT

- Do NOT put pour logic in GameplayManager — keep it in PuzzleEngine (pure C#, testable)
- Do NOT validate color matching in this story
- Do NOT use try-catch around pour logic — it's pure game logic
- Do NOT allocate new objects in the pour hot path — modify existing ContainerData in place
- Do NOT add animation coroutines — just instant refresh for now

### References

- [Source: _bmad-output/gdd.md#Game Mechanics] — pour one unit at a time, core action
- [Source: _bmad-output/game-architecture.md#State Patterns] — ContainerState.Pouring/Receiving for future animation
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — pour validation must check matching color OR empty AND available slot (Stories 1.4+1.5)
- [Source: _bmad-output/project-context.md#Performance Rules] — zero allocations in gameplay loop
- [Source: 1-2-container-selection.md#Senior Developer Review] — tests must exercise real logic, EventSystem exists

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — no runtime errors encountered.

### Completion Notes List

- Added RemoveTop(), AddToTop(), GetFirstEmptyIndex() to ContainerData — all use existing slot scanning patterns
- Created PuzzleEngine as static class with ExecutePour() — pure logic, no Unity deps, fully testable
- Rewrote GameplayManager.OnContainerTapped with 3-branch flow: no selection → select; same tap → ignore; different tap → pour
- Added AttemptPour helper: calls PuzzleEngine, refreshes both views, deselects, increments move counter
- Move counter exposed via MoveCount property for future HUD/star system
- 17 EditMode tests covering all pour operations: RemoveTop (4 tests), AddToTop (3), GetFirstEmptyIndex (3), ExecutePour (5), move counter (1), multiple sequential pours (1)

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleEngine.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/PuzzleEngineTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerData.cs` — added RemoveTop, AddToTop, GetFirstEmptyIndex
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — rewrote OnContainerTapped for pour flow, added move counter
