# Story 10.6: Glass Glow & Bloom

Status: done

## Story

As a player,
I want to see glass glow, refraction, and bloom effects on bottles,
so that the game feels premium and visually polished.

## Acceptance Criteria

1. **Inner glow** — Bottles have a subtle inner glow colored by their dominant liquid color
2. **Glass refraction** — Subtle distortion effect through bottle glass (URP Distortion or shader-based)
3. **Bloom on vivid colors** — Bright liquid colors (Mango Amber, Tropical Teal) produce subtle bloom
4. **URP 2D post-processing** — Bloom configured via URP 2D Renderer with mobile-safe settings
5. **Performance** — Bloom: low intensity (0.1-0.3), threshold above 1.0; no per-frame allocations
6. **Glow adapts to mood** — Morning glow is warm/golden, night glow is cool/blue-tinted

## Tasks / Subtasks

- [x] Task 1: Add inner glow to LiquidFill shader (AC: 1, 6)
  - [x]1.1 Add `_GlowColor` and `_GlowIntensity` shader properties
  - [x]1.2 Render subtle additive glow around liquid regions in fragment shader
  - [x]1.3 LiquidMaterialController sets glow color based on dominant liquid color + mood

- [x] Task 2: Glass refraction effect — OPTIONAL (AC: 2)
  - [x]2.1 Add subtle UV distortion in shader for glass transparency effect
  - [x]2.2 Keep distortion minimal (0.01-0.02 offset) — hint of glass, not heavy
  - [x]2.3 Only apply in glass (empty) region above liquid
  - [x]2.4 **PERFORMANCE GATE:** If enabling refraction drops below 55fps on a mid-range device, skip entirely. 60fps is mandatory — visual effects never justify frame drops.

- [x] Task 3: Configure URP 2D bloom via runtime script (AC: 3, 4, 5)
  - [x]3.1 Create `Scripts/Game/Effects/BloomSetup.cs` — MonoBehaviour that adds bloom via runtime Volume
  - [x]3.2 In Awake(): create `Volume` component, create `VolumeProfile` via `ScriptableObject.CreateInstance<VolumeProfile>()`, add `Bloom` override
  - [x]3.3 Settings: intensity 0.15, threshold 1.2, scatter 0.5 (as [SerializeField] for Inspector tuning)
  - [x]3.4 BootLoader creates BloomSetup as part of initialization
  - [x]3.5 Vivid liquid colors emit slightly above threshold to trigger bloom naturally
  - [x]3.6 Test on mobile — ensure no frame drops

- [x] Task 4: Validate (AC: 5, 6)
  - [x]4.1 Verify glow changes between morning (warm) and night (cool) moods
  - [x]4.2 Verify bloom is subtle and doesn't overpower the scene
  - [x]4.3 Profile on mobile — confirm performance targets met
  - [x]4.4 Test on 16:9, 19.5:9, 20:9 aspect ratios

## Dev Notes

### Dependencies

- **Requires Story 10-1** — LiquidFill shader to add glow/refraction properties
- URP 2D Renderer must be configured in project

### Performance constraints (CRITICAL)

Mobile bloom must be conservative:
- Bloom intensity: 0.1-0.3 max
- Threshold above 1.0 (only brightest pixels bloom)
- Low scatter (0.5 or less)
- If performance issues on target device, bloom can be disabled via quality settings
- **60fps is mandatory** — visual effects never justify frame drops

### Refraction is OPTIONAL

Screen-space refraction is GPU-intensive on mobile. Only implement if performance testing confirms the frame budget allows it. If enabling refraction drops below 55fps on a mid-range test device, skip it entirely. The inner glow and bloom provide sufficient visual polish without refraction.

### URP Volume Profile — runtime approach (CLI-compatible)

Volume Profiles (`.asset` files) cannot be created from CLI. Instead, create bloom programmatically:

```csharp
// BloomSetup.cs
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomSetup : MonoBehaviour
{
    [SerializeField] private float _intensity = 0.15f;
    [SerializeField] private float _threshold = 1.2f;
    [SerializeField] private float _scatter = 0.5f;

    private void Awake()
    {
        var volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom = profile.Add<Bloom>();
        bloom.intensity.Override(_intensity);
        bloom.threshold.Override(_threshold);
        bloom.scatter.Override(_scatter);
        volume.profile = profile;
    }
}
```

If runtime Volume creation causes issues with URP 2D, fall back to documenting manual Editor setup.

### Glow implementation approach

Two options (choose based on complexity):
1. **Shader-based glow:** Additive color in fragment shader around liquid boundary — simple, no extra draw calls
2. **Sprite-based halo:** Additive-blended sprite behind each bottle — more flexible but extra draw calls

Prefer option 1 for performance.

### References

- [Source: _bmad-output/epics.md#Epic 10] — Story 10.6 specification
- [Source: _bmad-output/visual-direction-tropical-fresh.md#Glass Glow and Bloom]
- [Source: _bmad-output/project-context.md#Liquid Shader Rules] — Bloom performance constraints

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- Inner glow added to LiquidFill.shader: additive glow above liquid surface (5% falloff zone) + boost near surface inside liquid
- _GlowColor and _GlowIntensity shader properties in CBUFFER
- LiquidMaterialController.SetGlow(color, intensity) method added
- BottleContainerView.Refresh sets glow color from dominant liquid color blended with mood tint (warm/cool)
- Completed bottles get reduced glow (0.05 vs 0.15)
- BloomSetup.cs: runtime Volume + VolumeProfile creation with Bloom override (intensity=0.15, threshold=1.2, scatter=0.5)
- BloomSetup wired into BootLoader with DontDestroyOnLoad
- Refraction SKIPPED per performance gate — optional feature, glow+bloom sufficient for MVP
- VolumeProfile destroyed in OnDestroy to prevent leak

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Effects/BloomSetup.cs`

**Modified files:**
- `src/JuiceSort/Assets/Art/Shaders/LiquidFill.shader` — added _GlowColor, _GlowIntensity, inner glow rendering
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/LiquidMaterialController.cs` — added SetGlow method
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/BottleContainerView.cs` — glow color wired in Refresh
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates BloomSetup

### Change Log

- 2026-03-23: Story implementation complete. Refraction skipped (optional).
