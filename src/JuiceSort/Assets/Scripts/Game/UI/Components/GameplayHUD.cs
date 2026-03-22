using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Gameplay HUD with organized top bar and bottom bar layout.
    /// Top bar: [Back] [Level Info + Moves] [Coin] [Settings]
    /// Bottom bar: [Undo] [Ad] [Restart] [Extra Bottle]
    /// Both bars respect Screen.safeArea and fit within layout reserves.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private Text _moveText;
        private Text _undoText;
        private Text _levelInfoText;
        private Text _coinText;
        private Text _extraBottleText;
        private Text _adText;
        private Button _undoButton;
        private Button _restartButton;
        private Button _extraBottleButton;
        private Button _adButton;
        private Button _settingsButton;

        private RectTransform _topBar;
        private RectTransform _bottomBar;

        public System.Action OnUndoPressed;
        public System.Action OnRestartPressed;
        public System.Action OnExtraBottlePressed;
        public System.Action OnAdWatchPressed;
        public System.Action OnBackPressed;
        public System.Action OnSettingsPressed;

        // Button sizing constants (in reference pixels at 1080x1920)
        private const float ButtonSize = 96f;   // 48dp * 2 for reference res
        private const float ButtonPadding = 16f; // 8dp * 2
        private const float BarHeight = 120f;    // Comfortable bar height
        private const float BarMargin = 20f;     // Inner margin from screen edges

        public void UpdateDisplay(int moves)
        {
            if (_moveText != null)
                _moveText.text = $"Moves: {moves}";
        }

        public void UpdateUndoState(int cost, bool canAfford, bool hasUndos)
        {
            if (_undoText != null)
                _undoText.text = hasUndos ? $"\u21B6 {cost}" : "\u21B6";

            if (_undoButton != null)
                _undoButton.interactable = canAfford && hasUndos;
        }

        public void UpdateExtraBottleState(int cost, bool canAfford, bool hasRemaining)
        {
            if (_extraBottleText != null)
                _extraBottleText.text = hasRemaining ? $"+ {cost}" : "";

            if (_extraBottleButton != null)
                _extraBottleButton.interactable = canAfford && hasRemaining;
        }

        public void UpdateAdButtonState(bool isAvailable, int rewardAmount)
        {
            if (_adText != null)
                _adText.text = $"\u25B6 {rewardAmount}";

            if (_adButton != null)
                _adButton.interactable = isAvailable;
        }

        public void UpdateCoinDisplay(int coinBalance)
        {
            if (_coinText != null)
                _coinText.text = coinBalance.ToString();
        }

        public void SetLevelInfo(int levelNumber, string cityName, string countryName, LevelGen.LevelMood mood)
        {
            if (_levelInfoText != null)
            {
                string moodIcon = mood == LevelGen.LevelMood.Morning ? "\u2600" : "\u263E";
                _levelInfoText.text = $"Level {levelNumber} {moodIcon}\n{cityName}, {countryName}";
                _levelInfoText.color = ThemeConfig.GetColor(ThemeColorType.TextPrimary);
            }
        }

        public static GameplayHUD Create(Transform canvasParent)
        {
            // Root container fills the full canvas
            var go = new GameObject("GameplayHUD");
            go.transform.SetParent(canvasParent, false);
            var rootRect = go.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var hud = go.AddComponent<GameplayHUD>();

            // Apply SafeArea to a container that holds both bars
            var safeGo = new GameObject("SafeArea");
            safeGo.transform.SetParent(go.transform, false);
            var safeRect = safeGo.AddComponent<RectTransform>();
            ApplySafeArea(safeRect);

            // --- TOP BAR ---
            hud._topBar = CreateBar(safeGo.transform, "TopBar", true);
            CreateTopBarContent(hud, hud._topBar);

            // --- BOTTOM BAR ---
            hud._bottomBar = CreateBar(safeGo.transform, "BottomBar", false);
            CreateBottomBarContent(hud, hud._bottomBar);

            return hud;
        }

        private static void ApplySafeArea(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;

            var safeArea = Screen.safeArea;
            var screenW = Screen.width;
            var screenH = Screen.height;

            if (screenW <= 0 || screenH <= 0)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                return;
            }

            // Convert safeArea pixel rect to anchor offsets
            var offsetMin = new Vector2(
                safeArea.x / screenW,
                safeArea.y / screenH
            );
            var offsetMax = new Vector2(
                -(screenW - safeArea.xMax) / screenW,
                -(screenH - safeArea.yMax) / screenH
            );

            // Apply as anchor-relative offsets
            rect.anchorMin = new Vector2(offsetMin.x, offsetMin.y);
            rect.anchorMax = new Vector2(1f + offsetMax.x, 1f + offsetMax.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateBar(Transform parent, string name, bool isTop)
        {
            var barGo = new GameObject(name);
            barGo.transform.SetParent(parent, false);
            var barRect = barGo.AddComponent<RectTransform>();

            if (isTop)
            {
                barRect.anchorMin = new Vector2(0f, 1f);
                barRect.anchorMax = new Vector2(1f, 1f);
                barRect.pivot = new Vector2(0.5f, 1f);
                barRect.sizeDelta = new Vector2(0f, BarHeight);
            }
            else
            {
                barRect.anchorMin = new Vector2(0f, 0f);
                barRect.anchorMax = new Vector2(1f, 0f);
                barRect.pivot = new Vector2(0.5f, 0f);
                barRect.sizeDelta = new Vector2(0f, BarHeight);
            }

            barRect.anchoredPosition = Vector2.zero;
            return barRect;
        }

        private static void CreateTopBarContent(GameplayHUD hud, RectTransform topBar)
        {
            // Back button (far left)
            var backBtn = CreateSquareButton(topBar, "BackButton",
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            AnchorLeft(backBtn.GetComponent<RectTransform>(), BarMargin);
            backBtn.GetComponentInChildren<Text>().text = "\u2190";
            backBtn.GetComponentInChildren<Text>().fontSize = 32;
            backBtn.onClick.AddListener(() => hud.OnBackPressed?.Invoke());

            // Level info + move counter (center)
            var infoGo = new GameObject("LevelInfo");
            infoGo.transform.SetParent(topBar, false);
            var infoRect = infoGo.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.2f, 0.05f);
            infoRect.anchorMax = new Vector2(0.57f, 0.95f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            hud._levelInfoText = infoGo.AddComponent<Text>();
            hud._levelInfoText.text = "Level 1";
            hud._levelInfoText.fontSize = 24;
            hud._levelInfoText.alignment = TextAnchor.MiddleLeft;
            hud._levelInfoText.color = Color.white;
            hud._levelInfoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Move counter (right of level info, no overlap)
            var moveGo = new GameObject("MoveCounter");
            moveGo.transform.SetParent(topBar, false);
            var moveRect = moveGo.AddComponent<RectTransform>();
            moveRect.anchorMin = new Vector2(0.58f, 0.05f);
            moveRect.anchorMax = new Vector2(0.72f, 0.95f);
            moveRect.offsetMin = Vector2.zero;
            moveRect.offsetMax = Vector2.zero;

            hud._moveText = moveGo.AddComponent<Text>();
            hud._moveText.text = "Moves: 0";
            hud._moveText.fontSize = 20;
            hud._moveText.alignment = TextAnchor.MiddleCenter;
            hud._moveText.color = ThemeConfig.GetColor(ThemeColorType.TextSecondary);
            hud._moveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Coin display (right of moves)
            var coinGo = new GameObject("CoinDisplay");
            coinGo.transform.SetParent(topBar, false);
            var coinRect = coinGo.AddComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0.72f, 0.15f);
            coinRect.anchorMax = new Vector2(0.88f, 0.85f);
            coinRect.offsetMin = Vector2.zero;
            coinRect.offsetMax = Vector2.zero;

            hud._coinText = coinGo.AddComponent<Text>();
            hud._coinText.text = "0";
            hud._coinText.fontSize = 22;
            hud._coinText.alignment = TextAnchor.MiddleRight;
            hud._coinText.color = ThemeConfig.GetColor(ThemeColorType.StarGold);
            hud._coinText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Settings button (far right)
            hud._settingsButton = CreateSquareButton(topBar, "SettingsButton",
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            AnchorRight(hud._settingsButton.GetComponent<RectTransform>(), BarMargin);
            hud._settingsButton.GetComponentInChildren<Text>().text = "\u2699";
            hud._settingsButton.GetComponentInChildren<Text>().fontSize = 32;
            hud._settingsButton.onClick.AddListener(() => hud.OnSettingsPressed?.Invoke());
        }

        private static void CreateBottomBarContent(GameplayHUD hud, RectTransform bottomBar)
        {
            // Undo button (left) — shows "↶ N" as single button
            hud._undoButton = CreateSquareButton(bottomBar, "UndoButton",
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            AnchorLeft(hud._undoButton.GetComponent<RectTransform>(), BarMargin);
            hud._undoText = hud._undoButton.GetComponentInChildren<Text>();
            hud._undoText.text = "\u21B6 0";
            hud._undoText.fontSize = 24;
            hud._undoButton.onClick.AddListener(() => hud.OnUndoPressed?.Invoke());

            // Watch Ad button (between undo and restart)
            hud._adButton = CreateSquareButton(bottomBar, "AdButton",
                ThemeConfig.GetColor(ThemeColorType.StarGold));
            var adRect = hud._adButton.GetComponent<RectTransform>();
            adRect.anchorMin = new Vector2(0.3f, 0.5f);
            adRect.anchorMax = new Vector2(0.3f, 0.5f);
            adRect.pivot = new Vector2(0.5f, 0.5f);
            adRect.anchoredPosition = Vector2.zero;
            hud._adText = hud._adButton.GetComponentInChildren<Text>();
            hud._adText.text = "\u25B6";
            hud._adText.fontSize = 20;
            hud._adButton.onClick.AddListener(() => hud.OnAdWatchPressed?.Invoke());

            // Restart button (center)
            hud._restartButton = CreateSquareButton(bottomBar, "RestartButton",
                ThemeConfig.GetColor(ThemeColorType.ButtonSecondary));
            AnchorCenter(hud._restartButton.GetComponent<RectTransform>());
            hud._restartButton.GetComponentInChildren<Text>().text = "\u21BB";
            hud._restartButton.GetComponentInChildren<Text>().fontSize = 32;
            hud._restartButton.onClick.AddListener(() => hud.OnRestartPressed?.Invoke());

            // Extra bottle button (right)
            hud._extraBottleButton = CreateSquareButton(bottomBar, "ExtraBottleButton",
                ThemeConfig.GetColor(ThemeColorType.ButtonPrimary));
            AnchorRight(hud._extraBottleButton.GetComponent<RectTransform>(), BarMargin);
            hud._extraBottleText = hud._extraBottleButton.GetComponentInChildren<Text>();
            hud._extraBottleText.text = "+2";
            hud._extraBottleText.fontSize = 28;
            hud._extraBottleButton.onClick.AddListener(() => hud.OnExtraBottlePressed?.Invoke());
        }

        // --- Button factory ---

        private static Button CreateSquareButton(Transform parent, string name, Color bgColor)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(ButtonSize, ButtonSize);

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = bgColor;

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

        // --- Anchor helpers ---

        private static void AnchorLeft(RectTransform rect, float leftOffset)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(leftOffset, 0f);
        }

        private static void AnchorRight(RectTransform rect, float rightOffset)
        {
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-rightOffset, 0f);
        }

        private static void AnchorCenter(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
