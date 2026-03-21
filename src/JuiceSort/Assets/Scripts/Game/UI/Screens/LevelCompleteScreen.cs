using System;
using UnityEngine;
using UnityEngine.UI;
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
        private Text _starText;
        private Text _infoText;
        private GameObject _nextLevelBtn;
        private GameObject _replayBtn;
        private GameObject _roadmapBtn;
        private GameObject _continueBtn;

        public event Action OnNextLevel;
        public event Action OnReplay;
        public event Action OnRoadmap;
        public event Action OnContinue;

        public void Show(int levelNumber, string cityName, int stars, int moves, int optimal, bool isReplay)
        {
            _starText.text = StarCalculator.GetStarText(stars);
            _infoText.text = $"Level {levelNumber} - {cityName}\nMoves: {moves} (Optimal: ~{optimal})";

            _nextLevelBtn.SetActive(!isReplay);
            _continueBtn.SetActive(isReplay);

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

            // Dark overlay background
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
            var starGo = CreateText(go.transform, "Stars", new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.72f), 80);
            screen._starText = starGo.GetComponent<Text>();
            screen._starText.color = ThemeConfig.GetColor(ThemeColorType.StarGold);

            // Info text
            var infoGo = CreateText(go.transform, "Info", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.55f), 34);
            screen._infoText = infoGo.GetComponent<Text>();

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

        private static GameObject CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize)
        {
            var textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btnGo;
        }
    }
}
