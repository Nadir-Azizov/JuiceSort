# Story 4.1: Main Menu & Screen Navigation

Status: done

## Story

As a player,
I want to see a main menu when I open the app,
so that I have a clear starting point for the game.

## Acceptance Criteria

1. **Main menu visible on start** — After boot, player sees a main menu instead of jumping straight into gameplay
2. **Game title** — "JuiceSort" title displayed prominently
3. **Play button** — Tapping "Play" transitions to Roadmap screen
4. **Settings button** — Button to access settings (Story 4.7)
5. **ScreenManager** — Coordinates which screen is visible, manages transitions
6. **GameFlowState enum** — Tracks current state: MainMenu, Roadmap, Playing, LevelComplete, Settings, Gate
7. **GameplayManager lifecycle** — GameplayManager gets a public `StartLevel(int)` method; `Start()` auto-load removed

## Tasks / Subtasks

- [ ] Task 1: Create GameFlowState enum (AC: 6)
  - [ ] 1.1 Create `Scripts/Game/UI/GameFlowState.cs` — enum: MainMenu, Roadmap, Playing, LevelComplete, Settings, Gate

- [ ] Task 2: Create ScreenManager (AC: 5)
  - [ ] 2.1 Create `Scripts/Game/UI/ScreenManager.cs` — MonoBehaviour managing screen transitions
  - [ ] 2.2 Dictionary<GameFlowState, GameObject> mapping states to screen root GameObjects
  - [ ] 2.3 RegisterScreen(GameFlowState, GameObject) — registers a screen
  - [ ] 2.4 TransitionTo(GameFlowState) — hides current screen, shows new one, updates CurrentState
  - [ ] 2.5 CurrentState property
  - [ ] 2.6 Created by BootLoader, registered via Services

- [ ] Task 3: Create MainMenuScreen (AC: 1, 2, 3, 4)
  - [ ] 3.1 Create `Scripts/Game/UI/Screens/MainMenuScreen.cs` — creates Canvas with title and buttons
  - [ ] 3.2 "JuiceSort" title text (large, centered top)
  - [ ] 3.3 "Play" button → ScreenManager.TransitionTo(Roadmap)
  - [ ] 3.4 "Settings" button → ScreenManager.TransitionTo(Settings)

- [ ] Task 4: Refactor GameplayManager lifecycle (AC: 7)
  - [ ] 4.1 Remove `Start() { LoadLevel(_currentLevelNumber); }` auto-load
  - [ ] 4.2 Add public `StartLevel(int levelNumber)` method — does what LoadLevel does but callable externally
  - [ ] 4.3 Add public `StartReplay(int levelNumber)` method — wrapper for LoadSpecificLevel
  - [ ] 4.4 GameplayManager is now passive until ScreenManager tells it to start a level

- [ ] Task 5: Update BootLoader (AC: 1)
  - [ ] 5.1 BootLoader creates ScreenManager, registers as service
  - [ ] 5.2 BootLoader creates MainMenuScreen, registers with ScreenManager
  - [ ] 5.3 BootLoader creates GameplayManager (passive, no auto-load)
  - [ ] 5.4 Initial state: ScreenManager.TransitionTo(MainMenu)
  - [ ] 5.5 Remove direct "Gameplay" scene loading

- [ ] Task 6: Write tests (AC: all)
  - [ ] 6.1 Test GameFlowState enum has all required values

## Dev Notes

### New boot flow
```
BootLoader.Awake:
  Services.Clear()
  Create SaveManager → register
  Create ProgressionManager → register
  Create ScreenManager → register
  Create MainMenuScreen → register with ScreenManager
  Create GameplayManager (passive)

BootLoader.Start:
  ScreenManager.TransitionTo(MainMenu)

Player taps Play:
  ScreenManager.TransitionTo(Roadmap)

Player taps level on Roadmap:
  GameplayManager.StartLevel(levelNumber)
  ScreenManager.TransitionTo(Playing)
```

### GameplayManager lifecycle change (CRITICAL)

Currently GameplayManager auto-loads in Start(). This must change:
- Remove Start() auto-load
- Add StartLevel(int) — called by roadmap/screen transitions
- GameplayManager created as DontDestroyOnLoad service, not scene-bound

### ScreenManager design

Screens are Canvas GameObjects that get SetActive(true/false). ScreenManager tracks them by GameFlowState key. Only one screen active at a time (except overlays like LevelComplete which overlay on Playing).

### References

- [Source: _bmad-output/game-architecture.md#Scene Structure] — Boot → MainMenu → Roadmap → Playing
- [Source: _bmad-output/game-architecture.md#State Patterns] — GameFlowState enum

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- GameFlowState enum: MainMenu, Roadmap, Playing, LevelComplete, Settings, Gate
- ScreenManager: RegisterScreen/TransitionTo/ShowOverlay/HideOverlay with Dictionary<GameFlowState, GameObject>
- MainMenuScreen: static Create() factory, Canvas with title + Play/Settings buttons
- GameplayManager refactored: removed Start() auto-load, added StartLevel(int)/StartReplay(int) public entry points, replaced LoadSpecificLevel
- BootLoader creates all services: SaveManager → ProgressionManager → ScreenManager → GameplayManager → MainMenuScreen
- Initial state: ScreenManager.TransitionTo(MainMenu) in BootLoader.Start()

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/GameFlowState.cs`
- `src/JuiceSort/Assets/Scripts/Game/UI/ScreenManager.cs`
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/MainMenuScreen.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ScreenManagerTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — removed Start() auto-load, added StartLevel/StartReplay, removed LoadSpecificLevel
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates ScreenManager, GameplayManager, MainMenuScreen as services
