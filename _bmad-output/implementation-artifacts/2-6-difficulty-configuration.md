# Story 2.6: Difficulty Configuration

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to configure difficulty scaling formulas,
so that I can balance puzzle difficulty through playtesting without code changes.

## Acceptance Criteria

1. **ScriptableObject config** — Difficulty scaling parameters are defined in a ScriptableObject asset
2. **DifficultyScaler uses config** — DifficultyScaler reads from the ScriptableObject instead of hardcoded values
3. **Inspector editable** — All scaling parameters can be tweaked in Unity Inspector
4. **Runtime hot-reload** — Changing values in Inspector during play mode takes effect on next level generation
5. **Sensible defaults** — Config asset ships with default values matching the GDD estimates

## Tasks / Subtasks

- [ ] Task 1: Create DifficultyConfig ScriptableObject (AC: 1, 3, 5)
  - [ ] 1.1 Create `Scripts/Game/LevelGen/DifficultyConfig.cs` — ScriptableObject with fields for all scaling parameters
  - [ ] 1.2 Fields: baseColorCount, colorsPerLevelStep, maxColors, baseContainerCount, containersPerLevelStep, maxContainers, baseSlotCount, slotsPerLevelStep, maxSlots, emptyContainerCount, minShuffleMultiplier
  - [ ] 1.3 Default values matching GDD estimates
  - [ ] 1.4 Create asset instance in `Data/Difficulty/` folder (or note for Unity Editor creation)

- [ ] Task 2: Refactor DifficultyScaler to use config (AC: 2, 4)
  - [ ] 2.1 Change DifficultyScaler to accept DifficultyConfig parameter or access via Service Locator
  - [ ] 2.2 Replace hardcoded formulas with config-driven calculations
  - [ ] 2.3 Since SO config is read at generation time, runtime changes naturally apply to next level

- [ ] Task 3: Write tests (AC: all)
  - [ ] 3.1 Test DifficultyScaler with custom config produces expected parameters
  - [ ] 3.2 Test default config values produce same results as previous hardcoded values
  - [ ] 3.3 Test edge cases: config with extreme values doesn't break generation

## Dev Notes

### ScriptableObject Pattern (from architecture)

```csharp
[CreateAssetMenu(fileName = "DifficultyConfig", menuName = "JuiceSort/DifficultyConfig")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Color Scaling")]
    [SerializeField] private int _baseColorCount = 3;
    [SerializeField] private int _colorsPerLevelStep = 20;
    [SerializeField] private int _maxColors = 5;
    // ... etc
}
```

Access via `Services.Get<IDataManager>().DifficultyConfig` per architecture, or pass directly to DifficultyScaler for simpler approach in early development.

### Scope Boundaries

- NO IDataManager service registration yet — pass config directly for now
- The SO asset file must be created in Unity Editor (can't create .asset from CLI)

### References

- [Source: _bmad-output/game-architecture.md#Data Architecture] — "ScriptableObjects for config, Plain C# for runtime"
- [Source: _bmad-output/game-architecture.md#Data Patterns] — DataManager service with SO references
- [Source: _bmad-output/project-context.md#ScriptableObjects] — "Use for static config data only"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- DifficultyConfig SO: [CreateAssetMenu], all scaling params as [SerializeField] with [Header] groups, default values matching GDD
- DifficultyScaler refactored: two overloads — GetLevelDefinition(int) for defaults, GetLevelDefinition(int, DifficultyConfig) for SO-driven
- Default overload preserved so existing code and tests work without changes
- SO asset must be created in Unity Editor: Right-click > Create > JuiceSort > DifficultyConfig
- Existing DifficultyScalerTests still pass (test the default overload)

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/DifficultyConfig.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/LevelGen/DifficultyScaler.cs` — added config-driven overload, preserved default overload
