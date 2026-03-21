---
project_name: 'JuiceSort'
user_name: 'Nadir'
date: '2026-03-20'
sections_completed:
  ['technology_stack', 'engine_rules', 'performance_rules', 'organization_rules', 'testing_rules', 'platform_rules', 'anti_patterns']
status: 'complete'
rule_count: 45
optimized_for_llm: true
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing game code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

- **Engine:** Unity 6.0 (6000.0.71f1) — Universal 2D template
- **Render Pipeline:** URP 17.0.4 (2D Renderer)
- **Input:** Input System 1.19.0
- **UI:** Unity UI (uGUI) 2.0.0
- **Testing:** Unity Test Framework 1.6.0
- **Asset Loading:** Unity Addressables (to be added)
- **Ads:** Google AdMob SDK (to be added)
- **Platform:** Android 10+ (API 29), Portrait only
- **Language:** C#
- **Project Path:** `D:\Projects\JuiceSort\src\JuiceSort\`

## Critical Implementation Rules

### Engine-Specific Rules

**Lifecycle:**
- Use `Awake()` for self-initialization (cache own components)
- Use `Start()` for cross-references (Service Locator lookups)
- Never rely on execution order between MonoBehaviours without Script Execution Order
- Use `LateUpdate()` for camera and UI updates after gameplay

**Serialization:**
- Use `[SerializeField] private` over `public` fields for Inspector exposure
- Use `[Header("Section")]` to organize Inspector fields
- Never serialize runtime state — only config references

**Assembly Definitions:**
- `Core.asmdef` — zero dependencies on Game code
- `Game.asmdef` — references Core only
- `Tests.asmdef` — references Core and Game
- Never create circular references between assemblies

**Service Locator:**
- Register all services in Boot scene `Awake()`
- Access via `Services.Get<IInterface>()` — never `FindObjectOfType` or singletons
- Always use interfaces, not concrete types

**ScriptableObjects:**
- Use for static config data only (difficulty, cities, drink palettes)
- Never store runtime state in ScriptableObjects
- Access via `Services.Get<IDataManager>()` — never load from Resources

**Async/Coroutines:**
- Use coroutines for frame-based sequences (animations, timed events)
- Use async/await with `Awaitable` for I/O operations (Addressables, file save/load)
- Never use `async void` except for Unity event methods — use `async Awaitable`

### Performance Rules

**Frame Budget:**
- Target: 60fps (16.6ms per frame)
- Puzzle logic is lightweight — main concern is rendering and asset loading
- Never block the main thread with synchronous I/O

**Hot Path Rules (Update/LateUpdate):**
- Never call `GetComponent<T>()` in Update — cache in Awake
- Never use `Find()`, `FindObjectOfType()`, or `FindObjectsOfType()` at runtime
- No string concatenation in Update — use StringBuilder if needed
- No LINQ in Update loops
- No `foreach` on non-List collections in Update (causes allocation)

**Memory Allocation:**
- Zero allocations in gameplay loop (no `new` in Update)
- Use `CompareTag()` instead of `tag ==` (avoids string allocation)
- Pre-allocate lists and arrays where possible

**Asset Loading:**
- Load city backgrounds via Addressables async — never synchronous
- Preload next city background during current level gameplay
- Unload previous background after scene transition completes
- Use Sprite Atlases for containers and drink sprites to reduce draw calls

**Object Creation:**
- Direct `Instantiate()` for containers (created once per level)
- Object pooling only for VFX if profiling shows need
- Always set parent transform when instantiating: `Instantiate(prefab, parent)`

### Code Organization Rules

**Project Structure:**
- By Type at top level (Art, Audio, Scripts, Scenes, Prefabs, Data)
- By feature/system within Scripts folder
- Create folders in Unity Editor (not File Browser) to generate .meta files
- Create folders as needed per epic — don't pre-create empty folders

**Script Placement:**
- Core services, events, interfaces → `Scripts/Core/`
- All game systems → `Scripts/Game/{SystemName}/`
- Tests → `Scripts/Tests/EditMode/` and `Scripts/Tests/PlayMode/`
- One primary class per file — file name matches class name

**Naming Conventions:**
- C# classes/methods: `PascalCase`
- Private fields: `_camelCase` (underscore prefix)
- Parameters/locals: `camelCase`
- Constants: `PascalCase`
- Interfaces: `IPascalCase`
- Scenes/Prefabs/SO assets: `PascalCase`
- Sprites: `kebab-case` (e.g., `bg-paris-morning.png`)
- SO Event Channels: `PascalCase` verb (e.g., `LevelCompleted`)

**Architectural Boundaries:**
- Core → zero dependencies on Game
- Game → references Core only
- Cross-system communication → Service Locator or SO Event Channels
- Internal communication → C# events/delegates
- Never use `GetComponent` across unrelated systems

### Testing Rules

**Test Organization:**
- EditMode tests → `Scripts/Tests/EditMode/` — pure logic tests (no scene required)
- PlayMode tests → `Scripts/Tests/PlayMode/` — tests requiring Unity runtime
- Test file naming: `{ClassName}Tests.cs` (e.g., `PuzzleEngineTests.cs`)
- Test method naming: `MethodName_Scenario_ExpectedResult`

**What to Test in EditMode:**
- PuzzleEngine — pour validation, win detection
- LevelGenerator — solvability guarantee, difficulty parameter scaling
- StarCalculator — move efficiency to star rating conversion
- UndoStack — push, pop, overflow behavior

**What to Test in PlayMode:**
- Game flow transitions (Boot → MainMenu → Gameplay → LevelComplete)
- Scene loading/unloading
- Save/load round-trip

**Mocking:**
- Use interfaces for all services — mock via Service Locator in tests
- `Services.Register<ISaveManager>(mockSaveManager)` in test setup
- Never depend on real file I/O or Addressables in EditMode tests

**Test Boundaries:**
- Puzzle logic is pure C# — test without MonoBehaviours where possible
- Keep EditMode tests fast — no scene loading, no async waits
- PlayMode tests can be slower — use for integration verification

### Platform & Build Rules

**Target Platform:**
- Android 10+ (API level 29) — modern devices only
- Portrait orientation only — locked in Player Settings
- Fully offline gameplay — internet only for rewarded ads

**Build Configuration:**
- Scripting backend: IL2CPP for release builds
- Output: AAB (Android App Bundle) for Google Play
- Strip Debug.Log/LogWarning from release builds via Scripting > Stack Trace settings
- Debug tools compiled out via `#if UNITY_EDITOR || DEBUG`

**Input Handling:**
- Touch input only — use Input System package
- Tap to select container, tap to pour — no complex gestures
- All interactions reachable with one thumb
- Invalid taps do nothing — no error feedback to player

**App Size:**
- City backgrounds are the heaviest assets — use Addressables groups
- Compress textures appropriately for mobile (ASTC format)
- Future: Play Asset Delivery to split background assets from base APK

**Google Play Compliance:**
- AdMob rewarded video only — no forced ads, no IAP
- No analytics or telemetry for MVP
- Must meet Play Store content and technical requirements

### Critical Don't-Miss Rules

**Anti-Patterns — NEVER Do:**
- Never use `FindObjectOfType()` or `GameObject.Find()` at runtime
- Never use singletons — use Service Locator pattern instead
- Never store runtime state in ScriptableObjects — they persist across play sessions in editor
- Never call `GetComponent<T>()` in Update — cache in Awake
- Never wrap pure game logic in try-catch — only at I/O boundaries
- Never use string-based state names — always use enums
- Never call methods directly across systems — use Service Locator or SO Events
- Never load assets from Resources folder — use Addressables or SerializeField

**Common Unity Gotchas:**
- ScriptableObject data is shared across all instances — changes in play mode persist in editor
- `DontDestroyOnLoad` objects accumulate if Boot scene is reloaded — check for duplicates in Awake
- `Awake()` runs before `Start()` but order between MonoBehaviours is undefined without Script Execution Order
- Addressables handles must be released to free memory — always release after unloading backgrounds
- `Application.persistentDataPath` differs per platform — never hardcode file paths

**Puzzle-Specific Gotchas:**
- Level seed must produce identical puzzle on replay — use `System.Random` with seed, not `UnityEngine.Random`
- Undo stack is fixed at 3 — when full, oldest snapshot drops silently (no error)
- Pour validation must check: matching color OR empty target, AND available slot — both conditions. Pour moves all consecutive same-color units from top, limited by target empty slots
- Win detection must check ALL containers, not just the one that received the pour
- Star rating depends on optimal move count — this must be calculated or estimated per generated puzzle
- Animations use coroutines (no DOTween/LeanTween) — `PourAnimator` is a static class with `IEnumerator Animate(...)`, called via `StartCoroutine` from GameplayManager
- `_isAnimating` flag on GameplayManager blocks all input (tap, undo, restart, back, extra bottle) during pour animation
- Pour animation indices (sourceTopIndex, targetFirstEmpty) must be captured BEFORE `ExecutePour` mutates the data — animation is visual-only, data changes immediately
- Selection animation lives in `BottleContainerView` — `ResetVisualState()` snaps to idle instantly when transitioning to pour
- Re-select on failed pour: when CanPour fails and tapped bottle is non-empty, deselect source and select target — never leave the player with a dead tap
- Closed bottle: `IsSorted() && !IsEmpty()` = full with one color. Cannot be selected, poured from, or poured into. Distinct from empty sorted bottles which are valid pour targets. `CanPour` must reject closed bottles as both source and target

---

## Usage Guidelines

**For AI Agents:**

- Read this file before implementing any game code
- Follow ALL rules exactly as documented
- When in doubt, prefer the more restrictive option
- Update this file if new patterns emerge

**For Humans:**

- Keep this file lean and focused on agent needs
- Update when technology stack changes
- Review quarterly for outdated rules
- Remove rules that become obvious over time

Last Updated: 2026-03-20
