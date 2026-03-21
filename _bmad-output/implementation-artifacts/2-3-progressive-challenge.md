# Story 2.3: Progressive Challenge

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want each new level to feel slightly more challenging than the last,
so that I feel a sense of progression and mastery.

## Acceptance Criteria

1. **Generated levels replace test puzzle** — GameplayManager uses LevelGenerator + DifficultyScaler instead of TestPuzzleData
2. **Level number tracking** — Game tracks current level number, starting at 1
3. **Next level flow** — After completing a level, player can advance to the next level (level number increments)
4. **Level number displayed** — Current level number is shown on screen (placeholder UI)
5. **Difficulty visibly increases** — Level 1 has fewer containers/colors than level 20+
6. **Consistent replay** — Replaying the same level number produces the same puzzle (seeded PRNG)

## Tasks / Subtasks

- [ ] Task 1: Replace TestPuzzleData with LevelGenerator in GameplayManager (AC: 1, 2)
  - [ ] 1.1 Add `_currentLevelNumber` field (int, starts at 1) to GameplayManager
  - [ ] 1.2 Replace `TestPuzzleData.CreateTestPuzzle()` with `LevelGenerator.Generate(DifficultyScaler.GetLevelDefinition(_currentLevelNumber))`
  - [ ] 1.3 Expose `CurrentLevelNumber` property
  - [ ] 1.4 Keep TestPuzzleData.cs for test use but remove it from gameplay flow

- [ ] Task 2: Implement next level flow with board recreation (AC: 3)
  - [ ] 2.1 Add `NextLevel()` public method to GameplayManager — increments level number, generates new puzzle, resets state
  - [ ] 2.2 Add a placeholder "Next Level" button on the win overlay
  - [ ] 2.3 NextLevel must DESTROY old PuzzleBoardView and CREATE a new one (container count changes per level)
  - [ ] 2.4 Unsubscribe from old board events (OnContainerTapped, OnBackgroundTapped) before destroying
  - [ ] 2.5 Subscribe to new board events after creation
  - [ ] 2.6 Recreate undo/restart buttons on new board (they're children of board Canvas)

- [ ] Task 3: Display level number (AC: 4)
  - [ ] 3.1 Add a placeholder level number text on the gameplay Canvas (e.g., "Level 1")
  - [ ] 3.2 Update text when level changes

- [ ] Task 4: Update RestartLevel for generated puzzles (AC: 6)
  - [ ] 4.1 RestartLevel should regenerate from the same level number (same seed = same puzzle)
  - [ ] 4.2 No need to store _initialPuzzle separately — regeneration produces identical puzzle

- [ ] Task 5: Write tests (AC: all)
  - [ ] 5.1 Test level 1 generates a valid puzzle
  - [ ] 5.2 Test NextLevel increments level number
  - [ ] 5.3 Test same level number produces same puzzle (seeded consistency)
  - [ ] 5.4 Test level 20 has more parameters than level 1

## Dev Notes

### Key Change: GameplayManager.LoadTestPuzzle → LoadLevel

Replace the hardcoded test puzzle with:
```csharp
private void LoadLevel(int levelNumber)
{
    var definition = DifficultyScaler.GetLevelDefinition(levelNumber);
    _currentPuzzle = LevelGenerator.Generate(definition);
    _initialPuzzle = _currentPuzzle.Clone(); // or just regenerate on restart
    // ... rest of setup
}
```

### PuzzleBoardView recreation on NextLevel (CRITICAL)

Container count changes per level. The board MUST be destroyed and recreated:
```
NextLevel():
  _currentLevelNumber++
  // Cleanup old board
  _boardView.OnContainerTapped -= OnContainerTapped
  _boardView.OnBackgroundTapped -= OnBackgroundTapped
  Destroy(_boardView.gameObject)  // destroys Canvas, containers, buttons, overlay
  // Generate and create new board
  LoadLevel(_currentLevelNumber)
```
This also destroys the undo/restart buttons and win overlay (they're children of the board's Canvas), so they get recreated in LoadLevel.

**RestartLevel can keep using RebindPuzzle** since container count doesn't change on restart (same level number = same params). Only NextLevel needs full recreation.

### Scope Boundaries

- NO star rating (Epic 3) — next level is just level++
- NO roadmap UI (Epic 4) — just a button
- NO save/load of current level (Epic 3)

### References

- [Source: _bmad-output/game-architecture.md#Level Generation] — runtime on-demand, seeded by level number
- [Source: _bmad-output/gdd.md#Core Gameplay Loop] — Select Level → Pour & Sort → Complete → Earn Stars → Next Level

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Replaced TestPuzzleData with LevelGenerator+DifficultyScaler in GameplayManager
- LoadLevel(levelNumber) generates puzzle, creates board, wires events, creates UI
- DestroyBoard() handles full cleanup: unsubscribe events, Destroy board GO, null UI refs
- NextLevel: DestroyBoard → LoadLevel(level+1). Board fully recreated for new container count.
- RestartLevel: DestroyBoard → LoadLevel(same level). Same seed = same puzzle.
- Removed _initialPuzzle — regeneration replaces cloning
- Added level number display, Next Level button on win overlay
- 6 tests: valid generation, next level increment, seed consistency, difficulty progression, restart consistency, 30-level batch validation

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ProgressiveChallengeTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — full rewrite: LoadLevel, DestroyBoard, NextLevel, level number display, removed TestPuzzleData dependency
