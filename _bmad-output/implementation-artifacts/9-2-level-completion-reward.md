# Story 9.2: Level Completion Coin Reward

Status: done

## Story

As a player,
I want to earn coins when completing a level, with the reward scaled by difficulty,
so that harder levels feel more rewarding and I'm motivated to play efficiently.

## Acceptance Criteria

1. **Base reward on completion** — Player earns base coins (CoinConfig.BaseLevelReward = 75) when completing any level
2. **Difficulty scaling** — Reward increases with level difficulty (more colors/bottles = more coins)
3. **Move efficiency bonus** — Player earns bonus coins for efficient play (fewer moves = higher bonus, up to +25% of base)
4. **Star-based multiplier** — 3 stars earns full efficiency bonus, 2 stars earns partial, 1 star earns base only
5. **Reward shown on level complete screen** — The coin reward amount is displayed on the LevelCompleteScreen (e.g., "+95 coins")
6. **HUD updates** — Coin balance on gameplay HUD updates immediately after reward is granted
7. **Replay rewards** — Replaying a level still earns coins (no penalty for replays)
8. **All values from CoinConfig** — Base reward and bonus percent come from CoinConfig, never hardcoded

## Tasks / Subtasks

- [x] Task 1: Create CoinRewardCalculator utility (AC: 1, 2, 3, 4, 8)
  - [x] 1.1 Created `Scripts/Game/Economy/CoinRewardCalculator.cs` as static class
  - [x] 1.2 Method: `int CalculateReward(int stars, LevelDefinition definition, CoinConfig config)` — simplified signature (moveCount/optimal not needed since stars encode efficiency)
  - [x] 1.3 Base reward from config.BaseLevelReward (75)
  - [x] 1.4 Difficulty multiplier: `1.0 + (colorCount - 3) * 0.15f`, clamped ≥ 1.0
  - [x] 1.5 Efficiency bonus: 3 stars = full (25%), 2 stars = half (12.5%), 1 star = none
  - [x] 1.6 Returns `(int)(baseDifficultyReward + efficiencyBonus)`

- [x] Task 2: Integrate reward into level completion flow (AC: 1, 6, 7)
  - [x] 2.1 In `OnLevelComplete()`, after star calc, calls `CoinRewardCalculator.CalculateReward()`
  - [x] 2.2 Calls `coinMgr.AddCoins(coinReward)` — auto-triggers HUD update via OnBalanceChanged
  - [x] 2.3 Passes coinReward to `ShowWinOverlay()`

- [x] Task 3: Display reward on LevelCompleteScreen (AC: 5)
  - [x] 3.1 Added `_coinRewardText` field
  - [x] 3.2 Created gold-colored text UI element between info text and buttons
  - [x] 3.3 Updated `Show()` signature with `int coinReward = 0` default param
  - [x] 3.4 Displays "+N coins" when coinReward > 0

- [x] Task 4: Update ShowWinOverlay call chain (AC: 5)
  - [x] 4.1 `ShowWinOverlay()` now accepts `int coinReward` parameter
  - [x] 4.2 Passes coinReward to `completeScreen.Show()`
  - [x] 4.3 BootLoader wiring unchanged — `Show()` has default param for backward compat

- [x] Task 5: Write EditMode tests for CoinRewardCalculator (AC: 1-4, 8)
  - [x] 5.1 Created `Scripts/Tests/EditMode/CoinRewardCalculatorTests.cs` — 7 tests
  - [x] 5.2 Test: 1 star, 3 colors = 75 coins
  - [x] 5.3 Test: 3 stars, 3 colors = 93 coins (full bonus)
  - [x] 5.4 Test: 2 stars = 84 coins (half bonus)
  - [x] 5.5 Test: 5 colors > 3 colors (97 vs 75)
  - [x] 5.6 Test: custom config values used, not hardcoded

## Dev Notes

### Coin Reward Formula

```
difficultyMultiplier = 1.0 + (colorCount - 3) * 0.15
baseDifficultyReward = BaseLevelReward * difficultyMultiplier

efficiencyBonus = 0                          (1 star)
efficiencyBonus = baseDifficultyReward * MoveEfficiencyBonusPercent * 0.5  (2 stars)
efficiencyBonus = baseDifficultyReward * MoveEfficiencyBonusPercent        (3 stars)

totalReward = (int)(baseDifficultyReward + efficiencyBonus)
```

**Examples:**
- Level with 3 colors, 1 star: 75 × 1.0 = 75 coins
- Level with 3 colors, 3 stars: 75 × 1.0 + 75 × 0.25 = 93 coins
- Level with 5 colors, 3 stars: 75 × 1.3 + 97.5 × 0.25 = 97 + 24 = 121 coins

### Key Architecture Patterns

- **Static calculator:** `CoinRewardCalculator` follows `StarCalculator`, `DifficultyScaler` pattern — pure static, no MonoBehaviour, easy to test
- **Service access:** Use `Services.TryGet<ICoinManager>()` in GameplayManager — never hold a reference
- **HUD auto-update:** CoinManager.AddCoins() fires OnBalanceChanged, which HUD is already subscribed to (from Story 9.1) — no additional wiring needed
- **LevelCompleteScreen.Show():** Add optional `coinReward` param with default value 0 for backward compatibility

### Existing Infrastructure (from Story 9.1)

- `ICoinManager` / `CoinManager` — already registered and working
- `CoinConfig` with `BaseLevelReward` (75) and `MoveEfficiencyBonusPercent` (0.25f) — already exists
- HUD coin display — already subscribed to `OnBalanceChanged` event
- Auto-save — `AddCoins()` triggers `ProgressionManager.AutoSave()` automatically

### Key Files

| File | Action | Location |
|------|--------|----------|
| CoinRewardCalculator.cs | NEW | `Scripts/Game/Economy/` |
| GameplayManager.cs | MODIFY | `Scripts/Game/Puzzle/` — call reward calculator in OnLevelComplete, pass to ShowWinOverlay |
| LevelCompleteScreen.cs | MODIFY | `Scripts/Game/UI/Screens/` — add coin reward text display |
| CoinRewardCalculatorTests.cs | NEW | `Scripts/Tests/EditMode/` |

### Integration Points

- `GameplayManager.OnLevelComplete()` line ~546: after star calculation, before ShowWinOverlay — inject coin reward calculation and AddCoins call
- `GameplayManager.ShowWinOverlay()` line ~579: pass coinReward to LevelCompleteScreen.Show()
- `LevelCompleteScreen.Show()`: add coinReward parameter, display "+N coins" text

### Previous Story Intelligence (9.1)

- CoinManager.AddCoins() fires OnBalanceChanged → HUD auto-updates. No need to manually call UpdateCoinDisplay.
- ProgressionManager.AutoSave() is public and called by CoinManager.TriggerSave() — coin balance is included in the save.
- CoinConfig accessed via `CoinManager.Config` property or `CoinConfig.Default()`.

### References

- [Source: _bmad-output/gdd.md#Coin Earning] — reward tables
- [Source: _bmad-output/game-architecture.md#Coin Economy System] — service spec
- [Source: _bmad-output/project-context.md#Coin System Rules] — persistence rules

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Completion Notes
- CoinRewardCalculator: pure static, formula = base * difficulty + star-based efficiency bonus
- Simplified method signature — stars already encode move efficiency (via StarCalculator), no need to pass moveCount/optimal separately
- GameplayManager.OnLevelComplete() awards coins before showing overlay
- LevelCompleteScreen shows "+N coins" in gold below move stats
- Show() method backward compatible via default param `coinReward = 0`
- 7 EditMode tests covering all formula paths + custom config

### File List
- **NEW:** `src/JuiceSort/Assets/Scripts/Game/Economy/CoinRewardCalculator.cs`
- **NEW:** `src/JuiceSort/Assets/Scripts/Tests/EditMode/CoinRewardCalculatorTests.cs`
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — coin reward calculation + AddCoins in OnLevelComplete, coinReward param in ShowWinOverlay
- **MODIFIED:** `src/JuiceSort/Assets/Scripts/Game/UI/Screens/LevelCompleteScreen.cs` — added _coinRewardText, updated Show() with coinReward param, gold text UI

### Change Log
- 2026-03-22: Implemented level completion coin rewards with difficulty scaling, star-based efficiency bonus, and LevelCompleteScreen display

