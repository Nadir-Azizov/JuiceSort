# Story 11.6: Gameplay HUD Overhaul

Status: ready-for-dev

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

- [ ] Task 1: Redesign top bar (AC: 1, 2, 3, 11)
  - [ ] 1.1 Rewrite `GameplayHUD.cs` top bar: coin display (left), level label (center), settings gear (right)
  - [ ] 1.2 Remove back button and old settings button from top bar
  - [ ] 1.3 Coin display: coin icon (24px circle, gold gradient) + balance text (StarGold color, 17px)
  - [ ] 1.4 Level label: "Level N" in styled pill/frame (dark semi-transparent bg, rounded corners, 22px bold). Small move counter text below or inline (e.g., "Level 10 • 12 moves" or small "Moves: 12" text below, 14-16px secondary color)
  - [ ] 1.5 Settings gear: 44×44px button, purple-tinted background, rounded corners

- [ ] Task 2: Build expandable settings panel (AC: 3, 4, 5)
  - [ ] 2.1 Create settings panel container anchored below gear button
  - [ ] 2.2 Add 5 buttons vertically: Music, SFX, Vibration, Restart, Exit
  - [ ] 2.3 Music/SFX toggles: green when on, gray when muted. Wire to AudioManager.
  - [ ] 2.4 Vibration toggle: wire to `Handheld.Vibrate()` setting (store in PlayerPrefs)
  - [ ] 2.5 Restart button: triggers `OnRestartPressed` (same as current)
  - [ ] 2.6 Exit button: red background, triggers return to Hub via ScreenManager
  - [ ] 2.7 Expand animation: buttons scale from 0 to 1, staggered (0.05s delay each), EaseOutBack
  - [ ] 2.8 Collapse animation: reverse. Also collapse when tapping outside panel.
  - [ ] 2.9 Track `_isSettingsOpen` bool, block gameplay taps while settings open

- [ ] Task 3: Redesign bottom action panel (AC: 6, 7, 8)
  - [ ] 3.1 Create styled bottom panel: rounded pill shape, dark semi-transparent, centered
  - [ ] 3.2 Undo button: icon + count/cost label. Uses existing `OnUndoPressed` wiring.
  - [ ] 3.3 Add bottle button: icon + coin cost label. Cost increases per use (from CoinConfig).
  - [ ] 3.4 Remove restart button from bottom bar
  - [ ] 3.5 Remove ad watch button from bottom bar
  - [ ] 3.6 Style buttons with consistent sizing, golden borders, slight glow

- [ ] Task 4: Update background and polish (AC: 10, 12)
  - [ ] 4.1 Ensure BackgroundManager uses dark gradient for gameplay (already mood-aware from 11.3)
  - [ ] 4.2 Remove any references to old HUD elements (move counter text, ad button, etc.)
  - [ ] 4.3 Update `GameplayManager` to work with new HUD API (remove OnAdWatchPressed, OnBackPressed calls if refactored)
  - [ ] 4.4 Verify undo/extra bottle/restart all still function correctly

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

### Debug Log References

### Completion Notes List

### File List
