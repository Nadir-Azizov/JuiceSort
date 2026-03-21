# Story 7.1: Extra Bottle & Rewarded Ad Framework

Status: done

## Story

As a player,
I want to tap "+" to watch an ad and receive an extra empty bottle (max 2 per level),
so that I have a safety net when I'm stuck without being forced to watch ads.

## Acceptance Criteria

1. **Extra bottle button** — "+" button visible during gameplay (placeholder, on HUD)
2. **Adds empty container** — Tapping "+" adds an empty container to the puzzle
3. **Max 2 per level** — Maximum 2 extra bottles per level, button disabled/hidden after limit
4. **Resets on restart/next level** — Extra bottle count resets when level restarts or advances
5. **Ad framework** — IAdManager interface + placeholder AdManager (logs instead of showing real ads)
6. **Rewarded flow** — Tap "+" → (placeholder ad) → receive bottle → refresh board
7. **No forced ads** — Ads only appear when player chooses to tap the button
8. **Board updates** — PuzzleBoardView and puzzle state update to show the new container

## Tasks / Subtasks

- [ ] Task 1: Create IAdManager and placeholder AdManager (AC: 5, 7)
  - [ ] 1.1 Create `Scripts/Game/Ads/IAdManager.cs` — interface: ShowRewardedAd(Action onRewarded, Action onFailed)
  - [ ] 1.2 Create `Scripts/Game/Ads/AdManager.cs` — placeholder that immediately calls onRewarded (simulates ad completion)
  - [ ] 1.3 Register in BootLoader

- [ ] Task 2: Implement extra bottle logic in PuzzleEngine (AC: 2)
  - [ ] 2.1 Add `AddExtraContainer(PuzzleState state, int slotCount)` to PuzzleEngine — creates new empty ContainerData, returns new PuzzleState with expanded container array
  - [ ] 2.2 Pure C# — returns a new PuzzleState (immutable pattern)

- [ ] Task 3: Add dynamic container support to PuzzleBoardView (AC: 8)
  - [ ] 3.1 Add `AddContainerView(ContainerData data, int index)` to PuzzleBoardView — dynamically creates and appends a new ContainerView to existing layout
  - [ ] 3.2 HorizontalLayoutGroup handles repositioning automatically — no full rebuild needed
  - [ ] 3.3 Subscribe new ContainerView.OnTapped to HandleContainerTapped
  - [ ] 3.4 Update _containerViews array (resize)

- [ ] Task 4: Implement extra bottle flow in GameplayManager (AC: 1, 3, 4, 6, 8)
  - [ ] 4.1 Add `_extraBottlesUsed` field (int), reset in LoadLevel
  - [ ] 4.2 Add `RequestExtraBottle()` method: checks limit, calls IAdManager.ShowRewardedAd, on reward calls PuzzleEngine.AddExtraContainer
  - [ ] 4.3 After adding container: update _currentPuzzle, call _boardView.AddContainerView for the new container (no full board rebuild)
  - [ ] 4.4 Clear undo stack on extra bottle — accepting help means committing to current state
  - [ ] 4.5 Expose `ExtraBottlesRemaining` property (MaxExtraBottles - _extraBottlesUsed)

- [ ] Task 4: Add extra bottle button to GameplayHUD (AC: 1, 3)
  - [ ] 4.1 Add "+" button to GameplayHUD
  - [ ] 4.2 Show remaining count: "+ (2)" → "+ (1)" → hidden/disabled at 0
  - [ ] 4.3 Button calls GameplayManager.RequestExtraBottle()

- [ ] Task 5: Write tests (AC: all)
  - [ ] 5.1 Create `Scripts/Tests/EditMode/ExtraBottleTests.cs`
  - [ ] 5.2 Test AddExtraContainer increases container count
  - [ ] 5.3 Test extra bottle limit (max 2)
  - [ ] 5.4 Test extra bottle count resets on new level
  - [ ] 5.5 Test new container is empty

## Dev Notes

### AddExtraContainer returns NEW PuzzleState

Since container count changes, we need a new PuzzleState with expanded array:
```csharp
public static PuzzleState AddExtraContainer(PuzzleState state, int slotCount)
{
    var newContainers = new ContainerData[state.ContainerCount + 1];
    for (int i = 0; i < state.ContainerCount; i++)
        newContainers[i] = state.GetContainer(i);
    newContainers[state.ContainerCount] = new ContainerData(slotCount);
    return new PuzzleState(newContainers);
}
```

### Dynamic container addition (no board rebuild)

Instead of destroying/recreating the board, PuzzleBoardView.AddContainerView dynamically adds a new ContainerView to the existing HorizontalLayoutGroup. The layout manager handles repositioning. Much smoother than full rebuild.

### Undo stack cleared on extra bottle

Adding an extra bottle is a "help" action — player commits to current state. Undo stack is cleared because old snapshots have fewer containers and can't be restored to a board with more containers. This is simpler than tracking container count changes in undo.

### AdMob integration (Unity Editor)

Real Google AdMob SDK integration requires:
1. Install AdMob Unity plugin via Unity Package Manager
2. Configure AdMob app ID in Assets/Plugins/Android
3. Replace placeholder AdManager with real AdMob rewarded ad calls
This is a Unity Editor task, not CLI.

### References

- [Source: _bmad-output/gdd.md#Game Mechanics] — "Extra Bottle: Watch ad for additional empty container, max 2"
- [Source: _bmad-output/game-architecture.md#Error Handling] — "AdMob ad requests → silently fail, hide extra bottle button"
- [Source: _bmad-output/project-context.md#Google Play Compliance] — "AdMob rewarded video only — no forced ads"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- IAdManager + placeholder AdManager: ShowRewardedAd immediately calls onRewarded (simulates ad)
- PuzzleEngine.AddExtraContainer: returns new PuzzleState with expanded array, original unchanged
- PuzzleBoardView.AddContainerView: dynamically adds container to existing layout (no full rebuild)
- GameplayManager.RequestExtraBottle: checks limit, calls ad, adds container, clears undo stack
- GameplayHUD: "+" button with remaining count, disabled at 0
- _extraBottlesUsed resets in LoadLevel
- 6 tests: container count increase, empty new container, existing preserved, limit enforcement, reset, immutability

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Ads/IAdManager.cs`
- `src/JuiceSort/Assets/Scripts/Game/Ads/AdManager.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ExtraBottleTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleEngine.cs` — added AddExtraContainer
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/PuzzleBoardView.cs` — added AddContainerView, _containerAreaTransform
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — RequestExtraBottle, _extraBottlesUsed, ExtraBottlesRemaining
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/GameplayHUD.cs` — "+" button, UpdateDisplay with extra bottles
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — registers AdManager
