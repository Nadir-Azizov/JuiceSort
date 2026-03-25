# Gameplay HUD — Implementation-Ready UI Specification

**Screen:** Gameplay HUD (overlay during puzzle play)
**Story:** 11.6 Gameplay HUD Overhaul
**Visual Mockup:** [gameplay-hud-mockup.html](gameplay-hud-mockup.html)

---

## 1. Canvas & Scaler

| Property | Value |
|---|---|
| Canvas render mode | `RenderMode.ScreenSpaceOverlay` |
| CanvasScaler mode | `ScaleMode.ScaleWithScreenSize` |
| Reference resolution | `1080 × 1920` |
| Match width or height | `0.5` |
| Pixels per unit | `100` (default) |

---

## 2. GameObject Hierarchy

```
GameplayHUD                          RectTransform (stretch-all)
├── OutsideTapBlocker                RectTransform (stretch-all) — hidden by default
│   └── [Image + Button]
├── SafeArea                         RectTransform (anchored to Screen.safeArea)
│   ├── TopBar                       RectTransform (stretch-top, h=100)
│   │   ├── CoinDisplay              RectTransform (180×48, left-center)
│   │   │   ├── CoinIcon             RectTransform (36×36, gold circle)
│   │   │   └── CoinText             TextMeshProUGUI ("1,250")
│   │   ├── LevelPill                RectTransform (280×60, anchor 0.58)
│   │   │   ├── [Image bg]
│   │   │   ├── LevelText            TextMeshProUGUI ("Level 10")
│   │   │   └── MoveText             TextMeshProUGUI ("12 moves")
│   │   └── SettingsGear             RectTransform (88×88, right-center)
│   │       ├── [Image bg + Button]
│   │       └── Text                 TextMeshProUGUI ("⚙")
│   ├── SettingsPanel                RectTransform (96×468, top-right) — hidden
│   │   ├── MusicToggle              [3D Button Container]
│   │   │   ├── Shadow               Image (72×72, bottom-anchored)
│   │   │   └── Face                 Image (72×72, top-anchored) + Button
│   │   │       ├── Highlight        Image (full-width × 4px, top)
│   │   │       └── Text             TextMeshProUGUI ("♫")
│   │   ├── SFXToggle                [same 3D structure]
│   │   ├── VibrationToggle          [same 3D structure]
│   │   ├── RestartButton            [same 3D structure]
│   │   └── ExitButton               [same 3D structure]
│   └── BottomBar                    RectTransform (stretch-bottom, h=100)
│       └── ActionPill               RectTransform (360×80, center)
│           ├── [Image bg]
│           ├── UndoButton           RectTransform (anchored 2-48%)
│           │   ├── [Image bg + Button]
│           │   ├── Text             TextMeshProUGUI ("↶  100")
│           │   └── CoinIcon         Image (18×18, gold)
│           └── ExtraBottleButton    RectTransform (anchored 52-98%)
│               ├── [Image bg + Button]
│               ├── BottleIcon       Image (14×36, white 70%)
│               │   └── Neck         Image (8×8, white 70%)
│               ├── Text             TextMeshProUGUI ("+  500")
│               └── CoinIcon         Image (18×18, gold)
```

---

## 3. RectTransform Values — Every Element

### 3.0 Root: `GameplayHUD`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| pivot | `(0.5, 0.5)` |

### 3.0b `OutsideTapBlocker`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| pivot | `(0.5, 0.5)` |
| sibling index | `0` (first child — behind everything) |
| default active | `false` |
| Image color | `rgba(0, 0, 0, 0)` — fully transparent |
| Image raycastTarget | `true` |
| Button onClick | → `CollapseSettingsIfOpen()` |

### 3.1 `SafeArea`

| Property | Value |
|---|---|
| anchorMin | `(safeArea.x / screenW, safeArea.y / screenH)` |
| anchorMax | `(safeArea.xMax / screenW, safeArea.yMax / screenH)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| pivot | `(0.5, 0.5)` |
| Notes | Recalculated in `LateUpdate()` when `Screen.safeArea` changes |

### 3.2 `TopBar`

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 100)` |
| anchoredPosition | `(0, 0)` |

### 3.3 `CoinDisplay` (child of TopBar)

| Property | Value |
|---|---|
| anchorMin | `(0, 0.5)` |
| anchorMax | `(0, 0.5)` |
| pivot | `(0, 0.5)` |
| anchoredPosition | `(24, 0)` — 24px from left edge |
| sizeDelta | `(180, 48)` |
| Image | ✅ 9-slice rounded sprite, `rgba(0,0,0,0.4)` — see §6 |

### 3.3a `CoinIcon` (child of CoinDisplay)

| Property | Value |
|---|---|
| anchorMin | `(0, 0.5)` |
| anchorMax | `(0, 0.5)` |
| pivot | `(0, 0.5)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(36, 36)` |
| Image color | `ThemeConfig.StarGold` — Night: `#FFB71E` `(1.0, 0.78, 0.2)` / Morning: `#FFD126` `(1.0, 0.82, 0.15)` |
| Image sprite | ✅ circular sprite — see §6.2 |
| raycastTarget | `false` |

### 3.3b `CoinText` (child of CoinDisplay)

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(44, 0)` — left padding past coin icon |
| offsetMax | `(0, 0)` |
| **TMPro** | |
| text | `"0"` (updated via `UpdateCoinDisplay()`) |
| fontSize | `26` |
| fontStyle | `FontStyles.Bold` |
| alignment | `TextAlignmentOptions.MidlineLeft` |
| color | ThemeConfig.StarGold — Night: `#FFB71E` / Morning: `#FFD126` |
| font | `Nunito-Regular SDF` (bold via fontStyle) |

### 3.4 `LevelPill` (child of TopBar)

| Property | Value |
|---|---|
| anchorMin | `(0.58, 0.5)` |
| anchorMax | `(0.58, 0.5)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(280, 60)` |
| Image color | `rgba(0, 0, 0, 0.35)` — `new Color(0, 0, 0, 0.35f)` |
| Image sprite | ✅ 9-slice rounded sprite (radius 18px) — see §6 |

### 3.4a `LevelText` (child of LevelPill)

| Property | Value |
|---|---|
| anchorMin | `(0, 0.35)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(12, 0)` |
| offsetMax | `(-12, 0)` |
| **TMPro** | |
| text | `"Level 1"` (set via `SetLevelInfo()`) |
| fontSize | `28` |
| fontStyle | `FontStyles.Bold` |
| alignment | `TextAlignmentOptions.Center` |
| color | ThemeConfig.TextPrimary — Night: `#E6E1EB` `(0.9, 0.88, 0.92)` / Morning: `#F2EBD9` `(0.95, 0.92, 0.85)` |
| font | `Nunito-Regular SDF` |

### 3.4b `MoveText` (child of LevelPill)

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0.4)` |
| offsetMin | `(12, 0)` |
| offsetMax | `(-12, 0)` |
| **TMPro** | |
| text | `""` (set via `UpdateDisplay()`, e.g. "12 moves") |
| fontSize | `16` |
| fontStyle | normal (no bold) |
| alignment | `TextAlignmentOptions.Center` |
| color | ThemeConfig.TextSecondary — Night: `#9994AD` `(0.6, 0.58, 0.68)` / Morning: `#B39E80` `(0.7, 0.62, 0.5)` |
| font | `Nunito-Regular SDF` |

### 3.5 `SettingsGear` (child of TopBar)

| Property | Value |
|---|---|
| anchorMin | `(1, 0.5)` |
| anchorMax | `(1, 0.5)` |
| pivot | `(1, 0.5)` |
| anchoredPosition | `(-24, 0)` — 24px from right edge |
| sizeDelta | `(88, 88)` |
| Image color | ThemeConfig.ButtonSecondary — Night: `#594770` `(0.35, 0.28, 0.42, 0.88)` / Morning: `#806B4D` `(0.5, 0.42, 0.3, 0.88)` |
| Image sprite | ✅ 9-slice rounded sprite (radius 14px) |
| Button | yes |
| ButtonBounce | yes |

### 3.5a `Text` (child of SettingsGear)

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| **TMPro** | |
| text | `"\u2699"` (⚙ gear) |
| fontSize | `36` |
| alignment | `TextAlignmentOptions.Center` |
| color | `#FFFFFF` `Color.white` |

---

### 3.6 `SettingsPanel` (child of SafeArea)

| Property | Value |
|---|---|
| anchorMin | `(1, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(1, 1)` |
| anchoredPosition | `(-24, -108)` — `-(BarMargin)`, `-(BarHeight + 8)` |
| sizeDelta | `(96, 468)` — width: 72+24, height: (72+6+10)×5 + 16 |
| default active | `false` |
| Image | **NONE** — no background |

### 3.6a Each 3D Settings Button Container

Each of the 5 buttons follows this template. The Y offset starts at `-8` and decreases by `88` per button (72 face + 6 shadow + 10 spacing).

| Button | Y Offset | Face Color | Shadow Color |
|---|---|---|---|
| MusicToggle | `-8` | VividGreen `#2BDE45` | `#0D8C21` |
| SFXToggle | `-96` | VividGreen `#2BDE45` | `#0D8C21` |
| VibrationToggle | `-184` | VividGreen `#2BDE45` | `#0D8C21` |
| RestartButton | `-272` | VividGreen `#2BDE45` | `#0D8C21` |
| ExitButton | `-360` | VividRed `#FF4545` | `#990D0D` |

**Container RectTransform:**

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(0, 1)` |
| pivot | `(0.5, 1)` |
| anchoredPosition | `(48, {yOffset})` — centerX = panelWidth × 0.5 = 48 |
| sizeDelta | `(72, 78)` — SmallButtonSize × (SmallButtonSize + ShadowDepth) |

**Shadow (child of Container):**

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0)` |
| pivot | `(0.5, 0)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(0, 72)` |
| Image color | shadow color (see table above) |
| Image sprite | ✅ 9-slice rounded sprite (radius 14px) |
| raycastTarget | `false` |

**Face (child of Container):**

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(0, 72)` |
| Image color | face color (see table above) |
| Image sprite | ✅ 9-slice rounded sprite (radius 14px) |
| Button | yes |
| ButtonBounce | yes |

**Highlight (child of Face):**

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(0, 4)` |
| Image color | `rgba(255, 255, 255, 0.25)` — `new Color(1, 1, 1, 0.25f)` |
| raycastTarget | `false` |

**Icon Text (child of Face):**

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| **TMPro** | |
| fontSize | `30` |
| alignment | `TextAlignmentOptions.Center` |
| color | `#FFFFFF` (ON state) or `rgba(255,255,255,0.4)` (OFF state) |
| font | `Nunito-Regular SDF` |

**Toggle Icons (ON → OFF):**

| Button | ON Icon | OFF Icon |
|---|---|---|
| Music | `\u266B` (♫) | `\u266B` (♫, dimmed to 40% alpha) |
| SFX | `\u266A` (♪) | `\u2022` (•, dimmed to 40% alpha) |
| Vibration | `\u2058` (⁘) | `\u2022` (•, dimmed to 40% alpha) |
| Restart | `\u21BB` (↻) | — (not a toggle) |
| Exit | `\u2192` (→) | — (not a toggle) |

**OFF State Colors:**

| Property | Value |
|---|---|
| Face Image color | MutedGray `#5E5E5E` — `new Color(0.37f, 0.37f, 0.37f, 1f)` |
| Shadow Image color | MutedGrayDark `#333333` — `new Color(0.2f, 0.2f, 0.2f, 1f)` |
| Icon TMPro color | `rgba(255, 255, 255, 0.4)` — `new Color(1f, 1f, 1f, 0.4f)` |

---

### 3.7 `BottomBar` (child of SafeArea)

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0)` |
| pivot | `(0.5, 0)` |
| sizeDelta | `(0, 100)` |
| anchoredPosition | `(0, 0)` |

### 3.7a `ActionPill` (child of BottomBar)

| Property | Value |
|---|---|
| anchorMin | `(0.5, 0.5)` |
| anchorMax | `(0.5, 0.5)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(360, 80)` |
| Image color | `rgba(0, 0, 0, 0.4)` — `new Color(0, 0, 0, 0.4f)` |
| Image sprite | ✅ 9-slice rounded sprite (radius 22px) — see §6 |

### 3.7b `UndoButton` (child of ActionPill)

| Property | Value |
|---|---|
| anchorMin | `(0.02, 0.08)` |
| anchorMax | `(0.48, 0.92)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| Image color | ThemeConfig.ButtonSecondary — Night: `#594770` `(0.35, 0.28, 0.42, 0.88)` / Morning: `#806B4D` `(0.5, 0.42, 0.3, 0.88)` |
| Image sprite | ✅ 9-slice rounded sprite (radius 16px) |
| Button | yes |
| ButtonBounce | yes |

**UndoButton > Text:**

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin/Max | `(0, 0)` |
| **TMPro** | |
| text | `"\u21B6  100"` (undo arrow + cost) |
| fontSize | `20` |
| alignment | `TextAlignmentOptions.Center` |
| color | `#FFFFFF` `Color.white` |

**UndoButton > CoinIcon:**

| Property | Value |
|---|---|
| anchorMin | `(0.55, 0.5)` |
| anchorMax | `(0.55, 0.5)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(18, 18)` |
| Image color | ThemeConfig.StarGold |
| raycastTarget | `false` |

### 3.7c `ExtraBottleButton` (child of ActionPill)

| Property | Value |
|---|---|
| anchorMin | `(0.52, 0.08)` |
| anchorMax | `(0.98, 0.92)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, 0)` |
| Image color | ThemeConfig.ButtonPrimary — Night: `#38738C` `(0.22, 0.45, 0.55, 0.92)` / Morning: `#40993A` `(0.25, 0.6, 0.35, 0.92)` |
| Image sprite | ✅ 9-slice rounded sprite (radius 16px) |
| Button | yes |
| ButtonBounce | yes |

**ExtraBottleButton > Text:**

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin/Max | `(0, 0)` |
| **TMPro** | |
| text | `"+  500"` (plus + cost) |
| fontSize | `20` |
| alignment | `TextAlignmentOptions.Center` |
| color | `#FFFFFF` |

**ExtraBottleButton > CoinIcon:**

| Property | Value |
|---|---|
| anchorMin | `(0.5, 0.5)` |
| anchorMax | `(0.5, 0.5)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(18, 18)` |
| Image color | ThemeConfig.StarGold |
| raycastTarget | `false` |

**ExtraBottleButton > BottleIcon (body):**

| Property | Value |
|---|---|
| anchorMin | `(0.12, 0.18)` |
| anchorMax | `(0.12, 0.82)` |
| pivot | `(0.5, 0.5)` |
| anchoredPosition | `(0, 0)` |
| sizeDelta | `(14, 0)` — width 14px, height stretches with anchors |
| Image color | `rgba(255, 255, 255, 0.7)` — `new Color(1, 1, 1, 0.7f)` |
| raycastTarget | `false` |

**BottleIcon > Neck:**

| Property | Value |
|---|---|
| anchorMin | `(0.5, 1)` |
| anchorMax | `(0.5, 1)` |
| pivot | `(0.5, 0)` |
| anchoredPosition | `(0, 1)` |
| sizeDelta | `(8, 8)` |
| Image color | `rgba(255, 255, 255, 0.7)` |
| raycastTarget | `false` |

---

## 4. Color Reference (All Hex Codes)

### 4.1 Constant Colors (mood-independent)

| Name | Hex | RGBA Float | Usage |
|---|---|---|---|
| VividGreen | `#2BDE45` | `(0.17, 0.87, 0.27, 1.0)` | Settings ON face |
| VividGreenDark | `#0D8C21` | `(0.05, 0.55, 0.13, 1.0)` | Settings ON shadow |
| MutedGray | `#5E5E5E` | `(0.37, 0.37, 0.37, 1.0)` | Settings OFF face |
| MutedGrayDark | `#333333` | `(0.20, 0.20, 0.20, 1.0)` | Settings OFF shadow |
| VividRed | `#FF4545` | `(1.00, 0.27, 0.27, 1.0)` | Exit button face |
| VividRedDark | `#990D0D` | `(0.60, 0.05, 0.05, 1.0)` | Exit button shadow |
| Highlight | `#FFFFFF40` | `(1.0, 1.0, 1.0, 0.25)` | 3D button top strip |
| Icon ON | `#FFFFFF` | `(1.0, 1.0, 1.0, 1.0)` | Button icon when enabled |
| Icon OFF | `#FFFFFF66` | `(1.0, 1.0, 1.0, 0.4)` | Button icon when disabled |
| Pill BG | `#00000066` | `(0.0, 0.0, 0.0, 0.4)` | Coin pill, action pill |
| Level Pill BG | `#00000059` | `(0.0, 0.0, 0.0, 0.35)` | Level pill |
| Blocker | `#00000000` | `(0.0, 0.0, 0.0, 0.0)` | Outside tap blocker |

### 4.2 ThemeConfig Mood Colors

| ThemeColorType | Night Hex | Night Float | Morning Hex | Morning Float |
|---|---|---|---|---|
| StarGold (coin) | `#FFB71E` | `(1.0, 0.78, 0.2)` | `#FFD126` | `(1.0, 0.82, 0.15)` |
| TextPrimary | `#E6E1EB` | `(0.9, 0.88, 0.92)` | `#F2EBD9` | `(0.95, 0.92, 0.85)` |
| TextSecondary | `#9994AD` | `(0.6, 0.58, 0.68)` | `#B39E80` | `(0.7, 0.62, 0.5)` |
| ButtonPrimary | `#38738C` | `(0.22, 0.45, 0.55, 0.92)` | `#40993A` | `(0.25, 0.6, 0.35, 0.92)` |
| ButtonSecondary | `#594770` | `(0.35, 0.28, 0.42, 0.88)` | `#806B4D` | `(0.5, 0.42, 0.3, 0.88)` |

### 4.3 Mockup Mood-Specific Overrides (from CSS)

| Element | Night | Morning |
|---|---|---|
| Coin pill border | `rgba(255,180,40,0.35)` | `rgba(180,130,30,0.4)` |
| Level pill bg | `rgba(0,0,0,0.45)` | `rgba(60,40,10,0.35)` |
| Action pill bg | `rgba(0,0,0,0.5)` | `rgba(60,40,10,0.4)` |
| Move text | ThemeConfig.TextSecondary | ThemeConfig.TextSecondary |
| Gear btn bg | ThemeConfig.ButtonSecondary | ThemeConfig.ButtonSecondary |

---

## 5. Font Specifications

| Element | Font Asset | Size | Style | Alignment |
|---|---|---|---|---|
| CoinText | `Nunito-Regular SDF` | `26` | Bold | MidlineLeft |
| LevelText | `Nunito-Regular SDF` | `28` | Bold | Center |
| MoveText | `Nunito-Regular SDF` | `16` | Normal | Center |
| Gear icon | `Nunito-Regular SDF` | `36` | Normal | Center |
| Settings icons | `Nunito-Regular SDF` | `30` | Normal | Center |
| Undo text | `Nunito-Regular SDF` | `20` | Normal | Center |
| ExtraBottle text | `Nunito-Regular SDF` | `20` | Normal | Center |

Font loaded from: `Resources.Load<TMP_FontAsset>("Fonts/Nunito-Regular SDF")`
Bold is applied via: `fontStyle = FontStyles.Bold` (not a separate font asset)

---

## 6. Sprite Specifications (✅ IMPLEMENTED via UIShapeUtils)

### 6.1 Rounded Rectangle 9-Slice Sprites

These are white textures tinted by `Image.color`. Use `Image.type = Image.Type.Sliced`.

| Sprite Name | Texture Size | Corner Radius | 9-Slice Borders (L/R/T/B) | Used By |
|---|---|---|---|---|
| `rounded-rect-22` | `64×64` px | 22px | `22, 22, 22, 22` | ActionPill |
| `rounded-rect-20` | `64×64` px | 20px | `20, 20, 20, 20` | CoinDisplay, LevelPill |
| `rounded-rect-16` | `48×48` px | 16px | `16, 16, 16, 16` | UndoButton, ExtraBottleButton |
| `rounded-rect-14` | `48×48` px | 14px | `14, 14, 14, 14` | SettingsGear, 3D button Face, 3D button Shadow |

**Implementation:** All generated via `UIShapeUtils.WhiteRoundedRect(radius, size)` — cached, SDF-based antialiased edges, 9-slice borders auto-calculated. Applied with `Image.type = Image.Type.Sliced` and tinted via `Image.color`.

### 6.2 Circular Sprite (for Coin Icons)

| Property | Value |
|---|---|
| Texture size | `64×64` px |
| Shape | Filled white circle, 1px antialiased edge |
| Usage | CoinIcon (36×36), MiniCoinIcon (18×18) |
| Tinted by | `Image.color = ThemeConfig.StarGold` |

### 6.3 Coin Gradient Sprite (optional polish)

| Property | Value |
|---|---|
| Texture size | `64×64` px |
| Type | Radial gradient |
| Center color | `#FFE866` — `(1.0, 0.91, 0.40)` |
| Edge color | `#F5A623` — `(0.96, 0.65, 0.14)` |
| Usage | CoinIcon Image.sprite (set Image.color = Color.white) |

---

## 7. Animation Specifications

### 7.1 Coin Icon Pulse (✅ IMPLEMENTED)

| Property | Value |
|---|---|
| Type | Looping coroutine (~30fps throttled via `WaitForSeconds(0.033f)`) |
| Target | Glow child Image behind CoinIcon (44×44px, `WhiteCircle` sprite) |
| Color | `ThemeConfig.GetColor(ThemeColorType.StarGold)` (refreshed each tick for mood-awareness) |
| Alpha range | `0.15` ↔ `0.55` |
| Period | `2.5` seconds |
| Easing | `Mathf.SmoothStep(0.15, 0.55, Mathf.PingPong(elapsed / 1.25f, 1f))` |
| Lifecycle | Started in `Create()`, stopped in `OnDestroy()` |

### 7.2 Settings Expand (✅ IMPLEMENTED)

| Property | Value |
|---|---|
| Per-button duration | `0.2s` |
| Stagger delay | `0.05s` between buttons |
| Order | Top → Bottom (Music, SFX, Vibration, Restart, Exit) |
| Scale | `Vector3.zero` → `Vector3.one` |
| Easing | EaseOutBack: `c1=1.70158, c3=c1+1` |

### 7.3 Settings Collapse (✅ IMPLEMENTED)

| Property | Value |
|---|---|
| Per-button duration | `0.15s` |
| Stagger delay | `0.03s` between buttons |
| Order | Bottom → Top (Exit, Restart, Vibration, SFX, Music) |
| Scale | `Vector3.one` → `Vector3.zero` |
| Easing | EaseOutBack (same formula) |
| Post-delay | `0.15s` before `SetActive(false)` |

### 7.4 ButtonBounce (✅ IMPLEMENTED)

| Property | Value |
|---|---|
| Trigger | OnPointerDown / OnPointerUp |
| Scale sequence | `1.0 → 0.9 → 1.05 → 1.0` |
| Duration | `~0.15s` total |

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
    → on complete: _settingsPanel.SetActive(false), _outsideTapBlocker.SetActive(false)
  fire OnSettingsPressed
```

### 8.2 Toggle Persistence

```
Music:   IProgressionManager.MusicEnabled  + IAudioManager.SetMusicEnabled()
SFX:     IProgressionManager.SoundEnabled  + IAudioManager.SetSoundEnabled()
Vibrate: PlayerPrefs.SetInt("VibrationEnabled", 0/1) + PlayerPrefs.Save()
```

### 8.3 Restart / Exit

```
Restart: CollapseSettingsIfOpen() → OnRestartPressed?.Invoke()
Exit:    CollapseSettingsIfOpen() → OnExitPressed?.Invoke() → GameplayManager.GoBackToHub()
```

### 8.4 Disabled Button States

| Button | Disabled When | Visual |
|---|---|---|
| Undo | `!canAfford \|\| !hasUndos` | `interactable = false`, CanvasGroup alpha 0.4, `Transition.None` ✅ |
| Extra Bottle | `!canAfford \|\| !hasRemaining` | `interactable = false`, CanvasGroup alpha 0.4, `Transition.None` ✅ |

---

## 9. Constants Reference (from code)

```csharp
private const float ButtonSize = 88f;            // Gear button
private const float SmallButtonSize = 72f;        // Settings panel buttons (face)
private const float ButtonPadding = 16f;          // General padding
private const float BarHeight = 100f;             // Top bar & bottom bar
private const float BarMargin = 24f;              // Left/right margin
private const float SettingsButtonSpacing = 10f;  // Gap between 3D buttons
private const float ShadowDepth = 6f;             // 3D shadow offset

// Derived
float panelWidth  = SmallButtonSize + 24f;                               // = 96
float panelHeight = (SmallButtonSize + ShadowDepth + SettingsButtonSpacing) * 5 + 16f;  // = 456
float stepY       = SmallButtonSize + ShadowDepth + SettingsButtonSpacing; // = 88
float btnCenterX  = panelWidth * 0.5f;                                    // = 48
```

---

## 10. Key Files

| File | Path | Role |
|---|---|---|
| GameplayHUD.cs | `Assets/Scripts/Game/UI/Components/GameplayHUD.cs` | All HUD construction + logic |
| GameplayManager.cs | `Assets/Scripts/Game/Puzzle/GameplayManager.cs` | Creates HUD, wires events |
| ThemeConfig.cs | `Assets/Scripts/Game/UI/ThemeConfig.cs` | Colors, fonts, gradients |
| UIShapeUtils.cs | `Assets/Scripts/Game/UI/Components/UIShapeUtils.cs` | Cached procedural sprites (rounded rects, circles) |
| ButtonBounce.cs | `Assets/Scripts/Game/UI/Components/ButtonBounce.cs` | Press animation component |

---

## 11. HTML Mockup — Exact CSS Specifications

Source: [gameplay-hud-mockup.html](gameplay-hud-mockup.html)
Reference device: 360×780px phone frame (maps to 1080×2340 at 3x)

All values below are CSS pixels from the mockup. To convert to Unity reference pixels (1080×1920): **multiply by 3**.

---

### 11.1 Coin Icon

```css
.coin-icon {
    width: 28px;                    /* Unity: 84px */
    height: 28px;                   /* Unity: 84px */
    background: radial-gradient(circle at 38% 32%, #ffe866, #f5a623);
    border-radius: 50%;
    border: 2px solid #cc8800;      /* Unity: 6px */
    font-size: 11px;                /* Unity: 33px — "$" symbol inside */
    font-weight: 800;
    color: #8B6914;                 /* dark gold text */
    box-shadow: 0 0 8px rgba(255,200,50,0.35);
    animation: coinPulse 2.5s ease-in-out infinite;
}
```

**Key details:**
- Shape: perfect circle (`border-radius: 50%`)
- Background: **radial gradient** — center offset to upper-left (`38% 32%`), bright gold `#ffe866` → amber `#f5a623`
- Border: 2px solid `#cc8800` (dark amber)
- Content: **"$" text** centered inside, 11px, font-weight 800, color `#8B6914`
- Glow: `box-shadow 0 0 8px rgba(255,200,50,0.35)` — static glow ring
- Pulse animation: `box-shadow` oscillates `6px → 14px` at `rgba(255,200,50, 0.25→0.55)`

**Mini coin (in action buttons):**
```css
.coin-icon-small {
    width: 16px;                    /* Unity: 48px */
    height: 16px;                   /* Unity: 48px */
    background: radial-gradient(circle at 38% 32%, #ffe866, #f5a623);
    border-radius: 50%;
    border: 1.5px solid #cc8800;    /* Unity: 4.5px */
    font-size: 7px;                 /* Unity: 21px — "$" symbol */
    font-weight: 800;
    color: #8B6914;
    box-shadow: 0 0 4px rgba(255,200,50,0.3);
}
```

---

### 11.2 Coin Display (pill container)

```css
.coin-display {
    display: flex;
    align-items: center;
    gap: 7px;                       /* Unity: 21px between icon and text */
    background: rgba(0,0,0,0.4);
    padding: 6px 14px 6px 8px;     /* Unity: 18px 42px 18px 24px */
    border-radius: 20px;            /* Unity: 60px */
    border: 1.5px solid rgba(255,200,50,0.4);  /* golden border */
    box-shadow: 0 0 10px rgba(255,200,50,0.1),
                inset 0 1px 0 rgba(255,255,255,0.08);
}
```

**Coin amount text:**
```css
.coin-amount {
    color: #ffe066;
    font-weight: 700;              /* Bold */
    font-size: 17px;               /* Unity: 51px */
    text-shadow: 0 0 6px rgba(255,220,50,0.2);
}
```

---

### 11.3 Level Pill

```css
.level-pill {
    position: absolute;
    left: 58%;
    transform: translateX(-50%);    /* visually centered between coin and gear */
    background: rgba(0,0,0,0.4);
    padding: 6px 22px;             /* Unity: 18px 66px */
    border-radius: 18px;           /* Unity: 54px */
    border: 1px solid rgba(255,255,255,0.06);
    text-align: center;
    box-shadow: 0 2px 8px rgba(0,0,0,0.2);
}
```

**Level text:**
```css
.level-text {
    color: rgba(255,245,230,0.95);  /* warm white */
    font-weight: 700;               /* Bold */
    font-size: 18px;                /* Unity: 54px */
    text-shadow: 0 1px 4px rgba(0,0,0,0.3);
}
```

**Move counter (below level text):**
```css
.move-text {
    color: rgba(180,170,200,0.7);   /* muted lavender */
    font-weight: 500;               /* Medium */
    font-size: 12px;                /* Unity: 36px */
    margin-top: 1px;                /* Unity: 3px gap below level text */
}
```

**Morning mood overrides:**
```css
.morning .level-pill {
    background: rgba(60,40,10,0.35);
    border-color: rgba(255,255,255,0.1);
}
.morning .move-text {
    color: rgba(120,95,55,0.8);     /* warm brown instead of lavender */
}
```

---

### 11.4 Settings Gear Button

```css
.settings-btn {
    width: 46px;                    /* Unity: 138px */
    height: 46px;                   /* Unity: 138px */
    border-radius: 14px;            /* Unity: 42px */
    font-size: 22px;                /* Unity: 66px — gear icon */
    color: rgba(255,255,255,0.85);
    border: 1.5px solid rgba(255,255,255,0.1);
    box-shadow: 0 0 8px rgba(140,100,220,0.15),
                inset 0 1px 0 rgba(255,255,255,0.1);
}
```

**Night theme:**
```css
.night .settings-btn {
    background: linear-gradient(135deg, rgba(90,70,150,0.7), rgba(60,40,120,0.6));
}
```

**Morning theme:**
```css
.morning .settings-btn {
    background: linear-gradient(135deg, rgba(120,100,60,0.7), rgba(90,75,40,0.6));
}
```

---

### 11.5 Settings Panel — 3D Buttons

**No panel background.** Buttons float directly below gear.

```css
.settings-panel {
    position: absolute;
    top: 106px;                     /* below gear button */
    right: 18px;
    display: flex;
    flex-direction: column;
    gap: 10px;                      /* Unity: 30px between buttons */
    padding: 0;
}
```

**Each 3D button:**
```css
.settings-panel .s-btn {
    width: 50px;                    /* Unity: 150px */
    height: 50px;                   /* Unity: 150px */
    border-radius: 14px;            /* Unity: 42px */
    font-size: 20px;                /* Unity: 60px — icon size */
    color: #fff;
    border: none;
    box-shadow:
        0 4px 0 rgba(0,0,0,0.35),       /* 3D depth (Unity: 12px) */
        0 6px 12px rgba(0,0,0,0.3),      /* drop shadow */
        inset 0 2px 0 rgba(255,255,255,0.2),  /* top highlight */
        inset 0 -1px 0 rgba(0,0,0,0.15);     /* bottom inner edge */
}
```

**Press state:**
```css
.settings-panel .s-btn:active {
    transform: translateY(3px) scale(0.97);
    box-shadow:
        0 1px 0 rgba(0,0,0,0.35),
        0 2px 6px rgba(0,0,0,0.2),
        inset 0 2px 0 rgba(255,255,255,0.1),
        inset 0 -1px 0 rgba(0,0,0,0.1);
}
```

**ON state (vivid green):**
```css
.s-btn.on {
    background: linear-gradient(180deg, #44ff66 0%, #22dd44 35%, #10cc30 65%, #08b825 100%);
    box-shadow:
        0 4px 0 #069920,                    /* green 3D depth */
        0 6px 16px rgba(20,220,60,0.45),     /* green glow */
        0 0 20px rgba(40,255,80,0.2),        /* outer glow */
        inset 0 2px 0 rgba(255,255,255,0.4), /* top highlight */
        inset 0 -1px 0 rgba(0,0,0,0.1);
    text-shadow: 0 1px 3px rgba(0,0,0,0.25);
    color: #fff;
}
```

**OFF state (gray):**
```css
.s-btn.off {
    background: linear-gradient(180deg, #7a7a7a 0%, #5e5e5e 50%, #484848 100%);
    box-shadow:
        0 4px 0 #333,
        0 6px 10px rgba(0,0,0,0.3),
        inset 0 2px 0 rgba(255,255,255,0.1),
        inset 0 -1px 0 rgba(0,0,0,0.15);
    color: rgba(255,255,255,0.4);           /* icon dimmed to 40% */
}
```

**Exit button (vivid red):**
```css
.s-btn.exit {
    background: linear-gradient(180deg, #ff4444 0%, #ee2020 35%, #dd1010 65%, #cc0505 100%);
    box-shadow:
        0 4px 0 #990505,
        0 6px 16px rgba(255,30,30,0.45),
        0 0 20px rgba(255,50,50,0.2),
        inset 0 2px 0 rgba(255,255,255,0.3),
        inset 0 -1px 0 rgba(0,0,0,0.15);
    text-shadow: 0 1px 3px rgba(0,0,0,0.3);
}
```

**Icon Font:** Material Icons Round (Google)
```html
<link href="https://fonts.googleapis.com/icon?family=Material+Icons+Round" rel="stylesheet">
```

**Icon mapping:**

| Button | ON Icon | OFF Icon | HTML |
|---|---|---|---|
| Music | `music_note` | `music_off` (built-in slash) | `<span class="material-icons-round">music_note</span>` |
| SFX | `volume_up` | `volume_off` (built-in X) | `<span class="material-icons-round">volume_up</span>` |
| Vibration | `vibration` | `mobile_off` (built-in slash) | `<span class="material-icons-round">vibration</span>` |
| Restart | `replay` | — | `<span class="material-icons-round">replay</span>` |
| Exit | `exit_to_app` | — | `<span class="material-icons-round">exit_to_app</span>` |

---

### 11.6 Bottom Action Panel

**Container pill:**
```css
.action-pill {
    display: flex;
    align-items: center;
    gap: 10px;                      /* Unity: 30px between undo and extra bottle */
    background: rgba(0,0,0,0.45);
    padding: 8px 10px;             /* Unity: 24px 30px */
    border-radius: 22px;           /* Unity: 66px */
    border: 1px solid rgba(255,255,255,0.06);
    box-shadow: 0 4px 16px rgba(0,0,0,0.3);
}
```

**Night mood:**
```css
.night .action-pill {
    background: rgba(0,0,0,0.5);
    border-color: rgba(255,255,255,0.08);
}
```

**Morning mood:**
```css
.morning .action-pill {
    background: rgba(60,40,10,0.4);
    border-color: rgba(255,255,255,0.1);
}
```

---

### 11.7 Action Buttons (Undo & Extra Bottle)

**Shared button style:**
```css
.action-btn {
    display: flex;
    align-items: center;
    gap: 5px;                       /* Unity: 15px between elements */
    padding: 10px 18px;            /* Unity: 30px 54px */
    border-radius: 16px;           /* Unity: 48px */
    font-size: 15px;               /* Unity: 45px */
    font-weight: 600;              /* SemiBold */
    color: #fff;
    border: 1px solid rgba(255,255,255,0.08);
}
```

**Undo button:**
```css
.action-btn.undo {
    background: linear-gradient(135deg, rgba(90,75,130,0.8), rgba(70,55,110,0.7));
    box-shadow: 0 0 8px rgba(140,100,220,0.15);
}
```

**Extra bottle button:**
```css
.action-btn.extra {
    background: linear-gradient(135deg, rgba(40,120,60,0.8), rgba(30,100,50,0.7));
    box-shadow: 0 0 8px rgba(80,200,100,0.15);
}
```

**Action icon (undo arrow, "+"):**
```css
.action-btn .action-icon {
    font-size: 18px;                /* Unity: 54px */
    line-height: 1;
}
```

**Cost text (gold number):**
```css
.action-btn .cost {
    color: rgba(255,220,100,0.9);   /* gold */
    font-size: 13px;                /* Unity: 39px */
    font-weight: 600;               /* SemiBold */
}
```

**Cost group (mini coin + number):**
```css
.action-btn .cost-group {
    display: flex;
    align-items: center;
    gap: 3px;                       /* Unity: 9px between coin and number */
}
```

**Disabled state:**
```css
.action-btn.disabled {
    opacity: 0.4;
    pointer-events: none;
}
```

---

### 11.8 Bottle Icon (in Extra Bottle button)

```css
.action-btn .bottle-icon {
    display: inline-block;
    width: 12px;                    /* Unity: 36px — bottle body */
    height: 26px;                   /* Unity: 78px — bottle body */
    border: 1.5px solid rgba(255,255,255,0.7);  /* Unity: 4.5px */
    border-radius: 2px 2px 5px 5px; /* top square, bottom rounded */
    position: relative;
    margin-right: 2px;             /* Unity: 6px gap to "+" */
}
```

**Bottle neck (::before pseudo):**
```css
.action-btn .bottle-icon::before {
    content: '';
    position: absolute;
    top: -4px;                      /* Unity: -12px above body */
    left: 50%;
    transform: translateX(-50%);
    width: 7px;                     /* Unity: 21px — neck width */
    height: 4px;                    /* Unity: 12px — neck height */
    border-radius: 2px 2px 0 0;    /* top rounded, bottom flat */
    border: 1.5px solid rgba(255,255,255,0.7);
    border-bottom: none;            /* open bottom connects to body */
}
```

---

### 11.9 Bottom Bar Button HTML Structure

**Undo button:**
```html
<div class="action-btn undo">
    <span class="action-icon">↶</span>          <!-- &#8630; undo arrow -->
    <span class="cost-group">
        <span class="coin-icon-small">$</span>   <!-- mini gold coin with "$" -->
        <span class="cost">100</span>             <!-- gold cost number -->
    </span>
</div>
```

**Extra bottle button:**
```html
<div class="action-btn extra">
    <span class="bottle-icon"></span>             <!-- bottle outline (CSS shape) -->
    <span class="action-icon">+</span>            <!-- plus sign -->
    <span class="cost-group">
        <span class="coin-icon-small">$</span>   <!-- mini gold coin with "$" -->
        <span class="cost">500</span>             <!-- gold cost number -->
    </span>
</div>
```

**Layout order (left to right):**
- Undo: `[↶ arrow] [🪙 $] [100]`
- Extra: `[🍶 bottle] [+] [🪙 $] [500]`

---

### 11.10 Top Bar Layout

```css
.top-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 52px 18px 8px 18px;   /* Unity: 156px top (status bar), 54px sides, 24px bottom */
    z-index: 10;
    position: relative;
}
```

---

### 11.11 Font Stack

```css
font-family: 'Fredoka', sans-serif;
```
```html
<link href="https://fonts.googleapis.com/css2?family=Fredoka:wght@400;500;600;700&display=swap" rel="stylesheet">
```

**Weight mapping:**
| CSS weight | Name | Used by |
|---|---|---|
| `400` | Regular | — |
| `500` | Medium | Move counter |
| `600` | SemiBold | Action button text, cost numbers |
| `700` | Bold | Coin amount, level text |
| `800` | ExtraBold | Coin "$" symbol |

**Note:** Mockup uses **Fredoka** (rounded, playful). Implementation uses **Nunito** (also rounded). Both are Google Fonts with similar feel — Nunito is already in the project.

---

### 11.12 Current Code vs HTML — Differences to Fix

| # | Element | HTML Mockup | Current Code | Action |
|---|---|---|---|---|
| 1 | Coin icon size | 28px (CSS) = **84px Unity** | 36px | **Increase to ~84px** or scale to match visual weight |
| 2 | Coin amount text | `font-size: 17px` = **51px Unity**, `font-weight: 700` | fontSize 26, Bold | **Increase to ~51px** |
| 3 | Level pill bg | `rgba(0,0,0,0.4)` both moods | Night: 0.45, Morning: different | **Match to `rgba(0,0,0,0.4)`** |
| 4 | Level text | `font-size: 18px` = **54px Unity**, `font-weight: 700` | fontSize 28, Bold | **Increase to ~54px** |
| 5 | Move counter | `font-size: 12px` = **36px**, `margin-top: 1px` | fontSize 16, present | **Already there** — verify size matches |
| 6 | Action pill padding | `padding: 8px 10px` = **24px 30px Unity** | sizeDelta (360, 80) | **Increase pill height** to accommodate larger content |
| 7 | Bottle icon | `width: 12px; height: 26px` = **36×78px Unity** | 14px wide, anchor-stretch | **Increase to match** |
| 8 | Undo icon | `font-size: 18px` = **54px Unity** | fontSize 20 | **Add separate icon elements** per HTML structure |
| 9 | Cost text | `font-size: 13px` = **39px**, `font-weight: 600` | fontSize 20 | **Increase to ~39px, SemiBold** |

**Note on 3x scaling:** The HTML mockup renders at 360px width which maps to 1080px Unity reference. The conversion factor is `×3` for all pixel values.
