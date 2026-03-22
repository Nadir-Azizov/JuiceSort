# Story 10.1: Shader Liquid Fill

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to see smooth, gradient-filled liquid in each bottle,
so that the game looks premium and visually rivals top competitors like Magic Sort.

## Acceptance Criteria

1. **HLSL/ShaderLab shader created** — `Assets/Art/Shaders/LiquidFill.shader` (plain text, URP 2D compatible) renders smooth liquid fills
2. **6-layer support** — Shader supports up to 6 color bands via HLSL arrays (`_FillLevels[6]`, `_LayerColors[6]`, `_LayerCount`) matching `MaxSlots = 6`
3. **Contiguous band rendering** — Liquid renders as continuous fill from bottom with stacked color bands (not separate rectangles per slot)
4. **Visual headroom** — Logically full bottles render liquid to ~80% height; top ~20% remains empty glass (`_MaxVisualFill` property, default 0.80)
5. **Completed dimming** — `_DimMultiplier` property (default 1.0) dims all layer colors when set to 0.7 for completed bottles
6. **Per-bottle material instance** — Each bottle gets its own runtime `new Material(shader)` for independent properties; material destroyed in OnDestroy to prevent leaks
7. **BottleContainerView refactored** — Internal rendering changed from sprite slot array to shader material; external API preserved (Refresh, Select, Deselect, PlayCompletionEffect)
8. **LiquidMaterialController** — New MonoBehaviour manages per-bottle shader parameters (band fills, colors, dim, headroom) — updates only on state change, never in Update
9. **Visual parity** — Selection animation, completion shimmer, glass sparkles all still work correctly
10. **Memory cleanup** — Runtime material instances destroyed in OnDestroy when bottles are destroyed

## Tasks / Subtasks

- [x] Task 1: Create LiquidFill HLSL shader (AC: 1, 2, 3, 4, 5)
  - [x] 1.1 Create folder `Assets/Art/Shaders/`
  - [x] 1.2 Create `LiquidFill.shader` — HLSL/ShaderLab format, URP 2D compatible
  - [x] 1.3 Declare HLSL uniform arrays: `float _FillLevels[6]` and `float4 _LayerColors[6]`
  - [x] 1.4 Add `_LayerCount` (int) — number of active bands
  - [x] 1.5 Add `_MaxVisualFill` (float, default 0.80) — visual headroom cap
  - [x] 1.6 Add `_DimMultiplier` (float, default 1.0) — completed bottle dimming
  - [x] 1.7 Add `_WobbleX`, `_WobbleZ` (float, default 0) — future wobble support
  - [x] 1.8 Implement fragment shader: stacked bands, scaled by `_MaxVisualFill`, dimmed by `_DimMultiplier`
  - [x] 1.9 Include Stencil block for SpriteMask support
  - [x] 1.10 Smooth gradient blending at band boundaries (1.5% blend zone)
  - [x] 1.11 Empty region above liquid renders transparent

- [x] Task 2: Create LiquidMaterialController (AC: 6, 8, 10)
  - [x] 2.1 Create `Scripts/Game/Puzzle/LiquidMaterialController.cs` — MonoBehaviour
  - [x] 2.2 On Initialize: `Shader.Find` + `new Material(shader)`, assign to SpriteRenderer
  - [x] 2.3 Method: `SetLayers(ContainerData data)` — contiguous band algorithm with `SetFloatArray` / `SetVectorArray`
  - [x] 2.4 Method: `SetDimmed(bool dimmed)` — sets `_DimMultiplier`
  - [x] 2.5 Method: `SetFillAmount(int bandIndex, float fill)` — for animated fills (Story 10.2)
  - [x] 2.6 No Update() — only on state change
  - [x] 2.7 OnDestroy: `Destroy(_material)` to prevent leaks

- [x] Task 3: Refactor BottleContainerView to use shader (AC: 7, 9)
  - [x] 3.1 Replace `_slotRenderers[]` with single `_liquidRenderer` + `_liquidController`
  - [x] 3.2 In Create(): single SpriteRenderer sized to bottle bounds, shader handles coloring
  - [x] 3.3 Set `maskInteraction = SpriteMaskInteraction.VisibleInsideMask`
  - [x] 3.4 Keep SpriteMask for bottle shape clipping
  - [x] 3.5 Refactor `Refresh()` → `_liquidController.SetLayers(_data)`
  - [x] 3.6 `Refresh()` calls `SetDimmed(true)` for completed bottles
  - [x] 3.7 Keep frame renderer, shimmer, sparkle unchanged
  - [x] 3.8 Preserve Select/Deselect animation unchanged
  - [x] 3.9 Preserve PlayCompletionEffect unchanged
  - [x] 3.10 `SetSlotVisible` / `SetSlotColorAndShow` kept as stubs → `SetLayers(_data)`
  - [x] 3.11 Expose `public LiquidMaterialController LiquidController` property
  - [x] 3.12 `ResetVisualState()` resets shader params (dim, wobble)
  - [x] 3.13 Removed `CompletedLiquidDim` constant

- [x] Task 4: Update BottleBoardView integration (AC: 7)
  - [x] 4.1 Verified BottleBoardView.Create — delegates to BottleContainerView.Create ✓
  - [x] 4.2 Verified RebindPuzzle — calls Refresh → LiquidMaterialController ✓
  - [x] 4.3 Verified AddContainerView — new bottles get shader material ✓

- [ ] Task 5: Validate and test (AC: 9, 3, 4) — requires Unity Editor play-test
  - [ ] 5.1 Play-test: verify liquid renders correctly for 3-5 color puzzles
  - [ ] 5.2 Verify bottles appear ~80% full when logically full (headroom visible)
  - [ ] 5.3 Verify contiguous same-color slots merge into single visual band
  - [ ] 5.4 Verify selection animation still works (lift + scale + golden glow)
  - [ ] 5.5 Verify completion shimmer still works on sorted bottles
  - [ ] 5.6 Verify completed bottles are dimmed (0.7x color)
  - [ ] 5.7 Verify glass sparkles still work on idle bottles
  - [ ] 5.8 Verify empty containers render correctly (all fills = 0, fully transparent liquid area)

## Dev Notes

### What you're replacing

Current rendering: `BottleContainerView` creates `N` SpriteRenderers (one per slot), each a solid-colored rectangle clipped by SpriteMask. This produces hard-edged color blocks that fill the bottle to the rim.

New rendering: Single SpriteRenderer per bottle with `LiquidFill.shader` material. The shader renders smooth contiguous color bands with gradient blending, 80% visual headroom, and completed dimming. Premium liquid visuals rivaling Magic Sort.

### Why HLSL/ShaderLab instead of Shader Graph

`.shadergraph` files are Unity Editor binary/JSON assets that cannot be created or edited from CLI. Since our workflow is AI-assisted via Claude Code, we use plain-text HLSL/ShaderLab `.shader` files for full programmatic control.

The shader must be URP 2D compatible. Use the `Sprites/Default` or custom unlit shader structure compatible with URP 2D Renderer. Key: support `SpriteRenderer` color tinting and `SpriteMask` interaction.

### Existing shader asset — USE AS REFERENCE ONLY

There's an imported shader at `Assets/GameAI/Bottle Fill Shader Graph/Runtime/BottleFill_Pro.shadergraph` with a demo controller at `BottleFillDemoController.cs`. This asset exposes: `_FillAmount`, `_SpriteHeight`, `_BandScale`, `_BandCount`, `_BandColor1-4`.

**DO NOT use this asset directly in production.** Create a new clean `.shader` file. You can reference the demo controller for property-setting patterns.

### Contiguous color band algorithm (CRITICAL)

The `SetLayers` method must NOT map slots 1:1 to shader layers. It must merge consecutive same-color slots into bands:

```csharp
private const int MaxBands = 6;
private float[] _fillArray = new float[MaxBands];
private Vector4[] _colorArray = new Vector4[MaxBands];

public void SetLayers(ContainerData data)
{
    int slotCount = data.SlotCount;
    int bandIndex = 0;

    // Walk slots bottom-to-top, merge same-color runs into bands
    int i = 0;
    while (i < slotCount && bandIndex < MaxBands)
    {
        var color = data.GetSlot(i);
        if (color == DrinkColor.None)
            break; // empty slot = end of liquid

        // Count consecutive slots of same color
        int runLength = 1;
        while (i + runLength < slotCount && data.GetSlot(i + runLength) == color)
            runLength++;

        // Band height = (runLength / totalSlots)
        _fillArray[bandIndex] = (float)runLength / slotCount;
        Color c = ThemeConfig.GetDrinkColor(color);
        _colorArray[bandIndex] = new Vector4(c.r, c.g, c.b, c.a);

        bandIndex++;
        i += runLength;
    }

    // Clear unused bands
    for (int b = bandIndex; b < MaxBands; b++)
    {
        _fillArray[b] = 0f;
        _colorArray[b] = Vector4.zero;
    }

    _material.SetFloatArray("_FillLevels", _fillArray);
    _material.SetVectorArray("_LayerColors", _colorArray);
    _material.SetInt("_LayerCount", bandIndex);
}
```

**Example:** Container [Mango, Mango, Berry, Empty] (4 slots):
- Band 0: Fill=0.50, Color=Mango (bottom 50% × 0.80 = 40% of bottle)
- Band 1: Fill=0.25, Color=Berry (next 25% × 0.80 = 20% of bottle)
- Bands 2-5: Fill=0, cleared
- Total visual fill: 0.75 × 0.80 = 60% of bottle height
- Top 40% is empty glass

**Example:** Container [Berry, Berry, Berry, Berry] (4 slots, full):
- Band 0: Fill=1.00, Color=Berry (100% × 0.80 = 80% of bottle)
- Bands 1-5: Fill=0, cleared
- Liquid fills to 80% — top 20% is empty glass (headroom)

### Visual headroom: _MaxVisualFill = 0.80

Even when a bottle is logically 100% full, liquid renders to only ~80% of bottle height. The top ~20% always shows empty glass. This provides:
- Natural realistic appearance (real bottles have space at the top)
- Headroom for liquid wobble animation (Story 10.4)
- Space for pour stream entry point (Story 10.3)
- Cork/cap placement area (Story 10.5)

`_MaxVisualFill` is a shader property so it can be tuned. The shader multiplies all fill heights by this value.

### Shader structure (CRITICAL)

The shader MUST include stencil support for SpriteMask clipping, HLSL arrays for band data, and URP 2D compatibility:

```hlsl
Shader "JuiceSort/LiquidFill"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaxVisualFill ("Max Visual Fill", Float) = 0.80
        _DimMultiplier ("Dim Multiplier", Float) = 1.0
        _WobbleX ("Wobble X", Float) = 0.0
        _WobbleZ ("Wobble Z", Float) = 0.0

        // SpriteMask stencil (set automatically by Unity based on maskInteraction)
        _StencilRef ("Stencil Ref", Float) = 0
        _StencilComp ("Stencil Comp", Float) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
        }

        Pass
        {
            HLSLPROGRAM
            // ... vertex/fragment programs ...

            // HLSL arrays — set from C# via SetFloatArray / SetVectorArray
            float _FillLevels[6];
            float4 _LayerColors[6];
            int _LayerCount;
            float _MaxVisualFill;
            float _DimMultiplier;

            half4 frag(v2f IN) : SV_Target
            {
                float y = IN.texcoord.y; // 0=bottom, 1=top
                float cumFill = 0;
                half4 liquidColor = half4(0, 0, 0, 0);

                for (int i = 0; i < _LayerCount; i++)
                {
                    float bandHeight = _FillLevels[i] * _MaxVisualFill;
                    float bandTop = cumFill + bandHeight;

                    if (y >= cumFill && y < bandTop)
                    {
                        liquidColor = _LayerColors[i] * _DimMultiplier;
                        liquidColor.a = 1.0;
                        break;
                    }
                    cumFill = bandTop;
                }

                return liquidColor; // alpha=0 in empty region
            }
            ENDHLSL
        }
    }
}
```

**Key points:**
- `_FillLevels[6]` / `_LayerColors[6]` are HLSL arrays — set from C# via `material.SetFloatArray()` / `material.SetVectorArray()`
- Individual ShaderLab properties (`_Fill0`, `_Fill1`, etc.) do NOT work for dynamic indexing — must use arrays
- Stencil block is REQUIRED for SpriteMask clipping — without it, liquid renders as unclipped rectangle
- If HLSL array indexing causes issues on some Android GPUs, fall back to unrolled if/else chain

### Material creation (runtime — no .mat file)

`.mat` files are binary Unity Editor assets that cannot be created from CLI. Create material at runtime:

```csharp
private Material _material;

public void Initialize(SpriteRenderer renderer, int slotCount)
{
    var shader = Shader.Find("JuiceSort/LiquidFill");
    if (shader == null)
    {
        Debug.LogError("[LiquidMaterialController] LiquidFill shader not found");
        return;
    }
    _material = new Material(shader);
    renderer.material = _material; // per-instance for independent properties
    _slotCount = slotCount;
}

private void OnDestroy()
{
    if (_material != null)
        Destroy(_material); // cleanup runtime material instance
}
```

### Stub methods for PourAnimator compatibility (CRITICAL)

PourAnimator.cs lines 65-68 call `SetSlotVisible()` and `SetSlotColorAndShow()`. These methods MUST NOT be deleted — PourAnimator still depends on them until Story 10-2 refactors it. Instead, re-implement them as thin stubs:

```csharp
// Keep these until Story 10-2 removes them
public void SetSlotVisible(int slotIndex, bool visible)
{
    // Stub: refresh entire shader state from current data
    if (_liquidController != null)
        _liquidController.SetLayers(_data);
}

public void SetSlotColorAndShow(int slotIndex, DrinkColor drinkColor)
{
    // Stub: refresh entire shader state from current data
    if (_liquidController != null)
        _liquidController.SetLayers(_data);
}
```

This is intentionally inefficient (full refresh per slot change) but keeps the game compiling. Story 10-2 replaces these with smooth fill animations.

### Expose LiquidController for Story 10-2

Add public property so PourAnimator can access the controller:
```csharp
public LiquidMaterialController LiquidController => _liquidController;
```

### What MUST NOT change

- `BottleContainerView` external API (Select, Deselect, Refresh, PlayCompletionEffect, Create, SetData, SetSlotVisible, SetSlotColorAndShow)
- `BottleBoardView` API (Initialize, RebindPuzzle, AddContainerView)
- `PourAnimator` static class (refactored in Story 10.2, not this story)
- `CompletionShimmer`, `GlassSparkle` components (keep attached, keep working)
- `ContainerData` data model (unchanged)
- Frame renderer (glass outline) — keep as separate SpriteRenderer
- Game logic treats all-slots-filled as "full"/"sorted" — headroom is visual only

### Architecture compliance

- Shader location: `Assets/Art/Shaders/LiquidFill.shader` (HLSL/ShaderLab plain text)
- Material: created at runtime via `new Material(shader)` — no `.mat` asset file
- LiquidMaterialController: `Scripts/Game/Puzzle/LiquidMaterialController.cs`
- URP 2D pipeline — custom unlit shader with Stencil block for SpriteMask
- Per-instance materials with `material.SetFloatArray()` / `material.SetVectorArray()` for HLSL arrays
- Destroy runtime material in OnDestroy to prevent leaks
- No Update() loops for shader updates — only on state change
- `_MaxVisualFill` configurable as shader property (tunable in Inspector on material)
- Test on aspect ratios: 16:9, 19.5:9, 20:9
- MaxBands = 6 (matching DifficultyConfig.MaxSlots)

### Previous story patterns to follow

- Static factory `Create()` pattern (used by all views)
- Service Locator access via `Services.TryGet<T>()`
- ThemeConfig for all color values (`ThemeConfig.GetDrinkColor()`)
- Coroutine-based animations with proper cleanup in OnDestroy
- CompletedLiquidDim pattern → replaced by `_DimMultiplier` shader property

### References

- [Source: _bmad-output/epics.md#Epic 10] — Liquid Visual Overhaul scope
- [Source: _bmad-output/game-architecture.md#Liquid Shader System] — Shader architecture spec
- [Source: _bmad-output/project-context.md#Liquid Shader Rules] — Implementation rules
- [Source: _bmad-output/gdd.md#Art Direction] — Tropical Fresh visual style
- [Source: Assets/GameAI/Bottle Fill Shader Graph/] — Reference shader asset (DO NOT use directly)

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Created `LiquidFill.shader` — URP 2D compatible HLSL with 6-band arrays, stencil for SpriteMask, headroom, dimming, wobble properties
- Created `LiquidMaterialController.cs` — contiguous band algorithm, runtime material management, SetLayers/SetDimmed/SetFillAmount/SetWobble API
- Refactored `BottleContainerView` — replaced N slot SpriteRenderers with single liquid renderer + shader; preserved all external APIs
- Kept SetSlotVisible/SetSlotColorAndShow as stubs for PourAnimator compatibility (removed in 10-2)
- Exposed LiquidController property for Story 10-2 PourAnimator access
- Task 5 (visual validation) requires Unity Editor play-testing

### File List

**New files:**
- `src/JuiceSort/Assets/Art/Shaders/LiquidFill.shader`
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleContainerView.cs` — replaced slot renderers with shader-based liquid rendering

### Change Log

- 2026-03-23: Story implementation complete (Tasks 1-4). Task 5 pending visual validation.
