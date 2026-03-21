# JuiceSort Visual Direction: "Tropical Fresh"

## Overview
Premium casual aesthetic — a fusion of playful organic warmth and elegant tropical sophistication. Stands out on the Play Store by avoiding the flat/cheap or overly clinical look common in water sort games. Says: "I'm polished, I'm warm, come relax and play."

---

## Color Palette

### Liquid Colors (Fruit-Inspired, Warm & Rich)
- **Mango Amber** — golden orange, sunlit warmth
- **Deep Berry** — luxurious purple, not neon
- **Tropical Teal** — refreshing blue-green, ocean-inspired
- **Watermelon Rose** — warm pink, soft and inviting
- **Lime Gold** — sunlit green, fresh but warm

### Background & UI Colors
- **Warm sunset gradient** — golden hour lighting
- **Soft bokeh accents** — distant warm lights
- **Palm silhouette darks** — subtle depth elements
- **UI surfaces** — semi-transparent, warm-tinted glass

---

## Containers
- **Shape:** Rounded glass vessels — smoothie glasses / mason jar style
- **Material:** Realistic glass with subtle transparency and light refraction
- **Feel:** Soft, bubbly shapes — not clinical, not cartoonish
- Liquid layers visible through glass with a slight inner glow

---

## Background & Atmosphere
- **Setting:** Tropical veranda at dusk — warm, inviting
- **Lighting:** Golden hour sunset gradient
- **Depth elements:** Soft bokeh lights, distant palm silhouettes
- **Progression:** Time of day subtly shifts as player advances — morning freshness to golden evening

---

## UI Design
- **Philosophy:** Minimal and elegant, but approachable
- **Buttons:** Rounded, touchable — soft-touch feel
- **Typography:** Clean sans-serif, medium weight — friendly but not childish
- **Layout:** Score and level indicators float gently, never cluttering the puzzle
- **Surfaces:** Semi-transparent with warm tinting

---

## Animations & Micro-Interactions

### Implemented
- **Bottle selection:** Lift 0.25 units + scale 108% + golden frame glow (EaseOutBack, 0.15s). Deselect smoothly returns to rest (EaseOutCubic, 0.12s)
- **Pour animation:** 4-phase coroutine — lift source → tilt 25° toward target → transfer liquid slots one-by-one (0.08s each) → untilt and return to rest. All input blocked during animation (~0.5s total)
- **Completion shimmer:** White diagonal sweep across sorted bottle (0.4s) + gold frame pulse (0.15s) → completed tint. Plays after pour, gates win-check
- **Screen transitions:** Crossfade + vertical slide between all screens (0.3s out + 0.3s in, EaseInOutCubic). Overlays fade only
- **Button bounce:** Scale punch on press (92%) → spring overshoot (105%) → settle (100%). Applied to all UI buttons via reusable ButtonBounce component
- **Glass sparkles:** Diamond-shaped particles flash on idle containers (1-2 per bottle every 2-3s, pooled max 8). Paused during pour
- **Floating lights:** Warm amber bokeh circles drift upward in background (8 particles, 0.1-0.3 alpha, persistent across screens)

### Animation Spec Details (Epic 8)

#### 8.1 Completion Shimmer Ripple
- **Trigger:** When a container reaches fully-sorted state (all slots same color)
- **Effect:** A white shimmer highlight sweeps diagonally across the glass from bottom-left to top-right
- **Duration:** 0.4s sweep + 0.2s fade-out (0.6s total)
- **Easing:** Linear sweep, EaseOutCubic fade
- **Details:** Semi-transparent white overlay sprite, masked to bottle shape. After shimmer, bottle frame briefly pulses to gold (0.15s) then settles to a subtle completed tint
- **Timing:** Plays immediately after the final pour animation completes, before win-condition check triggers level complete

#### 8.2 Screen Transitions
- **Type:** Crossfade with slight vertical slide
- **Duration:** 0.3s fade-out + 0.3s fade-in (0.6s total)
- **Easing:** EaseInOutCubic
- **Screens:** Main Menu ↔ Roadmap ↔ Gameplay ↔ Level Complete
- **Details:** A full-screen CanvasGroup fades alpha 1→0 on the outgoing screen while sliding 20px upward, then the incoming screen fades 0→1 while sliding from 20px below to rest position
- **Constraint:** Input blocked during transition

#### 8.3 Button Bounce Feedback
- **Trigger:** On pointer down for any UI button
- **Effect:** Scale punch — button scales to 92% on press, then springs to 105% on release, settling back to 100%
- **Duration:** 0.06s press + 0.15s release (0.21s total)
- **Easing:** Press: EaseInCubic, Release: EaseOutBack
- **Scope:** All interactive buttons (HUD, menus, level complete, settings)
- **Details:** Applied via a reusable `ButtonBounce` component attached to button GameObjects

#### 8.4 Glass Sparkle Particles
- **Trigger:** Idle state — sparkles appear on all non-empty containers
- **Effect:** Tiny white diamond-shaped particles that briefly flash and fade on the glass surface
- **Rate:** 1-2 sparkles per container every 2-3 seconds (randomized)
- **Duration:** Each sparkle: 0.3s fade-in + 0.2s hold + 0.3s fade-out
- **Size:** Very small (2-4px visual), semi-transparent white
- **Details:** Spawned at random positions within the bottle mask bounds. Uses a lightweight particle pool (max 8 active across all containers). Disabled during pour animations to avoid visual clutter

#### 8.5 Floating Light Particles (Background Ambiance)
- **Effect:** Soft, warm-toned bokeh circles that drift slowly upward in the background
- **Count:** 6-10 particles visible at any time
- **Size:** 8-16px, varying opacity (0.1-0.3 alpha)
- **Speed:** Very slow drift upward (40-60px/s) with slight horizontal sway (sine wave, ±10px)
- **Color:** Warm amber/gold tint matching the sunset palette
- **Lifecycle:** Particles spawn below screen, drift up, fade out near top
- **Layer:** Behind containers, in front of background — on a dedicated sorting layer
- **Details:** Simple particle system or coroutine-driven pool. Should feel like distant bokeh lights or fireflies. Does not interact with gameplay

### Design Goals
- Every interaction should feel satisfying and organic
- No external tween libraries — pure coroutine-based with easing functions
- Animations must never block or delay core gameplay feel
- Ambient effects should be subtle — enhance atmosphere without distraction

---

## Sound Design Direction (Reference)
- Soft glass clinks on interaction
- Realistic liquid pouring sounds
- Tropical ambient undertone (gentle, not distracting)
- Satisfying completion chimes — warm, not sharp

---

## Target Impression
| Element | Description |
|---|---|
| Colors | Saturated but warm — fruit through golden light |
| Containers | Rounded glass with realistic transparency |
| Background | Tropical warmth, sunset atmosphere |
| UI | Minimal, elegant, soft-touch |
| Animations | Organic, liquid, satisfying |
| Overall | Premium smoothie bar on a tropical island |
