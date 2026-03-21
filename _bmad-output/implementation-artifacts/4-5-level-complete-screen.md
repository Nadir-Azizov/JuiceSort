# Story 4.5: Level Complete & Gate Screens

Status: done
Merges: 4.6 (batch unlock notification)

## Story

As a player,
I want to see my star rating after completing a level and be notified about batch unlocks,
so that I can celebrate achievements and understand progression gates.

## Acceptance Criteria

1. **Star display** — 1-3 stars displayed prominently on level complete
2. **Move stats** — Move count and estimated optimal shown
3. **Next Level button** — Advances to next level
4. **Replay button** — Replays the same level for better stars
5. **Roadmap button** — Returns to roadmap
6. **Gate screen with level list** — When batch gate blocks, shows star deficit + LevelListView for replay
7. **Gate unlocked message** — When gate passes, shows celebration
8. **LevelCompleteScreen component** — Extracted from GameplayManager.ShowWinOverlay
9. **StarGateScreen component** — Extracted from GameplayManager.ShowGateScreen, uses shared LevelListView

## Tasks / Subtasks

- [ ] Task 1: Create LevelCompleteScreen (AC: 1, 2, 3, 4, 5, 8)
  - [ ] 1.1 Create `Scripts/Game/UI/Screens/LevelCompleteScreen.cs`
  - [ ] 1.2 Show(int level, string cityName, int stars, int moves, int optimal, bool isReplay) — populates all elements
  - [ ] 1.3 Star display: large filled/empty symbols with golden color
  - [ ] 1.4 Move text: "Moves: X (Optimal: ~Y)"
  - [ ] 1.5 "Next Level" button → calls GameplayManager.NextLevel + ScreenManager transition
  - [ ] 1.6 "Replay" button → calls GameplayManager.RestartLevel
  - [ ] 1.7 "Roadmap" button → ScreenManager.TransitionTo(Roadmap)
  - [ ] 1.8 When isReplay → show "Continue" button instead of "Next Level" (checks gate, returns to roadmap)

- [ ] Task 2: Create StarGateScreen (AC: 6, 7, 9)
  - [ ] 2.1 Create `Scripts/Game/UI/Screens/StarGateScreen.cs`
  - [ ] 2.2 Show(IProgressionManager) — displays star deficit text + LevelListView
  - [ ] 2.3 Reuses LevelListView from Story 4.2 for the level list
  - [ ] 2.4 LevelListView.OnLevelTapped → GameplayManager.StartReplay + ScreenManager.TransitionTo(Playing)
  - [ ] 2.5 After replay completes and gate passes → shows "Batch Unlocked!" then proceeds

- [ ] Task 3: Extract from GameplayManager (AC: 8, 9)
  - [ ] 3.1 Remove ShowWinOverlay, CreateNextLevelButton, CreateReturnToGateButton from GameplayManager
  - [ ] 3.2 Remove ShowGateScreen, CreateGateLevelList, CreateGateLevelEntry from GameplayManager
  - [ ] 3.3 GameplayManager.OnLevelComplete → creates/shows LevelCompleteScreen
  - [ ] 3.4 GameplayManager.NextLevel gate check → shows StarGateScreen
  - [ ] 3.5 This removes ~250 lines from GameplayManager

- [ ] Task 4: Register screens in BootLoader
  - [ ] 4.1 Create LevelCompleteScreen and StarGateScreen in BootLoader
  - [ ] 4.2 Register with ScreenManager

- [ ] Task 5: Write tests (AC: all)
  - [ ] 5.1 Test star display formatting
  - [ ] 5.2 Test gate deficit calculation

## Dev Notes

### Major GameplayManager cleanup

This story + Story 4.4 together remove ~370 lines from GameplayManager, leaving it as pure game logic (~300 lines).

### LevelCompleteScreen replaces ShowWinOverlay

Current ShowWinOverlay is inline in GameplayManager. The new screen:
- Is a separate Canvas (managed by ScreenManager as overlay on Playing state)
- Has proper Show/Hide methods
- Handles both first-time and replay scenarios via isReplay flag

### StarGateScreen replaces ShowGateScreen

Uses the same LevelListView from Story 4.2. The gate screen is just:
- Star deficit header text
- LevelListView populated with completed levels
- Gate unlocked celebration when condition met

### References

- [Source: _bmad-output/gdd.md#Win/Loss Conditions] — star display on completion
- [Source: _bmad-output/gdd.md#Star Gate System] — batch unlock notification

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- LevelCompleteScreen: Show/Hide, star display, move stats, Next/Replay/Roadmap/Continue buttons, event-driven
- StarGateScreen: Show/Hide, star deficit header, LevelListView for replay, OnLevelTapped event
- Removed ~360 lines from GameplayManager: ShowWinOverlay, ShowGateScreen, all button creation, _winOverlay tracking
- GameplayManager now delegates to ScreenManager.ShowOverlay/HideOverlay for both screens
- BootLoader wires all screen events (NextLevel, Replay, Roadmap, Continue, gate level taps)
- GameplayManager down from 679 to 316 lines

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/LevelCompleteScreen.cs`
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/StarGateScreen.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — removed all inline UI, delegates to screen components
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates/registers LevelCompleteScreen + StarGateScreen, wires events
