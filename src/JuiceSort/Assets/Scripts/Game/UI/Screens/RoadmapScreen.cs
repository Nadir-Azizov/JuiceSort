using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.UI;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// Scrollable roadmap showing all completed levels and the next available level.
    /// Players can tap any level to play/replay it.
    /// </summary>
    public class RoadmapScreen : MonoBehaviour
    {
        private LevelListView _levelList;
        private TextMeshProUGUI _headerText;

        private void OnEnable()
        {
            // Guard: OnEnable fires during AddComponent before fields are assigned in Create()
            if (_levelList != null)
                Refresh();
        }

        public void Refresh()
        {
            if (_levelList == null || !Services.TryGet<IProgressionManager>(out var progression))
                return;

            var nodes = BuildNodeList(progression);
            _levelList.Populate(nodes);
            _levelList.ScrollToLevel(progression.CurrentLevel);

            _headerText.text = $"Roadmap — {progression.GetTotalStars()} Stars";
        }

        private void OnLevelTapped(int levelNumber)
        {
            if (!Services.TryGet<IProgressionManager>(out var progression))
                return;

            if (!Services.TryGet<GameplayManager>(out var gameplay))
                return;

            if (!Services.TryGet<ScreenManager>(out var screenMgr))
                return;

            if (progression.IsLevelCompleted(levelNumber))
            {
                gameplay.StartReplay(levelNumber);
            }
            else if (gameplay.HasPausedLevel(levelNumber))
            {
                gameplay.ResumeLevel(levelNumber);
            }
            else
            {
                gameplay.StartLevel(levelNumber);
            }

            screenMgr.TransitionTo(GameFlowState.Playing);
        }

        private List<LevelNodeData> BuildNodeList(IProgressionManager progression)
        {
            var nodes = new List<LevelNodeData>();

            // Completed levels
            var records = progression.GetAllLevelRecords();
            foreach (var record in records)
            {
                nodes.Add(LevelNodeData.FromRecord(record));
            }

            // Current level (next to play, not yet completed)
            // Only show if not blocked by a gate
            int currentLevel = progression.CurrentLevel;
            int previousLevel = currentLevel - 1;
            bool gateBlocked = previousLevel > 0
                && progression.IsAtBatchGate(previousLevel)
                && !progression.CanPassBatchGate();

            if (!progression.IsLevelCompleted(currentLevel) && !gateBlocked)
            {
                var city = CityAssigner.AssignCity(currentLevel);
                var mood = CityAssigner.AssignMood(currentLevel);
                nodes.Add(LevelNodeData.ForCurrentLevel(currentLevel, city, mood));
            }

            return nodes;
        }

        public static GameObject Create()
        {
            var go = new GameObject("RoadmapScreen");

            // Canvas
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

            // Header
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(go.transform, false);
            var headerRect = headerGo.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.9f);
            headerRect.anchorMax = new Vector2(0.7f, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            var screen = go.AddComponent<RoadmapScreen>();
            screen._headerText = headerGo.AddComponent<TextMeshProUGUI>();
            screen._headerText.text = "Roadmap";
            screen._headerText.fontSize = ThemeConfig.FontSizeHeader;
            screen._headerText.alignment = TextAlignmentOptions.MidlineLeft;
            screen._headerText.color = ThemeConfig.GetColor(ThemeColorType.TextPrimary);
            screen._headerText.font = ThemeConfig.GetFontBold();
            screen._headerText.fontStyle = TMPro.FontStyles.Bold;

            // Level list area
            var listArea = new GameObject("ListArea");
            listArea.transform.SetParent(go.transform, false);
            var listRect = listArea.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.03f, 0.03f);
            listRect.anchorMax = new Vector2(0.97f, 0.88f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;

            screen._levelList = LevelListView.Create(listArea.transform);
            screen._levelList.OnLevelTapped += screen.OnLevelTapped;

            // Back button
            var backGo = new GameObject("BackButton");
            backGo.transform.SetParent(go.transform, false);
            var backRect = backGo.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.75f, 0.91f);
            backRect.anchorMax = new Vector2(0.97f, 0.98f);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;

            var backImage = backGo.AddComponent<Image>();
            backImage.color = ThemeConfig.GetColor(LevelMood.Morning, ThemeColorType.ButtonSecondary);

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
    }
}
