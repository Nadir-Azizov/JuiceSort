# Story 11.7: Loading Screen

Status: done

## Story

As a player,
I want to see a branded loading/splash screen when the app launches,
so that the first impression of the game feels polished and professional.

## Priority

MEDIUM — Visual polish. Game is playable without it, but first impressions matter for retention.

## Acceptance Criteria

1. **Loading scene** — A dedicated loading screen shown on app launch before the Hub screen.
2. **PNG background** — Full-screen display of `loading_background.png` from Resources/Backgrounds/.
3. **Minimum display time** — Loading screen displays for at least 2 seconds before transitioning.
4. **Transition to Hub** — Smoothly crossfade to Hub screen via ScreenManager once boot completes.
5. **Performance** — Loading screen renders immediately on first frame (PNG from Resources, no async loads).
6. **Fallback** — If PNG not found, graceful fallback to warm sunset gradient.

## Tasks / Subtasks

- [x] Task 1: Create loading screen infrastructure (AC: 1, 2, 3, 4, 5, 6)
  - [x] 1.1 Add `Loading` state to `GameFlowState` enum
  - [x] 1.2 Create `LoadingScreen.cs` in `Scripts/Game/UI/Screens/` with PNG background
  - [x] 1.3 Loading screen shows for minimum 2 seconds via `IsReady` flag
  - [x] 1.4 Fallback: Sprite → Texture2D → runtime warm gradient if PNG missing
  - [x] 1.5 Wire into `BootLoader.cs`: create LoadingScreen, register, show first
  - [x] 1.6 Coroutine in BootLoader waits for `IsReady` then transitions to MainMenu

## Dev Notes

### Implementation Approach (Revised)
- **User override**: PNG-based approach instead of original programmatic spec
- Single full-screen PNG image (`Resources/Backgrounds/loading_background.png` — already exists)
- Same image set as Unity splash background — provides seamless visual continuity
- Minimum 2-second display enforced by `IsReady` property on `LoadingScreen` component
- BootLoader shows Loading state first, coroutine waits for readiness, then crossfades to Hub
- Follows same Canvas/CanvasScaler/Create() factory pattern as all other screens

### Key Files to CREATE
- `Scripts/Game/UI/Screens/LoadingScreen.cs` — PNG-based loading screen

### Key Files to MODIFY
- `Scripts/Game/UI/GameFlowState.cs` — Add `Loading` state
- `Scripts/Game/Boot/BootLoader.cs` — Wire loading screen into boot flow

### Architecture Notes
- Loading screen is Canvas ScreenSpaceOverlay, sortingOrder 100 (above everything during boot)
- Same CanvasScaler setup (1080×1920, matchWidthOrHeight 0.5)
- After 2s minimum: ScreenManager transitions to Hub with standard crossfade
- Boot flow: Unity splash → LoadingScreen (2s) → crossfade → HubScreen

### References
- [Source: _bmad-output/epics.md#Epic-11] — Loading screen scope

## Dev Agent Record

### Agent Model Used
Claude Opus 4.6 (1M context)

### Debug Log References

### Completion Notes List
- PNG-based loading screen implemented (user override — simplified from original programmatic spec)
- Full-screen `loading_background.png` displayed via Resources.Load with 3-tier fallback (Sprite → Texture2D → runtime gradient)
- Minimum 2-second display enforced via `IsReady` property using `Time.unscaledDeltaTime`
- BootLoader shows Loading state on Start(), coroutine polls IsReady then crossfades to Hub
- Canvas sortingOrder 100 ensures loading screen renders above all other screens during boot
- Updated existing ScreenManagerTests to account for new Loading enum value (6 → 7 values)

### Change Log
- 2026-03-27: Story implemented with PNG-based approach per user direction (replaces original programmatic cupboard/bottles spec)
- 2026-03-27: Deleted loading-screen-mockup.html — obsolete, replaced by PNG-based implementation
- 2026-03-27: Code review — removed unnecessary GraphicRaycaster (no interactive elements on loading screen)
- 2026-03-27: Restored 2s minimum delay — instant transition looked jarring
- 2026-03-28: Post-completion — Background switched from Image+Sprite to RawImage+Texture2D, added AspectFillScaler for aspect-fill (cover) mode instead of stretching. Prevents distortion on non-16:9 devices.
- 2026-03-28: Post-completion — Created LoadingScene.unity (Build Index 0) with LoadingSceneManager that async-loads Boot scene. BootLoader detects via static flag whether splash already shown. Unity built-in splash disabled.
- 2026-03-28: Post-completion — loading_background.png replaced with taller 9:20 ratio image (1080x2400) with expendable top/bottom bleed zones for safe cropping on shorter screens.

### File List
- `Assets/Scripts/Game/UI/Screens/LoadingScreen.cs` (NEW)
- `Assets/Scripts/Game/UI/GameFlowState.cs` (MODIFIED — added Loading state)
- `Assets/Scripts/Game/Boot/BootLoader.cs` (MODIFIED — loading screen creation, boot flow, coroutine)
- `Assets/Scripts/Tests/EditMode/ScreenManagerTests.cs` (MODIFIED — updated enum count 6→7, added Loading assert)
- `Assets/Resources/Backgrounds/loading_background.png` (EXISTING — used as loading screen background)
- `ProjectSettings/ProjectSettings.asset` (MODIFIED — Unity splash: custom background image, Unity logo removed)
