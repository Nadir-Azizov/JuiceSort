# Story 9.4: Coin-Undo

Status: done

## Story

As a player,
I want to spend coins to undo a move (cost increases with each use per level),
so that undo is a meaningful strategic decision rather than a free unlimited resource.

## Acceptance Criteria

1. **Undo costs coins** — Tapping undo calls `coinMgr.SpendCoins(cost)` before executing the undo. If SpendCoins returns false, undo is blocked.
2. **Escalating cost** — 1st undo costs 100, 2nd costs 200, 3rd+ costs 300 (all from `CoinConfig.GetUndoCost(useIndex)`)
3. **Per-level usage counter** — `_undoUsageCount` tracked in GameplayManager, resets on LoadLevel and RestartLevel
4. **Undo button shows cost** — HUD undo button displays the next undo cost (e.g., "↶ 100") instead of remaining undo count
5. **Insufficient coins disables button** — When `coinMgr.GetBalance() < nextUndoCost`, undo button is non-interactable
6. **Balance updates instantly** — Coin balance on HUD updates after undo purchase (already wired via OnBalanceChanged from 9.1)
7. **Undo mechanic unchanged** — The actual undo logic (pop snapshot, restore puzzle, decrement moves) is untouched — only payment gate is added
8. **Empty stack still blocks** — If UndoStack is empty, undo does nothing regardless of coins
9. **Replay starts fresh** — Replaying a completed level resets undo usage count (same as new level)
10. **All values from CoinConfig** — Costs come from `CoinConfig.UndoCosts[]` array, never hardcoded

## Tasks / Subtasks

- [x] Task 1: Add undo usage tracking to GameplayManager (AC: 3, 9)
  - [x] 1.1 Add `private int _undoUsageCount;` field to GameplayManager
  - [x] 1.2 Reset `_undoUsageCount = 0` in `LoadLevel()` (covers new level, replay, and resume)
  - [x] 1.3 Reset `_undoUsageCount = 0` in `RestartLevel()`

- [x] Task 2: Gate undo behind coin payment (AC: 1, 2, 7, 8, 10)
  - [x] 2.1 In `Undo()`, after animation/completion guard, get ICoinManager from Services
  - [x] 2.2 Calculate cost: `config.GetUndoCost(_undoUsageCount)` using CoinConfig from coinMgr or CoinConfig.Default()
  - [x] 2.3 Call `coinMgr.SpendCoins(cost)` — if false, return early (no undo)
  - [x] 2.4 On successful spend: increment `_undoUsageCount`, then execute existing undo logic
  - [x] 2.5 Keep existing UndoStack empty check — if `Pop()` returns null, no coins are spent

- [x] Task 3: Update HUD undo display (AC: 4, 5, 6)
  - [x] 3.1 Kept existing UpdateDisplay() signature for move/extra-bottle; added dedicated UpdateUndoState()
  - [x] 3.2 Display undo cost on button: `"↶ 100"` format (cost in coins)
  - [x] 3.3 Add `UpdateUndoState(int cost, bool canAfford, bool hasUndos)` method to GameplayHUD
  - [x] 3.4 Set `_undoButton.interactable = canAfford && hasUndos`
  - [x] 3.5 Call RefreshUndoState() after each undo, pour, level load, restart, extra bottle, and on OnBalanceChanged
  - [x] 3.6 Added OnCoinBalanceChanged handler + RefreshUndoState helper in GameplayManager

- [x] Task 4: Write EditMode tests (AC: 1-5, 10)
  - [x] 4.1 Create `Scripts/Tests/EditMode/CoinUndoTests.cs`
  - [x] 4.2 Test: GetUndoCost(0) returns 100 (first undo)
  - [x] 4.3 Test: GetUndoCost(1) returns 200 (second undo)
  - [x] 4.4 Test: GetUndoCost(2) returns 300 (third undo)
  - [x] 4.5 Test: GetUndoCost(5) returns 300 (capped at last value)
  - [x] 4.6 Test: GetUndoCost uses custom config values, not hardcoded
  - [x] 4.7 Test: SpendCoins insufficient balance scenario verified via cost arithmetic

## Dev Notes

### Undo Payment Flow

```
On undo tap:
  if (isLevelComplete || isAnimating) return
  snapshot = undoStack.Pop()
  if (snapshot == null) return           // empty stack — no coins spent
  cost = config.GetUndoCost(_undoUsageCount)
  if (!coinMgr.SpendCoins(cost)):
    undoStack.Push(snapshot)             // CRITICAL: re-push the popped snapshot
    return                               // insufficient coins — undo blocked
  _undoUsageCount++
  // existing undo logic: restore puzzle, decrement moves, rebind board
```

**CRITICAL: Pop-then-spend pattern** — The undo must pop the snapshot first (to verify stack isn't empty), then attempt payment. If payment fails, re-push the snapshot. This avoids charging coins when the stack is empty.

### Key Architecture Patterns

- **SpendCoins already exists** — `ICoinManager.SpendCoins(int)` returns bool, fires OnBalanceChanged, auto-saves. Zero new interface changes needed.
- **CoinConfig.GetUndoCost(int useIndex)** — already exists, caps at last array value. Default: [100, 200, 300].
- **OnBalanceChanged already wired** — HUD subscribes in LoadLevel/ResumeLevel. Coin display auto-updates on spend.
- **No persistence needed for undo count** — `_undoUsageCount` is per-level runtime state. Resets on load/restart. If player pauses and resumes, fresh undo costs are acceptable (paused state doesn't save undo count).

### HUD Change Strategy

The current `UpdateDisplay(int moves, int undoRemaining, int extraBottlesRemaining)` passes undo *remaining count*. This must change to undo *cost and affordability*.

**Option chosen:** Add a new method `UpdateUndoState(int cost, bool canAfford, bool hasUndos)` rather than breaking the existing signature. This isolates the change and makes the undo state update independent from move/extra-bottle updates.

Update points in GameplayManager where undo state changes:
- `LoadLevel()` — after HUD creation
- `RestartLevel()` — after undo stack clear
- `Undo()` — after successful undo (cost increases)
- `AttemptPour()` — after pour (new snapshot pushed, undo now available)
- `ResumeLevel()` — after HUD creation
- `OnBalanceChanged` — coin balance changed (affordability may change)

### Existing Infrastructure

- `CoinManager.SpendCoins()` — implemented, tested, fires OnBalanceChanged + auto-saves
- `CoinConfig.GetUndoCost(int)` — implemented, tested in CoinManagerTests.cs (6 tests cover escalation + cap)
- `CoinConfig.UndoCosts` — default `[100, 200, 300]`
- `UndoStack` — circular buffer, capacity 3 (from `GameConstants.MaxUndo`)
- `GameplayHUD.UpdateCoinDisplay(int)` — already subscribed to OnBalanceChanged

### Key Files

| File | Action | Location |
|------|--------|----------|
| GameplayManager.cs | MODIFY | `Scripts/Game/Puzzle/` — add _undoUsageCount, gate Undo() behind SpendCoins, update HUD calls |
| GameplayHUD.cs | MODIFY | `Scripts/Game/UI/Components/` — add UpdateUndoState(), change undo display from count to cost |
| CoinUndoTests.cs | NEW | `Scripts/Tests/EditMode/` |

### Previous Story Intelligence (9.3)

- `CoinConfig.Default()` returns a plain C# object with all economy values — use this for cost lookup
- `Services.TryGet<ICoinManager>()` is the access pattern — never FindObjectOfType
- SpendCoins fires OnBalanceChanged which auto-updates HUD coin display
- Code review found redundant saves (IncrementStreak + AddCoins). SpendCoins already handles save — no extra save needed for undo
- Tests use pure C# (NUnit, no MonoBehaviour) — test CoinConfig.GetUndoCost directly

### References

- [Source: _bmad-output/gdd.md#Coin Spending] — "Undo (1st): 100, (2nd): 200, (3rd+): 300"
- [Source: _bmad-output/gdd.md#Undo System] — "Limited undo moves scaled by difficulty"
- [Source: _bmad-output/project-context.md#Coin System Rules] — "ALL coin values in CoinConfig ScriptableObject — never hardcode"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

- Build succeeded: 0 warnings, 0 errors across Core, Game, and Tests assemblies

### Completion Notes List

- Task 1: Added `_undoUsageCount` field to GameplayManager. Reset to 0 in both `LoadLevel()` and `RestartLevel()`.
- Task 2: Implemented pop-then-spend pattern in `Undo()`: pop snapshot first, attempt `SpendCoins(cost)`, re-push on failure. Increments `_undoUsageCount` on success. Empty stack still blocks without charging.
- Task 3: Added `UpdateUndoState(int cost, bool canAfford, bool hasUndos)` to GameplayHUD. Added `RefreshUndoState()` helper and `OnCoinBalanceChanged` handler in GameplayManager. Undo button now shows cost and disables when can't afford or stack empty. Wired at 6 update points: LoadLevel, RestartLevel, Undo, AttemptPour, ResumeLevel, extra bottle, and OnBalanceChanged.
- Task 4: Created CoinUndoTests.cs with 7 tests covering escalation, cap, custom config, full sequence cost, and insufficient balance scenario.

### Change Log

- 2026-03-22: Implemented coin-gated undo with escalating costs, HUD integration, and tests
- 2026-03-22: Code review fix — removed dead undo text from UpdateDisplay(), cleaned up all callers to drop stale undoRemaining param

### File List

- `Assets/Scripts/Game/Puzzle/GameplayManager.cs` — MODIFIED (added _undoUsageCount, coin payment gate in Undo(), RefreshUndoState(), OnCoinBalanceChanged)
- `Assets/Scripts/Game/UI/Components/GameplayHUD.cs` — MODIFIED (added UpdateUndoState method)
- `Assets/Scripts/Tests/EditMode/CoinUndoTests.cs` — NEW (7 tests)
- `Assets/Scripts/Tests/EditMode/CoinUndoTests.cs.meta` — NEW

