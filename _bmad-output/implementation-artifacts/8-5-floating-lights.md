# Story 8.5: Floating Light Particles

Status: done

## Story

As a player,
I want to see warm floating light particles drifting in the background,
so that the tropical atmosphere feels alive and immersive.

## Acceptance Criteria

1. **Bokeh circles** — Soft, warm-toned circular particles with blurred edges drift slowly upward in the background
2. **Count** — 6-10 particles visible at any time
3. **Size variation** — Each particle is 8-16px with randomized size on spawn
4. **Opacity** — Low opacity range (0.1-0.3 alpha) — barely visible, atmospheric only
5. **Movement** — Slow upward drift (40-60px/s randomized) with slight horizontal sway (sine wave, ±10px amplitude)
6. **Color** — Warm amber/gold tint matching the sunset palette `(1f, 0.9f, 0.6f)`
7. **Lifecycle** — Particles spawn below the visible screen area, drift upward, and fade out near the top
8. **Sorting layer** — Rendered behind containers but in front of the background image
9. **Always active** — Particles run continuously during gameplay and on menu screens (not just during puzzles)
10. **Performance** — Lightweight implementation using a small object pool, not Unity Particle System

## Tasks / Subtasks

- [x] Task 1: Create FloatingLights component (AC: 1, 2, 3, 4, 5, 6, 7)
  - [x] 1.1 Create `Scripts/Game/Effects/FloatingLights.cs` MonoBehaviour
  - [x] 1.2 Create or source a soft circular bokeh sprite (32x32px, white with feathered edges)
  - [x] 1.3 Implement `SpawnParticle()` — randomize size (8-16px), alpha (0.1-0.3), speed (40-60px/s), horizontal offset
  - [x] 1.4 Implement `UpdateParticles()` in `Update()` — move each particle upward + sine sway horizontally
  - [x] 1.5 When particle exits top of screen, recycle it to bottom with new random properties
  - [x] 1.6 Tint all particles warm amber/gold

- [x] Task 2: Implement particle pool (AC: 2, 10)
  - [x] 2.1 Pre-instantiate 10 SpriteRenderer GameObjects as children
  - [x] 2.2 Initialize all particles at random Y positions across the screen (staggered so they don't all start at bottom)
  - [x] 2.3 Pool is fixed size — no dynamic allocation during gameplay

- [x] Task 3: Set up sorting layer and placement (AC: 8, 9)
  - [x] 3.1 Place FloatingLights on a dedicated sorting layer between Background and Containers
  - [x] 3.2 Attach FloatingLights to a persistent GameObject that survives scene/screen transitions
  - [x] 3.3 Alternatively, attach to the main Camera or a root-level ambient effects parent

## Dev Notes

### Key Files to Modify
- **New file:** `FloatingLights.cs` — particle system component
- Scene setup — needs to be placed in the scene hierarchy on a persistent GameObject

### Key Architecture
- This is a standalone ambient effect — no dependency on gameplay systems
- Should persist across all screens (main menu, roadmap, gameplay)
- Uses `Update()` loop for smooth movement (not coroutines) since all particles move every frame
- Sine sway: `x += Mathf.Sin(Time.time * swaySpeed + phaseOffset) * swayAmplitude * Time.deltaTime`

### Constraints
- No Unity Particle System — use simple SpriteRenderer pool with manual movement
- No external tween libraries
- Must be extremely subtle — players should barely notice these consciously
- Performance: 10 sprites with position updates in `Update()` is negligible overhead
