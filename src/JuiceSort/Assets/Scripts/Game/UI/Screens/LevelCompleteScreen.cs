using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Game.Progression;
using JuiceSort.Game.UI;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Level complete overlay showing stars, move stats, and action buttons.
    /// </summary>
    public class LevelCompleteScreen : MonoBehaviour
    {
        private TextMeshProUGUI _starText;
        private TextMeshProUGUI _infoText;
        private TextMeshProUGUI _coinRewardText;
        private GameObject _nextLevelBtn;
        private GameObject _replayBtn;
        private GameObject _roadmapBtn;
        private GameObject _continueBtn;

        public event Action OnNextLevel;
        public event Action OnReplay;
        public event Action OnRoadmap;
        public event Action OnContinue;

        public void Show(int levelNumber, string cityName, int stars, int moves, int optimal, bool isReplay, int coinReward = 0)
        {
            if (_starText != null)
                _starText.text = StarCalculator.GetStarText(stars);
            if (_infoText != null)
                _infoText.text = $"Level {levelNumber} - {cityName}\nMoves: {moves} (Optimal: ~{optimal})";
            if (_coinRewardText != null)
                _coinRewardText.text = coinReward > 0 ? $"+{coinReward} coins" : "";

            if (_nextLevelBtn != null) _nextLevelBtn.SetActive(!isReplay);
            if (_continueBtn != null) _continueBtn.SetActive(isReplay);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public static GameObject Create()
        {
            var go = new GameObject("LevelCompleteScreen");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            // Dark overlay gradient background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = ThemeConfig.GetColor(ThemeColorType.Overlay);

            var screen = go.AddComponent<LevelCompleteScreen>();

            // Stars
            var starGo = CreateText(go.transform, "Stars", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.72f), ThemeConfig.FontSizeTitle);
            screen._starText = starGo.GetComponent<TextMeshProUGUI>();
            screen._starText.color = ThemeConfig.GetColor(ThemeColorType.StarGold);

            // Info text
            var infoGo = CreateText(go.transform, "Info", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.55f), ThemeConfig.FontSizeHeader);
            screen._infoText = infoGo.GetComponent<TextMeshProUGUI>();

            // Coin reward text
            var coinGo = CreateText(go.transform, "CoinReward", new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.44f), ThemeConfig.FontSizeBody);
            screen._coinRewardText = coinGo.GetComponent<TextMeshProUGUI>();
            screen._coinRewardText.color = ThemeConfig.GetColor(ThemeColorType.StarGold);

            // Next Level button
            screen._nextLevelBtn = CreateActionButton(go.transform, "Next Level",
                new Vector2(0.15f, 0.28f), new Vector2(0.52f, 0.38f),
                ThemeConfig.GetColor(ThemeColorType.ButtonPrimary),
                () => screen.OnNextLevel?.Invoke());

            // Continue button (for replay — checks gate)
            screen._continueBtn = CreateActionButton(go.transform, "Continue",
                new Vector2(0.15f, 0.28f), new Vector2(0.52f, 0.38f),
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary),
                () => screen.OnContinue?.Invoke());

            // Replay button
            screen._replayBtn = CreateActionButton(go.transform, "Replay",
                new Vector2(0.55f, 0.28f), new Vector2(0.85f, 0.38f),
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary),
                () => screen.OnReplay?.Invoke());

            // Roadmap button
            screen._roadmapBtn = CreateActionButton(go.transform, "Roadmap",
                new Vector2(0.3f, 0.16f), new Vector2(0.7f, 0.25f),
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary),
                () => screen.OnRoadmap?.Invoke());

            go.SetActive(false);
            return go;
        }

        private static GameObject CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float fontSize)
        {
            var textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = ThemeConfig.GetColor(ThemeColorType.TextPrimary);
            text.font = ThemeConfig.GetFont();
            return textGo;
        }

        private static GameObject CreateActionButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject($"{label}Button");
            btnGo.transform.SetParent(parent, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = color;
            var button = btnGo.AddComponent<Button>();
            btnGo.AddComponent<ButtonBounce>();
            button.onClick.AddListener(onClick);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = ThemeConfig.FontSizeBody;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.font = ThemeConfig.GetFont();

            return btnGo;
        }
    }
}
