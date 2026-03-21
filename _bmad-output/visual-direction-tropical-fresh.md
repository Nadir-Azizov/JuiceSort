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

### Not Yet Implemented
- **Completion:** Shimmer ripple across completed glass
- **UI feedback:** Gentle bounces on tap, soft transitions
- **Ambient:** Subtle glass sparkles, floating light particles
- **Screen transitions:** Fades/slides between menus

### Design Goals
- Every interaction should feel satisfying and organic
- No external tween libraries — pure coroutine-based with easing functions

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
