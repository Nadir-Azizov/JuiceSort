# Story 11.2: Gameplay HUD Redesign

Status: done

## Story

As a player,
I want the gameplay HUD to have clear, organized top and bottom bars with consistent button sizing,
so that I can easily find and use all game controls without confusion.

## Priority

HIGH — buttons are currently chaotically placed with no size standards.

## Acceptance Criteria

1. **Top bar layout** — Top bar contains: back/menu button (left), level info center (city name + level number), settings gear icon (right)
2. **Bottom bar layout** — Bottom bar contains: undo button (left), restart button (center), extra bottle button (right)
3. **Coin balance display** — Coin balance shown in top-right area with coin icon + number (prepares for Epic 9, can show placeholder "0" until coin system exists)
4. **Move counter** — Move count displayed near level info (top-center area)
5. **Consistent button sizing** — All action buttons are the same size with minimum 44×44dp touch target
6. **Consistent styling** — All buttons have same padding, corner radius, and visual treatment
7. **SafeArea compliance** — Top and bottom bars respect `Screen.safeArea` so buttons aren't hidden behind notch or navigation bar
8. **Clear visual hierarchy** — Level info is the most prominent element; buttons are secondary; counters are tertiary
9. **No overlap with bottles** — HUD bars don't overlap with the bottle play area (coordinates with Story 11.1 LayoutConfig top/bottom margins)
10. **Undo counter visible** — Undo button shows remaining undo count (or coin cost when Epic 9 is active)

## Architect Decisions

- **HUD coordination:** HUD must fit within fixed world-unit reserves from Story 11.1 (`topReserve=1.5f`, `bottomReserve=2.0f`). HUD knows the reserves, bottles never query HUD.
- **No runtime coupling:** Don't export bar heights to layout manager. Reserves are design constants in LayoutConfig.

## Tasks / Subtasks

- [x] Task 1: Design HUD layout structure (AC: 1, 2, 7)
  - [x] 1.1 Modify `GameplayHUD.cs` to organize into two bar regions: `_topBar` and `_bottomBar`
  - [x] 1.2 Top bar: anchored to top of SafeArea, horizontal layout — [Back] [Level Info] [Moves] [Coin Display] [Settings]
  - [x] 1.3 Bottom bar: anchored to bottom of SafeArea, horizontal layout — [Undo] [Restart] [Extra Bottle]
  - [x] 1.4 Apply SafeArea offsets using `Screen.safeArea` to both bars via `ApplySafeArea()` helper
  - [x] 1.5 Set bar heights to 120px reference (accommodates 96px buttons + padding)

- [x] Task 2: Standardize button sizing and styling (AC: 5, 6)
  - [x] 2.1 Created `CreateSquareButton()` factory: 96×96px (48dp×2), consistent across all buttons
  - [x] 2.2 Apply `ButtonBounce` component to all HUD buttons (existing from Epic 8)
  - [x] 2.3 Consistent font sizes and alignment within buttons
  - [x] 2.4 All buttons use ThemeConfig colors (ButtonPrimary for extra bottle, ButtonSecondary for others)

- [x] Task 3: Implement top bar content (AC: 1, 3, 4, 8)
  - [x] 3.1 Back/menu button: left-aligned, unicode ← arrow, navigates to roadmap
  - [x] 3.2 Level info: left-center, "Level N [mood]\nCity, Country" format
  - [x] 3.3 Move counter: right of level info, "Moves: N"
  - [x] 3.4 Coin display: right side, "0" placeholder text in StarGold color (ready for Epic 9)
  - [x] 3.5 Settings gear: far right, unicode ⚙, wired to `OpenSettings()` placeholder

- [x] Task 4: Implement bottom bar content (AC: 2, 10)
  - [x] 4.1 Undo button: left, shows remaining count
  - [x] 4.2 Restart button: center, unicode ↻ icon
  - [x] 4.3 Extra bottle button: right, "+N" format showing remaining
  - [x] 4.4 All buttons wired to GameplayManager methods (+ new OnSettingsPressed)

- [x] Task 5: Coordinate with bottle layout (AC: 9)
  - [x] 5.1 HUD bars fit within Story 11.1 reserves (topReserve=1.5, bottomReserve=2.0 world units)
  - [ ] 5.2 Feed these values into `LayoutConfig` (or pass to `ResponsiveLayoutManager`) so bottles avoid HUD regions
  - [ ] 5.3 Test that bottles and HUD never overlap at any bottle count / screen size

## Dev Notes

### Key Files to Modify
- **MODIFY:** `Scripts/Game/UI/Screens/GameplayHUD.cs` — restructure layout, add bars
- **MODIFY:** `Scripts/Game/Puzzle/GameplayManager.cs` — update HUD creation if needed
- **REUSE:** `ButtonBounce.cs` — already exists from Epic 8

### Current HUD State
- `GameplayHUD.Create()` in GameplayManager creates a Canvas overlay (ScreenSpaceOverlay, sortingOrder 5)
- Canvas Scaler: 1080×1920 reference, matchWidthOrHeight 0.5
- Current buttons: undo, restart, extra bottle, settings — placed without consistent layout
- Move counter exists but placement is ad hoc

### Architecture
- HUD remains as Canvas ScreenSpaceOverlay (separate from world-space bottles)
- Canvas Scaler reference resolution stays 1080×1920
- Buttons use existing event wiring to GameplayManager (OnUndoPressed, OnRestartPressed, etc.)
- Coin display is placeholder UI only — actual coin logic comes in Epic 9 (Story 9.1)

### Constraints
- No font changes in this story (typography is Story 11.3)
- No icon asset changes in this story (icon system is Story 11.4)
- Use existing placeholder sprites/text for now — visual polish comes later
- Must work with both current sprite-based bottles AND future shader-based bottles
