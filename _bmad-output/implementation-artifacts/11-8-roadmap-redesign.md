# Story 11.8: Roadmap Redesign

Status: ready-for-dev

## Story

As a player,
I want to access a vertical journey roadmap from the hub showing level nodes on a path with completed, current, and locked states,
so that I can see my progress and feel a sense of journey through the cities.

## Priority

MEDIUM — The roadmap already exists (from Epic 4) but needs visual redesign to match the new hub-driven flow and Magic Sort style.

## Acceptance Criteria

1. **Access from Hub** — Roadmap opens when player taps background on Hub screen OR taps a roadmap button. No more navigating from MainMenu.
2. **Close button** — X button (top-right) closes roadmap and returns to Hub.
3. **Vertical scrolling path** — Levels displayed on a vertical golden/colored line, scrollable top-to-bottom. Current level auto-scrolled to center.
4. **Level nodes** — Each level shown as a node on the path:
   - **Completed:** Checkmark badge, green highlight, stars shown
   - **Current:** Green/active highlight, pulsing glow, level number prominent
   - **Locked:** Dimmed, lock icon or grayed out, level number visible
5. **City chapter cards** — Between level groups, show chapter/city cards with the city name and level range (e.g., "Istanbul — Level 10-19"). Completed chapters show a checkmark.
6. **Star display** — Completed levels show earned stars (1-3) near the node.
7. **Tap to play** — Tapping a completed level replays it. Tapping current level starts it (same as Hub play button). Tapping locked levels does nothing.
8. **Gradient background** — Warm parchment/paper-toned gradient, distinct from gameplay dark gradient.
9. **Journey title** — "Journey" header at top of roadmap screen.
10. **Smooth scroll** — Standard Unity ScrollRect with momentum, snapping optional.

## Tasks / Subtasks

- [ ] Task 1: Restructure roadmap screen (AC: 1, 2, 9)
  - [ ] 1.1 Update `RoadmapScreen.cs` (or create new) to work as overlay opened from Hub
  - [ ] 1.2 Add close (X) button top-right — returns to Hub via ScreenManager
  - [ ] 1.3 Add "Journey" header title at top
  - [ ] 1.4 Update navigation: Hub ↔ Roadmap (not MainMenu ↔ Roadmap)

- [ ] Task 2: Build vertical path layout (AC: 3, 10)
  - [ ] 2.1 ScrollRect container with vertical Content
  - [ ] 2.2 Golden/colored vertical line down the center (narrow Image)
  - [ ] 2.3 Level nodes positioned along the line, alternating left/right or centered
  - [ ] 2.4 Auto-scroll to current level on open

- [ ] Task 3: Build level nodes (AC: 4, 6, 7)
  - [ ] 3.1 Completed node: green background, checkmark, star display (1-3 stars), level number
  - [ ] 3.2 Current node: green with pulsing glow, level number prominent, "play" feel
  - [ ] 3.3 Locked node: gray/dimmed, lock icon or just dimmed number
  - [ ] 3.4 Tap handler: completed → replay level, current → start level, locked → ignore
  - [ ] 3.5 Load star data from save system for each completed level

- [ ] Task 4: Build city chapter cards (AC: 5)
  - [ ] 4.1 Between level groups (every 10 levels or per city), show chapter card
  - [ ] 4.2 Card shows: city name, country, level range
  - [ ] 4.3 Completed chapters: checkmark or "completed" indicator
  - [ ] 4.4 Locked chapters: dimmed or partially visible
  - [ ] 4.5 Cards are wider than level nodes, interrupt the path line visually

- [ ] Task 5: Visual polish (AC: 8)
  - [ ] 5.1 Warm gradient background (parchment tones — distinct from gameplay dark gradient)
  - [ ] 5.2 Path line has subtle glow
  - [ ] 5.3 Level nodes have slight shadow
  - [ ] 5.4 Smooth transitions when opening/closing roadmap

## Dev Notes

### Current Roadmap (from Epic 4)
- `RoadmapScreen.cs` + `LevelListView.cs` — plain scrollable list of level entries
- Shows level number, city name, star count per level
- Functional but visually bare — just text entries in a scroll view

### New Roadmap
- Visual journey path (like Magic Sort screenshot 3)
- Level nodes on a vertical golden line
- City chapter cards between level groups
- X button to close (not back button)

### Key Files to MODIFY
- `Scripts/Game/UI/Screens/RoadmapScreen.cs` — Major visual rewrite
- `Scripts/Game/UI/Components/LevelListView.cs` — May need significant changes or replacement

### Key Files Referenced
- Save data for star counts: accessed via `Services.TryGet<ISaveManager>()`
- City data: `CityDatabase` ScriptableObject
- Level progression: existing progression system

### Design Reference
- Mockup: Magic Sort screenshot 3 (Journey screen)
- Vertical path with golden/green line
- Level nodes positioned along the line
- Chapter cards between level groups
- Close button (X) top-right

### Architecture Notes
- Roadmap is still a Canvas ScreenSpaceOverlay
- ScrollRect for vertical scrolling (existing pattern from LevelListView)
- Level data comes from progression system (already exists)
- City grouping logic already exists in CityDatabase
- Keep existing level start/replay wiring — just visual changes

### Dependencies
- Story 11.3 (TMPro + fonts) — text
- Story 11.4 (Hub screen) — roadmap accessed from Hub
- Existing save system for star data
- Existing CityDatabase for city grouping

### CRITICAL Anti-Patterns
1. **Do NOT break level start/replay functionality** — only visual changes
2. **Do NOT load all level data upfront** — lazy-load or virtualize if 100+ levels
3. **Do NOT use prefabs** — programmatic UI (project convention)
4. **Do NOT change save data format** — read existing star/progression data as-is

### References
- [Source: Magic Sort screenshot 3] — Journey/roadmap visual reference
- [Source: _bmad-output/epics.md#Epic-11] — Roadmap redesign scope
- [Source: _bmad-output/implementation-artifacts/4-2-scrollable-roadmap.md] — Original roadmap implementation

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
