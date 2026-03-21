# Story 8.4: Glass Sparkle Particles

Status: done

## Story

As a player,
I want to see subtle sparkles on glass containers while idle,
so that the game feels alive and visually premium even when I'm not interacting.

## Acceptance Criteria

1. **Sparkle visual** — Tiny white diamond-shaped particles briefly flash and fade on the glass surface of non-empty containers
2. **Sparkle rate** — 1-2 sparkles per container every 2-3 seconds (randomized intervals)
3. **Sparkle lifecycle** — Each sparkle: 0.3s fade-in → 0.2s hold at peak → 0.3s fade-out (0.8s total)
4. **Sparkle size** — Very small (2-4px visual), semi-transparent white at peak alpha (~0.6)
5. **Random positioning** — Sparkles spawn at random positions within the bottle mask bounds
6. **Pool limit** — Maximum 8 active sparkle particles across all containers to keep performance light
7. **Disabled during pour** — Sparkles pause during pour animations to avoid visual clutter
8. **Non-empty only** — Only containers with at least one liquid slot produce sparkles; empty containers do not sparkle

## Tasks / Subtasks

- [x] Task 1: Create GlassSparkle component (AC: 1, 2, 3, 4, 5)
  - [x] 1.1 Create `Scripts/Game/Puzzle/GlassSparkle.cs` MonoBehaviour
  - [x] 1.2 Create or source a small white diamond sprite for sparkle particles
  - [x] 1.3 Implement `SparkleLoop()` coroutine — waits random 2-3s, picks random position within bottle bounds, spawns sparkle
  - [x] 1.4 Implement `AnimateSparkle()` coroutine — fade-in 0.3s, hold 0.2s, fade-out 0.3s using SpriteRenderer alpha
  - [x] 1.5 Place sparkles on a sorting layer above liquid but below the bottle frame

- [x] Task 2: Implement sparkle pool (AC: 6)
  - [x] 2.1 Create a static pool of 8 SpriteRenderer GameObjects shared across all GlassSparkle instances
  - [x] 2.2 `SparkleLoop()` checks pool availability before spawning — if all 8 are active, skip this cycle
  - [x] 2.3 Return sparkle to pool after fade-out completes

- [x] Task 3: Integrate with gameplay (AC: 7, 8)
  - [x] 3.1 Attach `GlassSparkle` to each `BottleContainerView` during initialization
  - [x] 3.2 Add `SetSparklesEnabled(bool)` — called by GameplayManager to pause during pour animation (`_isAnimating`)
  - [x] 3.3 Only run sparkle loop if `ContainerData.IsEmpty()` is false
  - [x] 3.4 Re-check empty state after each pour (container may have become empty)

- [x] Task 4: Create sparkle sprite asset (AC: 1, 4)
  - [x] 4.1 Create a small white diamond/star sprite (16x16px with transparency)
  - [x] 4.2 Place in `Resources/Effects/sparkle.png`

## Dev Notes

### Key Files to Modify
- **New file:** `GlassSparkle.cs` — sparkle component
- `BottleContainerView.cs` — attach GlassSparkle during `Initialize()` or `Create()`
- `GameplayManager.cs` — call `SetSparklesEnabled(false)` when `_isAnimating = true`, re-enable after

### Key Architecture
- `BottleContainerView` uses SpriteMask-based rendering — sparkle sprites should use the same mask or be positioned within bottle bounds
- `BottleContainerView.Initialize()` sets up the bottle — good place to add GlassSparkle
- `_isAnimating` flag in GameplayManager tracks pour animation state

### Constraints
- No external tween libraries — pure coroutines with SpriteRenderer.color alpha Lerp
- Performance: max 8 pooled particles total, lightweight coroutines
- Sparkles should be barely noticeable — subtle ambient polish, not flashy
