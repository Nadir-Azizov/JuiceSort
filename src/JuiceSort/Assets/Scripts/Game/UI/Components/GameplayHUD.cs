using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.Audio;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Gameplay HUD built to UI Spec v3.2.
    /// Top-left: Coin display with glow pulse
    /// Top-center-right: Level/moves pill
    /// Top-right: Expandable settings gear with 3D toggle buttons
    /// Bottom: Action pill with Undo + Extra Bottle buttons
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        // Top bar
        private TextMeshProUGUI _coinText;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _moveText;
        private Button _settingsGearButton;

        // Settings panel
        private RectTransform _settingsPanel;
        private Button _musicToggleButton;
        private Button _sfxToggleButton;
        private Button _vibrationToggleButton;
        private Button _restartButton;
        private Button _exitButton;
        private Image _musicFaceImage;
        private Image _sfxFaceImage;
        private Image _vibrationFaceImage;
        private Image _musicIconImage;
        private Image _sfxIconImage;
        private Image _vibrationIconImage;

        // Cached icon sprites for toggle swapping
        private Sprite _musicOnSprite;
        private Sprite _musicOffSprite;
        private Sprite _sfxOnSprite;
        private Sprite _sfxOffSprite;
        private Sprite _restartSprite;
        private bool _isSettingsOpen;
        private bool _musicEnabled = true;
        private bool _sfxEnabled = true;
        private bool _vibrationEnabled = true;
        private Coroutine _settingsAnimCoroutine;

        // Outside tap blocker
        private GameObject _outsideTapBlocker;
        private Image _blockerImage;
        private Coroutine _blockerFadeCoroutine;

        // Bottom bar
        private TextMeshProUGUI _undoCostText;
        private TextMeshProUGUI _extraBottleCostText;
        private Button _undoButton;
        private Button _extraBottleButton;
        private CanvasGroup _undoCanvasGroup;
        private CanvasGroup _extraBottleCanvasGroup;
        private Image _undoFillImage;
        private Image _extraBottleFillImage;
        private Color _undoFillColor;
        private Color _extraBottleFillColor;

        // Coin pulse
        private Image _coinGlowImage;
        private Coroutine _coinPulseCoroutine;

        // Layout
        private RectTransform _topBar;
        private RectTransform _bottomBar;
        private RectTransform _safeAreaRect;
        private Rect _lastSafeArea;

        // Events
        public System.Action OnUndoPressed;
        public System.Action OnRestartPressed;
        public System.Action OnExtraBottlePressed;
        public System.Action OnExitPressed;
        public System.Action OnSettingsPressed;

        // === Sizing constants (§9, reference 1080×1920) ===
        private const float BarHeight = 186f;
        private const float BarMargin = 54f;
        private const float BottomBarHeight = 300f;
        private const float GearButtonSize = 108f;
        private const float SettingsButtonSize = 108f;
        private const float SettingsButtonSpacing = 30f;

        // === Colors (§4) ===
        private static readonly Color GreenON = new Color(0.133f, 0.867f, 0.267f);       // #22DD44
        private static readonly Color GreenON_Shadow = new Color(0.024f, 0.6f, 0.125f);  // #069920
        private static readonly Color GrayOFF = new Color(0.369f, 0.369f, 0.369f);       // #5E5E5E
        private static readonly Color GrayOFF_Shadow = new Color(0.2f, 0.2f, 0.2f);      // #333333
        private static readonly Color RedExit = new Color(0.933f, 0.125f, 0.125f);       // #EE2020
        private static readonly Color RedExit_Shadow = new Color(0.6f, 0.02f, 0.02f);    // #990505
        private static readonly Color CoinBorder = new Color(0.80f, 0.53f, 0f);          // #CC8800
        private static readonly Color CoinSymbolColor = new Color(0.545f, 0.412f, 0.078f);// #8B6914
        private static readonly Color CoinAmountGold = new Color(1f, 0.878f, 0.4f);      // #FFE066
        private static readonly Color CostGold = new Color(1f, 0.863f, 0.392f, 0.9f);

        // ============================
        // PUBLIC API
        // ============================

        public bool IsSettingsOpen => _isSettingsOpen;

        public void UpdateDisplay(int moves)
        {
            if (_moveText != null)
                _moveText.text = $"{moves} moves";
        }

        public void UpdateUndoState(int cost, bool canAfford, bool hasUndos)
        {
            if (_undoCostText != null)
                _undoCostText.text = $"{cost}";
            bool enabled = canAfford && hasUndos;
            if (_undoButton != null)
            {
                _undoButton.interactable = enabled;
                var bounce = _undoButton.GetComponent<ButtonBounce>();
                if (bounce != null) bounce.enabled = enabled;
            }
            if (_undoCanvasGroup != null) _undoCanvasGroup.alpha = enabled ? 1f : 0.6f;
            if (_undoFillImage != null)
                _undoFillImage.color = enabled ? _undoFillColor : GrayOFF;
        }

        public void UpdateExtraBottleState(int cost, bool canAfford, bool hasRemaining)
        {
            if (_extraBottleCostText != null)
                _extraBottleCostText.text = $"{cost}";
            bool enabled = canAfford && hasRemaining;
            if (_extraBottleButton != null)
            {
                _extraBottleButton.interactable = enabled;
                var bounce = _extraBottleButton.GetComponent<ButtonBounce>();
                if (bounce != null) bounce.enabled = enabled;
            }
            if (_extraBottleCanvasGroup != null) _extraBottleCanvasGroup.alpha = enabled ? 1f : 0.6f;
            if (_extraBottleFillImage != null)
                _extraBottleFillImage.color = enabled ? _extraBottleFillColor : GrayOFF;
        }

        public void UpdateCoinDisplay(int coinBalance)
        {
            if (_coinText != null)
                _coinText.text = coinBalance.ToString("N0");
        }

        public void SetLevelInfo(int levelNumber, string cityName, string countryName, LevelMood mood)
        {
            if (_levelText != null)
                _levelText.text = $"Level {levelNumber}";
        }

        public void CollapseSettingsIfOpen()
        {
            if (_isSettingsOpen)
            {
                Debug.Log("[GameplayHUD] CollapseSettingsIfOpen triggered");
                ToggleSettings();
            }
        }

        // ============================
        // FACTORY
        // ============================

        public static GameplayHUD Create(Transform canvasParent)
        {
            var go = new GameObject("GameplayHUD");
            go.transform.SetParent(canvasParent, false);
            var rootRect = go.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var hud = go.AddComponent<GameplayHUD>();

            // SafeArea
            var safeGo = new GameObject("SafeArea");
            safeGo.transform.SetParent(go.transform, false);
            hud._safeAreaRect = safeGo.AddComponent<RectTransform>();
            ApplySafeArea(hud._safeAreaRect);
            hud._lastSafeArea = Screen.safeArea;

            // OutsideTapBlocker — inside SafeArea so sibling order works with SettingsPanel
            hud._outsideTapBlocker = CreateOutsideTapBlocker(safeGo.transform, hud);

            bool isMorning = ThemeConfig.CurrentMood == LevelMood.Morning;

            // TopBar
            hud._topBar = CreateBar(safeGo.transform, "TopBar", true, BarHeight);
            CreateCoinDisplay(hud, hud._topBar, isMorning);
            CreateLevelPill(hud, hud._topBar, isMorning);
            CreateSettingsGear(hud, hud._topBar, isMorning);

            // SettingsPanel
            CreateSettingsPanel(hud, safeGo.transform);

            // BottomBar
            hud._bottomBar = CreateBar(safeGo.transform, "BottomBar", false, BottomBarHeight);
            CreateBottomBarContent(hud, hud._bottomBar, isMorning);

            hud.LoadToggleStates();
            hud._coinPulseCoroutine = hud.StartCoroutine(hud.AnimateCoinPulse());

            return hud;
        }

        // ============================
        // STRUCTURAL HELPERS
        // ============================

        private static GameObject CreateOutsideTapBlocker(Transform parent, GameplayHUD hud)
        {
            var go = new GameObject("OutsideTapBlocker");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            hud._blockerImage = go.AddComponent<Image>();
            hud._blockerImage.color = new Color(0f, 0f, 0f, 0f);
            hud._blockerImage.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => hud.CollapseSettingsIfOpen());
            go.transform.SetAsFirstSibling();
            go.SetActive(false);
            return go;
        }

        private static void ApplySafeArea(RectTransform rect)
        {
            var sa = Screen.safeArea;
            var sw = Screen.width;
            var sh = Screen.height;
            if (sw <= 0 || sh <= 0)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                return;
            }
            rect.anchorMin = new Vector2(sa.x / sw, sa.y / sh);
            rect.anchorMax = new Vector2(sa.xMax / sw, sa.yMax / sh);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateBar(Transform parent, string name, bool isTop, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            if (isTop)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
            }
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        // ============================
        // §3.3 COIN DISPLAY
        // ============================

        private static void CreateCoinDisplay(GameplayHUD hud, RectTransform topBar, bool isMorning)
        {
            var go = new GameObject("CoinDisplay");
            go.transform.SetParent(topBar, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(BarMargin, 0f);

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 15f;
            hlg.padding = new RectOffset(18, 30, 12, 12);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Fill only — no border ring or glow (caused defect lines)
            AddFillBackground(go, new Color(0f, 0f, 0f, 0.4f), 60, 128);

            // CoinGlow (ignoreLayout, behind icon)
            var glowGo = new GameObject("CoinGlow");
            glowGo.transform.SetParent(go.transform, false);
            var glowRect = glowGo.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0f, 0.5f);
            glowRect.anchorMax = new Vector2(0f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.anchoredPosition = new Vector2(46f, 0f);
            glowRect.sizeDelta = new Vector2(90f, 90f);
            hud._coinGlowImage = glowGo.AddComponent<Image>();
            hud._coinGlowImage.sprite = UIShapeUtils.WhiteCircle(64);
            hud._coinGlowImage.color = new Color(1f, 0.82f, 0.15f, 0.15f);
            hud._coinGlowImage.raycastTarget = false;
            glowGo.AddComponent<LayoutElement>().ignoreLayout = true;

            // CoinIcon (layout child #1)
            var iconGo = new GameObject("CoinIcon");
            iconGo.transform.SetParent(go.transform, false);
            var iconLE = iconGo.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 57f;
            iconLE.preferredHeight = 57f;
            AddCircleBorderWithGradient(iconGo, 6f, CoinBorder);

            // "$" symbol inside icon
            var symGo = new GameObject("CoinSymbol");
            symGo.transform.SetParent(iconGo.transform, false);
            var symRect = symGo.AddComponent<RectTransform>();
            symRect.anchorMin = Vector2.zero;
            symRect.anchorMax = Vector2.one;
            symRect.offsetMin = Vector2.zero;
            symRect.offsetMax = Vector2.zero;
            var sym = symGo.AddComponent<TextMeshProUGUI>();
            sym.text = "$";
            sym.fontSize = 33f;
            sym.fontStyle = FontStyles.Bold;
            sym.alignment = TextAlignmentOptions.Center;
            sym.color = CoinSymbolColor;
            sym.font = ThemeConfig.GetFont();
            sym.raycastTarget = false;

            // CoinText (layout child #2)
            var textGo = new GameObject("CoinText");
            textGo.transform.SetParent(go.transform, false);
            var textLE = textGo.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1f;
            textLE.minWidth = 60f;

            hud._coinText = textGo.AddComponent<TextMeshProUGUI>();
            hud._coinText.text = "0";
            hud._coinText.fontSize = 51f;
            hud._coinText.enableAutoSizing = true;
            hud._coinText.fontSizeMin = 24f;
            hud._coinText.fontSizeMax = 51f;
            hud._coinText.fontStyle = FontStyles.Bold;
            hud._coinText.alignment = TextAlignmentOptions.MidlineLeft;
            hud._coinText.color = CoinAmountGold;
            hud._coinText.font = ThemeConfig.GetFont();
            hud._coinText.raycastTarget = false;
            // Fatten glyphs — Bold alone is too thin for this coin display
            hud._coinText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.5f);
        }

        // ============================
        // §3.4 LEVEL PILL
        // ============================

        private static void CreateLevelPill(GameplayHUD hud, RectTransform topBar, bool isMorning)
        {
            var go = new GameObject("LevelPill");
            go.transform.SetParent(topBar, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.52f, 0.5f);
            rect.anchorMax = new Vector2(0.52f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 2f;
            vlg.padding = new RectOffset(36, 36, 10, 10);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Color borderColor = isMorning
                ? new Color(1f, 1f, 1f, 0.1f)
                : new Color(1f, 1f, 1f, 0.08f);
            Color fillColor = isMorning
                ? new Color(0.235f, 0.157f, 0.039f, 0.35f)
                : new Color(0f, 0f, 0f, 0.45f);
            AddFillBackground(go, fillColor, 60, 128);

            // LevelText
            var lvGo = new GameObject("LevelText");
            lvGo.transform.SetParent(go.transform, false);
            lvGo.AddComponent<LayoutElement>().preferredHeight = 54f;

            hud._levelText = lvGo.AddComponent<TextMeshProUGUI>();
            hud._levelText.text = "Level 1";
            hud._levelText.fontSize = 54f;
            hud._levelText.fontStyle = FontStyles.Bold;
            hud._levelText.alignment = TextAlignmentOptions.Center;
            hud._levelText.color = new Color(1f, 0.961f, 0.902f, 0.95f);
            hud._levelText.font = ThemeConfig.GetFont();
            hud._levelText.raycastTarget = false;
            // Extra bold — fatten glyphs well beyond Bold
            hud._levelText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.5f);
            // Text shadow via TMPro Underlay (§3.4a)
            hud._levelText.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.3f));
            hud._levelText.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 3f);
            hud._levelText.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.4f);
            hud._levelText.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);

            // MoveText
            var mvGo = new GameObject("MoveText");
            mvGo.transform.SetParent(go.transform, false);
            mvGo.AddComponent<LayoutElement>().preferredHeight = 36f;

            hud._moveText = mvGo.AddComponent<TextMeshProUGUI>();
            hud._moveText.text = "";
            hud._moveText.fontSize = 36f;
            hud._moveText.alignment = TextAlignmentOptions.Center;
            hud._moveText.color = isMorning
                ? new Color(0.471f, 0.373f, 0.216f, 0.8f)
                : new Color(0.706f, 0.667f, 0.784f, 0.7f);
            hud._moveText.font = ThemeConfig.GetFont();
            hud._moveText.raycastTarget = false;
        }

        // ============================
        // §3.5 SETTINGS GEAR
        // ============================

        private static void CreateSettingsGear(GameplayHUD hud, RectTransform topBar, bool isMorning)
        {
            var go = new GameObject("SettingsGear");
            go.transform.SetParent(topBar, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-BarMargin, 0f);
            rect.sizeDelta = new Vector2(GearButtonSize, GearButtonSize);

            Color frameColor = isMorning
                ? new Color(0.28f, 0.22f, 0.12f, 0.65f)
                : new Color(0.18f, 0.12f, 0.35f, 0.65f);
            Color faceColor = isMorning
                ? new Color(0.412f, 0.345f, 0.196f, 0.65f)
                : new Color(0.294f, 0.216f, 0.529f, 0.65f);

            // Single baked sprite — frame + face in one image, no defect lines
            var bgImg = go.AddComponent<Image>();
            bgImg.sprite = UIShapeUtils.BeveledButton(128, 28, 8,
                frameColor, Lighten(faceColor, 0.12f), Darken(faceColor, 0.08f));
            bgImg.type = Image.Type.Sliced;
            bgImg.color = Color.white;

            hud._settingsGearButton = go.AddComponent<Button>();
            go.AddComponent<ButtonBounce>();
            hud._settingsGearButton.onClick.AddListener(() => hud.ToggleSettings());

            // Gear icon — PNG sprite
            var iconGo = new GameObject("GearIcon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(72f, 72f);
            var gearImg = iconGo.AddComponent<Image>();
            gearImg.sprite = Resources.Load<Sprite>("Icons/icon-gear");
            gearImg.color = new Color(1f, 1f, 1f, 0.85f);
            gearImg.preserveAspect = true;
            gearImg.raycastTarget = false;
        }

        // ============================
        // §3.6 SETTINGS PANEL
        // ============================

        private static void CreateSettingsPanel(GameplayHUD hud, Transform safeArea)
        {
            var go = new GameObject("SettingsPanel");
            go.transform.SetParent(safeArea, false);
            hud._settingsPanel = go.AddComponent<RectTransform>();
            hud._settingsPanel.anchorMin = new Vector2(1f, 1f);
            hud._settingsPanel.anchorMax = new Vector2(1f, 1f);
            hud._settingsPanel.pivot = new Vector2(1f, 1f);
            hud._settingsPanel.anchoredPosition = new Vector2(-BarMargin, -BarHeight);

            // CanvasGroup ensures panel buttons block raycasts over the blocker
            var cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = SettingsButtonSpacing;
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var csf = go.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Cache icon sprites
            hud._musicOnSprite = Resources.Load<Sprite>("Icons/icon-music-on");
            hud._musicOffSprite = Resources.Load<Sprite>("Icons/icon-music-off");
            hud._sfxOnSprite = Resources.Load<Sprite>("Icons/icon-sfx-on");
            hud._sfxOffSprite = Resources.Load<Sprite>("Icons/icon-sfx-off");
            var vibrationSprite = Resources.Load<Sprite>("Icons/icon-vibration");
            hud._restartSprite = Resources.Load<Sprite>("Icons/icon-restart");
            var restartSprite = hud._restartSprite;
            var exitSprite = Resources.Load<Sprite>("Icons/icon-exit");

            // Music
            hud._musicToggleButton = Create3DButton(go.transform, "MusicToggle",
                hud._musicOnSprite, GreenON, GreenON_Shadow,
                out hud._musicFaceImage, out hud._musicIconImage);
            hud._musicToggleButton.onClick.AddListener(() => hud.ToggleMusic());

            // SFX
            hud._sfxToggleButton = Create3DButton(go.transform, "SFXToggle",
                hud._sfxOnSprite, GreenON, GreenON_Shadow,
                out hud._sfxFaceImage, out hud._sfxIconImage);
            hud._sfxToggleButton.onClick.AddListener(() => hud.ToggleSFX());

            // Vibration
            hud._vibrationToggleButton = Create3DButton(go.transform, "VibrationToggle",
                vibrationSprite, GreenON, GreenON_Shadow,
                out hud._vibrationFaceImage, out hud._vibrationIconImage);
            hud._vibrationToggleButton.onClick.AddListener(() => hud.ToggleVibration());

            // Restart
            hud._restartButton = Create3DButton(go.transform, "RestartButton",
                restartSprite, GreenON, GreenON_Shadow,
                out _, out _);
            hud._restartButton.onClick.AddListener(() =>
            {
                hud.TryHaptic();
                hud.CollapseSettingsIfOpen();
                hud.OnRestartPressed?.Invoke();
            });

            // Exit
            hud._exitButton = Create3DButton(go.transform, "ExitButton",
                exitSprite, RedExit, RedExit_Shadow,
                out _, out _);
            hud._exitButton.onClick.AddListener(() =>
            {
                hud.TryHaptic();
                hud.CollapseSettingsIfOpen();
                hud.OnExitPressed?.Invoke();
            });

            go.SetActive(false);
        }

        private static Color Lighten(Color c, float amount)
        {
            return new Color(
                Mathf.Min(c.r + amount, 1f),
                Mathf.Min(c.g + amount, 1f),
                Mathf.Min(c.b + amount, 1f), c.a);
        }

        private static Color Darken(Color c, float amount)
        {
            return new Color(
                Mathf.Max(c.r - amount, 0f),
                Mathf.Max(c.g - amount, 0f),
                Mathf.Max(c.b - amount, 0f), c.a);
        }

        private static Button Create3DButton(Transform parent, string name,
            Sprite iconSprite, Color faceColor, Color frameColor,
            out Image faceImage, out Image iconImage)
        {
            // Container
            var cGo = new GameObject(name);
            cGo.transform.SetParent(parent, false);
            var cLE = cGo.AddComponent<LayoutElement>();
            cLE.preferredWidth = SettingsButtonSize;
            cLE.preferredHeight = SettingsButtonSize;

            // Single baked sprite — frame + face + gradient in ONE image, no defect lines
            Color faceTop = Lighten(faceColor, 0.12f);
            Color faceBot = Darken(faceColor, 0.08f);
            faceImage = cGo.AddComponent<Image>();
            faceImage.sprite = UIShapeUtils.BeveledButton(128, 28, 8, frameColor, faceTop, faceBot);
            faceImage.type = Image.Type.Sliced;
            faceImage.color = Color.white;
            var btn = cGo.AddComponent<Button>();
            cGo.AddComponent<ButtonBounce>();

            // Icon — sprite Image, centered 60×60
            var iGo = new GameObject("Icon");
            iGo.transform.SetParent(cGo.transform, false);
            var iRect = iGo.AddComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0.5f, 0.5f);
            iRect.anchorMax = new Vector2(0.5f, 0.5f);
            iRect.sizeDelta = new Vector2(60f, 60f);
            iconImage = iGo.AddComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            return btn;
        }

        // ============================
        // §3.7 BOTTOM BAR
        // ============================

        private static void CreateBottomBarContent(GameplayHUD hud, RectTransform bottomBar, bool isMorning)
        {
            // ActionPill
            var pillGo = new GameObject("ActionPill");
            pillGo.transform.SetParent(bottomBar, false);
            var pillRect = pillGo.AddComponent<RectTransform>();
            pillRect.anchorMin = new Vector2(0.5f, 0f);
            pillRect.anchorMax = new Vector2(0.5f, 0f);
            pillRect.pivot = new Vector2(0.5f, 0f);
            pillRect.anchoredPosition = new Vector2(0f, 84f);

            var hlg = pillGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 15f;
            hlg.padding = new RectOffset(14, 14, 12, 12);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var csf = pillGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Color actionFill = isMorning
                ? new Color(0.235f, 0.157f, 0.039f, 0.4f) : new Color(0f, 0f, 0f, 0.5f);
            AddFillBackground(pillGo, actionFill, 50, 128);

            // UndoButton — gradient mid: rgba(80,65,120,0.75)
            CreateActionButton(hud, pillGo.transform, "UndoButton", 220f, 96f,
                new Color(0.314f, 0.255f, 0.471f, 0.75f),
                new Color(0.549f, 0.392f, 0.863f, 0.15f),
                true);

            // ExtraBottleButton
            CreateActionButton(hud, pillGo.transform, "ExtraBottleButton", 260f, 96f,
                new Color(0.137f, 0.431f, 0.216f, 0.75f),
                new Color(0.314f, 0.784f, 0.392f, 0.15f),
                false);
        }

        private static void CreateActionButton(GameplayHUD hud, Transform parent,
            string name, float prefW, float prefH, Color fillColor, Color glowColor, bool isUndo)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = prefW;
            le.preferredHeight = prefH;

            // Fill background — store reference for gray disabled state
            var fillImg = go.AddComponent<Image>();
            fillImg.sprite = UIShapeUtils.WhiteRoundedRect(40, 96);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = fillColor;

            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            go.AddComponent<ButtonBounce>();
            var cg = go.AddComponent<CanvasGroup>();

            if (isUndo)
            {
                hud._undoButton = btn;
                hud._undoCanvasGroup = cg;
                hud._undoFillImage = fillImg;
                hud._undoFillColor = fillColor;
                btn.onClick.AddListener(() => { hud.TryHaptic(); hud.OnUndoPressed?.Invoke(); });
            }
            else
            {
                hud._extraBottleButton = btn;
                hud._extraBottleCanvasGroup = cg;
                hud._extraBottleFillImage = fillImg;
                hud._extraBottleFillColor = fillColor;
                btn.onClick.AddListener(() => { hud.TryHaptic(); hud.OnExtraBottlePressed?.Invoke(); });
            }

            // Centered content via HorizontalLayoutGroup
            var content = new GameObject("Content");
            content.transform.SetParent(go.transform, false);
            var cRect = content.AddComponent<RectTransform>();
            cRect.anchorMin = Vector2.zero;
            cRect.anchorMax = Vector2.one;
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;
            var cHlg = content.AddComponent<HorizontalLayoutGroup>();
            cHlg.childAlignment = TextAnchor.MiddleCenter;
            cHlg.spacing = 8f;
            cHlg.padding = new RectOffset(12, 12, 8, 8);
            cHlg.childForceExpandWidth = false;
            cHlg.childForceExpandHeight = false;
            cHlg.childControlWidth = true;
            cHlg.childControlHeight = true;

            if (isUndo)
            {
                // Undo icon — reuse cached restart sprite
                var undoSprite = hud._restartSprite;
                var icoGo = new GameObject("UndoIcon");
                icoGo.transform.SetParent(content.transform, false);
                var icoLE = icoGo.AddComponent<LayoutElement>();
                icoLE.preferredWidth = 36f;
                icoLE.preferredHeight = 36f;
                var icoImg = icoGo.AddComponent<Image>();
                icoImg.sprite = undoSprite;
                icoImg.color = Color.white;
                icoImg.preserveAspect = true;
                icoImg.raycastTarget = false;

                // MiniCoin
                var coinGo = new GameObject("MiniCoin");
                coinGo.transform.SetParent(content.transform, false);
                var coinLE = coinGo.AddComponent<LayoutElement>();
                coinLE.preferredWidth = 30f;
                coinLE.preferredHeight = 30f;
                var coinImg = coinGo.AddComponent<Image>();
                coinImg.sprite = UIShapeUtils.CoinGradient(128);
                coinImg.color = Color.white;
                coinImg.preserveAspect = true;
                coinImg.raycastTarget = false;

                // Cost
                hud._undoCostText = CreateLayoutTMP(content.transform, "100", 28f, CostGold);
            }
            else
            {
                // Bottle icon
                var bottleSprite = UIShapeUtils.WhiteRoundedRect(6, 32);
                var bGo = new GameObject("BottleIcon");
                bGo.transform.SetParent(content.transform, false);
                var bLE = bGo.AddComponent<LayoutElement>();
                bLE.preferredWidth = 24f;
                bLE.preferredHeight = 50f;
                var bImg = bGo.AddComponent<Image>();
                bImg.sprite = bottleSprite;
                bImg.type = Image.Type.Sliced;
                bImg.color = new Color(1f, 1f, 1f, 0.7f);
                bImg.raycastTarget = false;

                // Plus
                CreateLayoutTMP(content.transform, "+", 36f, Color.white);

                // MiniCoin
                var coinGo = new GameObject("MiniCoin");
                coinGo.transform.SetParent(content.transform, false);
                var coinLE = coinGo.AddComponent<LayoutElement>();
                coinLE.preferredWidth = 30f;
                coinLE.preferredHeight = 30f;
                var coinImg = coinGo.AddComponent<Image>();
                coinImg.sprite = UIShapeUtils.CoinGradient(128);
                coinImg.color = Color.white;
                coinImg.preserveAspect = true;
                coinImg.raycastTarget = false;

                // Cost
                hud._extraBottleCostText = CreateLayoutTMP(content.transform, "500", 28f, CostGold);
            }
        }

        private static TextMeshProUGUI CreateLayoutTMP(Transform parent, string text,
            float fontSize, Color color)
        {
            var go = new GameObject("T");
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = color;
            tmp.font = ThemeConfig.GetFont();
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = fontSize;
            return tmp;
        }

        // ============================
        // VISUAL HELPERS
        // ============================

        private static void AddFillBackground(GameObject container, Color fillColor,
            int spriteRadius, int spriteSize)
        {
            var fGo = new GameObject("Fill");
            fGo.transform.SetParent(container.transform, false);
            fGo.transform.SetAsFirstSibling();
            var fRect = fGo.AddComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;
            var fImg = fGo.AddComponent<Image>();
            fImg.sprite = UIShapeUtils.WhiteRoundedRect(spriteRadius, spriteSize);
            fImg.type = Image.Type.Sliced;
            fImg.color = fillColor;
            fImg.raycastTarget = false;
            fGo.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        /// <summary>
        /// Circle border with baked radial gradient fill (§6.1 coin gradient).
        /// </summary>
        private static void AddCircleBorderWithGradient(GameObject container, float borderWidth,
            Color borderColor)
        {
            var bGo = new GameObject("BorderRing");
            bGo.transform.SetParent(container.transform, false);
            bGo.transform.SetAsFirstSibling();
            var bRect = bGo.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = new Vector2(-borderWidth, -borderWidth);
            bRect.offsetMax = new Vector2(borderWidth, borderWidth);
            var bImg = bGo.AddComponent<Image>();
            bImg.sprite = UIShapeUtils.WhiteCircle(64);
            bImg.color = borderColor;
            bImg.raycastTarget = false;

            var fGo = new GameObject("Fill");
            fGo.transform.SetParent(container.transform, false);
            fGo.transform.SetSiblingIndex(1);
            var fRect = fGo.AddComponent<RectTransform>();
            fRect.anchorMin = Vector2.zero;
            fRect.anchorMax = Vector2.one;
            fRect.offsetMin = Vector2.zero;
            fRect.offsetMax = Vector2.zero;
            var fImg = fGo.AddComponent<Image>();
            fImg.sprite = UIShapeUtils.CoinGradient(128);
            fImg.color = Color.white; // gradient is baked, don't tint
            fImg.raycastTarget = false;
        }

        // ============================
        // ANIMATIONS
        // ============================

        private IEnumerator AnimateCoinPulse()
        {
            var wait = new WaitForSeconds(0.033f);
            float elapsed = 0f;
            while (true)
            {
                if (_coinGlowImage != null)
                {
                    elapsed += 0.033f;
                    float t = Mathf.PingPong(elapsed / 1.25f, 1f);
                    float alpha = Mathf.SmoothStep(0.15f, 0.55f, t);
                    var gold = ThemeConfig.GetColor(ThemeColorType.StarGold);
                    _coinGlowImage.color = new Color(gold.r, gold.g, gold.b, alpha);
                }
                yield return wait;
            }
        }

        private IEnumerator AnimateSettingsExpand()
        {
            var buttons = new Transform[]
            {
                _musicToggleButton.transform,
                _sfxToggleButton.transform,
                _vibrationToggleButton.transform,
                _restartButton.transform,
                _exitButton.transform
            };

            foreach (var b in buttons)
                b.localScale = Vector3.zero;

            for (int i = 0; i < buttons.Length; i++)
            {
                StartCoroutine(ScaleButton(buttons[i], Vector3.zero, Vector3.one, 0.2f));
                yield return new WaitForSeconds(0.05f);
            }
            _settingsAnimCoroutine = null;
        }

        private IEnumerator AnimateSettingsCollapse()
        {
            var buttons = new Transform[]
            {
                _exitButton.transform,
                _restartButton.transform,
                _vibrationToggleButton.transform,
                _sfxToggleButton.transform,
                _musicToggleButton.transform
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                StartCoroutine(ScaleButton(buttons[i], Vector3.one, Vector3.zero, 0.15f));
                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.15f);
            _settingsPanel.gameObject.SetActive(false);
            // Fade blocker out then hide
            if (_blockerFadeCoroutine != null) StopCoroutine(_blockerFadeCoroutine);
            _blockerFadeCoroutine = StartCoroutine(FadeBlockerOut());
            _settingsAnimCoroutine = null;
        }

        private IEnumerator FadeBlocker(float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration));
                if (_blockerImage != null)
                    _blockerImage.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }
            if (_blockerImage != null)
                _blockerImage.color = new Color(0f, 0f, 0f, toAlpha);
            _blockerFadeCoroutine = null;
        }

        private IEnumerator FadeBlockerOut()
        {
            float startAlpha = _blockerImage != null ? _blockerImage.color.a : 0.4f;
            float elapsed = 0f;
            const float duration = 0.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / duration));
                if (_blockerImage != null)
                    _blockerImage.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }
            if (_blockerImage != null)
                _blockerImage.color = new Color(0f, 0f, 0f, 0f);
            if (_outsideTapBlocker != null) _outsideTapBlocker.SetActive(false);
            _blockerFadeCoroutine = null;
        }

        private static IEnumerator ScaleButton(Transform btn, Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float eased = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                btn.localScale = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }
            btn.localScale = to;
        }

        // ============================
        // HAPTIC FEEDBACK
        // ============================

        private void TryHaptic()
        {
            if (!_vibrationEnabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var vibrator = new AndroidJavaClass("android.os.Vibrator"))
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var vib = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    // Android 26+ (API level O): use VibrationEffect for short tap
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        var effect = effectClass.CallStatic<AndroidJavaObject>(
                            "createOneShot", 20L, 80); // 20ms, medium amplitude
                        vib.Call("vibrate", effect);
                    }
                }
            }
            catch { Handheld.Vibrate(); }
#endif
        }

        // ============================
        // SETTINGS TOGGLE LOGIC
        // ============================

        private void ToggleSettings()
        {
            if (_settingsAnimCoroutine != null)
                StopCoroutine(_settingsAnimCoroutine);

            TryHaptic();
            _isSettingsOpen = !_isSettingsOpen;

            if (_isSettingsOpen)
            {
                _settingsPanel.gameObject.SetActive(true);
                _outsideTapBlocker.SetActive(true);
                // Sibling order: blocker covers everything, then settings panel + topbar on top
                _outsideTapBlocker.transform.SetAsLastSibling();
                _settingsPanel.transform.SetAsLastSibling();
                _topBar.transform.SetAsLastSibling();
                // Fade blocker to dim
                if (_blockerFadeCoroutine != null) StopCoroutine(_blockerFadeCoroutine);
                _blockerFadeCoroutine = StartCoroutine(FadeBlocker(0f, 0.4f, 0.15f));
                _settingsAnimCoroutine = StartCoroutine(AnimateSettingsExpand());
            }
            else
            {
                _settingsAnimCoroutine = StartCoroutine(AnimateSettingsCollapse());
            }

            OnSettingsPressed?.Invoke();
        }

        private void ToggleMusic()
        {
            TryHaptic();
            _musicEnabled = !_musicEnabled;
            if (Services.TryGet<IProgressionManager>(out var prog))
                prog.MusicEnabled = _musicEnabled;
            if (Services.TryGet<IAudioManager>(out var audio))
                audio.SetMusicEnabled(_musicEnabled);
            UpdateToggleVisuals();
        }

        private void ToggleSFX()
        {
            TryHaptic();
            _sfxEnabled = !_sfxEnabled;
            if (Services.TryGet<IProgressionManager>(out var prog))
                prog.SoundEnabled = _sfxEnabled;
            if (Services.TryGet<IAudioManager>(out var audio))
                audio.SetSoundEnabled(_sfxEnabled);
            UpdateToggleVisuals();
        }

        private void ToggleVibration()
        {
            TryHaptic();
            _vibrationEnabled = !_vibrationEnabled;
            PlayerPrefs.SetInt("VibrationEnabled", _vibrationEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdateToggleVisuals();
        }

        private void LoadToggleStates()
        {
            if (Services.TryGet<IProgressionManager>(out var prog))
            {
                _musicEnabled = prog.MusicEnabled;
                _sfxEnabled = prog.SoundEnabled;
            }
            _vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
            UpdateToggleVisuals();
        }

        private void UpdateToggleVisuals()
        {
            UpdateToggle(_musicFaceImage, _musicIconImage, _musicEnabled,
                _musicOnSprite, _musicOffSprite);
            UpdateToggle(_sfxFaceImage, _sfxIconImage, _sfxEnabled,
                _sfxOnSprite, _sfxOffSprite);
            UpdateToggle(_vibrationFaceImage, _vibrationIconImage, _vibrationEnabled,
                null, null); // vibration uses same sprite, just dims alpha
        }

        private void UpdateToggle(Image face, Image iconImg, bool isOn,
            Sprite onSprite, Sprite offSprite)
        {
            // Swap baked button sprite for ON/OFF state
            if (face != null)
            {
                Color fc = isOn ? GreenON : GrayOFF;
                Color fr = isOn ? GreenON_Shadow : GrayOFF_Shadow;
                face.sprite = UIShapeUtils.BeveledButton(128, 28, 8,
                    fr, Lighten(fc, 0.12f), Darken(fc, 0.08f));
            }
            // Swap icon sprite and alpha
            if (iconImg != null)
            {
                if (onSprite != null && offSprite != null)
                    iconImg.sprite = isOn ? onSprite : offSprite;
                iconImg.color = isOn ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            }
        }

        // ============================
        // LIFECYCLE
        // ============================

        void LateUpdate()
        {
            if (Screen.safeArea != _lastSafeArea)
            {
                _lastSafeArea = Screen.safeArea;
                ApplySafeArea(_safeAreaRect);
            }
        }

        void OnDestroy()
        {
            OnUndoPressed = null;
            OnRestartPressed = null;
            OnExtraBottlePressed = null;
            OnExitPressed = null;
            OnSettingsPressed = null;
            if (_settingsAnimCoroutine != null) StopCoroutine(_settingsAnimCoroutine);
            if (_coinPulseCoroutine != null) StopCoroutine(_coinPulseCoroutine);
        }
    }
}
