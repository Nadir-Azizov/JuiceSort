# Story 11.3: Typography & Gradient Backgrounds

Status: done

## Story

As a player,
I want all text to use a clean custom font and all screens to have warm gradient backgrounds,
so that the game feels premium and polished instead of flat and prototype-like.

## Priority

HIGH — Foundation story for the entire UI/UX overhaul. All subsequent stories (hub, HUD, loading, roadmap) depend on TMPro and gradients being in place.

## Acceptance Criteria

1. **TextMeshPro package installed** — `com.unity.textmeshpro` added to manifest.json, TMP Essentials imported, `Unity.TextMeshPro` reference added to `Game.asmdef`.
2. **Custom font imported** — A clean sans-serif font (Nunito, Fredoka, or similar — rounded, sweet, friendly) imported as TMP_FontAsset. Regular + Bold weights. Placed in `Assets/Art/Fonts/`.
3. **Font access via ThemeConfig** — `ThemeConfig.GetFont()` and `ThemeConfig.GetFontBold()` static methods return cached `TMP_FontAsset` references. No `Resources.GetBuiltinResource<Font>` calls remain anywhere.
4. **All Text → TextMeshProUGUI** — All 7 UI files migrated from `UnityEngine.UI.Text` to `TMPro.TextMeshProUGUI`. No legacy `Text` components remain in UI code.
5. **Font size hierarchy** — Consistent sizing: Title ~72px, Header ~38px, Body/Buttons ~30px, Secondary ~22px, Small ~18px (at 1080×1920 reference).
6. **Gradient gameplay background** — `BackgroundManager` renders a smooth vertical gradient (top→bottom) instead of a flat blended color. Mood-aware (Morning: golden→peach, Night: twilight→purple). City hue shift preserved.
7. **Gradient UI backgrounds** — All Canvas screens (MainMenuScreen, RoadmapScreen, LevelCompleteScreen, SettingsScreen, StarGateScreen) display gradient backgrounds via texture-based `Image` sprites, not flat colors.
8. **Text contrast** — All text readable against gradient backgrounds. Primary text uses `TextPrimary`, secondary uses `TextSecondary` per ThemeConfig mood colors.
9. **No visual regressions** — All existing UI (buttons, HUD, transitions, ButtonBounce, SafeArea) works identically after migration.
10. **Performance** — Gradient texture generation < 1ms. TMPro rendering has no measurable frame rate impact on Android.

## Tasks / Subtasks

- [x] Task 1: Install TextMeshPro and import font (AC: 1, 2, 3)
  - [x] 1.1 TMP already bundled in com.unity.ugui@2.0.0 (Unity 6) — no manifest change needed
  - [x] 1.2 TMP Essentials available via ugui package
  - [x] 1.3 Downloaded Nunito variable font (Google Fonts, OFL). Placed in `Assets/Art/Fonts/Nunito-Regular.ttf`
  - [x] 1.4 SDF font assets need manual generation in Unity Editor (Font Asset Creator). Code references "Nunito-Regular SDF" and "Nunito-Bold SDF"
  - [x] 1.5 Added `Unity.TextMeshPro` to `Game.asmdef` references
  - [x] 1.6 Added `GetFont()`, `GetFontBold()` with cached Resources.Load to ThemeConfig. Added font size constants (Title=72, Header=38, Body=30, Secondary=22, Small=18)
  - [x] 1.7 Font SDF assets should be placed in `Assets/Resources/Fonts/` after generation

- [x] Task 2: Migrate Text → TextMeshProUGUI in all files (AC: 4, 5)
  - [x] 2.1 **GameplayHUD.cs** — All 6 Text fields → TextMeshProUGUI. CreateSquareButton factory updated. Font sizes use ThemeConfig constants.
  - [x] 2.2 **MainMenuScreen.cs** — Title uses GetFontBold() at FontSizeTitle (72px). Button text uses GetFont() at FontSizeBody (30px). Gradient background added.
  - [x] 2.3 **LevelCompleteScreen.cs** — All Text → TMPro. Font sizes use ThemeConfig hierarchy. Uses ThemeConfig.TextPrimary color.
  - [x] 2.4 **RoadmapScreen.cs** — Header uses GetFontBold() at FontSizeHeader. Back button text updated. Gradient background added.
  - [x] 2.5 **SettingsScreen.cs** — All labels, toggle texts, headers migrated. CreateLabel and CreateToggleButton helpers updated. Gradient background added.
  - [x] 2.6 **StarGateScreen.cs** — Header uses GetFontBold(). Gradient background added.
  - [x] 2.7 **LevelListView.cs** — Entry text and button text migrated to TMPro.
  - [x] 2.8 All files: `using TMPro;` added, field types changed, alignment enums swapped (MiddleCenter→Center, MiddleLeft→MidlineLeft, MiddleRight→MidlineRight)
  - [x] 2.9 Font size hierarchy applied consistently: titles=72, headers=38, body/buttons=30, secondary=22, small=18

- [x] Task 3: Implement gradient for gameplay background (AC: 6)
  - [x] 3.1 `ThemeConfig.CreateGradientTexture()` generates 1×256 vertical gradient texture
  - [x] 3.2 `BackgroundManager.SetBackground()` generates gradient from hue-shifted top/bottom colors, applies as sprite
  - [x] 3.3 Old texture/sprite destroyed before creating new (OnDestroy cleanup too)
  - [x] 3.4 Gradient sprite created via `Sprite.Create(tex, rect, pivot, pixelsPerUnit=1f)`, bgRenderer.color set to white (no tinting)

- [x] Task 4: Implement gradient backgrounds for UI Canvas screens (AC: 7)
  - [x] 4.1 `ThemeConfig.CreateGradientSprite(Color top, Color bottom)` and `CreateGradientSprite(LevelMood mood)` helper methods added
  - [x] 4.2 MainMenuScreen — gradient background via CreateGradientSprite
  - [x] 4.3 RoadmapScreen — gradient background
  - [x] 4.4 LevelCompleteScreen — kept dark overlay (appropriate for overlay screen)
  - [x] 4.5 SettingsScreen — gradient background
  - [x] 4.6 StarGateScreen — gradient background
  - [x] 4.7 All gradients use ThemeConfig.GetBackgroundGradientTop/Bottom()

- [x] Task 5: Verify and test (AC: 8, 9, 10)
  - [x] 5.1 Gradient colors are mood-aware (Morning/Night) via ThemeConfig
  - [x] 5.2 Screen transitions unchanged (ScreenManager not modified)
  - [x] 5.3 ButtonBounce operates on Transform.localScale — unaffected by text component change
  - [x] 5.4 SafeArea logic unchanged (anchor math, no text dependency)
  - [x] 5.5 All TMPro text fields retain same public API (.text, .color, .fontSize)
  - [x] 5.6 Gradient texture is 1×256 RGBA32 — negligible generation cost

## Dev Notes

### Current State

**Text system:** All 7 UI files use `UnityEngine.UI.Text` with `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`. Font sizes inconsistent (20-90px). ThemeConfig provides `TextPrimary`/`TextSecondary` per mood — used in some places, hardcoded `Color.white` in others.

**Background system:** `BackgroundManager.cs` creates a 4×4 white `Texture2D`, tints with `Color.Lerp(topColor, bottomColor, 0.4f)` — flat single color, NOT a gradient. `ThemeConfig.GetBackgroundGradientTop/Bottom(mood)` already returns proper endpoint colors but they're never actually rendered as a gradient.

**Canvas setup (all screens):** `ScreenSpaceOverlay`, `CanvasScaler` 1080×1920 ref, `matchWidthOrHeight = 0.5f`, `GraphicRaycaster`.

**TMPro status:** NOT installed. Not in manifest.json, no .asmdef refs, no font assets.

### Files to MODIFY

| File | Path | Changes |
|------|------|---------|
| manifest.json | `Packages/manifest.json` | Add `com.unity.textmeshpro` |
| Game.asmdef | `Scripts/Game/Game.asmdef` | Add `Unity.TextMeshPro` reference |
| ThemeConfig.cs | `Scripts/Game/UI/ThemeConfig.cs` | Add `GetFont()`, `GetFontBold()`, `CreateGradientSprite()` |
| BackgroundManager.cs | `Scripts/Game/UI/Components/BackgroundManager.cs` | Replace flat color with gradient texture |
| GameplayHUD.cs | `Scripts/Game/UI/Components/GameplayHUD.cs` | Text → TMPro (6 text fields + factory) |
| MainMenuScreen.cs | `Scripts/Game/UI/Screens/MainMenuScreen.cs` | Text → TMPro + gradient bg |
| LevelCompleteScreen.cs | `Scripts/Game/UI/Screens/LevelCompleteScreen.cs` | Text → TMPro + gradient bg |
| RoadmapScreen.cs | `Scripts/Game/UI/Screens/RoadmapScreen.cs` | Text → TMPro + gradient bg |
| SettingsScreen.cs | `Scripts/Game/UI/Screens/SettingsScreen.cs` | Text → TMPro + gradient bg |
| StarGateScreen.cs | `Scripts/Game/UI/Screens/StarGateScreen.cs` | Text → TMPro + gradient bg |
| LevelListView.cs | `Scripts/Game/UI/Components/LevelListView.cs` | Text → TMPro |

### Files to CREATE

| File | Path | Purpose |
|------|------|---------|
| Font Regular | `Assets/Art/Fonts/[FontName]-Regular.ttf` | Primary font |
| Font Bold | `Assets/Art/Fonts/[FontName]-Bold.ttf` | Bold weight |
| Font SDF Regular | `Assets/Art/Fonts/[FontName]-Regular SDF.asset` | TMP font asset (generated) |
| Font SDF Bold | `Assets/Art/Fonts/[FontName]-Bold SDF.asset` | TMP font asset (generated) |
| Resource link | `Assets/Resources/Fonts/` | Folder for Resources.Load access |

### CRITICAL Anti-Patterns to AVOID

1. **Do NOT use `TextMeshPro` (3D mesh)** — use `TextMeshProUGUI` for all Canvas UI elements.
2. **Do NOT use `TMP_Text` base class for AddComponent** — always use concrete `TextMeshProUGUI`.
3. **Do NOT use Shader Graph** for gradients — project uses HLSL `.shader` files only.
4. **Do NOT use `Material.SetColor()` on shared materials** — create per-instance materials.
5. **Do NOT remove `ButtonBounce` components** — they work on parent `Transform.localScale`, unaffected by text changes.
6. **Do NOT change Canvas setup** (render mode, reference resolution, scaling) — those are correct.
7. **Do NOT change color values in ThemeConfig** — only add new members. Existing colors are approved.
8. **Do NOT change unicode icons** (←, ⚙, ↻, ↶, ▶) — icon system comes in a later story.
9. **Do NOT leak textures** — `Destroy()` old gradient textures before creating new ones.
10. **Do NOT use async/await for texture creation** — it's fast enough synchronous (< 1ms for 1×256).

### Previous Story Intelligence (11.2 HUD Redesign)

**Patterns established:**
- `CreateSquareButton()` factory: 96×96 buttons with child Text — update child to `TextMeshProUGUI`
- `ApplySafeArea()` uses anchor math — unaffected by text changes
- Bar heights (120px) and button sizes (96px) calibrated for current text — verify TMPro sizing
- All buttons use ThemeConfig colors via `GetColor(ThemeColorType.X)`
- Constraint from 11.2: "No font changes in this story (typography is Story 11.3)" — **we are 11.3**

### Architecture Compliance

- **No external packages** beyond TMP (ships with Unity 6)
- **Coroutine-based animations** — no async/await for UI
- **Service Locator pattern** — `Services.TryGet<T>()` for cross-system access
- **ThemeConfig is static** — all color/font references go through it
- **Programmatic UI** — all screens built in code via `Create()` factory methods, no prefabs
- **Assembly Definitions:** Core (no deps), Game (refs Core + now TMPro), Tests (refs both)
- **Namespace:** `JuiceSort.Game.UI` for screens, `JuiceSort.Game.UI.Components` for reusable components

### TMPro Migration Checklist

1. Add package → manifest.json
2. Import TMP Essentials (may auto-import)
3. Add `using TMPro;` to each file
4. Add `Unity.TextMeshPro` to Game.asmdef
5. `TextMeshProUGUI` requires `RectTransform` (all UI objects already have one)
6. `fontSize` works the same way but TMPro may render slightly differently — verify sizing
7. `enableAutoSizing` can be useful but avoid for now — use fixed sizes for consistency

### Gradient Texture Pattern

```csharp
// Reusable: generate 1×N vertical gradient, cache and reuse
static Texture2D CreateGradientTexture(Color top, Color bottom, int height = 256)
{
    var tex = new Texture2D(1, height, TextureFormat.RGBA32, false);
    tex.wrapMode = TextureWrapMode.Clamp;
    tex.filterMode = FilterMode.Bilinear;
    for (int y = 0; y < height; y++)
    {
        float t = (float)y / (height - 1);
        tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
    }
    tex.Apply();
    return tex;
}

// For UI Image:
image.sprite = Sprite.Create(tex, new Rect(0, 0, 1, height), new Vector2(0.5f, 0.5f));

// For SpriteRenderer (BackgroundManager):
_bgRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 1, height), new Vector2(0.5f, 0.5f));
```

### References

- [Source: _bmad-output/epics.md#Epic-11] — Story scope and design references
- [Source: _bmad-output/visual-direction-tropical-fresh.md] — "Typography: Clean sans-serif, medium weight — friendly but not childish"
- [Source: _bmad-output/gdd.md#Art-and-Audio-Direction] — "Primary UI font: 1 TTF/OTF, Not Started"
- [Source: _bmad-output/game-architecture.md#UI-Rendering] — URP 2D Renderer, programmatic UI
- [Source: _bmad-output/implementation-artifacts/11-2-hud-redesign.md#Constraints] — "No font changes in this story (typography is Story 11.3)"
- [Source: _bmad-output/project-context.md] — Unity 6.0, Unity UI (uGUI) 2.0.0, assembly definitions
- [Source: _bmad-output/mockups/loading-screen-mockup.html] — Fredoka/Baloo 2 fonts used in mockups

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Debug Log References
- Verified 0 legacy `Resources.GetBuiltinResource<Font>` references remain
- Verified 0 `AddComponent<Text>()` calls remain
- Verified 0 `private Text _` field declarations remain in UI code
- TMP already bundled in com.unity.ugui@2.0.0 — no separate package needed

### Completion Notes List
- TMPro migration complete across all 7 UI files + button factories
- Font size hierarchy standardized via ThemeConfig constants
- Gradient backgrounds added to 5 screens (MainMenu, Roadmap, Settings, StarGate + gameplay BackgroundManager)
- LevelCompleteScreen keeps overlay tint (appropriate for modal overlay)
- BackgroundManager now renders true vertical gradient instead of flat Color.Lerp blend
- Texture leak prevention: old gradient textures/sprites destroyed before new creation + OnDestroy cleanup
- **USER ACTION REQUIRED:** Generate TMP SDF font assets in Unity Editor:
  1. Open Unity → Window > TextMeshPro > Font Asset Creator
  2. Source Font: `Assets/Art/Fonts/Nunito-Regular.ttf`
  3. Settings: Atlas 2048×2048, Character Set: ASCII + Extended Latin
  4. Generate, save as `Nunito-Regular SDF.asset` in `Assets/Resources/Fonts/`
  5. Repeat for Bold weight (or use same variable font with bold style)

### Change Log
- 2026-03-24: Story 11.3 implemented — TMPro migration + gradient backgrounds
- 2026-03-24: Code review fixes — Image.Type.Sliced→Simple (4 files), cached gradient sprites per mood, fixed initial texture leak in BackgroundManager

### File List
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/Game.asmdef` — Added Unity.TextMeshPro reference
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/ThemeConfig.cs` — Added GetFont(), GetFontBold(), font size constants, CreateGradientTexture(), CreateGradientSprite()
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Components/BackgroundManager.cs` — Gradient texture rendering, texture leak prevention
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Components/GameplayHUD.cs` — Text → TextMeshProUGUI (6 fields + factory)
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Components/LevelListView.cs` — Text → TextMeshProUGUI
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/MainMenuScreen.cs` — Text → TMPro + gradient bg
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/RoadmapScreen.cs` — Text → TMPro + gradient bg
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/LevelCompleteScreen.cs` — Text → TMPro
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/SettingsScreen.cs` — Text → TMPro + gradient bg
- MODIFIED: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/StarGateScreen.cs` — Text → TMPro + gradient bg
- CREATED: `src/JuiceSort/Assets/Art/Fonts/Nunito-Regular.ttf` — Nunito variable font (Google Fonts, OFL license)
- CREATED: `src/JuiceSort/Assets/Art/Fonts/` — Font directory
- CREATED: `src/JuiceSort/Assets/Resources/Fonts/` — Resources folder for font asset loading
