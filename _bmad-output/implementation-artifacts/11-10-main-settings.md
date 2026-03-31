# Story 11.10: Main Settings — Premium 3D Button Redesign

Status: done

## Story

As a player,
I want a polished, visually rich settings panel with 3D-style buttons and clear toggle states,
so that managing my game preferences feels premium and intuitive.

## Priority

HIGH — Fully-designed Magic Sort-style settings panel with 3D buttons, toggles, and cards. Uses **uGUI procedural C#** (same proven approach as all other screens).

## Acceptance Criteria

1. Settings panel renders matching the approved HTML mockup design
2. All buttons have 3-layer 3D structure with correct bevel lighting (gold ring, color border, face + gloss overlay)
3. Text outlines visible on all labels (TMPro outline)
4. Notifications toggle switches between ON (green, knob right) / OFF (gray, knob left) with visual state change
5. Audio buttons (Sound, Haptic, Music) toggle red diagonal strike-line visibility for OFF state
6. Google Sign-In, Support, Terms, Privacy buttons trigger correct placeholder actions
7. Press animation works on all 3D buttons (ButtonBounce component)
8. Panel dismisses via close button (red 3D button with gold ring, circular)
9. Icon sprites loaded from Resources/Icons/ (icon-sfx-on, icon-vibration, icon-music-on)
10. Existing settings functionality preserved: Sound/Music/Haptic persist via IProgressionManager + PlayerPrefs

## Prerequisites

- [x] Settings mockup HTML available at `_bmad-output/mockups/story-11-10/settings-panel-mockup.html`
- [x] UI spec created at `_bmad-output/mockups/story-11-10/settings-panel-ui-spec-v3.md`

## Tasks / Subtasks

- [x] Task 1: Build SettingsScreen.Create() with full hierarchy (AC: 1, 2, 8)
  - [x] 1.1 Create new `SettingsScreen.cs` using static `Create()` factory (same pattern as HubScreen)
  - [x] 1.2 Canvas + CanvasScaler (1080×1920, match 0.5, sortingOrder 10)
  - [x] 1.3 Background (stretch-all, solid `#2d2280`)
  - [x] 1.4 Header (stretch-top h=180, bg `#6b4cb8`, gold separator `#7a5510` h=11)
  - [x] 1.5 Title TMPro ("Settings", 103px bold, outline 11px)
  - [x] 1.6 CloseButton — 3-layer circular: gold ring (146×146) → red ring (130×130) → red face (114×114) + gloss + "X" TMPro
  - [x] 1.7 ScrollRect + VLayoutGroup (spacing 48, padding 54/43/108/43)

- [x] Task 2: Build card containers and notification toggle (AC: 3, 4)
  - [x] 2.1 Card template helper: `CreateCard()` → rounded rect `#2c2585`, border-ring 8px `#1e1862`, padding 54
  - [x] 2.2 NotificationCard: HLayoutGroup row with label + toggle
  - [x] 2.3 NotifLabel: bell icon (65×65) + TMPro "Notifications:" (60px)
  - [x] 2.4 Toggle: track (254×119, green/gray), knob (92×92, gold/gray), label TMPro (38px "ON"/"OFF")
  - [x] 2.5 Toggle knob animation via coroutine (0.3s slide, anchoredPosition.x ±73)

- [x] Task 3: Build audio 3D buttons (AC: 2, 5, 7, 9)
  - [x] 3.1 AudioCard with HLayoutGroup (space-around)
  - [x] 3.2 3D button helper: `Create3DButton()` → gold ring → color border → color face → gloss overlay
  - [x] 3.3 Three audio items (Sound, Haptic, Music): VLayoutGroup, label TMPro (54px) + 3D button (232×232, green)
  - [x] 3.4 Icon sprites loaded via `Resources.Load<Sprite>("Icons/icon-sfx-on")` etc.
  - [x] 3.5 StrikeLine overlay (130×16, red `#d02020`, rotated -45°, toggled via SetActive)
  - [x] 3.6 ButtonBounce component on each audio button

- [x] Task 4: Build Google, Support, Legal buttons (AC: 2, 6, 7)
  - [x] 4.1 SaveCard: TMPro "Save Your Progress" (65px) + Google 3D button (stretch, blue, gold ring, rounded 86)
  - [x] 4.2 Google button face: white circle (97×97) + Google G logo (59×59) + TMPro "Sign in with Google" (51px)
  - [x] 4.3 SupportButton: 3D button (72% width, green, gold ring, rounded 59) + icon (76×76) + TMPro "Support" (60px)
  - [x] 4.4 LegalRow: HLayoutGroup (gap 54) + two purple 3D buttons (NO gold ring) with TMPro "Terms"/"Privacy" (54px)

- [x] Task 5: Wire all interactions (AC: 5, 6, 8, 10)
  - [x] 5.1 Audio toggles: IProgressionManager (SoundEnabled, MusicEnabled) + IAudioManager + strike-line SetActive
  - [x] 5.2 Haptic toggle: PlayerPrefs("VibrationEnabled") + strike-line SetActive
  - [x] 5.3 Notification toggle: PlayerPrefs("NotificationsEnabled") + knob slide + track color + label text
  - [x] 5.4 Close button: HapticUtils.TryVibrate() + ScreenManager.TransitionTo(MainMenu)
  - [x] 5.5 Google: Debug.Log placeholder
  - [x] 5.6 Support: Application.OpenURL("mailto:support@juicesort.com")
  - [x] 5.7 Terms/Privacy: Application.OpenURL placeholders
  - [x] 5.8 HapticUtils.TryVibrate() on every button press
  - [x] 5.9 Refresh() method: reload all toggle states from services on screen show

- [x] Task 6: Visual polish and testing (AC: 1, 2, 3)
  - [x] 6.1 Compare rendered panel against HTML mockup — verify colors, sizes, spacing
  - [x] 6.2 Verify all TMPro text has correct outline width/color per spec §6
  - [x] 6.3 Test on 16:9 and 20:9 aspect ratios
  - [x] 6.4 Test all toggle states persist across panel open/close/reopen

## Dev Notes

### Implementation Approach

uGUI procedural C# — same proven approach as all other screens.

**Follows exact patterns from:**
- `HubScreen.Create()` — static factory, Canvas setup, programmatic hierarchy
- `GameplayHUD.Create()` — 3D button construction, toggle wiring, icon loading
- `UIShapeUtils` — cached procedural sprites (rounded rects, circles)
- `ThemeConfig` — font access (Nunito-Regular SDF)
- `ButtonBounce` — press animation component
- `ScreenManager` — screen transitions and registration

### UI Spec Reference

**Full implementation spec:** `_bmad-output/mockups/story-11-10/settings-panel-ui-spec-v3.md`

This spec contains:
- Complete GameObject hierarchy with exact sizes
- All colors (§5), text properties (§6), sprite specs (§7)
- Constants block (§10) — copy-paste into C#
- Interaction logic (§8), animation specs (§9)

**Conversion factor:** HTML renders at 400px width → 1080px Unity. **All values = CSS × 2.7**

### Architecture Compliance

- **Service Locator:** `Services.TryGet<IProgressionManager>()`, `Services.TryGet<IAudioManager>()`
- **No singletons, no FindObjectOfType, no GameObject.Find()**
- **[SerializeField] private** not needed — all sprites loaded from Resources
- **Namespace:** `JuiceSort.Game.UI.Screens`
- **Assembly:** Game (refs Core, Unity.TextMeshPro)

### Key Services Used

```csharp
// Audio toggles (matches GameplayHUD pattern exactly)
Services.TryGet<IProgressionManager>(out var prog)  // .SoundEnabled, .MusicEnabled (read/write, auto-saves)
Services.TryGet<IAudioManager>(out var audio)         // .SetSoundEnabled(), .SetMusicEnabled()

// Haptic toggle
PlayerPrefs.GetInt("VibrationEnabled", 1)  // read (1 = enabled)
PlayerPrefs.SetInt("VibrationEnabled", value ? 1 : 0)  // write
PlayerPrefs.Save()

// Notifications toggle (placeholder)
PlayerPrefs.GetInt("NotificationsEnabled", 1)  // read
PlayerPrefs.SetInt("NotificationsEnabled", value ? 1 : 0)  // write
PlayerPrefs.Save()

// Navigation
Services.TryGet<ScreenManager>(out var sm)  // .TransitionTo(GameFlowState.MainMenu)
```

### 3-Layer 3D Button Construction (uGUI)

```csharp
// Pseudocode — each 3D button is 3 nested Images
var goldRing = CreateImage(parent, size, UIShapeUtils.WhiteRoundedRect(radius), goldColor);
  // border-ring child: top #f5dc68, bottom #786010
var colorBorder = CreateImage(goldRing, size-8, UIShapeUtils.WhiteRoundedRect(innerRadius), borderColor);
var colorFace = CreateImage(colorBorder, size-14, UIShapeUtils.WhiteRoundedRect(faceRadius), faceColor);
var gloss = CreateImage(colorFace, stretchTop45pct, UIShapeUtils.WhiteRoundedRect(faceRadius), white30alpha);
// + icon Image centered on face
// + ButtonBounce on root
```

### Icon Sprites (from Resources/Icons/)

| Element | Resource Path |
|---------|--------------|
| Sound button | `Icons/icon-sfx-on` |
| Haptic button | `Icons/icon-vibration` |
| Music button | `Icons/icon-music-on` |
| Support button | placeholder or new asset |
| Google G | placeholder or new asset |
| Bell icon | placeholder or new asset |

### Files to MODIFY

| File | Change |
|------|--------|
| `Assets/Scripts/Game/UI/Screens/SettingsScreen.cs` | Complete `Create()` implementation with 3D buttons, toggles, cards |
| `Assets/Scripts/Game/Boot/BootLoader.cs` | Creates + registers SettingsScreen, wires Refresh on state change |

### Files NOT Modified

| File | Reason |
|------|--------|
| `Assets/Scripts/Game/UI/Screens/HubScreen.cs` | `GoSettings()` already calls `ScreenManager.TransitionTo(GameFlowState.Settings)` |
| `Assets/Scripts/Game/UI/GameFlowState.cs` | `Settings` state already exists |
| `Assets/Scripts/Game/UI/Components/GameplayHUD.cs` | Inline settings dropdown is separate |
| `Assets/Scripts/Game/UI/Components/UIShapeUtils.cs` | Reused as-is |
| `Assets/Scripts/Game/UI/Components/ButtonBounce.cs` | Reused as-is |

### References

- [Source: UI Spec v3] `_bmad-output/mockups/story-11-10/settings-panel-ui-spec-v3.md` — Implementation-verified spec
- [Source: HTML Mockup] `_bmad-output/mockups/story-11-10/settings-panel-mockup.html` — Approved visual reference
- [Source: _bmad-output/epics.md — Epic 11: UI/UX Overhaul]
- [Source: _bmad-output/game-architecture.md — UI Navigation system, Service Locator pattern]
- [Source: _bmad-output/project-context.md — Anti-patterns, performance rules]
- [Source: _bmad-output/mockups/gameplay-hud-ui-spec-v3.2.md — Proven HUD spec format, 3D button pattern]
- [Source: Assets/Scripts/Game/UI/Screens/HubScreen.cs — Reference Create() factory pattern]
- [Source: Assets/Scripts/Game/UI/Components/GameplayHUD.cs — Reference 3D button + toggle pattern]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

None — clean implementation, no runtime errors detected.

### Completion Notes List

- ✅ Tasks 1-5: Built complete SettingsScreen.cs (1050+ lines) with full uGUI procedural hierarchy matching UI spec
- ✅ Task 6: Verified all colors, font sizes, outline widths, constants, and interaction logic against spec — all compliant
- ✅ All 10 acceptance criteria satisfied
- ✅ Follows exact patterns from HubScreen.Create(), GameplayHUD, UIShapeUtils, ButtonBounce, ThemeConfig
- ✅ Service Locator pattern used throughout (no singletons, no FindObjectOfType, no GameObject.Find)

### Change Log

- 2026-03-30: Full uGUI implementation completed. SettingsScreen.cs with Create() factory pattern.
- 2026-03-30: Code review: Fixed close button raycast, removed dead code, spec compliance (bevels, padding, underlay, toggle label shift, card shadows). Verified across 3 review passes against spec v3.
- 2026-03-31: Visual polish: 3D button layers with Mask+gradient, smooth gloss, face gradients, white icons via LoadWhiteIcon, cylinder toggle/Google pill shapes, icon shadows, bell/google-logo/support-icon from Resources, all text bold with OUTLINE_ON shader keyword. Code review clean — production ready.

### File List

**Modified:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/SettingsScreen.cs` — Full uGUI procedural C# with 3D buttons, toggles, cards
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — Creates + registers SettingsScreen, wires Refresh
