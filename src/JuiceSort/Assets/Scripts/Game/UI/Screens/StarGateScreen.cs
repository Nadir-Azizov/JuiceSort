using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;
using JuiceSort.Game.UI;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Star gate screen shown when batch progression is blocked.
    /// Displays star deficit and a button to go to the roadmap for replaying levels.
    /// </summary>
    public class StarGateScreen : MonoBehaviour
    {
        private TextMeshProUGUI _headerText;

        public event Action<int> OnLevelTapped;

        public void Show(IProgressionManager progression)
        {
            int currentStars = progression.GetCurrentBatchStars();
            int requiredStars = progression.GetBatchRequiredStars();
            int deficit = requiredStars - currentStars;

            _headerText.text = deficit > 0
                ? $"Batch Gate\nNeed {deficit} more stars ({currentStars}/{requiredStars})"
                : "Batch Unlocked!";

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public static GameObject Create()
        {
            var go = new GameObject("StarGateScreen");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 25;

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

            var screen = go.AddComponent<StarGateScreen>();

            // Header
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(go.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.82f);
            headerRect.anchorMax = new Vector2(0.95f, 0.97f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;
            screen._headerText = headerGo.AddComponent<TextMeshProUGUI>();
            screen._headerText.fontSize = ThemeConfig.FontSizeHeader;
            screen._headerText.alignment = TextAlignmentOptions.Center;
            screen._headerText.color = ThemeConfig.GetColor(ThemeColorType.TextPrimary);
            screen._headerText.font = ThemeConfig.GetFontBold();
            screen._headerText.fontStyle = TMPro.FontStyles.Bold;

            // "Go to Roadmap" button
            var btnGo = new GameObject("RoadmapButton");
            btnGo.transform.SetParent(go.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.2f, 0.4f);
            btnRect.anchorMax = new Vector2(0.8f, 0.48f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = ThemeConfig.GetColor(ThemeColorType.ButtonPrimary);

            var btn = btnGo.AddComponent<Button>();
            btnGo.AddComponent<ButtonBounce>();
            btn.onClick.AddListener(() =>
            {
                if (Services.TryGet<ScreenManager>(out var sm))
                {
                    sm.HideOverlay(GameFlowState.Gate);
                    sm.TransitionTo(GameFlowState.Roadmap);
                }
            });

            var btnTextGo = new GameObject("BtnText");
            btnTextGo.transform.SetParent(btnGo.transform, false);
            var btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            var btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnText.text = "Replay Levels";
            btnText.fontSize = ThemeConfig.FontSizeSecondary;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.font = ThemeConfig.GetFont();

            go.SetActive(false);
            return go;
        }
    }
}
