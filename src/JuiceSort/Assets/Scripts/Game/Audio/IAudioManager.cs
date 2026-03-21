using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.Audio
{
    /// <summary>
    /// Audio service interface. In Game assembly (references LevelMood).
    /// </summary>
    public interface IAudioManager
    {
        void PlayMusic(LevelMood mood);
        void StopMusic();
        void PlaySFX(AudioClipType type);
        void SetMusicEnabled(bool enabled);
        void SetSoundEnabled(bool enabled);
        bool MusicEnabled { get; }
        bool SoundEnabled { get; }
    }
}
