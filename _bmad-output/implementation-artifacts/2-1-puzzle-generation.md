# Story 2.1: Puzzle Generation

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want the system to generate a solvable puzzle from difficulty parameters,
so that unlimited unique puzzles can be created algorithmically.

## Acceptance Criteria

1. **Reverse-from-solved algorithm** — Generator starts with a solved state (each container holds one color) and applies random reverse-pours to scramble it
2. **Seeded PRNG** — Level number is used as the seed for `System.Random`, ensuring the same level always produces the same puzzle
3. **Parameterized generation** — Generator accepts container count, color count, slot count, and empty container count as parameters
4. **Valid puzzle output** — Generated puzzle has the correct number of containers, colors, slots, and empty containers
5. **LevelDefinition data class** — A plain C# class holds level metadata: level number, container count, color count, slot count, empty count, seed
6. **Minimum shuffle quality** — Generator applies enough reverse-pours to ensure the puzzle is meaningfully scrambled (not trivially close to solved)

## Tasks / Subtasks

- [x] Task 1: Create LevelDefinition data class (AC: 5)
  - [x] 1.1 Created LevelDefinition with LevelNumber, ContainerCount, ColorCount, SlotCount, EmptyContainerCount, Seed properties
  - [x] 1.2 Constructor + FilledContainerCount computed property

- [x] Task 2: Create LevelGenerator with reverse-from-solved algorithm (AC: 1, 2, 3, 4, 6)
  - [x] 2.1 Created LevelGenerator.cs — static class, pure C#
  - [x] 2.2 Generate(LevelDefinition) → PuzzleState
  - [x] 2.3 CreateSolvedState: N containers filled with one color each + empty containers
  - [x] 2.4 System.Random with definition.Seed
  - [x] 2.5 Scramble: random source/target, CanPour check, ExecutePour
  - [x] 2.6 shuffleCount = containerCount * slotCount * 3
  - [x] 2.7 Static ColorPalette array maps indices to DrinkColor enum values

- [x] Task 3: Write EditMode tests (AC: all)
  - [x] 3.1 Created LevelGeneratorTests.cs — 9 tests
  - [x] 3.2 Correct container count
  - [x] 3.3 Total color units preserved (colorCount * slotCount)
  - [x] 3.4 Same seed → identical puzzle (slot-by-slot comparison)
  - [x] 3.5 Different seeds → different puzzles
  - [x] 3.6 Generated puzzle is not solved
  - [x] 3.7 Color units per color = slotCount, distinct colors = colorCount

## Dev Notes

### Architecture

**LevelGenerator as plain C# class:**
- Located in `Scripts/Game/LevelGen/` per architecture doc
- No MonoBehaviour — pure logic, testable in EditMode
- Uses `System.Random` with seed (NOT `UnityEngine.Random`) — critical for deterministic replay

**Reverse-from-solved algorithm:**
```
1. Create N containers of `slotCount` slots each, filled with colors 0..colorCount-1
2. Add `emptyContainerCount` empty containers
3. Seed System.Random with level number
4. For i = 0 to shuffleCount:
   a. Pick random source container (non-empty)
   b. Pick random target container (different from source, not full)
   c. If PuzzleEngine.CanPour → execute pour
5. Return scrambled PuzzleState
```

**DrinkColor mapping:** For generation, map color indices to DrinkColor enum values. Colors 0→MangoAmber, 1→DeepBerry, 2→TropicalTeal, 3→WatermelonRose, 4→LimeGold. Need a helper to map int → DrinkColor.

**Shuffle quality:** The number of shuffle steps should be enough to produce a non-trivial puzzle. A good heuristic: `containerCount * slotCount * 3`. This creates enough randomization without being excessive.

### Scope Boundaries — DO NOT IMPLEMENT

- NO difficulty scaling logic (Story 2.2) — this story takes hardcoded params
- NO integration with GameplayManager (Story 2.3) — this is standalone generation
- NO solvability verification (Story 2.4) — guaranteed by algorithm design
- NO city/mood assignment (Story 2.5)
- NO ScriptableObject config (Story 2.6)

### References

- [Source: _bmad-output/game-architecture.md#Level Generation] — reverse-from-solved, seeded PRNG, runtime on-demand
- [Source: _bmad-output/gdd.md#Level Generation Approach] — "Start from solved state, apply random valid reverse-pours"
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — "Level seed must produce identical puzzle on replay — use System.Random with seed, not UnityEngine.Random"
- [Source: _bmad-output/game-architecture.md#Project Structure] — Scripts/Game/LevelGen/ folder

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- LevelDefinition: plain C# with all generation params, Seed = LevelNumber, FilledContainerCount computed
- LevelGenerator: static class, CreateSolvedState fills N containers with one color each, Scramble applies random valid pours using System.Random(seed)
- ColorPalette static array maps int indices to DrinkColor enum values (supports up to 5 colors)
- Shuffle count heuristic: containerCount * slotCount * 3
- 9 tests covering: container count, color unit preservation, seed determinism, different seeds, not-solved, minimal puzzle

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/LevelDefinition.cs`
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/LevelGenerator.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/LevelGeneratorTests.cs`
