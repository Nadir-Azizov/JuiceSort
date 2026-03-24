# Story 11.4: Main Hub Screen

Status: done

## Story

As a player,
I want to land on a hub screen with a prominent Level N play button, coin display, settings gear, and bottom navigation bar when I open the app,
so that I can start playing with a single tap instead of navigating through menus.

## Priority

HIGH — Removes the useless Play/Settings main menu. Matches industry standard (Magic Sort, SortPuz, Water Sort Puzzle all use this pattern).

## Acceptance Criteria

1. **No MainMenu screen** — The old MainMenuScreen.cs is deleted. App launches directly to the hub. HubScreen registers under `GameFlowState.MainMenu` enum value.
2. **Level N play button** — Large green gradient pill button (~84% screen width) centered on screen showing "Level {N}" (90px bold white). One tap starts gameplay. 3D extruded shadow (3 layers), top tint highlight, shine dots, pulsing green glow aura behind button.
3. **Coin display** — Top-left: dark pill background with gold circle coin icon ("$") + balance number in gold (46px). ~~Pulsing glow on coin icon~~ (not implemented — only play button has glow pulse).
4. **Settings gear** — Top-right: PNG sprite button loaded from `Resources/Icons/icon_settings`. Taps open settings screen via `GoSettings()`.
5. **City info** — Center area shows "NEXT DESTINATION" label (32px, letter spacing 6) + city name (72px bold white with 4-layer 3D extruded purple depth text behind it). Refresh() loads live data from CityAssigner.
6. **~~Decorative bottles~~** — DEFERRED to future polish pass. Skip for now to reduce scope.
7. **Bottom navigation bar** — 5 icon-only buttons: Shop, Leaders, Home (active/highlighted), Teams, Collect. Home raised with golden dot indicator and purple glow. Icons loaded as flat PNG sprites from `Resources/Icons/flat_icon_*`. No labels, no SOON badges, no dimming — clean icon-only design. Golden top border line with glow.
8. **Roadmap access** — Tapping center area (13%-87% height) opens roadmap. Roadmap button (PNG sprite from `Resources/Icons/icon_worldmap`) below settings gear. "TAP ANYWHERE FOR ROADMAP" hint text (15px) with pulse animation (alpha 0.15→0.55).
9. **Gradient background** — Hardcoded 6-stop sunset gradient (dark purple → purple → magenta → orange-red → orange → golden), not using ThemeConfig. Static, not mood-aware.
10. **Sparkles and shine** — Simplified to: play button glow pulse, hint text alpha pulse, nav glow cubic-bezier pulse. FloatingLights from Epic 8 provide ambient particles globally.
11. **Screen transitions** — Hub uses existing ScreenManager transition system (crossfade + slide).

## Tasks / Subtasks

- [x] Task 1: Remove MainMenuScreen and update flow (AC: 1, 11)
  - [x] 1.1 Reused `GameFlowState.MainMenu` enum value — HubScreen registers under it, avoiding breaking changes
  - [x] 1.2 Created `HubScreen.cs` in `Scripts/Game/UI/Screens/`
  - [x] 1.3 BootLoader now creates `HubScreen.Create()` and registers under `GameFlowState.MainMenu`
  - [x] 1.4 MainMenuScreen.cs deleted (and .meta file)

- [x] Task 2: Build hub screen layout (AC: 2, 3, 4, 5, 9)
  - [x] 2.1 Canvas: ScreenSpaceOverlay, 1080×1920 reference, matchWidthOrHeight 0.5
  - [x] 2.2 Gradient background: hardcoded 6-stop sunset gradient texture (512px), not using ThemeConfig
  - [x] 2.3 Top bar: coin display (left, dark pill bg, gold circle icon + text) + settings gear (right, PNG sprite via `SpriteBtn`). SafeArea container.
  - [x] 2.4 Center: "NEXT DESTINATION" label (32px, white, letter spacing 6) + city name (72px bold white, 4-layer 3D purple depth text)
  - [x] 2.5 Refresh() loads current level from IProgressionManager, city from CityAssigner, coins from ICoinManager

- [x] Task 3: Build Level N play button (AC: 2, 10)
  - [x] 3.1 Large green button (~70% screen width), solid green with shadow below
  - [x] 3.2 "Level {N}" text in bold white, 90px (with FaceDilate + outline for extra weight)
  - [x] 3.3 Top highlight shine (white overlay on upper half of button)
  - [x] 3.4 Glow aura behind button (green semi-transparent, pulsing via coroutine)
  - [x] 3.5 Sparkle effects deferred (kept simpler — glow pulse + highlight sufficient for now)
  - [x] 3.6 3D shadow: darker green rect behind button at slight offset
  - [x] 3.7 On tap: calls GameplayManager.StartLevel(_currentLevel) + ScreenManager.TransitionTo(Playing)

- [x] ~~Task 4: Build decorative bottles (AC: 6)~~ — DEFERRED to future polish

- [x] Task 5: Build bottom navigation bar (AC: 7)
  - [x] 5.1 5 nav items with flat PNG icons: Shop, Leaders, Home, Teams, Collect (loaded from `Resources/Icons/flat_icon_*`)
  - [x] 5.2 Home has active state: raised position, purple glow behind icon, golden dot indicator below
  - [x] 5.3 ~~SOON badges~~ — removed. Clean icon-only design, no labels or badges
  - [x] 5.4 ~~Dimming~~ — removed. All icons show at full original color
  - [x] 5.5 Transparent bar (no background) with golden top border line + gold glow effect

- [x] Task 6: Roadmap access and ambient effects (AC: 8, 10)
  - [x] 6.1 Roadmap button below settings (PNG sprite `Icons/icon_worldmap` via `SpriteBtn`, ButtonBounce)
  - [x] 6.2 Center area tap (13%-87% height) → roadmap via transparent Button overlay
  - [x] 6.3 "TAP ANYWHERE FOR ROADMAP" hint text with pulse coroutine (alpha 0.2→0.55)
  - [x] 6.4-6.6 Ambient sparkles/bokeh/bubbles — simplified to coin glow + play button glow coroutines. FloatingLights already created globally in BootLoader.

## Dev Notes

### Current Flow (to be changed)
```
Boot → MainMenu (Play/Settings) → Roadmap → Tap level → Gameplay
```
### New Flow
```
Boot → Hub (Level N + bottom nav) → Tap Level N → Gameplay
                                   → Tap background → Roadmap (X to close) → Hub
```

### Key Files to MODIFY
- `Scripts/Game/UI/ScreenManager.cs` — Update initial state, add Hub transitions
- `Scripts/Game/UI/GameFlowState.cs` — Add/rename state for Hub (or reuse MainMenu)
- `Scripts/Game/Puzzle/GameplayManager.cs` — Update flow if it references MainMenu

### Key Files to CREATE
- `Scripts/Game/UI/Screens/HubScreen.cs` — New hub screen

### Key Files DELETED
- `Scripts/Game/UI/Screens/MainMenuScreen.cs` — Deleted, replaced by HubScreen
- `Scripts/Game/UI/Screens/MainMenuScreen.cs.meta` — Deleted

### Design Reference
- Mockup: `_bmad-output/mockups/hub-screen-mockup.html` — open in browser for exact layout
- Fonts: Baloo 2 / Fredoka (from mockup), use whatever TMPro font was set up in Story 11.3
- Colors: All from ThemeConfig (ButtonPrimary for play button green, StarGold for coins, TextPrimary/Secondary)

### Architecture Compliance
- Programmatic UI via `Create()` factory — no prefabs
- ScreenManager handles transitions (crossfade + slide, 0.3s)
- Service Locator for save data access: `Services.TryGet<ISaveManager>()`
- ThemeConfig for all colors and fonts
- ButtonBounce on interactive buttons
- SafeArea for top bar positioning

### Animation Patterns (coroutine-based)
- **Shine sweep:** Animate a UI Image (white gradient) anchoredPosition from left to right, repeat
- **Pulse glow:** Animate CanvasGroup alpha or Image color alpha (sin wave)
- **Float:** Animate RectTransform anchoredPosition.y (sin wave, per-bottle phase offset)
- **Sparkle:** Animate scale + rotation + alpha (0→1→0 cycle, staggered)
- All animations via coroutines — no DOTween, no async

### Dependencies
- Story 11.3 must be done first (TMPro fonts + gradient backgrounds)
- Current level number from progression/save system (already exists)
- City data from level generation (already exists via `CityDatabase`)

### CRITICAL Anti-Patterns
1. **Do NOT create a new scene** — Hub is a Canvas overlay, same as current MainMenu
2. **Do NOT break the Roadmap flow** — Roadmap still needs to work, just accessed differently
3. **Do NOT implement Shop/Leaders/Teams/Collect** — icon-only placeholders, non-interactive
4. **Do NOT change GameplayManager** logic — only UI flow changes
5. **Do NOT forget to update back navigation** — from Gameplay, "back" should go to Hub, not MainMenu

### References
- [Source: _bmad-output/epics.md#Epic-11] — Hub screen scope and flow
- [Source: _bmad-output/mockups/hub-screen-mockup.html] — Visual reference
- [Source: Magic Sort screenshots] — Industry standard hub pattern

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Debug Log References
- Reused GameFlowState.MainMenu enum to avoid breaking tests/references — Hub registers under same state
- Settings + Roadmap "Back" buttons already navigate to GameFlowState.MainMenu (now Hub) — no changes needed
- GameplayManager.GoBackToRoadmap() still transitions to Roadmap — correct behavior
- FloatingLights ambient effects already exist globally from BootLoader (Epic 8)
- BootLoader OnStateChanged now refreshes Hub when transitioning to MainMenu state

### Completion Notes List
- Created HubScreen.cs with full programmatic UI (no prefabs), 607 lines
- Level N play button: green 4-stop gradient pill (BakeGradientPill), 3D extruded shadow (3 layers), top tint, shine dots, pulsing green glow aura
- Coin display: dark pill bg, gold circle icon with "$", gold balance text (46px, FaceDilate+outline for weight)
- City name: 72px bold white with 4-layer 3D purple depth text (2px offsets per layer)
- Settings gear: PNG sprite button (`Icons/icon_settings` via SpriteBtn)
- Roadmap button: PNG sprite button (`Icons/icon_worldmap` via SpriteBtn)
- Bottom nav bar: 5 flat PNG icon buttons (no labels, no SOON badges), Home raised with golden dot + purple glow, golden top border line
- "TAP ANYWHERE FOR ROADMAP" hint with pulse animation (alpha 0.15→0.55)
- Center area tap → Roadmap navigation (moved from full-screen to avoid blocking other buttons)
- Animations: single Anim() coroutine driving hint pulse, play glow pulse, shine sweep, nav glow cubic-bezier pulse
- Refresh() loads live data from IProgressionManager, CityAssigner, ICoinManager
- MainMenuScreen.cs and .meta deleted

### Change Log
- 2026-03-24: Story 11.4 implemented — HubScreen replaces MainMenuScreen
- 2026-03-24: Code review fixes — Fixed bg tap blocking all clicks (moved to center-only overlay), deleted dead MainMenuScreen.cs, replaced hardcoded colors with ThemeConfig
- 2026-03-24: Icon fix — Replaced invisible Unicode text icons (⚙/≡) on settings/roadmap buttons with PNG sprites via new SpriteBtn helper (Resources.Load from Icons/)

### File List
- CREATED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/HubScreen.cs` — Full hub screen (607 lines)
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — HubScreen.Create() replaces MainMenuScreen.Create(), added Hub refresh handler
- DELETED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/MainMenuScreen.cs` — Replaced by HubScreen
- DELETED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/MainMenuScreen.cs.meta`
