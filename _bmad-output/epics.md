# JuiceSort - Development Epics

## Epic Overview

| # | Epic Name | Dependencies | Stories | Status |
|---|---|---|---|---|
| 1 | Core Puzzle Engine | None | 9 implemented | Done |
| 2 | Level Generation | Epic 1 | 6 implemented | Done |
| 3 | Progression System | Epic 1, 2 | 5 implemented, 2 merged | Done |
| 4 | UI & Menus | Epic 1 | 5 implemented, 2 merged | Done |
| 5 | Visual Theme | Epic 1, 4 | 4 implemented, 2 merged | Done |
| 6 | Audio | Epic 1 | 2 implemented, 3 merged | Done |
| 7 | Monetization & Publishing | Epic 1-6 | 2 implemented, 4 merged | Done |
| 8 | Animation Polish | Epic 1, 4, 5 | 5 implemented | Done |
| 9 | Coin Economy System | Epic 1, 3, 7 | 6 implemented | Done |
| 10 | Liquid Visual Overhaul | Epic 1, 5 | 6 implemented | Done |
| 11 | UI/UX Overhaul | Epic 4, 5, 9 | 3 implemented, 4 remaining | In Progress |

**Total: 47 implemented stories / 57 originally planned (13 merged during development)**

---

## Epic 1: Core Puzzle Engine

### Goal
Get the fundamental puzzle mechanics working — a player can pour drinks between containers and solve a puzzle.

### Scope
**Includes:**
- Container data model (slots, colors)
- Tap-to-select container interaction
- Pour mechanic (multi-unit consecutive same-color, matching color or empty, slot availability check)
- Pour animation (implemented: lift → tilt → slot-by-slot transfer → return)
- Selection animation (implemented: lift + scale + golden glow with EaseOutBack)
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
- **1.1** As a player, I can see containers with colored liquid on screen → [1-1-container-rendering.md](implementation-artifacts/1-1-container-rendering.md)
- **1.2** As a player, I can tap a container to select it (visual highlight) → [1-2-container-selection.md](implementation-artifacts/1-2-container-selection.md)
- **1.3** As a player, I can tap another container to pour liquid (all consecutive same-color units) → [1-3-pour-mechanic.md](implementation-artifacts/1-3-pour-mechanic.md)
- **1.4** As a player, I can only pour onto matching colors or empty containers → [1-4-pour-color-validation.md](implementation-artifacts/1-4-pour-color-validation.md)
- **1.5** As a player, I can only pour if the target has an empty slot → [1-5-pour-slot-validation.md](implementation-artifacts/1-5-pour-slot-validation.md)
- **1.6** As a player, I can deselect or re-select by tapping (same container, empty space, or failed pour target) → [1-6-deselect-container.md](implementation-artifacts/1-6-deselect-container.md)
- **1.7** As a player, I see the level complete when all containers are sorted; completed bottles are locked → [1-7-win-condition.md](implementation-artifacts/1-7-win-condition.md)
- **1.8** As a player, I can undo my last pour (limited uses) → [1-8-undo-mechanic.md](implementation-artifacts/1-8-undo-mechanic.md)
- **1.9** As a player, I can restart the level at any time → [1-9-level-restart.md](implementation-artifacts/1-9-level-restart.md)

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
- **2.1** As a developer, the system generates a solvable puzzle from difficulty parameters → [2-1-puzzle-generation.md](implementation-artifacts/2-1-puzzle-generation.md)
- **2.2** As a developer, difficulty parameters scale based on level number → [2-2-difficulty-scaling.md](implementation-artifacts/2-2-difficulty-scaling.md)
- **2.3** As a player, each new level feels slightly more challenging than the last → [2-3-progressive-challenge.md](implementation-artifacts/2-3-progressive-challenge.md)
- **2.4** As a player, I never encounter an unsolvable puzzle → [2-4-solvability-guarantee.md](implementation-artifacts/2-4-solvability-guarantee.md)
- **2.5** As a developer, each level is assigned a random city and mood → [2-5-city-mood-assignment.md](implementation-artifacts/2-5-city-mood-assignment.md)
- **2.6** As a developer, I can configure difficulty scaling formulas → [2-6-difficulty-configuration.md](implementation-artifacts/2-6-difficulty-configuration.md)

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
- **3.1** As a player, I earn 1-3 stars based on my move efficiency when completing a level → [3-1-star-rating.md](implementation-artifacts/3-1-star-rating.md)
- ~~**3.2** As a player, I see my star rating on the level complete screen~~ *(merged into 3.1 — star display is part of star rating)*
- **3.3** As a player, my level completions are tracked with city, mood, and star ratings → [3-3-minimum-star-pass.md](implementation-artifacts/3-3-minimum-star-pass.md)
- **3.4** As a player, I need ~80% of possible stars to unlock the next batch of 50 levels → [3-4-star-gate-unlock.md](implementation-artifacts/3-4-star-gate-unlock.md)
- **3.5** As a player, I can replay any completed level to improve my star rating → [3-5-level-replay.md](implementation-artifacts/3-5-level-replay.md)
- **3.6** As a player, my progress is saved automatically and persists across sessions → [3-6-auto-save.md](implementation-artifacts/3-6-auto-save.md)
- ~~**3.7** As a player, my progress persists when I close and reopen the app~~ *(merged into 3.6 — persistence is part of auto-save)*

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
- Screen transitions (not yet implemented)

**Excludes:**
- Tropical Fresh styling (Epic 5 — use placeholder styling here)
- Sound effects for UI (Epic 6)

### Dependencies
Epic 1 (in-game interactions)

### Deliverable
Navigate from main menu → roadmap → level → completion → next level. Full flow working.

### Stories
- **4.1** As a player, I see a main menu when I open the app → [4-1-main-menu.md](implementation-artifacts/4-1-main-menu.md)
- **4.2** As a player, I can view my progress on a scrollable roadmap → [4-2-scrollable-roadmap.md](implementation-artifacts/4-2-scrollable-roadmap.md)
- ~~**4.3** As a player, I can tap to start the next available level~~ *(merged into 4.2 — level selection is part of roadmap)*
- **4.4** As a player, I see undo count, move count, and restart button during gameplay → [4-4-gameplay-hud.md](implementation-artifacts/4-4-gameplay-hud.md)
- **4.5** As a player, I see my star rating and can continue or replay after completing a level → [4-5-level-complete-screen.md](implementation-artifacts/4-5-level-complete-screen.md)
- ~~**4.6** As a player, I see a notification when I unlock the next batch of levels~~ *(merged into 4.5 — batch unlock notification is part of level complete flow)*
- **4.7** As a player, I can access settings from the main menu → [4-7-settings-screen.md](implementation-artifacts/4-7-settings-screen.md)

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
- City-specific music (post-MVP)
- Note: Pour and selection animations were implemented in Epic 1

### Dependencies
Epic 1 (container rendering), Epic 4 (UI to style)

### Deliverable
The game looks like JuiceSort — beautiful, themed, and visually distinct from competitors.

### Stories
- **5.1** As a player, I see beautiful rounded glass containers with drink-themed colors → [5-1-container-visuals.md](implementation-artifacts/5-1-container-visuals.md)
- **5.2** As a player, I see a unique city background with a landmark for each level → [5-2-city-backgrounds.md](implementation-artifacts/5-2-city-backgrounds.md)
- ~~**5.3** As a player, I see the city name and country on screen~~ *(merged into 5.1 — city/country display is part of container visuals & ThemeConfig)*
- **5.4** As a player, morning levels feel like a bright café and night levels feel like a cozy bar → [5-4-morning-night-atmosphere.md](implementation-artifacts/5-4-morning-night-atmosphere.md)
- ~~**5.5** As a player, the roadmap looks like a visual journey across the world~~ *(merged into 4.2 — roadmap visual design handled in scrollable roadmap story)*
- **5.6** As a player, the entire UI feels premium and cohesive → [5-6-ui-styling.md](implementation-artifacts/5-6-ui-styling.md)

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
- **6.1** As a player, I hear calming music that matches the morning or night mood → [6-1-ambient-music.md](implementation-artifacts/6-1-ambient-music.md)
- **6.2** As a player, I hear a satisfying pour sound when I move liquid → [6-2-pour-sound.md](implementation-artifacts/6-2-pour-sound.md)
- ~~**6.3** As a player, I hear pleasant sounds for UI interactions~~ *(merged into 6.2 — UI SFX consolidated into gameplay sound effects)*
- ~~**6.4** As a player, I hear a celebration sound when I complete a level~~ *(merged into 6.2 — level complete SFX is part of gameplay sounds)*
- ~~**6.5** As a player, I can toggle music and sound effects on/off~~ *(split across 6.1 music toggle + 6.2 SFX toggle)*

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
- **7.1** As a player, I can tap "+" to watch an ad and receive an extra empty bottle → [7-1-rewarded-ad-extra-bottle.md](implementation-artifacts/7-1-rewarded-ad-extra-bottle.md)
- ~~**7.2** As a player, I can use max 2 extra bottles per level~~ *(merged into 7.1 — max 2 limit is part of extra bottle feature)*
- ~~**7.3** As a player, ads only appear when I choose to watch them (no forced ads)~~ *(merged into 7.1 — no forced ads is part of rewarded ad design)*
- **7.4** As a developer, the app meets Play Store requirements → [7-4-play-store-requirements.md](implementation-artifacts/7-4-play-store-requirements.md)
- ~~**7.5** As a developer, the app is optimized for size and performance~~ *(merged into 7.4 — optimization is part of Play Store readiness)*
- ~~**7.6** As a developer, the game is published and downloadable on the Play Store~~ *(merged into 7.4 — publication is the deliverable of Play Store requirements)*

---

## Epic 8: Animation Polish

### Goal
Add visual polish animations that elevate JuiceSort from functional to premium — satisfying completion feedback, smooth screen transitions, tactile button responses, and ambient visual effects.

### Scope
**Includes:**
- Completion shimmer ripple on fully-sorted containers
- Screen transitions (crossfade + slide) between all navigation screens
- Button bounce feedback on all interactive UI buttons
- Glass sparkle particles on idle containers
- Floating light particles in background atmosphere

**Excludes:**
- Pour and selection animations (already implemented in Epic 1)
- Sound effects for animations (Epic 6)
- City-specific visual effects (post-MVP)

### Dependencies
Epic 1 (gameplay events for completion trigger), Epic 4 (UI screens for transitions and buttons), Epic 5 (container visuals for sparkle positioning)

### Deliverable
Every interaction feels satisfying and polished. Completing a bottle triggers a visible celebration. Navigating between screens is smooth. Buttons feel tactile. The game has subtle ambient visual life even when idle.

### Stories
- **8.1** As a player, I see a shimmer ripple across a container when it becomes fully sorted → [8-1-completion-shimmer.md](implementation-artifacts/8-1-completion-shimmer.md)
- **8.2** As a player, I experience smooth crossfade transitions between all screens → [8-2-screen-transitions.md](implementation-artifacts/8-2-screen-transitions.md)
- **8.3** As a player, buttons feel tactile with a satisfying bounce on tap → [8-3-button-bounce.md](implementation-artifacts/8-3-button-bounce.md)
- **8.4** As a player, I see subtle sparkles on glass containers while idle → [8-4-glass-sparkles.md](implementation-artifacts/8-4-glass-sparkles.md)
- **8.5** As a player, I see warm floating light particles drifting in the background → [8-5-floating-lights.md](implementation-artifacts/8-5-floating-lights.md)

---

## Epic 9: Coin Economy System

### Goal
Add a coin economy that monetizes boosters (undo, extra bottle) and rewards efficient play. Two parallel currencies: stars (progression) + coins (boosters).

### Scope
**Includes:**
- Coin balance tracking and persistence
- Level completion coin rewards (base + difficulty scaling + move efficiency bonus)
- Consecutive win streak tracking with bonus rewards
- Coin spending for undo (escalating cost per level)
- Coin spending for extra bottle (escalating cost, max 2 per level)
- Rewarded ad to earn coins
- Coin balance display on gameplay HUD
- CoinConfig ScriptableObject for all values (never hardcode)

**Excludes:**
- In-app purchase coin packs (post-MVP)
- Continue after failure mechanic (post-MVP)

### Dependencies
Epic 1 (puzzle engine), Epic 3 (progression system), Epic 7 (ads)

### Deliverable
Players earn coins by completing levels efficiently, maintain streaks for bonus coins, and spend coins on undo and extra bottle instead of getting them for free/ads.

### Stories
- **9.1** As a player, I see my coin balance on the gameplay HUD and it persists across sessions → [9-1-coin-balance-hud.md](implementation-artifacts/9-1-coin-balance-hud.md)
- **9.2** As a player, I earn coins when completing a level (base reward scaled by difficulty) → [9-2-level-completion-reward.md](implementation-artifacts/9-2-level-completion-reward.md)
- **9.3** As a player, I earn bonus coins for consecutive level wins (streak system) → [9-3-streak-bonus.md](implementation-artifacts/9-3-streak-bonus.md)
- **9.4** As a player, I can spend coins to undo a move (cost increases with each use per level) → [9-4-coin-undo.md](implementation-artifacts/9-4-coin-undo.md)
- **9.5** As a player, I can spend coins to add an extra bottle (cost increases, max 2 per level) → [9-5-coin-extra-bottle.md](implementation-artifacts/9-5-coin-extra-bottle.md)
- **9.6** As a player, I can watch a rewarded ad to earn coins → [9-6-rewarded-ad-coins.md](implementation-artifacts/9-6-rewarded-ad-coins.md)

---

## Epic 10: Liquid Visual Overhaul

### Goal
Replace the sprite-based liquid rendering with a shader-based system that delivers Magic Sort-quality visual polish — smooth fills, satisfying pours, liquid wobble, and bottle cap animations.

### Scope
**Includes:**
- HLSL/ShaderLab liquid fill shader (replaces sprite slot rendering)
- Smooth pour animation (fill amount lerp + dynamic tilt angles)
- Visible liquid stream between bottles during pour (LineRenderer/particle VFX)
- Select/deselect liquid wobble (shader-driven damped oscillation)
- Bottle cap/cork close animation on sorted completion
- Glass glow, refraction, and bloom effects (URP 2D lights + post-processing)

**Excludes:**
- City background changes (existing system preserved)
- Sound effects changes (existing system preserved)

### Dependencies
Epic 1 (bottle container view, pour animator), Epic 5 (container visuals)

### Deliverable
Bottles render with smooth, animated liquid that wobbles on selection, flows visibly during pours, and celebrates with a cork animation on completion. The visual quality rivals Magic Sort.

### Stories
- **10.1** As a player, I see smooth, gradient-filled liquid in each bottle (HLSL shader with contiguous color bands, 80% visual headroom) → [10-1-shader-liquid-fill.md](implementation-artifacts/10-1-shader-liquid-fill.md)
- **10.2** As a player, I see the liquid level smoothly rise and fall during a pour (fill amount lerp + dynamic tilt angle based on pour count) → [10-2-smooth-pour-animation.md](implementation-artifacts/10-2-smooth-pour-animation.md)
- **10.3** As a player, I see a flowing liquid stream between bottles during a pour (LineRenderer/particle stream VFX) → [10-3-pour-stream-vfx.md](implementation-artifacts/10-3-pour-stream-vfx.md)
- **10.4** As a player, I see the liquid wobble/slosh when I select or deselect a bottle (shader-driven damped oscillation) → [10-4-select-wobble.md](implementation-artifacts/10-4-select-wobble.md)
- **10.5** As a player, I see a cork/cap close on a bottle when it becomes fully sorted (sprite drop + bounce + confetti burst) → [10-5-bottle-cap-animation.md](implementation-artifacts/10-5-bottle-cap-animation.md)
- **10.6** As a player, I see glass glow, refraction, and bloom effects on bottles (URP 2D lights + post-processing) → [10-6-glass-glow-bloom.md](implementation-artifacts/10-6-glass-glow-bloom.md)
- **10.7** As a player, I see the source bottle physically move above the target bottle and progressively tilt based on liquid depth during a pour (horizontal movement + multi-row support + depth-based tilt 15°–100°+) → [10-7-pour-movement-overhaul.md](implementation-artifacts/10-7-pour-movement-overhaul.md)
- **10.8** As a player, I see liquid decrease smoothly in the source bottle during a pour, with correct fill levels and no visual glitches (synchronized source drain + fix extra-slot bug + enforce 75–80% visual cap) → [10-8-pour-visual-fixes.md](implementation-artifacts/10-8-pour-visual-fixes.md)
- **10.9** As a player, I see the liquid stream originate from the tilted bottle's mouth opening and adapt its length based on the target's water level (mouth-origin stream + adaptive arc + dynamic length) → [10-9-pour-stream-rework.md](implementation-artifacts/10-9-pour-stream-rework.md)

---

## Epic 11: UI/UX Overhaul

### Goal
Fix all layout, sizing, and visual quality issues to deliver a polished, premium-feeling UI.

### Scope
**Includes:**
- Responsive bottle layout (dynamic positioning based on screen size and bottle count)
- HUD redesign (organized top/bottom bars, consistent button sizing, coin balance display)
- Gradient backgrounds and typography improvements
- Consistent icon system
- Extra bottle re-layout animation
- Main menu visual redesign
- Roadmap redesign (Candy Crush-style curved path)

**Excludes:**
- Gameplay mechanics changes
- Audio changes

### Dependencies
Epic 4 (existing UI), Epic 5 (visual theme), Epic 9 (coin balance display)

### Deliverable
All bottles fit on screen regardless of count. HUD is organized and readable. Background and typography look premium. Roadmap feels like a real journey.

### Stories
- **11.1** [SO HIGH] As a player, all bottles fit on my screen regardless of count, with proper margins and multi-row layout when needed (responsive layout system) → [11-1-responsive-bottle-layout.md](implementation-artifacts/11-1-responsive-bottle-layout.md)
- **11.2** [HIGH] As a player, the gameplay HUD has clear, organized top and bottom bars with consistent button sizing (HUD redesign) → [11-2-hud-redesign.md](implementation-artifacts/11-2-hud-redesign.md)
- **11.3** [HIGH] As a player, the game background uses warm gradient colors and text has proper contrast and readability (gradient backgrounds + typography)
- **11.4** [HIGH] As a player, all icons are consistent in style and size across the game (icon system)
- **11.5** [MEDIUM] As a player, when an extra bottle is added mid-level, all bottles smoothly rearrange to fit (re-layout animation) → [11-5-extra-bottle-relayout.md](implementation-artifacts/11-5-extra-bottle-relayout.md)
- **11.6** [LOW] As a player, the main menu looks polished with gradient background, styled logo, and animated elements
- **11.7** [LOW] As a player, the roadmap shows a Candy Crush-style curved path with level nodes, stars, and city labels
