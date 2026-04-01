# UI Spec: Nav Bar & Play Button Overhaul (uGUI, Programmatic)

## Context
Overhaul the main hub bottom navigation bar and play button in `HubScreen.cs`. Replace the current flat nav bar with a premium casual-game style bar with 3D icons, and upgrade the play button with gold border, 3D shadow, and animations. Part of Epic 11 (UI/UX Overhaul).

**IMPORTANT:** Do NOT modify any code from Epics 1-8. This is forward-only new work.

**IMPORTANT:** All UI is 100% programmatic — NO prefabs, NO scene objects. Everything is built inside `HubScreen.Create()` using the static factory pattern.

**IMPORTANT:** All animations use coroutines + `Mathf.Lerp` with easing functions. NO DOTween, NO LeanTween, NO Animator.

## What to Build

### 1. Nav Bar (Bottom of HubScreen)

Overhaul the existing `NavBar()` static method and `NI()` helper in `HubScreen.cs`.

**Programmatic Hierarchy (built in code, no prefabs):**
```
NavBarPanel (Image - gradient background)
├── GoldBorderTop (Image - gold line)
│   └── ShimmerBand (Image - white highlight, animated via coroutine)
├── Separator_0_1 (Image - gold vertical line)
├── NavItem_Shop
│   └── IconImage (Image - icon_shop.png)
├── Separator_1_2
├── NavItem_Leaderboard
│   └── IconImage (Image - icon_leaderboard.png)
├── Separator_2_3
├── NavItem_Home
│   ├── GlowBg (Image - gold glow sprite behind icon)
│   └── IconImage (Image - icon_home.png)
├── Separator_3_4
├── NavItem_Teams
│   └── IconImage (Image - icon_teams.png)
├── NavItem_Collections
│   └── IconImage (Image - icon_collections.png)
└── (anchored to bottom, stretch horizontal)
```

**Bar specs:**
- Height: use anchors relative to 1920 reference (anchorMin.y = 0, anchorMax.y ~= 82/1920 = 0.0427)
- Background: gradient via `BakeGradientPillTracked()` or `ThemeConfig.CreateGradientSprite()` — top `#4A2D8A`, bottom `#251060`
- Top gold border: 3.5px Image, color `#C8A84E`
- Gold shimmer: coroutine scrolls a white highlight band horizontally across the gold border, 5s loop + 1s pause

**Gold shimmer animation (coroutine):**
```csharp
IEnumerator AnimateGoldShimmer(RectTransform shimmer, float barWidth, float shimmerWidth)
{
    while (true)
    {
        shimmer.anchoredPosition = new Vector2(-shimmerWidth, 0);
        float elapsed = 0f;
        while (elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.Lerp(-shimmerWidth, barWidth, elapsed / 5f);
            shimmer.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
    }
}
```

**Separator lines:**
- Width: 1.5px (use sizeDelta.x in 1080-reference space)
- Vertical margin: 14px top and bottom (offsetMin.y = 14, offsetMax.y = -14)
- Color: `new Color(0.784f, 0.659f, 0.306f, 0.35f)` — semi-transparent gold
- Placed between each tab using anchors

**Icons:**
- Asset path: `Assets/Resources/Icons/NavBar/` — files: `icon_shop.png`, `icon_leaderboard.png`, `icon_home.png`, `icon_teams.png`, `icon_collections.png`
- Load via: `Resources.Load<Texture2D>("Icons/NavBar/icon_shop")` then `Sprite.Create()`
- Import settings: Sprite (2D and UI), filter Bilinear, max size 256
- Display size: 48x48 in RectTransform (use anchors within nav item cell)
- Fallback: if PNG not found, use existing text icon fallback pattern from current `NI()` method

**Icon States:**

INACTIVE:
- Scale: `localScale = Vector3.one`
- Color tint on Image component: `new Color(0.55f, 0.55f, 0.55f, 0.7f)`

ACTIVE:
- Scale: `localScale = Vector3.one * 1.45f`
- Color tint: `Color.white`
- Glow: child Image with `UIShapeUtils.Glow()` sprite behind icon, gold-tinted `new Color(1f, 0.84f, 0f, 0.4f)`

**Tab Switch Animation (coroutine):**
```csharp
IEnumerator AnimateTabSwitch(int oldIndex, int newIndex)
{
    float duration = 0.45f;
    float elapsed = 0f;
    var oldIcon = _navIcons[oldIndex];
    var newIcon = _navIcons[newIndex];
    var oldImg = _navImages[oldIndex];
    var newImg = _navImages[newIndex];
    Color inactiveTint = new Color(0.55f, 0.55f, 0.55f, 0.7f);

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Activate new: EaseOutBack, 0.45s
        float c1 = 1.70158f, c3 = c1 + 1f;
        float easeBack = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        newIcon.localScale = Vector3.one * Mathf.LerpUnclamped(1f, 1.45f, easeBack);
        newImg.color = Color.Lerp(inactiveTint, Color.white, t);

        // Deactivate old: EaseOutQuad, 0.25s (faster)
        float dt = Mathf.Clamp01(elapsed / 0.25f);
        float easeQuad = 1f - (1f - dt) * (1f - dt);
        oldIcon.localScale = Vector3.one * Mathf.Lerp(1.45f, 1f, easeQuad);
        oldImg.color = Color.Lerp(Color.white, inactiveTint, easeQuad);

        yield return null;
    }
    // Snap final
    newIcon.localScale = Vector3.one * 1.45f;
    newImg.color = Color.white;
    oldIcon.localScale = Vector3.one;
    oldImg.color = inactiveTint;

    // Show/hide glow children
    if (_navGlows[oldIndex] != null) _navGlows[oldIndex].SetActive(false);
    if (_navGlows[newIndex] != null) _navGlows[newIndex].SetActive(true);
}
```

**Home tab (index 2) is active by default.**

**Instance fields to add to HubScreen (wired in Create(), not serialized):**
```csharp
private RectTransform[] _navIcons;      // icon RectTransforms for scale animation
private Image[] _navImages;             // icon Images for color tint
private GameObject[] _navGlows;         // glow GameObjects for show/hide
private int _activeTab = 2;             // Home default
private Coroutine _tabSwitchCoroutine;
```

**Tab click handling:**
```csharp
// In NavBar() construction, for each tab button:
int tabIndex = i;  // capture for closure
btn.onClick.AddListener(() => {
    HapticUtils.TryVibrate();
    hub.OnTabClicked(tabIndex);
});

void OnTabClicked(int index)
{
    if (index == _activeTab) return;
    if (_tabSwitchCoroutine != null) StopCoroutine(_tabSwitchCoroutine);
    _tabSwitchCoroutine = StartCoroutine(AnimateTabSwitch(_activeTab, index));
    _activeTab = index;
    OnTabChanged?.Invoke(index);
}

public event System.Action<int> OnTabChanged;
```

**Tabs:** 0=Shop, 1=Leaderboard, 2=Home, 3=Teams, 4=Collections
- Only Home (2) is functional (stays on hub). Others are placeholder — tap switches visual state only, add `// TODO: wire to screen` comments.

---

### 2. Play Button (Above Nav Bar)

Overhaul the existing play button section in `HubScreen.Create()`. Replace Edge3, Edge2, Play, TopTint, ShL, ShR, and old Glow nodes.

**Programmatic Hierarchy:**
```
PlayButtonContainer (RectTransform - centered above nav bar)
├── GlowEffect (Image - green radial glow, behind button, CanvasGroup for alpha)
├── ShadowLayer2 (Image - gold edge shadow, offset Y +10)
├── ShadowLayer1 (Image - dark green shadow, offset Y +8)
├── ButtonFace (Image - green gradient via BakeGradientPillTracked)
│   ├── GlossOverlay (Image - white gradient, top 45%, alpha 0.35)
│   ├── ShimmerMask (RectMask2D)
│   │   └── ShimmerBand (Image - white semi-transparent band, animated)
│   └── LevelText (TextMeshProUGUI - "Level 14")
├── GoldBorderRing (Image - oversized rounded rect, #C8A84E)
└── Button + ButtonBounce components on ButtonFace
```

**Button positioning:**
- Width: ~60% of screen → anchors `(0.20, ?, 0.80, ?)` horizontally
- Positioned above nav bar with 24px margin (in 1920 reference: anchorMin.y based on nav top + 24/1920)
- Height: proportional via anchors, ~80-90px equivalent

**Colors & layers:**
- Gold border ring: oversized child behind button face, `#C8A84E`, 3.5px visible border (offsetMin/Max = -3.5)
- Button face gradient (use `BakeGradientPillTracked`): top `#85E655`, mid `#45A828`, bottom `#3A9A20`
- Shadow layer 1 (green depth): same shape, offset anchoredPosition.y = -8, color `#1E6A0E`
- Shadow layer 2 (gold edge): same shape, offset anchoredPosition.y = -10, color `#C8A84E`
- Gloss overlay: anchors top 45% of button face, white Image, alpha 0.35, pill sprite

**Text:**
- Font: `ThemeConfig.GetFont()` (Nunito-Regular SDF) with `FontStyles.Bold`
- Size: 34
- Color: white
- Extra weight: `material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.3f)`
- Outline: `material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f)`, color `#005000`
- Content: `$"<b>Level {currentLevel}</b>"` from `IProgressionManager.CurrentLevel`

**Animations (all coroutine-based):**

1. **Pulse breathing** (idle, continuous):
```csharp
IEnumerator AnimatePlayPulse(RectTransform button)
{
    Vector3 baseScale = button.localScale;
    while (true)
    {
        float t = Time.time;
        float scale = Mathf.Lerp(1f, 1.02f, (Mathf.Sin(t * Mathf.PI / 1.5f) + 1f) * 0.5f);
        button.localScale = baseScale * scale;
        yield return null;
    }
}
```

2. **Glow pulse** (behind button, uses CanvasGroup):
```csharp
IEnumerator AnimatePlayGlow(CanvasGroup glowCG, RectTransform glowRT)
{
    while (true)
    {
        float t = Time.time;
        float sine = (Mathf.Sin(t * Mathf.PI / 1.25f) + 1f) * 0.5f;
        glowCG.alpha = Mathf.Lerp(0.5f, 0.9f, sine);
        float s = Mathf.Lerp(1f, 1.08f, sine);
        glowRT.localScale = new Vector3(s, s, 1f);
        yield return null;
    }
}
```

3. **Shimmer sweep** (white band slides across inside RectMask2D):
```csharp
IEnumerator AnimatePlayShimmer(RectTransform shimmer, float buttonWidth, float shimmerWidth)
{
    while (true)
    {
        yield return new WaitForSeconds(1f);
        shimmer.anchoredPosition = new Vector2(-shimmerWidth, 0);
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 2f;
            // InOutSine easing
            float ease = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
            float x = Mathf.Lerp(-shimmerWidth, buttonWidth, ease);
            shimmer.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }
    }
}
```

4. **Press feedback** (implement `IPointerDownHandler`, `IPointerUpHandler` on a helper component or use coroutine):
```csharp
// On pointer down — shift button face down 6px, kill pulse
void OnPlayPointerDown()
{
    if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
    _playButton.anchoredPosition = _playButtonBasePos + new Vector2(0, -6f);
    _playButton.localScale = Vector3.one; // snap to normal scale
}

// On pointer up — return to base position, restart pulse
void OnPlayPointerUp()
{
    _playButton.anchoredPosition = _playButtonBasePos;
    _pulseCoroutine = StartCoroutine(AnimatePlayPulse(_playButton));
}
```

**Instance fields to add to HubScreen:**
```csharp
private RectTransform _playButton;
private Vector2 _playButtonBasePos;
private Coroutine _pulseCoroutine;
private Coroutine _glowCoroutine;
private Coroutine _shimmerCoroutine;
private Coroutine _goldShimmerCoroutine;
```

**All coroutines started in the `Create()` method (after hierarchy built), stopped in `OnDestroy()`.**

---

## File Changes

**Modify:**
- `Assets/Scripts/Game/UI/Screens/HubScreen.cs` — overhaul `NavBar()`, `NI()`, play button section in `Create()`, replace `Anim()` coroutine

**Add assets:**
- `Assets/Resources/Icons/NavBar/icon_shop.png`
- `Assets/Resources/Icons/NavBar/icon_leaderboard.png`
- `Assets/Resources/Icons/NavBar/icon_home.png`
- `Assets/Resources/Icons/NavBar/icon_teams.png`
- `Assets/Resources/Icons/NavBar/icon_collections.png`

**No new script files.** No prefabs. No scene changes.

## Existing Code to Reuse

| Component | Path | Usage |
|-----------|------|-------|
| `UIShapeUtils` | `Scripts/Game/UI/Components/UIShapeUtils.cs` | `WhiteRoundedRect()`, `WhiteCircle()`, `Glow()`, `WhitePill()` |
| `ThemeConfig` | `Scripts/Game/UI/ThemeConfig.cs` | `GetFont()`, `CreateGradientSprite()` |
| `ButtonBounce` | `Scripts/Game/UI/Components/ButtonBounce.cs` | Scale 92% -> 105% -> 100% press feedback |
| `HapticUtils` | `Scripts/Game/UI/Components/HapticUtils.cs` | `TryVibrate()` on all taps |
| `BakeGradientPillTracked()` | `HubScreen.cs` | Multi-stop gradient baked into rounded rect texture |
| Helper methods | `HubScreen.cs` | `R()`, `Img()`, `Txt()`, `V()`, `ApplySafeArea()` |

## Dependencies
- TextMeshPro (already in project)
- No additional packages required

## DO NOT
- Use DOTween, LeanTween, or any external tween library
- Create separate controller scripts (NavBarController.cs, PlayButtonController.cs)
- Create prefabs — all UI is programmatic
- Add new fonts — use existing Nunito-Regular SDF via ThemeConfig
- Use [SerializeField] — all references wired programmatically in Create()
- Modify any existing gameplay scripts or Epics 1-8 code
- Change the existing GameplayHUD
- Remove any existing UI outside of HubScreen's nav bar and play button sections

## Acceptance Criteria
1. Nav bar shows 5 tabs with PNG icons, no labels
2. Home tab active by default at 1.45x scale with gold glow
3. Tap any tab -> bounce animation on new (EaseOutBack, 0.45s), shrink on old (EaseOutQuad, 0.25s)
4. Inactive icons dimmed with gray tint (0.55, 0.55, 0.55, 0.7)
5. Gold separators visible between tabs
6. Gold shimmer animates on top border (5s horizontal scroll)
7. Play button has gold border ring + 3D shadow layers (green depth + gold edge)
8. Play button pulse/glow runs continuously via coroutines
9. Shimmer sweep visible on button face (2s sweep, 1s pause)
10. Press shifts button down 6px, release springs back
11. Button width ~60% of screen, centered above nav bar
12. "Level N" text updates from IProgressionManager
13. 60fps on Android — no per-frame allocations in coroutine loops
