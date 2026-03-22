# Story 11.1: Responsive Bottle Layout

Status: done

## Story

As a player,
I want all bottles to fit on my screen regardless of count, with proper margins and multi-row layout when needed,
so that I can always see and tap every bottle without any being cut off.

## Priority

SO HIGH — bottles are currently cut off on screen edges. This is the #1 usability bug.

## Acceptance Criteria

1. **All bottles visible** — No bottle is clipped or hidden off-screen at any bottle count (4-14 bottles)
2. **Dynamic scaling** — Bottle scale adjusts based on bottle count so all fit within the usable screen area
3. **SafeArea respected** — Layout uses fixed world-unit reserves (1.5 top, 2.0 bottom) that provide clearance for HUD + SafeArea. Actual `Screen.safeArea` pixel insets are handled at the HUD Canvas layer (Story 11.2)
4. **Aspect ratio support** — Layout works correctly on 16:9, 19.5:9, and 20:9 aspect ratios
5. **Single row (1-6 bottles)** — Bottles arranged in one centered horizontal row
6. **Two rows (7+ bottles)** — Bottles split into two centered rows (upper and lower) when count exceeds 6
7. **Minimum margins** — At least 5% screen margin on left and right edges
8. **HUD clearance** — Bottles don't overlap with top bar (level info, settings) or bottom bar (undo, restart, extra bottle)
9. **Centered layout** — Bottles are horizontally centered on screen; rows are vertically centered in the play area
10. **Consistent spacing** — Equal spacing between all bottles within a row

## Architect Decisions

- **Location:** `Game/Layout/` (not Core — game-specific concepts)
- **LayoutConfig:** Plain C# class with `LayoutConfig.Default()` static factory (BottleBoardView uses `new GameObject + AddComponent`, so `[SerializeField]` won't work). Can become SO later.
- **HUD coordination:** Fixed world-unit reserves (`topReserve=1.5f`, `bottomReserve=2.0f`). Layout never queries HUD. One-directional dependency.
- **Row distribution:** Top row gets extra on odd count. `topCount = (total+1)/2`
- **Create() signature:** Add `yPosition=0f` and `scale=0.18f` default params (backward compatible)
- **Defaults:** `horizontalMargin=0.075f`, `rowThreshold=7`, `minScale=0.10f`, `maxScale=0.20f`, `topReserve=1.5f`, `bottomReserve=2.0f`

## Tasks / Subtasks

- [x] Task 1: Create LayoutConfig and BottleLayout (AC: 7, 8)
  - [x] 1.1 Create `Scripts/Game/Layout/LayoutConfig.cs` — plain C# class (not SO)
  - [x] 1.2 Fields: `HorizontalMargin` (0.075f), `RowThreshold` (7), `TopReserve` (1.5f), `BottomReserve` (2.0f), `MinScale` (0.10f), `MaxScale` (0.20f), `MinSpacing` (0.15f)
  - [x] 1.3 Static factory method `LayoutConfig.Default()` returning instance with defaults
  - [x] 1.4 Create `BottleLayout` struct: `Vector3[] Positions`, `float Scale`, `int RowCount`, `int TopRowCount`, `int BottomRowCount`

- [x] Task 2: Create ResponsiveLayoutManager (AC: 1-4, 7, 9, 10)
  - [x] 2.1 Create `Scripts/Game/Layout/ResponsiveLayoutManager.cs` as a static class
  - [x] 2.2 Implement `CalculateLayout(int bottleCount, float camOrthoSize, float camAspect, LayoutConfig config)` returning `BottleLayout`
  - [x] 2.3 Calculate usable world-space area: width = `camOrthoSize * 2 * camAspect * (1 - 2*horizontalMargin)`, height = `camOrthoSize * 2 - topReserve - bottomReserve`
  - [x] 2.4 Determine row count: 1 for < rowThreshold, 2 for >= rowThreshold
  - [x] 2.5 For 2-row: `topCount = (total+1)/2`, `bottomCount = total - topCount`
  - [x] 2.6 Calculate uniform scale: fit widest row within usable width, clamp between minScale and maxScale
  - [x] 2.7 Calculate vertical center offset accounting for reserves (play area center = `(topReserve - bottomReserve) / 2` offset from camera center)
  - [x] 2.8 Return centered positions for each bottle (symmetric around x=0)

- [x] Task 3: Write EditMode unit tests (AC: 1-6, 9, 10)
  - [x] 3.1 Create `Scripts/Tests/EditMode/ResponsiveLayoutTests.cs`
  - [x] 3.2 Test: single-row layout (4, 5, 6 bottles) — positions centered, symmetric
  - [x] 3.3 Test: two-row transition (6 vs 7 bottles) — verify row count changes at threshold
  - [x] 3.4 Test: row distribution (7, 9, 11 bottles) — top row gets extra
  - [x] 3.5 Test: scale clamping (12-14 bottles) — scale ≥ minScale
  - [x] 3.6 Test: 1 bottle — centered at origin
  - [x] 3.7 Test: symmetry — all layouts symmetric around x=0
  - [x] 3.8 Test: no position falls in reserve zones (y bounds check)
  - [x] 3.9 Test: 16:9 and 20:9 aspect ratios produce valid layouts

- [x] Task 4: Refactor BottleBoardView.Initialize() (AC: 1-10)
  - [x] 4.1 Replace fixed spacing/scale math with `ResponsiveLayoutManager.CalculateLayout()` call
  - [x] 4.2 Apply returned positions and scale to each `BottleContainerView`
  - [x] 4.3 Remove hardcoded `bottleWorldWidth`, `bottleWorldHeight`, `0.85f` multiplier
  - [x] 4.4 Board transform.position uses play area vertical center from layout
  - [x] 4.5 Preserve existing `OnContainerTapped` event wiring and all other functionality

- [x] Task 5: Update BottleContainerView.Create() signature (AC: 2)
  - [x] 5.1 Add `float yPosition = 0f` and `float scale = 0.18f` default parameters
  - [x] 5.2 Apply passed scale instead of hardcoded `0.18f`
  - [x] 5.3 Set `localPosition = new Vector3(xPosition, yPosition, 0f)`
  - [x] 5.4 Verify all child sprites render correctly at different scales

- [x] Task 6: Manual test across aspect ratios (AC: 3, 4)
  - [x] 6.1 Test with 4, 6, 7, 10, 14 bottles in Unity Game view
  - [x] 6.2 Test on 16:9, 19.5:9, 20:9 resolutions
  - [x] 6.3 Verify no bottles clipped, margins respected, HUD clearance maintained

## Dev Notes

### Key Files to Modify
- **NEW:** `Scripts/Game/Layout/ResponsiveLayoutManager.cs` — static layout calculator
- **NEW:** `Scripts/Game/Layout/LayoutConfig.cs` — ScriptableObject for layout parameters
- **MODIFY:** `BottleBoardView.cs` — replace `Initialize()` layout math with ResponsiveLayoutManager call
- **MODIFY:** `BottleContainerView.cs` — accept dynamic scale in `Create()`

### Current Layout Code (to replace)
- `BottleBoardView.cs` lines 27-45: hardcoded `bottleWorldWidth = 0.92f`, `bottleWorldHeight = 1.83f`, `usableWidth = camWidth * 0.85f`
- `BottleContainerView.cs` line 384: hardcoded `bottleScale = 0.18f`
- `BottleBoardView.cs` line 43: board Y = `camBottom + bottleWorldHeight * 0.6f`

### Architecture
- `ResponsiveLayoutManager` is a pure static utility (no MonoBehaviour) — matches `PourAnimator`, `DifficultyScaler` pattern
- `LayoutConfig` is plain C# class with `Default()` factory — not SO (BottleBoardView isn't prefab-based)
- Fixed world-unit reserves for HUD (1.5 top, 2.0 bottom) — layout never queries HUD
- Existing `BottleBoardView` pattern preserved — only internals of `Initialize()` change
- Camera reference: `Camera.main` (cached in BottleBoardView)

### Constraints
- No external tween libraries — layout is instant (no animation in this story, animation is Story 11.5)
- Must work with existing sprite-based rendering (shader refactor is Epic 10)
- `BottleContainerView.Create()` gets default params (backward compatible, no breaking changes)
