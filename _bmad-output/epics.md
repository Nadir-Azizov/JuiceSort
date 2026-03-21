# JuiceSort - Development Epics

## Epic Overview

| # | Epic Name | Dependencies | Deliverable |
|---|---|---|---|
| 1 | Core Puzzle Engine | None | Playable pour/sort puzzle |
| 2 | Level Generation | Epic 1 | Unlimited generated puzzles |
| 3 | Progression System | Epic 1, 2 | Stars, roadmap, batch gates |
| 4 | UI & Menus | Epic 1 | Full navigation flow |
| 5 | Visual Theme | Epic 1, 4 | Tropical Fresh look with cities |
| 6 | Audio | Epic 1 | Music and sound effects |
| 7 | Monetization & Publishing | Epic 1-6 | Play Store ready build |

---

## Epic 1: Core Puzzle Engine

### Goal
Get the fundamental puzzle mechanics working — a player can pour drinks between containers and solve a puzzle.

### Scope
**Includes:**
- Container data model (slots, colors)
- Tap-to-select container interaction
- Pour mechanic (one unit, matching color or empty, slot availability check)
- Pour animation (placeholder is fine)
- Win condition detection (all containers sorted)
- Undo mechanic (single pour reversal, limited count)
- Level restart
- Basic container rendering (placeholder visuals OK)

**Excludes:**
- Procedural level generation (Epic 2)
- Star rating system (Epic 3)
- City themes and Tropical Fresh visuals (Epic 5)
- Sound effects (Epic 6)

### Dependencies
None — this is the foundation.

### Deliverable
A single hand-crafted test puzzle that can be played to completion with pour, sort, undo, and restart.

### Stories
- As a player, I can see containers with colored liquid on screen
- As a player, I can tap a container to select it (visual highlight)
- As a player, I can tap another container to pour one unit of liquid
- As a player, I can only pour onto matching colors or empty containers
- As a player, I can only pour if the target has an empty slot
- As a player, I can deselect by tapping the same container or empty space
- As a player, I see the level complete when all containers are sorted
- As a player, I can undo my last pour (limited uses)
- As a player, I can restart the level at any time

---

## Epic 2: Level Generation

### Goal
Generate unlimited solvable puzzles with scaling difficulty parameters.

### Scope
**Includes:**
- Puzzle generation algorithm (recommended: reverse from solved state)
- Solvability guarantee
- Difficulty parameter system (container count, color count, slot count)
- Difficulty scaling by level number
- Random city/mood assignment per level

**Excludes:**
- Hidden layers, locked containers, special drinks (post-MVP)
- Star rating calculation (Epic 3)
- Visual city themes (Epic 5)

### Dependencies
Epic 1 (container data model and mechanics)

### Deliverable
Play through 20+ generated levels with visible difficulty increase. Every puzzle is solvable.

### Stories
- As a developer, the system generates a solvable puzzle from difficulty parameters
- As a developer, difficulty parameters scale based on level number
- As a player, each new level feels slightly more challenging than the last
- As a player, I never encounter an unsolvable puzzle
- As a developer, each level is assigned a random city and mood
- As a developer, I can configure difficulty scaling formulas

---

## Epic 3: Progression System

### Goal
Give players a sense of journey and progress with stars, gates, and a roadmap.

### Scope
**Includes:**
- Star rating calculation (move efficiency vs optimal)
- Star display on level complete
- Total star tracking (persistent)
- Level batch system (groups of 50)
- Star gate check between batches
- Level replay for star improvement
- Roadmap data model (current position, completed levels)
- Local save/load for all progress data

**Excludes:**
- Visual roadmap UI (Epic 4)
- Online leaderboards (post-MVP)

### Dependencies
Epic 1 (level completion), Epic 2 (level generation, optimal move calculation)

### Deliverable
Complete levels, earn stars, replay for better ratings, and unlock batch gates.

### Stories
- As a player, I earn 1-3 stars based on my move efficiency when completing a level
- As a player, I see my star rating on the level complete screen
- As a player, I need minimum 1 star to pass a level
- As a player, I need ~80% of possible stars to unlock the next batch of 50 levels
- As a player, I can replay any completed level to improve my star rating
- As a player, my progress is saved automatically
- As a player, my progress persists when I close and reopen the app

---

## Epic 4: UI & Menus

### Goal
Build the full navigation flow so the game feels complete and polished.

### Scope
**Includes:**
- Main menu screen
- Roadmap/level select screen (scrollable path)
- In-game HUD (undo button, restart, extra bottle, move counter)
- Level complete screen (stars, next level, replay)
- Star gate screen (batch unlock notification)
- Settings screen (sound toggle, etc.)
- Screen transitions

**Excludes:**
- Tropical Fresh styling (Epic 5 — use placeholder styling here)
- Sound effects for UI (Epic 6)

### Dependencies
Epic 1 (in-game interactions)

### Deliverable
Navigate from main menu → roadmap → level → completion → next level. Full flow working.

### Stories
- As a player, I see a main menu when I open the app
- As a player, I can view my progress on a scrollable roadmap
- As a player, I can tap to start the next available level
- As a player, I see undo count, move count, and restart button during gameplay
- As a player, I see my star rating and can continue or replay after completing a level
- As a player, I see a notification when I unlock the next batch of levels
- As a player, I can access settings from the main menu

---

## Epic 5: Visual Theme

### Goal
Transform placeholder visuals into the "Tropical Fresh" aesthetic with city themes.

### Scope
**Includes:**
- Tropical Fresh container designs (rounded glass with refraction)
- Drink visuals (juice and cocktail themed colors)
- 76 city backgrounds (38 cities × morning/night)
- 38 landmark icons/signs
- City name and country display
- Morning/night atmospheric differences
- Roadmap visual design
- UI styling to match Tropical Fresh direction

**Excludes:**
- Animations beyond basic (stretch goal)
- City-specific music (post-MVP)

### Dependencies
Epic 1 (container rendering), Epic 4 (UI to style)

### Deliverable
The game looks like JuiceSort — beautiful, themed, and visually distinct from competitors.

### Stories
- As a player, I see beautiful rounded glass containers with drink-themed colors
- As a player, I see a unique city background with a landmark for each level
- As a player, I see the city name and country on screen
- As a player, morning levels feel like a bright café and night levels feel like a cozy bar
- As a player, the roadmap looks like a visual journey across the world
- As a player, the entire UI feels premium and cohesive

---

## Epic 6: Audio

### Goal
Add music and sound effects that complete the relaxing atmosphere.

### Scope
**Includes:**
- 1 morning music track (light, breezy, café-like)
- 1 night music track (smooth, jazzy, lounge-bar)
- Pour sound effects (1-3 variants)
- UI tap/select sounds
- Level completion sound
- Star award sounds
- Music toggle in settings

**Excludes:**
- City-specific music (post-MVP)
- Voice acting

### Dependencies
Epic 1 (gameplay events to trigger sounds)

### Deliverable
The game sounds as good as it looks — satisfying pours, calming music, polished feedback.

### Stories
- As a player, I hear calming music that matches the morning or night mood
- As a player, I hear a satisfying pour sound when I move liquid
- As a player, I hear pleasant sounds for UI interactions
- As a player, I hear a celebration sound when I complete a level
- As a player, I can toggle music and sound effects on/off

---

## Epic 7: Monetization & Publishing

### Goal
Integrate ads, the extra bottle feature, and prepare for Play Store submission.

### Scope
**Includes:**
- Google AdMob SDK integration
- Rewarded video ad flow for extra bottle
- Extra bottle mechanic (max 2 per level)
- Play Store listing preparation (screenshots, description, icon)
- Android build optimization (size, performance)
- Play Store submission

**Excludes:**
- In-app purchases
- Analytics (post-MVP)
- Online features (post-MVP)

### Dependencies
All previous epics (game must be feature-complete)

### Deliverable
A published JuiceSort game on the Google Play Store with working ad monetization.

### Stories
- As a player, I can tap "+" to watch an ad and receive an extra empty bottle
- As a player, I can use max 2 extra bottles per level
- As a player, ads only appear when I choose to watch them (no forced ads)
- As a developer, the app meets Play Store requirements
- As a developer, the app is optimized for size and performance
- As a developer, the game is published and downloadable on the Play Store
