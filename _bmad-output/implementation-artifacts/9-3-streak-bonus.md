# Story 9.3: Consecutive Win Streak Bonus

Status: done

## Story

As a player,
I want to earn bonus coins for consecutive level wins,
so that I'm rewarded for sustained performance and motivated to keep playing.

## Acceptance Criteria

1. **Streak tracking** — The game tracks how many levels the player completes consecutively without failure
2. **3-win streak bonus** — After completing 3 consecutive levels, player earns 200 bonus coins (CoinConfig.StreakBonus3)
3. **5-win streak bonus** — After completing 5 consecutive levels, player earns 500 bonus coins (CoinConfig.StreakBonus5)
4. **Streak resets on failure** — If the player fails a level (stuck state, no valid moves), the streak resets to 0
5. **Streak persists** — Streak count survives app close and reopen (saved in SaveData)
6. **Streak does NOT reset on restart** — Restarting a level is a local retry, not a failure — streak is preserved
7. **Streak bonus shown** — When a streak bonus is earned, it's included in the coin reward shown on LevelCompleteScreen
8. **Replay counts** — Replaying a completed level for star improvement counts toward the streak
9. **Values from CoinConfig** — Streak bonus amounts come from CoinConfig, never hardcoded

## Tasks / Subtasks

- [x] Task 1: Add streak tracking to CoinManager (AC: 1, 4, 6)
  - [x] 1.1 Add `_streakCount` private field to CoinManager
  - [x] 1.2 Add `int StreakCount { get; }` to ICoinManager interface
  - [x] 1.3 Add `void IncrementStreak()` method — increments streak, auto-saves
  - [x] 1.4 Add `void ResetStreak()` method — sets streak to 0, auto-saves
  - [x] 1.5 Load streak from SaveData in `Start()` (alongside coinBalance)

- [x] Task 2: Add streak to persistence (AC: 5)
  - [x] 2.1 Add `public int consecutiveWinStreak;` field to SaveData.cs
  - [x] 2.2 Update `SaveData.FromProgressionData()` to accept streak count (add parameter or read from ICoinManager like coinBalance)
  - [x] 2.3 Update `ProgressionManager.AutoSave()` to include streak count from ICoinManager
  - [x] 2.4 Update `CoinManager.Start()` to load streak from SaveData

- [x] Task 3: Calculate streak bonus in reward flow (AC: 2, 3, 7, 9)
  - [x] 3.1 In `GameplayManager.OnLevelComplete()`, after base coin reward, call `coinMgr.IncrementStreak()`
  - [x] 3.2 Check if new streak count == 3 → add CoinConfig.StreakBonus3 (200) to reward
  - [x] 3.3 Check if new streak count == 5 → add CoinConfig.StreakBonus5 (500) to reward
  - [x] 3.4 Add streak bonus to total coinReward before calling AddCoins
  - [x] 3.5 Pass total reward (base + streak) to ShowWinOverlay for display

- [x] Task 4: Reset streak on failure (AC: 4, 6)
  - [x] 4.1 Verified: no failure/stuck detection exists in GameplayManager (known limitation — ResetStreak() ready but deferred until stuck-state dialog is added)
  - [x] 4.2 ResetStreak() implemented in CoinManager — no caller yet (no stuck-state trigger exists)
  - [x] 4.3 Verify: RestartLevel() does NOT reset streak (it's a local retry)
  - [x] 4.4 Verify: GoBackToRoadmap() does NOT reset streak (player may resume later)

- [x] Task 5: Write EditMode tests (AC: 1-5, 9)
  - [x] 5.1 Create `Scripts/Tests/EditMode/StreakTests.cs`
  - [x] 5.2 Test: streak starts at 0
  - [x] 5.3 Test: IncrementStreak increases count
  - [x] 5.4 Test: ResetStreak sets count to 0
  - [x] 5.5 Test: streak bonus at 3 wins returns correct amount from config
  - [x] 5.6 Test: streak bonus at 5 wins returns correct amount
  - [x] 5.7 Test: no bonus at counts other than 3 and 5

## Dev Notes

### Streak Bonus Logic

```
On level complete:
  streak++
  baseReward = CoinRewardCalculator.CalculateReward(stars, definition, config)
  streakBonus = 0
  if streak == 3: streakBonus = config.StreakBonus3 (200)
  if streak == 5: streakBonus = config.StreakBonus5 (500)
  totalReward = baseReward + streakBonus
  coinMgr.AddCoins(totalReward)

On failure (stuck state):
  streak = 0
```

### Key Architecture Patterns

- **Streak lives in CoinManager** — it's coin economy state, not progression state
- **Persistence via SaveData** — same pattern as coinBalance (ProgressionManager.AutoSave reads from ICoinManager)
- **ICoinManager interface** — add StreakCount getter + IncrementStreak/ResetStreak methods
- **No streak reset on restart** — RestartLevel() doesn't touch progression, so streak stays
- **Streak bonus is additive** — added on top of base reward from CoinRewardCalculator

### Failure Detection

The current codebase doesn't have explicit stuck-state detection. The player either:
- Completes the level (OnLevelComplete fires)
- Restarts manually
- Goes back to roadmap

**For now:** Only reset streak if we add explicit failure detection in the future. Streak resets only if a future "you're stuck" dialog is implemented. For MVP, streak only resets on explicit failure — which currently doesn't exist as an auto-detected state. Document this as a known limitation.

### Existing Infrastructure

- `CoinConfig.StreakBonus3` (200) and `CoinConfig.StreakBonus5` (500) — already exist
- `CoinManager.AddCoins()` — fires OnBalanceChanged, auto-saves
- `SaveData.coinBalance` — pattern to follow for adding `consecutiveWinStreak`
- `ProgressionManager.AutoSave()` — already reads `ICoinManager.GetBalance()`, will also read streak

### Key Files

| File | Action | Location |
|------|--------|----------|
| ICoinManager.cs | MODIFY | `Scripts/Core/Interfaces/` — add StreakCount, IncrementStreak, ResetStreak |
| CoinManager.cs | MODIFY | `Scripts/Game/Economy/` — implement streak tracking |
| SaveData.cs | MODIFY | `Scripts/Game/Save/` — add consecutiveWinStreak field |
| ProgressionManager.cs | MODIFY | `Scripts/Game/Progression/` — include streak in AutoSave |
| GameplayManager.cs | MODIFY | `Scripts/Game/Puzzle/` — call IncrementStreak + calculate bonus in OnLevelComplete |
| StreakTests.cs | NEW | `Scripts/Tests/EditMode/` |

### Previous Story Intelligence (9.2)

- CoinRewardCalculator returns base + efficiency bonus (no streak). Streak bonus is calculated separately in GameplayManager and added to total.
- `coinMgr.AddCoins(totalReward)` is called once with the combined amount — not separately for base and streak.
- LevelCompleteScreen.Show() already accepts coinReward param — streak bonus is included in the total shown.

### References

- [Source: _bmad-output/gdd.md#Coin Earning] — "Consecutive win streak (3): 200, (5): 500"
- [Source: _bmad-output/project-context.md#Coin System Rules] — "Streak count resets on level failure, persists across sessions"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

- Build succeeded: 0 warnings, 0 errors across Core, Game, and Tests assemblies

### Completion Notes List

- Task 1: Added `StreakCount`, `IncrementStreak()`, `ResetStreak()` to ICoinManager interface and CoinManager implementation. Streak stored as `_streakCount` field, loaded from SaveData in `Start()`.
- Task 2: Added `consecutiveWinStreak` field to SaveData. Updated `FromProgressionData()` with streak parameter. ProgressionManager.AutoSave() now reads `StreakCount` from ICoinManager.
- Task 3: Created `StreakBonusCalculator` static class for testable pure logic. GameplayManager.OnLevelComplete() now increments streak, calculates streak bonus via calculator, and adds to total reward before `AddCoins()`. Total reward (base + streak) passed to ShowWinOverlay.
- Task 4: Verified RestartLevel() and GoBackToRoadmap() do NOT reset streak. No explicit stuck-state detection exists in current codebase — streak reset on failure is a known limitation documented in Dev Notes, to be implemented when stuck-state dialog is added.
- Task 5: Created StreakTests.cs with 10 tests covering: config default values, bonus at streak 3 and 5, no bonus at other counts (0, 1, 2, 4, 6, 10), and custom config values.

**Code Review Fixes (2026-03-22):**
- [H1] Added 5 streak counting flow tests (StreakTests.cs) — simulates full increment/reset/sequence patterns through StreakBonusCalculator. Total: 15 tests.
- [M1] Corrected misleading Task 4.1/4.2 checkboxes — now honestly document that ResetStreak() is implemented but has no caller (stuck-state detection deferred).
- [M2] Removed redundant TriggerSave() from IncrementStreak() — was causing 3 file writes on level complete (CompleteLevelWithStars + IncrementStreak + AddCoins). Now down to 2 (the AddCoins save captures streak state).

### Change Log

- 2026-03-22: Implemented streak tracking, persistence, bonus calculation, and tests
- 2026-03-22: Code review fixes — added flow tests, corrected task documentation, eliminated redundant save

### File List

- `Assets/Scripts/Core/Interfaces/ICoinManager.cs` — MODIFIED (added StreakCount, IncrementStreak, ResetStreak)
- `Assets/Scripts/Game/Economy/CoinManager.cs` — MODIFIED (implemented streak tracking)
- `Assets/Scripts/Game/Economy/StreakBonusCalculator.cs` — NEW (pure logic streak bonus calculation)
- `Assets/Scripts/Game/Economy/StreakBonusCalculator.cs.meta` — NEW
- `Assets/Scripts/Game/Save/SaveData.cs` — MODIFIED (added consecutiveWinStreak field)
- `Assets/Scripts/Game/Progression/ProgressionManager.cs` — MODIFIED (streak in AutoSave)
- `Assets/Scripts/Game/Puzzle/GameplayManager.cs` — MODIFIED (streak bonus in OnLevelComplete)
- `Assets/Scripts/Tests/EditMode/StreakTests.cs` — NEW (10 tests)
- `Assets/Scripts/Tests/EditMode/StreakTests.cs.meta` — NEW

