# Story 9.5: Coin-Extra Bottle

Status: done

## Story

As a player,
I want to spend coins to add an extra bottle (cost increases, max 2 per level),
so that I have a coin-based safety net when stuck instead of watching ads.

## Acceptance Criteria

1. **Extra bottle costs coins** — Tapping "+" calls `coinMgr.SpendCoins(cost)` before adding the bottle. If SpendCoins returns false, nothing happens.
2. **Escalating cost** — 1st extra bottle costs 500, 2nd costs 900 (from `CoinConfig.GetExtraBottleCost(useIndex)`)
3. **Max 2 per level** — Hard limit enforced by `_extraBottlesUsed >= GameConstants.MaxExtraBottles`
4. **Button shows cost** — HUD "+" button displays the next extra bottle cost (e.g., "+ 500") instead of remaining count
5. **Insufficient coins disables button** — When `coinMgr.GetBalance() < nextCost`, button is non-interactable
6. **Max reached disables button** — After 2 uses, button disabled regardless of coin balance
7. **Bottle added immediately on spend** — Existing animated re-layout from Epic 7/11 unchanged
8. **Balance updates instantly** — Via OnBalanceChanged (already wired from 9.1)
9. **Replay starts fresh** — Extra bottle usage resets on LoadLevel (same as new level)
10. **Replaces ad-based flow** — `RequestExtraBottle()` no longer uses IAdManager. Coins replace ads for extra bottle.
11. **All values from CoinConfig** — Costs from `CoinConfig.ExtraBottleCosts[]`, never hardcoded

## Tasks / Subtasks

- [x] Task 1: Replace ad flow with coin payment in RequestExtraBottle (AC: 1, 2, 3, 7, 10, 11)
  - [x] 1.1 Remove IAdManager usage from `RequestExtraBottle()`
  - [x] 1.2 Get ICoinManager from Services, calculate cost via `config.GetExtraBottleCost(_extraBottlesUsed)`
  - [x] 1.3 Call `coinMgr.SpendCoins(cost)` — if false, return early
  - [x] 1.4 On successful spend: execute existing bottle-add logic (AddExtraContainer, AddContainerView, clear undo stack)
  - [x] 1.5 Keep existing max-2 guard (`_extraBottlesUsed >= GameConstants.MaxExtraBottles`)

- [x] Task 2: Update HUD extra bottle display (AC: 4, 5, 6, 8)
  - [x] 2.1 Add `UpdateExtraBottleState(int cost, bool canAfford, bool hasRemaining)` to GameplayHUD
  - [x] 2.2 Display cost on button: `"+ 500"` format (cost in coins)
  - [x] 2.3 Set `_extraBottleButton.interactable = canAfford && hasRemaining`
  - [x] 2.4 Remove extra bottle text/interactable logic from `UpdateDisplay()` — now only handles move count
  - [x] 2.5 Add `RefreshExtraBottleState()` helper to GameplayManager
  - [x] 2.6 Call `RefreshExtraBottleState()` at all update points: LoadLevel, RestartLevel, after bottle added, ResumeLevel, OnBalanceChanged

- [x] Task 3: Write EditMode tests (AC: 1-6, 11)
  - [x] 3.1 Create `Scripts/Tests/EditMode/CoinExtraBottleTests.cs`
  - [x] 3.2 Test: GetExtraBottleCost(0) returns 500 (first use)
  - [x] 3.3 Test: GetExtraBottleCost(1) returns 900 (second use)
  - [x] 3.4 Test: GetExtraBottleCost(5) returns 900 (capped)
  - [x] 3.5 Test: custom config values respected
  - [x] 3.6 Test: full sequence cost (500 + 900 = 1400)
  - [x] 3.7 Test: insufficient balance + exact balance scenarios

## Dev Notes

### Extra Bottle Payment Flow

```
On "+" tap:
  if (isLevelComplete || isAnimating) return
  if (_extraBottlesUsed >= MaxExtraBottles) return
  cost = config.GetExtraBottleCost(_extraBottlesUsed)
  if (!coinMgr.SpendCoins(cost)) return  // insufficient coins
  // existing bottle-add logic (unchanged):
  AddExtraContainer, _extraBottlesUsed++, animate re-layout, clear undo stack
  RefreshExtraBottleState()
  RefreshUndoState()  // undo stack cleared, so undo state changes too
```

**Simpler than undo**: No pop-then-spend pattern needed. The max-2 and SpendCoins checks happen before any state changes, so no rollback required.

### Key Architecture Patterns

- **Same pattern as 9.4 Undo**: `SpendCoins(cost)` → if false, return. If true, proceed with action.
- **CoinConfig.GetExtraBottleCost(int)** — already exists, returns [500, 900], caps at last.
- **_extraBottlesUsed already tracked** — already incremented in existing code. Just add coin gate before it.
- **Animated re-layout untouched** — `AddContainerView` + `SetAllSparklesEnabled` pattern stays the same.
- **Undo stack cleared** — existing behavior. `RefreshUndoState()` already called after extra bottle.

### HUD Change Strategy

Same approach as 9.4: add `UpdateExtraBottleState(int cost, bool canAfford, bool hasRemaining)` and remove extra bottle logic from `UpdateDisplay()`. This completes the cleanup started in 9.4 where undo was extracted — now extra bottle follows suit.

After this story, `UpdateDisplay()` will only handle move count display.

### Removing Ad Flow

`RequestExtraBottle()` currently calls `Services.TryGet<IAdManager>()` → `ShowRewardedAd()`. This entire ad callback pattern is replaced by a direct `SpendCoins` call. The ad-based extra bottle becomes a coin-based extra bottle.

**Note**: Story 9.6 (rewarded-ad-coins) will add a separate "watch ad to earn coins" feature — ads don't disappear entirely, they just move to coin earning rather than direct bottle granting.

### Key Files

| File | Action | Location |
|------|--------|----------|
| GameplayManager.cs | MODIFY | `Scripts/Game/Puzzle/` — replace ad flow with coin payment, add RefreshExtraBottleState |
| GameplayHUD.cs | MODIFY | `Scripts/Game/UI/Components/` — add UpdateExtraBottleState(), clean up UpdateDisplay() |
| CoinExtraBottleTests.cs | NEW | `Scripts/Tests/EditMode/` |

### Previous Story Intelligence (9.4)

- `Undo()` uses pop-then-spend pattern — extra bottle is simpler (no rollback needed)
- `RefreshUndoState()` helper pattern: get config, calculate cost, check affordability, update HUD
- `OnCoinBalanceChanged` handler already exists and calls `RefreshUndoState()` — add `RefreshExtraBottleState()` to it
- Code review cleaned up `UpdateDisplay()` to remove dead undo text — do the same for extra bottle text
- `CoinConfig.Default()` for cost lookup, `Services.TryGet<ICoinManager>()` for balance checks

### References

- [Source: _bmad-output/gdd.md#Coin Spending] — "Extra bottle (1st): 500, (2nd): 900, Max: 2"
- [Source: _bmad-output/gdd.md#Extra Bottle] — "Max 2 extra bottles per level"
- [Source: _bmad-output/project-context.md#Coin System Rules] — "ALL coin values in CoinConfig — never hardcode"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

- Build succeeded: 0 warnings, 0 errors across Core, Game, and Tests assemblies

### Completion Notes List

- Task 1: Replaced IAdManager.ShowRewardedAd() with ICoinManager.SpendCoins() in RequestExtraBottle(). Removed ad callback pattern — now a direct synchronous coin gate. All checks (max-2, SpendCoins) happen before state changes. Existing animated re-layout logic unchanged.
- Task 2: Added UpdateExtraBottleState(cost, canAfford, hasRemaining) to GameplayHUD. Removed extra bottle logic from UpdateDisplay() — now only handles move count. Added RefreshExtraBottleState() helper to GameplayManager. Wired at 5 update points: LoadLevel, RestartLevel, after bottle added, ResumeLevel, OnBalanceChanged.
- Task 3: Created CoinExtraBottleTests.cs with 7 tests covering escalation, cap, custom config, full sequence total, insufficient balance, and exact balance.

### Change Log

- 2026-03-22: Replaced ad-based extra bottle with coin-based, HUD integration, UpdateDisplay cleanup, tests
- 2026-03-22: Code review fix — removed dead `using JuiceSort.Game.Ads` import

### File List

- `Assets/Scripts/Game/Puzzle/GameplayManager.cs` — MODIFIED (coin payment gate in RequestExtraBottle, RefreshExtraBottleState, UpdateDisplay cleanup)
- `Assets/Scripts/Game/UI/Components/GameplayHUD.cs` — MODIFIED (added UpdateExtraBottleState, cleaned UpdateDisplay to moves-only)
- `Assets/Scripts/Tests/EditMode/CoinExtraBottleTests.cs` — NEW (7 tests)
- `Assets/Scripts/Tests/EditMode/CoinExtraBottleTests.cs.meta` — NEW

