using UnityEngine;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;

namespace JuiceSort.Game.Audio
{
    /// <summary>
    /// Centralized audio service. Manages music and SFX playback.
    /// Placeholder: logs instead of playing audio until actual clips are assigned.
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private bool _musicEnabled = true;
        private bool _soundEnabled = true;
        private LevelMood _currentMood = (LevelMood)(-1); // invalid initial so first PlayMusic always triggers

        public bool MusicEnabled => _musicEnabled;
        public bool SoundEnabled => _soundEnabled;

        private void Awake()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }

        private void Start()
        {
            // Read initial settings from progression
            if (Services.TryGet<IProgressionManager>(out var progression))
            {
                _musicEnabled = progression.MusicEnabled;
                _soundEnabled = progression.SoundEnabled;
            }
        }

        public void PlayMusic(LevelMood mood)
        {
            if (!_musicEnabled)
                return;

            // Don't restart if already playing the same mood
            if (mood == _currentMood && _musicSource.isPlaying)
                return;

            _currentMood = mood;

            // Placeholder: log until actual AudioClips are assigned
            // When audio assets exist, load clip by mood and play:
            // _musicSource.clip = GetMusicClip(mood);
            // _musicSource.Play();
            Debug.Log($"[AudioManager] Playing {mood} music");
        }

        public void StopMusic()
        {
            _musicSource.Stop();
            Debug.Log("[AudioManager] Music stopped");
        }

        public void PlaySFX(AudioClipType type)
        {
            if (!_soundEnabled)
                return;

            // Placeholder: log until actual AudioClips are assigned
            // When audio assets exist:
            // var clip = GetSFXClip(type);
            // if (clip != null) _sfxSource.PlayOneShot(clip);
            Debug.Log($"[AudioManager] SFX: {type}");
        }

        public void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;

            if (!enabled)
            {
                _musicSource.Stop();
            }
            else if (_currentMood != (LevelMood)(-1))
            {
                PlayMusic(_currentMood);
            }

            Debug.Log($"[AudioManager] Music {(enabled ? "enabled" : "disabled")}");
        }

        public void SetSoundEnabled(bool enabled)
        {
            _soundEnabled = enabled;
            Debug.Log($"[AudioManager] Sound {(enabled ? "enabled" : "disabled")}");
        }
    }
}
