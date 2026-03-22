# Story 3.6: Save & Load System

Status: done
Merges: 3.7 (progress persistence across sessions)

## Story

As a player,
I want my progress saved automatically and persisted when I close and reopen the app,
so that I can continue my journey from where I left off with all my level records intact.

## Acceptance Criteria

1. **Auto-save on level complete** — Progress saves automatically when a level is completed
2. **SaveData class** — JSON-serializable class holding all progress including per-level records
3. **SaveManager service** — Handles save/load via Application.persistentDataPath, registered in Service Locator
4. **JSON serialization** — Save data stored as JSON file
5. **Load on startup** — When the game starts, loads saved progress if save file exists
6. **Resume level** — Game starts at the saved current level, not level 1
7. **Level records preserved** — All level records (city, mood, stars) restored on load for roadmap display
8. **No save = fresh start** — If no save file exists, start from level 1 with no records
9. **Corruption handled** — If save file is corrupted, start fresh (don't crash)

## Tasks / Subtasks

- [x] Task 1: Create ISaveManager interface (AC: 3)
  - [x] 1.1 Create `Scripts/Core/Interfaces/ISaveManager.cs` — interface with Save(string json), LoadJson() → string, HasSave() → bool, DeleteSave(). Uses string (not SaveData) so Core has no Game dependency. SaveManager in Game handles serialization.

- [x] Task 2: Create SaveData class (AC: 2, 7)
  - [x] 2.1 Create `Scripts/Game/Save/SaveData.cs` — `[System.Serializable]` class
  - [x] 2.2 Fields: currentLevel (int), levelRecords (SavedLevelRecord[] — serializable version of LevelRecord)
  - [x] 2.3 Create `SavedLevelRecord` — `[System.Serializable]` with: levelNumber, cityName, countryName, mood (int), stars
  - [x] 2.4 Static methods: FromProgressionData(ProgressionData) → SaveData, ToProgressionData() → ProgressionData
  - [x] 2.5 Settings fields: soundEnabled (bool), musicEnabled (bool) — defaults true

- [x] Task 3: Create SaveManager (AC: 3, 4, 9)
  - [x] 3.1 Create `Scripts/Game/Save/SaveManager.cs` — MonoBehaviour implementing ISaveManager
  - [x] 3.2 Save: JsonUtility.ToJson → File.WriteAllText to persistentDataPath/save.json
  - [x] 3.3 Load: File.ReadAllText → JsonUtility.FromJson, try-catch at I/O boundary
  - [x] 3.4 HasSave: File.Exists check
  - [x] 3.5 On deserialization failure → return null, log warning
  - [x] 3.6 Update BootLoader: create SaveManager GO FIRST, register as ISaveManager

- [x] Task 4: Auto-save on level complete (AC: 1)
  - [x] 4.1 In ProgressionManager.CompleteLevelWithStars → call ISaveManager.Save after updating data

- [x] Task 5: Load on startup in ProgressionManager.Start() (AC: 5, 6, 7, 8)
  - [x] 5.1 ProgressionManager loads save in **Start()** (not Awake) — ensures SaveManager is registered first
  - [x] 5.2 If ISaveManager.HasSave() → load, convert SaveData to ProgressionData
  - [x] 5.3 If no save or load fails → fresh ProgressionData (level 1, no records)
  - [x] 5.4 BootLoader creation order: SaveManager first, ProgressionManager second

- [x] Task 6: Write tests (AC: all)
  - [x] 6.1 Create `Scripts/Tests/EditMode/SaveSystemTests.cs`
  - [x] 6.2 Test SaveData serialization round-trip (JsonUtility.ToJson → FromJson → same values)
  - [x] 6.3 Test SaveData ↔ ProgressionData conversion with level records
  - [x] 6.4 Test level records preserved through save/load (city, mood, stars all match)
  - [x] 6.5 Test load with no save → fresh start values
  - [x] 6.6 Test SavedLevelRecord preserves all fields through JSON

## Dev Notes

### BootLoader Service Creation Order (CRITICAL)

SaveManager MUST be created before ProgressionManager:
```csharp
// BootLoader.Awake():
Services.Clear();

// 1. SaveManager — no dependencies
var smGo = new GameObject("SaveManager");
DontDestroyOnLoad(smGo);
Services.Register<ISaveManager>(smGo.AddComponent<SaveManager>());

// 2. ProgressionManager — loads from SaveManager in Start()
var pmGo = new GameObject("ProgressionManager");
DontDestroyOnLoad(pmGo);
Services.Register<IProgressionManager>(pmGo.AddComponent<ProgressionManager>());
```

### ProgressionManager loads in Start(), not Awake()

This ensures SaveManager is already registered when ProgressionManager calls `Services.Get<ISaveManager>()`.

### SaveData serialization — JsonUtility limitations

JsonUtility doesn't serialize Dictionary. Use arrays:
```csharp
[System.Serializable]
public class SaveData
{
    public int currentLevel;
    public SavedLevelRecord[] levelRecords;
    public bool soundEnabled = true;
    public bool musicEnabled = true;
}

[System.Serializable]
public class SavedLevelRecord
{
    public int levelNumber;
    public string cityName;
    public string countryName;
    public int mood; // 0=Morning, 1=Night (enum cast)
    public int stars;
}
```

### References

- [Source: _bmad-output/game-architecture.md#Save/Persistence] — JSON, persistentDataPath
- [Source: _bmad-output/game-architecture.md#Error Handling] — try-catch at I/O boundaries only
- [Source: _bmad-output/project-context.md#Common Unity Gotchas] — "persistentDataPath differs per platform"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- ISaveManager in Core: uses string (JSON) to avoid Core→Game dependency
- SaveManager: File I/O with try-catch at boundaries, persistentDataPath + "save.json"
- SaveData/SavedLevelRecord: [Serializable] classes, FromProgressionData/ToProgressionData converters
- ProgressionManager loads save in Start() (not Awake) — ensures SaveManager registered first
- AutoSave called after every CompleteLevelWithStars
- BootLoader creates SaveManager BEFORE ProgressionManager (order critical)
- Corrupted save → fresh start with warning log
- 6 tests: JSON round-trip, conversion both ways, record preservation, empty/null handling

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Core/Interfaces/ISaveManager.cs`
- `src/JuiceSort/Assets/Scripts/Game/Save/SaveData.cs`
- `src/JuiceSort/Assets/Scripts/Game/Save/SaveManager.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/SaveSystemTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionManager.cs` — Start() loads save, AutoSave after complete
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates SaveManager before ProgressionManager
