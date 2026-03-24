# Story 11.7: Loading Screen

Status: ready-for-dev

## Story

As a player,
I want to see a sweet loading screen with a cupboard of colorful bottles, an animated filling bottle, and a fruit-themed loading bar while the game loads,
so that the first impression of the game feels premium and appetizing.

## Priority

MEDIUM — Visual polish. Game is playable without it, but first impressions matter for retention.

## Acceptance Criteria

1. **Loading scene** — A dedicated loading scene (or splash screen) shown on app launch before the Hub screen.
2. **Cupboard background** — Upper half shows a wooden cupboard/shelf with 2 shelves displaying ~8 decorative bottles/items:
   - Top shelf: cocktail glass (pink, with cherry + umbrella), wine bottle (berry, with cork + label), mason jar (honey/amber), juice jug (orange, with handle)
   - Bottom shelf: smoothie cup (green, with straw + leaf), fruit squeezer (yellow, with lemon), berry bottle (purple), tropical bottle (red, with pineapple)
3. **Title** — "Juice" (gold, large) on one line, "Sort" (white, large) on the next. Bubbly/rounded font. Entrance animation (scale bounce). Shine sweep across text.
4. **Subtitle** — "Sort • Sip • Travel" below title, muted warm color.
5. **Feature bottle** — Large center bottle with golden cap, rounded shape. Fills up with 4 colored liquid layers (Mango→Teal→Rose→Berry) in sequence, then empties and repeats. Juice splash drops floating around it. Warm glow behind.
6. **Fruit loading bar** — Bottom area: "Loading" text + rainbow gradient bar that fills/empties. Small colored circle indicators (in our 5 drink colors) slide across the top of the bar as a playful touch. If TMPro emoji rendering works on target devices, use fruit emoji; otherwise use colored dots/shapes.
7. **Ambient effects** — Cross-star sparkles (8+), bokeh lights (3-4), rising bubbles (4-5). Warm sunset gradient background.
8. **Transition to Hub** — After loading completes, smoothly transition to Hub screen.
9. **Performance** — Loading screen itself must render immediately (lightweight assets only — no heavy textures or Addressables loads in the loading scene).

## Tasks / Subtasks

- [ ] Task 1: Create loading screen infrastructure (AC: 1, 8)
  - [ ] 1.1 Create `LoadingScreen.cs` in `Scripts/Game/UI/Screens/`
  - [ ] 1.2 Add loading screen to game flow: Boot → LoadingScreen → Hub
  - [ ] 1.3 Loading screen shows for minimum 2 seconds (or until async loading completes)
  - [ ] 1.4 Transition to Hub via ScreenManager when ready

- [ ] Task 2: Build cupboard background (AC: 2, 7)
  - [ ] 2.1 Warm sunset gradient background (full screen)
  - [ ] 2.2 Wooden cupboard frame: side panels, top header, 2 shelf boards with brackets
  - [ ] 2.3 All cupboard elements built programmatically with UI Images + colored rects
  - [ ] 2.4 8 decorative bottle/item silhouettes on shelves (colored shapes with liquid fills)
  - [ ] 2.5 Small sparkle effects on bottles (scale pulse animation)
  - [ ] 2.6 Warm glow behind each shelf

- [ ] Task 3: Build title and subtitle (AC: 3, 4)
  - [ ] 3.1 "Juice" text: gold color, large (68-72px), bold font, 3D shadow effect (offset colors below)
  - [ ] 3.2 "Sort" text: white, large (72-76px), bold, purple shadow depth
  - [ ] 3.3 Title entrance animation: scale from 0.5→1.08→1.0 over 1.2s, EaseOut
  - [ ] 3.4 Shine sweep: white gradient Image sliding across title, repeating every 3.5s
  - [ ] 3.5 Subtitle: "Sort • Sip • Travel", 13-14px, muted warm color, letterSpacing 5px

- [ ] Task 4: Build feature bottle with fill animation (AC: 5)
  - [ ] 4.1 Bottle shape: neck (narrow) + body (rounded bottom), glass border, subtle reflections
  - [ ] 4.2 Golden cap on top
  - [ ] 4.3 4 liquid layers fill sequentially from bottom (each ~34px height, 4.5s total cycle):
    - Layer 1 (Mango/gold): fills at 5-18%
    - Layer 2 (Teal): fills at 18-35%
    - Layer 3 (Rose): fills at 32-50%
    - Layer 4 (Berry): fills at 46-65%
    - Hold filled 78-90%, then drain to 0 and repeat
  - [ ] 4.4 Juice splash drops (4) floating around bottle (scale + translate animation)
  - [ ] 4.5 Warm radial glow behind bottle (pulsing opacity)

- [ ] Task 5: Build fruit loading bar (AC: 6)
  - [ ] 5.1 "Loading" text label above bar
  - [ ] 5.2 Bar track: 220×22px, dark semi-transparent, rounded
  - [ ] 5.3 Bar fill: rainbow gradient (shifting via UV animation), width animates 8%→85%→8% over 3s
  - [ ] 5.4 Top highlight on fill bar (inner reflection)
  - [ ] 5.5 Fruit indicators: 5 small colored circles (drink colors) or fruit emoji sliding left→right along the bar top, staggered. Use Image circles as safe fallback if emoji don't render on Android.

- [ ] Task 6: Ambient effects (AC: 7)
  - [ ] 6.1 Cross-star sparkles: 8 small cross shapes (CSS-style: 2 perpendicular lines), rotating + fading, scattered across screen
  - [ ] 6.2 Bokeh lights: 3-4 large soft circles, warm/purple colors, gentle float up/down
  - [ ] 6.3 Rising bubbles: 4-5 small circles with thin borders, drifting up from bottom edge

## Dev Notes

### Design Reference
- Primary mockup: `_bmad-output/mockups/loading-screen-mockup.html` — open in browser for exact visual
- All elements are CSS/HTML in the mockup → translate to Unity UI (Canvas + Image + TextMeshProUGUI + coroutine animations)

### Implementation Approach
- All elements built programmatically (no prefabs, no external art assets needed)
- Cupboard items are colored UI rectangles with rounded corners (UI Image shapes)
- Bottles are layered Image components: body border + liquid fill + reflection overlay
- Animations all via coroutines (sin waves for float, lerp for fills, translate for slides)

### Key Files to CREATE
- `Scripts/Game/UI/Screens/LoadingScreen.cs` — Main loading screen

### Key Files to MODIFY
- `Scripts/Game/UI/ScreenManager.cs` — Add LoadingScreen to flow
- `Scripts/Game/UI/GameFlowState.cs` — Add Loading state

### Performance Constraints
- Loading screen must render on the FIRST frame — no async dependencies
- All shapes are simple UI Images — no textures to load
- Gradient background: single 1×256 texture (< 1ms to generate)
- Target: 60fps on low-end Android during loading screen

### Architecture Notes
- Loading screen is a Canvas ScreenSpaceOverlay (same as all screens)
- Use same CanvasScaler setup (1080×1920, matchWidthOrHeight 0.5)
- During loading: actual game initialization happens in background (service setup, save data load, etc.)
- After loading: ScreenManager transitions to Hub with standard crossfade

### CRITICAL Anti-Patterns
1. **Do NOT load Addressables during loading screen render** — loading screen must be instant
2. **Do NOT use external sprite assets** — all visual elements are programmatic shapes
3. **Do NOT block the main thread** — any heavy init goes to coroutines
4. **Do NOT skip the loading screen** — minimum 2s display even if loading is instant (brand impression)

### References
- [Source: _bmad-output/mockups/loading-screen-mockup.html] — Full visual reference
- [Source: _bmad-output/epics.md#Epic-11] — Loading screen scope
- [Source: Magic Sort screenshot 1] — Industry reference for loading screen

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
