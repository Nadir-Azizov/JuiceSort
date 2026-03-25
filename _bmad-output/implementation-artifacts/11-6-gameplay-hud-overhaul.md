# Story 11.6: Gameplay HUD Overhaul

Status: review

## Story

As a player,
I want the gameplay HUD to show coins top-left, a "Level N" label top-center, and an expandable settings gear top-right with toggle buttons, plus a styled bottom action panel,
so that the gameplay screen feels clean, modern, and consistent with top puzzle games.

## Priority

HIGH — Redesigns the HUD from Story 11.2's structure to match Magic Sort's proven gameplay UI pattern.

## Acceptance Criteria

1. **Top-left: Coin display** — Coin icon + balance number. Pulsing golden glow on icon. No other elements on the left.
2. **Top-center: Level label** — "Level N" text in a styled frame/pill shape. Prominent but not oversized.
3. **Top-right: Settings gear** — Gear icon button. When tapped, expands vertically to reveal 4 toggle buttons below it. Tap gear again to collapse.
4. **Settings panel buttons** — When expanded, shows vertically (top to bottom):
   - Music toggle (mute/unmute)
   - SFX toggle (mute/unmute)
   - Vibration toggle (enable/disable)
   - Restart level button
   - Exit level button (red, returns to Hub)
5. **Settings collapse** — Tapping gear when expanded collapses all buttons with smooth animation. Tapping anywhere on gameplay area also collapses.
6. **Bottom action panel** — Styled bar/pill containing:
   - Undo button (with remaining count or coin cost)
   - Add bottle button (with coin cost that increases per use)
7. **No restart in bottom bar** — Restart moved to expandable settings panel.
8. **No ad watch button** — Removed from gameplay HUD entirely.
9. **Move counter repositioned** — Move counter shown small near the level label (e.g., "Level 10 • 12 moves") or as a subtle secondary text below it. Not removed, just de-emphasized.
10. **Dark gradient background** — Gameplay uses dark blue/purple gradient (deep, like Magic Sort screenshot 4).
11. **SafeArea compliance** — All elements respect Screen.safeArea.
12. **No regressions** — Undo, restart, extra bottle, back/exit functionality all work correctly.

## Tasks / Subtasks

- [x] Task 1: Redesign top bar (AC: 1, 2, 3, 11)
  - [x] 1.1 Rewrite `GameplayHUD.cs` top bar: coin display (left), level label (center), settings gear (right)
  - [x] 1.2 Remove back button and old settings button from top bar
  - [x] 1.3 Coin display: coin icon (36px circle, gold) + balance text (StarGold color, 26px bold)
  - [x] 1.4 Level label: "Level N" in styled pill/frame (dark semi-transparent bg, 28px bold). Small move counter text below (16px secondary color)
  - [x] 1.5 Settings gear: 88×88px button, ButtonSecondary color, rounded

- [x] Task 2: Build expandable settings panel (AC: 3, 4, 5)
  - [x] 2.1 Create settings panel container anchored below gear button
  - [x] 2.2 Add 5 buttons vertically: Music, SFX, Vibration, Restart, Exit
  - [x] 2.3 Music/SFX toggles: green when on, gray when muted. Wire to AudioManager via IProgressionManager.
  - [x] 2.4 Vibration toggle: wire to PlayerPrefs("VibrationEnabled")
  - [x] 2.5 Restart button: triggers `OnRestartPressed` (same as current), auto-collapses settings
  - [x] 2.6 Exit button: red background, triggers `OnExitPressed` → `GoBackToHub()` via GameplayManager
  - [x] 2.7 Expand animation: buttons scale from 0 to 1, staggered (0.05s delay each), EaseOutBack coroutine
  - [x] 2.8 Collapse animation: reverse (0.03s stagger). Full-screen invisible blocker collapses on outside tap.
  - [x] 2.9 Track `_isSettingsOpen` bool, block gameplay taps while settings open (checked in `OnContainerTapped`)

- [x] Task 3: Redesign bottom action panel (AC: 6, 7, 8)
  - [x] 3.1 Create styled bottom panel: rounded pill shape (360×80), dark semi-transparent, centered
  - [x] 3.2 Undo button: icon + count/cost label. Uses existing `OnUndoPressed` wiring.
  - [x] 3.3 Add bottle button: icon + coin cost label. Cost increases per use (from CoinConfig).
  - [x] 3.4 Remove restart button from bottom bar (moved to settings panel)
  - [x] 3.5 Remove ad watch button from bottom bar (removed entirely from HUD)
  - [x] 3.6 Style buttons with consistent sizing within pill container

- [x] Task 4: Update background and polish (AC: 10, 12)
  - [x] 4.1 BackgroundManager already uses mood-aware dark gradient (from 11.3) — verified
  - [x] 4.2 Removed all references to old HUD elements (_adText, _adButton, OnAdWatchPressed, OnBackPressed, UpdateAdButtonState)
  - [x] 4.3 Updated `GameplayManager`: removed ad/back/settings wiring, added `OnExitPressed` → `GoBackToHub()`, added settings-open check in `OnContainerTapped`
  - [x] 4.4 Undo/extra bottle/restart all wired correctly through existing event system

- [x] Task 5: Visual polish pass — match mockup (AC: all visual)
  - [x] 5.1 Rounded corner sprites via `UIShapeUtils.WhiteRoundedRect()` on all pills/buttons (r=14..22, `Image.Type.Sliced`)
  - [x] 5.2 Circular coin icons via `UIShapeUtils.WhiteCircle(64)` on top-bar coin + mini coins
  - [x] 5.3 Disabled button alpha (CanvasGroup 0.4f) on undo/extra-bottle, `Transition.None` to avoid double-dimming
  - [x] 5.4 Coin pulse glow animation (30fps coroutine, SmoothStep alpha 0.15-0.55, 2.5s period)
  - [x] 5.5 Mood-aware pill backgrounds (Morning: `rgba(60,40,10)` / Night: black, varying alpha per element)
  - [x] 5.6 Extracted mood pill colors to `static readonly` constants (`MorningPillBg`, `NightPillBg`)

## Dev Notes

### Current HUD Structure (from Story 11.2)
```
Top bar:    [Back ←] [Level Info + Mood] [Moves: N] [Coin: 0] [Settings ⚙]
Bottom bar: [Undo ↶ N] [Ad ▶ N] [Restart ↻] [Extra Bottle +N]
```

### New HUD Structure
```
Top-left:   [Coin 💰 1250]
Top-center: [Level 10 • 12 moves]
Top-right:  [⚙] → expands to [⚙] [🎵] [🔊] [📳] [↻] [🚪]
Bottom:     [Undo ↶ 3] [+Bottle 🪙900]
```

### Key Files to MODIFY
- `Scripts/Game/UI/Components/GameplayHUD.cs` — Major rewrite of layout and content
- `Scripts/Game/Puzzle/GameplayManager.cs` — Update HUD creation, remove ad button wiring, update back navigation

### Fields to REMOVE from GameplayHUD
- `_adText`, `_adButton` (ad button removed)
- `OnAdWatchPressed` action (removed)
- `OnBackPressed` action (replaced by exit in settings panel)

### Fields to ADD
- `_settingsPanel` (RectTransform for expandable container)
- `_isSettingsOpen` (bool)
- `_musicToggle`, `_sfxToggle`, `_vibrationToggle` (Button references)
- Settings panel button references

### Existing Methods to UPDATE
- `UpdateDisplay(int moves)` — may simplify or remove move display
- `UpdateUndoState()` — keep, update button styling
- `UpdateExtraBottleState()` — keep, update button styling
- `UpdateAdButtonState()` — DELETE
- `UpdateCoinDisplay()` — keep, reposition
- `SetLevelInfo()` — simplify to just "Level N" text

### Architecture Notes
- Keep `CreateSquareButton()` factory but add variants for settings toggles
- Settings panel uses same Canvas overlay — no separate canvas needed
- Collapse on outside tap: use a full-screen invisible `Image` + `Button` behind the panel
- Animations via coroutines (scale + alpha, staggered)
- Wire exit button to `ScreenManager.TransitionTo(GameFlowState.Hub)`

### Dependencies
- Story 11.3 (TMPro + fonts) — text uses TextMeshProUGUI
- Story 11.4 (Hub screen) — exit button navigates to Hub, not MainMenu

### CRITICAL Anti-Patterns
1. **Do NOT remove the coin system integration** — keep `UpdateCoinDisplay()` working
2. **Do NOT change undo/extra-bottle game logic** — only UI changes
3. **Do NOT break `_isAnimating` pattern** — pour animation must still block input
4. **Do NOT use async for settings panel animation** — coroutines only
5. **Do NOT forget to collapse settings before pour** — if settings open and user somehow triggers action

### References
- [Source: Magic Sort screenshot 4] — Gameplay HUD layout reference
- [Source: Magic Sort screenshot 5] — Expandable settings panel reference
- [Source: _bmad-output/implementation-artifacts/11-2-hud-redesign.md] — Current HUD implementation
- [Source: _bmad-output/mockups/hub-screen-mockup.html] — Visual style reference

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Debug Log References
- Brace balance verified: GameplayHUD.cs (59/59), GameplayManager.cs (102/102)
- No remaining references to removed APIs (OnAdWatchPressed, OnBackPressed, GoBackToRoadmap, UpdateAdButtonState, RefreshAdButtonState)
- Unity EditMode tests not directly impacted (HUD is UI/MonoBehaviour, tests are pure logic)

### Completion Notes List
- **Task 1**: Complete top bar rewrite — coin icon + balance (left), level pill with move counter (center), settings gear (right). Removed back button and old layout.
- **Task 2**: Expandable settings panel with 5 vertically stacked buttons (Music, SFX, Vibration, Restart, Exit). EaseOutBack coroutine animations with staggered expand/collapse. Full-screen invisible blocker for outside-tap collapse. Toggle states read from IProgressionManager (music/sfx) and PlayerPrefs (vibration). Green/gray visual feedback on toggles.
- **Task 3**: Bottom action pill (360x80, dark semi-transparent) with Undo and Extra Bottle buttons only. Restart and ad watch removed from bottom bar.
- **Task 4**: Verified BackgroundManager uses mood-aware gradients. Cleaned all old API references. GameplayManager updated: renamed GoBackToRoadmap → GoBackToHub (navigates to MainMenu/Hub), added IsSettingsOpen check in OnContainerTapped, removed RefreshAdButtonState, added OnExitPressed event wiring.
- **Task 5**: Visual polish pass — rounded corners on all pills/buttons via UIShapeUtils (9-slice), circular coin icons, disabled button CanvasGroup alpha, coin pulse glow coroutine (30fps throttled), mood-aware pill backgrounds. Adversarial review: 7 findings fixed (throttled animation, no double-dimming, extracted constants, defensive CanvasGroup guard).

### Change Log
- 2026-03-24: Complete HUD overhaul — redesigned top bar, expandable settings panel, bottom action pill, GameplayManager API cleanup
- 2026-03-25: Visual polish pass — rounded sprites, circular coins, disabled alpha, coin pulse glow, mood-aware pills, review fixes

### File List
- Assets/Scripts/Game/UI/Components/GameplayHUD.cs (major rewrite)
- Assets/Scripts/Game/Puzzle/GameplayManager.cs (modified — API changes, exit/settings/ad cleanup)
