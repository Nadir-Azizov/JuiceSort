# Story 2.4: Solvability Guarantee

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to never encounter an unsolvable puzzle,
so that I always have a fair chance to complete the level.

## Acceptance Criteria

1. **Solvability verified** — A solver algorithm can confirm that generated puzzles are solvable
2. **100% solvability rate** — All tested generated puzzles (sample of levels) are confirmed solvable
3. **Solver finds a solution** — The solver returns whether a solution exists and the move count of that solution
4. **Algorithm correctness** — The reverse-from-solved generation approach mathematically guarantees solvability

## Tasks / Subtasks

- [ ] Task 1: Create PuzzleSolver (AC: 1, 3)
  - [ ] 1.1 Create `Scripts/Game/LevelGen/PuzzleSolver.cs` — DFS solver with depth limit
  - [ ] 1.2 Method: `Solve(PuzzleState state, int maxDepth)` → returns SolveResult (isSolvable bool, moveCount int)
  - [ ] 1.3 Use DFS with iterative deepening — faster than BFS, avoids OOM on large puzzles
  - [ ] 1.4 State hashing for visited set to avoid revisiting states
  - [ ] 1.5 Default maxDepth = 200 (generous limit — most puzzles solvable in far fewer moves)
  - [ ] 1.6 Optimal move count (for star rating) deferred to Epic 3 — this solver finds *a* solution, not necessarily the shortest

- [ ] Task 2: Write solvability verification tests (AC: 2, 4)
  - [ ] 2.1 Create `Scripts/Tests/EditMode/SolvabilityTests.cs`
  - [ ] 2.2 Test: hand-crafted test puzzle is solvable
  - [ ] 2.3 Test: generated puzzle at level 1 is solvable
  - [ ] 2.4 Test: generated puzzles at levels 1, 5, 10, 20, 50 are all solvable
  - [ ] 2.5 Test: solver returns a positive move count for solvable puzzles
  - [ ] 2.6 Test: solver identifies an intentionally unsolvable state correctly
  - [ ] 2.7 Batch test: generate 20 puzzles across various levels, verify all solvable

## Dev Notes

### DFS Solver Design (not BFS)

BFS finds optimal but explores ALL states — can OOM on puzzles with 6+ containers. DFS with depth limit is much more memory-efficient:

```
Solve(state, maxDepth):
  visited = new HashSet<string>
  return DFS(state, 0, maxDepth, visited)

DFS(state, depth, maxDepth, visited):
  if state.IsAllSorted() → return SolveResult(true, depth)
  if depth >= maxDepth → return SolveResult(false, -1)

  hash = Hash(state)
  if hash in visited → return SolveResult(false, -1)
  visited.Add(hash)

  for each valid pour (source, target):
    newState = state.Clone()
    ExecutePour(newState, source, target)
    result = DFS(newState, depth+1, maxDepth, visited)
    if result.IsSolvable → return result

  return SolveResult(false, -1)
```

**SolveResult struct:**
```csharp
public struct SolveResult
{
    public bool IsSolvable;
    public int MoveCount;  // moves in found solution (not necessarily optimal)
}
```

**Optimal move count deferred:** Epic 3 (star rating) needs optimal move count. At that point, we can either add BFS for small puzzles or estimate optimal from solver's solution (e.g., optimal ≈ solution * 0.6). Not needed now.

**State hashing:** Convert each container's slots to a string or numeric key. Concatenate all containers sorted to handle container-order equivalence (optional optimization).

### Scope Boundaries

- NO optimal move count calculation (deferred to Epic 3)
- Solver is for testing/verification — not used at runtime during gameplay
- Keep solver in Game assembly (uses PuzzleEngine)

### References

- [Source: _bmad-output/gdd.md#Level Design Principles] — "Every puzzle must be solvable"
- [Source: _bmad-output/gdd.md#Level Generation Approach] — "guarantees solvability"
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — "Star rating depends on optimal move count"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- PuzzleSolver: DFS with depth limit (default 200), HashSet<string> for visited states
- State hashing: StringBuilder with slot values separated by commas, containers by pipes
- SolveResult struct: IsSolvable + MoveCount
- 7 tests: hand-crafted solvable, generated level 1, multiple levels, positive move count, unsolvable detection, already-solved (0 moves), batch of 20 levels

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/PuzzleSolver.cs`
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/SolveResult.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/SolvabilityTests.cs`
