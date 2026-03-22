# Story 5.2: City Backgrounds

Status: done

## Story

As a player,
I want to see a unique city background with a landmark for each level,
so that each puzzle feels like visiting a new destination.

## Acceptance Criteria

1. **Background behind containers** — A background image/gradient is visible behind the puzzle containers
2. **Morning/night gradient** — Morning levels have warm sunrise gradient, night levels have cool twilight gradient
3. **City-aware** — Background adapts based on the level's assigned city (placeholder: gradient color varies per city)
4. **BackgroundManager** — Component that loads/displays backgrounds, can be upgraded to Addressables later
5. **Background changes per level** — Each new level shows a different background

## Tasks / Subtasks

- [x] Task 1: Create BackgroundManager (AC: 1, 4, 5)
  - [x]1.1 Create `Scripts/Game/UI/Components/BackgroundManager.cs` — manages background display
  - [x]1.2 Creates a full-screen Image behind all gameplay elements (low sorting order)
  - [x]1.3 Method: SetBackground(string cityName, LevelMood mood) — sets gradient based on city+mood
  - [x]1.4 Placeholder: generate gradient colors procedurally from city name hash (each city gets unique warm hues)

- [x] Task 2: Create morning/night gradients (AC: 2, 3)
  - [x]2.1 Morning: warm sunrise gradient (golden top → soft peach bottom)
  - [x]2.2 Night: cool twilight gradient (deep blue top → warm purple bottom)
  - [x]2.3 City variation: shift hue slightly based on city index for variety

- [x] Task 3: Integrate into GameplayManager (AC: 5)
  - [x]3.1 When LoadLevel runs, call BackgroundManager.SetBackground with city/mood from LevelDefinition
  - [x]3.2 Background updates on NextLevel and RestartLevel

- [x] Task 4: Write tests (AC: all)
  - [x]4.1 Test gradient generation produces different colors for morning vs night
  - [x]4.2 Test different cities produce different gradients

## Dev Notes

### Placeholder backgrounds — real city art comes from AI tools

This story creates the FRAMEWORK for backgrounds. Actual city images (76 backgrounds) will be generated with AI art tools and loaded via Addressables. For now, procedural gradients give each level visual variety.

### Future: Addressables integration

Architecture specifies Unity Addressables for 76 backgrounds. This story uses simple Image color/gradient. Epic 5 can add Addressables loading as a follow-up if needed before launch.

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- BackgroundManager: Canvas at sorting order -1, full-screen Image, procedural city gradients via HSV hue shift
- SetBackground(city, mood): uses ThemeConfig gradient colors, shifts hue deterministically per city name hash
- Created lazily in GameplayManager.LoadLevel (persists across levels)
- ThemeConfig.CurrentMood set on each LoadLevel for mood-aware styling

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/BackgroundManager.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — creates BackgroundManager, sets background and ThemeConfig.CurrentMood on LoadLevel
