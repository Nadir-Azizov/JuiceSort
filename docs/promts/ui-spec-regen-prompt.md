# Prompt: Regenerate Gameplay HUD UI Spec — HTML Mockup as Single Source of Truth

## Context

I have two files attached:
1. **gameplay-hud-mockup.html** — the visual design mockup (THIS IS THE SOURCE OF TRUTH)
2. **gameplay-hud-ui-spec.md** — current implementation spec

The current spec has a critical problem: **Section §3 (RectTransform Values) describes the OLD code values, while Section §11 (HTML CSS) has the CORRECT design values.** When a dev agent reads §3, it implements wrong sizes. The §11.12 differences table proves this — almost every element has mismatched values between code and mockup.

## Task

Regenerate the entire UI spec as a single, conflict-free document where **every value comes from the HTML mockup**. The HTML renders at 360px width = 1080px Unity reference, so the conversion factor is **×3 for all pixel values**.

## Rules

### 1. Single Source of Truth
- Open the HTML file, extract every CSS value for every element
- Multiply all px values by 3 to get Unity reference pixels
- There must be ZERO leftover "current code" values — if HTML says coin icon is 28px (=84px Unity), write 84px, not 36px
- Do NOT include a "differences" table — there should be nothing to diff because the spec IS the target

### 2. RectTransform Calculations
For each element, calculate from the HTML CSS values:
- `anchorMin`, `anchorMax` — based on CSS position/layout (flexbox center = anchor 0.5, left-aligned = anchor 0, etc.)
- `pivot` — based on CSS transform-origin
- `sizeDelta` — width and height from CSS ×3
- `anchoredPosition` — from CSS margin/padding/position ×3
- `offsetMin`, `offsetMax` — when using stretch anchors

### 3. Visual Effects Translation
HTML has CSS effects that Unity cannot do 1:1. For each, provide the Unity equivalent:

| CSS Effect | Unity Equivalent |
|---|---|
| `linear-gradient(...)` | Note: "Requires gradient Material or top-to-bottom sprite. Flat color fallback: [hex]" — provide both the ideal and fallback |
| `box-shadow` (outer glow) | Note: "Add glow child Image behind element, size +Xpx each side, color rgba(...), sprite = blurred circle/rect" |
| `box-shadow` (3D depth, e.g. `0 4px 0 #color`) | Note: "Shadow child Image, anchored Ypx below face, color #hex" (already in current spec for settings buttons — keep this pattern) |
| `inset box-shadow` (top highlight) | Note: "Highlight child Image, anchored to top, height Xpx, color rgba(255,255,255,α)" |
| `border: Xpx solid color` | Note: "Use Outline component (width=X, color=#hex) OR 9-slice sprite with built-in border" |
| `text-shadow` | Note: "TMPro → Face → Underlay: color=#hex, offsetX=0, offsetY=Y, dilate=Z, softness=W" |
| `radial-gradient` (coin) | Note: "Use procedural gradient sprite (center #hex → edge #hex) OR baked gradient texture" |
| `backdrop-filter: blur()` | Note: "Not natively supported. Use GrabPass blur shader or pre-blurred background sprite" |
| `animation / @keyframes` | Provide Unity coroutine spec: property, range, duration, easing formula |

### 4. Font Size Mapping
Extract every `font-size` from HTML, multiply ×3, and write as TMPro fontSize. Map `font-weight` to TMPro fontStyle:
- 400 = Normal
- 500 = Normal (Nunito Medium not available as separate asset, note this)
- 600 = Bold (closest match in Nunito)
- 700 = Bold
- 800 = Bold (note: extra-bold not available, Bold is max)

### 5. Structure Requirements
Keep the same overall structure as the current spec but merge everything:

```
## 1. Canvas & Scaler (keep as-is)
## 2. GameObject Hierarchy (update sizes in comments if changed)
## 3. RectTransform Values — ALL FROM HTML ×3
     - For each element: RectTransform table + Visual Properties table
     - Visual Properties = Image color, sprite, TMPro settings, effects notes
## 4. Color Reference (extract from HTML, organize by mood)
## 5. Font Specifications (from HTML ×3)
## 6. Sprite Specifications (update sizes if changed)
## 7. Animation Specifications (from HTML @keyframes ×3)
## 8. Interaction Logic (keep as-is)
## 9. Constants Reference (UPDATE all constants to match HTML ×3)
## 10. Key Files (keep as-is)
```

**Remove Section §11 entirely** — its content should now BE sections §3-§9.

### 6. Special Attention Items

These elements have the biggest visual impact and the biggest current mismatch:

1. **CoinIcon** — HTML has radial gradient + border + "$" text + glow animation. Current spec has flat color circle. Fully spec the gradient coin with all layers.

2. **CoinDisplay pill** — HTML has golden border (`rgba(255,200,50,0.4)`), inner shadow, glow. Current spec has plain dark bg. Add all decorative layers.

3. **Settings 3D buttons** — HTML has multi-stop linear gradient + multiple box-shadows (3D depth + glow + highlight + inner edge). Current spec has flat face + flat shadow. Spec each visual layer as a child GameObject.

4. **Action buttons (Undo/Extra)** — HTML has gradient backgrounds + border + shadow. Current spec has flat color. Provide gradient note + fallback.

5. **Settings gear button** — HTML is 46px = 138px Unity. Current spec is 88px. This is a 57% size increase — make sure the TopBar height and layout accommodates this.

6. **All font sizes** — systematically larger in HTML. Every single TMPro fontSize must be updated.

### 7. Validation Checklist
Before finalizing, verify:
- [ ] Every element in §2 hierarchy has a corresponding §3 entry
- [ ] Every §3 entry has values derived from HTML CSS (not from old code)
- [ ] Every color hex in §4 matches what's in the HTML
- [ ] Every font size in §5 = HTML font-size × 3
- [ ] §9 constants match the new values
- [ ] No "differences table" exists — spec is unified
- [ ] Every CSS visual effect has a Unity implementation note
- [ ] Coin icon fully specced with gradient + border + "$" + glow layers

## Output Format
Output a single clean markdown file: `gameplay-hud-ui-spec-v2.md`
No commentary outside the spec. Production-ready for a dev agent to implement without any ambiguity.
