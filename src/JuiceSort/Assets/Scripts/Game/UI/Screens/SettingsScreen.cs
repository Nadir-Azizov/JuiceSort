using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.Audio;
using JuiceSort.Game.Progression;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Settings screen with sound/music toggles.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        private TextMeshProUGUI _soundText;
        private TextMeshProUGUI _musicText;
        private bool _soundEnabled = true;
        private bool _musicEnabled = true;

        private void OnEnable()
        {
            // Guard: OnEnable fires during AddComponent before fields are assigned in Create()
            if (_soundText != null)
                Refresh();
        }

        public void Refresh()
        {
            if (_soundText == null) return;
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

            // Gradient Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.sprite = ThemeConfig.CreateGradientSprite(ThemeConfig.CurrentMood);
            bgImage.type = Image.Type.Simple;

            var screen = go.AddComponent<SettingsScreen>();

            // Title
            CreateLabel(go.transform, "Title", "Settings", ThemeConfig.FontSizeHeader,
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
            backGo.AddComponent<ButtonBounce>();
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
            var backText = backTextGo.AddComponent<TextMeshProUGUI>();
            backText.text = "Back";
            backText.fontSize = ThemeConfig.FontSizeBody;
            backText.alignment = TextAlignmentOptions.Center;
            backText.color = Color.white;
            backText.font = ThemeConfig.GetFont();

            return go;
        }

        private static void CreateLabel(Transform parent, string name, string text, float fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = TextAlignmentOptions.Center;
            t.color = ThemeConfig.GetColor(ThemeColorType.TextPrimary);
            t.font = ThemeConfig.GetFontBold();
            t.fontStyle = TMPro.FontStyles.Bold;
        }

        private static TextMeshProUGUI CreateToggleButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
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
            btnGo.AddComponent<ButtonBounce>();
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = name;
            text.fontSize = ThemeConfig.FontSizeBody;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.font = ThemeConfig.GetFont();
            return text;
        }
    }
}
