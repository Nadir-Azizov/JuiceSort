# Story 3.5: Level Replay

Status: done

## Story

As a player,
I want to replay any completed level to improve my star rating,
so that I can earn more stars to pass batch gates.

## Acceptance Criteria

1. **Replay any completed level** — Player can load and replay any previously completed level
2. **Star improvement** — Replaying updates the star rating only if the new rating is better
3. **Total stars update** — Total star count recalculates after replay improvement
4. **LoadSpecificLevel method** — GameplayManager accepts a level number to load (not just sequential)
5. **Guard against uncompleted** — Cannot replay a level that hasn't been completed yet
6. **Return context** — After completing a replay, game knows it was a replay (not first-time completion) for gate re-check

## Tasks / Subtasks

- [ ] Task 1: Add LoadSpecificLevel to GameplayManager (AC: 1, 4, 5)
  - [ ] 1.1 Add `LoadSpecificLevel(int levelNumber)` public method
  - [ ] 1.2 Guard: check ProgressionManager.IsLevelCompleted(levelNumber) — if not, log warning and return
  - [ ] 1.3 Set `_isReplay = true` flag so OnLevelComplete knows it's a replay
  - [ ] 1.4 DestroyBoard + LoadLevel(levelNumber) as normal

- [ ] Task 2: Handle replay completion (AC: 2, 3, 6)
  - [ ] 2.1 In OnLevelComplete, if `_isReplay` → call CompleteLevelWithStars (ProgressionData keeps best)
  - [ ] 2.2 After replay completion, if gate was active → re-check gate and show result
  - [ ] 2.3 After replay completion, if no gate → show "Return to current level?" or just show win overlay

- [ ] Task 3: Write tests (AC: all)
  - [ ] 3.1 Test replay keeps better star rating (1→3 upgrades)
  - [ ] 3.2 Test replay with worse rating doesn't overwrite (3→1 stays 3)
  - [ ] 3.3 Test total stars updates correctly after improvement
  - [ ] 3.4 Test LoadSpecificLevel with uncompleted level → rejected

## Dev Notes

### Replay vs First-Time flow

```
First-time: OnLevelComplete → CompleteLevelWithStars → advance CurrentLevel → show win + NextLevel
Replay:     OnLevelComplete → CompleteLevelWithStars → DON'T advance → show win + "Return" or re-check gate
```

The `_isReplay` flag distinguishes these paths. Reset to false in LoadLevel.

### Integration with Story 3.4 (Gate)

Gate screen calls `LoadSpecificLevel(levelNumber)` when player taps a level. After replay completion, re-check the gate condition.

### References

- [Source: _bmad-output/gdd.md#Replayability] — "Any completed level can be replayed at any time"
- [Source: _bmad-output/gdd.md#Star Gate System] — "Replaying is the primary way to earn stars"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- LoadSpecificLevel implemented in GameplayManager (Story 3.4) — validates IsLevelCompleted, sets _isReplay flag
- ProgressionData.SetLevelRecord already keeps best stars (TryUpgradeStars) — replay improvement handled
- _isReplay flag distinguishes first-time vs replay completion
- Replay tests covered via StarGateTests (improvement scenario) and ProgressionDataTests (star upgrade)

### File List

Implemented as part of Story 3.4 — no additional files.
