using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Reusable scrollable level list component.
    /// Used by both RoadmapScreen and StarGateScreen.
    /// </summary>
    public class LevelListView : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private RectTransform _contentRect;
        private readonly List<GameObject> _entries = new List<GameObject>();

        public event Action<int> OnLevelTapped;

        public void Initialize(Transform parent)
        {
            // ScrollRect container
            var scrollRect = gameObject.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            var scrollImage = gameObject.AddComponent<Image>();
            scrollImage.color = ThemeConfig.GetColor(ThemeColorType.Overlay);

            _scrollRect = gameObject.AddComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;

            // Content container
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(transform, false);
            _contentRect = contentGo.AddComponent<RectTransform>();
            _contentRect.anchorMin = new Vector2(0f, 1f);
            _contentRect.anchorMax = new Vector2(1f, 1f);
            _contentRect.pivot = new Vector2(0.5f, 1f);
            _contentRect.offsetMin = Vector2.zero;
            _contentRect.offsetMax = Vector2.zero;

            _scrollRect.content = _contentRect;

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;
            layout.padding = new RectOffset(10, 10, 10, 10);

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public void Populate(List<LevelNodeData> nodes)
        {
            ClearEntries();

            foreach (var node in nodes)
            {
                CreateEntry(node);
            }
        }

        public void ScrollToLevel(int levelNumber)
        {
            if (_entries.Count == 0)
                return;

            // Approximate scroll position based on level index
            int index = 0;
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].name == $"Level_{levelNumber}")
                {
                    index = i;
                    break;
                }
            }

            float normalizedPos = 1f - ((float)index / Mathf.Max(1, _entries.Count - 1));
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
        }

        private void CreateEntry(LevelNodeData node)
        {
            var entryGo = new GameObject($"Level_{node.LevelNumber}");
            entryGo.transform.SetParent(_contentRect, false);

            var entryLayout = entryGo.AddComponent<LayoutElement>();
            entryLayout.preferredHeight = 70f;

            var entryImage = entryGo.AddComponent<Image>();
            entryImage.color = node.IsCurrentLevel
                ? ThemeConfig.GetColor(ThemeColorType.ButtonPrimary)
                : ThemeConfig.GetColor(ThemeColorType.Overlay);

            // Level info text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(entryGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.03f, 0f);
            textRect.anchorMax = new Vector2(0.72f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            string moodIcon = node.Mood == LevelMood.Morning ? "\u2600" : "\u263E";
            string starText = node.IsCompleted ? StarCalculator.GetStarText(node.Stars) : "";
            text.text = $"Level {node.LevelNumber} - {node.CityName} {moodIcon} {starText}";
            text.fontSize = 26;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Play button
            if (node.IsCompleted || node.IsCurrentLevel)
            {
                var btnGo = new GameObject("PlayBtn");
                btnGo.transform.SetParent(entryGo.transform, false);
                var btnRect = btnGo.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.76f, 0.1f);
                btnRect.anchorMax = new Vector2(0.97f, 0.9f);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;

                var btnImage = btnGo.AddComponent<Image>();
                btnImage.color = node.IsCurrentLevel
                    ? ThemeConfig.GetColor(ThemeColorType.ButtonPrimary)
                    : ThemeConfig.GetColor(ThemeColorType.ButtonSecondary);

                var btn = btnGo.AddComponent<Button>();
                int levelNum = node.LevelNumber;
                btn.onClick.AddListener(() => OnLevelTapped?.Invoke(levelNum));

                var btnTextGo = new GameObject("BtnText");
                btnTextGo.transform.SetParent(btnGo.transform, false);
                var btnTextRect = btnTextGo.AddComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.offsetMin = Vector2.zero;
                btnTextRect.offsetMax = Vector2.zero;

                var btnText = btnTextGo.AddComponent<Text>();
                btnText.text = node.IsCurrentLevel ? "Start" : "Replay";
                btnText.fontSize = 22;
                btnText.alignment = TextAnchor.MiddleCenter;
                btnText.color = Color.white;
                btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            _entries.Add(entryGo);
        }

        private void ClearEntries()
        {
            foreach (var entry in _entries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            _entries.Clear();
        }

        public static LevelListView Create(Transform parent)
        {
            var go = new GameObject("LevelListView");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<LevelListView>();
            view.Initialize(parent);
            return view;
        }
    }
}
