using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Gameplay HUD showing move count, undo button, restart button, and level info.
    /// Created by GameplayManager, positioned above the puzzle board.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private Text _moveText;
        private Text _undoText;
        private Text _levelInfoText;
        private Text _extraBottleText;
        private Button _undoButton;
        private Button _restartButton;
        private Button _extraBottleButton;

        public System.Action OnUndoPressed;
        public System.Action OnRestartPressed;
        public System.Action OnExtraBottlePressed;
        public System.Action OnBackPressed;

        public void UpdateDisplay(int moves, int undoRemaining, int extraBottlesRemaining = -1)
        {
            if (_moveText != null)
                _moveText.text = $"Moves: {moves}";

            if (_undoText != null)
                _undoText.text = $"Undo ({undoRemaining})";

            if (_extraBottleText != null && extraBottlesRemaining >= 0)
            {
                _extraBottleText.text = extraBottlesRemaining > 0 ? $"+ ({extraBottlesRemaining})" : "";
                if (_extraBottleButton != null)
                    _extraBottleButton.interactable = extraBottlesRemaining > 0;
            }
        }

        public void SetLevelInfo(int levelNumber, string cityName, string countryName, LevelGen.LevelMood mood)
        {
            if (_levelInfoText != null)
            {
                string moodIcon = mood == LevelGen.LevelMood.Morning ? "\u2600" : "\u263E";
                _levelInfoText.text = $"Level {levelNumber} — {cityName}, {countryName} {moodIcon}";
                _levelInfoText.color = ThemeConfig.GetColor(ThemeColorType.TextPrimary);
            }
        }

        public static GameplayHUD Create(Transform boardParent)
        {
            var go = new GameObject("GameplayHUD");
            go.transform.SetParent(boardParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.92f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var hud = go.AddComponent<GameplayHUD>();

            // Undo button (left)
            hud._undoButton = CreateHUDButton(go.transform, "UndoButton",
                new Vector2(0.02f, 0.05f), new Vector2(0.3f, 0.95f),
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            hud._undoButton.onClick.AddListener(() => hud.OnUndoPressed?.Invoke());
            hud._undoText = hud._undoButton.GetComponentInChildren<Text>();
            hud._undoText.text = "Undo (0)";

            // Level info (center)
            var infoGo = new GameObject("LevelInfo");
            infoGo.transform.SetParent(go.transform, false);
            var infoRect = infoGo.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.32f, 0.05f);
            infoRect.anchorMax = new Vector2(0.68f, 0.95f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            hud._levelInfoText = infoGo.AddComponent<Text>();
            hud._levelInfoText.text = "Level 1";
            hud._levelInfoText.fontSize = 28;
            hud._levelInfoText.alignment = TextAnchor.MiddleCenter;
            hud._levelInfoText.color = Color.white;
            hud._levelInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Move counter (center-right)
            var moveGo = new GameObject("MoveCounter");
            moveGo.transform.SetParent(go.transform, false);
            var moveRect = moveGo.AddComponent<RectTransform>();
            moveRect.anchorMin = new Vector2(0.32f, 0.05f);
            moveRect.anchorMax = new Vector2(0.68f, 0.95f);
            moveRect.offsetMin = Vector2.zero;
            moveRect.offsetMax = Vector2.zero;

            // Move text overlaps level info — position below
            var moveTextGo = new GameObject("MoveText");
            moveTextGo.transform.SetParent(go.transform, false);
            var moveTextRect = moveTextGo.AddComponent<RectTransform>();
            moveTextRect.anchorMin = new Vector2(0.32f, -0.9f);
            moveTextRect.anchorMax = new Vector2(0.68f, 0f);
            moveTextRect.offsetMin = Vector2.zero;
            moveTextRect.offsetMax = Vector2.zero;
            hud._moveText = moveTextGo.AddComponent<Text>();
            hud._moveText.text = "Moves: 0";
            hud._moveText.fontSize = 24;
            hud._moveText.alignment = TextAnchor.MiddleCenter;
            hud._moveText.color = ThemeConfig.GetColor(ThemeColorType.TextSecondary);
            hud._moveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Restart button (right)
            hud._restartButton = CreateHUDButton(go.transform, "RestartButton",
                new Vector2(0.7f, 0.05f), new Vector2(0.98f, 0.95f),
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            hud._restartButton.onClick.AddListener(() => hud.OnRestartPressed?.Invoke());
            hud._restartButton.GetComponentInChildren<Text>().text = "Restart";

            // Extra bottle button (between undo and level info, below HUD bar)
            hud._extraBottleButton = CreateHUDButton(go.transform, "ExtraBottleButton",
                new Vector2(0.02f, -1.2f), new Vector2(0.2f, -0.1f),
                ThemeConfig.GetColor(ThemeColorType.ButtonPrimary));
            hud._extraBottleButton.onClick.AddListener(() => hud.OnExtraBottlePressed?.Invoke());
            hud._extraBottleText = hud._extraBottleButton.GetComponentInChildren<Text>();
            hud._extraBottleText.text = "+ (2)";

            // Back button (top-left area, below HUD)
            var backBtn = CreateHUDButton(go.transform, "BackButton",
                new Vector2(0.22f, -1.2f), new Vector2(0.42f, -0.1f),
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            backBtn.onClick.AddListener(() => hud.OnBackPressed?.Invoke());
            backBtn.GetComponentInChildren<Text>().text = "\u2190";
            backBtn.GetComponentInChildren<Text>().fontSize = 30;

            return hud;
        }

        private static Button CreateHUDButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
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

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            text.text = name;
            text.fontSize = 26;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return button;
        }
    }
}
