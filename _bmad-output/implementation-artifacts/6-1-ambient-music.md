# Story 6.1: Audio Manager, Mood Music & Settings

Status: done

## Story

As a player,
I want to hear calming music that matches the morning or night mood and toggle it on/off,
so that the atmosphere enhances my relaxation experience.

## Acceptance Criteria

1. **AudioManager service** — Centralized audio service registered in Service Locator
2. **Music playback** — Plays background music via AudioSource (looping)
3. **Mood switching** — Switches between morning and night music when level mood changes
4. **Don't restart same mood** — If already playing the correct mood's music, don't restart the track
5. **Music toggle** — Respects MusicEnabled setting, toggling off stops music immediately
6. **Sound toggle** — Respects SoundEnabled for SFX, toggling off mutes SFX immediately
7. **Settings wired** — SettingsScreen toggles call AudioManager directly (not just ProgressionManager)
8. **IAudioManager interface** — Methods for PlayMusic, StopMusic, PlaySFX, SetMusicEnabled, SetSoundEnabled

## Tasks / Subtasks

- [ ] Task 1: Create IAudioManager, AudioClipType, and AudioManager (AC: 1, 2, 3, 4, 8)
  - [ ] 1.1 Create `Scripts/Game/Audio/IAudioManager.cs` — interface in Game assembly
  - [ ] 1.2 Methods: PlayMusic(LevelMood mood), StopMusic(), PlaySFX(AudioClipType type), SetMusicEnabled(bool), SetSoundEnabled(bool), bool MusicEnabled, bool SoundEnabled
  - [ ] 1.3 Create `Scripts/Game/Audio/AudioClipType.cs` — enum: Pour, Select, Deselect, LevelComplete, StarAwarded, UITap
  - [ ] 1.4 Create `Scripts/Game/Audio/AudioManager.cs` — MonoBehaviour implementing IAudioManager
  - [ ] 1.5 Two AudioSources: _musicSource (looping), _sfxSource (one-shot)
  - [ ] 1.6 PlayMusic: if same mood already playing → skip. Otherwise switch track. Placeholder: logs instead of actual playback
  - [ ] 1.7 PlaySFX: checks _soundEnabled, plays one-shot. Placeholder: logs
  - [ ] 1.8 Track _currentMood to avoid restarting same music

- [ ] Task 2: Register in BootLoader (AC: 1)
  - [ ] 2.1 BootLoader creates AudioManager GO, registers as IAudioManager
  - [ ] 2.2 Creation order: after SaveManager + ProgressionManager, before screens

- [ ] Task 3: Wire settings toggles (AC: 5, 6, 7)
  - [ ] 3.1 AudioManager.Start reads MusicEnabled/SoundEnabled from ProgressionManager
  - [ ] 3.2 SettingsScreen.ToggleSound also calls AudioManager.SetSoundEnabled
  - [ ] 3.3 SettingsScreen.ToggleMusic also calls AudioManager.SetMusicEnabled
  - [ ] 3.4 SetMusicEnabled(false) → stops _musicSource. SetMusicEnabled(true) → resumes PlayMusic with current mood

- [ ] Task 4: Wire music to level loading (AC: 3, 4)
  - [ ] 4.1 GameplayManager.LoadLevel calls AudioManager.PlayMusic(mood) after setting ThemeConfig.CurrentMood
  - [ ] 4.2 PlayMusic checks if _currentMood matches → skips restart if same

- [ ] Task 5: Write tests (AC: all)
  - [ ] 5.1 Create `Scripts/Tests/EditMode/AudioManagerTests.cs`
  - [ ] 5.2 Test AudioClipType enum has all 6 values
  - [ ] 5.3 Test IAudioManager interface has all required method signatures

## Dev Notes

### Don't restart same mood music

```csharp
public void PlayMusic(LevelMood mood)
{
    if (!_musicEnabled) return;
    if (mood == _currentMood && _musicSource.isPlaying) return; // skip if same
    _currentMood = mood;
    // switch track...
}
```

This prevents music restarting when navigating Roadmap → Play if mood is the same.

### Placeholder audio — framework only

No actual AudioClip files can be created from CLI. PlayMusic/PlaySFX log to console. When audio assets are created externally:
1. Create AudioClip assets (.ogg for music, .wav for SFX)
2. Assign to AudioManager via [SerializeField] or Addressables
3. Replace log statements with `_musicSource.clip = clip; _musicSource.Play();`

### Audio assets needed (created externally)
- 1 morning music (.ogg, looping, café vibe)
- 1 night music (.ogg, looping, jazz lounge)
- Pour SFX (1-3 variants, .wav)
- Select/Deselect SFX (.wav)
- Level complete SFX (.wav)
- Star award SFX (.wav)
- UI tap SFX (.wav)

### References

- [Source: _bmad-output/game-architecture.md#Engine-Provided Architecture] — Unity Audio (AudioSource/AudioMixer)
- [Source: _bmad-output/gdd.md#Audio and Music] — 2 tracks, mood-matching, seamless looping

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (1M context)

### Debug Log References

N/A

### Completion Notes List

- IAudioManager: interface with PlayMusic, StopMusic, PlaySFX, Set*Enabled, *Enabled properties
- AudioClipType: 6 SFX types (Pour, Select, Deselect, LevelComplete, StarAwarded, UITap)
- AudioManager: 2 AudioSources (music looping, SFX one-shot), mood tracking, don't-restart-same-mood
- BootLoader: creates AudioManager after ProgressionManager, before ScreenManager
- SettingsScreen: toggles now call AudioManager.Set*Enabled in addition to ProgressionManager
- GameplayManager.LoadLevel: calls PlayMusic(mood) after setting ThemeConfig.CurrentMood
- Placeholder: all audio methods log to console until actual AudioClips are assigned
- 2 tests: AudioClipType enum validation

### File List

**New files:**
- `src/JuiceSort/Assets/Scripts/Game/Audio/IAudioManager.cs`
- `src/JuiceSort/Assets/Scripts/Game/Audio/AudioClipType.cs`
- `src/JuiceSort/Assets/Scripts/Game/Audio/AudioManager.cs`
- `src/JuiceSort/Assets/Scripts/Tests/EditMode/AudioManagerTests.cs`

**Modified files:**
- `src/JuiceSort/Assets/Scripts/Game/Boot/BootLoader.cs` — creates and registers AudioManager
- `src/JuiceSort/Assets/Scripts/Game/UI/Screens/SettingsScreen.cs` — toggles wire to AudioManager
- `src/JuiceSort/Assets/Scripts/Game/Puzzle/GameplayManager.cs` — calls PlayMusic on LoadLevel
