# Story 1.5: Pour Slot Validation

Status: done

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a player,
I want to only be able to pour if the target container has an empty slot,
so that containers don't overflow and the puzzle rules are consistent.

## Acceptance Criteria

1. **Full target rejected** — Pouring into a container with all slots filled is not allowed
2. **No state change on rejection** — When a pour is rejected due to full target, neither container changes
3. **Source stays selected** — When pour fails due to full target, source remains selected (consistent with Story 1.4 behavior)
4. **Nearly full target accepts** — A target with exactly 1 empty slot can still receive a pour (if color matches or target empty rule applies)
5. **Validation combines with color check** — Both slot availability AND color matching must pass for a pour to execute (CanPour covers both)

## Tasks / Subtasks

- [x] Task 1: Verify CanPour handles slot validation (AC: 1, 2, 5)
  - [x] 1.1 Verified: PuzzleEngine.CanPour line 21 checks `target.IsFull()` — confirmed
  - [x] 1.2 No code changes needed

- [x] Task 2: Write edge case tests for slot validation (AC: all)
  - [x] 2.1 NearlyFullTargetMatchingColor → true
  - [x] 2.2 NearlyFullTargetMismatchedColor → false
  - [x] 2.3 FullTargetMatchingColor → false (slot overrides color)
  - [x] 2.4 FillToCapacity: last pour succeeds, next fails
  - [x] 2.5 Failed pour preserves source selection (replicates GameplayManager logic)

## Dev Notes

### Previous Story Intelligence (Story 1.4)

**CanPour already handles this:** The `PuzzleEngine.CanPour()` method implemented in Story 1.4 already includes `target.IsFull()` as its second check. This story exists to verify that behavior with focused edge case tests and confirm no gaps.

**Current CanPour logic:**
```csharp
public static bool CanPour(PuzzleState state, int sourceIndex, int targetIndex)
{
    if (source.IsEmpty()) return false;
    if (target.IsFull()) return false;    // ← THIS IS THE SLOT VALIDATION
    if (target.IsEmpty()) return true;
    return source.GetTopColor() == target.GetTopColor();
}
```

**GameplayManager behavior:** Failed pours keep source selected (implemented in Story 1.4). This applies to slot validation failures too — no separate handling needed.

### This is primarily a test/verification story

No new production code is expected. The value is in edge case test coverage that specifically targets slot boundaries (nearly full, exactly full, fill-to-capacity sequences).

### References

- [Source: _bmad-output/gdd.md#Game Mechanics] — "Can only pour if the target container has an empty slot available"
- [Source: _bmad-output/project-context.md#Puzzle-Specific Gotchas] — "Pour validation must check: matching color OR empty target, AND available slot — both conditions"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A — verification story, no runtime execution.

### Completion Notes List

- Verified CanPour already checks target.IsFull() at line 21 — no production code changes needed
- Added 5 edge case tests to PuzzleEngineTests.cs covering slot boundary conditions
- Tests cover: nearly-full with match/mismatch, full with match, fill-to-capacity sequence, selection preservation

### File List

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/PuzzleEngineTests.cs` — added 5 slot validation edge case tests
