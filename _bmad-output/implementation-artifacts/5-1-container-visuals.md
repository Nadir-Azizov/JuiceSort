# Story 5.1: Container Visuals & ThemeConfig

Status: done

## Story

As a player,
I want to see beautiful containers with drink-themed colors,
so that the puzzle feels premium and inviting.

## Acceptance Criteria

1. **ThemeConfig with mood palettes** — Centralized color configuration including morning/night palettes from day one
2. **Improved drink palette** — Richer, warmer Tropical Fresh tones replacing flat placeholder colors
3. **Container styling** — Container body has warm glass tint instead of plain gray
4. **Empty slot distinct** — Empty slots use subtle warm tint, not cold gray
5. **Selection highlight** — Selected container has warm golden glow
6. **Mood-aware colors** — ThemeConfig provides GetColor(LevelMood, ColorType) for mood-dependent styling

## Tasks / Subtasks

- [ ] Task 1: Create ThemeConfig with mood palettes (AC: 1, 6)
  - [ ] 1.1 Create `Scripts/Game/UI/ThemeConfig.cs` — static class with all visual constants
  - [ ] 1.2 Drink colors: Tropical Fresh palette (warm golden Mango, luxurious Berry, ocean Teal, soft Rose, sunlit Lime)
  - [ ] 1.3 Container colors: idle morning (warm glass), idle night (cool glass), selected (golden glow)
  - [ ] 1.4 UI colors: background morning/night, button primary/secondary, text, star gold
  - [ ] 1.5 Morning palette: warm golden backgrounds, bright text, sunlit accents
  - [ ] 1.6 Night palette: deep blue backgrounds, soft white text, warm amber accents
  - [ ] 1.7 Enum `ThemeColorType`: Background, ContainerIdle, ContainerSelected, ButtonPrimary, ButtonSecondary, TextPrimary, TextSecondary, StarGold, EmptySlot
  - [ ] 1.8 Method: `GetColor(LevelMood mood, ThemeColorType type)` for mood-aware retrieval
  - [ ] 1.9 Method: `GetDrinkColor(DrinkColor)` replacing DrinkColorMap

- [ ] Task 2: Update DrinkColorMap (AC: 2)
  - [ ] 2.1 DrinkColorMap.GetColor now delegates to ThemeConfig.GetDrinkColor
  - [ ] 2.2 Richer warm tones per visual direction

- [ ] Task 3: Update ContainerView styling (AC: 3, 4, 5)
  - [ ] 3.1 ContainerView.IdleColor and SelectedColor read from ThemeConfig
  - [ ] 3.2 SlotView empty color from ThemeConfig.GetColor(EmptySlot)

- [ ] Task 4: Write tests (AC: all)
  - [ ] 4.1 Create `Scripts/Tests/EditMode/ThemeConfigTests.cs`
  - [ ] 4.2 Test GetColor returns different values for morning vs night
  - [ ] 4.3 Test all DrinkColor enum values produce non-transparent colors
  - [ ] 4.4 Test ThemeColorType has all required values

## Dev Notes

### ThemeConfig is the single source of truth for all visual constants

Replaces hardcoded colors in: ContainerView, DrinkColorMap, SlotView, all screen classes. Everything references ThemeConfig.

### Mood palettes included from the start

No need for a separate story to add mood palettes — they're part of the same color system. Story 5-4 will WIRE mood into containers/HUD/backgrounds.

### Color direction from visual-direction-tropical-fresh.md

- "Saturated but warm — fruit through golden light"
- "Realistic glass with subtle transparency"
- "Semi-transparent with warm tinting"

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- ThemeConfig: static class with GetColor(mood, type), GetDrinkColor, GetBackgroundGradientTop/Bottom, CurrentMood property
- ThemeColorType enum: 10 color types covering all UI elements
- Morning palette: warm golden tones. Night palette: cool blue with warm accents.
- DrinkColorMap now delegates to ThemeConfig.GetDrinkColor — richer Tropical Fresh tones
- ContainerView: removed static IdleColor/SelectedColor, uses ThemeConfig.GetColor dynamically
- SlotView: empty slot uses ThemeConfig EmptySlot color
- 9 tests: mood comparison, drink colors, alpha validation, enum completeness, gradient differences

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/ThemeConfig.cs`
- `src/JuiceSort/Assets/Scripts/Game/UI/ThemeColorType.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/ThemeConfigTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/DrinkColorMap.cs` — delegates to ThemeConfig
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/ContainerView.cs` — uses ThemeConfig for idle/selected/empty colors
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/SlotView.cs` — uses ThemeConfig for empty slot color
