# Story 2.2: Difficulty Scaling

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want difficulty parameters to scale based on level number,
so that puzzles get progressively harder as the player advances.

## Acceptance Criteria

1. **Color count scales** — Number of distinct colors increases approximately every 20 levels (4→5→6...)
2. **Container count scales** — Number of containers increases approximately every 10 levels
3. **Slot count scales** — Slots per container increases approximately every 100 levels (4→5→6)
4. **Empty containers** — At least 1 empty container is always provided for maneuvering
5. **DifficultyScaler class** — A plain C# class maps level number to LevelDefinition parameters
6. **Gentle curve** — No sudden difficulty spikes (aligned with Relaxation pillar)
7. **Level 1 is trivial** — First levels use minimum parameters (4 colors, few containers, 4 slots)

## Tasks / Subtasks

- [x] Task 1: Create DifficultyScaler (AC: 1, 2, 3, 4, 5, 6, 7)
  - [x] 1.1 Created DifficultyScaler.cs — static class with constants
  - [x] 1.2 GetLevelDefinition(levelNumber) returns computed LevelDefinition
  - [x] 1.3 Colors: 3 + (level-1)/20, clamped to 5
  - [x] 1.4 Containers: colorCount + 1 + (level-1)/10, clamped to 10 + empty
  - [x] 1.5 Slots: 4 + (level-1)/100, clamped to 6
  - [x] 1.6 Always 1 empty container
  - [x] 1.7 Seed = levelNumber

- [x] Task 2: Write EditMode tests (AC: all)
  - [x] 2.1 Created DifficultyScalerTests.cs — 7 tests
  - [x] 2.2 Level 1 minimum parameters
  - [x] 2.3 Level 21 color count increases
  - [x] 2.4 Level 101 slot count increases
  - [x] 2.5 Parameters never below minimums (tested 1-200)
  - [x] 2.6 Level 1000 clamped at maximums
  - [x] 2.7 Progressive: params never decrease (tested 1-200)

## Dev Notes

### GDD Difficulty Table (initial estimates)

| Parameter | Scaling Rate | Example |
|---|---|---|
| Colors | Every ~20 levels | 3 (L1) → 4 (L20) → 5 (L40) |
| Containers | Every ~10 levels | colorCount+1 at start, +1 every 10 levels |
| Slots/container | Every ~100 levels | 4 (L1-100) → 5 (L100-200) → 6 (L200+) |

These are starting values — will be balanced through playtesting.

### Important: DrinkColor enum has 5 values

MangoAmber, DeepBerry, TropicalTeal, WatermelonRose, LimeGold — max 5 colors available. Color count must clamp at 5 until more are added.

### Scope Boundaries

- NO ScriptableObject config for these values (Story 2.6)
- NO integration with GameplayManager (Story 2.3)
- Hardcoded scaling formulas for now

### References

- [Source: _bmad-output/gdd.md#Difficulty Curve] — scaling rates and example values
- [Source: _bmad-output/gdd.md#Tutorial Integration] — first ~10 levels serve as implicit tutorial

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- DifficultyScaler uses (levelNumber-1) for zero-based step calculation — level 1 gets base values
- Color scaling: 3→4 at level 21, 4→5 at level 41, capped at 5
- Container scaling: always colorCount+1+extra, grows every 10 levels, capped at 10+empty
- Slot scaling: 4→5 at level 101, 5→6 at level 201, capped at 6
- 7 tests including bulk verification across 200 levels for monotonic increase and bounds

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/DifficultyScaler.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/DifficultyScalerTests.cs`
