---
title: 'Game Architecture'
project: 'JuiceSort'
date: '2026-03-18'
author: 'Nadir'
version: '1.0'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
status: 'complete'
engine: 'Unity 6.3 LTS'
platform: 'Android'

# Source Documents
gdd: '_bmad-output/gdd.md'
epics: '_bmad-output/epics.md'
brief: '_bmad-output/game-brief.md'
---

# Game Architecture

## Document Status

This architecture document is being created through the GDS Architecture Workflow.

**Steps Completed:** 9 of 9 (Complete)

---

## Executive Summary

**JuiceSort** architecture is designed for Unity 6.3 LTS targeting Android mobile.

**Key Architectural Decisions:**

- **State Management:** Immutable state snapshots (fixed stack of 3) for undo + enum state machine for game flow
- **Scene Structure:** Bootstrap + Additive Scenes — persistent managers in Boot scene, screens load additively
- **Level Generation:** Runtime on-demand with seeded PRNG and reverse-from-solved algorithm
- **Asset Loading:** Unity Addressables for 76 city backgrounds with preloading strategy
- **Persistence:** JSON save file via Application.persistentDataPath
- **Service Architecture:** Simple Service Locator + ScriptableObject Event Channels

**Project Structure:** By Type organization with 3 Assembly Definitions (Core, Game, Tests) and 15 mapped systems.

**Implementation Patterns:** 4 standard patterns + 8 consistency rules ensuring AI agent consistency.

**Ready for:** Epic implementation phase

---

## Project Context

### Game Overview

**JuiceSort** — A casual mobile puzzle game that transforms liquid-sorting mechanics into a world travel journey. Players pour and sort themed drinks (juices by day, cocktails by night) across 38 iconic cities, each with unique landmarks and atmospheric moods.

### Technical Scope

**Platform:** Android mobile (Android 10+, modern devices)
**Genre:** Puzzle (liquid sort)
**Project Level:** Medium complexity — proven genre mechanics with procedural generation and themed content pipeline

### Core Systems

| System | Complexity | Description |
|---|---|---|
| Container/Liquid System | Medium | Pour mechanics, slot management, color matching, win detection |
| Procedural Level Generator | High | Reverse-from-solved algorithm, solvability guarantee, difficulty scaling |
| Progression System | Medium | Star ratings, batch gates (50 levels), roadmap tracking, save/load |
| Undo System | Low | Move history stack, limited uses scaled by difficulty (1/2/3) |
| Theme/Mood System | Low | City + mood assignment, background loading, morning/night visuals |
| Ad/Monetization System | Low | Google AdMob rewarded video for extra bottle mechanic |
| UI Navigation | Medium | Main menu, roadmap, in-game HUD, level complete, settings, transitions |
| Coin Economy System | Medium | Coin earning (completion, streak), spending (undo, extra bottle), persistence |
| Liquid Shader System | High | Shader Graph liquid fill, wobble physics, pour stream VFX, glass effects |
| Responsive Layout System | Medium | Dynamic bottle positioning, multi-row layout, SafeArea support |

### Technical Requirements

- **Performance:** 60fps, portrait orientation, 2D flat rendering
- **Offline:** Full gameplay offline; internet only for rewarded ads
- **Persistence:** Local save/load for progress, stars, and level state
- **App Size:** Optimize for minimal footprint — 76 city backgrounds are primary asset cost
- **Distribution:** Google Play Store compliant

### Complexity Drivers

1. **Procedural Level Generation** — Must guarantee solvability, scale difficulty across containers/colors/slots, and calculate optimal move counts for star ratings
2. **Asset Pipeline** — 76 unique city backgrounds (38 cities × 2 moods) require memory-efficient loading, caching, and app size management
3. **State Management** — Game state, undo history, progression data, and persistent saves must all be consistent and reliable

### Technical Risks

1. Procedural generation algorithm quality — must produce fun, varied, solvable puzzles across all difficulty levels
2. Optimal move count computation — needed for star rating system, potentially expensive
3. Asset volume (76 backgrounds) — impact on app size, memory, and load times
4. First Unity project — learning curve for Unity-specific patterns and best practices

---

## Engine & Framework

### Selected Engine

**Unity 6.3 LTS** (Unity 6000.3)

**Rationale:** Unity's C# scripting leverages Nadir's 8 years of .NET expertise, URP provides excellent 2D mobile rendering, mature Android build pipeline, and the free Personal tier covers this project's scope. LTS release ensures stability through development.

### Project Initialization

Create a new Unity 2D (URP) project via Unity Hub using the **2D (URP)** template. This provides:
- Universal Render Pipeline configured for 2D
- 2D Renderer asset with sprite sorting
- Default 2D project settings

### Engine-Provided Architecture

| Component | Solution | Notes |
|---|---|---|
| Rendering | URP 2D Renderer | Sprite rendering, Sprite Atlas batching, 2D lights |
| Physics | Unity 2D Physics (Box2D) | Available if needed; puzzle logic likely code-driven |
| Audio | Unity Audio (AudioSource/AudioMixer) | Sufficient for 2 music tracks + SFX |
| Input | Input System package | Touch input, tap detection |
| Scene Management | SceneManager | Async loading, additive scenes |
| Build Pipeline | Android (IL2CPP) | AAB output for Google Play |
| UI Framework | Unity UI (uGUI) | Canvas-based UI for menus, HUD, roadmap |
| Testing | Unity Test Framework | NUnit-based EditMode and PlayMode tests |
| 2D Sprites | SpriteRenderer + Sprite Atlas | Draw call batching for containers and drinks |

### Remaining Architectural Decisions

The following decisions must be made explicitly (covered in upcoming steps):

1. **State Management** — Game state pattern, undo system, level state
2. **Data Architecture** — ScriptableObjects vs plain C# for configs and runtime data
3. **Scene Structure** — Single scene vs multi-scene, bootstrap pattern
4. **Asset Loading** — Strategy for 76 city backgrounds (Resources vs Addressables)
5. **Project Structure** — Folder layout and Assembly Definitions
6. **Service Architecture** — Dependency injection / service locator approach
7. **Level Generation** — Algorithm design and execution architecture
8. **Persistence** — Save/load system approach

### Development Environment

#### MCP Servers (AI-Assisted Development)

**MCP Unity** ([CoderGamester/mcp-unity](https://github.com/CoderGamester/mcp-unity))
- 30+ tools for scene, asset, and component management
- Create, modify, and inspect GameObjects and components directly
- Run Unity Test Runner tests from AI
- Requirements: Unity 6+, Node.js 18+, npm 9+
- Install: Unity Package Manager (git URL) + Node.js MCP server
- Supported clients: Claude Code, Cursor, Windsurf, Codex CLI

**Context7** ([upstash/context7](https://github.com/upstash/context7))
- Up-to-date Unity API documentation lookup
- Version-specific code examples and usage patterns
- Install: `claude mcp add context7 -- npx -y @upstash/context7-mcp`
- Requirements: Node.js (for npx)

---

## Architectural Decisions

### Decision Summary

| Category | Decision | Rationale |
|---|---|---|
| State Management | Immutable state snapshots (fixed stack of 3) + enum state machine | Max 3 undos per level; snapshots make undo trivial |
| Scene Structure | Bootstrap + Additive Scenes | Boot scene holds persistent managers; screens load additively |
| Level Generation | Runtime on-demand with seeded PRNG | Trivially fast generation; seeded by level number for consistent replay |
| Data Architecture | ScriptableObjects for config + Plain C# for runtime | SO for tweakable data, plain C# for game state; clean separation |
| Asset Loading | Unity Addressables | Fine-grained load/unload for 76 backgrounds; future Play Asset Delivery |
| Save/Persistence | JSON file (Application.persistentDataPath) | Structured, human-readable, familiar from .NET |
| Service Architecture | Simple Service Locator + SO Event Channels | Loose coupling via interfaces; SO events for fire-and-forget |
| Project Structure | Minimal Assembly Definitions (Core, Game, Tests) | Core separated from game code; low overhead for solo dev |

### State Management

**Approach:** Immutable state snapshots + enum-based state machine

**Puzzle State:** Each pour creates a snapshot of the current puzzle state (container contents). A fixed stack of 3 snapshots is maintained — matching the maximum undo count for MVP. When the stack is full, the oldest snapshot drops off. Undo simply pops the previous snapshot.

**Game Flow:** Enum-based state machine for screen/game states:
`Boot → MainMenu → Roadmap → Playing → LevelComplete → Roadmap`

### Scene Structure

**Approach:** Bootstrap + Additive Scenes

**Boot Scene:** Lightweight scene that initializes all managers (GameManager, AudioManager, SaveManager, LevelGenerator) via `DontDestroyOnLoad`. Always loaded first.

**Additive Scenes:**
- `MainMenu.unity` — Title screen, play button
- `Roadmap.unity` — Level select, progress visualization
- `Gameplay.unity` — Puzzle containers, HUD, pour mechanics

Transitions load the new scene additively, then unload the previous one.

### Level Generation

**Approach:** Runtime on-demand, reverse-from-solved algorithm, seeded PRNG

**Algorithm:** Start with a solved state (each container holds one color). Apply random valid reverse-pours to shuffle. This guarantees solvability.

**Seeding:** Level number is the PRNG seed — same level always produces the same puzzle. Enables consistent replay for star improvement.

**Difficulty Parameters:** Container count, color count, and slot count scale based on level number. Configured via ScriptableObject difficulty curves.

### Data Architecture

**Approach:** ScriptableObjects for config, Plain C# for runtime

**ScriptableObjects (static config):**
- City definitions (name, country, landmark, mood assignments)
- Difficulty curves (container/color/slot scaling per level)
- Drink color palettes
- SO Event Channel assets
- `CoinConfig` — all coin earning/spending values (base reward, streak bonuses, undo costs, extra bottle costs, ad reward amounts)
- `LayoutConfig` — responsive layout parameters (margins, row thresholds, min/max scale)
- `LiquidShaderConfig` — wave speed, wobble damping, pour tilt angles per layer count

**Plain C# (runtime state):**
- PuzzleState (container contents, selected container)
- UndoStack (fixed stack of 3 snapshots)
- ProgressionData (current level, stars earned, batch progress)
- GameFlowState (current game state enum)
- CoinData (current balance, streak count, undo uses this level, extra bottles this level)

### Asset Loading

**Approach:** Unity Addressables

**Strategy:**
- City backgrounds grouped by batch (50 levels per batch) in Addressable groups
- Load current city background on level start
- Preload next city background during gameplay
- Unload previous background after transition
- Future: Play Asset Delivery to reduce initial APK size

### Save/Persistence

**Approach:** JSON file via Application.persistentDataPath

**Save Data Structure:**
- Current level number
- Star ratings array (per completed level)
- Total stars count
- Settings (sound toggle, music toggle)
- Current batch progress

**Serialization:** Unity JsonUtility or Newtonsoft.Json
**Location:** `Application.persistentDataPath` for Android compatibility
**Frequency:** Auto-save on level complete and settings change

### Service Architecture

**Approach:** Simple Service Locator + ScriptableObject Event Channels

**Service Locator:**
- Boot scene registers all services via interfaces
- `Services.Register<ISaveManager>(saveManager)`
- Game code accesses via `Services.Get<ISaveManager>()`
- Interfaces enable test mocking

**SO Event Channels (fire-and-forget):**
- LevelCompleted event
- StarEarned event
- PourExecuted event
- UndoExecuted event
- CoinEarnedEvent — triggers coin balance UI update, save
- CoinSpentEvent — triggers coin balance UI update, save
- StreakUpdatedEvent — triggers streak bonus display

### Project Structure

**Assembly Definitions:**
- `Core.asmdef` — Services, events, interfaces (no game dependencies)
- `Game.asmdef` — Gameplay, UI, generation, progression (references Core)
- `Tests.asmdef` — EditMode and PlayMode tests (references Core, Game)

**Folder Layout:**
```
Assets/
├── Art/
│   ├── Sprites/
│   ├── Backgrounds/      (Addressable groups)
│   └── Animations/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Data/
│   ├── Cities/
│   ├── Difficulty/
│   └── Events/
├── Prefabs/
│   ├── Gameplay/
│   └── UI/
├── Scenes/
│   ├── Boot.unity
│   ├── MainMenu.unity
│   ├── Gameplay.unity
│   └── Roadmap.unity
├── Scripts/
│   ├── Core/
│   ├── Game/
│   └── Tests/
└── Settings/
```

---

## Cross-cutting Concerns

These patterns apply to ALL systems and must be followed by every implementation.

### Error Handling

**Strategy:** Try-catch at system boundaries + global safety net

**Boundary errors (try-catch with graceful recovery):**
- Save/load file operations → retry once, then log error
- Addressables asset loading → fall back to default background
- AdMob ad requests → silently fail, hide extra bottle button
- Scene loading → log and remain on current scene

**Global safety net:**
- `Application.logMessageReceived` catches unhandled exceptions
- Logs error with stack trace for debugging
- Game continues without crashing — never show errors to players

**Example:**
```csharp
// Boundary: asset loading with fallback
try
{
    var handle = Addressables.LoadAssetAsync<Sprite>(cityBackgroundKey);
    await handle.Task;
    background.sprite = handle.Result;
}
catch (Exception e)
{
    Debug.LogError($"[ThemeManager] Failed to load background: {e.Message}");
    background.sprite = defaultBackground;
}
```

### Logging

**Format:** Tagged Debug.Log with system name prefix

**Convention:** `Debug.Log("[SystemName] message")`

**Tags:**
- `[PuzzleEngine]` — Pour, undo, win detection
- `[LevelGen]` — Puzzle generation, difficulty
- `[Progression]` — Stars, batch gates, level unlock
- `[SaveManager]` — Save/load operations
- `[ThemeManager]` — City backgrounds, mood
- `[AudioManager]` — Music, SFX
- `[AdManager]` — Rewarded ads, extra bottles

**Log levels:** `Debug.Log` (info), `Debug.LogWarning` (unexpected but handled), `Debug.LogError` (failures)

**Release builds:** Stripped via Unity build settings (Scripting > Stack Trace > None for Log/Warning)

### Configuration

**Approach:** Static C# constants + ScriptableObjects + JSON save

**Game Constants (static class):**
```csharp
public static class GameConstants
{
    public const int MaxUndo = 3;
    public const int MaxExtraBottles = 2;
    public const int StarsPerLevel = 3;
    public const int LevelsPerBatch = 50;
    public const float StarGatePercent = 0.8f;
}
```

**Balancing Values (ScriptableObjects):**
- Difficulty curves (container/color/slot scaling per level)
- Star rating thresholds (move count percentages)
- City definitions (name, country, landmark, mood)
- Drink color palettes

**Player Settings (JSON save file):**
- Sound on/off, music on/off

### Event System

**Pattern:** SO Event Channels for cross-system + C# events for internal

**Cross-system (ScriptableObject Event Channels):**
- `LevelCompletedEvent` — triggers save, UI update, audio celebration
- `StarEarnedEvent` — triggers progression check
- `PourExecutedEvent` — triggers audio, animation
- `UndoExecutedEvent` — triggers audio, state restore

**Internal (C# events/delegates):**
- Container selection/deselection within gameplay
- UI button callbacks within a screen
- Animation completion within a component

### Debug Tools

**Activation:** `#if UNITY_EDITOR || DEBUG` — compiled out of release builds

**Available Tools:**
- **Skip to level N** — Jump to any level for testing difficulty and city themes
- **Force star rating** — Complete level with specific star count for testing gates
- **Show puzzle seed** — Display PRNG seed for bug reproduction
- **Unlock all levels** — Remove progression gates for free exploration

---

## Project Structure

### Organization Pattern

**Pattern:** By Type (Unity convention)

**Rationale:** Assets organized by type at the top level (Art, Audio, Scripts, Scenes), with feature/system folders within Scripts. Standard Unity layout, works well for solo dev, and aligns with AI agent expectations.

### Directory Structure

```
Assets/
├── Art/
│   ├── Sprites/                   # Containers, drinks, UI elements
│   ├── Backgrounds/               # 76 city backgrounds (Addressable groups)
│   └── Animations/                # Coroutine-based (PourAnimator.cs, BottleContainerView selection anim)
├── Audio/
│   ├── Music/                     # 2 tracks (morning/night)
│   └── SFX/                       # Pour, UI, completion sounds
├── Data/                          # ScriptableObject instances
│   ├── Cities/                    # City definitions
│   ├── Difficulty/                # Difficulty curve configs
│   ├── Drinks/                    # Drink color palettes
│   └── Events/                    # SO Event Channel assets
├── Prefabs/
│   ├── Gameplay/                  # Container, liquid prefabs
│   └── UI/                        # Screen prefabs, HUD elements
├── Scenes/
│   ├── Boot.unity
│   ├── MainMenu.unity
│   ├── Gameplay.unity
│   └── Roadmap.unity
├── Scripts/
│   ├── Core/                      # Core.asmdef
│   │   ├── Services/
│   │   │   └── Services.cs
│   │   ├── Events/
│   │   │   ├── VoidEvent.cs
│   │   │   ├── IntEvent.cs
│   │   │   └── VoidEventListener.cs
│   │   ├── Interfaces/
│   │   │   ├── ISaveManager.cs
│   │   │   ├── IAudioManager.cs
│   │   │   ├── ILevelGenerator.cs
│   │   │   ├── IProgressionManager.cs
│   │   │   └── ICoinManager.cs
│   │   └── GameConstants.cs
│   ├── Game/                      # Game.asmdef (references Core)
│   │   ├── Boot/
│   │   │   └── BootLoader.cs
│   │   ├── Puzzle/
│   │   │   ├── PuzzleState.cs
│   │   │   ├── PuzzleEngine.cs
│   │   │   ├── UndoStack.cs
│   │   │   ├── Container.cs
│   │   │   └── ContainerSlot.cs
│   │   ├── LevelGen/
│   │   │   ├── LevelGenerator.cs
│   │   │   ├── DifficultyScaler.cs
│   │   │   └── LevelDefinition.cs
│   │   ├── Progression/
│   │   │   ├── ProgressionManager.cs
│   │   │   ├── ProgressionData.cs
│   │   │   └── StarCalculator.cs
│   │   ├── Theme/
│   │   │   ├── ThemeManager.cs
│   │   │   └── CityAssigner.cs
│   │   ├── Save/
│   │   │   ├── SaveManager.cs
│   │   │   └── SaveData.cs
│   │   ├── Audio/
│   │   │   ├── AudioManager.cs
│   │   │   └── MusicMoodController.cs
│   │   ├── Ads/
│   │   │   ├── AdManager.cs
│   │   │   └── ExtraBottleFlow.cs
│   │   ├── UI/
│   │   │   ├── Screens/
│   │   │   │   ├── MainMenuScreen.cs
│   │   │   │   ├── RoadmapScreen.cs
│   │   │   │   ├── GameplayHUD.cs
│   │   │   │   ├── LevelCompleteScreen.cs
│   │   │   │   └── SettingsScreen.cs
│   │   │   └── Components/
│   │   │       ├── StarDisplay.cs
│   │   │       └── RoadmapNode.cs
│   │   ├── Economy/
│   │   │   ├── CoinManager.cs
│   │   │   ├── CoinConfig.cs
│   │   │   └── CoinData.cs
│   │   ├── Liquid/
│   │   │   ├── LiquidMaterialController.cs
│   │   │   ├── PourStreamVFX.cs
│   │   │   └── BottleCapAnimation.cs
│   │   ├── Layout/
│   │   │   └── ResponsiveLayoutManager.cs
│   │   ├── Input/
│   │   │   └── TouchInputHandler.cs
│   │   └── Debug/
│   │       └── DebugTools.cs
│   └── Tests/                     # Tests.asmdef (references Core, Game)
│       ├── EditMode/
│       │   ├── PuzzleEngineTests.cs
│       │   ├── LevelGeneratorTests.cs
│       │   ├── StarCalculatorTests.cs
│       │   └── UndoStackTests.cs
│       └── PlayMode/
│           └── GameFlowTests.cs
└── Settings/                      # URP, Input System, Addressables configs
```

### System Location Mapping

| System | Location | Responsibility |
|---|---|---|
| Service Locator | `Core/Services/` | Register and resolve services |
| SO Event Channels | `Core/Events/` | Cross-system communication |
| Interfaces | `Core/Interfaces/` | Service contracts |
| Bootstrap | `Game/Boot/` | Initialize managers, load first scene |
| Puzzle Engine | `Game/Puzzle/` | Pour, undo, win detection, container state |
| Level Generator | `Game/LevelGen/` | Procedural puzzle creation, difficulty scaling |
| Progression | `Game/Progression/` | Stars, batch gates, level tracking |
| Theme/Mood | `Game/Theme/` | City backgrounds, morning/night |
| Save System | `Game/Save/` | JSON persistence |
| Audio | `Game/Audio/` | Music, SFX, mood-based track switching |
| Ad Integration | `Game/Ads/` | AdMob, extra bottle flow |
| UI Screens | `Game/UI/Screens/` | Each screen as a separate component |
| UI Components | `Game/UI/Components/` | Reusable UI widgets |
| Touch Input | `Game/Input/` | Tap detection and routing |
| Debug Tools | `Game/Debug/` | Development-only utilities |
| Coin Economy | `Game/Economy/` | CoinManager, CoinConfig, streak tracking, coin persistence |
| Liquid Shader | `Game/Liquid/` | Shader Graph materials, LiquidMaterialController, PourStreamVFX |
| Responsive Layout | `Game/Layout/` | ResponsiveLayoutManager, dynamic bottle positioning |

### Naming Conventions

#### Files and Code

| Element | Convention | Example |
|---|---|---|
| C# classes | PascalCase | `PuzzleEngine`, `SaveManager` |
| Methods | PascalCase | `ExecutePour()`, `CalculateStars()` |
| Private fields | _camelCase | `_currentLevel`, `_undoStack` |
| Parameters/locals | camelCase | `containerIndex`, `starCount` |
| Constants | PascalCase | `MaxUndo`, `LevelsPerBatch` |
| Interfaces | IPascalCase | `ISaveManager`, `IAudioManager` |

#### Game Assets

| Asset Type | Convention | Example |
|---|---|---|
| Scenes | PascalCase | `Boot.unity`, `MainMenu.unity` |
| Prefabs | PascalCase | `Container.prefab`, `RoadmapNode.prefab` |
| Sprites | kebab-case | `bg-paris-morning.png`, `drink-mango.png` |
| SO assets | PascalCase | `DifficultyConfig.asset`, `LevelCompleted.asset` |
| Event channels | PascalCase (verb) | `LevelCompleted`, `PourExecuted` |

### Architectural Boundaries

- **Core → zero dependencies** on Game — never reference Game code from Core
- **Game → references Core only** — never reference Unity Editor APIs
- **Tests → references both** Core and Game
- **Cross-system communication** goes through Service Locator or SO Events — no direct GetComponent between systems

---

## Implementation Patterns

These patterns ensure consistent implementation across all AI agents.

### Novel Patterns

None required — JuiceSort uses standard puzzle game patterns that fit well into established architectural approaches.

### Communication Patterns

**Pattern:** Service Locator for system-to-system, SO Events for broadcast, C# events for internal

| Communication Type | Pattern | When to Use |
|---|---|---|
| System → System | `Services.Get<T>()` | One system calls another directly |
| System → Multiple listeners | SO Event Channel | Action notifies multiple systems |
| Internal within system | C# events/delegates | Within or between tightly-related components |
| Inspector wiring | `[SerializeField]` | Direct reference to child/sibling component |

**Example (cross-system via Service Locator):**
```csharp
public class GameplayHUD : MonoBehaviour
{
    private IProgressionManager _progression;

    private void Start()
    {
        _progression = Services.Get<IProgressionManager>();
    }

    private void UpdateStarDisplay()
    {
        int totalStars = _progression.GetTotalStars();
        // update UI...
    }
}
```

**Example (broadcast via SO Event):**
```csharp
// PuzzleEngine raises event when level is completed
[SerializeField] private VoidEvent _levelCompletedEvent;

private void CheckWinCondition()
{
    if (AllContainersSorted())
    {
        _levelCompletedEvent.Raise();
    }
}
```

### Entity Patterns

**Creation:** Direct prefab instantiation (default), object pooling only for VFX if profiling requires it

**Example (container creation at level start):**
```csharp
[SerializeField] private GameObject _containerPrefab;
[SerializeField] private Transform _containerParent;

private void SpawnContainers(LevelDefinition level)
{
    foreach (var containerData in level.Containers)
    {
        var go = Instantiate(_containerPrefab, _containerParent);
        var container = go.GetComponent<Container>();
        container.Initialize(containerData);
    }
}
```

### State Patterns

**Pattern:** Enum + switch for game flow and entity states

**Game Flow States:**
```csharp
public enum GameFlowState
{
    Boot,
    MainMenu,
    Roadmap,
    Playing,
    LevelComplete
}
```

**Container States:**
```csharp
public enum ContainerState
{
    Idle,
    Selected,
    Pouring,
    Receiving
}

private void UpdateContainer()
{
    switch (_state)
    {
        case ContainerState.Idle:
            // waiting for tap
            break;
        case ContainerState.Selected:
            // highlighted, waiting for target
            break;
        case ContainerState.Pouring:
            // playing pour animation
            break;
        case ContainerState.Receiving:
            // receiving liquid animation
            break;
    }
}
```

### Data Patterns

**Access:** Central DataManager service — holds all ScriptableObject config references

**Example:**
```csharp
public interface IDataManager
{
    DifficultyConfig DifficultyConfig { get; }
    CityDatabase CityDatabase { get; }
    DrinkPalette DrinkPalette { get; }
    StarThresholds StarThresholds { get; }
}

public class DataManager : MonoBehaviour, IDataManager
{
    [SerializeField] private DifficultyConfig _difficultyConfig;
    [SerializeField] private CityDatabase _cityDatabase;
    [SerializeField] private DrinkPalette _drinkPalette;
    [SerializeField] private StarThresholds _starThresholds;

    public DifficultyConfig DifficultyConfig => _difficultyConfig;
    public CityDatabase CityDatabase => _cityDatabase;
    public DrinkPalette DrinkPalette => _drinkPalette;
    public StarThresholds StarThresholds => _starThresholds;
}

// Usage in any system:
var config = Services.Get<IDataManager>().DifficultyConfig;
int colorCount = config.GetColorCount(levelNumber);
```

### Consistency Rules

| Pattern | Convention | Enforcement |
|---|---|---|
| Service access | Always via `Services.Get<IInterface>()` | Never use `FindObjectOfType` or singletons |
| Cross-system events | Always via SO Event Channels | Never call methods directly across systems |
| Internal events | C# `event Action` delegates | Never use SO Events for internal communication |
| MonoBehaviour init | Cache references in `Awake()`, cross-references in `Start()` | Never use `GetComponent` in `Update()` |
| Data access | Always via `Services.Get<IDataManager>()` | Never load SOs directly from Resources |
| State changes | Enum + switch | Never use string-based state names |
| Prefab creation | `Instantiate()` with parent transform | Always set parent to keep hierarchy clean |
| Error boundaries | Try-catch at I/O and external calls only | Never wrap pure game logic in try-catch |

---

## New Systems (Epics 9-11)

### Coin Economy System (Epic 9)

**Service:** `ICoinManager` / `CoinManager` — registered in Service Locator during Boot.

**Responsibilities:**
- Track coin balance (persisted via SaveManager)
- Calculate level completion rewards (base + difficulty scaling + move efficiency bonus)
- Track consecutive win streak, award streak bonuses
- Process coin spending (undo, extra bottle) with escalating costs per level
- Expose coin balance for UI display

**Data:**
- `CoinConfig` ScriptableObject — all earning/spending values, tunable in Inspector
- `CoinData` — runtime state (balance, streak, per-level usage counts)

**Events:**
- `CoinEarnedEvent` (SO Event Channel) — raised on level complete, ad reward
- `CoinSpentEvent` (SO Event Channel) — raised on undo/extra bottle purchase
- `StreakUpdatedEvent` (SO Event Channel) — raised on streak change

**Integration:**
- Listens to `LevelCompletedEvent` to award coins
- Called by GameplayHUD when player taps undo/extra bottle
- Balance persisted in SaveData JSON alongside star data

### Liquid Shader System (Epic 10)

**Components:**
- `LiquidShader.shadergraph` — Shader Graph material for bottle liquid rendering (URP 2D, Sprite-Unlit base)
- `LiquidMaterialController` — MonoBehaviour managing per-bottle material instances, fill amounts, colors, wobble
- `PourStreamVFX` — LineRenderer/particle system for visible liquid stream between bottles during pour
- `BottleCapAnimation` — cork/cap closing effect on sorted bottle completion

**Shader Parameters:**
- `_Fill0-3`, `_Color0-3` — per-layer fill and color
- `_WobbleX`, `_WobbleZ` — damped oscillation for select/deselect wobble
- `_WaveSpeed`, `_WaveAmplitude` — sinusoidal water surface animation

**Refactoring:**
- `BottleContainerView` — internally refactored to use shader material instead of sprite slots (external API preserved)
- `PourAnimator` — internally refactored for smooth fill lerp + dynamic tilt angles (static class pattern preserved)

### Responsive Layout System (Epic 11)

**Component:** `ResponsiveLayoutManager` — calculates bottle positions, scale, and row arrangement.

**Algorithm:**
- Input: bottle count, `Camera.main.orthographicSize`, `Screen.safeArea`, `Camera.main.aspect`
- Output: position array, uniform scale, row count
- Single row for 1-6 bottles, two rows for 7+
- 5-8% margin on each side
- Supports 16:9 to 20:9 aspect ratios

**Integration:**
- Called by `BottleContainerView.Create()` at level start
- Re-invoked when extra bottle is added mid-level (smooth re-layout animation)
- Accounts for HUD regions (top bar, bottom bar) in available play area

---

## Architecture Validation

### Validation Summary

| Check | Result | Notes |
|---|---|---|
| Decision Compatibility | PASS | All decisions align with Unity 6.3 LTS and each other |
| GDD Coverage | PASS | All 7 core systems and all technical requirements covered |
| Pattern Completeness | PASS | All 6 pattern categories defined with code examples |
| Epic Mapping | PASS | All 7 epics map to architecture locations and patterns |
| Document Completeness | PASS | All required sections present, no placeholder text |

### Coverage Report

**Systems Covered:** 10/10 (7 original + 3 new: Coin Economy, Liquid Shader, Responsive Layout)
**Patterns Defined:** 4 standard + 8 consistency rules
**Decisions Made:** 8

### Validation Date

2026-03-22 (updated for Epics 9-11)

---

## Development Environment

### Prerequisites

- Unity Hub (latest)
- Unity 6.3 LTS (Unity 6000.3)
- Android Build Support module (via Unity Hub)
- Android SDK & NDK (installed via Unity Hub)
- Node.js 18+ (for MCP servers)
- IDE: Visual Studio, Rider, or VS Code with C# extension

### Unity Project Location

```
D:\Projects\JuiceSort\
├── _bmad/                    # BMAD workflow system
├── _bmad-output/             # Planning artifacts (GDD, architecture, epics)
├── docs/                     # Project documentation
└── src/
    └── JuiceSort/            # Unity project root
        ├── Assets/
        ├── Packages/
        ├── ProjectSettings/
        └── ...
```

### AI Tooling (MCP Servers)

| MCP Server | Purpose | Install Type |
|---|---|---|
| MCP Unity (CoderGamester) | Scene, asset, and component management | Unity Package + Node.js |
| Context7 (Upstash) | Up-to-date Unity API documentation | npx command |

**MCP Unity Setup:**
1. Install Node.js 18+
2. In Unity: Window > Package Manager > + > Add package from git URL
3. URL: `https://github.com/CoderGamester/mcp-unity.git`
4. In Unity: Tools > MCP Unity > Server Window > Configure
5. Click Start Server

**Context7 Setup:**
```bash
claude mcp add context7 -- npx -y @upstash/context7-mcp
```

### Project Setup (Already Complete)

1. Unity project created at `D:\Projects\JuiceSort\src\JuiceSort\` using Universal 2D template
2. Target platform set to Android
3. Orientation set to Portrait
4. Minimum API level: Android 10 (API 29)

### First Steps

1. Create initial folder structure in Unity Editor (Scripts/Core/, Scripts/Game/, Scenes/)
2. Create Core.asmdef, Game.asmdef, and Tests.asmdef
3. Configure MCP servers per the AI Tooling instructions above
4. Implement the Boot scene with Service Locator and BootLoader
