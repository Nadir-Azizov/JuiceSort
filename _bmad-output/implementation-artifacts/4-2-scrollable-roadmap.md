# Story 4.2: Scrollable Roadmap & Level Start

Status: done
Merges: 4.3 (tap to start level), 5.5 (roadmap visual journey)

## Story

As a player,
I want to view my progress on a scrollable roadmap and tap levels to play them,
so that I can see my journey and choose what to play next.

## Acceptance Criteria

1. **Scrollable level list** — Vertical ScrollRect showing all completed levels plus the next available level
2. **Level node info** — Each node shows: level number, city name, country, mood indicator, star rating
3. **Current level highlighted** — Next playable level is visually distinct (highlighted color)
4. **Tap completed level → replay** — Tapping a completed level loads it for replay
5. **Tap next level → starts game** — Tapping the current level starts it
6. **Auto-scroll to current** — Roadmap auto-scrolls to show the current level
7. **Back to menu** — Button to return to main menu
8. **Shared LevelListView** — Reusable level list component shared with StarGateScreen (Story 4.5)

## Tasks / Subtasks

- [ ] Task 1: Create LevelListView shared component (AC: 8)
  - [ ] 1.1 Create `Scripts/Game/UI/Components/LevelListView.cs` — reusable scrollable level list
  - [ ] 1.2 Method: Populate(List<LevelNodeData> nodes) — creates ScrollRect + entries
  - [ ] 1.3 LevelNodeData: levelNumber, cityName, countryName, mood, stars, isCurrentLevel, isCompleted
  - [ ] 1.4 Each entry: "Level N — City, Country [☀/🌙] ★★☆ [Play]"
  - [ ] 1.5 Current level entry has highlighted background
  - [ ] 1.6 Event: OnLevelTapped(int levelNumber) — fires when Play button tapped
  - [ ] 1.7 Auto-scroll support: ScrollToLevel(int levelNumber)

- [ ] Task 2: Create LevelNodeData helper (AC: 2, 3)
  - [ ] 2.1 Create `Scripts/Game/UI/Components/LevelNodeData.cs` — plain C# class
  - [ ] 2.2 Built from LevelRecord (completed levels) and LevelDefinition (current level)

- [ ] Task 3: Create RoadmapScreen (AC: 1, 6, 7)
  - [ ] 3.1 Create `Scripts/Game/UI/Screens/RoadmapScreen.cs`
  - [ ] 3.2 Creates Canvas with LevelListView + Back button
  - [ ] 3.3 Refresh() — rebuilds level list from ProgressionManager data + current level
  - [ ] 3.4 Auto-scrolls to current level on show

- [ ] Task 4: Wire level taps to gameplay (AC: 4, 5)
  - [ ] 4.1 LevelListView.OnLevelTapped → RoadmapScreen handler
  - [ ] 4.2 If completed level → GameplayManager.StartReplay(levelNumber)
  - [ ] 4.3 If current level → GameplayManager.StartLevel(levelNumber)
  - [ ] 4.4 Both → ScreenManager.TransitionTo(Playing)

- [ ] Task 5: Integrate into flow
  - [ ] 5.1 MainMenu "Play" → ScreenManager.TransitionTo(Roadmap)
  - [ ] 5.2 Register RoadmapScreen with ScreenManager in BootLoader

- [ ] Task 6: Write tests (AC: all)
  - [ ] 6.1 Test LevelNodeData creation from LevelRecord
  - [ ] 6.2 Test roadmap builds correct node count
  - [ ] 6.3 Test current level is correctly identified

## Dev Notes

### LevelListView is shared with StarGateScreen

Both the roadmap and gate screen need a scrollable level list. LevelListView is a reusable component:
- RoadmapScreen uses it for full navigation
- StarGateScreen (Story 4.5) uses it for replay selection during gate block

### Building the node list

```csharp
var nodes = new List<LevelNodeData>();
// Completed levels from progression
foreach (var record in progression.GetAllLevelRecords())
    nodes.Add(LevelNodeData.FromRecord(record));
// Current level (not yet completed)
var currentDef = DifficultyScaler.GetLevelDefinition(progression.CurrentLevel);
var city = CityAssigner.AssignCity(progression.CurrentLevel);
nodes.Add(LevelNodeData.ForCurrentLevel(progression.CurrentLevel, city, currentDef));
```

### References

- [Source: memory/project_roadmap_vision.md] — scrollable level history with city/mood/stars, tap to replay
- [Source: _bmad-output/gdd.md#Level Structure] — linear roadmap, auto progression

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- LevelNodeData: data class for level nodes, FromRecord/ForCurrentLevel factory methods
- LevelListView: reusable ScrollRect+VerticalLayoutGroup, entries with city/mood/stars/play button, OnLevelTapped event, ScrollToLevel
- RoadmapScreen: Canvas with header (total stars), LevelListView, Back button. Refresh() builds nodes from ProgressionManager
- Tapping completed → StartReplay, tapping current → StartLevel, both transition to Playing
- BootLoader registers RoadmapScreen, auto-refreshes on transition to Roadmap state
- 4 tests: LevelNodeData creation, current level, node count

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/LevelNodeData.cs`
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/LevelListView.cs`
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/RoadmapScreen.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/RoadmapTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — registers RoadmapScreen, auto-refresh on state change
