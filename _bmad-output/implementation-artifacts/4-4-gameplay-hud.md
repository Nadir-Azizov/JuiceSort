# Story 4.4: Gameplay HUD

Status: done

## Story

As a player,
I want to see undo count, move count, and restart button during gameplay,
so that I have the information and tools I need while solving the puzzle.

## Acceptance Criteria

1. **Move counter visible** — Current move count displayed during gameplay
2. **Undo button with count** — Undo button shows remaining undo count, tappable
3. **Restart button** — Restart button visible and tappable
4. **Level info** — Current level number and city name displayed
5. **HUD as separate component** — All HUD elements extracted from GameplayManager into a GameplayHUD class
6. **HUD updates reactively** — Move count and undo count update after each pour/undo

## Tasks / Subtasks

- [x] Task 1: Create GameplayHUD component (AC: 1, 2, 3, 4, 5)
  - [x]1.1 Create `Scripts/Game/UI/Components/GameplayHUD.cs` — MonoBehaviour managing all HUD elements
  - [x]1.2 Creates: move counter text, undo button with count, restart button, level/city info text
  - [x]1.3 Method: Initialize(GameplayManager) — wires button callbacks
  - [x]1.4 Method: UpdateDisplay(int moves, int undoRemaining) — refreshes text
  - [x]1.5 Positioned in top area of screen (above containers)

- [x] Task 2: Extract HUD code from GameplayManager (AC: 5, 6)
  - [x]2.1 Remove CreateUndoButton, CreateRestartButton, CreateLevelNumberDisplay, UpdateUndoButton from GameplayManager
  - [x]2.2 GameplayManager creates GameplayHUD instead, calls UpdateDisplay after pours/undos
  - [x]2.3 Significant reduction in GameplayManager line count

- [x] Task 3: Write tests (AC: all)
  - [x]3.1 Test HUD display formatting

## Dev Notes

### Major GameplayManager cleanup

This story removes ~120 lines of placeholder UI code from GameplayManager and puts it in a proper component. GameplayManager becomes cleaner — just game logic + delegation.

### References

- [Source: _bmad-output/game-architecture.md#Project Structure] — Game/UI/Components/ for reusable widgets

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- GameplayHUD: move counter, undo button with count, restart button, level/city info. Create(boardParent) factory.
- Removed ~100 lines from GameplayManager: CreateUndoButton, CreateRestartButton, CreateLevelNumberDisplay, UpdateUndoButton
- GameplayManager now calls _hud.UpdateDisplay(moves, undoRemaining) and _hud.SetLevelInfo(level, city)
- HUD uses callback pattern: OnUndoPressed/OnRestartPressed wired to GameplayManager methods

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/GameplayHUD.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — removed 4 HUD methods, added _hud field, uses GameplayHUD.Create
