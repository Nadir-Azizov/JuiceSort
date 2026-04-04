# Story 11.8: Roadmap Screen Redesign

Status: review

## Story

As a player,
I want to access a vertical journey roadmap from the hub showing island level nodes on an ocean path with completed, current, and locked states,
so that I can see my progress and feel a sense of adventure through a tropical island world.

## Priority

MEDIUM — The roadmap already exists (from Epic 4) but needs full visual redesign to match the new hub-driven flow and tropical island theme.

## UI Spec

**Approved Mockup:** v10 (PNG) + v11 (HTML) — see `_bmad-output/mockups/roadmap_mockup.html`

---

### 1. Screen Overview

Roadmap is a vertically scrollable level selection screen. Player scrolls up/down through islands representing levels. Level 1 is at the **bottom**, higher levels go **upward**. Camera auto-scrolls to current level on open.

**Architecture:** Camera-scrolling with SpriteRenderers for map content, uGUI overlay for header/badges. See Section 16 for full architecture details.

---

### 2. Layer Structure (back to front)

```
Sorting Layer 0 — Sky Gradient          (no scroll — fullscreen quad or camera bg)
Sorting Layer 1 — Clouds Far            (0.2x scroll speed)
Sorting Layer 2 — Volcano               (0.3x scroll speed)
Sorting Layer 3 — Cliffs                (0.6x scroll speed)
Sorting Layer 4 — Clouds Near           (0.4x scroll speed)
Sorting Layer 5 — Stepping Stones       (1.0x scroll speed)
Sorting Layer 6 — Islands               (1.0x scroll speed)
(logical)    — Level Badges + Stars  (1.0x scroll speed, uGUI WorldSpace Canvas on RoadmapIslands sorting layer, sortingOrder 10)
UI Overlay       — Header, Close btn    (fixed, uGUI ScreenSpaceOverlay)
```

**Coordinate system:** All pixel values in this spec are in **design pixels at 1080x1920 reference resolution**. When using SpriteRenderers at 100 Pixels Per Unit, divide by 100 to get world units (e.g., 330px spacing = 3.3 world units).

---

### 3. Background

#### 3.1 Ocean Gradient (Shader/Code — no PNG)
```
Top:     #87CEEB (light sky blue)
Mid-top: #4DD8FF (bright cyan)
Mid:     #0E94C8 (ocean blue)
Mid-bot: #0A7DB0 (deeper blue)
Bottom:  #064A68 (deep ocean)
```
**Phase 1 approach:** Generate a gradient `Texture2D` programmatically. NOTE: `ThemeConfig.CreateGradientSprite()` only supports 2 colors — the ocean needs a **custom 5-stop gradient generator**. Create a 1px-wide, 256px-tall texture, interpolate between the 5 color stops at equal intervals (0%, 25%, 50%, 75%, 100%). Use `tex.SetPixel()` in a loop, then `tex.Apply()`. Wrap with `Sprite.Create()` and assign to a SpriteRenderer quad scaled to fill the camera viewport. The quad should be on `RoadmapSky` sorting layer and NOT scroll with the camera (parent it to the camera transform, or reposition it each frame). Cache the generated texture — destroy it in `OnDestroy` to avoid leaks. Size the quad to fill the camera frustum: `width = camera.orthographicSize * 2f * camera.aspect`, `height = camera.orthographicSize * 2f` (same pattern as `BackgroundManager.cs:96`).

**Phase 2 approach:** Replace with a custom shader or material for smoother gradients and dynamic effects.

#### 3.2 Caustic Light Effect
- Subtle white elliptical patches scattered across ocean
- `alpha: 0.04-0.08`
- Slow breathing animation (6s cycle, ease-in-out)
- Implementation: transparent SpriteRenderer quads with additive shader, or particle system
- **Phase 2 item** — skip in initial implementation

#### 3.3 Clouds
- **Asset:** `cloud.png` (single cloud, reused with scale/flip variation)
- **Count:** 4-5 SpriteRenderer instances, randomly placed
- **Parallax:** Far clouds 0.2x, near clouds 0.4x scroll speed
- **Animation:** Slow horizontal drift (+/-20px, 18-25s cycle)
- **Opacity:** SpriteRenderer color alpha 0.3-0.5
- **Scale variation:** transform.localScale 0.6x-1.0x
- **Phase 2 item** — skip in initial implementation

#### 3.4 Cliffs (Side Decorations)
- **Assets:** `cliff_left.png`, `cliff_right.png`
- **Placement:** SpriteRenderers alternating left/right, every ~800px (8.0 world units) vertical distance
- **Position:** Partially off-screen (anchored to screen edges)
- **Parallax:** 0.6x scroll speed
- **Opacity:** SpriteRenderer color alpha 0.85-0.9
- **Color boost:** Saturation 1.8x, Brightness 1.25x, Contrast 1.15x (applied in Unity import or material)
- **Phase 2 item** — skip in initial implementation

#### 3.5 Volcano (Reserved — Post Level 30)
- **Asset:** `volcano.png`
- NOT displayed in first 30 levels
- Will appear after Level 30 as "coming soon" / zone 2 teaser

#### 3.6 Palm Leaves (Reserved — Milestone Decoration)
- **Assets:** `palm_left.png`, `palm_right.png`
- NOT displayed initially
- Reserved for Level 30 milestone frame decoration

---

### 4. Level Nodes (Islands)

#### 4.1 Island Assets & Rotation

| Asset | Use | Design Size (px) | World Size (units) |
|-------|-----|-------------------|-------------------|
| `island_A.png` | Normal variant 1 (small, one palm) | 260x260 | 2.6x2.6 |
| `island_B.png` | Normal variant 2 (rocky, barrel) | 260x260 | 2.6x2.6 |
| `island_D.png` | Normal variant 3 (bushy, compact) | 260x260 | 2.6x2.6 |
| `island_E.png` | Normal variant 4 (bushy, bigger) | 260x260 | 2.6x2.6 |
| `island_C.png` | BOSS level (every 10th) | 380x380 | 3.8x3.8 |
| `island_locked.png` | Locked/unavailable levels | 230x230 | 2.3x2.3 (native size, NO additional LockedScale applied) |

**Rotation Pattern (repeating):**

Formula: `variant = (levelNumber - 1) % 4` → 0=A, 1=B, 2=D, 3=E. Boss (every 10th) = C.
Flip: `cyclePos = (levelNumber - 1) / 4`, flip when `cyclePos % 2 == 1`. Boss never flips.

**Note:** The boss level does NOT reset the variant counter. Level 11 uses `(11-1)%4 = 2` → island_D, NOT island_A. The variant is computed from raw `levelNumber`, ignoring boss slots.

```
Level 1  -> island_A   variant=0  cyclePos=0  no flip
Level 2  -> island_B   variant=1  cyclePos=0  no flip
Level 3  -> island_D   variant=2  cyclePos=0  no flip
Level 4  -> island_E   variant=3  cyclePos=0  no flip
Level 5  -> island_A   variant=0  cyclePos=1  FLIP
Level 6  -> island_B   variant=1  cyclePos=1  FLIP
Level 7  -> island_D   variant=2  cyclePos=1  FLIP
Level 8  -> island_E   variant=3  cyclePos=1  FLIP
Level 9  -> island_A   variant=0  cyclePos=2  no flip
Level 10 -> island_C   BOSS       (never flips)
Level 11 -> island_D   variant=2  cyclePos=2  no flip
Level 12 -> island_E   variant=3  cyclePos=2  no flip
Level 13 -> island_A   variant=0  cyclePos=3  FLIP
Level 14 -> island_B   variant=1  cyclePos=3  FLIP
```

See `RoadmapConfig.GetIslandSprite()` and `RoadmapConfig.ShouldFlip()` for implementation.

**IMPORTANT:** Apply flip via `SpriteRenderer.flipX = true`, NOT by negating `transform.localScale.x`. Negative scale would mirror all child GameObjects (WorldSpace Canvas with badges/stars), causing text to appear backwards.

#### 4.2 Layout — Zigzag Pattern

```
Level 1 (bottom):  LEFT   x: -2.6 (ZigzagOffsetX from center)
Level 2:           RIGHT  x: +2.6
Level 3:           LEFT   x: -2.6
Level 4:           RIGHT  x: +2.6
...alternating (odd=LEFT, even=RIGHT)...
Level 10 (BOSS):   CENTER x: 0
Level 11:          LEFT   x: -2.6
...continues...
Level 20 (BOSS):   CENTER x: 0
...
Level 30 (BOSS):   CENTER x: 0
```

X offset is a fixed constant (`RoadmapConfig.ZigzagOffsetX = 2.6f`) from screen center, NOT calculated from screen edge.

**Note:** The zigzag breaks around boss levels. Since bosses are centered and the odd/even rule uses raw `levelNumber`, levels 9 (odd=LEFT) and 11 (odd=LEFT) are both on the left side with boss 10 in the center between them. This is intentional — the stepping stones create a natural path through the centered boss.

**Vertical Spacing (world units at 100 PPU):**
- Normal -> Normal: 3.3 units
- Normal -> Boss or Boss -> Normal: 4.0 units

#### 4.3 Island States

**Completed:**
- Full color island sprite (A/B/D/E based on rotation)
- Normal scale (1.0x)

**Current Level:**
- Full color island sprite
- **Scale: 1.35x** (larger than others)
- **Purple glow effect** underneath (SpriteRenderer with additive shader):
  - Color: `RoadmapConfig.GlowColor` (purple, ~rgba(232, 84, 209, 0.25)) -> transparent
  - Ellipse shape, ~1.3x the **scaled** island size (i.e., 2.6 * 1.35 * 1.3 ≈ 4.56 world units for normal islands)
  - Pulse animation: opacity 0.7<->1.0, scale 1.0<->1.15, 2s cycle
  - **Phase 1:** implement glow as a simple scaled, semi-transparent circle sprite. No asset needed — generate a white circle texture at runtime using the same `Texture2D` + `Sprite.Create()` technique as stepping stones (Section 7.3). Tint via `SpriteRenderer.color` using `RoadmapConfig.GlowColor`. The glow SpriteRenderer must use `sortingOrder = -1` on the `RoadmapIslands` sorting layer (island sprites default to sortingOrder 0) so it renders **behind** the island.
  - **Phase 2:** add pulse animation

**Locked (Normal):**
- Uses `island_locked.png` (foggy, desaturated) — already smaller at native size (2.3 vs 2.6 units)
- No additional scale applied — the smaller PNG provides the visual distinction
- `RoadmapConfig.LockedScale` (0.88f) is defined as a constant but currently unused — reserved for future use if locked islands need further scaling

**Boss Locked:**
- `island_C.png` with SpriteRenderer.color tinted: `new Color(0.6f, 0.6f, 0.6f, 0.7f)`

---

### 5. Level Badge (Below Island)

Circle badge positioned at the bottom edge of each island. Rendered as **uGUI elements on a WorldSpace Canvas** that is a child of each island's transform (so badges scroll with islands at 1.0x).

**WorldSpace Canvas setup per island:**
- `RenderMode = WorldSpace`
- `RectTransform` size: 200 x 400 (canvas pixels). Width 200 is fine (badge is 56-62px). Height must be 400 (not 200) because badge is at y ≈ -148 canvas pixels from center — a 200px canvas only spans ±100, so the badge would be outside bounds and invisible. At 400px height, canvas spans ±200 from center, accommodating the badge at -148.
- `Canvas.localScale`: `new Vector3(0.01f, 0.01f, 1f)` — converts 100 canvas pixels to 1 world unit (matching 100 PPU)
- `Canvas.sortingLayerName`: `"RoadmapIslands"` — **MUST be same or later sorting layer as islands**, because sorting layer order takes absolute priority over sorting order
- `Canvas.sortingOrder`: 10 (islands use default sortingOrder 0, so 10 puts badges above)
- Position: offset from island center per badge/star offset constants

#### 5.1 Badge Dimensions (design pixels)
- Normal: 56x56, font-size 22
- Boss/Current: 62x62, font-size 26

#### 5.2 Badge States

**Phase 1 — Functional badges (solid colors, no gradients/gloss):**
- Completed: solid purple `#C22AA8`, white level number text (e.g., "1", "10", "25"), white border
- Completed Boss: solid gold `#E8A800`, white level number text, white border
- Current: bright pink `#E855D0`, white level number text, white border
- Locked: gray `#6A7789`, lock icon (use TMPro text with Unicode lock character `\U0001F512` or plain text "🔒" — no separate icon asset needed), subtle border

**Phase 2 — Full visual polish (add gradients, shadows, gloss, pulse):**

Completed (Normal):
```
Background: linear-gradient(145deg, #E855D0, #C22AA8, #9C1E88)
Border: 3px solid rgba(255,255,255,0.4)
Shadow: 0 4px 0 #7A1568
Gloss: top 46% semi-circle, rgba(255,255,255,0.28) -> transparent
Text: white, bold
```

Completed (Boss):
```
Background: linear-gradient(145deg, #FFD700, #E8A800, #C48800)
Border: 3.5px solid rgba(255,255,255,0.6)
Shadow: 0 4px 0 #8A6000
Text: white, bold
```

Current Level:
```
Background: linear-gradient(145deg, #FF70F0, #E855D0, #C22AA8)
Border: 3px solid rgba(255,255,255,0.6)
Glow: 0 0 22px rgba(232,85,208,0.7), 0 0 44px rgba(232,85,208,0.3)
Pulse animation: glow intensity oscillates, 2s cycle
Text: white, bold
```

Locked:
```
Background: linear-gradient(145deg, #8A95A7, #6A7789, #525F70)
Border: 2px solid rgba(255,255,255,0.12)
Shadow: 0 4px 0 #3D4854
Icon: TMPro lock character (Unicode U+1F512), rgba(255,255,255,0.65)
```

#### 5.3 Badge Position
- Centered horizontally on island
- `BadgeOffsetY = -0.18f` world units below the **island's center-bottom edge** (NOT below the transform origin/sprite center). For a 2.6-unit island, center-bottom is at `y = origin.y - islandHalfHeight` (i.e., `origin.y - 1.3`). Badge center is at `origin.y - 1.3 - 0.18 = origin.y - 1.48`.
- Formula: `badgeWorldY = islandWorldPos.y - (islandWorldSize / 2) + BadgeOffsetY`

---

### 6. Stars (3-Star Rating)

#### 6.1 Appearance
- 3 stars per completed/current level
- Size: 22x22 design pixels each, 3px gap between
- Position: between island bottom edge and badge, `StarsOffsetY = 0.08f` above badge center (i.e., at badge y + 0.08 world units)
- Rendered as uGUI Image elements on the same WorldSpace Canvas as badges

#### 6.2 Star States

**Earned Star (Phase 1 — fill + tint only):**
```
Fill: #FFD700 (gold) — apply via Image.color
```

**Empty Star (Phase 1 — fill + tint only):**
```
Fill: #556075 (dark blue-gray) — apply via Image.color
Opacity: 0.5 — apply via Image.color alpha
```

**Phase 2 enhancements (add stroke, shadow):**
- Earned stroke: `#A08600`, width 1.2px — use `UnityEngine.UI.Outline` component
- Empty stroke: `#3A4560`, width 1.5px — use `UnityEngine.UI.Outline` component
- Drop shadow: offset (0, -1), color `rgba(0,0,0,0.5)` — use `UnityEngine.UI.Shadow` component

#### 6.3 Visibility
- Shown only for completed and current levels
- NOT shown for locked levels

#### 6.4 Star Sprite Source
No star PNG asset exists in the project. Generate a star shape at runtime:
- Create a small `Texture2D` (e.g., 64x64) and draw a 5-pointed star polygon using `SetPixel()`, fill white, then `Apply()`. Wrap with `Sprite.Create()`.
- Tint via uGUI `Image.color` to gold (#FFD700) for earned or dark gray (#556075 @ 50% alpha) for empty.
- Cache the generated sprite in `RoadmapMapView` — create once, reuse for all star Image elements.
- The same star sprite is also used for the header star counter icon (Section 9.3).
- Alternatively, use a TMPro `TextMeshProUGUI` with Unicode star character `★` (U+2605) sized to 22px — simpler but less control over fill/stroke styling.

#### 6.5 Star Data Source
Stars come from existing `LevelRecord.Stars` (0-3) via `IProgressionManager.GetAllLevelRecords()`. No new save data fields required.

---

### 7. Stepping Stones (Path Between Islands)

#### 7.1 Approach — Procedural (No PNG Asset)
Stones are drawn programmatically along a **cubic bezier S-curve** between islands using SpriteRenderers (simple ellipse sprites or procedural mesh).

#### 7.2 Bezier Curve Definition
For each pair of adjacent levels:

```csharp
// Calculate endpoints offset from island centers by half-height
// (so stones start/end at island edges, not centers)
float lowerHalf = lowerNode.IsBoss ? BossIslandSize / 2f : NormalIslandSize / 2f;
float upperHalf = upperNode.IsBoss ? BossIslandSize / 2f : NormalIslandSize / 2f;

Vector2 P0 = lowerNode.WorldPosition + new Vector2(0, lowerHalf);  // center-top of lower
Vector2 P3 = upperNode.WorldPosition - new Vector2(0, upperHalf);  // center-bottom of upper

// P1, P2 = control points creating S-curve
// P1 pulls curve away from start side, P2 pulls toward end side
bool goingLeftToRight = P3.x > P0.x;
// Control points create S-curve: P1 swings away from start, P2 approaches end
// Values tuned from HTML mockup v11 — adjust visually at runtime if needed
if (goingLeftToRight) {
    P1 = new Vector2(P0.x - 0.3f, Mathf.Lerp(P0.y, P3.y, 0.45f));  // swing left, then curve right
    P2 = new Vector2(P3.x + 0.2f, Mathf.Lerp(P0.y, P3.y, 0.35f));  // approach from right side
} else {
    P1 = new Vector2(P0.x + 0.3f, Mathf.Lerp(P0.y, P3.y, 0.45f));  // swing right, then curve left
    P2 = new Vector2(P3.x - 0.2f, Mathf.Lerp(P0.y, P3.y, 0.35f));  // approach from left side
}
```

**Note:** The S-shape comes from horizontal offsets (P1 overshoots start side, P2 overshoots end side). `BossIslandSize = 3.8`, `NormalIslandSize = 2.6` (from `RoadmapConfig`). Tune control point values visually at runtime.

**Tuning tip:** The S-curve shape is most visible when islands are on opposite sides (L↔R). When both islands are on the same side (e.g., L→C→L around a boss), the curve becomes more vertical. Expose P1/P2 offsets as `[SerializeField]` fields during development for real-time tuning in the Inspector, then hardcode final values.

#### 7.3 Stone Rendering — Phase 1 (Simple)
- **Count:** 8 stones per path segment
- **Distribution:** t = 0.04 to 0.96 along bezier curve, evenly spaced
- **Shape:** Simple ellipse SpriteRenderer
  - Scale: ~0.44 x 0.28 world units
  - Random variation: +/-0.04 width, +/-0.02 height (use deterministic randomization via `new System.Random(levelNumber * 100 + stoneIndex)` — do NOT use `UnityEngine.Random.InitState()` as it corrupts the global random state for all other systems)
- **Sprite:** No asset needed — generate a small white circle texture at runtime via `Texture2D` + `Sprite.Create()`, then tint via SpriteRenderer.color. Stone sprite generation and rendering logic should live in `RoadmapMapView`, not in `RoadmapConfig`.
- **Unlocked path:** stone color `new Color(0.67f, 0.61f, 0.53f, 0.75f)`
- **Locked path:** stone color `new Color(0.31f, 0.39f, 0.47f, 0.5f)` (dark, muted)

#### 7.4 Stone Rendering — Phase 2 (Full Detail)
Add layered visuals per stone (bottom to top):
1. Water ring: `rgba(50, 190, 225, 0.18)`, rx+8, ry+4
2. Shadow: `rgba(30, 60, 80, 0.22)`, offset y+4
3. Stone body: `rgba(170, 155, 135, 0.75)`, stroke `rgba(130,115,95,0.4)`
4. Highlight: `rgba(215, 200, 178, 0.55)`, offset y-2, smaller
5. Moss patch: `rgba(110, 165, 55, 0.5)`, small ellipse, random offset

Locked path: No water ring, no moss, muted highlight.

---

### 8. Scroll & Camera System

#### 8.1 Scroll Mechanics
- **Method:** Move the roadmap Camera's `transform.position.y` via touch drag input
- **Direction:** Drag up = camera moves up = higher levels visible
- **Momentum:** Deceleration after release. Apply framerate-independent decay: `velocity *= Mathf.Pow(ScrollDecay, Time.deltaTime * 60f)` where `ScrollDecay = 0.92f` (tuned for 60fps). This normalizes scroll feel across different framerates.
- **Bounds:**
  - Bottom: Level 1 world position.y - 1.0 units
  - Top: Last **rendered** level position.y + 2.0 units (includes the 5 locked levels from Section 12.5, so all rendered nodes are scrollable)

#### 8.2 Auto-Scroll on Open
- On screen activation, camera smoothly scrolls to current level
- Duration: 0.8s, ease-out curve (manual lerp — DOTween is NOT installed in this project)
- Current level centered vertically on screen

#### 8.3 Parallax Implementation (Phase 2)
```csharp
void LateUpdate() {
    float scrollY = _camera.transform.position.y;
    _cloudsFarParent.position    = new Vector3(0, scrollY * 0.2f, 0);
    _volcanoParent.position      = new Vector3(0, scrollY * 0.3f, 0);
    _cloudsNearParent.position   = new Vector3(0, scrollY * 0.4f, 0);
    _cliffsParent.position       = new Vector3(0, scrollY * 0.6f, 0);
    // Islands, stones, badges at 1.0x — they are children of the world, camera moves past them
}
```

---

### 9. Header (Fixed UI Overlay)

Rendered as a **uGUI ScreenSpaceOverlay Canvas** (sorting order above map camera), separate from the map content. This integrates with the existing ScreenManager pattern.

#### 9.1 Layout
- Fixed at top of screen
- The header's ScreenSpaceOverlay Canvas must include a `CanvasScaler` with `ScaleMode = ScaleWithScreenSize`, `referenceResolution = 1080x1920`, `matchWidthOrHeight = 0.5f` (same config as existing screens — see `RoadmapScreen.Create()` lines 107-110)
- Gradient fade: dark teal `rgba(8,50,75,0.95)` -> transparent (bottom edge)
- Height: ~90px design pixels (including status bar safe area)
- **Gradient technique:** Use `ThemeConfig.CreateGradientSprite(Color top, Color bottom)` (existing project utility) to generate a gradient texture at runtime. Pass `HeaderBgColor` as top and `new Color(0.03f, 0.20f, 0.29f, 0f)` (same color, alpha 0) as bottom. Apply to a uGUI `Image` component. **Important:** This method creates an uncached texture — store a reference and call `Destroy(sprite.texture)` in `OnDestroy` to prevent memory leaks. Alternatively, create once in `Create()` and reuse across screen activations.

#### 9.2 Left — Title
- Text: "JuiceSort"
- Font: Nunito Bold (existing project font — Fredoka is not available as TMP SDF asset)
- Size: 22 design pixels
- Color: white
- Text shadow: via TMPro shadow settings

#### 9.3 Right — Star Counter
- Background: Image with color `rgba(0,0,0,0.4)`, rounded corners (use a small 9-sliced rounded rect texture generated at runtime, or skip rounding and use a plain rect — rounded corners are a nice-to-have, not blocking)
- Border: outline `rgba(255,215,0,0.5)`
- Content: star icon (gold, reuse the same runtime-generated star sprite from Section 6.4) + earned star count text (e.g., "18")
- Font: Nunito SemiBold, 14 design pixels, white
- Star count display: show earned stars only (e.g., "18"), no denominator. Total possible stars is not tracked by the save system. Use `IProgressionManager.GetTotalStars()` if available, otherwise sum `Stars` from `GetAllLevelRecords()`.

#### 9.4 Close Button (X)
- Position: top-right corner, within safe area
- Visual: circular semi-transparent background `rgba(0,0,0,0.3)`, white X icon (use TMPro text with "✕" U+2715 character — no separate icon asset needed, consistent with lock icon approach)
- Size: 44x44 design pixels
- Action: `ScreenManager.TransitionTo(GameFlowState.MainMenu)` (MainMenu = Hub screen)
- Must be present in Phase 1

---

### 10. Level Detail Popup (On Tap)

*Phase 3 item — NOT in scope for Phase 1 or Phase 2*

#### 10.1 Trigger
- Tap any island node -> popup appears

#### 10.2 Popup Variants
- **Completed:** Island preview, stars, REPLAY button (green)
- **Current:** Island preview, empty stars, PLAY button (green, pulsing)
- **Locked:** Desaturated island, lock icon, "Complete Level X to unlock", LOCKED button (gray)

#### 10.3 Phase 1 Tap Behavior (Without Popup)
Until Phase 3 popup is implemented, tapping an island directly triggers level action:
- Completed -> `GameplayManager.StartReplay(levelNumber)`
- Current (paused) -> `GameplayManager.ResumeLevel(levelNumber)` (preserves in-progress game)
- Current (fresh) -> `GameplayManager.StartLevel(levelNumber)`
- Locked -> no action (ignore tap)
- Gate-blocked -> no action (the current level node is not shown if gate-blocked, matching existing behavior — see Section 12.5)
- Then transition: `ScreenManager.TransitionTo(GameFlowState.Playing)`

This preserves existing `RoadmapScreen.OnLevelTapped()` behavior, including the paused-level resume path. Use `GameplayManager.HasPausedLevel(levelNumber)` to distinguish paused vs fresh.

**Tap Detection:** Islands are SpriteRenderers (not UI elements), so taps cannot use the uGUI EventSystem. On touch/click begin:
1. Convert screen position to world: `Vector2 worldPos = _camera.ScreenToWorldPoint(Input.mousePosition);`
2. Query: `var hit = Physics2D.OverlapPoint(worldPos);` (NOT `Physics2D.Raycast` — that casts a ray with direction, `OverlapPoint` is the correct API for point queries)
3. Each island GameObject must have a `BoxCollider2D` sized to its island sprite bounds
4. Each island GameObject must have a small `RoadmapIslandMarker : MonoBehaviour` component with a `public int LevelNumber` field (since `RoadmapNodeData` is a plain class, not a MonoBehaviour, and can't be attached to a GO). Alternatively, maintain a `Dictionary<Collider2D, int>` lookup in `RoadmapMapView`.
5. **Tap vs scroll guard:** Only process the tap if the touch was short (duration < 0.2s) and drag distance was small (< 10px). If the user was scrolling (momentum velocity > `ScrollMinVelocity`), ignore the release as a scroll gesture, not a tap.
6. If hit is not null and tap is valid, retrieve the level number and call `OnLevelTapped(levelNumber)`

#### 10.4 Popup Animation (Phase 3)
- Appear: scale(0.7->1) + fade, 0.3s, cubic-bezier overshoot
- Dismiss: scale(1->0.9) + fade out, 0.2s
- Background overlay: `rgba(0,0,0,0.55)`, fade in 0.2s

---

### 11. Boss Level Specifics

| Property | Normal Level | Boss Level (every 10th) |
|----------|-------------|------------------------|
| Island asset | A/B/D/E rotation | island_C |
| Island size | 2.6 world units | 3.8 world units |
| Badge style | Purple (solid Phase 1, gradient Phase 2) | Gold (solid Phase 1, gradient Phase 2) |
| Layout position | Left/Right zigzag | Center |
| Spacing above/below | 3.3 (normal↔normal), 4.0 (if adjacent to boss) | 4.0 (always — any level adjacent to a boss uses 4.0) |
| Coin reward | 1× (standard) | 3× (triple) — implemented in gameplay/economy system, NOT in roadmap UI |

**Note:** Boss coin multiplier (3×) is a gameplay mechanic, not a roadmap UI concern. It will be implemented in the economy/gameplay epic. The roadmap only needs to display the boss island visually larger and with a gold badge.

---

### 12. Data Model

#### 12.1 Existing Data — DO NOT MODIFY

The existing `LevelNodeData` class (in `Scripts/Game/UI/Components/LevelNodeData.cs`) is **shared with `StarGateScreen`**. Do NOT modify it.

Existing `LevelRecord` fields available from save data:
- `LevelNumber` (int)
- `CityName` (string)
- `CountryName` (string)
- `Mood` (LevelMood)
- `Stars` (int, 0-3)

**Note:** `bestMoves`, `bestTime`, `coinsEarned` are NOT stored in the existing save system. Do not reference them.

#### 12.2 New Data Class — RoadmapNodeData ✅ IMPLEMENTED

**File:** `Scripts/Game/UI/Components/RoadmapNodeData.cs`

This does NOT replace `LevelNodeData`.

```csharp
public enum RoadmapLevelState { Locked, Current, Completed }

public class RoadmapNodeData
{
    public int LevelNumber;
    public RoadmapLevelState State; // Completed, Current, Locked
    public int StarsEarned;         // 0-3
    public bool IsBoss;             // true for every 10th level
    public Sprite IslandSprite;     // resolved sprite reference
    public bool FlipX;              // whether to flip horizontally
    public Vector2 WorldPosition;   // calculated position in world space
}
```

#### 12.3 Island Assignment ✅ IMPLEMENTED

See `RoadmapConfig.GetIslandSprite()` and `RoadmapConfig.ShouldFlip()` for actual implementation. Sprites are loaded via `Resources.Load<Sprite>()` with caching (see Section 13.1).

```csharp
// From RoadmapConfig.cs — uses Resources.Load with string paths
Sprite GetIslandSprite(int levelNumber, RoadmapLevelState state) {
    if (state == RoadmapLevelState.Locked && levelNumber % 10 != 0)
        return LoadSprite("Roadmap/Islands/island_locked");
    if (levelNumber % 10 == 0)
        return LoadSprite("Roadmap/Islands/island_C"); // Boss

    int variant = (levelNumber - 1) % 4;
    string path = variant switch {
        0 => "Roadmap/Islands/island_A",
        1 => "Roadmap/Islands/island_B",
        2 => "Roadmap/Islands/island_D",
        3 => "Roadmap/Islands/island_E",
        _ => "Roadmap/Islands/island_A"
    };
    return LoadSprite(path);
}

bool ShouldFlip(int levelNumber) {
    if (levelNumber % 10 == 0) return false; // boss never flips
    int cyclePos = (levelNumber - 1) / 4;
    return cyclePos % 2 == 1;
}
```

#### 12.4 Position Calculation ✅ IMPLEMENTED

See `RoadmapConfig.GetNodePosition()` for actual implementation. Uses fixed `ZigzagOffsetX` constant, NOT screen-width-based calculation.

```csharp
// From RoadmapConfig.cs
Vector2 GetNodePosition(int levelNumber) {
    float y = CalculateY(levelNumber); // cumulative spacing
    float x;

    if (levelNumber % 10 == 0) {
        x = 0f; // boss = centered
    } else {
        x = (levelNumber % 2 == 1) ? -ZigzagOffsetX : ZigzagOffsetX;
        // ZigzagOffsetX = 2.6f (fixed offset from center)
    }

    return new Vector2(x, y);
}
```

#### 12.5 Level Node Population

When building the roadmap node list:
1. **Completed levels** — all levels from `IProgressionManager.GetAllLevelRecords()`, state = `Completed`
2. **Current level** — `IProgressionManager.CurrentLevel`, state = `Current`. **NOT shown if gate-blocked** (preserve existing behavior from `RoadmapScreen.BuildNodeList()`: check `IsAtBatchGate(previousLevel) && !CanPassBatchGate()`)
3. **Locked levels** — show **5 locked levels** beyond the current level (or gate-blocked level) to give the player a sense of upcoming progression. State = `Locked`. These use `island_locked.png` (normal) or tinted `island_C` (boss).
4. **Total node count** — at most `completedCount + 1 (current) + 5 (locked)`. Object pooling further limits what's rendered to ~2 screen heights around camera.
5. **Design target:** 30 levels in v1.0 launch. The system supports unlimited levels — adding more only requires new `LevelRecord` entries in save data. No code changes needed for level count increases. Volcano asset and Zone 2 theming are reserved for post-level-30 content expansion.

---

### 13. Asset File Structure

```
Assets/Art/Roadmap/
  Islands/
    island_A.png
    island_B.png
    island_C.png          (Boss)
    island_D.png
    island_E.png
    island_locked.png
  Decorations/
    cliff_left.png
    cliff_right.png
    palm_left.png         (reserved — Phase 2+)
    palm_right.png        (reserved — Phase 2+)
    volcano.png           (reserved — Phase 2+)
  Background/
    cloud.png             (PROVIDED — ready for Phase 2)
```

**Unity Import Settings for all sprites:**
- Texture Type: Sprite (2D and UI)
- Pixels Per Unit: 100
- Filter Mode: Bilinear
- Max Size: 1024 (islands), 2048 (cliffs)
- Compression: ASTC 6x6 (Android)

#### 13.1 Sprite Loading Strategy ✅ IMPLEMENTED

Since the project uses programmatic UI (no prefabs, no inspector assignments), sprites are loaded at runtime via `Resources.Load<Sprite>()` — matching the existing project convention.

**Approach:** `RoadmapConfig.LoadSprite(string resourcePath)` with an internal `Dictionary<string, Sprite>` cache. Falls back to `Resources.Load<Texture2D>()` → `Sprite.Create()` if the asset isn't imported as Sprite type.

```csharp
// From RoadmapConfig.cs — already implemented
public static Sprite LoadSprite(string resourcePath) {
    if (_spriteCache.TryGetValue(resourcePath, out var cached) && cached != null)
        return cached;
    var sprite = Resources.Load<Sprite>(resourcePath);
    if (sprite == null) {
        var tex = Resources.Load<Texture2D>(resourcePath);
        if (tex != null)
            sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),
                new Vector2(0.5f,0.5f), 100f);
    }
    if (sprite != null) _spriteCache[resourcePath] = sprite;
    return sprite;
}
```

**Resource paths** (defined as constants in `RoadmapConfig`):
- `"Roadmap/Islands/island_A"` through `"Roadmap/Islands/island_E"`
- `"Roadmap/Islands/island_locked"`

Assets are duplicated from `Assets/Art/Roadmap/` to `Assets/Resources/Roadmap/` for runtime loading.

**Do NOT create a `RoadmapAssets` ScriptableObject** — the project doesn't use ScriptableObjects for sprite references.

---

### 14. Performance Targets

- **Memory:** < 20MB total roadmap textures
- **Draw calls:** < 50 (individual sprites, no atlas — each island PNG is a separate texture. Stones can batch if they share a single runtime-generated texture. Badges batch per WorldSpace Canvas.)
- **Frame rate:** 60fps during scroll
- **Object pooling:** Only create island GameObjects for levels within ~2 screen heights of camera. Recycle when scrolled far away.
- **Off-screen:** Deactivate island GameObjects > 2 screen heights from camera center
- **Stone pooling:** A stepping stone segment between islands N and N+1 should remain active if **either** adjacent island is within the pooling margin. Only deactivate a stone segment when both connected islands are outside the margin.

---

### 15. Implementation Phases

#### Phase 1: Core Map (This Story — Definition of Done)

**Project setup (before coding):**
- [x] Add Unity Layer `Roadmap` in Tags and Sorting Layers > Layers
- [x] Add sorting layers: RoadmapSky, RoadmapStones, RoadmapIslands (see Section 16)
- [x] Exclude `Roadmap` layer from main/gameplay camera culling mask

**Implementation:**
- [x] New `RoadmapMapView` component (SpriteRenderer-based map content)
- [x] New `RoadmapIslandMarker` MonoBehaviour (tap detection, stores level number)
- [x] Camera-based scroll system with touch drag + framerate-independent momentum
- [x] Ocean gradient background (runtime 5-stop gradient texture on fullscreen quad)
- [x] Island node placement (zigzag pattern, world-space SpriteRenderers)
- [x] Island state rendering (completed/current/locked) with correct sprites
- [x] Current level glow (runtime-generated circle sprite, scaled to 1.3x rendered island)
- [x] Level badge with states (solid colors, uGUI WorldSpace Canvas)
- [x] Stars display (gold/gray, runtime-generated star sprite, uGUI on WorldSpace Canvas)
- [x] Stepping stones (runtime-generated circle sprite, tinted ellipses along bezier path)
- [x] Auto-scroll to current level on open
- [x] Close (X) button in header -> back to Hub
- [x] Tap island to play/replay level (`Physics2D.OverlapPoint` + `BoxCollider2D`)
- [x] Object pooling for islands and stone segments outside viewport
- [x] 5 locked levels rendered beyond current level

#### Phase 2: Polish (Separate story or extension)
- [ ] Parallax layers (clouds, cliffs) with drift animation
- [ ] Caustic light effect on ocean
- [ ] Current level glow pulse animation
- [ ] Badge gradient backgrounds, shadows, gloss
- [ ] Badge pulse animation for current level
- [ ] Cloud drift animation
- [ ] Full 5-layer stepping stone rendering

#### Phase 3: Interaction (Separate story)
- [ ] Level tap -> detail popup (replaces direct action)
- [ ] Popup variants (completed/current/locked)
- [ ] Play/Replay button in popup -> scene transition
- [ ] Custom transition animation (water swoosh) from roadmap to gameplay scene (replaces default ScreenManager fade)

## Acceptance Criteria

### Phase 1 (This Story)
- [x] Level 1 appears at bottom, levels increase upward
- [x] 4 island variants rotate with flip for variety
- [x] Boss island (C) appears every 10th level, centered, larger
- [x] Current level is 1.35x scale with purple glow (static, no animation needed yet)
- [x] 5 locked levels rendered beyond current level (foggy island + lock badge)
- [x] Locked stepping stones rendered in dark/muted color
- [x] Locked levels ignore taps (no action)
- [x] Stepping stones follow S-curve bezier between islands (simple ellipses OK)
- [x] Smooth scroll with framerate-independent momentum, bounded to all rendered levels
- [x] Auto-scroll to current level on open
- [x] Stars visible for completed levels (gold earned, gray empty)
- [x] Badge below island, not on top
- [x] Close button returns to Hub
- [x] Tapping completed level replays it; tapping current level starts it (with tap vs scroll guard)
- [x] 60fps on target Android devices

### Phase 2 (Future)
- [ ] Parallax clouds and cliffs
- [ ] Badge gradients, shadows, gloss
- [ ] Glow pulse animation on current level
- [ ] Cloud drift animation
- [ ] Caustic light effect

### Phase 3 (Future)
- [ ] Tap island shows detail popup instead of direct action

## Dev Notes

### 16. Architecture: Camera + SpriteRenderer + uGUI Overlay

This screen uses a different rendering approach from other screens, but **integrates with the existing `ScreenManager` via the same `GameFlowState.Roadmap` registration**.

**How it works:**
1. `RoadmapScreen` remains a MonoBehaviour on a root GameObject registered with `ScreenManager` as `GameFlowState.Roadmap` (same as today).
2. When activated (`OnEnable`), it creates/shows a dedicated **orthographic Camera** (`orthographicSize = 9.6f`, matching half the 1920 reference height at 100 PPU — so the full viewport is 19.2 world units tall, showing ~5 levels at once) for the map content and a **uGUI ScreenSpaceOverlay Canvas** for the header.
3. Map content (islands, stones, ocean) are **SpriteRenderer** GameObjects parented under the roadmap root. The camera scrolls by moving its `transform.position.y`.
4. Badges and stars are **uGUI elements on a WorldSpace Canvas** parented to each island transform, so they move with the islands.
5. When deactivated (`OnDisable` / `SetActive(false)` by ScreenManager), everything hides naturally.
6. The roadmap camera should use a **culling mask** that only renders roadmap layers, to avoid conflict with the gameplay camera.
7. The roadmap root GameObject must include a **CanvasGroup** component for ScreenManager transition compatibility (it uses `CanvasGroup.alpha` for fade animations). Since CanvasGroup alpha does NOT affect SpriteRenderers, also handle fade by enabling/disabling the roadmap camera and SpriteRenderers in `OnEnable`/`OnDisable`. The ScreenManager fade will gracefully handle the overlay Canvas (header), while the map content simply appears/disappears with the camera.

**Setup required — Unity Layer (for culling mask):**
Add a Unity Layer named `Roadmap` in `Edit > Project Settings > Tags and Sorting Layers > Layers` (use any free slot, e.g., Layer 8). Assign ALL roadmap GameObjects (islands, stones, ocean quad, WorldSpace canvases) to this layer. Set the roadmap camera's `cullingMask` to only render the `Roadmap` layer. Set the main/gameplay camera's culling mask to exclude `Roadmap`. This prevents roadmap content from appearing in the gameplay camera and vice versa.

**Setup required — Sorting Layers (for draw order within roadmap):**
The project currently only has the "Default" sorting layer. Add the following sorting layers in `Edit > Project Settings > Tags and Sorting Layers > Sorting Layers` (order matters — listed bottom-to-top):
- `RoadmapSky` (ocean gradient)
- `RoadmapCloudsFar` (Phase 2)
- `RoadmapVolcano` (Phase 2)
- `RoadmapCliffs` (Phase 2)
- `RoadmapCloudsNear` (Phase 2)
- `RoadmapStones` (stepping stones)
- `RoadmapIslands` (island sprites)

Badges/stars use uGUI WorldSpace Canvas (`Canvas.sortingOrder`, not sorting layers). Phase 1 only needs: RoadmapSky, RoadmapStones, RoadmapIslands.

**Null sprite handling:**
If `RoadmapConfig.LoadSprite()` returns null (asset missing), log a warning via `Debug.LogWarning()` and skip rendering that island node. Do NOT crash or throw.

**Why not pure uGUI ScrollRect?**
- Parallax layers require independent scroll speeds — impossible with a single ScrollRect
- SpriteRenderer gives better sprite batching for many small objects (stones)
- Camera-based scrolling is more natural for a "world map" with decorations at varying depths

**Why not UI Toolkit?**
- The project has zero UI Toolkit usage. All screens use uGUI. Introducing UI Toolkit for one screen would be inconsistent and add unnecessary complexity.

### Key Files to CREATE
- `Scripts/Game/UI/Components/RoadmapMapView.cs` — New SpriteRenderer-based map content manager
- `Scripts/Game/UI/Components/RoadmapIslandMarker.cs` — Small MonoBehaviour attached to each island GO for tap detection (stores `LevelNumber`)
- ✅ `Scripts/Game/UI/Components/RoadmapNodeData.cs` — Data class + `RoadmapLevelState` enum (DONE)
- ✅ `Scripts/Game/UI/Components/RoadmapConfig.cs` — Constants, colors, sprite loading, layout calculation (DONE)

### Key Files to MODIFY
- `Scripts/Game/UI/Screens/RoadmapScreen.cs` — Major rewrite: replace LevelListView usage with RoadmapMapView, add camera setup, add header overlay

### Key Files to NOT MODIFY
- `Scripts/Game/UI/Components/LevelListView.cs` — **Shared with `StarGateScreen`**. Do NOT modify or replace.
- `Scripts/Game/UI/Components/LevelNodeData.cs` — **Shared with `StarGateScreen`**. Do NOT modify. Create `RoadmapNodeData` instead.

### Key Files Referenced
- Save data for star counts: `IProgressionManager.GetAllLevelRecords()` returns `List<LevelRecord>`
- `LevelRecord` has: `LevelNumber`, `CityName`, `CountryName`, `Mood`, `Stars`
- Current level: `IProgressionManager.CurrentLevel`
- Level actions: `GameplayManager.StartLevel()`, `GameplayManager.StartReplay()`
- Screen transitions: `ScreenManager.TransitionTo(GameFlowState.MainMenu)` returns to Hub

### Dependencies
- Story 11.3 (TMPro + Nunito font) — text rendering
- Story 11.4 (Hub screen) — roadmap accessed from Hub via `GameFlowState.Roadmap`
- Existing save system for star data
- All Phase 1 island assets are ready in `Assets/Art/Roadmap/Islands/`

### CRITICAL Anti-Patterns
1. **Do NOT modify `LevelListView.cs`** — it is shared with `StarGateScreen`. Create `RoadmapMapView` instead.
2. **Do NOT modify `LevelNodeData.cs`** — it is shared. Create `RoadmapNodeData` instead.
3. **Do NOT break level start/replay functionality** — preserve existing `OnLevelTapped` behavior
4. **Do NOT load all level data upfront** — use object pooling, only render visible islands
5. **Do NOT use prefabs** — maintain programmatic creation pattern
6. **Do NOT change save data format** — read existing `LevelRecord` data as-is
7. **Do NOT introduce UI Toolkit** — use uGUI for all UI elements (consistent with project)
8. **Do NOT reference `bestMoves`, `bestTime`, `coinsEarned`** — these fields don't exist in save data
9. **Do NOT create a `RoadmapAssets` ScriptableObject** — use `RoadmapConfig.LoadSprite()` with `Resources.Load<Sprite>()` (matches project convention)
10. **Do NOT use DOTween or any external tween library** — not installed in project. Use manual lerp with coroutines or Update() for animations

### References
- [Source: _bmad-output/mockups/roadmap_mockup.html] — Interactive HTML mockup (v11)
- [Source: _bmad-output/mockups/roadmap-architecture.excalidraw] — Architecture diagram (ScreenManager + Camera + Layer stack)
- [Source: _bmad-output/mockups/roadmap-island-node.excalidraw] — Island node detail (GO hierarchy, states, data flow)
- [Source: _bmad-output/epics.md#Epic-11] — Roadmap redesign scope
- [Source: _bmad-output/implementation-artifacts/4-2-scrollable-roadmap.md] — Original roadmap implementation

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Debug Log References

### Completion Notes List
- Implemented full Phase 1 roadmap redesign: camera-based SpriteRenderer map with islands, stepping stones, badges, stars, ocean gradient, glow, scroll, tap, and object pooling
- Created RoadmapMapView.cs as the core map content manager with runtime texture generation (ocean gradient, circle for stones/glow, star polygon)
- Created RoadmapIslandMarker.cs for tap detection via Physics2D.OverlapPoint + BoxCollider2D
- Rewrote RoadmapScreen.cs: dedicated orthographic camera (orthoSize 9.6), touch drag scroll with framerate-independent momentum, auto-scroll to current level, tap vs scroll guard, header overlay with title/star counter/close button
- Added Roadmap layer (slot 8) and sorting layers (RoadmapSky, RoadmapStones, RoadmapIslands) to TagManager.asset
- Main camera culling mask excludes Roadmap layer (set in Start())
- All runtime textures properly cleaned up in OnDestroy to prevent memory leaks
- Island variant rotation follows A→B→D→E pattern with flip every other cycle, boss (C) every 10th level centered
- Bezier S-curve stepping stones with deterministic random size variation (System.Random, not UnityEngine.Random)
- WorldSpace Canvas per island for badges/stars, ScreenSpaceOverlay Canvas for header
- 5 locked levels rendered beyond current level with foggy sprites and lock badges

### File List
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/RoadmapMapView.cs` — NEW: SpriteRenderer-based map content manager
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/RoadmapIslandMarker.cs` — NEW: MonoBehaviour for island tap detection
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/RoadmapScreen.cs` — MODIFIED: Full rewrite with camera, scroll, header overlay
- `src/JuiceSort/ProjectSettings/TagManager.asset` — MODIFIED: Added Roadmap layer and 3 sorting layers

### Change Log
- 2026-04-02: Phase 1 implementation — full roadmap redesign with camera-based map, islands, stones, badges, stars, scroll, tap, header overlay
