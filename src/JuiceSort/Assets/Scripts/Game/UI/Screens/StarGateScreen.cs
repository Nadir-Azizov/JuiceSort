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
    /// Displays star deficit and scrollable level list for replay.
    /// </summary>
    public class StarGateScreen : MonoBehaviour
    {
        private TextMeshProUGUI _headerText;
        private LevelListView _levelList;

        public event Action<int> OnLevelTapped;

        public void Show(IProgressionManager progression)
        {
            int currentStars = progression.GetCurrentBatchStars();
            int requiredStars = progression.GetBatchRequiredStars();
            int deficit = requiredStars - currentStars;

            _headerText.text = deficit > 0
                ? $"Batch Gate\nNeed {deficit} more stars ({currentStars}/{requiredStars})"
                : "Batch Unlocked!";

            var nodes = new List<LevelNodeData>();
            foreach (var record in progression.GetAllLevelRecords())
            {
                nodes.Add(LevelNodeData.FromRecord(record));
            }

            _levelList.Populate(nodes);
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

            // Level list area
            var listArea = new GameObject("ListArea");
            listArea.transform.SetParent(go.transform, false);
            var listRect = listArea.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.03f, 0.03f);
            listRect.anchorMax = new Vector2(0.97f, 0.8f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;

            screen._levelList = LevelListView.Create(listArea.transform);
            screen._levelList.OnLevelTapped += (levelNum) => screen.OnLevelTapped?.Invoke(levelNum);

            go.SetActive(false);
            return go;
        }
    }
}
