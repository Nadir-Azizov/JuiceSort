# Story 3.4: Star Gate Unlock

Status: done

## Story

As a player,
I want to need ~80% of possible stars to unlock the next batch of 50 levels,
so that I'm motivated to replay levels for better ratings.

## Acceptance Criteria

1. **Batch system** — Levels grouped in batches of 50 (GameConstants.LevelsPerBatch)
2. **Star gate check** — At level 50, 100, 150, etc., player needs 80% of possible stars
3. **Gate blocks progression** — If stars insufficient, NextLevel shows a gate screen instead of loading next level
4. **Gate screen shows deficit** — "Need X more stars to unlock next batch" with total/required counts
5. **Gate screen shows roadmap** — Placeholder scrollable list of completed levels (level number, city, stars) so player can pick one to replay
6. **Replay from gate** — Player can tap any completed level to replay it and earn more stars
7. **Gate passes automatically** — If stars sufficient after replay, NextLevel proceeds normally
8. **Gate auto-check after replay** — After completing a replay, if gate condition now met, offer to continue

## Tasks / Subtasks

- [x] Task 1: Add batch gate logic to ProgressionManager (AC: 1, 2, 7)
  - [x] 1.1 Add `IsAtBatchGate(int levelNumber)` — returns true if level % LevelsPerBatch == 0
  - [x] 1.2 Add `CanPassBatchGate()` — checks total stars in current batch >= required
  - [x] 1.3 Add `GetCurrentBatchNumber()` — returns (CurrentLevel - 1) / LevelsPerBatch + 1
  - [x] 1.4 Add `GetBatchStarCount(int batchNumber)` — sums stars for levels in that batch
  - [x] 1.5 Add `GetBatchRequiredStars()` — returns LevelsPerBatch * StarsPerLevel * StarGatePercent (120)

- [x] Task 2: Create placeholder gate screen with level list (AC: 3, 4, 5, 6)
  - [x] 2.1 In GameplayManager.NextLevel, check if current level is batch boundary AND gate fails
  - [x] 2.2 If gate blocked → show gate overlay instead of loading next level
  - [x] 2.3 Gate overlay shows: "Batch Gate — Need X more stars" text
  - [x] 2.4 Gate overlay shows scrollable list of completed levels from GetAllLevelRecords: "Level 1 - Paris ★★☆" format
  - [x] 2.5 Each level entry is a button — tap to call LoadSpecificLevel (from Story 3.5)
  - [x] 2.6 Use Unity UI ScrollRect + VerticalLayoutGroup for the level list
  - [x] 2.7 This is placeholder — Epic 4 builds the proper visual roadmap

- [x] Task 3: Gate re-check after replay (AC: 7, 8)
  - [x] 3.1 After completing a replay while gate is active, re-check CanPassBatchGate
  - [x] 3.2 If now passing → show "Gate Unlocked! Continue?" button
  - [x] 3.3 If still not passing → return to gate screen with updated star counts

- [x] Task 4: Write tests (AC: all)
  - [x] 4.1 Test batch boundaries: level 50, 100, 150 are gates
  - [x] 4.2 Test gate pass with 120+ stars in batch
  - [x] 4.3 Test gate block with <120 stars
  - [x] 4.4 Test GetBatchStarCount sums correctly
  - [x] 4.5 Test gate re-check after star improvement

## Dev Notes

### Gate flow
```
Complete level 50 → OnLevelComplete
  → ProgressionManager.CompleteLevelWithStars
  → NextLevel button pressed
  → Check: IsAtBatchGate(50) = true
  → Check: CanPassBatchGate() = false (only 90 stars, need 120)
  → Show gate screen with level list
  → Player taps "Level 12 - Tokyo ★☆☆"
  → LoadSpecificLevel(12) — replay
  → Complete with ★★★ (gained 2 stars)
  → OnLevelComplete → check gate again
  → Still blocked? → return to gate screen
  → Now passing? → "Gate Unlocked!" → proceed to level 51
```

### Placeholder level list UI

Use ScrollRect with VerticalLayoutGroup. Each entry is a horizontal panel:
```
[Level 1] [Paris, France] [Morning] [★★☆]  [Play]
[Level 2] [Tokyo, Japan]  [Night]   [★★★]
...
```

This is the data-driven foundation that Epic 4's visual roadmap will replace.

### Batch math
- Batch 1: levels 1-50, gate after level 50
- Max stars: 50 × 3 = 150
- Required: ceil(150 × 0.8) = 120

### References

- [Source: _bmad-output/gdd.md#Star Gate System] — 80% threshold, batches of 50
- [Source: _bmad-output/gdd.md#Replayability] — "Replaying is the primary way to earn stars needed for batch gates"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- ProgressionData.GetBatchStarCount: sums stars for levels in a batch range
- ProgressionManager: IsAtBatchGate, CanPassBatchGate, GetCurrentBatchStars, GetBatchRequiredStars
- GameplayManager.NextLevel: checks gate before proceeding, shows gate screen if blocked
- Gate screen: dark overlay with star deficit text + scrollable level list using ScrollRect/VerticalLayoutGroup
- Each level entry shows "Level N - CityName (Mood) ★★☆" with Play button
- Play button calls LoadSpecificLevel (Story 3.5 integration)
- 9 tests: batch boundaries, required stars (120), batch star counting, gate pass/block, re-check after improvement

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/StarGateTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionData.cs` — added GetBatchStarCount
- `src/JuiceSort/Assets/Scripts/Game/Progression/IProgressionManager.cs` — added gate methods
- `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionManager.cs` — implemented gate methods
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — gate check in NextLevel, ShowGateScreen, CreateGateLevelList, LoadSpecificLevel, _isReplay flag
