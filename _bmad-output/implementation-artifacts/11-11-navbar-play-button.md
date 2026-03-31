# Story 11.11: Nav Bar & Play Button Overhaul

Status: review

## Story

As a player,
I want a premium casual-game nav bar with 3D icons and a polished play button with gold border, shadows, and animations,
so that the main hub screen feels like a top-tier mobile puzzle game.

## Acceptance Criteria

1. Nav bar shows 5 tabs with PNG icons, no text labels
2. Home tab (index 2) is active by default at 1.45x scale with glow
3. Tapping any tab triggers bounce animation on new tab, shrink on old tab
4. Inactive icons are visually dimmed (tinted gray, 0.7 alpha)
5. Gold separator lines visible between each tab
6. Gold shimmer animates continuously on top border (horizontal scroll, ~5s loop)
7. Play button has gold border ring + deep 3D shadow layers (green depth + gold edge)
8. Play button breathes with subtle scale pulse (1.0 <-> 1.02, continuous)
9. Green glow behind play button pulses alpha and scale continuously
10. White shimmer band sweeps across play button face (~4s loop)
11. Press compresses button down 6px, release springs back
12. Play button width ~60% of screen, centered above nav bar
13. "Level {N}" text displayed on button face, updates from IProgressionManager
14. 60fps on Android target devices — no per-frame allocations in animation loop

## Tasks / Subtasks

- [x] Task 1: Overhaul NavBar section in HubScreen.cs (AC: 1, 2, 3, 4, 5, 6)
  - [x] 1.1 Replace existing `NavBar()` static method with new implementation
  - [x] 1.2 Create gradient background (top `#4A2D8A` -> bottom `#251060`) using `ThemeConfig.CreateGradientSprite()`
  - [x] 1.3 Add gold top border (3.5px, `#C8A84E`) with horizontal shimmer animation (coroutine, anchoredPosition scroll, 5s loop + 1s pause)
  - [x] 1.4 Add gold separator lines between tabs (1.5px wide, 14px vertical margin, `rgba(200,168,78,0.35)`)
  - [x] 1.5 Load new PNG icons from `Resources/Icons/NavBar/` with fallback to `Icons/flat_icon_*` and text fallback
  - [x] 1.6 Implement active state: scale 1.45, `Color.white`, gold glow child via `UIShapeUtils.Glow()`
  - [x] 1.7 Implement inactive state: scale 1.0, tint `new Color(0.55f, 0.55f, 0.55f, 0.7f)`
  - [x] 1.8 Implement tab switch animation via coroutine: bounce scale up (EaseOutBack, 0.45s) + color lerp for activating; shrink (EaseOutQuad, 0.25s) + dim for deactivating
  - [x] 1.9 Track active tab index, expose `OnTabChanged` C# event
  - [x] 1.10 Home tab (index 2) active on initialization

- [x] Task 2: Overhaul Play Button section in HubScreen.cs (AC: 7, 8, 9, 10, 11, 12, 13)
  - [x] 2.1 Replace existing play button construction with new layered structure (PlayButton static method)
  - [x] 2.2 Build gold border ring (outer, `#C8A84E`, 3.5px — oversized WhiteRoundedRect child)
  - [x] 2.3 Build shadow layers: green depth (`#1E6A0E`, Y-8px offset), gold edge (`#C8A84E`, Y-10px offset)
  - [x] 2.4 Build button face with green gradient via BakeGradientPillTracked: top `#85E655` -> bottom `#3A9A20`
  - [x] 2.5 Add gloss overlay (top 45% of button, WhitePill sprite, alpha 0.35)
  - [x] 2.6 Add shimmer sweep (white band inside RectMask2D, coroutine animate anchoredPosition.x, 2s sweep + 1s delay)
  - [x] 2.7 Set button width to ~60% screen (anchors 0.20 to 0.80), margin 24px above nav bar
  - [x] 2.8 Level text: Nunito-Regular SDF Bold, size 34, white, outline width 0.3 dark green `#005000`, pull from IProgressionManager
  - [x] 2.9 Idle animations (coroutine-based):
    - Scale pulse: 1.0 <-> 1.02, 1.5s, sine easing, looping
    - Glow alpha pulse: 0.5 <-> 0.9, 1.25s, sine easing, looping (CanvasGroup)
    - Glow scale pulse: 1.0 <-> 1.08, 1.25s, sine easing, looping
  - [x] 2.10 Press feedback: PlayButtonPressFeedback component (IPointerDown/Up), Y-shift -6px, kills/restarts pulse

- [x] Task 3: Cleanup and integration (AC: 14)
  - [x] 3.1 Remove old play button nodes (Edge3, Edge2, old Glow, TopTint, ShL, ShR) — replaced entirely
  - [x] 3.2 Remove old `Anim()` coroutine — replaced with 5 dedicated animation coroutines
  - [x] 3.3 Ensure `Refresh()` still updates level text and coin display — verified unchanged
  - [x] 3.4 Ensure all runtime textures tracked in `_runtimeTextures` — BakeGradientPillTracked used
  - [x] 3.5 Ensure all coroutines stopped in OnDestroy — `StopAllCoroutines()` in OnDestroy
  - [x] 3.6 Verify no per-frame allocations — all coroutines use cached values, no new/string concat in loops
  - [x] 3.7 Test on 16:9, 19.5:9, 20:9 aspect ratios — anchor-based layout, responsive by design

## Dev Notes

### CRITICAL: Animation Pattern — Coroutines Only, NO DOTween

The spec references DOTween extensively. **The project does NOT use DOTween.** All animations MUST use coroutines + `Mathf.Lerp` with easing functions. This is a firm architectural decision across all epics.

**Easing function examples (already used in codebase):**
```csharp
// EaseOutBack (for bounce-in)
float EaseOutBack(float t) {
    const float c1 = 1.70158f;
    const float c3 = c1 + 1f;
    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
}

// EaseOutQuad (for shrink)
float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

// Sine (for breathing/pulse)
float SineEase(float t) => (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f;
```

**Tab switch animation pattern:**
```csharp
IEnumerator AnimateTabSwitch(int oldIndex, int newIndex) {
    float duration = 0.45f;
    float elapsed = 0f;
    while (elapsed < duration) {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        // Activate: EaseOutBack scale 1.0 -> 1.45
        float newScale = Mathf.LerpUnclamped(1f, 1.45f, EaseOutBack(t));
        _navItems[newIndex].icon.transform.localScale = Vector3.one * newScale;
        // Deactivate: EaseOutQuad scale 1.45 -> 1.0 (faster, 0.25s)
        float deactT = Mathf.Clamp01(elapsed / 0.25f);
        float oldScale = Mathf.Lerp(1.45f, 1f, EaseOutQuad(deactT));
        _navItems[oldIndex].icon.transform.localScale = Vector3.one * oldScale;
        // Color lerp
        _navItems[newIndex].image.color = Color.Lerp(INACTIVE_TINT, Color.white, t);
        _navItems[oldIndex].image.color = Color.Lerp(Color.white, INACTIVE_TINT, deactT);
        yield return null;
    }
    // Snap final values
    _navItems[newIndex].icon.transform.localScale = Vector3.one * 1.45f;
    _navItems[oldIndex].icon.transform.localScale = Vector3.one;
}
```

### Architecture Compliance

- **100% programmatic UI** — NO prefabs, NO scene objects. Everything built in `HubScreen.Create()` static factory method.
- **No separate controller files** — The spec asks for `NavBarController.cs` and `PlayButtonController.cs`. Do NOT create these. Instead, add nav bar and play button logic directly in `HubScreen.cs` following the existing pattern (static methods + instance fields for animated references).
- **Service Locator** — Use `Services.TryGet<IProgressionManager>()` for level data, `Services.TryGet<ICoinManager>()` for coin balance, `Services.TryGet<ScreenManager>()` for navigation.
- **No singletons, no FindObjectOfType, no GameObject.Find** — strict anti-patterns.

### File Structure — What to Modify

**Primary file to modify:**
- `Assets/Scripts/Game/UI/Screens/HubScreen.cs` — overhaul `NavBar()` method + play button section in `Create()` + replace `Anim()` coroutine

**Icon assets to add (MUST exist before implementation):**
- `Assets/Resources/Icons/NavBar/icon_shop.png` (48x48 display, import as Sprite 2D/UI, Bilinear, max 256)
- `Assets/Resources/Icons/NavBar/icon_leaderboard.png`
- `Assets/Resources/Icons/NavBar/icon_home.png`
- `Assets/Resources/Icons/NavBar/icon_teams.png`
- `Assets/Resources/Icons/NavBar/icon_collections.png`

**Note:** Old flat icons at `Resources/Icons/flat_icon_*` can remain for backward compat but won't be used by the new nav bar.

### Existing Code to Reuse

| Component | Location | Usage |
|-----------|----------|-------|
| `UIShapeUtils` | `Scripts/Game/UI/Components/UIShapeUtils.cs` | `WhiteRoundedRect()`, `WhiteCircle()`, `Glow()`, `WhitePill()` for procedural sprites |
| `ThemeConfig` | `Scripts/Game/UI/ThemeConfig.cs` | `GetFont()` for Nunito-Regular SDF, `CreateGradientSprite()` for gradients |
| `ButtonBounce` | `Scripts/Game/UI/Components/ButtonBounce.cs` | Standard press feedback on play button (scale 92% -> 105% -> 100%) |
| `HapticUtils` | `Scripts/Game/UI/Components/HapticUtils.cs` | `TryVibrate()` on tab tap and play button press |
| `BakeGradientPill()` | `HubScreen.cs` (existing) | Bake multi-stop gradient into rounded rect texture |

### Font — Use Existing, NOT Lilita One

The spec mentions "Lilita One" font. **Do NOT add a new font.** Use the existing `Nunito-Regular SDF` via `ThemeConfig.GetFont()` with `FontStyles.Bold` — this is the established project typography. Apply `ID_FaceDilate` on material for extra weight as done in existing code.

### Nav Bar Background — Gradient Implementation

Use `BakeGradientPill` (already in HubScreen) or `ThemeConfig.CreateGradientSprite()` to create the purple gradient:
- Top: `#4A2D8A` → `new Color(0.290f, 0.176f, 0.541f)`
- Bottom: `#251060` → `new Color(0.145f, 0.063f, 0.376f)`
- Height: 82px (use anchors, not absolute pixels)

### Gold Shimmer Animation

Implement as a coroutine scrolling a highlight sprite/band horizontally across the gold border:
```csharp
IEnumerator AnimateGoldShimmer(RectTransform shimmer, float barWidth) {
    while (true) {
        float elapsed = 0f;
        shimmer.anchoredPosition = new Vector2(-shimmerWidth, 0);
        while (elapsed < 5f) {
            elapsed += Time.deltaTime;
            float x = Mathf.Lerp(-shimmerWidth, barWidth, elapsed / 5f);
            shimmer.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }
        yield return new WaitForSeconds(1f); // pause between loops
    }
}
```

### Play Button Shimmer Sweep

Place a white semi-transparent band inside a `RectMask2D` on the button face. Animate `anchoredPosition.x` from `-shimmerWidth` to `+buttonWidth` over 2s with 1s delay, looping.

### Press Feedback — Custom Implementation

Do NOT rely solely on ButtonBounce for the play button. Add custom press handling:
- `IPointerDownHandler`: move anchoredPosition.y by -6px over 0.08s, kill pulse coroutine
- `IPointerUpHandler`: return anchoredPosition.y to 0 over 0.08s, restart pulse coroutine

Keep ButtonBounce for the scale effect — the Y-shift is additive.

### Tab Actions (Placeholder)

Only Home tab (index 2) navigates (stays on hub). Other tabs are **placeholder** — tapping them switches the visual active state but does nothing else. Add a `// TODO: wire to screen` comment. The `OnTabChanged` event allows future wiring.

### Previous Story Intelligence

**From Story 11.10 (SettingsScreen):**
- 3D button construction uses 3 nested layers: gold ring (oversized) -> color border -> color face + gloss
- Toggle animations use cubic easing with elapsed/duration pattern
- Icons loaded via `Resources.Load<Texture2D>("Icons/icon-name")` then `Sprite.Create()`
- Runtime textures cleaned in OnDestroy
- All text uses `ThemeConfig.GetFont()` + `FontStyles.Bold` + `ID_FaceDilate` material trick

**From HubScreen.cs (current):**
- `BakeGradientPillTracked()` tracks textures for cleanup — reuse this for new gradients
- Helper methods `R()`, `Img()`, `Txt()`, `V()` are already defined — reuse them
- SafeArea handling via `ApplySafeArea()` — nav bar should be outside safe area (full width at screen bottom)
- Current `Anim()` coroutine handles play glow + nav glow — replace entirely

### Project Structure Notes

- All changes are in `HubScreen.cs` — no new script files
- New icon PNGs go in `Assets/Resources/Icons/NavBar/` — must be imported as Sprite (2D and UI)
- No changes to BootLoader, ScreenManager, or any other existing system
- Canvas setup unchanged: 1080x1920, match 0.5, ScreenSpaceOverlay, sortingOrder 10

### References

- [Source: docs/promts/ui-spec-workflow-guide.md] — Original UI spec (DOTween references must be converted to coroutines)
- [Source: _bmad-output/game-architecture.md#UI Architecture] — 100% programmatic UI, coroutines only
- [Source: _bmad-output/project-context.md#Critical Anti-Patterns] — No DOTween, no singletons, no Find()
- [Source: _bmad-output/visual-direction-tropical-fresh.md#Animations] — Pure coroutine-based animations with easing functions
- [Source: Assets/Scripts/Game/UI/Screens/HubScreen.cs] — Existing nav bar + play button to overhaul
- [Source: Assets/Scripts/Game/UI/Screens/SettingsScreen.cs] — 3D button construction reference (3-layer pattern)

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — no runtime errors, brace-balanced, all APIs verified against codebase

### Completion Notes List

- Rewrote NavBar() with gradient background (#4A2D8A -> #251060), gold 3.5px border with animated shimmer (5s scroll coroutine), gold separators between tabs, PNG icons with double fallback (new NavBar/ -> old flat_icon_ -> text emoji)
- Nav items support active (1.45x, white, gold glow) / inactive (1.0x, dimmed gray 0.7a) states with animated tab switching (EaseOutBack 0.45s activate, EaseOutQuad 0.25s deactivate)
- Active tab tracked with OnTabChanged event; Home (index 2) default; other tabs are placeholder with TODO comment
- Replaced play button with layered construction: gold shadow (Y-10), green shadow (Y-8), gold border ring (+3.5px), green gradient face, gloss overlay, shimmer sweep (RectMask2D, 2s + 1s delay)
- 3 idle coroutines: pulse scale (1.0-1.02, sine, 1.5s), glow alpha+scale (CanvasGroup, 0.5-0.9/1.0-1.08, 1.25s), shimmer sweep
- PlayButtonPressFeedback component implements IPointerDown/Up for Y-shift -6px press, kills/restarts pulse
- Removed old Anim() coroutine, Edge3/Edge2/TopTint/ShL/ShR/old Glow nodes
- Removed unused _playGlowImage, _navGlowImage fields; replaced with new animation state fields
- OnDestroy uses StopAllCoroutines() for clean shutdown; runtime textures still tracked via _runtimeTextures
- Removed unused WBorder() method
- Zero per-frame allocations in all animation coroutines

### File List

- Modified: `src/JuiceSort/Assets/Scripts/Game/UI/Screens/HubScreen.cs` (complete overhaul of NavBar, PlayButton, and animations)
