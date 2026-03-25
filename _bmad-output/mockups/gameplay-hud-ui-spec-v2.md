# Gameplay HUD — UI Specification v2

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
│   ├── TopBar                           RectTransform (stretch-top, h=180)
│   │   ├── CoinDisplay                  RectTransform (auto×54, left)
│   │   │   ├── [Image: pill bg + golden border]
│   │   │   ├── CoinGlow                 Image (96×96, glow pulse)
│   │   │   ├── CoinIcon                 Image (84×84, radial gradient circle)
│   │   │   │   └── CoinSymbol           TMPro ("$", 33px)
│   │   │   └── CoinText                 TMPro ("1,250", 51px bold gold)
│   │   ├── LevelPill                    RectTransform (auto×54, anchor 0.58)
│   │   │   ├── [Image: pill bg]
│   │   │   ├── LevelText                TMPro ("Level 10", 54px bold)
│   │   │   └── MoveText                 TMPro ("12 moves", 36px medium)
│   │   └── SettingsGear                 RectTransform (138×138, right)
│   │       ├── [Image: gradient bg + Button]
│   │       └── GearIcon                 TMPro ("⚙", 66px)
│   ├── SettingsPanel                    RectTransform (150+gap, top-right) — hidden
│   │   ├── MusicToggle                  [3D Button: 150×150 + 12px shadow]
│   │   ├── SFXToggle                    [3D Button]
│   │   ├── VibrationToggle             [3D Button]
│   │   ├── RestartButton               [3D Button]
│   │   └── ExitButton                  [3D Button]
│   └── BottomBar                        RectTransform (stretch-bottom, h=108)
│       └── ActionPill                   RectTransform (auto×auto, center)
│           ├── [Image: pill bg]
│           ├── UndoButton               RectTransform (auto)
│           │   ├── [Image: gradient bg + Button]
│           │   ├── UndoIcon             TMPro ("↶", 54px white)
│           │   ├── CoinIcon             Image (48×48, gold circle)
│           │   │   └── CoinSymbol       TMPro ("$", 21px)
│           │   └── CostText             TMPro ("100", 39px bold gold)
│           └── ExtraBottleButton        RectTransform (auto)
│               ├── [Image: gradient bg + Button]
│               ├── BottleIcon           Image (36×78 body + 21×12 neck)
│               ├── PlusIcon             TMPro ("+", 54px white)
│               ├── CoinIcon             Image (48×48, gold circle)
│               │   └── CoinSymbol       TMPro ("$", 21px)
│               └── CostText             TMPro ("500", 39px bold gold)
```

---

## 3. RectTransform & Visual Properties — Every Element

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

### 3.2 `TopBar`

CSS: `.top-bar { padding: 52px 18px 8px 18px }` → total height ~60px CSS = ~180px Unity

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 180)` |
| anchoredPosition | `(0, 0)` |

---

### 3.3 `CoinDisplay`

CSS: `.coin-display { padding: 6px 14px 6px 8px; gap: 7px; border-radius: 20px; border: 1.5px solid }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0.5)` | left-aligned in flex row |
| anchorMax | `(0, 0.5)` | |
| pivot | `(0, 0.5)` | |
| anchoredPosition | `(54, 0)` | padding-left 18px × 3 = 54 |
| sizeDelta | **auto-width** or `(240, 54)` | padding 6/14/6/8 × 3. Height ≈ 18 + coin(84) padded ≈ 54px min |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteRoundedRect(20, 64)` + `Sliced` | `border-radius: 20px` (not ×3 — sprite radius) |
| Image color | `rgba(0,0,0,0.4)` | `background: rgba(0,0,0,0.4)` |
| Border | `Outline` component or 9-slice with border baked in | `border: 1.5px solid` = 4.5px Unity |
| Border color Night | `rgba(255,180,40,0.35)` | `.night .coin-display` |
| Border color Morning | `rgba(180,130,30,0.4)` | `.morning .coin-display` |
| Outer glow | Glow child Image, +30px each side, `rgba(255,200,50,0.1)` | `box-shadow: 0 0 10px rgba(255,200,50,0.1)` |
| Inner highlight | Highlight child, top 3px, `rgba(255,255,255,0.08)` | `inset 0 1px 0 rgba(255,255,255,0.08)` |

### 3.3a `CoinGlow` (child of CoinDisplay, behind CoinIcon)

| Property | Value |
|---|---|
| anchorMin | `(0, 0.5)` |
| anchorMax | `(0, 0.5)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(42, 0)` — centered on coin icon |
| sizeDelta | `(96, 96)` |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteCircle(64)` | circular glow |
| Image color | animated — see §7.1 | `box-shadow: 0 0 8px rgba(255,200,50,0.35)` base |
| raycastTarget | `false` | |

### 3.3b `CoinIcon` (child of CoinDisplay)

CSS: `.coin-icon { width: 28px; height: 28px }` → **84 × 84**

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0.5)` | left-aligned within pill |
| anchorMax | `(0, 0.5)` | |
| pivot | `(0.5, 0.5)` | |
| anchoredPosition | `(42, 0)` | centered at pill left + half coin |
| sizeDelta | `(84, 84)` | 28px × 3 |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | Procedural radial gradient circle (see §6.1) OR `UIShapeUtils.WhiteCircle(64)` with gold tint as fallback | `radial-gradient(circle at 38% 32%, #ffe866, #f5a623)` |
| Image color (fallback) | `#F5A623` (amber gold) | dominant edge color |
| Border | Outline component, 6px, `#CC8800` | `border: 2px solid #cc8800` × 3 = 6px |
| raycastTarget | `false` | |

**Ideal:** Baked 84×84 radial gradient texture. Center `#FFE866` at offset (38%, 32%), edge `#F5A623`. Convert to `Sprite`. Set `Image.color = Color.white`.

**Fallback:** `UIShapeUtils.WhiteCircle(64)` tinted `#F5A623`. Loses gradient but keeps circle shape.

### 3.3c `CoinSymbol` (child of CoinIcon)

CSS: `.coin-icon { font-size: 11px; font-weight: 800; color: #8B6914 }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0)` | stretch-fill parent |
| anchorMax | `(1, 1)` | |
| offsetMin / offsetMax | `(0, 0)` | |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"$"` | coin emblem |
| fontSize | `33` | 11px × 3 |
| fontStyle | `Bold` | weight 800 → Bold (max available) |
| alignment | `Center` | flex center |
| color | `#8B6914` → `(0.545, 0.412, 0.078, 1)` | dark gold |
| font | `Nunito-Regular SDF` | Fredoka in mockup → Nunito in project |
| raycastTarget | `false` | |

### 3.3d `CoinText` (child of CoinDisplay)

CSS: `.coin-amount { font-size: 17px; font-weight: 700; color: #ffe066 }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0)` | |
| anchorMax | `(1, 1)` | stretch-fill, offset from left |
| offsetMin | `(96, 0)` | 84px coin + 7px gap × 3 ≈ 96px |
| offsetMax | `(0, 0)` | |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"0"` | updated by `UpdateCoinDisplay()` |
| fontSize | `51` | 17px × 3 |
| fontStyle | `Bold` | weight 700 |
| alignment | `MidlineLeft` | |
| color | `#FFE066` → `(1.0, 0.878, 0.4, 1)` | |
| font | `Nunito-Regular SDF` | |
| **Effect:** text-shadow | TMPro Underlay: color `rgba(255,220,50,0.2)`, offset (0,0), dilate 0.3, softness 0.5 | `text-shadow: 0 0 6px rgba(255,220,50,0.2)` |

---

### 3.4 `LevelPill`

CSS: `.level-pill { left: 58%; padding: 6px 22px; border-radius: 18px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0.58, 0.5)` | `left: 58%; transform: translateX(-50%)` |
| anchorMax | `(0.58, 0.5)` | |
| pivot | `(0.5, 0.5)` | `translateX(-50%)` |
| anchoredPosition | `(0, 0)` | |
| sizeDelta | `(auto, auto)` or `(240, 72)` | padding 6/22 × 3 + content |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteRoundedRect(18, 64)` + `Sliced` | `border-radius: 18px` |
| Image color Night | `rgba(0,0,0,0.45)` | `.night .level-pill` |
| Image color Morning | `rgba(60,40,10,0.35)` → `(0.235, 0.157, 0.039, 0.35)` | `.morning .level-pill` |
| Border Night | `rgba(255,255,255,0.08)` | `.night .level-pill border-color` |
| Border Morning | `rgba(255,255,255,0.1)` | `.morning .level-pill border-color` |
| Drop shadow | Shadow child, offset (0, 6px), blur 24px, `rgba(0,0,0,0.2)` | `box-shadow: 0 2px 8px rgba(0,0,0,0.2)` × 3 |

### 3.4a `LevelText` (child of LevelPill)

CSS: `.level-text { font-size: 18px; font-weight: 700; color: rgba(255,245,230,0.95) }`

| Property | Value |
|---|---|
| anchorMin | `(0, 0.35)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(12, 0)` |
| offsetMax | `(-12, 0)` |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"Level 1"` | set by `SetLevelInfo()` |
| fontSize | `54` | 18px × 3 |
| fontStyle | `Bold` | weight 700 |
| alignment | `Center` | `text-align: center` |
| color | `rgba(255,245,230,0.95)` → `(1.0, 0.961, 0.902, 0.95)` | Night |
| font | `Nunito-Regular SDF` | |
| **Effect:** text-shadow | TMPro Underlay: color `rgba(0,0,0,0.3)`, offsetY=3, dilate 0, softness 0.4 | `text-shadow: 0 1px 4px rgba(0,0,0,0.3)` |

### 3.4b `MoveText` (child of LevelPill)

CSS: `.move-text { font-size: 12px; font-weight: 500; color: rgba(180,170,200,0.7); margin-top: 1px }`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0.4)` |
| offsetMin | `(12, 0)` |
| offsetMax | `(-12, 0)` |

| TMPro | Value | CSS Source |
|---|---|---|
| text | `""` | set by `UpdateDisplay()`, e.g. "12 moves" |
| fontSize | `36` | 12px × 3 |
| fontStyle | `Normal` | weight 500 → Normal (no medium weight available) |
| alignment | `Center` | |
| color Night | `rgba(180,170,200,0.7)` → `(0.706, 0.667, 0.784, 0.7)` | |
| color Morning | `rgba(120,95,55,0.8)` → `(0.471, 0.373, 0.216, 0.8)` | `.morning .move-text` |
| font | `Nunito-Regular SDF` | |

---

### 3.5 `SettingsGear`

CSS: `.settings-btn { width: 46px; height: 46px; border-radius: 14px; font-size: 22px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(1, 0.5)` | right-aligned in flex row |
| anchorMax | `(1, 0.5)` | |
| pivot | `(1, 0.5)` | |
| anchoredPosition | `(-54, 0)` | padding-right 18px × 3 = 54 |
| sizeDelta | `(138, 138)` | 46px × 3 |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteRoundedRect(14, 48)` + `Sliced` | `border-radius: 14px` |
| Image color Night | Gradient fallback: `rgba(75,55,135,0.65)` | `linear-gradient(135deg, rgba(90,70,150,0.7), rgba(60,40,120,0.6))` mid-blend |
| Image color Morning | Gradient fallback: `rgba(105,88,50,0.65)` | `linear-gradient(135deg, rgba(120,100,60,0.7), rgba(90,75,40,0.6))` mid-blend |
| **Ideal:** gradient | Gradient Material or baked sprite: top-left `rgba(90,70,150,0.7)` → bottom-right `rgba(60,40,120,0.6)` | 135deg gradient |
| Border | 4.5px, `rgba(255,255,255,0.1)` | `border: 1.5px solid` × 3 |
| Glow | Glow child, +24px, `rgba(140,100,220,0.15)` | `box-shadow: 0 0 8px` × 3 |
| Inner highlight | Top 3px, `rgba(255,255,255,0.1)` | `inset 0 1px 0` × 3 |

**Gear icon TMPro:**

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"\u2699"` (⚙) | `&#9881;` |
| fontSize | `66` | 22px × 3 |
| color | `rgba(255,255,255,0.85)` | |
| alignment | `Center` | |
| font | `Nunito-Regular SDF` | |

---

### 3.6 `SettingsPanel`

CSS: `.settings-panel { top: 106px; right: 18px; gap: 10px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(1, 1)` | top-right absolute |
| anchorMax | `(1, 1)` | |
| pivot | `(1, 1)` | |
| anchoredPosition | `(-54, -318)` | right 18×3=54, top 106×3=318 |
| sizeDelta | `(174, 840)` | width: 150+24 padding, height: 5×(150+12+30)+padding |
| default active | `false` | `.hidden { display: none }` |
| Image | **NONE** — no background | |

### 3.6a Each 3D Settings Button

CSS: `.s-btn { width: 50px; height: 50px; border-radius: 14px }`

Each button = Container (150 × 162) holding Shadow + Face + Highlight.

| Button | Y Offset | Face Color (ON/default) | Shadow Color |
|---|---|---|---|
| MusicToggle | 0 | Green gradient (see below) | `#069920` |
| SFXToggle | -192 | Green gradient | `#069920` |
| VibrationToggle | -384 | Green gradient | `#069920` |
| RestartButton | -576 | Green gradient | `#069920` |
| ExitButton | -768 | Red gradient (see below) | `#990505` |

Step Y = 150 (face) + 12 (shadow) + 30 (gap) = 192

**Container RectTransform:**

| Property | Value |
|---|---|
| anchorMin | `(0.5, 1)` |
| anchorMax | `(0.5, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(150, 162)` — 150 face + 12 shadow depth |

**Shadow (child):**

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0)` |
| pivot | `(0.5, 0)` |
| sizeDelta | `(0, 150)` | |
| Image sprite | `UIShapeUtils.WhiteRoundedRect(14, 48)` + `Sliced` | `border-radius: 14px` |
| Image color | See per-button shadow color | `0 4px 0 #color` × 3 = 12px depth |
| raycastTarget | `false` | |

**Face (child):**

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 150)` |
| Image sprite | `UIShapeUtils.WhiteRoundedRect(14, 48)` + `Sliced` |
| Button | yes |
| ButtonBounce | yes |

**Face color states:**

| State | CSS Gradient | Flat Fallback | CSS Source |
|---|---|---|---|
| ON (green) | `linear-gradient(180deg, #44ff66 0%, #22dd44 35%, #10cc30 65%, #08b825 100%)` | `#22DD44` | `.s-btn.on` |
| OFF (gray) | `linear-gradient(180deg, #7a7a7a 0%, #5e5e5e 50%, #484848 100%)` | `#5E5E5E` | `.s-btn.off` |
| Exit (red) | `linear-gradient(180deg, #ff4444 0%, #ee2020 35%, #dd1010 65%, #cc0505 100%)` | `#EE2020` | `.s-btn.exit` |

**Ideal gradient:** Use `ThemeConfig.CreateGradientSprite(top, bottom)` for each state.
**Fallback:** Flat color from table above.

**Highlight (child of Face):**

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 6)` | `inset 0 2px 0` × 3 |
| Image color (ON) | `rgba(255,255,255,0.4)` | `.s-btn.on inset` |
| Image color (OFF) | `rgba(255,255,255,0.1)` | `.s-btn.off inset` |
| Image color (Exit) | `rgba(255,255,255,0.3)` | `.s-btn.exit inset` |
| raycastTarget | `false` | |

**Glow (ON/Exit only — child of Container, behind Shadow):**

| Property | Value | CSS Source |
|---|---|---|
| sizeDelta | `(210, 210)` — face + 60px glow | `0 0 20px` × 3 = 60px spread |
| color (ON) | `rgba(40,255,80,0.2)` | `.s-btn.on` outer glow |
| color (Exit) | `rgba(255,50,50,0.2)` | `.s-btn.exit` outer glow |
| sprite | `UIShapeUtils.Glow(128, Color.white, 0.5f)` tinted | |

**Icon TMPro (child of Face):**

| TMPro | Value | CSS Source |
|---|---|---|
| fontSize | `72` | `.material-icons-round { font-size: 24px }` × 3 |
| alignment | `Center` | |
| color (ON) | `#FFFFFF` | |
| color (OFF) | `rgba(255,255,255,0.4)` | `.s-btn.off { color }` |
| font | `Nunito-Regular SDF` (Unicode fallback) | Material Icons Round in mockup |

**Icon mapping:**

| Button | ON | OFF | Ideal (Material Icons) | Fallback (Unicode) |
|---|---|---|---|---|
| Music | `music_note` | `music_off` | Material Icons SDF font | `\u266B` (♫) |
| SFX | `volume_up` | `volume_off` | Material Icons SDF font | `\u266A` / `\u2022` |
| Vibration | `vibration` | `mobile_off` | Material Icons SDF font | `\u2058` / `\u2022` |
| Restart | `replay` | — | Material Icons SDF font | `\u21BB` (↻) |
| Exit | `exit_to_app` | — | Material Icons SDF font | `\u2192` (→) |

**Ideal:** Import Material Icons Round TTF → generate TMP SDF font asset → use as icon font.
**Fallback:** Unicode glyphs in Nunito (current approach — visually weaker).

---

### 3.7 `BottomBar`

CSS: `.bottom-bar { padding: 0 18px 28px 18px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0, 0)` | |
| anchorMax | `(1, 0)` | |
| pivot | `(0.5, 0)` | |
| sizeDelta | `(0, 108)` | padding-bottom 28px × 3 = 84 + content space |
| anchoredPosition | `(0, 0)` | |

### 3.7a `ActionPill`

CSS: `.action-pill { gap: 10px; padding: 8px 10px; border-radius: 22px }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0.5, 0.5)` | centered |
| anchorMax | `(0.5, 0.5)` | |
| pivot | `(0.5, 0.5)` | |
| sizeDelta | **auto** or `(480, 96)` | content-driven, gap 10×3=30, padding 8/10 × 3 = 24/30 |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteRoundedRect(22, 64)` + `Sliced` | `border-radius: 22px` |
| Image color (base) | `rgba(0,0,0,0.45)` | `.action-pill background` |
| Image color Night | `rgba(0,0,0,0.5)` | `.night .action-pill` |
| Image color Morning | `rgba(60,40,10,0.4)` → `(0.235, 0.157, 0.039, 0.4)` | `.morning .action-pill` |
| Border Night | `rgba(255,255,255,0.08)` | `.night .action-pill border-color` |
| Border Morning | `rgba(255,255,255,0.1)` | `.morning .action-pill border-color` |
| Drop shadow | Shadow child, offset (0, 12px), blur 48px, `rgba(0,0,0,0.3)` | `box-shadow: 0 4px 16px` × 3 |

---

### 3.7b `UndoButton`

CSS: `.action-btn { padding: 10px 18px; border-radius: 16px; gap: 5px }`
CSS: `.action-btn.undo { background: linear-gradient(135deg, rgba(90,75,130,0.8), rgba(70,55,110,0.7)) }`

| Property | Value | CSS Source |
|---|---|---|
| anchorMin | `(0.02, 0.06)` | left half of pill |
| anchorMax | `(0.48, 0.94)` | |
| offsetMin / offsetMax | `(0, 0)` | |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteRoundedRect(16, 48)` + `Sliced` | `border-radius: 16px` |
| Image color | Gradient fallback: `rgba(80,65,120,0.75)` | mid-blend of gradient |
| **Ideal:** gradient | Material: top-left `rgba(90,75,130,0.8)` → bottom-right `rgba(70,55,110,0.7)` | 135deg |
| Border | 3px, `rgba(255,255,255,0.08)` | `border: 1px solid` × 3 |
| Glow | Child, +24px, `rgba(140,100,220,0.15)` | `box-shadow: 0 0 8px` × 3 |
| Button transition | `Selectable.Transition.None` | CanvasGroup handles disabled |
| ButtonBounce | yes | |

**UndoButton children (left to right):**

**UndoIcon:**

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"\u21B6"` (↶) | `&#8630;` |
| fontSize | `54` | `.action-icon { font-size: 18px }` × 3 |
| color | `#FFFFFF` | |
| alignment | `Center` | |
| Anchor | left 35% of button | |

**CoinIcon (mini, in undo):**

CSS: `.coin-icon-small { width: 16px; height: 16px }` → **48 × 48**

| Property | Value |
|---|---|
| sizeDelta | `(48, 48)` |
| Image | Radial gradient circle (same as top coin, smaller) OR `WhiteCircle` tinted `#F5A623` |
| Border | 4.5px `#CC8800` | `border: 1.5px solid` × 3 |
| "$" child | fontSize `21` (7px × 3), color `#8B6914`, Bold |

**CostText:**

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"100"` | updated by `UpdateUndoState()` |
| fontSize | `39` | `.cost { font-size: 13px }` × 3 |
| fontStyle | `Bold` | weight 600 → Bold |
| color | `rgba(255,220,100,0.9)` → `(1.0, 0.863, 0.392, 0.9)` | `.cost color` |
| alignment | `MidlineRight` | |

### 3.7c `ExtraBottleButton`

CSS: `.action-btn.extra { background: linear-gradient(135deg, rgba(40,120,60,0.8), rgba(30,100,50,0.7)) }`

| Property | Value |
|---|---|
| anchorMin | `(0.52, 0.06)` |
| anchorMax | `(0.98, 0.94)` |
| offsetMin / offsetMax | `(0, 0)` |

| Visual | Value | CSS Source |
|---|---|---|
| Image sprite | `UIShapeUtils.WhiteRoundedRect(16, 48)` + `Sliced` | |
| Image color | Gradient fallback: `rgba(35,110,55,0.75)` | mid-blend |
| **Ideal:** gradient | `rgba(40,120,60,0.8)` → `rgba(30,100,50,0.7)` | 135deg |
| Border | 3px, `rgba(255,255,255,0.08)` | |
| Glow | Child, +24px, `rgba(80,200,100,0.15)` | |

**ExtraBottleButton children (left to right):**

**BottleIcon:**

CSS: `.bottle-icon { width: 12px; height: 26px; border: 1.5px solid rgba(255,255,255,0.7); border-radius: 2px 2px 5px 5px }`

| Property | Value | CSS Source |
|---|---|---|
| Body sizeDelta | `(36, 78)` | 12×3, 26×3 |
| Body border-radius | `UIShapeUtils.WhiteRoundedRect(6, 32)` + `Sliced` | bottom corners 5px, top 2px |
| Body border | 4.5px, `rgba(255,255,255,0.7)` | 1.5px × 3 |
| Body Image color | `rgba(255,255,255,0.7)` | outline only — or use sprite with hollow center |
| Neck sizeDelta | `(21, 12)` | 7×3, 4×3 |
| Neck position | `(0, 6)` above body top | `top: -4px` × 3, with 2px gap |
| Neck border | 4.5px, `rgba(255,255,255,0.7)`, no bottom | `border-bottom: none` |

**PlusIcon:**

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"+"` | |
| fontSize | `54` | `font-size: 18px` × 3 |
| color | `#FFFFFF` | |

**CoinIcon (mini):** Same as undo mini coin — 48×48, gradient, "$", border.

**CostText:**

| TMPro | Value | CSS Source |
|---|---|---|
| text | `"500"` | updated by `UpdateExtraBottleState()` |
| fontSize | `39` | 13px × 3 |
| fontStyle | `Bold` | weight 600 |
| color | `rgba(255,220,100,0.9)` | |

---

## 4. Color Reference

### 4.1 Constant Colors

| Name | Hex | RGBA Float | Usage |
|---|---|---|---|
| CoinGradientCenter | `#FFE866` | `(1.0, 0.91, 0.40)` | Coin radial gradient center |
| CoinGradientEdge | `#F5A623` | `(0.96, 0.65, 0.14)` | Coin radial gradient edge |
| CoinBorder | `#CC8800` | `(0.80, 0.53, 0.0)` | Coin circle border |
| CoinSymbol | `#8B6914` | `(0.545, 0.412, 0.078)` | "$" text inside coin |
| CoinAmountGold | `#FFE066` | `(1.0, 0.878, 0.40)` | Coin balance text |
| CostGold | — | `(1.0, 0.863, 0.392, 0.9)` | Action button cost numbers |
| GreenON | `#22DD44` | flat fallback | Settings ON face |
| GreenON Shadow | `#069920` | | Settings ON shadow |
| GreenGradient | `#44FF66 → #08B825` | 4-stop | Settings ON full gradient |
| GrayOFF | `#5E5E5E` | flat fallback | Settings OFF face |
| GrayOFF Shadow | `#333333` | | Settings OFF shadow |
| GrayGradient | `#7A7A7A → #484848` | 3-stop | Settings OFF full gradient |
| RedExit | `#EE2020` | flat fallback | Exit button face |
| RedExit Shadow | `#990505` | | Exit button shadow |
| RedGradient | `#FF4444 → #CC0505` | 4-stop | Exit full gradient |
| HighlightON | — | `(1,1,1,0.4)` | Green button top highlight |
| HighlightOFF | — | `(1,1,1,0.1)` | Gray button top highlight |
| HighlightExit | — | `(1,1,1,0.3)` | Red button top highlight |
| IconON | `#FFFFFF` | `(1,1,1,1)` | Icon when enabled |
| IconOFF | — | `(1,1,1,0.4)` | Icon when disabled |
| PillBg | — | `(0,0,0,0.4)` | Default pill background |
| Blocker | — | `(0,0,0,0)` | Outside tap blocker |

### 4.2 Mood-Specific Colors

| Element | Night | Morning |
|---|---|---|
| Coin pill border | `rgba(255,180,40,0.35)` | `rgba(180,130,30,0.4)` |
| Level pill bg | `rgba(0,0,0,0.45)` | `rgba(60,40,10,0.35)` |
| Level pill border | `rgba(255,255,255,0.08)` | `rgba(255,255,255,0.1)` |
| Gear bg gradient | `rgba(90,70,150,0.7)` → `rgba(60,40,120,0.6)` | `rgba(120,100,60,0.7)` → `rgba(90,75,40,0.6)` |
| Move text color | `rgba(180,170,200,0.7)` | `rgba(120,95,55,0.8)` |
| Action pill bg | `rgba(0,0,0,0.5)` | `rgba(60,40,10,0.4)` |
| Action pill border | `rgba(255,255,255,0.08)` | `rgba(255,255,255,0.1)` |
| Level text | `rgba(255,245,230,0.95)` | ThemeConfig.TextPrimary |

---

## 5. Font Specifications

| Element | CSS font-size | × 3 = Unity fontSize | CSS weight | TMPro fontStyle | CSS color |
|---|---|---|---|---|---|
| Coin "$" | 11px | **33** | 800 | Bold | `#8B6914` |
| Coin amount | 17px | **51** | 700 | Bold | `#FFE066` |
| Level text | 18px | **54** | 700 | Bold | `rgba(255,245,230,0.95)` |
| Move counter | 12px | **36** | 500 | Normal | `rgba(180,170,200,0.7)` |
| Gear icon | 22px | **66** | — | Normal | `rgba(255,255,255,0.85)` |
| Settings icons | 24px | **72** | — | Normal | `#FFFFFF` / 40% |
| Action btn text | 15px | **45** | 600 | Bold | `#FFFFFF` |
| Action icon (↶, +) | 18px | **54** | — | Normal | `#FFFFFF` |
| Cost number | 13px | **39** | 600 | Bold | `rgba(255,220,100,0.9)` |
| Mini coin "$" | 7px | **21** | 800 | Bold | `#8B6914` |

**Font:** Mockup uses Fredoka (Google Fonts). Project uses **Nunito-Regular SDF** — similar rounded sans-serif.
**Bold** via `fontStyle = FontStyles.Bold` (Nunito supports bold rendering through TMPro).

---

## 6. Sprite Specifications

### 6.1 Coin Radial Gradient (84×84)

| Property | Value |
|---|---|
| Texture size | 84×84 px (or 128×128 for quality, scaled down) |
| Type | Radial gradient, center offset to (38%, 32%) from top-left |
| Center color | `#FFE866` |
| Edge color | `#F5A623` |
| Generation | Procedural `Texture2D`, iterate pixels, lerp by distance from offset center |
| Cache | Static sprite, created once |
| Usage | CoinIcon (84×84), MiniCoin (48×48) — same sprite, different Image size |

### 6.2 Rounded Rectangle 9-Slice Sprites (via UIShapeUtils)

| Radius | Size | Used By |
|---|---|---|
| 22 | 64×64 | ActionPill |
| 20 | 64×64 | CoinDisplay |
| 18 | 64×64 | LevelPill |
| 16 | 48×48 | UndoButton, ExtraBottleButton |
| 14 | 48×48 | SettingsGear, 3D button Face, 3D button Shadow |
| 6 | 32×32 | Bottle body |
| 3 | 16×16 | Bottle neck |

### 6.3 Circle Sprite (via UIShapeUtils)

| Size | Used By |
|---|---|
| 64 | CoinGlow (tinted), CoinIcon (fallback), MiniCoinIcon |

### 6.4 Glow Sprite (via UIShapeUtils)

| Size | Used By |
|---|---|
| 128 | Settings button outer glow (ON/Exit) |

---

## 7. Animation Specifications

### 7.1 Coin Icon Pulse

CSS: `@keyframes coinPulse { 0%,100%: box-shadow 6px 0.25α; 50%: 14px 0.55α }`

| Property | Value |
|---|---|
| Target | `_coinGlowImage` (96×96 behind coin icon) |
| Color base | `ThemeConfig.GetColor(ThemeColorType.StarGold)` (refreshed each tick) |
| Alpha range | `0.15` ↔ `0.55` |
| Period | `2.5` seconds |
| Easing | `Mathf.SmoothStep(0.15f, 0.55f, Mathf.PingPong(elapsed / 1.25f, 1f))` |
| Update rate | ~30fps (`WaitForSeconds(0.033f)`) |
| Lifecycle | Start in `Create()`, stop in `OnDestroy()` |

### 7.2 Settings Expand

| Property | Value |
|---|---|
| Per-button | Scale `Vector3.zero` → `Vector3.one` |
| Duration | 0.2s per button |
| Stagger | 0.05s (top → bottom) |
| Easing | EaseOutBack: `c1=1.70158, c3=c1+1` |

### 7.3 Settings Collapse

| Property | Value |
|---|---|
| Per-button | Scale `Vector3.one` → `Vector3.zero` |
| Duration | 0.15s per button |
| Stagger | 0.03s (bottom → top) |
| Post-delay | 0.15s before `SetActive(false)` |

### 7.4 ButtonBounce

| Property | Value |
|---|---|
| Press | scale to 0.92 (60ms) |
| Release | spring to 1.05 (90ms) → settle 1.0 (60ms) |
| Cancel | return 1.0 (100ms) |

### 7.5 3D Button Press (CSS: `:active`)

| Property | Value | CSS Source |
|---|---|---|
| Face translateY | `+9px` (3px × 3) | `translateY(3px)` |
| Face scale | `0.97` | `scale(0.97)` |
| Shadow depth | reduces from 12px → 3px | `0 4px → 0 1px` × 3 |
| Note | Combined with ButtonBounce — may use ButtonBounce only for simplicity |

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
| Music | `IProgressionManager.MusicEnabled` | `.MusicEnabled` + `IAudioManager.SetMusicEnabled()` |
| SFX | `IProgressionManager.SoundEnabled` | `.SoundEnabled` + `IAudioManager.SetSoundEnabled()` |
| Vibration | `PlayerPrefs("VibrationEnabled")` | `PlayerPrefs.SetInt()` + `.Save()` |

### 8.3 Restart / Exit

| Action | Flow |
|---|---|
| Restart | `CollapseSettingsIfOpen()` → `OnRestartPressed?.Invoke()` |
| Exit | `CollapseSettingsIfOpen()` → `OnExitPressed?.Invoke()` → `GoBackToHub()` |

### 8.4 Disabled Button States

CSS: `.action-btn.disabled { opacity: 0.4; pointer-events: none }`

| Button | Disabled When | Visual |
|---|---|---|
| Undo | `!canAfford \|\| !hasUndos` | `CanvasGroup.alpha = 0.4`, `interactable = false`, `Transition.None` |
| Extra Bottle | `!canAfford \|\| !hasRemaining` | `CanvasGroup.alpha = 0.4`, `interactable = false`, `Transition.None` |

### 8.5 Gameplay Blocking

| Condition | Blocked? |
|---|---|
| Settings panel open | All gameplay taps blocked |
| Pour animation playing | All HUD + gameplay taps blocked |
| Level complete | All gameplay taps blocked |

---

## 9. Constants Reference (HTML × 3)

```csharp
// Top bar
private const float BarHeight = 180f;           // CSS: padding-top 52px + content + padding-bottom 8px → ~60px × 3
private const float BarMargin = 54f;             // CSS: padding 18px × 3

// Coin display
private const float CoinIconSize = 84f;          // CSS: 28px × 3
private const float CoinGlowSize = 96f;          // CSS: icon + glow spread
private const float CoinDisplayHeight = 54f;      // CSS: pill height

// Level pill
private const float LevelPillWidth = 240f;        // CSS: auto ~80px × 3
private const float LevelPillHeight = 72f;         // CSS: ~24px × 3

// Settings gear
private const float GearButtonSize = 138f;         // CSS: 46px × 3

// Settings panel 3D buttons
private const float SettingsButtonSize = 150f;     // CSS: 50px × 3
private const float SettingsShadowDepth = 12f;     // CSS: 4px × 3
private const float SettingsButtonSpacing = 30f;   // CSS: gap 10px × 3
private const float SettingsButtonStep = 192f;     // 150 + 12 + 30

// Bottom action panel
private const float ActionPillBorderRadius = 22f;  // CSS: 22px (sprite radius)
private const float ActionButtonBorderRadius = 16f; // CSS: 16px (sprite radius)
private const float ActionButtonGap = 30f;          // CSS: gap 10px × 3

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
