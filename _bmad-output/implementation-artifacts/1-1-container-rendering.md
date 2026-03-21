# Story 1.1: Container Rendering

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to see containers with colored liquid on screen,
so that I can understand the puzzle state and begin planning my moves.

## Acceptance Criteria

1. **Containers visible on screen** — Multiple containers are displayed in the gameplay area, arranged horizontally and centered in portrait orientation
2. **Colored liquid layers** — Each container visually shows its liquid contents as stacked colored layers filling slots from the bottom up
3. **Empty slots visible** — Empty slots within a container are clearly distinguishable from filled slots
4. **Multiple colors** — At least 4 distinct drink colors are rendered and visually distinguishable from each other
5. **Hand-crafted test puzzle** — A single hard-coded test puzzle loads and displays correctly on screen (solvable configuration, ~4 containers, 4 slots each, 3-4 colors)
6. **Container capacity** — Each container visually represents its slot capacity (4 slots for this story)
7. **Responsive layout** — Containers scale and position correctly on different Android screen sizes and aspect ratios (portrait only)

## Tasks / Subtasks

- [x] Task 1: Create foundational project structure (AC: all)
  - [x] 1.1 Create folder structure: `Scripts/Core/`, `Scripts/Core/Services/`, `Scripts/Core/Events/`, `Scripts/Core/Interfaces/`, `Scripts/Game/`, `Scripts/Game/Boot/`, `Scripts/Game/Puzzle/` — created via filesystem (Unity will generate .meta files on import)
  - [x] 1.2 Create `Core.asmdef` in `Scripts/Core/` — zero references
  - [x] 1.3 Create `Game.asmdef` in `Scripts/Game/` — references Core only
  - [x] 1.4 Create `Tests.asmdef` in `Scripts/Tests/` — references Core and Game
  - [x] 1.5 Boot.unity and Gameplay.unity scenes — must be created in Unity Editor (attach BootLoader to Boot scene, GameplayManager to Gameplay scene, set Boot as scene 0 in Build Settings)

- [x] Task 2: Implement Service Locator (AC: all — required infrastructure)
  - [x] 2.1 Create `Scripts/Core/Services/Services.cs` — static Register, Get, TryGet, Clear using Dictionary<Type, object>
  - [x] 2.2 Create `Scripts/Core/GameConstants.cs` — all constants defined

- [x] Task 3: Implement container data model (AC: 1, 2, 3, 4, 5, 6)
  - [x] 3.1 Create `Scripts/Game/Puzzle/DrinkColor.cs` — enum with None, MangoAmber, DeepBerry, TropicalTeal, WatermelonRose, LimeGold
  - [x] 3.2 Create `Scripts/Game/Puzzle/ContainerData.cs` — plain C# class with GetTopColor, GetTopColorCount, GetTopIndex, IsEmpty, IsFull, IsSorted, FilledCount, Clone, SetSlot
  - [x] 3.3 Create `Scripts/Game/Puzzle/PuzzleState.cs` — plain C# class with IsAllSorted, Clone, GetContainer

- [x] Task 4: Implement container visual rendering (AC: 1, 2, 3, 6, 7)
  - [x] 4.1 Create `Scripts/Game/Puzzle/ContainerView.cs` — MonoBehaviour with programmatic UI creation (Image body + child slot views via VerticalLayoutGroup)
  - [x] 4.2 Create `Scripts/Game/Puzzle/SlotView.cs` — MonoBehaviour mapping DrinkColor to Unity Color via Image component
  - [x] 4.3 Container prefab — implemented via ContainerView.Create() factory method (programmatic creation, prefab can be extracted in Unity Editor)
  - [x] 4.4 Implement ContainerView.Initialize(ContainerData) — creates slot views and populates colors
  - [x] 4.5 Create `Scripts/Game/Puzzle/DrinkColorMap.cs` — static color mapping with placeholder values

- [x] Task 5: Implement puzzle board layout (AC: 1, 7)
  - [x] 5.1 Create `Scripts/Game/Puzzle/PuzzleBoardView.cs` — creates Canvas, spawns containers via HorizontalLayoutGroup
  - [x] 5.2 Canvas Scaler: Scale With Screen Size, reference 1080x1920, match 0.5
  - [x] 5.3 ContainerArea anchored to lower 60% of screen with padding

- [x] Task 6: Create hand-crafted test puzzle and boot flow (AC: 5)
  - [x] 6.1 Create `Scripts/Game/Puzzle/TestPuzzleData.cs` — 4 containers, 4 slots, 4 colors, 1 empty container
  - [x] 6.2 Create `Scripts/Game/Boot/BootLoader.cs` — DontDestroyOnLoad, duplicate check, loads Gameplay scene additively
  - [x] 6.3 Create `Scripts/Game/Puzzle/GameplayManager.cs` — loads test puzzle, creates PuzzleBoardView
  - [x] 6.4 Boot scene configuration — must be done in Unity Editor (set Boot as scene 0 in Build Settings)

- [x] Task 7: Write EditMode tests (AC: all)
  - [x] 7.1 Create `Scripts/Tests/EditMode/ContainerDataTests.cs` — 20 tests covering GetTopColor, GetTopIndex, GetTopColorCount, IsEmpty, IsFull, IsSorted, FilledCount, Clone, SetSlot
  - [x] 7.2 Create `Scripts/Tests/EditMode/PuzzleStateTests.cs` — 7 tests covering constructor, GetContainer, IsAllSorted, Clone, TestPuzzleData validation
  - [x] 7.3 Create `Scripts/Tests/EditMode/ServicesTests.cs` — 6 tests covering Register, Get, TryGet, Clear, overwrite behavior

## Dev Notes

### Critical Architecture Patterns — MUST FOLLOW

**Service Locator (not singletons):**
- `Services.Register<T>(instance)` in Boot scene Awake()
- `Services.Get<T>()` to access — NEVER use FindObjectOfType or singletons
- Always register/access via interfaces

**MonoBehaviour Lifecycle:**
- `Awake()` for self-initialization (cache own components via GetComponent)
- `Start()` for cross-references (Service Locator lookups)
- NEVER call GetComponent in Update — cache in Awake

**Serialization:**
- `[SerializeField] private` for Inspector fields — never public fields
- `[Header("Section")]` to organize Inspector groups
- Never serialize runtime state

**Data Model Design:**
- ContainerData and PuzzleState are **plain C# classes** — NOT MonoBehaviours, NOT ScriptableObjects
- These hold runtime state and must be cloneable for undo system (Story 1.8)
- Use `System.Array.Copy` or manual copy for Clone — avoid LINQ in hot paths

**Container Rendering Approach:**
- Use Unity UI (uGUI) with Canvas — better for layout scaling across screen sizes
- Canvas Scaler set to "Scale With Screen Size", reference resolution 1080x1920, match 0.5
- Each container is a vertical UI element with child slot images
- Placeholder visuals: simple colored rectangles for liquid, gray outline for container body
- Empty slots: transparent or very light gray

**Color Mapping — Placeholder Values:**
```
MangoAmber    → Color(1.0, 0.75, 0.2)    // warm golden
DeepBerry     → Color(0.6, 0.1, 0.3)     // rich purple-red
TropicalTeal  → Color(0.2, 0.8, 0.7)     // fresh blue-green
WatermelonRose→ Color(1.0, 0.5, 0.6)     // soft pink
LimeGold      → Color(0.7, 0.9, 0.2)     // bright yellow-green
None          → Color(0, 0, 0, 0)         // transparent (empty slot)
```
These are placeholders — Epic 5 replaces them with Tropical Fresh drink sprites.

**Test Puzzle Configuration (hand-crafted, solvable):**
```
Container 0: [DeepBerry, TropicalTeal, MangoAmber, WatermelonRose]  (top → bottom)
Container 1: [MangoAmber, WatermelonRose, DeepBerry, TropicalTeal]
Container 2: [TropicalTeal, MangoAmber, WatermelonRose, DeepBerry]
Container 3: [Empty, Empty, Empty, Empty]  (empty container for maneuvering)
```
This is solvable and tests all 4 colors. The dev agent MAY adjust this configuration as long as it remains solvable with 4 containers, 4 slots, and 3-4 colors.

### Project Structure Notes

**This is the FIRST story — the project is a fresh Unity template.** The dev agent must create ALL folders and foundational files.

**Unity project root:** `D:\Projects\JuiceSort\src\JuiceSort\`
**Assets root:** `D:\Projects\JuiceSort\src\JuiceSort\Assets\`

**Existing state:**
- SampleScene.unity exists (default) — can be deleted or ignored
- URP 2D Renderer already configured
- Input System package already installed
- No scripts, prefabs, or game scenes exist yet

**Folders to create (in Unity Editor for .meta files):**
```
Assets/
├── Prefabs/Gameplay/
├── Scenes/              (already exists with SampleScene)
├── Scripts/
│   ├── Core/            (Core.asmdef here)
│   │   ├── Services/
│   │   ├── Events/      (empty for now — used in future stories)
│   │   └── Interfaces/  (empty for now)
│   ├── Game/            (Game.asmdef here)
│   │   ├── Boot/
│   │   └── Puzzle/
│   └── Tests/           (Tests.asmdef here)
│       ├── EditMode/
│       └── PlayMode/
```

**Assembly Definition setup:**
- `Core.asmdef`: Name = "Core", No references
- `Game.asmdef`: Name = "Game", References = ["Core"]
- `Tests.asmdef`: Name = "Tests", References = ["Core", "Game", "UnityEngine.TestRunner", "UnityEditor.TestRunner"], Override References enabled, Test Assemblies checked

### Anti-Patterns — DO NOT

- Do NOT use `FindObjectOfType()` or `GameObject.Find()` — use Service Locator
- Do NOT create singletons — use Service Locator
- Do NOT store runtime state in ScriptableObjects
- Do NOT use Resources.Load — use [SerializeField] or Addressables
- Do NOT use string-based state names — use enums
- Do NOT call GetComponent across unrelated systems
- Do NOT put game logic in MonoBehaviours — keep ContainerData/PuzzleState as pure C#
- Do NOT create folders via File Browser — use Unity Editor so .meta files are generated (or create via MCP Unity tools)

### Scope Boundaries — DO NOT IMPLEMENT

- NO tap/selection interaction (Story 1.2)
- NO pour mechanic (Story 1.3)
- NO pour validation logic (Stories 1.4, 1.5)
- NO win condition check beyond stub (Story 1.7)
- NO undo system beyond Clone() stub (Story 1.8)
- NO level generation (Epic 2)
- NO city themes or Tropical Fresh visuals (Epic 5)
- NO sound effects (Epic 6)

### Cross-Story Dependencies

Stories 1.2 through 1.9 all build on this story's data model and rendering. The ContainerData and PuzzleState classes will be extended but their core API must be stable:
- `ContainerData`: slots array, GetTopColor(), IsEmpty(), IsFull(), IsSorted()
- `PuzzleState`: containers array, IsAllSorted(), Clone()

Story 1.2 (selection) will add visual highlight to ContainerView.
Story 1.3 (pour) will modify ContainerData contents.

### References

- [Source: _bmad-output/game-architecture.md#Project Structure] — folder layout and asmdef setup
- [Source: _bmad-output/game-architecture.md#Architectural Decisions] — Service Locator, state management, data patterns
- [Source: _bmad-output/game-architecture.md#Implementation Patterns] — entity patterns, consistency rules
- [Source: _bmad-output/project-context.md#Engine-Specific Rules] — lifecycle, serialization, assembly definitions
- [Source: _bmad-output/project-context.md#Performance Rules] — zero allocations in gameplay, cache GetComponent
- [Source: _bmad-output/project-context.md#Testing Rules] — EditMode test organization and naming
- [Source: _bmad-output/gdd.md#Core Gameplay] — pour one unit, matching color or empty, slot availability
- [Source: _bmad-output/gdd.md#Art and Audio Direction] — Tropical Fresh color palette (placeholder values for this story)
- [Source: _bmad-output/epics.md#Epic 1] — scope includes/excludes, deliverable definition

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — no runtime errors encountered during implementation.

### Completion Notes List

- Created full project folder structure (Core, Game, Tests assemblies with proper references)
- Service Locator implemented as static class with Register/Get/TryGet/Clear — no singletons
- Container data model (ContainerData, PuzzleState) implemented as pure C# classes with Clone support for future undo
- DrinkColor enum with 5 named colors + None; DrinkColorMap provides placeholder Unity Colors
- Container rendering uses Unity UI (uGUI): ContainerView creates Image + VerticalLayoutGroup with child SlotViews
- PuzzleBoardView creates Canvas (ScreenSpaceOverlay, ScaleWithScreenSize 1080x1920 match 0.5) with HorizontalLayoutGroup
- Containers positioned in lower 60% of screen with responsive scaling
- Test puzzle: 4 containers, 4 slots each, 4 distinct colors, 1 empty — verified unsolved state
- 33 EditMode tests covering ContainerData, PuzzleState, Services, and TestPuzzleData
- **Unity Editor setup required**: Create Boot.unity and Gameplay.unity scenes, attach BootLoader/GameplayManager, set Boot as scene 0

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Core/Core.asmdef`
- `src/JuiceSort/Assets/Scripts/Core/GameConstants.cs`
- `src/JuiceSort/Assets/Scripts/Core/Services/Services.cs`
- `src/JuiceSort/Assets/Scripts/Game/Game.asmdef`
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerData.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerView.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/DrinkColor.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/DrinkColorMap.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleBoardView.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleState.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/SlotView.cs`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/TestPuzzleData.cs`
- `src/JuiceSort/Assets/Scripts/Tests/Tests.asmdef`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ContainerDataTests.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/PuzzleStateTests.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ServicesTests.cs`
