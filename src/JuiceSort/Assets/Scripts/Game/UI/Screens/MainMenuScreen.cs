using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Core;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Main menu screen with title, Play, and Settings buttons.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        public static GameObject Create()
        {
            var go = new GameObject("MainMenuScreen");

            // Canvas
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
            bgImage.color = ThemeConfig.GetBackgroundGradientBottom(LevelGen.LevelMood.Morning);

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(go.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.6f);
            titleRect.anchorMax = new Vector2(0.9f, 0.85f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "JuiceSort";
            titleText.fontSize = 90;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = ThemeConfig.GetColor(LevelGen.LevelMood.Morning, ThemeColorType.StarGold);
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Play button
            CreateButton(go.transform, "PlayButton", "Play", new Vector2(0.25f, 0.35f), new Vector2(0.75f, 0.45f),
                ThemeConfig.GetColor(LevelGen.LevelMood.Morning, ThemeColorType.ButtonPrimary), () =>
                {
                    if (Services.TryGet<ScreenManager>(out var sm))
                        sm.TransitionTo(GameFlowState.Roadmap);
                });

            // Settings button
            CreateButton(go.transform, "SettingsButton", "Settings", new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.3f),
                ThemeConfig.GetColor(LevelGen.LevelMood.Morning, ThemeColorType.ButtonSecondary), () =>
                {
                    if (Services.TryGet<ScreenManager>(out var sm))
                        sm.TransitionTo(GameFlowState.Settings);
                });

            return go;
        }

        private static void CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject(name);
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
            text.fontSize = 48;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
