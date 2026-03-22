# Story 5.4: Morning/Night Atmosphere & City Display

Status: done

## Story

As a player,
I want morning levels to feel like a bright café and night levels like a cozy bar, with the city name visible,
so that the mood system adds variety and I know which destination I'm visiting.

## Acceptance Criteria

1. **Morning palette applied** — Morning levels use warm, bright, golden tones throughout
2. **Night palette applied** — Night levels use cool, deeper tones with warm accents
3. **Container tint adapts** — Container body color subtly shifts with mood via ThemeConfig
4. **HUD adapts** — HUD text/button colors use mood-appropriate ThemeConfig colors
5. **Background adapts** — BackgroundManager gradient uses mood palette from ThemeConfig
6. **City name visible** — City name, country, and mood indicator shown on GameplayHUD
7. **Automatic per level** — All mood colors apply automatically from LevelDefinition.Mood

## Tasks / Subtasks

- [x] Task 1: Wire mood into ContainerView (AC: 1, 2, 3)
  - [x]1.1 ContainerView receives current LevelMood on Initialize (or via ThemeConfig static state)
  - [x]1.2 Idle color = ThemeConfig.GetColor(mood, ContainerIdle)
  - [x]1.3 Selected color stays golden (consistent selection feedback across moods)

- [x] Task 2: Wire mood into GameplayHUD (AC: 4, 6)
  - [x]2.1 GameplayHUD.SetLevelInfo shows: "Level N — CityName, Country ☀/☾"
  - [x]2.2 HUD text colors from ThemeConfig.GetColor(mood, TextPrimary/Secondary)
  - [x]2.3 Button colors from ThemeConfig.GetColor(mood, ButtonPrimary/Secondary)

- [x] Task 3: Wire mood into BackgroundManager (AC: 5)
  - [x]3.1 BackgroundManager.SetBackground uses ThemeConfig.GetColor(mood, Background) as base
  - [x]3.2 Morning gradient: golden top → soft peach bottom
  - [x]3.3 Night gradient: deep blue top → warm purple bottom

- [x] Task 4: Wire mood in GameplayManager.LoadLevel (AC: 7)
  - [x]4.1 Pass mood to ContainerView, GameplayHUD, BackgroundManager after level loads
  - [x]4.2 Set ThemeConfig.CurrentMood static property so all components can access it

- [x] Task 5: Write tests (AC: all)
  - [x]5.1 Test ThemeConfig.GetColor returns different colors for Morning vs Night for each type
  - [x]5.2 Test morning and night colors are valid (non-zero alpha)

## Dev Notes

### ThemeConfig.CurrentMood as static state

Simple approach: ThemeConfig has a `public static LevelMood CurrentMood` property. Components read from it. Set once in LoadLevel. Avoids passing mood through every constructor.

### "Café vs bar" through color alone

Morning = warm golden tones (relaxed breakfast vibe). Night = cool blue tones with warm amber accents (sophisticated lounge). No different assets — just palette shifts.

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- GameplayHUD.SetLevelInfo now shows "Level N — City, Country ☀/☾" with ThemeConfig text colors
- ThemeConfig.CurrentMood already set in LoadLevel (Story 5.2)
- ContainerView already reads ThemeConfig dynamically (Story 5.1) — mood affects container idle color
- BackgroundManager already uses mood for gradient (Story 5.2)
- Mood atmosphere is primarily delivered through ThemeConfig.GetColor returning different palettes for Morning/Night

### File List

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Components/GameplayHUD.cs` — SetLevelInfo shows city/country/mood, uses ThemeConfig text colors
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — passes country and mood to SetLevelInfo
