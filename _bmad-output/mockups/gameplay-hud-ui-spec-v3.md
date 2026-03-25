# Gameplay HUD — UI Specification v3

**Screen:** Gameplay HUD (overlay during puzzle play)
**Source of Truth:** [gameplay-hud-mockup.html](gameplay-hud-mockup.html)
**Conversion:** HTML renders at 360px width = 1080px Unity reference. **All values = CSS × 3.**

---

## 1. Canvas & Scaler

| Property | Value |
|---|---|
| Canvas render mode | `RenderMode.ScreenSpaceOverlay` |
| CanvasScaler mode | `ScaleMode.ScaleWithScreenSize` |
| Reference resolution | `1080 × 1920` |
| Match width or height | `0.5` |
| Pixels per unit | `100` |

---

## 2. GameObject Hierarchy

```
GameplayHUD                              RectTransform (stretch-all)
├── OutsideTapBlocker                    RectTransform (stretch-all) — hidden
│   └── [Image (transparent) + Button]
├── SafeArea                             RectTransform (Screen.safeArea)
│   ├── TopBar                           RectTransform (stretch-top, h=198)
│   │   ├── CoinDisplay                  (HLayoutGroup + ContentSizeFitter)
│   │   │   ├── CoinGlow                 Image (108×108, glow pulse)
│   │   │   ├── CoinIcon                 Image (84×84, gradient circle + border ring)
│   │   │   │   └── CoinSymbol           TMPro ("$", 33px)
│   │   │   └── CoinText                 TMPro ("1,250", 51px bold gold)
│   │   ├── LevelPill                    (VLayoutGroup + ContentSizeFitter)
│   │   │   ├── LevelText                TMPro ("Level 10", 54px bold)
│   │   │   └── MoveText                 TMPro ("12 moves", 36px medium)
│   │   └── SettingsGear                 RectTransform (138×138, right)
│   │       ├── [Image bg + border ring + Button]
│   │       └── GearIcon                 TMPro ("⚙", 66px)
│   ├── SettingsPanel                    (VLayoutGroup + ContentSizeFitter) — hidden
│   │   ├── MusicToggle                  [3D Button: 150×162]
│   │   ├── SFXToggle                    [3D Button]
│   │   ├── VibrationToggle             [3D Button]
│   │   ├── RestartButton               [3D Button]
│   │   └── ExitButton                  [3D Button]
│   └── BottomBar                        RectTransform (stretch-bottom, h=198)
│       └── ActionPill                   (HLayoutGroup + ContentSizeFitter)
│           ├── UndoButton               (HLayoutGroup, fixed 210×114)
│           │   ├── UndoIcon             TMPro ("↶", 54px white)
│           │   ├── MiniCoin             Image (48×48) + "$" child
│           │   └── CostText             TMPro ("100", 39px bold gold)
│           └── ExtraBottleButton        (HLayoutGroup, fixed 240×114)
│               ├── BottleIcon           Image (36×78 body + 21×12 neck)
│               ├── PlusIcon             TMPro ("+", 54px white)
│               ├── MiniCoin             Image (48×48) + "$" child
│               └── CostText             TMPro ("500", 39px bold gold)
```

---

## 3. RectTransform & Visual Properties

<!-- Fixed: Issue 6 — Single border strategy defined upfront -->
### 3.BORDERS — Border Strategy

**All borders use a "border ring" child Image** placed behind the fill Image. This is the only border approach used in this HUD:

```
BorderRing (child, sibling index 0 — behind Fill)
├── Image: WhiteRoundedRect (same radius as fill, slightly larger)
│   sprite = UIShapeUtils.WhiteRoundedRect(radius, size)
│   type = Sliced
│   color = border color
│   raycastTarget = false
├── RectTransform: same anchor as fill, but sizeDelta += (borderWidth*2, borderWidth*2)
```

The fill Image sits on top, `borderWidth` pixels smaller on each side, creating the visual border.

**Do NOT use Unity's `Outline` component** — it produces blocky results on rounded shapes.

| Element | Border Width | Border Color | Sprite Radius |
|---|---|---|---|
| CoinDisplay | 4.5 | Night: `rgba(255,180,40,0.35)` / Morning: `rgba(180,130,30,0.4)` | 20 |
| CoinIcon | 6 | `#CC8800` | circle |
| MiniCoin | 4.5 | `#CC8800` | circle |
| LevelPill | 3 | Night: `rgba(255,255,255,0.08)` / Morning: `rgba(255,255,255,0.1)` | 18 |
| SettingsGear | 4.5 | `rgba(255,255,255,0.1)` | 14 |
| ActionPill | 3 | Night: `rgba(255,255,255,0.08)` / Morning: `rgba(255,255,255,0.1)` | 22 |
| UndoButton | 3 | `rgba(255,255,255,0.08)` | 16 |
| ExtraBottleButton | 3 | `rgba(255,255,255,0.08)` | 16 |

**Sprite radius values are in sprite-texture pixels, NOT Unity reference pixels.** Do not multiply by ×3.

---

### 3.0 Root: `GameplayHUD`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin / offsetMax | `(0, 0)` |

### 3.0b `OutsideTapBlocker`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin / offsetMax | `(0, 0)` |
| sibling index | `0` (behind everything) |
| default active | `false` |

| Visual | Value |
|---|---|
| Image color | `rgba(0,0,0,0)` — fully transparent |
| Image raycastTarget | `true` |
| Button onClick | `CollapseSettingsIfOpen()` |

### 3.1 `SafeArea`

| Property | Value |
|---|---|
| anchorMin | `(safeArea.x / screenW, safeArea.y / screenH)` |
| anchorMax | `(safeArea.xMax / screenW, safeArea.yMax / screenH)` |
| offsetMin / offsetMax | `(0, 0)` |
| Update | Recalculated in `LateUpdate()` when `Screen.safeArea` changes |

<!-- Fixed: Issue 3 — TopBar height recalculated -->
### 3.2 `TopBar`

CSS: `.top-bar { padding: 52px 18px 8px 18px }` — the 52px includes phone status bar. SafeArea already handles status bar offset, so TopBar only needs content-area height.

Effective content area: topMargin(12px) + tallestElement(46px gear) + bottomMargin(8px) = 66px CSS × 3 = **198px Unity**

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 198)` |
| anchoredPosition | `(0, 0)` |
| Layout | **No LayoutGroup** — TopBar uses `justify-content: space-between` which maps to manual left/center/right anchoring |

---

<!-- Fixed: Issue 1 + Issue 2 + Issue 5 — CoinDisplay concrete size + LayoutGroup -->
### 3.3 `CoinDisplay`

CSS: `.coin-display { display: flex; align-items: center; gap: 7px; padding: 6px 14px 6px 8px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0.5)` | left-aligned in top bar |
| anchorMax | `(0, 0.5)` | |
| pivot | `(0, 0.5)` | |
| anchoredPosition | `(54, 0)` | bar margin 18px × 3 |

| Component | Value | CSS Source |
|---|---|---|
| **HorizontalLayoutGroup** | | `display: flex` |
| childAlignment | `MiddleLeft` | `align-items: center` |
| spacing | `21` | `gap: 7px` × 3 |
| padding L/R/T/B | `24, 42, 18, 18` | `padding: 6px 14px 6px 8px` × 3 |
| childForceExpandWidth | `false` | |
| childForceExpandHeight | `false` | |
| childControlWidth | `true` | |
| childControlHeight | `true` | |
| **ContentSizeFitter** | | pill wraps content |
| horizontalFit | `PreferredSize` | |
| verticalFit | `PreferredSize` | |

**Resulting size:** ~`(24 + 84 + 21 + 120 + 42, 18 + 84 + 18)` = **~(291, 120)** — calculated automatically by layout.

| Visual | Value | CSS Source |
|---|---|---|
| Fill Image sprite | `UIShapeUtils.WhiteRoundedRect(20, 64)` + `Sliced` | `border-radius: 20px` |
| Fill Image color | `rgba(0,0,0,0.4)` | `background: rgba(0,0,0,0.4)` |
| Border ring | See §3.BORDERS — 4.5px, mood-colored | `border: 1.5px solid` × 3 |
| Outer glow | Glow child Image, +30px each side, `rgba(255,200,50,0.1)`, `UIShapeUtils.Glow(128, white)` | `box-shadow: 0 0 10px` |
| Inner highlight | Highlight child, stretch-top, height 3px, `rgba(255,255,255,0.08)` | `inset 0 1px 0` × 3 |

### 3.3a `CoinGlow` (child of CoinDisplay, behind CoinIcon)

**Note:** CoinGlow is NOT a LayoutGroup child. Place it as first child with `LayoutElement.ignoreLayout = true` and position manually via anchors.

| Property | Value |
|---|---|
| anchorMin | `(0, 0.5)` |
| anchorMax | `(0, 0.5)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(66, 0)` — centered on coin icon (padding 24 + half coin 42) |
| sizeDelta | `(108, 108)` — coin 84 + glow spread 24 |
| LayoutElement | `ignoreLayout = true` |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteCircle(64)` | circular glow |
| Image color | animated — see §7.1 | `box-shadow: 0 0 8px rgba(255,200,50,0.35)` |
| raycastTarget | `false` | |

### 3.3b `CoinIcon` (child of CoinDisplay — LayoutGroup child #1)

CSS: `.coin-icon { width: 28px; height: 28px }` → **84 × 84**

| LayoutElement | Value |
|---|---|
| preferredWidth | `84` |
| preferredHeight | `84` |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | Procedural radial gradient circle (§6.1) OR `WhiteCircle(64)` tinted `#F5A623` | `radial-gradient(circle at 38% 32%, #ffe866, #f5a623)` |
| Border ring | See §3.BORDERS — 6px `#CC8800`, circle behind coin | `border: 2px solid #cc8800` × 3 |
| raycastTarget | `false` | |

**Ideal:** Baked 128×128 radial gradient texture. Center `#FFE866` at offset (38%, 32%), edge `#F5A623`. Set `Image.color = Color.white`.
**Fallback:** `UIShapeUtils.WhiteCircle(64)` tinted `#F5A623`.

### 3.3c `CoinSymbol` (child of CoinIcon)

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin / offsetMax | `(0, 0)` |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"$"` | coin emblem |
| fontSize | `33` | 11px × 3 |
| fontStyle | `Bold` | weight 800 |
| alignment | `Center` | |
| color | `#8B6914` → `(0.545, 0.412, 0.078, 1)` | |
| font | `Nunito-Regular SDF` | |
| raycastTarget | `false` | |

### 3.3d `CoinText` (child of CoinDisplay — LayoutGroup child #2)

CSS: `.coin-amount { font-size: 17px; font-weight: 700; color: #ffe066 }`

| LayoutElement | Value |
|---|---|
| flexibleWidth | `1` — takes remaining horizontal space |
| minWidth | `60` | |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"0"` | updated by `UpdateCoinDisplay()` |
| fontSize | `51` | 17px × 3 |
| fontStyle | `Bold` | weight 700 |
| alignment | `MidlineLeft` | |
| color | `#FFE066` → `(1.0, 0.878, 0.4, 1)` | |
| font | `Nunito-Regular SDF` | |
| **Effect:** text-shadow | TMPro Underlay: color `rgba(255,220,50,0.2)`, offset (0,0), dilate 0.3, softness 0.5 | `text-shadow: 0 0 6px` |

---

<!-- Fixed: Issue 1 + Issue 2 — LevelPill concrete + LayoutGroup -->
### 3.4 `LevelPill`

CSS: `.level-pill { left: 58%; padding: 6px 22px; border-radius: 18px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0.58, 0.5)` | `left: 58%; transform: translateX(-50%)` |
| anchorMax | `(0.58, 0.5)` | |
| pivot | `(0.5, 0.5)` | |
| anchoredPosition | `(0, 0)` | |

| Component | Value | CSS Source |
|---|---|---|
| **VerticalLayoutGroup** | | text stacked vertically |
| childAlignment | `MiddleCenter` | `text-align: center` |
| spacing | `3` | `margin-top: 1px` × 3 |
| padding L/R/T/B | `66, 66, 18, 18` | `padding: 6px 22px` × 3 |
| childForceExpandWidth | `true` | text stretches to pill width |
| childForceExpandHeight | `false` | |
| childControlWidth | `true` | |
| childControlHeight | `true` | |
| **ContentSizeFitter** | | |
| horizontalFit | `PreferredSize` | |
| verticalFit | `PreferredSize` | |

**Resulting size:** ~`(66 + 200 + 66, 18 + 54 + 3 + 36 + 18)` = **~(332, 129)** — auto-calculated.

| Visual | Value | CSS Source |
|---|---|---|
| Fill Image sprite | `UIShapeUtils.WhiteRoundedRect(18, 64)` + `Sliced` | `border-radius: 18px` |
| Fill Image color Night | `rgba(0,0,0,0.45)` | `.night .level-pill` |
| Fill Image color Morning | `rgba(60,40,10,0.35)` | `.morning .level-pill` |
| Border ring | See §3.BORDERS — 3px, mood-colored | |
| Drop shadow | Shadow child (ignoreLayout), offset (0, -6px), `rgba(0,0,0,0.2)`, `UIShapeUtils.Glow(128, white, 0.3f)` | `box-shadow: 0 2px 8px` × 3 |

### 3.4a `LevelText` (LayoutGroup child #1)

CSS: `.level-text { font-size: 18px; font-weight: 700 }`

| LayoutElement | Value |
|---|---|
| preferredHeight | `54` — fontSize as min height |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"Level 1"` | set by `SetLevelInfo()` |
| fontSize | `54` | 18px × 3 |
| fontStyle | `Bold` | weight 700 |
| alignment | `Center` | |
| color | `rgba(255,245,230,0.95)` → `(1.0, 0.961, 0.902, 0.95)` | Night |
| font | `Nunito-Regular SDF` | |
| **Effect:** text-shadow | TMPro Underlay: color `rgba(0,0,0,0.3)`, offsetY=3, softness 0.4 | `text-shadow: 0 1px 4px` |

### 3.4b `MoveText` (LayoutGroup child #2)

CSS: `.move-text { font-size: 12px; font-weight: 500 }`

| LayoutElement | Value |
|---|---|
| preferredHeight | `36` |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `""` | set by `UpdateDisplay()` |
| fontSize | `36` | 12px × 3 |
| fontStyle | `Normal` | weight 500 → Normal (no medium available) |
| alignment | `Center` | |
| color Night | `rgba(180,170,200,0.7)` → `(0.706, 0.667, 0.784, 0.7)` | |
| color Morning | `rgba(120,95,55,0.8)` → `(0.471, 0.373, 0.216, 0.8)` | |
| font | `Nunito-Regular SDF` | |

---

### 3.5 `SettingsGear`

CSS: `.settings-btn { width: 46px; height: 46px; border-radius: 14px; font-size: 22px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(1, 0.5)` | right-aligned |
| anchorMax | `(1, 0.5)` | |
| pivot | `(1, 0.5)` | |
| anchoredPosition | `(-54, 0)` | margin 18px × 3 |
| sizeDelta | `(138, 138)` | 46px × 3 |

| Visual | Value | CSS Source |
|---|---|---|
| Fill Image sprite | `UIShapeUtils.WhiteRoundedRect(14, 48)` + `Sliced` | `border-radius: 14px` |
| Fill color Night | Flat fallback: `rgba(75,55,135,0.65)` | `linear-gradient(135deg, rgba(90,70,150,0.7), rgba(60,40,120,0.6))` mid |
| Fill color Morning | Flat fallback: `rgba(105,88,50,0.65)` | `linear-gradient(135deg, rgba(120,100,60,0.7), rgba(90,75,40,0.6))` mid |
| **Ideal:** gradient | `ThemeConfig.CreateGradientSprite(topLeft, bottomRight)` | 135deg |
| Border ring | See §3.BORDERS — 4.5px, `rgba(255,255,255,0.1)` | |
| Glow child | +24px each side, `rgba(140,100,220,0.15)` | `box-shadow: 0 0 8px` × 3 |
| Highlight child | Top 3px, `rgba(255,255,255,0.1)` | `inset 0 1px 0` × 3 |

| Gear Icon TMPro | Value | CSS Source |
|---|---|---|
| text | `"\u2699"` (⚙) | `&#9881;` |
| fontSize | `66` | 22px × 3 |
| color | `rgba(255,255,255,0.85)` | |
| alignment | `Center` | |

---

<!-- Fixed: Issue 2 + Issue 4 — SettingsPanel LayoutGroup + position recalculated -->
### 3.6 `SettingsPanel`

CSS: `.settings-panel { top: 106px; right: 18px; gap: 10px }`

In CSS, `top: 106px` equals the TopBar total height (52+46+8=106). In Unity, SafeArea handles status bar, so panel starts at TopBar bottom edge.

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(1, 1)` | top-right |
| anchorMax | `(1, 1)` | |
| pivot | `(1, 1)` | |
| anchoredPosition | `(-54, -198)` | right: 18×3=54, Y: TopBarHeight(198) |
| default active | `false` | |
| Fill Image | **NONE** — no background | |

| Component | Value | CSS Source |
|---|---|---|
| **VerticalLayoutGroup** | | `flex-direction: column` |
| childAlignment | `MiddleCenter` | centered buttons |
| spacing | `30` | `gap: 10px` × 3 |
| padding L/R/T/B | `0, 0, 0, 0` | no panel padding |
| childForceExpandWidth | `false` | |
| childForceExpandHeight | `false` | |
| **ContentSizeFitter** | | panel wraps button stack |
| horizontalFit | `PreferredSize` | |
| verticalFit | `PreferredSize` | |

**Resulting size:** ~`(150, 5×162 + 4×30)` = **~(150, 930)** — auto-calculated.

### 3.6a Each 3D Settings Button

CSS: `.s-btn { width: 50px; height: 50px; border-radius: 14px }`

Each button = Container (150 × 162) holding Shadow + Face + Highlight + Icon.

| LayoutElement (on Container) | Value |
|---|---|
| preferredWidth | `150` |
| preferredHeight | `162` — face(150) + shadow depth(12) |

| Button | Face Color (ON) | Shadow Color |
|---|---|---|
| MusicToggle | Green gradient | `#069920` |
| SFXToggle | Green gradient | `#069920` |
| VibrationToggle | Green gradient | `#069920` |
| RestartButton | Green gradient | `#069920` |
| ExitButton | Red gradient | `#990505` |

**Container RectTransform:**

| Property | Value |
|---|---|
| sizeDelta | `(150, 162)` |

**Shadow (child, sibling 0):**

| Property | Value |
|---|---|
| anchorMin / anchorMax | `(0,0)` / `(1,0)` |
| pivot | `(0.5, 0)` |
| sizeDelta | `(0, 150)` |
| Image sprite | `UIShapeUtils.WhiteRoundedRect(14, 48)` + `Sliced` |
| Image color | per-button shadow color |
| raycastTarget | `false` |

**Face (child, sibling 1):**

| Property | Value |
|---|---|
| anchorMin / anchorMax | `(0,1)` / `(1,1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 150)` |
| Image sprite | `UIShapeUtils.WhiteRoundedRect(14, 48)` + `Sliced` |
| Button | yes |
| ButtonBounce | yes |

**Face color states:**

| State | CSS Gradient | Flat Fallback |
|---|---|---|
| ON (green) | `#44FF66 → #22DD44 → #10CC30 → #08B825` (180deg) | `#22DD44` |
| OFF (gray) | `#7A7A7A → #5E5E5E → #484848` (180deg) | `#5E5E5E` |
| Exit (red) | `#FF4444 → #EE2020 → #DD1010 → #CC0505` (180deg) | `#EE2020` |

**Ideal:** `ThemeConfig.CreateGradientSprite(top, bottom)` per state.
**Fallback:** Flat color.

**Highlight (child of Face):**

| Property | Value |
|---|---|
| anchorMin / anchorMax | `(0,1)` / `(1,1)` |
| sizeDelta | `(0, 6)` |
| Image color (ON) | `rgba(255,255,255,0.4)` |
| Image color (OFF) | `rgba(255,255,255,0.1)` |
| Image color (Exit) | `rgba(255,255,255,0.3)` |
| raycastTarget | `false` |

**Glow (ON/Exit only — child of Container, sibling 0, behind Shadow):**

| Property | Value |
|---|---|
| sizeDelta | `(210, 210)` |
| Image sprite | `UIShapeUtils.Glow(128, Color.white, 0.5f)` tinted |
| color (ON) | `rgba(40,255,80,0.2)` |
| color (Exit) | `rgba(255,50,50,0.2)` |

**Icon TMPro (child of Face):**

| TMPro | Value |
|---|---|
| fontSize | `72` |
| alignment | `Center` |
| color (ON) | `#FFFFFF` |
| color (OFF) | `rgba(255,255,255,0.4)` |
| font | `Nunito-Regular SDF` (Unicode fallback) |

**Icon mapping:**

| Button | ON | OFF | Ideal (Material Icons) | Fallback (Unicode) |
|---|---|---|---|---|
| Music | `music_note` | `music_off` | Material Icons SDF | `\u266B` (♫) |
| SFX | `volume_up` | `volume_off` | Material Icons SDF | `\u266A` / `\u2022` |
| Vibration | `vibration` | `mobile_off` | Material Icons SDF | `\u2058` / `\u2022` |
| Restart | `replay` | — | Material Icons SDF | `\u21BB` (↻) |
| Exit | `exit_to_app` | — | Material Icons SDF | `\u2192` (→) |

**Ideal:** Import Material Icons Round TTF → generate TMP SDF font asset → use as icon font.
**Fallback:** Unicode glyphs in Nunito (current approach).

---

<!-- Fixed: Issue 1 — BottomBar height recalculated -->
### 3.7 `BottomBar`

CSS: `.bottom-bar { padding: 0 18px 28px 18px }`

BottomBar needs: ActionPill (~162px) + bottom padding (84px) = **246px minimum**. Using 198px to match TopBar symmetry — ActionPill can overflow upward slightly via center anchoring.

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0)` |
| pivot | `(0.5, 0)` |
| sizeDelta | `(0, 198)` |
| anchoredPosition | `(0, 0)` |

<!-- Fixed: Issue 1 + Issue 2 — ActionPill LayoutGroup + concrete size -->
### 3.7a `ActionPill`

CSS: `.action-pill { display: flex; align-items: center; gap: 10px; padding: 8px 10px; border-radius: 22px }`

| Property | Value |
|---|---|
| anchorMin | `(0.5, 0.5)` |
| anchorMax | `(0.5, 0.5)` |
| pivot | `(0.5, 0.5)` |

| Component | Value | CSS Source |
|---|---|---|
| **HorizontalLayoutGroup** | | `display: flex` |
| childAlignment | `MiddleCenter` | `align-items: center` |
| spacing | `30` | `gap: 10px` × 3 |
| padding L/R/T/B | `30, 30, 24, 24` | `padding: 8px 10px` × 3 |
| childForceExpandWidth | `false` | |
| childForceExpandHeight | `false` | |
| childControlWidth | `true` | |
| childControlHeight | `true` | |
| **ContentSizeFitter** | | pill wraps buttons |
| horizontalFit | `PreferredSize` | |
| verticalFit | `PreferredSize` | |

**Resulting size:** ~`(30 + 210 + 30 + 240 + 30, 24 + 114 + 24)` = **~(540, 162)** — auto-calculated.

| Visual | Value | CSS Source |
|---|---|---|
| Fill Image sprite | `UIShapeUtils.WhiteRoundedRect(22, 64)` + `Sliced` | `border-radius: 22px` |
| Fill Image color Night | `rgba(0,0,0,0.5)` | `.night .action-pill` |
| Fill Image color Morning | `rgba(60,40,10,0.4)` | `.morning .action-pill` |
| Border ring | See §3.BORDERS — 3px, mood-colored | |
| Drop shadow | Shadow child (ignoreLayout), offset (0,-12), `rgba(0,0,0,0.3)` | `box-shadow: 0 4px 16px` × 3 |

---

<!-- Fixed: Issue 1 + Issue 2 — UndoButton LayoutGroup + fixed size -->
### 3.7b `UndoButton`

CSS: `.action-btn { display: flex; align-items: center; gap: 5px; padding: 10px 18px }`
CSS: `.action-btn.undo { background: linear-gradient(135deg, rgba(90,75,130,0.8), rgba(70,55,110,0.7)) }`

| LayoutElement (in ActionPill) | Value |
|---|---|
| preferredWidth | `210` — icon(54) + gap(15) + coin(48) + gap(15) + cost(~60) + padding(~18) |
| preferredHeight | `114` — padding(30) + content(54) + padding(30) |

| Component | Value | CSS Source |
|---|---|---|
| **HorizontalLayoutGroup** | | `display: flex` |
| childAlignment | `MiddleCenter` | `align-items: center` |
| spacing | `15` | `gap: 5px` × 3 |
| padding L/R/T/B | `30, 30, 30, 30` | `padding: 10px 18px` × 3 (averaged) |
| childForceExpandWidth | `false` | |
| childForceExpandHeight | `false` | |

| Visual | Value | CSS Source |
|---|---|---|
| Fill Image sprite | `UIShapeUtils.WhiteRoundedRect(16, 48)` + `Sliced` | `border-radius: 16px` |
| Fill Image color | Gradient fallback: `rgba(80,65,120,0.75)` | mid-blend |
| **Ideal:** gradient | `rgba(90,75,130,0.8)` → `rgba(70,55,110,0.7)` 135deg | |
| Border ring | See §3.BORDERS — 3px, `rgba(255,255,255,0.08)` | |
| Glow child | +24px, `rgba(140,100,220,0.15)`, ignoreLayout | |
| Button transition | `Selectable.Transition.None` | CanvasGroup handles disabled |
| ButtonBounce | yes | |

**Children (LayoutGroup order):**

**UndoIcon (child #1):**

| LayoutElement | `preferredWidth=54, preferredHeight=54` |
|---|---|
| TMPro text | `"\u21B6"` (↶) |
| fontSize | `54` |
| color | `#FFFFFF` |
| alignment | `Center` |

**MiniCoin (child #2):**

CSS: `.coin-icon-small { width: 16px; height: 16px }` → **48 × 48**

| LayoutElement | `preferredWidth=48, preferredHeight=48` |
|---|---|
| Image | Radial gradient circle OR `WhiteCircle(64)` tinted `#F5A623` |
| Border ring | 4.5px `#CC8800` (see §3.BORDERS) |
| "$" child TMPro | fontSize `21`, color `#8B6914`, Bold, Center |

**CostText (child #3):**

| LayoutElement | `flexibleWidth=1, minWidth=30` |
|---|---|
| TMPro text | `"100"` — updated by `UpdateUndoState()` |
| fontSize | `39` |
| fontStyle | `Bold` |
| color | `rgba(255,220,100,0.9)` → `(1.0, 0.863, 0.392, 0.9)` |
| alignment | `MidlineLeft` |

---

<!-- Fixed: Issue 1 + Issue 2 — ExtraBottleButton LayoutGroup + fixed size -->
### 3.7c `ExtraBottleButton`

CSS: `.action-btn.extra { background: linear-gradient(135deg, rgba(40,120,60,0.8), rgba(30,100,50,0.7)) }`

| LayoutElement (in ActionPill) | Value |
|---|---|
| preferredWidth | `240` — bottle(36) + gap + plus(54) + gap + coin(48) + gap + cost(~60) + padding |
| preferredHeight | `114` |

| Component | Value |
|---|---|
| **HorizontalLayoutGroup** | same settings as UndoButton |
| spacing | `15` |
| padding L/R/T/B | `30, 30, 30, 30` |

| Visual | Value | CSS Source |
|---|---|---|
| Fill Image sprite | `UIShapeUtils.WhiteRoundedRect(16, 48)` + `Sliced` | |
| Fill Image color | Gradient fallback: `rgba(35,110,55,0.75)` | mid-blend |
| **Ideal:** gradient | `rgba(40,120,60,0.8)` → `rgba(30,100,50,0.7)` 135deg | |
| Border ring | See §3.BORDERS — 3px, `rgba(255,255,255,0.08)` | |
| Glow child | +24px, `rgba(80,200,100,0.15)`, ignoreLayout | |

**Children (LayoutGroup order):**

**BottleIcon (child #1):**

CSS: `.bottle-icon { width: 12px; height: 26px; border: 1.5px solid rgba(255,255,255,0.7) }`

| LayoutElement | `preferredWidth=36, preferredHeight=78` |
|---|---|
| Body Image sprite | `UIShapeUtils.WhiteRoundedRect(6, 32)` + `Sliced` |
| Body Image color | `rgba(255,255,255,0.7)` |
| Neck child | sizeDelta `(21, 12)`, anchored above body top, offset `(0, 6)` |
| Neck Image sprite | `UIShapeUtils.WhiteRoundedRect(3, 16)` + `Sliced` |
| Neck Image color | `rgba(255,255,255,0.7)` |

**PlusIcon (child #2):**

| LayoutElement | `preferredWidth=54, preferredHeight=54` |
|---|---|
| TMPro text | `"+"` |
| fontSize | `54` |
| color | `#FFFFFF` |

**MiniCoin (child #3):** Same as UndoButton MiniCoin — 48×48, gradient circle, "$", border ring.

**CostText (child #4):**

| LayoutElement | `flexibleWidth=1, minWidth=30` |
|---|---|
| TMPro text | `"500"` — updated by `UpdateExtraBottleState()` |
| fontSize | `39` |
| fontStyle | `Bold` |
| color | `rgba(255,220,100,0.9)` |
| alignment | `MidlineLeft` |

---

## 4. Color Reference

### 4.1 Constant Colors

| Name | Hex | RGBA Float | Usage |
|---|---|---|---|
| CoinGradientCenter | `#FFE866` | `(1.0, 0.91, 0.40)` | Coin radial gradient center |
| CoinGradientEdge | `#F5A623` | `(0.96, 0.65, 0.14)` | Coin radial gradient edge |
| CoinBorderDefault | `rgba(255,200,50,0.4)` | `(1.0, 0.784, 0.196, 0.4)` | Coin display border (base, non-themed) |
| CoinBorder | `#CC8800` | `(0.80, 0.53, 0.0)` | Coin circle border |
| CoinSymbol | `#8B6914` | `(0.545, 0.412, 0.078)` | "$" text inside coin |
| CoinAmountGold | `#FFE066` | `(1.0, 0.878, 0.40)` | Coin balance text |
| CostGold | — | `(1.0, 0.863, 0.392, 0.9)` | Action button cost numbers |
| GreenON | `#22DD44` | `(0.133, 0.867, 0.267)` | Settings ON face (flat) |
| GreenON Shadow | `#069920` | `(0.024, 0.6, 0.125)` | Settings ON shadow |
| GrayOFF | `#5E5E5E` | `(0.369, 0.369, 0.369)` | Settings OFF face (flat) |
| GrayOFF Shadow | `#333333` | `(0.2, 0.2, 0.2)` | Settings OFF shadow |
| RedExit | `#EE2020` | `(0.933, 0.125, 0.125)` | Exit face (flat) |
| RedExit Shadow | `#990505` | `(0.6, 0.02, 0.02)` | Exit shadow |
| HighlightON | — | `(1,1,1,0.4)` | Green button highlight |
| HighlightOFF | — | `(1,1,1,0.1)` | Gray button highlight |
| HighlightExit | — | `(1,1,1,0.3)` | Red button highlight |
| IconON | `#FFFFFF` | `(1,1,1,1)` | Icon enabled |
| IconOFF | — | `(1,1,1,0.4)` | Icon disabled |

### 4.2 Mood-Specific Colors

| Element | Night | Morning |
|---|---|---|
| Coin pill border | `rgba(255,180,40,0.35)` | `rgba(180,130,30,0.4)` |
| Level pill bg | `rgba(0,0,0,0.45)` | `rgba(60,40,10,0.35)` |
| Level pill border | `rgba(255,255,255,0.08)` | `rgba(255,255,255,0.1)` |
| Gear bg | `rgba(90,70,150,0.7)` → `rgba(60,40,120,0.6)` | `rgba(120,100,60,0.7)` → `rgba(90,75,40,0.6)` |
| Move text | `rgba(180,170,200,0.7)` | `rgba(120,95,55,0.8)` |
| Action pill bg | `rgba(0,0,0,0.5)` | `rgba(60,40,10,0.4)` |
| Action pill border | `rgba(255,255,255,0.08)` | `rgba(255,255,255,0.1)` |
| Level text | `rgba(255,245,230,0.95)` | ThemeConfig.TextPrimary |

---

## 5. Font Specifications

| Element | CSS px | Unity (×3) | Weight | TMPro Style | Color |
|---|---|---|---|---|---|
| Coin "$" | 11 | **33** | 800 | Bold | `#8B6914` |
| Coin amount | 17 | **51** | 700 | Bold | `#FFE066` |
| Level text | 18 | **54** | 700 | Bold | `rgba(255,245,230,0.95)` |
| Move counter | 12 | **36** | 500 | Normal | `rgba(180,170,200,0.7)` |
| Gear icon | 22 | **66** | — | Normal | `rgba(255,255,255,0.85)` |
| Settings icons | 24 | **72** | — | Normal | `#FFFFFF` / 40% |
| Action icon (↶, +) | 18 | **54** | — | Normal | `#FFFFFF` |
| Cost number | 13 | **39** | 600 | Bold | `rgba(255,220,100,0.9)` |
| Mini coin "$" | 7 | **21** | 800 | Bold | `#8B6914` |

**Font:** Mockup = Fredoka. Project = **Nunito-Regular SDF** (similar rounded sans-serif).
**Bold** via `fontStyle = FontStyles.Bold`.

---

## 6. Sprite Specifications

**Sprite radius values are in sprite-texture pixels, NOT Unity reference pixels.** Do not multiply ×3.

### 6.1 Coin Radial Gradient

| Property | Value |
|---|---|
| Texture size | 128×128 (downscaled when rendered at 84×84 and 48×48) |
| Type | Radial gradient, center offset to (38%, 32%) |
| Center color | `#FFE866` |
| Edge color | `#F5A623` |
| Cache | Static sprite, created once |
| Usage | CoinIcon (84×84), MiniCoin (48×48) |

### 6.2 Rounded Rectangle 9-Slice Sprites (via UIShapeUtils)

| Radius | Size | Used By |
|---|---|---|
| 22 | 64×64 | ActionPill |
| 20 | 64×64 | CoinDisplay |
| 18 | 64×64 | LevelPill |
| 16 | 48×48 | UndoButton, ExtraBottleButton |
| 14 | 48×48 | SettingsGear, 3D button Face/Shadow |
| 6 | 32×32 | Bottle body |
| 3 | 16×16 | Bottle neck |

### 6.3 Circle Sprite — `UIShapeUtils.WhiteCircle(64)`

Used by: CoinGlow, CoinIcon (fallback), MiniCoin, border rings for circles.

### 6.4 Glow Sprite — `UIShapeUtils.Glow(128, Color.white, 0.5f)`

Used by: Settings button outer glow (ON/Exit), CoinDisplay outer glow, drop shadows.

---

## 7. Animation Specifications

### 7.1 Coin Icon Pulse

CSS: `@keyframes coinPulse { 0%,100%: box-shadow 6px α=0.25; 50%: 14px α=0.55 }`

| Property | Value |
|---|---|
| Target | `_coinGlowImage` (108×108 behind coin icon) |
| Color | `ThemeConfig.GetColor(ThemeColorType.StarGold)` (refreshed each tick) |
| Alpha range | `0.15` ↔ `0.55` |
| Size range | `90` ↔ `126` (simulates box-shadow spread 6px↔14px × 3, optional) |
| Period | `2.5s` |
| Easing | `Mathf.SmoothStep(0.15f, 0.55f, Mathf.PingPong(elapsed / 1.25f, 1f))` |
| Update rate | ~30fps (`WaitForSeconds(0.033f)`) |
| Lifecycle | Start in `Create()`, stop in `OnDestroy()` |

### 7.2 Settings Expand

| Per-button | Scale `Vector3.zero` → `Vector3.one`, 0.2s, EaseOutBack, 0.05s stagger top→bottom |

### 7.3 Settings Collapse

| Per-button | Scale `Vector3.one` → `Vector3.zero`, 0.15s, EaseOutBack, 0.03s stagger bottom→top. 0.15s delay before `SetActive(false)` |

### 7.4 ButtonBounce

| Press 0.92 (60ms) | Release spring 1.05 (90ms) → settle 1.0 (60ms) | Cancel 1.0 (100ms) |

---

## 8. Interaction Logic

### 8.1 Settings Panel Toggle

```
ToggleSettings():
  if animating → StopCoroutine
  flip _isSettingsOpen
  if opening:
    _settingsPanel.SetActive(true)
    _outsideTapBlocker.SetActive(true)
    _settingsPanel.SetAsLastSibling()
    _topBar.SetAsLastSibling()
    StartCoroutine(AnimateSettingsExpand)
  else:
    StartCoroutine(AnimateSettingsCollapse)
    → on complete: SetActive(false), blocker off
  fire OnSettingsPressed
```

### 8.2 Toggle Persistence

| Toggle | Read | Write |
|---|---|---|
| Music | `IProgressionManager.MusicEnabled` | + `IAudioManager.SetMusicEnabled()` |
| SFX | `IProgressionManager.SoundEnabled` | + `IAudioManager.SetSoundEnabled()` |
| Vibration | `PlayerPrefs("VibrationEnabled")` | `PlayerPrefs.SetInt()` + `.Save()` |

### 8.3 Restart / Exit

| Restart | `CollapseSettingsIfOpen()` → `OnRestartPressed?.Invoke()` |
| Exit | `CollapseSettingsIfOpen()` → `OnExitPressed?.Invoke()` → `GoBackToHub()` |

### 8.4 Disabled Button States

| Button | Disabled When | Visual |
|---|---|---|
| Undo | `!canAfford \|\| !hasUndos` | `CanvasGroup.alpha = 0.4`, `interactable = false`, `Transition.None` |
| Extra Bottle | `!canAfford \|\| !hasRemaining` | `CanvasGroup.alpha = 0.4`, `interactable = false`, `Transition.None` |

### 8.5 Gameplay Blocking

| Settings open | Pour animating | Level complete | → All taps blocked |

---

<!-- Fixed: Issue 3 — Constants updated to match -->
## 9. Constants Reference (HTML × 3)

```csharp
// Top bar
private const float BarHeight = 198f;             // CSS: (52-status+content+8)px effective ~66px × 3
private const float BarMargin = 54f;               // CSS: 18px × 3

// Coin display — uses ContentSizeFitter, no fixed size constant needed
private const float CoinIconSize = 84f;            // CSS: 28px × 3
private const float CoinGlowSize = 108f;           // icon(84) + glow spread(24)

// Level pill — uses ContentSizeFitter, no fixed size constant needed

// Settings gear
private const float GearButtonSize = 138f;         // CSS: 46px × 3

// Settings panel 3D buttons
private const float SettingsButtonSize = 150f;     // CSS: 50px × 3
private const float SettingsShadowDepth = 12f;     // CSS: 4px × 3
private const float SettingsButtonSpacing = 30f;   // CSS: gap 10px × 3

// Bottom action panel — uses ContentSizeFitter
private const float ActionButtonGap = 30f;         // CSS: gap 10px × 3

// Icons and text
private const float MiniCoinSize = 48f;            // CSS: 16px × 3
private const float BottleBodyWidth = 36f;         // CSS: 12px × 3
private const float BottleBodyHeight = 78f;        // CSS: 26px × 3
private const float BottleNeckWidth = 21f;         // CSS: 7px × 3
private const float BottleNeckHeight = 12f;        // CSS: 4px × 3
```

---

## 10. Key Files

| File | Path | Role |
|---|---|---|
| GameplayHUD.cs | `Assets/Scripts/Game/UI/Components/GameplayHUD.cs` | All HUD construction + logic |
| GameplayManager.cs | `Assets/Scripts/Game/Puzzle/GameplayManager.cs` | Creates HUD, wires events |
| ThemeConfig.cs | `Assets/Scripts/Game/UI/ThemeConfig.cs` | Colors, fonts, gradients |
| UIShapeUtils.cs | `Assets/Scripts/Game/UI/Components/UIShapeUtils.cs` | Cached procedural sprites |
| ButtonBounce.cs | `Assets/Scripts/Game/UI/Components/ButtonBounce.cs` | Press animation component |
