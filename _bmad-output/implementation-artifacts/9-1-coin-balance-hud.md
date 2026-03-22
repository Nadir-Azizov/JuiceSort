# Story 9.1: Coin Balance HUD

Status: done

## Story

As a player,
I want to see my coin balance on the gameplay HUD and have it persist across sessions,
so that I always know how many coins I have available for boosters.

## Acceptance Criteria

1. **Coin balance visible** — The gameplay HUD displays the current coin balance in the top bar (coin icon area, already positioned in Story 11.2)
2. **Balance persists** — Coin balance survives app close and reopen (saved to JSON via SaveManager)
3. **Balance loads on boot** — When the game starts, the coin balance loads from the save file and is available via `ICoinManager`
4. **Service accessible** — Any system can read/modify coin balance via `Services.Get<ICoinManager>()`
5. **Initial balance** — New players start with 0 coins (configurable via CoinConfig)
6. **Balance updates in real-time** — When coins are earned or spent (future stories), the HUD updates immediately
7. **CoinConfig ScriptableObject** — All coin economy values stored in a tunable ScriptableObject, never hardcoded
8. **Auto-save on change** — Coin balance auto-saves whenever it changes (same pattern as ProgressionManager)

## Tasks / Subtasks

- [x] Task 1: Create ICoinManager interface in Core (AC: 4)
  - [x] 1.1 Create `Scripts/Core/Interfaces/ICoinManager.cs`
  - [x] 1.2 Methods: `int GetBalance()`, `void AddCoins(int amount)`, `bool SpendCoins(int amount)`, `event Action<int> OnBalanceChanged`

- [x] Task 2: Create CoinConfig (AC: 7)
  - [x] 2.1 Created `Scripts/Game/Economy/CoinConfig.cs` as plain C# class with `Default()` factory
  - [x] 2.2 All fields with defaults: InitialBalance(0), BaseLevelReward(75), etc.
  - [x] 2.3 Helper methods: `GetUndoCost(int)`, `GetExtraBottleCost(int)` — cap at last array value
  - [x] 2.4 Documented: can be promoted to SO later

- [x] Task 3: Create CoinManager service (AC: 3, 4, 5, 6, 8)
  - [x] 3.1 Created `Scripts/Game/Economy/CoinManager.cs` as MonoBehaviour implementing ICoinManager
  - [x] 3.2 Loads coin balance from SaveData in `Start()` (reads JSON directly, same as ProgressionManager)
  - [x] 3.3 `AddCoins(int)` — increases balance, invokes OnBalanceChanged, triggers save via ProgressionManager.AutoSave()
  - [x] 3.4 `SpendCoins(int)` — decreases if sufficient, invokes OnBalanceChanged, triggers save, returns bool
  - [x] 3.5 `GetBalance()` — returns current balance
  - [x] 3.6 Auto-save via ProgressionManager.AutoSave() which reads ICoinManager.GetBalance() — no independent SaveData writes
  - [x] 3.7 Uses `CoinConfig.Default()` for config values

- [x] Task 4: Update SaveData for coin persistence (AC: 2)
  - [x] 4.1 Added `public int coinBalance;` field to SaveData.cs
  - [x] 4.2 Updated `FromProgressionData()` to accept `coinBalance` parameter

- [x] Task 5: Register CoinManager in BootLoader (AC: 3)
  - [x] 5.1 Created CoinManager GO and registered as `ICoinManager` in `CreateServices()`
  - [x] 5.2 Placed after ProgressionManager, before AudioManager
  - [x] 5.3 `DontDestroyOnLoad` applied

- [x] Task 6: Wire HUD to CoinManager (AC: 1, 6)
  - [x] 6.1 LoadLevel: replaced placeholder with `coinMgr.GetBalance()` + subscribe to OnBalanceChanged
  - [x] 6.2 ResumeLevel: same pattern
  - [x] 6.3 Subscribe to `ICoinManager.OnBalanceChanged` → `_hud.UpdateCoinDisplay`
  - [x] 6.4 Unsubscribe in `DestroyBoard()` to prevent leaks

- [x] Task 7: Write EditMode tests (AC: 7)
  - [x] 7.1 Created `Scripts/Tests/EditMode/CoinManagerTests.cs` (tests CoinConfig since CoinManager is MonoBehaviour)
  - [x] 7.2 Test: initial balance default is 0
  - [x] 7.3 Test: all default values match spec (BaseLevelReward=75, StreakBonus3=200, etc.)
  - [x] 7.4 Test: GetUndoCost escalation and capping
  - [x] 7.5 Test: GetExtraBottleCost escalation and capping
  - [x] 7.6 Test: all config field defaults

## Dev Notes

### Key Architecture Patterns (MUST FOLLOW)

- **Service Locator:** Register via `Services.Register<ICoinManager>(instance)` in BootLoader. Access via `Services.Get<ICoinManager>()` or `Services.TryGet<>()`. NEVER use FindObjectOfType or singletons.
- **Interfaces in Core:** `ICoinManager` goes in `Scripts/Core/Interfaces/` (Core assembly, zero game deps). `CoinManager` implementation goes in `Scripts/Game/Economy/` (Game assembly, refs Core).
- **SaveData pattern:** Add `coinBalance` field to `SaveData.cs`. JsonUtility auto-handles new fields (defaults to 0 for old saves).
- **Save coordination:** CoinManager does NOT call SaveManager directly. Instead, it updates the coin field on ProgressionManager's SaveData and delegates saving to ProgressionManager. This avoids a race condition where two managers write different versions of the same JSON file.
- **CoinConfig as plain C# class:** BootLoader uses `new GameObject + AddComponent` (not prefab), so `[SerializeField]` won't be populated. Use `CoinConfig.Default()` static factory (same pattern as LayoutConfig from Story 11.1). Can be promoted to SO later.
- **Coroutines/async:** Not needed for this story — coin operations are synchronous.
- **Event pattern:** Use `System.Action<int>` for OnBalanceChanged (consistent with existing codebase — project uses C# Actions, NOT ScriptableObject event channels despite architecture doc mentioning them).

### Existing Infrastructure (DO NOT RECREATE)

- **HUD coin display:** Already exists in `GameplayHUD.cs` — `_coinText` field, `UpdateCoinDisplay(int coinBalance)` method, positioned in top bar with StarGold color. Just call it with real data.
- **SaveManager:** `ISaveManager` already registered. `SaveData` serializes to JSON at `Application.persistentDataPath/save.json`.
- **BootLoader:** `CreateServices()` method handles all service initialization with `DontDestroyOnLoad`.

### File Locations

| File | Action | Location |
|------|--------|----------|
| ICoinManager.cs | NEW | `Scripts/Core/Interfaces/` |
| CoinConfig.cs | NEW (plain C# class) | `Scripts/Game/Economy/` |
| CoinManager.cs | NEW | `Scripts/Game/Economy/` |
| SaveData.cs | MODIFY | `Scripts/Game/Save/` — add `coinBalance` field |
| ProgressionManager.cs | MODIFY | `Scripts/Game/Progression/` — expose save method for CoinManager, add coin field to save/load |
| BootLoader.cs | MODIFY | `Scripts/Game/Boot/` — register CoinManager |
| GameplayManager.cs | MODIFY | `Scripts/Game/Puzzle/` — wire HUD to real balance |
| CoinManagerTests.cs | NEW | `Scripts/Tests/EditMode/` |

### CoinConfig Default Values (from reconstruction prompt)

| Field | Default | Source |
|-------|---------|--------|
| initialBalance | 0 | New players start empty |
| baseLevelReward | 75 | Mid-range of 50-100 spec |
| moveEfficiencyBonusPercent | 0.25f | 25% bonus for efficient play |
| streakBonus3 | 200 | 3-win streak bonus |
| streakBonus5 | 500 | 5-win streak bonus |
| undoCosts | [100, 200, 300] | Escalating per use per level |
| extraBottleCosts | [500, 900] | Escalating per use per level |
| adRewardAmount | 250 | Mid-range of 200-300 spec |

### Previous Story Intelligence

Stories 11.1, 11.2, 11.5 were completed in this sprint. Key learnings:
- `BottleContainerView` caches rest state — any external position/scale changes must call `UpdateRestState()`
- HUD is Canvas ScreenSpaceOverlay (sorting order 5), bottles are world-space — don't mix coordinate systems
- BootLoader creates services sequentially — order matters for cross-service dependencies in `Start()`

### References

- [Source: _bmad-output/gdd.md#Economy and Resources] — Coin earning/spending tables
- [Source: _bmad-output/game-architecture.md#New Systems] — CoinManager service spec
- [Source: _bmad-output/project-context.md#Coin System Rules] — Persistence and config rules

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Completion Notes
- ICoinManager interface in Core with GetBalance, AddCoins, SpendCoins, OnBalanceChanged
- CoinConfig as plain C# class (not SO) with Default() factory — all economy values tunable
- CoinManager MonoBehaviour loads balance in Start(), saves via ProgressionManager.AutoSave() to avoid race condition
- SaveData.coinBalance field added; FromProgressionData() now accepts coinBalance param
- ProgressionManager.AutoSave() made public, queries ICoinManager for coin balance when saving
- GameplayHUD wired to real coin balance with OnBalanceChanged subscription + cleanup in DestroyBoard
- 12 EditMode tests covering CoinConfig defaults, escalation, and capping

### File List
- **NEW:** `src/JuiceSort/Assets/Scripts/Core/Interfaces/ICoinManager.cs`
- **NEW:** `src/JuiceSort/Assets/Scripts/Game/Economy/CoinConfig.cs`
- **NEW:** `src/JuiceSort/Assets/Scripts/Game/Economy/CoinManager.cs`
- **NEW:** `src/JuiceSort/Assets/Scripts/Tests/EditMode/CoinManagerTests.cs`
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Save/SaveData.cs` — added coinBalance field, updated FromProgressionData()
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionManager.cs` — AutoSave() made public, includes coin balance
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — registered CoinManager as ICoinManager
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — wired HUD to real balance, subscribe/unsubscribe OnBalanceChanged

### Change Log
- 2026-03-22: Full implementation of coin balance system with persistence, HUD wiring, and tests

