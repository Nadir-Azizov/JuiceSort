# Settings Panel — UI Specification v3 (Implementation-Ready)

**Screen:** Settings (full-screen, accessed from Hub gear button)
**Source of Truth:** [settings-panel-mockup.html](settings-panel-mockup.html) + verified implementation in `SettingsScreen.cs`
**Conversion:** HTML renders at 400px max-width = 1080px Unity reference. **All values = CSS × 2.7.**
**Implementation:** uGUI procedural C# via `SettingsScreen.Create()` static factory (same pattern as HubScreen, GameplayHUD)
**Status:** Implementation diverged from spec during visual polish (2026-03-31). Spec values are original CSS×2.7 targets; actual code uses tuned values for visual match with HTML mockup. See SettingsScreen.cs for current implementation truth.

---

## 1. Canvas & Scaler

| Property | Value |
|---|---|
| Canvas render mode | `RenderMode.ScreenSpaceOverlay` |
| CanvasScaler mode | `ScaleMode.ScaleWithScreenSize` |
| Reference resolution | `1080 × 1920` |
| Match width or height | `0.5` |
| Canvas sortingOrder | `10` |
| Pixels per unit | `100` |

---

## 2. GameObject Hierarchy

```
SettingsScreen                                Canvas + CanvasScaler + GraphicRaycaster + CanvasGroup
├── Background                                Image (stretch-all, #2d2280, raycastTarget=true)
├── Header                                    RectTransform (stretch-top, h=184)
│   ├── HeaderBg                              Image (stretch-all, #6b4cb8)
│   ├── HeaderHighlight                       Image (stretch-top, h=5, rgba(255,255,255,0.15))
│   ├── GoldSeparator                         Image (stretch-bottom, h=11, #7a5510)
│   ├── TitleText                             TMPro ("Settings", 103px bold, underlay shadow)
│   └── CloseButton                           RectTransform (146×146, right-38px)
│       ├── GoldRing                          Image (circle, #f5dc68) + Mask
│       │   └── Bevel                         Image (gradient: clear→#786010)
│       ├── RedRing                           Image (130×130, #cc2020)
│       ├── RedFace                           Image (114×114, #d42828)
│       │   └── Gloss                         Image (top 50%, 15% H-inset, rgba(255,255,255,0.25))
│       ├── XLabel                            TMPro ("✕", 59px bold)
│       ├── Image (clear)                     ← raycast target for Button
│       ├── Button                            onClick → OnClose()
│       └── ButtonBounce
├── ScrollArea                                ScrollRect (below header, vertical, elastic)
│   └── Viewport                              Image(clear) + Mask
│       └── Content                           VLayoutGroup (spacing=49, padding 54/43/108/43) + CSF
│           ├── NotifCard                     [CARD] + LayoutElement
│           │   └── NotifRow                  RectTransform (padded CardPadding)
│           │       ├── BellIcon              Image (59×59, circle placeholder)
│           │       ├── NotifText             TMPro ("Notifications:", 59px bold)
│           │       └── ToggleSwitch          RectTransform (254×119, right-anchored)
│           │           ├── TrackBorder       Image (pill, dynamic color)
│           │           ├── Track             Image (pill, dynamic color)
│           │           ├── ToggleLabel       TMPro ("ON"/"OFF", 38px, shifts L/R alignment)
│           │           ├── Knob              Image (92×92, circle, travel ±73)
│           │           │   └── Highlight     Image (top half, rgba(255,255,255,0.5))
│           │           ├── Image (clear)     ← raycast target
│           │           ├── Button            onClick → OnNotifToggle()
│           │           └── ButtonBounce
│           ├── AudioCard                     [CARD] + LayoutElement
│           │   └── AudioGrid                 HLayoutGroup (expandWidth=true)
│           │       ├── SoundItem             VLayoutGroup (spacing=27)
│           │       │   ├── SoundLabel        TMPro ("Sound", 54px, raycast=false)
│           │       │   └── SoundBtn          [3D-BTN 232×232 green + gold bevel]
│           │       ├── HapticItem            (same structure)
│           │       └── MusicItem             (same structure)
│           ├── SaveCard                      [CARD] + LayoutElement
│           │   └── Inner                     VLayoutGroup (spacing=43)
│           │       ├── SaveTitle             TMPro ("Save Your Progress", 65px, raycast=false)
│           │       └── GoogleBtn             [3D-BTN stretch blue + gold bevel]
│           ├── SupportWrapper                HLayoutGroup (center)
│           │   └── SupportBtn               [3D-BTN 72% green + gold bevel]
│           └── LegalRow                      HLayoutGroup (spacing=54, padding L/R=22)
│               ├── TermsBtn                  [3D-BTN purple + purple bevel, NO gold ring]
│               └── PrivacyBtn                [3D-BTN purple + purple bevel, NO gold ring]
```

---

## 3. Visual Effect Translation Rules

| CSS Effect | Unity Equivalent |
|---|---|
| `linear-gradient(180deg, A, B, C)` | Flat color fallback = midtone. Ideal: `ThemeConfig.CreateGradientSprite(top, bottom)` |
| `box-shadow: 0 Ypx Zpx rgba(...)` (outer) | Shadow child Image behind element, offset (0, -Y×2.7), `UIShapeUtils.Glow()` |
| `box-shadow: inset 0 Ypx Zpx rgba(...)` | Highlight child Image, stretch-top, height Y×2.7, rgba color |
| `border: Xpx solid #color` | Border-ring child Image (see §3.BORDERS) |
| `-webkit-text-stroke: Xpx rgba(...)` | TMPro Outline: width = X×2.7, color = rgba |
| `text-shadow: 0 Ypx Zpx rgba(...)` | TMPro Underlay: offsetY = Y×2.7, softness = Z×0.1 |
| `border-radius: Xpx` | `UIShapeUtils.WhiteRoundedRect(radius, size)` where radius = sprite-texture px (NOT ×2.7) |
| `transition: all 0.3s` | Coroutine-based animation |

---

## 3.BORDERS — Border Strategy

Two techniques used:

### Technique 1: Border-ring child Image (cards, toggles)

Same as GameplayHUD — oversized child Image behind fill.

```
BorderRing (child, sibling index 0 — behind Fill)
├── Image: WhiteRoundedRect (same radius, slightly larger)
│   sprite = UIShapeUtils.WhiteRoundedRect(radius, size)
│   type = Sliced, color = border color, raycastTarget = false
├── RectTransform: same anchor, sizeDelta += (borderWidth×2, borderWidth×2)
```

| Element | Border Width | Border Color | Sprite Radius |
|---|---|---|---|
| Card | 8 | `#1e1862` | 20+4=24 |
| Toggle ON | 8 | `#166622` | 22 |
| Toggle OFF | 8 | `#333333` | 22 |

### Technique 2: Mask + Gradient Bevel (gold rings, purple outer)

Simulates top-to-bottom gradient bevel on shaped sprites. Base Image uses the **bright** color; a `Mask` component clips a gradient overlay to the sprite shape.

```csharp
// AddBevelOverlay(parent, darkColor):
parent.AddComponent<Mask>().showMaskGraphic = true;  // clips children to sprite shape
var bevel = stretch-all child Image
bevel.sprite = ThemeConfig.CreateGradientSprite(Color.clear, darkColor);
bevel.raycastTarget = false;
```

| Element | Base Color (bright) | Gradient Dark | Sprite Radius |
|---|---|---|---|
| GoldRing (close) | `#f5dc68` | `#786010` | circle |
| GoldRing (audio) | `#f5dc68` | `#786010` | 21 |
| GoldRing (google) | `#f5dc68` | `#786010` | 32 |
| GoldRing (support) | `#f5dc68` | `#786010` | 22 |
| PurpleOuter (legal) | `#a090d8` | `#382890` | 16 |

**Do NOT use Unity's `Outline` component.**
**Sprite radius values are in sprite-texture pixels, NOT Unity reference pixels.**

---

## 3.3D — 3D Button Template

All 3D buttons share this nested structure. Colors vary per button type (see §5.3).

**CRITICAL: Every Button needs a transparent `Image` component on the same GO for raycast targeting.**

```
ButtonRoot              LayoutElement (fixed size)
├── GoldRing            Image (full size, bright gold #f5dc68) + Mask
│   └── Bevel           Image (stretch-all, gradient clear→#786010)
├── ColorBorder         Image (inset 8px each side, darker color)
│                       sprite = WhiteRoundedRect(borderRadius, size), Sliced
├── ColorFace           Image (inset 13px from gold ring)
│                       sprite = WhiteRoundedRect(faceRadius, size), Sliced
│   └── GlossOverlay    Image (top 45-48% of face, semi-transparent white)
│                       anchorMin=(0, 0.52-0.55), anchorMax=(1, 1)
│                       sprite = same as face, Sliced
│                       color = rgba(255,255,255, alpha) — see §5.3
│   └── Content         Icon Image + StrikeLine (if audio)
├── Image (clear)       ← raycast target for Button component
├── Button              onClick → handler
└── ButtonBounce

Gloss alpha by type:
  audio/google = 0.3, support = 0.25, legal = 0.15, close = 0.25
```

**No gold ring variant (Legal only):** PurpleOuter replaces GoldRing, uses same Mask+gradient bevel technique with `#a090d8` base → `#382890` dark.

---

## 4. Element Specifications

### 4.0 Root: `SettingsScreen`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin / offsetMax | `(0, 0)` |

### 4.0b `Background`

CSS: `.phone { background: linear-gradient(180deg, #3a2e92 0%, #2d2280 40%, #261d70 100%) }`

| Property | Value |
|---|---|
| anchorMin / anchorMax | `(0,0)` / `(1,1)` |
| offsetMin / offsetMax | `(0,0)` |

| Visual | Value | CSS Source |
|---|---|---|
| Image color | `#2d2280` | midtone of gradient |
| **Ideal:** gradient sprite | `ThemeConfig.CreateGradientSprite(#3a2e92, #261d70)` | linear-gradient(180deg) |
| sprite type | `Simple` | |
| raycastTarget | `true` | blocks taps through to gameplay |

---

### 4.1 `Header`

CSS: `.header { padding: 20px 20px 24px; background: linear-gradient(180deg, #8b6ad4 0%, #6b4cb8 60%, #5a3da6 100%); border-bottom: 4px solid #7a5510; position: relative }`

Derivation: padding-top(20×2.7=54) + title(~38×2.7=103 font height) + padding-bottom(24×2.7=65) ≈ **184px**. Close button (146px) fits within this because it's absolutely positioned.

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| pivot | `(0.5, 1)` |
| sizeDelta | `(0, 184)` |

#### 4.1a `HeaderBg`

| Visual | Value |
|---|---|
| anchorMin / anchorMax | `(0,0)` / `(1,1)` |
| Image color | `#6b4cb8` |
| **Ideal:** gradient | top `#8b6ad4` → bottom `#5a3da6` |

#### 4.1b `HeaderHighlight`

CSS: `box-shadow: inset 0 2px 3px rgba(255,255,255,0.15)`

| Property | Value |
|---|---|
| anchorMin | `(0, 1)` |
| anchorMax | `(1, 1)` |
| sizeDelta | `(0, 5)` |
| Image color | `rgba(255,255,255,0.15)` |
| raycastTarget | `false` |

#### 4.1c `GoldSeparator`

CSS: `border-bottom: 4px solid #7a5510`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 0)` |
| pivot | `(0.5, 0)` |
| sizeDelta | `(0, 11)` |
| Image color | `#7a5510` |
| raycastTarget | `false` |

#### 4.1d `TitleText`

CSS: `.header h1 { font-size: 38px; font-weight: 700; -webkit-text-stroke: 4px rgba(0,0,0,0.75); text-shadow: 0 2px 0 rgba(0,0,0,0.35) }`

| Property | Value |
|---|---|
| anchorMin / anchorMax | `(0,0)` / `(1,1)` |
| offsetMin / offsetMax | `(0,0)` |

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Settings"` | |
| fontSize | `103` | 38 × 2.7 |
| fontStyle | `Bold` | weight 700 |
| alignment | `Center` | `justify-content: center` |
| color | `#FFFFFF` | |
| font | `Nunito-Regular SDF` | |
| outlineWidth | `0.25` | (TMPro 0-1 range, maps to ~4px CSS stroke) |
| outlineColor | `rgba(0,0,0,0.75)` | |
| raycastTarget | `false` | |

**Underlay (text shadow) — requires material instance:**

```csharp
var mat = new Material(title.fontSharedMaterial);
mat.EnableKeyword("UNDERLAY_ON");
mat.SetFloat("_UnderlayOffsetY", -5f);
mat.SetFloat("_UnderlaySoftness", 0.3f);
mat.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.35f));
title.fontMaterial = mat;
```

---

### 4.2 `CloseButton`

CSS: `.close-btn { position: absolute; right: 14px; width: 54px; height: 54px; border-radius: 50%; padding: 3px }`

| Property | Value | CSS × 2.7 |
|---|---|---|
| anchorMin | `(1, 0.5)` | right-aligned, vertically centered |
| anchorMax | `(1, 0.5)` | |
| pivot | `(1, 0.5)` | |
| anchoredPosition | `(-38, 0)` | right: 14 × 2.7 |
| sizeDelta | `(146, 146)` | 54 × 2.7 |

**Layer 1 — GoldRing (146×146) with Bevel:**

CSS: `background: linear-gradient(180deg, #f5dc68 0%, #c8a030 50%, #806010 100%); padding: 3px`

| Visual | Value |
|---|---|
| sprite | `UIShapeUtils.WhiteCircle(64)` |
| color | `#f5dc68` (bright gold — bevel overlay darkens bottom) |
| Mask | `showMaskGraphic = true` |
| Bevel child | `ThemeConfig.CreateGradientSprite(Color.clear, #786010)`, stretch-all |

**Layer 2 — RedRing (inset 8px = 130×130):**

CSS: `.close-red-ring { border-radius: 50%; background: linear-gradient(180deg, #ff7070 0%, #cc2020 50%, #881010 100%); padding: 3px }`

| Visual | Value |
|---|---|
| anchorMin / anchorMax | centered |
| sizeDelta | `(130, 130)` | 48 × 2.7 |
| sprite | `UIShapeUtils.WhiteCircle(64)` |
| color | `#cc2020` |

**Layer 3 — RedFace (inset 8px from ring = 114×114):**

CSS: `.close-face { border-radius: 50%; background: linear-gradient(180deg, #f04848 0%, #d42828 40%, #b81818 100%) }`

| Visual | Value |
|---|---|
| sizeDelta | `(114, 114)` | 42 × 2.7 |
| sprite | `UIShapeUtils.WhiteCircle(64)` |
| color | `#d42828` |

**GlossOverlay (top 50% of face, 15% horizontal inset):**

CSS: `.close-face::before { top: 0; left: 15%; right: 15%; height: 50%; background: linear-gradient(180deg, rgba(255,255,255,0.5), transparent); border-radius: 50% }`

| Visual | Value |
|---|---|
| anchorMin | `(0.15, 0.5)` |
| anchorMax | `(0.85, 1)` |
| offsetMin / offsetMax | `(0, 0)` |
| sprite | `UIShapeUtils.WhiteCircle(64)` |
| color | `rgba(255,255,255,0.25)` |
| raycastTarget | `false` |

**XLabel:**

CSS: `.close-face span { font-size: 22px; font-weight: 700; -webkit-text-stroke: 3px rgba(0,0,0,0.65) }`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"✕"` | |
| fontSize | `59` | 22 × 2.7 |
| fontStyle | `Bold` | weight 700 |
| alignment | `Center` | |
| color | `#FFFFFF` | |
| outlineWidth | `0.2` | ~3px stroke |
| outlineColor | `rgba(0,0,0,0.65)` | |
| raycastTarget | `false` | |

---

### 4.3 `ScrollArea`

| Property | Value |
|---|---|
| anchorMin | `(0, 0)` |
| anchorMax | `(1, 1)` |
| offsetMin | `(0, 0)` |
| offsetMax | `(0, -184)` | below header |
| ScrollRect | vertical=true, horizontal=false, movementType=Elastic, scrollSensitivity=40 |
| scrollbar | none (no Scrollbar component) |

#### 4.3a `ScrollContent`

CSS: `.content { padding: 20px 16px 40px; gap: 18px; flex-direction: column }`

| Component | Value | CSS × 2.7 |
|---|---|---|
| **VerticalLayoutGroup** | | |
| spacing | `49` | gap 18 × 2.7 |
| padding T/B/L/R | `54, 108, 43, 43` | 20/40/16/16 × 2.7 |
| childAlignment | `UpperCenter` | |
| childForceExpandWidth | `true` | |
| childForceExpandHeight | `false` | |
| childControlWidth | `true` | |
| childControlHeight | `true` | |
| **ContentSizeFitter** | | |
| verticalFit | `PreferredSize` | |

---

### 4.4 `NotifCard`

Uses §3.BORDERS card template.

CSS: `.card { border-radius: 20px; padding: 20px; border: 3px solid #1e1862; background: linear-gradient(180deg, #262078 0%, #2c2585 50%, #302a90 100%) }`

| Visual | Value | CSS × 2.7 |
|---|---|---|
| Fill sprite | `UIShapeUtils.WhiteRoundedRect(20, 64)` Sliced | `border-radius: 20px` |
| Fill color | `#2c2585` | midtone |
| Border-ring | 8px, `#1e1862`, radius=24 | 3 × 2.7 ≈ 8 |
| Padding | `54` all sides | 20 × 2.7 |
| TopShadow | child Image, stretch-top, h=**8**, `rgba(0,0,0,0.15)`, raycast=false | CSS `inset` shadow at top |
| BottomHighlight | child Image, stretch-bottom, h=**5**, `rgba(255,255,255,0.04)`, raycast=false | subtle light edge |

#### 4.4a `NotifRow`

CSS: `.notif-row { display: flex; align-items: center; justify-content: space-between }`

No LayoutGroup — manual left/right anchoring (same pattern as TopBar in HUD spec).

#### 4.4b Notification Row Children (manual anchor positioning, no LayoutGroup)

**BellIcon (left-anchored):**

| Property | Value |
|---|---|
| anchorMin | `(0, 0.5)` |
| anchorMax | `(0, 0.5)` |
| pivot | `(0, 0.5)` |
| sizeDelta | `(59, 59)` |

| Visual | Value |
|---|---|
| sprite | `UIShapeUtils.WhiteCircle(64)` (placeholder) |
| color | `rgba(255,255,255,0.6)` |
| raycastTarget | `false` |

**NotifText (stretch, padded from bell and toggle):**

| Property | Value |
|---|---|
| anchorMin / anchorMax | `(0,0)` / `(1,1)` (stretch) |
| offsetMin | `(80, 0)` — clears bell icon |
| offsetMax | `(-ToggleWidth - 20, 0)` — clears toggle |

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Notifications:"` | |
| fontSize | `59` | 22 × 2.7 |
| fontStyle | `Bold` | weight 600 → Bold (closest) |
| alignment | `MidlineLeft` | |
| color | `#FFFFFF` | |
| outlineWidth | `0.2` | |
| outlineColor | `rgba(0,0,0,0.8)` | `-webkit-text-stroke: 3.5px` |
| raycastTarget | `false` | |

#### 4.4c `ToggleSwitch` (right-anchored)

CSS: `.toggle { width: 94px; height: 44px; border-radius: 22px; position: relative }`

| Property | Value | CSS × 2.7 |
|---|---|---|
| anchorMin | `(1, 0.5)` | right-aligned |
| anchorMax | `(1, 0.5)` | |
| pivot | `(1, 0.5)` | |
| sizeDelta | `(254, 119)` | 94×44 × 2.7 |

**Track Image:**

| Visual — ON | Value | CSS Source |
|---|---|---|
| sprite | `UIShapeUtils.WhiteRoundedRect(22, 48)` Sliced | `border-radius: 22px` |
| color | `#209830` | midtone `#24a838 → #1d8a2e` |
| Border-ring | 8px, `#166622` | `border: 3px solid #166622` × 2.7 |

| Visual — OFF | Value | CSS Source |
|---|---|---|
| color | `#4a4a4a` | midtone `#5a5a5a → #404040` |
| Border-ring | 8px, `#333333` | `border: 3px solid #333` × 2.7 |

**Knob:**

CSS: `.toggle-knob { width: 34px; height: 34px; border-radius: 50%; position: absolute; top: 2px }`

| Property | Value | CSS × 2.7 |
|---|---|---|
| sizeDelta | `(92, 92)` | 34 × 2.7 |
| anchorMin / anchorMax | `(0.5, 0.5)` | centered, positioned via anchoredPosition |
| pivot | `(0.5, 0.5)` | |
| **ON** anchoredPosition.x | `+73` | right: 3px → (94/2 - 34/2 - 3) × 2.7 |
| **OFF** anchoredPosition.x | `-73` | left: 3px → -(94/2 - 34/2 - 3) × 2.7 |

| Visual — ON | Value | CSS Source |
|---|---|---|
| sprite | `UIShapeUtils.WhiteCircle(64)` | circle |
| color | `#d4a832` | midtone `#f5e088 → #b8902a` |
| Highlight child | top half (anchor 0.5→1), inset 4px, `rgba(255,255,255,0.5)` | approximates `inset 0 3px 5px` |

| Visual — OFF | Value |
|---|---|
| color | `#aaaaaa` | midtone `#e0e0e0 → #888` |
| Highlight child | same as ON (static, does not change with toggle state) |

**ToggleLabel:**

CSS: `.toggle-text { font-size: 14px; font-weight: 700; -webkit-text-stroke: 2.5px rgba(0,0,0,0.7) }`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"ON"` or `"OFF"` | dynamic |
| fontSize | `38` | 14 × 2.7 |
| fontStyle | `Bold` | weight 700 |
| color | `#FFFFFF` | |
| outlineWidth | `0.18` | ~2.5px stroke |
| outlineColor | `rgba(0,0,0,0.7)` | |
| alignment | ON: `MidlineLeft`, OFF: `MidlineRight` | dynamic, set in `UpdateNotifVisuals` |
| offsetMin | `(32, 0)` | pads text 32px from left edge |
| offsetMax | `(-32, 0)` | pads text 32px from right edge |
| **Result** | ON: text hugs left, OFF: text hugs right — opposite of knob position |

---

### 4.5 `AudioCard`

Uses same card template as §4.4 (fill `#2c2585`, border-ring 8px `#1e1862`, padding 54).

#### 4.5a `AudioGrid`

CSS: `.audio-grid { display: flex; justify-content: space-around }`

| Component | Value |
|---|---|
| HorizontalLayoutGroup | childAlignment=MiddleCenter, spacing=0 |
| childForceExpandWidth | `true` (distributes evenly) |
| childForceExpandHeight | `false` |

#### 4.5b Each `AudioItem` (Sound, Haptic, Music)

CSS: `.audio-item { display: flex; flex-direction: column; align-items: center; gap: 10px }`

| Component | Value | CSS × 2.7 |
|---|---|---|
| VerticalLayoutGroup | spacing=27, childAlignment=UpperCenter | gap 10 × 2.7 |
| childForceExpandWidth | `false` | |
| childControlHeight | `true` | |

**AudioLabel:**

CSS: `.audio-item .label { font-size: 20px; font-weight: 700 }` + `.stroke-text`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Sound"` / `"Haptic"` / `"Music"` | |
| fontSize | `54` | 20 × 2.7 |
| fontStyle | `Bold` | weight 700 |
| color | `#FFFFFF` | |
| outlineWidth | `0.2` | |
| outlineColor | `rgba(0,0,0,0.8)` | `-webkit-text-stroke: 3.5px` |
| alignment | `Center` | |
| raycastTarget | `false` | |
| LayoutElement | preferredHeight=`54` | |

#### 4.5c `Audio3DButton` (232×232)

CSS: `.audio-gold { width: 86px; height: 86px; border-radius: 21px; padding: 3px; padding-bottom: 4px; background: linear-gradient(180deg, #f5dc68, #c8a030, #786010) }`

Uses §3.3D template.

| LayoutElement | Value | CSS × 2.7 |
|---|---|---|
| preferredWidth | `232` | 86 × 2.7 |
| preferredHeight | `232` | 86 × 2.7 |

| Layer | Sprite Radius | Size | Color | CSS Source |
|---|---|---|---|---|
| GoldRing | `21` | 232×232 | `#f5dc68` + Mask+Bevel→`#786010` | `border-radius: 21px` |
| GreenBorder | `18` | inset 8 each side | `#1a7820` | `border-radius: 18px`, midtone `#2a9830 → #106018` |
| | | | | padding: 3px top/sides, 4px bottom (×2.7: 8/8/8/11) |
| GreenFace | `16` | 206×203 | `#38c038` | `border-radius: 16px`, midtone `#5ee85e → #209420` |
| | | | | padding: 2px top/sides, 3px bottom (×2.7: 5/5/5/8) |
| GlossOverlay | `16` (top) | stretch-top 45% | `rgba(255,255,255,0.3)` | `::before { height: 45% }` |

**Press animation:**

CSS: `.audio-item:active .audio-gold { transform: translateY(3px); padding-bottom: 3px; padding-top: 4px }`

Implemented via `ButtonBounce` component (scale 0.95 → spring 1.03 → settle 1.0). The CSS translateY+padding-swap is approximated by scale bounce — simpler and consistent with all other buttons in the project.

**Icon (centered on face):**

CSS: `.icon-wrap { width: 36px; height: 36px }`

| Property | Value | CSS × 2.7 |
|---|---|---|
| sizeDelta | `(97, 97)` | 36 × 2.7 |
| Image sprite | from Resources (see §7) | white icon |
| Image color | `Color.white` | |
| raycastTarget | `false` | |

**StrikeLine (OFF state, centered on face):**

CSS: `.strike { width: 48px; height: 6px; border-radius: 3px; transform: rotate(-45deg); background: linear-gradient(180deg, #f03030, #b01010) }`

| Property | Value | CSS × 2.7 |
|---|---|---|
| sizeDelta | `(130, 16)` | 48×6 × 2.7 |
| rotation | `-45°` | `transform: rotate(-45deg)` |
| Image sprite | `UIShapeUtils.WhiteRoundedRect(3, 16)` | `border-radius: 3px` |
| Image color | `#d02020` | midtone `#f03030 → #b01010` |
| active | `false` when ON (hidden), `true` when OFF (shown) | `.audio-item.on .strike { opacity: 0 }` |
| raycastTarget | `false` | |

---

### 4.6 `SaveCard`

Same card template. Inner VLayoutGroup: spacing=43, childAlignment=**MiddleCenter**, childForceExpandWidth=true.

#### 4.6a `SaveTitle`

CSS: `.save-section h2 { font-size: 24px; font-weight: 700; margin-bottom: 16px }`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Save Your Progress"` | |
| fontSize | `65` | 24 × 2.7 |
| fontStyle | `Bold` | |
| color | `#FFFFFF` | |
| outlineWidth | `0.2` | |
| outlineColor | `rgba(0,0,0,0.8)` | |
| alignment | `Center` | |
| raycastTarget | `false` | |
| LayoutElement | preferredHeight=`65`, margin-bottom via VLayout spacing or padding | |

#### 4.6b `GoogleButton`

CSS: `.google-gold { border-radius: 32px; padding: 3px; padding-bottom: 4px; background: linear-gradient(180deg, #f5dc68, #c8a030, #786010) }`

Uses §3.3D template (gold ring variant), stretch width.

| Property | Value |
|---|---|
| LayoutElement | flexibleWidth=1 (stretch to card width) |

| Layer | Sprite Radius | Color | CSS Source |
|---|---|---|---|
| GoldRing | `32` | `#f5dc68` + Mask+Bevel→`#786010` | `border-radius: 32px` |
| BlueBorder | `29` | `#1870a8` | midtone `#2088c8 → #105888` |
| BlueFace | `27` | `#28b0f0` | midtone `#60d8ff → #0878c0` |
| GlossOverlay | `27` top | `rgba(255,255,255,0.3)` | `::before { height: 48% }` |

**Face content (HLayoutGroup):**

CSS: `.google-face { display: flex; align-items: center; justify-content: center; gap: 12px; padding: 13px 18px }`

| Component | Value | CSS × 2.7 |
|---|---|---|
| HorizontalLayoutGroup | spacing=32, childAlignment=MiddleCenter | gap 12 × 2.7 |
| padding L/R/T/B | `49, 49, 35, 35` | 18/18/13/13 × 2.7 |

**GoogleLogo (white circle + G image):**

CSS: `.google-logo { width: 36px; height: 36px; background: #fff; border-radius: 50% }`

| Property | Value | CSS × 2.7 |
|---|---|---|
| LayoutElement | preferredWidth=`97`, preferredHeight=`97` | 36 × 2.7 |
| Image sprite | `UIShapeUtils.WhiteCircle(64)` | circle |
| Image color | `#FFFFFF` | |

Child: Google G image (59×59, 22px × 2.7), sprite from Resources or placeholder.

**GoogleText:**

CSS: `.google-text { font-size: 19px; font-weight: 700 }`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Sign in with Google"` | |
| fontSize | `51` | 19 × 2.7 |
| fontStyle | `Bold` | |
| color | `#FFFFFF` | |
| outlineWidth | `0.2` | |
| outlineColor | `rgba(0,0,0,0.8)` | |

---

### 4.7 `SupportButton`

CSS: `.support-gold { width: 72%; margin: 0 auto; border-radius: 22px; padding: 3px; padding-bottom: 5px; background: linear-gradient(180deg, #f5dc68, #c8a030, #786010) }`

Uses §3.3D template (gold ring variant).

| Property | Value |
|---|---|
| LayoutElement | preferredWidth = parent.width × 0.72 |
| OR anchorMin/Max | `(0.14, 0.5)` / `(0.86, 0.5)` — 14% margin each side = 72% width |

| Layer | Sprite Radius | Color |
|---|---|---|
| GoldRing | `22` | `#f5dc68` + Mask+Bevel→`#786010` |
| GreenBorder | `19` | `#1a8820` (midtone `#2aaa30 → #107010`) |
| GreenFace | `17` | `#3cc820` (midtone `#6ef848 → #1c8c0c`) |
| GlossOverlay | `17` top | `rgba(255,255,255,0.25)` height 48% |

**Face content (HLayoutGroup):**

CSS: `.support-face { padding: 13px 22px; gap: 12px }`

| Component | Value | CSS × 2.7 |
|---|---|---|
| HLayoutGroup | spacing=32, padding 35/35/59/59 | 13/13/22/22 × 2.7 |

**SupportIcon:**

CSS: `.support-face .icon { width: 28px; height: 28px }`

| Property | Value |
|---|---|
| LayoutElement | preferredWidth=`76`, preferredHeight=`76` |
| Image sprite | chat bubble from Resources or placeholder |
| color | `Color.white` |

**SupportText:**

CSS: `.support-face .text { font-size: 22px; font-weight: 700 }`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Support"` | |
| fontSize | `59` | 22 × 2.7 |
| fontStyle | `Bold` | |
| color | `#FFFFFF` | |
| outlineWidth | `0.2` | |
| outlineColor | `rgba(0,0,0,0.8)` | |

---

### 4.8 `LegalRow`

CSS: `.legal-row { display: flex; justify-content: space-between; gap: 20px; padding: 0 8px }`

| Component | Value | CSS × 2.7 |
|---|---|---|
| HorizontalLayoutGroup | spacing=54, padding L/R=22 | gap 20, padding 8 × 2.7 |
| childForceExpandWidth | `true` | `flex: 1` on each |
| childForceExpandHeight | `true` | buttons fill row height |

#### 4.8a `TermsButton` / `PrivacyButton`

CSS: `.legal-outer { flex: 1; border-radius: 16px; padding: 3px; padding-bottom: 4px; background: linear-gradient(180deg, #a090d8, #6050b0, #382890) }`

**NO gold ring** — purple outer directly with its own border bevel.

| Layer | Sprite Radius | Color | CSS Source |
|---|---|---|---|
| PurpleOuter | `16` | `#a090d8` + Mask+Bevel→`#382890` | gradient `#a090d8 → #382890` |
| PurpleBorder | `13` | `#483890` | midtone `#5848a8 → #382880` |
| PurpleFace | `11` | `#6c5cb8` | midtone `#9080d4 → #5040a0` |
| GlossOverlay | `11` top | `rgba(255,255,255,0.15)` | `::before { height: 48% }` |

**Face text:**

CSS: `.legal-face span { font-size: 20px; font-weight: 700 }` + `.stroke-text-thin`

| TMPro | Value | CSS × 2.7 |
|---|---|---|
| text | `"Terms"` / `"Privacy"` | |
| fontSize | `54` | 20 × 2.7 |
| fontStyle | `Bold` | |
| color | `#FFFFFF` | |
| outlineWidth | `0.18` | |
| outlineColor | `rgba(0,0,0,0.75)` | `-webkit-text-stroke: 3px` |
| alignment | `Center` | |
| padding T/B/L/R | `27, 27, 43, 43` | `padding: 10px 16px` × 2.7 |

---

## 5. Color Reference

### 5.1 Background & Structure

| Element | Color | CSS Source |
|---|---|---|
| Screen bg | `#2d2280` | midtone `#3a2e92 → #261d70` |
| Header bg | `#6b4cb8` | midtone `#8b6ad4 → #5a3da6` |
| Header highlight | `rgba(255,255,255,0.15)` | `inset 0 2px 3px` |
| Gold separator | `#7a5510` | `border-bottom: 4px solid` |
| Card fill | `#2c2585` | midtone `#262078 → #302a90` |
| Card border | `#1e1862` | `border: 3px solid` |
| Card inset shadow | `rgba(0,0,0,0.15)` | `inset 0 3px 8px` |

### 5.2 Gold Ring (shared by audio, google, support, close)

| Part | Color | Implementation |
|---|---|---|
| Base Image | `#f5dc68` | bright gold, used as Mask source |
| Bevel gradient | `Color.clear` → `#786010` | `ThemeConfig.CreateGradientSprite`, child of Masked gold ring |
| **Visual result** | bright gold top → dark gold bottom | gradient bevel effect |

### 5.3 Button Colors

| Type | Face | Border | Outer/Ring | Gloss Alpha |
|---|---|---|---|---|
| Audio (green) | `#38c038` | `#1a7820` | Gold ring | 0.3 |
| Google (blue) | `#28b0f0` | `#1870a8` | Gold ring | 0.3 |
| Support (green) | `#3cc820` | `#1a8820` | Gold ring | 0.25 |
| Close (red) | `#d42828` | `#cc2020` | Gold ring | 0.25 |
| Legal (purple) | `#6c5cb8` | `#483890` | `#a090d8` base + bevel→`#382890` (no gold) | 0.15 |

### 5.4 Toggle Colors

| State | Track | Track Border | Knob | Knob Highlight |
|---|---|---|---|---|
| ON | `#209830` | `#166622` | `#d4a832` (gold) | top `rgba(255,255,255,0.5)` |
| OFF | `#4a4a4a` | `#333333` | `#aaaaaa` (gray) | top `rgba(255,255,255,0.5)` |

### 5.5 Strike Line

| Property | Value |
|---|---|
| Color | `#d02020` |
| Shown | when toggle is OFF (`SetActive(true)`) |
| Hidden | when toggle is ON (`SetActive(false)`) |

---

## 6. Font Specifications

All text uses **Nunito-Regular SDF** (TMPro), white, bold, with outline.

| Element | fontSize | outlineWidth (0-1) | outlineColor | fontStyle | CSS Source |
|---|---|---|---|---|---|
| Header title | `103` | `0.25` | `rgba(0,0,0,0.75)` | Bold | 38px, stroke 4px |
| Close X | `59` | `0.2` | `rgba(0,0,0,0.65)` | Bold | 22px, stroke 3px |
| Notif label | `59` | `0.2` | `rgba(0,0,0,0.8)` | Bold | 22px, stroke 3.5px |
| Toggle text | `38` | `0.18` | `rgba(0,0,0,0.7)` | Bold | 14px, stroke 2.5px |
| Audio labels | `54` | `0.2` | `rgba(0,0,0,0.8)` | Bold | 20px, stroke 3.5px |
| Save title | `65` | `0.2` | `rgba(0,0,0,0.8)` | Bold | 24px, stroke 3.5px |
| Google text | `51` | `0.2` | `rgba(0,0,0,0.8)` | Bold | 19px, stroke 3.5px |
| Support text | `59` | `0.2` | `rgba(0,0,0,0.8)` | Bold | 22px, stroke 3.5px |
| Legal text | `54` | `0.18` | `rgba(0,0,0,0.75)` | Bold | 20px, stroke 3px |

**Font weight mapping:** 600/700 → Bold (Nunito has no separate SemiBold SDF asset).

---

## 7. Sprite Specifications

| Sprite | Source | Used By |
|---|---|---|
| Rounded rect (various radii) | `UIShapeUtils.WhiteRoundedRect(radius, size)` Sliced | Cards, buttons, toggle track, strike |
| Circle | `UIShapeUtils.WhiteCircle(64)` | Close layers, knob, google logo circle |
| SFX icon (white) | `Resources.Load<Sprite>("Icons/icon-sfx-on")` | Sound button |
| Vibration icon (white) | `Resources.Load<Sprite>("Icons/icon-vibration")` | Haptic button |
| Music icon (white) | `Resources.Load<Sprite>("Icons/icon-music-on")` | Music button |
| Bell icon | `UIShapeUtils.WhiteCircle(64)`, color `rgba(255,255,255,0.6)` (placeholder) | Notifications label |
| Support chat | `UIShapeUtils.WhiteCircle(64)`, color `Color.white` (placeholder) | Support button |
| Google G | placeholder — multicolor G or white `G` TMPro fallback | Google button |

**Sprite radius = sprite-texture pixels, NOT Unity reference pixels.**

---

## 8. Interaction Logic

### 8.1 Toggle Persistence

| Toggle | Read | Write |
|---|---|---|
| Sound | `IProgressionManager.SoundEnabled` | + `IAudioManager.SetSoundEnabled()` |
| Music | `IProgressionManager.MusicEnabled` | + `IAudioManager.SetMusicEnabled()` |
| Haptic | `PlayerPrefs.GetInt("VibrationEnabled", 1)` | `PlayerPrefs.SetInt()` + `.Save()` |
| Notifications | `PlayerPrefs.GetInt("NotificationsEnabled", 1)` | `PlayerPrefs.SetInt()` + `.Save()` |

### 8.2 Audio Toggle

```
ToggleAudio(ref bool state, GameObject strikeGo):
  HapticUtils.TryVibrate()
  state = !state
  persist via §8.1
  strikeGo.SetActive(!state)  // strike visible when OFF
```

### 8.3 Notification Toggle

```
ToggleNotification():
  HapticUtils.TryVibrate()
  _notificationsOn = !_notificationsOn
  persist via §8.1
  UpdateTrackImage(on ? #209830 : #4a4a4a)
  UpdateTrackBorder(on ? #166622 : #333333)
  StartCoroutine(AnimateKnob(on ? +73 : -73, 0.3s))
  UpdateKnobColor(on ? #d4a832 : #aaaaaa)
  UpdateLabel(on ? "ON" left-aligned : "OFF" right-aligned)
```

### 8.4 Close

```
OnClose():
  HapticUtils.TryVibrate()
  Services.TryGet<ScreenManager>(out var sm)
  sm.TransitionTo(GameFlowState.MainMenu)
```

### 8.5 Placeholder Actions

| Button | Action |
|---|---|
| Google Sign-In | `Debug.Log("[Settings] Google Sign-In clicked")` |
| Support | `Application.OpenURL("mailto:support@juicesort.com")` |
| Terms | `Application.OpenURL("https://juicesort.com/terms")` |
| Privacy | `Application.OpenURL("https://juicesort.com/privacy")` |

### 8.6 Refresh (called on screen show)

```
Refresh():
  LoadToggleStates()  // read from IProgressionManager + PlayerPrefs
  UpdateAllVisuals()  // set track colors, knob positions, strike visibility, label text
```

---

## 9. Animation Specifications

### 9.1 ButtonBounce (all buttons)

Reuse existing `ButtonBounce` component.

| Phase | Scale | Duration | Easing |
|---|---|---|---|
| Press | `0.95` | 60ms | EaseOutQuad |
| Release | `1.03` | 90ms | EaseOutBack |
| Settle | `1.0` | natural | |

### 9.2 Toggle Knob Slide

| Property | Value |
|---|---|
| Animated | `anchoredPosition.x` |
| ON target | `+73` |
| OFF target | `-73` |
| Duration | `0.3s` |
| Easing | manual `EaseInOutCubic` (`t<0.5 ? 4t³ : 1-(-2t+2)³/2`) |
| Method | Coroutine |

### 9.3 Screen Transition

Uses existing `ScreenManager` crossfade + slide (0.3s out + 0.3s in, EaseInOutCubic).

---

## 10. Constants Reference (CSS × 2.7)

```csharp
// Header
private const float HeaderHeight = 184f;              // CSS: 20+title+24 padding
private const float GoldSepHeight = 11f;              // CSS: 4px × 2.7
private const float CloseButtonSize = 146f;           // CSS: 54px × 2.7
private const float CloseButtonMargin = 38f;          // CSS: right 14px × 2.7
private const float CloseRedRingSize = 130f;          // CSS: 48px × 2.7
private const float CloseFaceSize = 114f;             // CSS: 42px × 2.7

// Content
private const float ContentPaddingTop = 54f;          // CSS: 20px × 2.7
private const float ContentPaddingLR = 43f;           // CSS: 16px × 2.7
private const float ContentPaddingBottom = 108f;      // CSS: 40px × 2.7
private const float CardGap = 49f;                    // CSS: gap 18px × 2.7
private const float CardPadding = 54f;                // CSS: padding 20px × 2.7
private const float CardBorderWidth = 8f;             // CSS: border 3px × 2.7

// Toggle
private const float ToggleWidth = 254f;               // CSS: 94px × 2.7
private const float ToggleHeight = 119f;              // CSS: 44px × 2.7
private const float KnobSize = 92f;                   // CSS: 34px × 2.7
private const float KnobTravel = 73f;                 // distance from center to ON/OFF

// Audio buttons
private const float AudioButtonSize = 232f;           // CSS: 86px × 2.7
private const float AudioIconSize = 97f;              // CSS: 36px × 2.7
private const float StrikeWidth = 130f;               // CSS: 48px × 2.7
private const float StrikeHeight = 16f;               // CSS: 6px × 2.7
private const float AudioItemSpacing = 27f;           // CSS: gap 10px × 2.7

// Google button
private const float GoogleLogoSize = 97f;             // CSS: 36px × 2.7
private const float GoogleLogoImgSize = 59f;          // CSS: 22px × 2.7
private const float GoogleFacePaddingTB = 35f;        // CSS: 13px × 2.7
private const float GoogleFacePaddingLR = 49f;        // CSS: 18px × 2.7
private const float GoogleContentGap = 32f;           // CSS: gap 12px × 2.7

// Support button
private const float SupportIconSize = 76f;            // CSS: 28px × 2.7
private const float SupportFacePaddingTB = 35f;       // CSS: 13px × 2.7
private const float SupportFacePaddingLR = 59f;       // CSS: 22px × 2.7
private const float SupportContentGap = 32f;          // CSS: gap 12px × 2.7

// Legal
private const float LegalGap = 54f;                   // CSS: gap 20px × 2.7
private const float LegalPaddingLR = 22f;             // CSS: padding 0 8px × 2.7
private const float LegalFacePaddingTB = 27f;         // CSS: 10px × 2.7
private const float LegalFacePaddingLR = 43f;         // CSS: 16px × 2.7

// Sprite radii (sprite-texture px, NOT ×2.7)
private const int CardRadius = 20;
private const int ToggleRadius = 22;
private const int AudioGoldRadius = 21;
private const int AudioBorderRadius = 18;
private const int AudioFaceRadius = 16;
private const int GoogleGoldRadius = 32;
private const int GoogleBorderRadius = 29;
private const int GoogleFaceRadius = 27;
private const int SupportGoldRadius = 22;
private const int SupportBorderRadius = 19;
private const int SupportFaceRadius = 17;
private const int LegalOuterRadius = 16;
private const int LegalBorderRadius = 13;
private const int LegalFaceRadius = 11;
```

---

## 11. Key Files

| File | Path | Role |
|---|---|---|
| SettingsScreen.cs | `Assets/Scripts/Game/UI/Screens/SettingsScreen.cs` | All construction + logic |
| BootLoader.cs | `Assets/Scripts/Game/Boot/BootLoader.cs` | Creates + registers screen |
| ThemeConfig.cs | `Assets/Scripts/Game/UI/ThemeConfig.cs` | Font access, gradient helper |
| UIShapeUtils.cs | `Assets/Scripts/Game/UI/Components/UIShapeUtils.cs` | Cached procedural sprites |
| ButtonBounce.cs | `Assets/Scripts/Game/UI/Components/ButtonBounce.cs` | Press animation |
| HapticUtils.cs | `Assets/Scripts/Game/UI/Components/HapticUtils.cs` | Vibration |
| ScreenManager.cs | `Assets/Scripts/Game/UI/ScreenManager.cs` | Screen transitions |

---

## Validation Checklist

- [x] Every element in §2 hierarchy has a corresponding §4 entry
- [x] Every §4 entry has values derived from HTML CSS × 2.7
- [x] Every color hex in §5 matches the HTML mockup
- [x] Every font size in §6 = HTML font-size × 2.7
- [x] §10 constants match all values in §4 and actual code
- [x] No "differences table" — spec is unified single source of truth
- [x] Every CSS visual effect has a Unity implementation note
- [x] All 3D button layers fully specified with radii, colors, sizes
- [x] Bevel technique documented (Mask+gradient overlay, not CSS border-ring)
- [x] Every Button has transparent Image for raycast targeting
- [x] Title underlay material setup documented with code snippet
- [x] Toggle label L/R shift documented
- [x] Card shadow/highlight heights and colors verified (top=8px dark, bottom=5px light)
- [x] All raycastTarget=false set on non-interactive text/images
- [x] Verified across 3 adversarial code review passes against `SettingsScreen.cs`

**Document Status:** IMPLEMENTATION VERIFIED (v3) — 2026-03-30
