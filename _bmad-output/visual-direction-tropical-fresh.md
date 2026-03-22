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

### Shader-Based Liquid Fill (Epic 10)
- **Rendering:** Shader Graph material per bottle replaces sprite slot system
- **Fill parameters:** `_Fill0-3` (0-1) per color layer — smooth, continuous fill levels
- **Color parameters:** `_Color0-3` — each layer independently colored
- **Water surface:** Sinusoidal wave at each fill boundary (`sin(_Time.y * speed) * amplitude`)
- **Glass effect:** Shader handles transparency, subtle refraction, and inner glow
- **Result:** Liquid looks fluid and alive, not stepped/blocky like sprite slots

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

### New Animations (Epics 10-11)

#### Pour Animation (Shader-Based, replaces sprite slot toggle)
- **Source bottle:** `_FillAmount` smoothly decreases via Lerp coroutine
- **Dynamic tilt:** 1 layer=15°, 2 layers=25°, 3 layers=35°, 4 layers=45° (ease-in curve)
- **Liquid stream:** LineRenderer bezier curve from source mouth to target mouth, color-matched
- **Target bottle:** `_FillAmount` smoothly increases (synchronized with source drain)
- **Splash:** Small particle/shader wave effect when liquid lands in target

#### Select/Deselect Wobble
- **Trigger:** On bottle select or deselect
- **Effect:** Liquid surface tilts and oscillates — spring-damper physics
- **Formula:** `wobble = amplitude * sin(frequency * time) * exp(-damping * time)`
- **Duration:** 0.3-0.5s decay to zero
- **Implementation:** Shader `_WobbleX` parameter driven by coroutine

#### Bottle Cap/Cork Close
- **Trigger:** When bottle becomes fully sorted (after shimmer)
- **Sequence:** Cork sprite drops from above (bounce easing) → confetti/sparkle burst → scale punch (105% → 100%)
- **Duration:** ~0.5s total
- **Cork sprite:** Simple rounded shape, colored to match bottle — procedurally generated

#### Extra Bottle Pop-In (Epic 11)
- **Trigger:** When extra bottle is added mid-level
- **Effect:** All bottles smoothly animate to new positions (re-layout), new bottle scales from 0 to full size
- **Duration:** 0.3s re-layout + 0.2s pop-in

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

## Lighting & Effects (Epic 10)

### Glass Refraction
- URP Distortion node in bottle shader
- Subtle distortion behind glass — objects/background slightly warped through bottle
- Low intensity to avoid performance cost on mobile

### Bloom / Inner Glow
- URP Post Processing Bloom on liquid colors
- Low intensity (0.1-0.3), high threshold (>1.0) — only bright liquid colors bloom
- Creates soft glow inside bottles, especially vivid colors like Mango Amber and Tropical Teal
- Performance: use sparingly — expensive on mobile, test on low-end target devices

### Ambient Light
- Sprite-based halo behind each bottle (additive blending)
- Color tinted to match the dominant liquid color in the bottle
- Very subtle (0.05-0.15 alpha) — enhances depth without overpowering

### URP 2D Point Lights
- One 2D point light per bottle (or per row of bottles)
- Soft glow in the dominant liquid color
- Radius sized to cover bottle + small margin
- Intensity: low (0.3-0.5) — ambiance, not spotlight

### Color Enhancement
- Liquid colors more saturated than current sprite-based system
- Increased contrast against dark gradient backgrounds
- Warm golden filter applied through shader — fruit through golden light effect

### Free Resources (No Paid Assets)
- Unity Shader Graph — free with URP
- URP 2D Lights — built-in
- URP Post Processing / Bloom — built-in
- LineRenderer — built-in
- Particle System — built-in
- All shapes/sprites can be created procedurally

---

## Target Impression
| Element | Description |
|---|---|
| Colors | Saturated but warm — fruit through golden light |
| Containers | Rounded glass with shader-based liquid, refraction, and glow |
| Background | Tropical warmth, sunset gradient (#1a0a2e → #2d1b4e → #ff6b35) |
| UI | Minimal, elegant, soft-touch, organized HUD with coin display |
| Animations | Organic, liquid, satisfying — smooth pours, wobble, cork close |
| Lighting | Soft glow, bloom, 2D point lights — premium depth |
| Overall | Premium smoothie bar on a tropical island — Magic Sort quality |
