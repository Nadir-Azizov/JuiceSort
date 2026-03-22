# Story 9.6: Rewarded Ad Coins

Status: done

## Story

As a player,
I want to watch a rewarded ad to earn coins,
so that I have a free way to earn coins when my balance is too low for undo or extra bottle.

## Acceptance Criteria

1. **Watch ad earns coins** — After successfully watching a rewarded ad, player receives `CoinConfig.AdRewardAmount` (250) coins via `coinMgr.AddCoins()`
2. **Ad button on HUD** — A "watch ad" button is visible on the gameplay HUD bottom bar
3. **Ad availability check** — Button is only interactable when `adManager.IsAdAvailable` is true
4. **Balance updates instantly** — Coin balance on HUD updates after ad reward (via OnBalanceChanged)
5. **Ad failure handled gracefully** — If ad fails to show or user cancels, no coins are awarded and no error shown to player
6. **Button disabled during animation** — Ad button blocked when `_isAnimating` or `_isLevelComplete`
7. **Values from CoinConfig** — Reward amount comes from `CoinConfig.AdRewardAmount`, never hardcoded
8. **Multiple views per level** — Player can watch multiple ads per level (no per-level limit on ad watching)
9. **Works during gameplay** — Ad can be watched at any time during active gameplay (not just when stuck)

## Tasks / Subtasks

- [x] Task 1: Add "Watch Ad" button to GameplayHUD (AC: 2, 3)
  - [x] 1.1 Add `_adButton` (Button) and `_adText` (Text) fields to GameplayHUD
  - [x] 1.2 Create ad button in `CreateBottomBarContent()` — positioned between Undo and Restart at 30% anchor
  - [x] 1.3 Display coin reward on button: "▶ 250" format
  - [x] 1.4 Wire `OnAdWatchPressed` action delegate (same pattern as OnUndoPressed)
  - [x] 1.5 Add `UpdateAdButtonState(bool isAvailable)` method to GameplayHUD

- [x] Task 2: Implement ad-for-coins flow in GameplayManager (AC: 1, 4, 5, 6, 7, 8, 9)
  - [x] 2.1 Add `WatchAdForCoins()` method to GameplayManager
  - [x] 2.2 Guard: return if `_isLevelComplete || _isAnimating`
  - [x] 2.3 Get IAdManager from Services, call `ShowRewardedAd()`
  - [x] 2.4 In `onRewarded` callback: get CoinConfig, call `coinMgr.AddCoins(config.AdRewardAmount)`
  - [x] 2.5 In `onFailed` callback: log and do nothing (graceful failure)
  - [x] 2.6 Wire `_hud.OnAdWatchPressed = WatchAdForCoins` in LoadLevel and ResumeLevel

- [x] Task 3: Refresh ad button state (AC: 3, 6)
  - [x] 3.1 Add `RefreshAdButtonState()` helper to GameplayManager
  - [x] 3.2 Check `adManager.IsAdAvailable` for button interactability
  - [x] 3.3 Call `RefreshAdButtonState()` in LoadLevel, RestartLevel, ResumeLevel, after ad completes

- [x] Task 4: Write EditMode tests (AC: 1, 7)
  - [x] 4.1 Create `Scripts/Tests/EditMode/CoinAdRewardTests.cs`
  - [x] 4.2 Test: AdRewardAmount default is 250
  - [x] 4.3 Test: custom AdRewardAmount respected
  - [x] 4.4 Test: AdRewardAmount is positive (sanity check)

## Dev Notes

### Ad-for-Coins Flow

```
On ad button tap:
  if (isLevelComplete || isAnimating) return
  if (!adManager available) return
  adManager.ShowRewardedAd(
    onRewarded: () =>
      coinMgr.AddCoins(config.AdRewardAmount)  // 250 coins
      RefreshAdButtonState()
      // OnBalanceChanged fires → HUD coin display + undo/extraBottle affordability auto-update
    onFailed: () =>
      Debug.Log("Ad failed")  // silent failure
  )
```

### Key Architecture Patterns

- **IAdManager.ShowRewardedAd()** — callback-based, async. Takes `onRewarded` and `onFailed` actions.
- **AdManager placeholder** — currently immediately invokes onRewarded (no real ad SDK). Production will integrate Google AdMob.
- **AddCoins fires OnBalanceChanged** — this auto-updates coin display AND triggers RefreshUndoState + RefreshExtraBottleState (button affordability updates for free).
- **No spending involved** — this is purely earning. No SpendCoins, no rollback needed.
- **CoinConfig.AdRewardAmount** — already exists, defaults to 250.

### HUD Button Placement

Current bottom bar: `[Undo]  [Restart]  [Extra Bottle]`
After this story: `[Undo]  [▶ 250]  [Restart]  [Extra Bottle]`

Place the ad button between Undo and Restart. Use a slightly different anchor position — the bottom bar uses manual anchoring (AnchorLeft, AnchorCenter, AnchorRight). The ad button can be placed at ~30% horizontal position.

### Key Files

| File | Action | Location |
|------|--------|----------|
| GameplayHUD.cs | MODIFY | `Scripts/Game/UI/Components/` — add ad button, UpdateAdButtonState() |
| GameplayManager.cs | MODIFY | `Scripts/Game/Puzzle/` — add WatchAdForCoins(), RefreshAdButtonState(), wire button |
| CoinAdRewardTests.cs | NEW | `Scripts/Tests/EditMode/` |

### Previous Story Intelligence (9.5)

- OnCoinBalanceChanged already calls RefreshUndoState() + RefreshExtraBottleState() — when ad grants coins, undo/extra-bottle buttons auto-refresh affordability
- UpdateDisplay() now only handles move count — clean separation
- HUD buttons use CreateSquareButton() factory with AnchorLeft/Center/Right helpers
- Ad button follows same pattern: CreateSquareButton + anchor positioning + onClick listener

### References

- [Source: _bmad-output/gdd.md#Coin Earning] — "Watch rewarded ad: 200-300 coins"
- [Source: _bmad-output/gdd.md#Monetization] — "No forced ads, rewarded video only"
- [Source: _bmad-output/project-context.md#Coin System Rules] — "CoinConfig.AdRewardAmount for ad reward value"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

- Build succeeded: 0 warnings, 0 errors across Core, Game, and Tests assemblies

### Completion Notes List

- Task 1: Added _adButton and _adText fields, OnAdWatchPressed delegate, and ad button in CreateBottomBarContent() anchored at 30% horizontal between Undo and Restart. Button shows "▶ 250" with StarGold color. Added UpdateAdButtonState(bool isAvailable).
- Task 2: Added WatchAdForCoins() to GameplayManager with _isLevelComplete/_isAnimating guards. Uses IAdManager.ShowRewardedAd() callback pattern. onRewarded adds CoinConfig.AdRewardAmount coins via AddCoins(). onFailed logs silently. Wired in both LoadLevel and ResumeLevel.
- Task 3: Added RefreshAdButtonState() helper checking adManager.IsAdAvailable. Called in LoadLevel, RestartLevel, ResumeLevel, and after ad completes. OnCoinBalanceChanged already refreshes undo/extra-bottle affordability via AddCoins → OnBalanceChanged chain.
- Task 4: Created CoinAdRewardTests.cs with 3 tests: default value (250), custom value, positive sanity check.

### Change Log

- 2026-03-22: Implemented rewarded-ad-coins with HUD button, ad flow, and tests — final Epic 9 story
- 2026-03-22: Code review fixes — ad button text now reads from CoinConfig.AdRewardAmount (was hardcoded "250"), updated class comment

### File List

- `Assets/Scripts/Game/Puzzle/GameplayManager.cs` — MODIFIED (added WatchAdForCoins, RefreshAdButtonState, re-added Ads using, wired button)
- `Assets/Scripts/Game/UI/Components/GameplayHUD.cs` — MODIFIED (added ad button, OnAdWatchPressed, UpdateAdButtonState)
- `Assets/Scripts/Tests/EditMode/CoinAdRewardTests.cs` — NEW (3 tests)
- `Assets/Scripts/Tests/EditMode/CoinAdRewardTests.cs.meta` — NEW

