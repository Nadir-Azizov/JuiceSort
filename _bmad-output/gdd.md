---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
inputDocuments:
  - game-brief.md
documentCounts:
  briefs: 1
  research: 0
  brainstorming: 0
  projectDocs: 0
workflowType: 'gdd'
lastStep: 14
project_name: 'JuiceSort'
user_name: 'Nadir'
date: '2026-03-18'
game_type: 'puzzle'
game_name: 'JuiceSort'
---

# JuiceSort - Game Design Document

**Author:** Nadir
**Game Type:** Puzzle
**Target Platform(s):** Android mobile

---

## Executive Summary

### Game Name

JuiceSort

### Core Concept

JuiceSort is a casual mobile puzzle game that transforms the familiar liquid-sorting mechanic into a memorable journey around the world. Players pour and sort themed drinks — fresh juices in morning restaurant settings and cocktails in nighttime bar atmospheres — across 38 iconic cities.

Unlike generic water sort games that offer endless levels of identical colored tubes, JuiceSort wraps every puzzle in a unique city experience. Each location features recognizable landmarks, signature local drinks, and distinct day/night moods that make players feel like they're collecting travel postcards one puzzle at a time.

Designed for short, relaxing play sessions, JuiceSort targets the universal casual audience — anyone looking for a satisfying puzzle to unwind with during a commute, a break, or a quiet moment. The game prioritizes relaxation over challenge, visual delight over complexity, and discovery over repetition.

### Game Type

**Type:** Puzzle
**Framework:** This GDD uses the puzzle template with type-specific sections for core puzzle mechanics, puzzle progression, level structure, player assistance, and replayability.

---

## Target Platform(s)

### Primary Platform

Android mobile (modern devices only)

### Platform Considerations

- **Performance:** Targeting modern Android phones — no need to optimize for legacy/budget devices
- **Distribution:** Google Play Store
- **Monetization:** To be determined (free with ads, paid, or freemium)
- **Network:** Fully offline for MVP — no internet connection required
- **Battery/Thermal:** Puzzle game with 2D visuals — low power consumption expected

### Control Scheme

- **Touch-based:** Tap to select a container, tap another to pour
- **Single-hand friendly:** Designed for one-handed use during commutes
- **No complex gestures:** Simple tap interactions only — accessible to all skill levels

---

## Target Audience

### Demographics

All ages — core engagement from ages 35-65, but designed to be universally accessible. No gaming experience required.

### Gaming Experience

Casual — players with little to no gaming background. The game must be immediately intuitive without tutorials or onboarding friction.

### Genre Familiarity

Mixed — welcoming to both first-time puzzle players and experienced water sort veterans. Mechanics are self-explanatory.

### Session Length

Short bursts — a few minutes per session. Perfect for commutes, waiting rooms, or idle moments.

### Player Motivations

- **Relaxation:** Unwind and de-stress through calm, satisfying puzzle solving
- **Discovery:** Curiosity about which city and drinks come next
- **Accomplishment:** The satisfaction of completing a puzzle cleanly

---

## Goals and Context

### Project Goals

1. **Creative:** Create a puzzle game that makes players feel like they're actually travelling the world — not just solving generic puzzles
2. **Technical:** Ship a stable, polished game on the Google Play Store for Android
3. **Personal:** Learn Unity game development, leveraging existing C# expertise
4. **Strategic:** Validate the AI-assisted + BMAD methodology as a viable game development approach — if successful, scale to bigger game ideas

### Background and Rationale

JuiceSort was born from playing existing water sort games and seeing untapped potential. The genre has proven demand with millions of installs, but every game looks the same — generic colored tubes with no personality, no story, no reason to remember them. JuiceSort fills that gap by wrapping proven puzzle mechanics in a world-travel experience with city landmarks, themed drinks, and day/night moods.

This project also serves as a strategic test: can a solo developer with strong backend skills but no game dev experience use AI tools and structured methodology (BMAD) to ship a quality mobile game? Success here opens the door to more ambitious projects.

---

## Unique Selling Points (USPs)

1. **World Travel Journey** — Every level is set in a real city with iconic landmarks, turning puzzle solving into a virtual travel experience
2. **Day/Night Mood System** — Morning restaurant levels (juices) and night bar levels (cocktails) give the same mechanic completely different atmospheres
3. **Themed Drinks Over Generic Tubes** — Players sort recognizable cocktails and juices, not abstract colored liquids
4. **Emotional Memory** — City signs, landmarks, and drink themes create a postcard-like experience players actually remember

### Competitive Positioning

In a market flooded with identical water sort clones, JuiceSort is the first to give every level a sense of place and identity. Competitors offer puzzles — JuiceSort offers a journey.

---

## Core Gameplay

### Game Pillars

1. **Relaxation** — No stress, no pressure. The game should feel calming even when puzzles get challenging. No punishing timers or harsh penalties.
2. **Visual Delight** — Every screen should look beautiful. Cities, drinks, animations, and UI all contribute to a premium, inviting experience.
3. **Discovery** — Every new level feels like arriving somewhere new. Fresh city visuals, new drinks, day/night moods keep players curious.

**Pillar Prioritization:** When pillars conflict, prioritize in this order:
Relaxation > Visual Delight > Discovery

### Core Gameplay Loop

Players select a level in a city → view the themed drink containers → tap to pour drinks between containers, sorting by color/type → complete the puzzle when all drinks are properly sorted → earn stars based on move efficiency → unlock the next level and discover a new city or mood.

**Loop Diagram:**
```
Select Level → Pour & Sort Drinks → Puzzle Complete?
    ↑              ↓ (stuck)              ↓ Yes
    │         Use Undo / Watch Ad    Earn Stars (1-3)
    │         for Extra Bottle        ↓
    │              ↓ (fail)      Unlock Next Level
    └──── Restart Level ←────── New City / Mood
```

**Loop Timing:** 1-5 minutes per puzzle depending on difficulty

**Loop Variation:** Each level differs through city theme, drink types, number of containers, color count, hidden layers, and day/night mood

### Win/Loss Conditions

#### Victory Conditions

- **Level Win:** All drinks sorted into matching containers — each container holds only one type/color
- **Star Rating:** 1-3 stars based on move count — fewer moves = more stars
- **Overall Progress:** Advance through cities, unlocking new destinations

#### Failure Conditions

- **Stuck State:** No valid moves remaining — player is stuck and loses the level
- **No timer-based failure** — aligns with Relaxation pillar

#### Failure Recovery

- **Undo System:** Limited undo moves scaled by difficulty:
  - Easy levels: 1 undo
  - Medium levels: 2 undos
  - Hard levels: 3 undos
- **Extra Bottle:** Player can watch a Google ad to receive an additional empty container, creating more room to solve the puzzle
- **Restart:** If fully stuck, player restarts the level from the beginning — no lives system, no penalty beyond replaying

---

## Game Mechanics

### Primary Mechanics

**1. Pour (Core Action)**
- Tap a source container, then tap a target container to pour
- Pours **all consecutive same-color units** from the top in one tap, limited by available empty slots in the target
- Can only pour onto a **matching color** at the top of the target container
- Can **always pour into an empty container**
- Can only pour if the target container has at least **one empty slot available**
- Serves pillars: Relaxation (simple, intuitive), Visual Delight (satisfying pour animation)

**2. Sort (Goal Mechanic)**
- Organize all drinks so each container holds only one color/type
- Level is complete when all containers are sorted
- Star rating (1-3) based on total move count
- Serves pillars: Relaxation (satisfying completion), Discovery (unlocks next city)

**3. Undo (Recovery Mechanic)**
- Goes back **one pour action at a time** (reverses the entire multi-unit pour)
- Limited uses per level, scaled by difficulty:
  - First 10 levels: 1 undo per level
  - Medium levels: 2 undos per level
  - Hard levels: 3 undos per level
- Serves pillar: Relaxation (reduces frustration)

**4. Extra Bottle (Ad-Rewarded Assist)**
- Watch a Google ad to receive an additional empty container
- Maximum **2 extra bottles per level**
- Creates more room to maneuver when stuck
- Serves pillar: Relaxation (safety net before restart)

### Mechanic Interactions

- **Pour + Sort:** Every pour is a step toward (or away from) the sorted goal state
- **Undo + Pour:** Undo reverses a pour, giving players a chance to rethink strategy
- **Extra Bottle + Pour:** Additional container creates new pouring options, potentially saving a stuck state
- **All mechanics are simple individually** — complexity comes from the puzzle state, not the controls

### Mechanic Progression

- **Early levels (1-10):** Few containers, 4 slots, few colors, 1 undo — teaches basic mechanics
- **Mid levels:** More containers, more colors, 2 undos — requires more planning
- **Hard levels:** Many containers, more slots, many colors, 3 undos — deep strategic thinking
- Difficulty scales across three dimensions: container count, slot count per container, and color count

---

## Controls and Input

### Control Scheme (Android Mobile)

| Action | Input | Notes |
|---|---|---|
| Select container | Tap container | Highlights selected container |
| Pour | Tap target container | Pours all consecutive same-color units (limited by target space) |
| Deselect | Tap selected container again or tap empty space | Cancel selection |
| Undo | Tap undo button | UI button, limited uses |
| Extra Bottle | Tap "+" button | Triggers ad, then adds empty container |
| Restart | Tap restart button | Menu/UI button |

### Input Feel

- **Responsive:** Immediate visual feedback on tap — container lifts, scales up (108%), and glows golden with an ease-out-back bounce (0.15s)
- **Satisfying:** Pour animation lifts source bottle, tilts toward target, transfers liquid slot-by-slot, then returns to rest (coroutine-based, ~0.5s total)
- **Forgiving:** Tapping an invalid target does nothing (no error state, no punishment)
- **One-handed:** All interactions reachable with thumb on a standard phone

### Accessibility Controls

- No specific accessibility features planned for initial release
- Natural accessibility: simple tap controls, no timing pressure, no complex gestures

---

## Puzzle Game Specific Design

### Core Puzzle Mechanics

**Primary Puzzle Mechanic:** Liquid sorting — pour drinks between containers to group matching colors together.

**Puzzle Elements:**
- **Color matching:** Each container must end up with only one color/type of drink
- **Slot management:** Containers have limited slots — players must plan pours carefully
- **Hidden layers:** Some colors are hidden beneath others, only revealed when the layer above is poured off — adds discovery and forces adaptive strategy

**Future Mechanics (post-MVP):**
- **Locked containers:** Certain containers that can't be poured from/into until unlocked
- **Special drinks:** Unique drink types with special color properties
- These features reserved for higher difficulty levels, not early game

**Constraint Systems:**
- One unit pours at a time
- Can only pour onto matching color or empty container
- Must have empty slot available in target
- Limited undo moves per level

### Puzzle Progression

**Difficulty Progression:**
- **Tutorial (Levels 1-10):** Very easy puzzles that teach mechanics by doing — few containers, few colors, 4 slots, 1 undo. No explicit tutorial screens.
- **Core (Early-Mid):** Introduce more containers and colors gradually. 2 undos.
- **Advanced (Mid-Late):** More containers, more slots per container, more colors, hidden layers revealed. 3 undos.
- **Expert (Post-MVP):** Locked containers, special drinks, maximum complexity.

**Pacing:** Difficulty increases gradually — no sudden spikes. Aligns with Relaxation pillar.

### Level Structure

**Level Organization:**
- **Linear roadmap:** Players progress along a visual roadmap/path
- **Automatic progression:** Game assigns each level to a city and mood (morning/night) — cities are visual themes, not gameplay-affecting
- **76 level environments:** 38 cities × 2 moods, with levels distributed across them
- **Start button:** Player sees their position on the roadmap, taps to start the next level

**Unlock Progression:**
- Complete a level to unlock the next one on the roadmap
- No branching paths — single linear journey
- Players can go back and replay any completed level

### Player Assistance

**Help Systems:**
- **Undo:** Limited per level, scaled by difficulty (1/2/3)
- **Extra Bottle:** Watch ad for additional empty container (max 2 per level)
- **Free Reset:** Restart any level at any time with no penalty
- **Tutorial by design:** First levels are easy enough to teach mechanics naturally
- **Hints (post-MVP):** Highlight a valid next move — not in initial release

### Replayability

**Replay Elements:**
- **Star improvement:** Replay completed levels to achieve 3-star rating with fewer moves
- **No daily puzzles** for initial release
- **No challenge modes** for initial release
- Future consideration: daily puzzles, challenge modes, procedural generation

---

## Progression and Balance

### Player Progression

**Progression Types:**
- **Content (Primary):** Unlimited procedurally generated levels — players always have a new puzzle to solve
- **Skill (Organic):** Players naturally improve at planning pours and recognizing patterns
- **Star Collection (Gate):** Stars earned per level serve as progression gates between level batches

#### Progression Pacing

- Levels are generated based on difficulty parameters, not hand-crafted
- Cities and morning/night moods are assigned randomly as visual themes — no gameplay effect
- Linear roadmap presentation — players move through one level at a time

### Difficulty Curve

**Pattern:** Gentle linear with multi-dimensional scaling

**Challenge Scaling (example values — to be balanced through playtesting):**

| Parameter | Scaling Rate | Example |
|---|---|---|
| **Colors** | Every ~20 levels | 4 colors (L1-20) → 5 (L20-40) → 6 (L40-60) → ... |
| **Containers** | Every ~10 levels | +1 container at L10, L20, L30... up to L100 |
| **Slots per container** | Every ~100 levels | 4 slots (L1-100) → 5 (L100-200) → 6 (L200-300) |
| **Hidden layers** | Post-MVP | Introduced at higher levels |
| **Locked containers** | Post-MVP | Introduced at higher levels |

*All numbers are initial estimates — final values require calculation and playtesting.*

#### Star Gate System

- Levels are grouped in **batches of 50**
- To unlock the next batch, players must earn a **minimum percentage (~80%) of total possible stars** from the current batch
- Maximum possible stars per batch: 150 (50 levels × 3 stars)
- Required to advance: ~120 stars (80% of 150)
- Players must play levels **sequentially within a batch** — no skipping
- Minimum **1 star required** to pass any individual level

#### Star Rating System

Stars are awarded based on **move efficiency** relative to optimal move count:

| Rating | Condition |
|---|---|
| 1 Star | Level completed (any move count) |
| 2 Stars | Completed within ~60% of optimal move count |
| 3 Stars | Completed within ~80% of optimal move count |

*Optimal move count and percentages to be calculated per level difficulty — these are placeholder values.*

#### Difficulty Options

- No explicit difficulty selector — difficulty scales automatically with level progression
- Undo system scales with implicit difficulty (1/2/3 undos)
- Extra bottles via ads provide additional help when needed
- Free restart available anytime

### Economy and Resources

_This game does not feature an in-game currency or resource system. Stars serve as the sole progression gate. Monetization is through rewarded ads (extra bottles) only._

---

## Level Design Framework

### Structure Type

**Procedural/Endless** — Unlimited levels generated algorithmically based on difficulty parameters. Levels are presented along a linear visual roadmap. No hand-crafted levels needed.

### Level Types

All levels are drink-sorting puzzles. Variety comes from parameter differences, not fundamentally different level types:

| Parameter | Variation |
|---|---|
| **Container count** | Scales up with progression |
| **Colors/drink types** | More colors = more complexity |
| **Slots per container** | More slots at higher levels |
| **Hidden layers** | Post-MVP — colors revealed as layers are removed |
| **City theme** | Randomly assigned — visual variety only |
| **Mood** | Morning (restaurant/juices) or night (bar/cocktails) — randomly assigned |

#### Tutorial Integration

- First ~10 levels serve as implicit tutorial through simplicity
- No explicit tutorial screens or text instructions
- Mechanics are self-explanatory: tap, pour, sort
- Early levels use few containers and few colors so players can't fail

#### Special Levels

- No boss levels or special challenge levels in MVP
- Future consideration: milestone levels at batch boundaries (every 50 levels)

### Level Progression

**Model:** Sequential with star gates

- Players progress one level at a time along a visual roadmap
- Levels grouped in batches of 50
- Star gate (~80% of possible stars) required to unlock next batch
- Within a batch, complete each level sequentially
- Minimum 1 star to pass any individual level

#### Unlock System

- Complete level → unlock next level in sequence
- Earn enough stars across batch → unlock next batch of 50
- No branching, no player choice in level order

#### Replayability

- Any completed level can be replayed at any time
- Replay to improve star rating (1 → 2 → 3 stars)
- Replaying is the primary way to earn stars needed for batch gates

### Level Design Principles

1. **Every puzzle must be solvable** — guaranteed through generation algorithm (recommended: build from solved state, shuffle backwards)
2. **Difficulty scales gradually** — no sudden spikes, aligned with Relaxation pillar

### Level Generation Approach

**Recommended method:** Start from a solved state (all containers sorted) and apply random valid reverse-pours to create the puzzle. This mathematically guarantees solvability. Exact implementation to be determined during development.

---

## Art and Audio Direction

### Art Style

**"Tropical Fresh"** — Premium casual aesthetic approved in visual direction phase.

- **Style:** 2D flat with warm, inviting visuals
- **Containers:** Rounded glass vessels with refraction effects — feel like real glassware
- **Liquids:** Fruit-inspired colors that look like actual drinks, not abstract colored blocks
- **Backgrounds:** City scenes with iconic landmark silhouettes/signs, atmospheric lighting
- **UI:** Minimal, elegant — doesn't compete with the visual experience
- **Animations:** Coroutine-based, no external tween library
  - **Selection:** Bottle lifts 0.25 units + scales 108% + golden glow (EaseOutBack, 0.15s)
  - **Deselection:** Smooth return to rest (EaseOutCubic, 0.12s)
  - **Pour:** 4-phase sequence — lift → tilt toward target → slot-by-slot liquid transfer → return (EaseOutCubic, ~0.5s total)
  - **Input blocked** during pour animation to prevent state corruption
  - Completion celebration: not yet implemented

#### Visual References

- Magic Sort (quality benchmark for polish and feel)
- Full visual direction spec: `_bmad-output/visual-direction-tropical-fresh.md`

#### Color Palette

- **Mango Amber** — warm golden tones
- **Deep Berry** — rich purple-reds
- **Tropical Teal** — fresh blue-greens
- **Watermelon Rose** — soft pinks
- **Lime Gold** — bright yellow-greens
- All filtered through warm, golden lighting

#### Camera and Perspective

- **2D flat view** — containers arranged across the screen
- Portrait orientation (standard mobile puzzle layout)
- City backgrounds visible behind containers
- City name and country displayed on screen

### Audio and Music

#### Music Style

Two distinct moods matching the day/night system:

| Mood | Style | Feel |
|---|---|---|
| **Morning** | Light, breezy, café-like | Relaxed breakfast vibe, acoustic warmth |
| **Night** | Smooth, jazzy, lounge-bar | Sophisticated evening atmosphere, mellow |

- **2 tracks total for MVP:** 1 morning track, 1 night track
- Future consideration: city-specific music variations
- Music should loop seamlessly and never feel intrusive

#### Sound Design

- **Pour sounds:** Satisfying liquid pouring — the most important sound in the game
- **Completion:** Gentle celebration sound on level complete
- **UI sounds:** Soft taps for container selection, subtle feedback
- **Star award:** Distinct pleasant sound for each star earned
- **All sounds support Relaxation pillar** — nothing jarring or loud

#### Voice/Dialogue

None — no voice acting planned.

### Aesthetic Goals

- **Relaxation:** Warm colors, soft sounds, no visual clutter — everything feels calming
- **Visual Delight:** Premium look that stands out from generic water sort games
- **Discovery:** Each city background feels fresh and recognizable — players notice they're somewhere new

---

## Asset Inventory

### Status Legend
- **Implemented** — asset exists in project
- **Placeholder** — code references it but asset is missing/stub
- **Not Started** — not yet created or sourced

### Visual Assets

| Asset | Count | Format | Status | Location |
|---|---|---|---|---|
| Bottle frame sprite | 1 | PNG | Implemented | Resources/Bottles/bottle_frame.png |
| Bottle mask sprite | 1 | PNG | Implemented | Resources/Bottles/bottle_mask.png |
| City backgrounds | 76 (38 cities × 2 moods) | PNG | Not Started | Addressables (planned) |
| Landmark icons/signs | 38 | PNG | Not Started | Addressables (planned) |
| UI sprites (buttons, panels) | ~10-15 | PNG | Placeholder | TBD |
| App icon | 1 | PNG | Not Started | — |
| Play Store screenshots | 5-8 | PNG | Not Started | — |

### Audio Assets

| Asset | Count | Format | Status | Location |
|---|---|---|---|---|
| Morning music track | 1 | OGG | Not Started | — |
| Night music track | 1 | OGG | Not Started | — |
| Pour SFX | 1-3 variants | WAV | Not Started | — |
| Select SFX | 1 | WAV | Not Started | — |
| Deselect SFX | 1 | WAV | Not Started | — |
| Level complete SFX | 1 | WAV | Not Started | — |
| Star award SFX | 1 | WAV | Not Started | — |
| UI tap SFX | 1 | WAV | Not Started | — |

### Font Assets

| Asset | Count | Format | Status | Location |
|---|---|---|---|---|
| Primary UI font | 1 | TTF/OTF | Not Started | — |

### Notes
- City backgrounds are loaded via Addressables to manage memory — each loaded on demand, released after
- Audio files referenced by `AudioClipType` enum in code but no actual clips are wired yet
- Full visual spec: `_bmad-output/visual-direction-tropical-fresh.md`

---

## Technical Specifications

### Performance Requirements

#### Frame Rate Target

- **60fps** — smooth and responsive for touch interactions and pour animations

#### Resolution Support

- Adaptive to device screen resolution
- Portrait orientation only
- UI must scale properly across different Android screen sizes and aspect ratios

#### Load Times

- Level loading should feel instant — puzzles are procedurally generated, not loading heavy assets
- City backgrounds should preload during transitions

### Platform-Specific Details

#### Android Requirements

- **Minimum OS:** Android 10+ (API level 29)
- **Orientation:** Portrait only
- **Network:** Fully offline — internet only required for ad-rewarded extra bottles
- **Storage:** Keep app size as small as possible — optimize city background assets
- **In-app purchases:** None planned — monetization through rewarded ads only
- **Google Play:** Must meet Play Store requirements for publishing

### Asset Requirements

#### Art Assets

| Asset Type | Estimated Count | Notes |
|---|---|---|
| **City backgrounds** | 76 (38 cities × 2 moods) | Morning and night variants per city |
| **Landmark icons/signs** | 38 (1 per city) | Iconic city identifier |
| **Container designs** | Small set of glass vessel variants | Reusable across all levels |
| **Drink colors/textures** | ~10-15 distinct drink visuals | Juice and cocktail themed |
| **UI elements** | Buttons, star icons, roadmap, menus | Minimal elegant design |
| **Roadmap art** | 1 scrollable roadmap | Linear path visualization |

#### Audio Assets

| Asset Type | Count | Notes |
|---|---|---|
| **Music tracks** | 2 | 1 morning, 1 night — loopable |
| **Pour SFX** | 1-3 variants | Core gameplay sound |
| **UI SFX** | 5-8 | Tap, select, deselect, complete, star award |
| **Completion SFX** | 1-2 | Level complete celebration |

#### External Assets

- Art: AI-assisted generation for backgrounds and visual elements
- Audio: AI-generated or royalty-free music and sound effects
- No asset store purchases planned initially

### Technical Constraints

- **Engine:** Unity (C#)
- **Rendering:** 2D flat — no 3D rendering needed
- **Data persistence:** Local save for player progress, star counts, and level state
- **Ad SDK:** Google AdMob or equivalent for rewarded video ads
- **No server infrastructure** for MVP — all game logic runs locally

---

## Development Epics

### Epic Overview

| # | Epic Name | Scope | Dependencies | Est. Stories |
|---|---|---|---|---|
| 1 | Core Puzzle Engine | Pour/sort mechanics, container system, win detection | None | 8-10 |
| 2 | Level Generation | Procedural puzzle creation, difficulty scaling | Epic 1 | 6-8 |
| 3 | Progression System | Roadmap, stars, batch gates, replay | Epic 1, 2 | 8-10 |
| 4 | UI & Menus | Main menu, roadmap screen, in-game HUD, settings | Epic 1 | 6-8 |
| 5 | Visual Theme | City backgrounds, landmarks, drink visuals, Tropical Fresh styling | Epic 1, 4 | 10-12 |
| 6 | Audio | Music tracks, SFX, pour sounds | Epic 1 | 4-6 |
| 7 | Monetization & Publishing | AdMob, extra bottle flow, Play Store build | Epic 1-6 | 5-7 |

### Recommended Sequence

1. **Epic 1: Core Puzzle Engine** — Must come first. Nothing works without the basic puzzle mechanics.
2. **Epic 2: Level Generation** — Prove that unlimited puzzles can be generated and are solvable.
3. **Epic 4: UI & Menus** — Basic navigation to make the game playable end-to-end.
4. **Epic 3: Progression System** — Stars, gates, and roadmap make it feel like a real game.
5. **Epic 5: Visual Theme** — Apply Tropical Fresh styling, city backgrounds, and drink visuals.
6. **Epic 6: Audio** — Add music and sound effects for polish.
7. **Epic 7: Monetization & Publishing** — AdMob integration and Play Store submission last.

### Vertical Slice

**The first playable milestone (after Epics 1-2):** A working puzzle game where players can tap to pour drinks between containers, complete a level by sorting all colors, and get a new procedurally generated puzzle. No UI polish, no city themes — just proof that the core gameplay is fun.

_Detailed epic breakdown with stories available in: `_bmad-output/epics.md`_

---

## Success Metrics

### Technical Metrics

#### Key Technical KPIs

| Metric | Target | Measurement Method |
|---|---|---|
| Frame rate | Stable 60fps | Unity profiler during testing |
| Crash rate | Zero crashes in normal gameplay | Manual testing + post-launch Play Store reports |
| Load time | Instant level transitions | Manual testing |
| App size | As small as possible | Android build output |
| Battery impact | Minimal drain during play sessions | Manual testing on device |

### Gameplay Metrics

#### Key Gameplay KPIs

| Metric | Target | Measurement Method |
|---|---|---|
| Play Store rating | 4.5+ stars | Play Store console |
| Puzzle solvability | 100% of generated puzzles are solvable | Automated testing during development |
| First 10 levels | Completable by anyone without confusion | Friend/family playtesting |
| Star gate fairness | Players can reasonably earn enough stars to progress | Playtesting |
| Undo/extra bottle usage | Players use them but don't feel forced to | Playtesting observation |

### Qualitative Success Criteria

- Players describe the game as **"relaxing"** and **"beautiful"** — the two key pillar words
- Friends/family testers want to keep playing after testing session ends
- Players notice and appreciate the city themes — they mention cities in feedback
- The game feels premium and polished, not like a generic clone
- Play Store reviews mention the unique travel/city experience

### Metric Review Cadence

**Pre-launch:**
- Personal playtesting throughout development (every epic)
- Friend/family testing after Epic 4 (full flow working) and after Epic 6 (polished)
- Final round of testing before Epic 7 (publishing)

**Post-launch:**
- Monitor Play Store ratings and reviews weekly
- Download counts tracked (growth dependent on marketing efforts)
- Read user reviews for qualitative feedback on pillars (relaxation, visual delight, discovery)

---

## Out of Scope

The following features and items are explicitly NOT in scope for JuiceSort v1.0:

**Features:**
- Online leaderboards, player of the day/month, best player
- Hint system (highlight valid next move)
- Hidden layers mechanic
- Locked containers
- Special drink types
- World map navigation / explore mode
- Collection system
- Daily puzzles / challenge modes
- In-app purchases
- Analytics / telemetry
- Multiplayer or social features

**Content:**
- City-specific music (only 2 tracks: morning/night)
- More than 38 cities
- Animations beyond basic (stretch goal)

**Platforms:**
- iOS
- PC / Web / Console

**Other:**
- Marketing strategy and execution
- Localization / multiple languages
- Voice acting

### Deferred to Post-Launch

1. Online features (leaderboards, player rankings)
2. Hint system
3. Advanced puzzle mechanics (hidden layers, locked containers, special drinks)
4. Explore/collection mode
5. City-specific music
6. Daily/challenge modes
7. Analytics integration
8. iOS port
9. Marketing campaign

---

## Assumptions and Dependencies

### Key Assumptions

- Unity free tier is sufficient for this project's scope and revenue level
- AI tools can produce acceptable quality art and audio assets for the "Tropical Fresh" direction
- Procedural puzzle generation via reverse-shuffling will produce fun, varied puzzles at all difficulty levels
- A solo developer with AI assistance can produce 76 city backgrounds of consistent quality
- The water sort puzzle genre will remain popular on mobile
- Players will accept rewarded ads as the sole monetization method

### External Dependencies

- **Unity Engine:** LTS version stability and Android build support
- **Google AdMob:** SDK for rewarded video ads
- **Google Play Store:** Approval process and ongoing compliance
- **AI Art Tools:** For generating city backgrounds, drink visuals, and UI assets
- **AI Audio Tools / Royalty-Free Packs:** For music tracks and sound effects

### Risk Factors

- If AI-generated art quality is inconsistent, manual touch-up time increases significantly
- If puzzle generation algorithm produces too-easy or too-hard puzzles, difficulty balancing requires iteration
- Play Store policy changes could affect ad monetization approach
- Solo developer availability interruptions could extend development timeline

---

## Document Information

**Document:** JuiceSort - Game Design Document
**Version:** 1.0
**Created:** 2026-03-18
**Author:** Nadir
**Status:** Complete

### Change Log

| Version | Date | Changes |
|---|---|---|
| 1.0 | 2026-03-18 | Initial GDD complete |
