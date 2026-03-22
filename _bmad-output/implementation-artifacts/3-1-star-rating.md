# Story 3.1: Star Rating & Display

Status: done
Merges: 3.2 (star display on level complete screen)

## Story

As a player,
I want to earn 1-3 stars based on my move efficiency and see them on the level complete screen,
so that I have a goal beyond just completing the puzzle and know how well I performed.

## Acceptance Criteria

1. **1 star minimum** — Completing a level always earns at least 1 star
2. **3 star maximum** — Best possible rating is 3 stars
3. **Move efficiency based** — Fewer moves relative to an estimate = more stars
4. **Star calculation** — 1 star = completed, 2 stars = within 150% of estimated optimal, 3 stars = within 120% of estimated optimal
5. **StarCalculator class** — Pure C# class computes star rating from move count and estimated optimal
6. **Optimal move estimation** — Use PuzzleSolver to estimate optimal move count per level (heuristic fallback if solver too slow)
7. **Stars shown on win** — Star rating displayed on level complete overlay as filled/empty star symbols
8. **Move context shown** — Move count and estimated optimal shown alongside stars

## Tasks / Subtasks

- [x] Task 1: Create StarCalculator (AC: 1, 2, 3, 4, 5)
  - [x] 1.1 Create `Scripts/Game/Progression/StarCalculator.cs` — pure C# static class
  - [x] 1.2 Implement `CalculateStars(int moveCount, int estimatedOptimal)` → returns 1, 2, or 3
  - [x] 1.3 Logic: 3 stars if moveCount <= optimal * 1.2, 2 stars if moveCount <= optimal * 1.5, else 1 star
  - [x] 1.4 Guard: always return at least 1, never more than 3
  - [x] 1.5 Add `GetStarText(int stars)` helper → "★★★", "★★☆", "★☆☆"

- [x] Task 2: Create OptimalMoveEstimator (AC: 6)
  - [x] 2.1 Create `Scripts/Game/Progression/OptimalMoveEstimator.cs` — caches results per level
  - [x] 2.2 Try PuzzleSolver first with tight depth limit 30 for speed (avoids UI stutter on complex puzzles)
  - [x] 2.3 Fallback heuristic if solver fails or exceeds limit: estimate = colorCount * slotCount * 2
  - [x] 2.4 Cache via Dictionary<int, int> (levelNumber → optimal moves) to avoid re-solving on replay

- [x] Task 3: Integrate into GameplayManager win overlay (AC: 7, 8)
  - [x] 3.1 In OnLevelComplete, estimate optimal moves and calculate stars
  - [x] 3.2 Update ShowWinOverlay to display star text: "★★☆" format
  - [x] 3.3 Show move count and estimated optimal: "Moves: 12 (Optimal: ~8)"

- [x] Task 4: Write tests (AC: all)
  - [x] 4.1 Create `Scripts/Tests/EditMode/StarCalculatorTests.cs`
  - [x] 4.2 Test: exact optimal moves = 3 stars
  - [x] 4.3 Test: 1.2x optimal = 3 stars (boundary)
  - [x] 4.4 Test: 1.5x optimal = 2 stars (boundary)
  - [x] 4.5 Test: 2x optimal = 1 star
  - [x] 4.6 Test: always returns at least 1, never more than 3
  - [x] 4.7 Test: GetStarText produces correct symbols
  - [x] 4.8 Test: OptimalMoveEstimator returns positive value for level 1

## Dev Notes

### Star Thresholds

| Rating | Condition |
|---|---|
| 3 Stars | moveCount <= estimatedOptimal * 1.2 |
| 2 Stars | moveCount <= estimatedOptimal * 1.5 |
| 1 Star | Level completed (any move count) |

### Scope: NO persistence yet — Stories 3.5 and 3.6 handle save/load

### References

- [Source: _bmad-output/gdd.md#Star Rating System] — move efficiency thresholds
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — "Star rating depends on optimal move count"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- StarCalculator: CalculateStars (3/2/1 based on move efficiency), GetStarText (★★☆ format)
- OptimalMoveEstimator: PuzzleSolver with depth 30, heuristic fallback, Dictionary cache per level
- GameplayManager: OnLevelComplete calculates stars, ShowWinOverlay displays star text + move context
- Added _currentDefinition and _lastStarRating fields to GameplayManager
- 15 tests: star thresholds, boundaries, guards, star text, estimator positive/cache

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Progression/StarCalculator.cs`
- `src/JuiceSort/Assets/Scripts/Game/Progression/OptimalMoveEstimator.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/StarCalculatorTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — star calculation in OnLevelComplete, star display in ShowWinOverlay
