# Story 4.7: Settings Screen

Status: done

## Story

As a player,
I want to access settings from the main menu,
so that I can toggle sound and music preferences.

## Acceptance Criteria

1. **Settings accessible** — Settings button on main menu opens settings screen
2. **Sound toggle** — Toggle to enable/disable sound effects
3. **Music toggle** — Toggle to enable/disable music
4. **Settings persist** — Sound/music preferences saved via SaveManager
5. **Back button** — Returns to main menu
6. **Settings stored in ProgressionManager** — Sound/music bools accessible at runtime

## Tasks / Subtasks

- [x] Task 1: Add settings to ProgressionData (AC: 6)
  - [x]1.1 Add `SoundEnabled` (bool, default true) and `MusicEnabled` (bool, default true) to ProgressionData
  - [x]1.2 Add matching properties to IProgressionManager and ProgressionManager
  - [x]1.3 SaveData.FromProgressionData/ToProgressionData includes sound/music settings

- [x] Task 2: Create SettingsScreen (AC: 1, 2, 3, 5)
  - [x]2.1 Create `Scripts/Game/UI/Screens/SettingsScreen.cs`
  - [x]2.2 Sound toggle button: shows "Sound: ON" / "Sound: OFF", toggles on tap
  - [x]2.3 Music toggle button: shows "Music: ON" / "Music: OFF", toggles on tap
  - [x]2.4 "Back" button → ScreenManager.TransitionTo(MainMenu)

- [x] Task 3: Persist on toggle (AC: 4)
  - [x]3.1 On toggle change → update ProgressionManager settings → auto-save

- [x] Task 4: Register in BootLoader
  - [x]4.1 Create SettingsScreen, register with ScreenManager

- [x] Task 5: Write tests (AC: all)
  - [x]5.1 Test settings default to true
  - [x]5.2 Test toggle changes value
  - [x]5.3 Test settings survive save/load round-trip

## Dev Notes

Sound/music toggles are UI-only for now — actual audio is Epic 6. These store preferences that Epic 6's AudioManager will read.

Settings go into ProgressionData (simple) rather than a separate SettingsManager (over-engineered for 2 bools). SaveData already has soundEnabled/musicEnabled fields.

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- SettingsScreen: Sound/Music toggle buttons, reads/writes to ProgressionData, Refresh() on show
- ProgressionData: added SoundEnabled/MusicEnabled bool properties (default true)
- SaveData: includes sound/music in FromProgressionData/ToProgressionData conversions
- BootLoader registers SettingsScreen, refreshes on transition

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/SettingsScreen.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Progression/ProgressionData.cs` — added SoundEnabled, MusicEnabled
- `src/JuiceSort/Assets/Scripts/Game/Save/SaveData.cs` — includes settings in conversion
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — registers SettingsScreen
