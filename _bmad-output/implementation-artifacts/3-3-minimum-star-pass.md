# Story 3.3: Progression Tracking & Level Records

Status: done

## Story

As a player,
I want my level completions tracked with city, mood, and star ratings,
so that I can see my journey and replay any previous level from a roadmap.

## Acceptance Criteria

1. **1 star guaranteed on completion** — Completing a level always awards at least 1 star
2. **Level record stored** — Each completed level stores: level number, city name, country, mood, stars earned
3. **Best stars kept** — Replaying a level only updates the star rating if the new rating is better
4. **Next level unlocked** — Passing a level unlocks the next sequential level
5. **ProgressionData class** — Tracks current level, all completed level records, total stars
6. **LevelRecord class** — Plain C# class holding per-level metadata for roadmap display
7. **ProgressionManager service** — Registered in Service Locator via IProgressionManager
8. **BootLoader creates service** — BootLoader programmatically creates ProgressionManager and registers it
9. **Level records queryable** — Can retrieve all completed level records for roadmap display (ordered list)

## Tasks / Subtasks

- [x] Task 1: Create LevelRecord data class (AC: 2, 6)
  - [x] 1.1 Create `Scripts/Game/Progression/LevelRecord.cs` — plain C# class
  - [x] 1.2 Fields: LevelNumber (int), CityName (string), CountryName (string), Mood (LevelMood), Stars (int)
  - [x] 1.3 This class is what the roadmap UI (Epic 4) will render per level node

- [x] Task 2: Create IProgressionManager interface (AC: 7)
  - [x] 2.1 Create `Scripts/Game/Progression/IProgressionManager.cs` — in Game assembly (not Core), since it references Game types (LevelRecord, LevelDefinition)
  - [x] 2.2 Methods: CompleteLevelWithStars(int level, int stars, LevelDefinition definition), GetStarRating(int level), GetTotalStars(), GetLevelRecord(int level), GetAllLevelRecords() → List<LevelRecord>, CurrentLevel, HighestCompletedLevel, IsLevelCompleted(int level)
  - [x] 2.3 Note: interface stays in Game because LevelRecord and LevelDefinition are Game types. Core can't reference Game. This is fine — only GameplayManager (also in Game) consumes it.

- [x] Task 3: Create ProgressionData (AC: 3, 5, 9)
  - [x] 3.1 Create `Scripts/Game/Progression/ProgressionData.cs` — plain C# class
  - [x] 3.2 Internal storage: Dictionary<int, LevelRecord> (levelNumber → record)
  - [x] 3.3 CurrentLevel (int) — next level to play
  - [x] 3.4 SetLevelRecord(LevelRecord) — stores record, keeps best stars if record already exists
  - [x] 3.5 GetStarRating(level) → stars from record, 0 if not completed
  - [x] 3.6 GetTotalStars() → sum of all record stars
  - [x] 3.7 GetAllLevelRecords() → List<LevelRecord> ordered by level number
  - [x] 3.8 IsLevelCompleted(level) → true if record exists
  - [x] 3.9 HighestCompletedLevel → max level number in records

- [x] Task 4: Create ProgressionManager (AC: 4, 7, 8)
  - [x] 4.1 Create `Scripts/Game/Progression/ProgressionManager.cs` — MonoBehaviour implementing IProgressionManager
  - [x] 4.2 Holds ProgressionData instance
  - [x] 4.3 CompleteLevelWithStars: creates LevelRecord from LevelDefinition metadata (city, mood), stores in data, advances CurrentLevel if completing current
  - [x] 4.4 Update BootLoader to create ProgressionManager GO and register

- [x] Task 5: Integrate with GameplayManager (AC: 1, 4)
  - [x] 5.1 In OnLevelComplete, call ProgressionManager.CompleteLevelWithStars with level number, stars, and current LevelDefinition (for city/mood metadata)
  - [x] 5.2 In Start, read CurrentLevel from ProgressionManager
  - [x] 5.3 Store current LevelDefinition as field so it's available at win time for record creation

- [x] Task 6: Write tests (AC: all)
  - [x] 6.1 Create `Scripts/Tests/EditMode/ProgressionDataTests.cs`
  - [x] 6.2 Test: SetLevelRecord stores record with city/mood/stars
  - [x] 6.3 Test: SetLevelRecord keeps best stars (3 then 2 → keeps 3)
  - [x] 6.4 Test: SetLevelRecord upgrades stars (1 then 3 → stores 3)
  - [x] 6.5 Test: GetTotalStars sums all ratings
  - [x] 6.6 Test: IsLevelCompleted returns true after completion
  - [x] 6.7 Test: CurrentLevel advances after completing current level
  - [x] 6.8 Test: Completing non-current level doesn't advance CurrentLevel
  - [x] 6.9 Test: GetAllLevelRecords returns ordered list with city/mood data
  - [x] 6.10 Test: LevelRecord preserves city and mood on star upgrade

## Dev Notes

### LevelRecord — the roadmap data unit

Each node on the roadmap (Epic 4) will display:
- Level number
- City name + country
- Morning/Night mood icon
- Stars (1-3 filled, or empty if not completed)

By storing LevelRecords now, Epic 4 just reads `GetAllLevelRecords()` and renders them.

### CompleteLevelWithStars signature

Takes LevelDefinition to extract metadata:
```csharp
public void CompleteLevelWithStars(int levelNumber, int stars, LevelDefinition definition)
{
    var record = new LevelRecord(levelNumber, definition.CityName, definition.CountryName, definition.Mood, stars);
    _data.SetLevelRecord(record);
    if (levelNumber == _data.CurrentLevel)
        _data.CurrentLevel++;
}
```

### Star upgrade preserves city/mood

When replaying, if stars improve, only the Stars field updates — CityName/CountryName/Mood stay the same (they're deterministic by level number anyway).

### Why IProgressionManager is in Game, not Core

The architecture says Core has zero Game dependencies. IProgressionManager references LevelRecord and LevelDefinition (both Game types), so it must stay in Game. Service Locator still works: `Services.Register<IProgressionManager>(pm)` and `Services.Get<IProgressionManager>()` use the Type key — the interface just needs to be accessible to the caller, which is GameplayManager (also in Game).

### BootLoader pattern
```csharp
var pmGo = new GameObject("ProgressionManager");
DontDestroyOnLoad(pmGo);
Services.Register<IProgressionManager>(pmGo.AddComponent<ProgressionManager>());
```
Note: BootLoader is in Game.Boot, so it can reference Game.Progression.IProgressionManager.

### References

- [Source: _bmad-output/game-architecture.md#Service Architecture] — Service Locator
- [Source: _bmad-output/game-architecture.md#Architectural Boundaries] — Core → zero dependencies on Game
- [Source: _bmad-output/game-architecture.md#Data Architecture] — ProgressionData as plain C# runtime state

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- LevelRecord: per-level data class with city/country/mood/stars, TryUpgradeStars keeps best
- IProgressionManager: in Game.Progression (not Core), references Game types
- ProgressionData: Dictionary<int, LevelRecord>, GetAllLevelRecords sorted, CurrentLevel tracking
- ProgressionManager: MonoBehaviour, CompleteLevelWithStars creates LevelRecord from LevelDefinition metadata
- BootLoader updated: creates ProgressionManager GO + registers as IProgressionManager
- GameplayManager: reads CurrentLevel on Start, calls CompleteLevelWithStars on win
- Uses TryGet for graceful degradation if service not registered
- 12 tests covering record storage, star upgrade/downgrade, total stars, ordering, metadata preservation

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Progression/LevelRecord.cs`
- `src/JuiceSort/Assets/Scripts/Game/Progression/IProgressionManager.cs`
- `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionData.cs`
- `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionManager.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ProgressionDataTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates and registers ProgressionManager
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — reads CurrentLevel, calls CompleteLevelWithStars
