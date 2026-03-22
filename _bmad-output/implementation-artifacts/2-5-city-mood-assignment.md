# Story 2.5: City & Mood Assignment

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want each level to be assigned a random city and mood,
so that levels have thematic variety for the visual layer (Epic 5).

## Acceptance Criteria

1. **City assigned per level** — Each level has a city name and country from the 38-city list
2. **Mood assigned per level** — Each level is either "morning" or "night"
3. **Deterministic assignment** — Same level number always gets the same city and mood (seeded)
4. **All cities used** — Cities cycle/distribute across levels so all 38 cities appear
5. **LevelDefinition extended** — LevelDefinition includes CityName, CountryName, and Mood fields

## Tasks / Subtasks

- [x] Task 1: Define city data (AC: 1)
  - [x] 1.1 Create `Scripts/Game/LevelGen/CityData.cs` — plain C# class with CityName, CountryName fields
  - [x] 1.2 Create `Scripts/Game/LevelGen/CityDatabase.cs` — static class with a hardcoded list of 38 cities (name + country pairs)

- [x] Task 2: Create CityAssigner (AC: 1, 2, 3, 4)
  - [x] 2.1 Create `Scripts/Game/LevelGen/CityAssigner.cs` — plain C# class
  - [x] 2.2 Implement `AssignCity(int levelNumber)` — returns CityData using `levelNumber % 38` to cycle through cities
  - [x] 2.3 Implement `AssignMood(int levelNumber)` — returns "Morning" or "Night" using seeded random (alternating or random per level)

- [x] Task 3: Extend LevelDefinition (AC: 5)
  - [x] 3.1 Add `CityName`, `CountryName`, `Mood` (enum: Morning/Night) fields to LevelDefinition
  - [x] 3.2 CityAssigner populates city/mood on LevelDefinition AFTER DifficultyScaler creates it — keep CityAssigner independent from DifficultyScaler to avoid circular dependency
  - [x] 3.3 GameplayManager (or a future LevelLoader) calls both: DifficultyScaler for params, then CityAssigner for theme

- [x] Task 4: Write tests (AC: all)
  - [x] 4.1 Test city assignment is deterministic (same level → same city)
  - [x] 4.2 Test all 38 cities appear across levels 1-38
  - [x] 4.3 Test mood assignment is deterministic
  - [x] 4.4 Test LevelDefinition has city/mood populated

## Dev Notes

### 38 Cities List (from GDD)

Use well-known world cities. Examples: Paris, Tokyo, New York, Rio de Janeiro, Sydney, Cairo, London, Bangkok, Rome, Istanbul, Dubai, Barcelona, Mumbai, San Francisco, Berlin, Amsterdam, Cape Town, Buenos Aires, Seoul, Mexico City, Vienna, Prague, Lisbon, Singapore, Marrakech, Havana, Athens, Stockholm, Dublin, Vancouver, Montreal, Kyoto, Florence, Santorini, Bali, Reykjavik, Cartagena, Zanzibar.

### Mood enum

```csharp
public enum LevelMood { Morning, Night }
```

### City data is visual metadata only — no gameplay effect

Per GDD: "cities are visual themes, not gameplay-affecting". The assignment is purely for Epic 5's visual layer.

### References

- [Source: _bmad-output/gdd.md#Level Structure] — "Game assigns each level to a city and mood"
- [Source: _bmad-output/gdd.md#Art and Audio Direction] — 38 cities × 2 moods = 76 environments
- [Source: _bmad-output/game-architecture.md#Data Architecture] — City definitions as config data

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- CityDatabase: 38 world cities as static readonly array
- CityAssigner: deterministic assignment via (level-1) % 38 for city, odd/even for mood
- LevelMood enum: Morning, Night
- CityData: plain C# class with CityName, CountryName
- LevelDefinition extended with settable CityName, CountryName, Mood — populated independently from DifficultyScaler
- 7 tests: deterministic city/mood, all 38 cities appear, cycling, alternating mood, database count

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/LevelMood.cs`
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/CityData.cs`
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/CityDatabase.cs`
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/CityAssigner.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/CityAssignerTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/LevelDefinition.cs` — added CityName, CountryName, Mood fields
