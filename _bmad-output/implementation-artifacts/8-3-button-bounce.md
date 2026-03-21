# Story 8.3: Button Bounce Feedback

Status: done

## Story

As a player,
I want buttons to feel tactile with a satisfying bounce on tap,
so that every interaction feels responsive and premium.

## Acceptance Criteria

1. **Press scale** — On pointer down, the button scales to 92% (EaseInCubic, 0.06s)
2. **Release spring** — On pointer up, the button springs to 105% then settles to 100% (EaseOutBack, 0.15s)
3. **Reusable component** — The bounce effect is a reusable `ButtonBounce` MonoBehaviour that can be attached to any button GameObject
4. **All buttons** — Applied to all interactive buttons: HUD buttons (undo, restart, extra bottle, back), menu buttons, level complete buttons, settings buttons
5. **No interference** — The bounce animation does not interfere with button click events or other animations
6. **Cancellation** — If the pointer exits the button before release, the button smoothly returns to 100% scale without the spring overshoot

## Tasks / Subtasks

- [x] Task 1: Create ButtonBounce component (AC: 1, 2, 3, 5, 6)
  - [x] 1.1 Create `Scripts/Game/UI/Components/ButtonBounce.cs` MonoBehaviour
  - [x] 1.2 Implement `IPointerDownHandler` — start press animation (scale to 0.92, EaseInCubic, 0.06s)
  - [x] 1.3 Implement `IPointerUpHandler` — start release animation (scale to 1.05 then 1.0, EaseOutBack, 0.15s)
  - [x] 1.4 Implement `IPointerExitHandler` — cancel press, smooth return to 1.0 (EaseOutCubic, 0.1s)
  - [x] 1.5 Use coroutines with `transform.localScale` — stop previous coroutine before starting new one
  - [x] 1.6 Add EaseInCubic helper: `t*t*t`

- [x] Task 2: Attach ButtonBounce to HUD buttons (AC: 4)
  - [x] 2.1 In `GameplayHUD.cs`, add `ButtonBounce` component to each button created by `CreateHUDButton()`

- [x] Task 3: Attach ButtonBounce to menu and screen buttons (AC: 4)
  - [x] 3.1 In `MainMenuScreen.cs`, add `ButtonBounce` to menu buttons
  - [x] 3.2 In `LevelCompleteScreen.cs`, add `ButtonBounce` to action buttons
  - [x] 3.3 In `StarGateScreen.cs`, add `ButtonBounce` to buttons (if any)
  - [x] 3.4 In `SettingsScreen.cs` (or `4-7-settings-screen`), add `ButtonBounce` to settings buttons

## Dev Notes

### Key Files to Modify
- **New file:** `ButtonBounce.cs` — reusable component
- `GameplayHUD.cs` — attach to HUD buttons via `AddComponent<ButtonBounce>()`
- `MainMenuScreen.cs` — attach to menu buttons
- `LevelCompleteScreen.cs` — attach to action buttons
- Other screen classes as needed

### Key Architecture
- Buttons are created programmatically via helper methods like `CreateHUDButton()` and `CreateActionButton()`
- Buttons use `UnityEngine.UI.Button` with `onClick.AddListener()` — ButtonBounce must not conflict with this
- `IPointerDownHandler` / `IPointerUpHandler` are compatible with `Button` component on same GameObject

### Constraints
- No external tween libraries — pure coroutines with `transform.localScale` Lerp
- EaseOutBack already exists in `BottleContainerView.cs` — consider extracting to a shared `Easing` utility or duplicate in ButtonBounce
- Must handle rapid taps (stop previous coroutine before starting new one)
