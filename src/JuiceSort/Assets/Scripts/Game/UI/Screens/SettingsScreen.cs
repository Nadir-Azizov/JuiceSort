using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Core;
using JuiceSort.Game.Audio;
using JuiceSort.Game.Progression;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Settings screen with sound/music toggles.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        private Text _soundText;
        private Text _musicText;
        private bool _soundEnabled = true;
        private bool _musicEnabled = true;

        public void Refresh()
        {
            if (Services.TryGet<IProgressionManager>(out var progression))
            {
                _soundEnabled = progression.SoundEnabled;
                _musicEnabled = progression.MusicEnabled;
            }

            UpdateDisplay();
        }

        private void ToggleSound()
        {
            _soundEnabled = !_soundEnabled;

            if (Services.TryGet<IProgressionManager>(out var progression))
                progression.SoundEnabled = _soundEnabled;

            if (Services.TryGet<IAudioManager>(out var audio))
                audio.SetSoundEnabled(_soundEnabled);

            UpdateDisplay();
        }

        private void ToggleMusic()
        {
            _musicEnabled = !_musicEnabled;

            if (Services.TryGet<IProgressionManager>(out var progression))
                progression.MusicEnabled = _musicEnabled;

            if (Services.TryGet<IAudioManager>(out var audio))
                audio.SetMusicEnabled(_musicEnabled);

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_soundText != null)
                _soundText.text = $"Sound: {(_soundEnabled ? "ON" : "OFF")}";
            if (_musicText != null)
                _musicText.text = $"Music: {(_musicEnabled ? "ON" : "OFF")}";
        }

        public static GameObject Create()
        {
            var go = new GameObject("SettingsScreen");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = ThemeConfig.GetColor(LevelGen.LevelMood.Morning, ThemeColorType.Background);

            var screen = go.AddComponent<SettingsScreen>();

            // Title
            CreateLabel(go.transform, "Title", "Settings", 60,
                new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.9f));

            // Sound toggle
            screen._soundText = CreateToggleButton(go.transform, "SoundToggle",
                new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.65f),
                screen.ToggleSound);

            // Music toggle
            screen._musicText = CreateToggleButton(go.transform, "MusicToggle",
                new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.5f),
                screen.ToggleMusic);

            // Back button
            var backGo = new GameObject("BackButton");
            backGo.transform.SetParent(go.transform, false);
            var backRect = backGo.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.3f, 0.2f);
            backRect.anchorMax = new Vector2(0.7f, 0.3f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;
            var backImage = backGo.AddComponent<Image>();
            backImage.color = ThemeConfig.GetColor(LevelGen.LevelMood.Morning, ThemeColorType.ButtonSecondary);
            var backBtn = backGo.AddComponent<Button>();
            backBtn.onClick.AddListener(() =>
            {
                if (Services.TryGet<ScreenManager>(out var sm))
                    sm.TransitionTo(GameFlowState.MainMenu);
            });

            var backTextGo = new GameObject("Text");
            backTextGo.transform.SetParent(backGo.transform, false);
            var backTextRect = backTextGo.AddComponent<RectTransform>();
            backTextRect.anchorMin = Vector2.zero;
            backTextRect.anchorMax = Vector2.one;
            backTextRect.offsetMin = Vector2.zero;
            backTextRect.offsetMax = Vector2.zero;
            var backText = backTextGo.AddComponent<Text>();
            backText.text = "Back";
            backText.fontSize = 40;
            backText.alignment = TextAnchor.MiddleCenter;
            backText.color = Color.white;
            backText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return go;
        }

        private static void CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Text CreateToggleButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = btnGo.AddComponent<Image>();
            img.color = ThemeConfig.GetColor(LevelGen.LevelMood.Morning, ThemeColorType.ButtonSecondary);
            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.text = "Toggle";
            text.fontSize = 40;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }
    }
}
