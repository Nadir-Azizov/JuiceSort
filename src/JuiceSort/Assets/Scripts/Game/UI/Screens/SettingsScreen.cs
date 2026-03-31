using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.Audio;
using JuiceSort.Game.Progression;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    public class SettingsScreen : MonoBehaviour
    {
        #region Fields

        private bool _sfxEnabled = true;
        private bool _hapticEnabled = true;
        private bool _musicEnabled = true;
        private bool _notificationsOn = true;

        private GameObject _soundStrike;
        private GameObject _hapticStrike;
        private GameObject _musicStrike;

        private RectTransform _knobRect;
        private Image _trackImage;
        private Image _trackBorderImage;
        private Image _knobImage;
        private TextMeshProUGUI _toggleLabel;
        private Coroutine _knobAnim;

        #endregion

        #region Lifecycle & State

        private void OnEnable()
        {
            if (_soundStrike != null) Refresh();
        }

        public void Refresh()
        {
            LoadToggleStates();
            UpdateAllVisuals();
        }

        private void LoadToggleStates()
        {
            if (Services.TryGet<IProgressionManager>(out var prog))
            {
                _sfxEnabled = prog.SoundEnabled;
                _musicEnabled = prog.MusicEnabled;
            }
            _hapticEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
            _notificationsOn = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
        }

        private void UpdateAllVisuals()
        {
            UpdateStrike(_soundStrike, _sfxEnabled);
            UpdateStrike(_hapticStrike, _hapticEnabled);
            UpdateStrike(_musicStrike, _musicEnabled);
            UpdateNotifVisuals(false);
        }

        private static void UpdateStrike(GameObject strike, bool isOn)
        {
            if (strike != null) strike.SetActive(!isOn);
        }

        private void UpdateNotifVisuals(bool animate)
        {
            if (_trackImage == null) return;
            bool on = _notificationsOn;
            _trackImage.color = Col(on ? "#209830" : "#4a4a4a");
            _trackBorderImage.color = Col(on ? "#166622" : "#333333");
            _knobImage.color = Col(on ? "#d4a832" : "#aaaaaa");
            _toggleLabel.text = on ? "ON" : "OFF";
            _toggleLabel.alignment = on ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight;
            float targetX = on ? KnobTravel : -KnobTravel;
            if (animate)
            {
                if (_knobAnim != null) StopCoroutine(_knobAnim);
                _knobAnim = StartCoroutine(AnimateKnob(targetX, 0.3f));
            }
            else
                _knobRect.anchoredPosition = new Vector2(targetX, 0f);
        }

        private IEnumerator AnimateKnob(float targetX, float duration)
        {
            float startX = _knobRect.anchoredPosition.x;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                _knobRect.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, ease), 0f);
                yield return null;
            }
            _knobRect.anchoredPosition = new Vector2(targetX, 0f);
            _knobAnim = null;
        }

        #endregion

        #region Toggle Handlers

        private void OnToggleSFX()
        {
            HapticUtils.TryVibrate();
            _sfxEnabled = !_sfxEnabled;
            if (Services.TryGet<IProgressionManager>(out var prog)) prog.SoundEnabled = _sfxEnabled;
            if (Services.TryGet<IAudioManager>(out var audio)) audio.SetSoundEnabled(_sfxEnabled);
            UpdateStrike(_soundStrike, _sfxEnabled);
        }

        private void OnToggleHaptic()
        {
            HapticUtils.TryVibrate();
            _hapticEnabled = !_hapticEnabled;
            PlayerPrefs.SetInt("VibrationEnabled", _hapticEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdateStrike(_hapticStrike, _hapticEnabled);
        }

        private void OnToggleMusic()
        {
            HapticUtils.TryVibrate();
            _musicEnabled = !_musicEnabled;
            if (Services.TryGet<IProgressionManager>(out var prog)) prog.MusicEnabled = _musicEnabled;
            if (Services.TryGet<IAudioManager>(out var audio)) audio.SetMusicEnabled(_musicEnabled);
            UpdateStrike(_musicStrike, _musicEnabled);
        }

        private void OnNotifToggle()
        {
            HapticUtils.TryVibrate();
            _notificationsOn = !_notificationsOn;
            PlayerPrefs.SetInt("NotificationsEnabled", _notificationsOn ? 1 : 0);
            PlayerPrefs.Save();
            UpdateNotifVisuals(true);
        }

        private void OnClose()
        {
            HapticUtils.TryVibrate();
            if (Services.TryGet<ScreenManager>(out var sm))
                sm.TransitionTo(GameFlowState.MainMenu);
        }

        #endregion

        #region Constants

        private const float HeaderHeightBase = 184f;
        private const float GoldSepHeight = 11f;
        private const float CloseButtonSize = 146f;
        private const float CloseButtonMargin = 38f;
        // Close: gold=146, red ring=130 (inset 8), face=114 (inset 8 from ring)
        private const float ContentPaddingTop = 54f;
        private const float ContentPaddingLR = 43f;
        private const float ContentPaddingBottom = 108f;
        private const float CardGap = 49f;
        private const float CardPadding = 54f;
        private const float CardBorderWidth = 8f;
        private const float ToggleWidth = 254f;
        private const float ToggleHeight = 119f;
        private const float KnobSize = 92f;
        private const float KnobTravel = 73f;
        private const float AudioButtonSize = 200f;
        private const float AudioIconSize = 65f;
        private const float StrikeWidth = 130f;
        private const float StrikeHeight = 16f;
        private const float AudioItemSpacing = 27f;
        private const float GoogleLogoSize = 97f;
        private const float GoogleFacePaddingTB = 35f;
        private const float GoogleFacePaddingLR = 49f;
        private const float GoogleContentGap = 32f;
        private const float SupportIconSize = 76f;
        private const float SupportFacePaddingTB = 35f;
        private const float SupportFacePaddingLR = 59f;
        private const float SupportContentGap = 32f;
        private const float LegalGap = 54f;
        private const float LegalPaddingLR = 22f;
        private const float LegalFacePaddingTB = 27f;
        private const float LegalFacePaddingLR = 43f;
        private const int CardRadius = 24;
        // Toggle uses WhitePill sprite directly
        private const int AudioGoldRadius = 28;
        private const int AudioBorderRadius = 25;
        private const int AudioFaceRadius = 22;
        private const int GoogleGoldRadius = 64;  // full cylinder/pill
        private const int GoogleBorderRadius = 60;
        private const int GoogleFaceRadius = 56;
        private const int SupportGoldRadius = 32;
        private const int SupportBorderRadius = 29;
        private const int SupportFaceRadius = 26;
        private const int LegalOuterRadius = 24;
        private const int LegalBorderRadius = 21;
        private const int LegalFaceRadius = 18;

        #endregion

        #region Create — Main Screen Builder

        public static GameObject Create()
        {
            var go = new GameObject("SettingsScreen");
            var cv = go.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            cv.sortingOrder = 10;
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080f, 1920f);
            sc.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<CanvasGroup>();
            var screen = go.AddComponent<SettingsScreen>();

            // Header uses safe area top inset so title/close sit below the notch.
            // Convert Screen.safeArea (physical pixels) to reference-resolution units
            // using the same logic CanvasScaler applies (matchWidthOrHeight = 0.5).
            float refW = 1080f, refH = 1920f;
            float logW = Mathf.Log(Screen.width / refW, 2f);
            float logH = Mathf.Log(Screen.height / refH, 2f);
            float canvasScale = Mathf.Pow(2f, Mathf.Lerp(logW, logH, 0.5f));
            float notchPad = Mathf.Max((Screen.height - Screen.safeArea.yMax) / canvasScale, 0f);
            // Guarantee a minimum so the title clears status bar even without a notch
            notchPad = Mathf.Max(notchPad, 60f);
            float HeaderHeight = HeaderHeightBase + notchPad;

            // Background
            var bg = R(go, "Background");
            bg.anchorMin = V(0, 0); bg.anchorMax = V(1, 1);
            Img(bg, null, Col("#2d2280")).raycastTarget = true;

            // Header — taller to include notch/status bar padding
            var header = R(go, "Header");
            header.anchorMin = V(0, 1); header.anchorMax = V(1, 1);
            header.pivot = V(0.5f, 1); header.sizeDelta = V(0, HeaderHeight);

            var headerBg = R(header.gameObject, "HeaderBg");
            headerBg.anchorMin = V(0, 0); headerBg.anchorMax = V(1, 1);
            Img(headerBg, null, Col("#6b4cb8")).raycastTarget = false;

            var headerHl = R(header.gameObject, "HeaderHighlight");
            headerHl.anchorMin = V(0, 1); headerHl.anchorMax = V(1, 1);
            headerHl.pivot = V(0.5f, 1); headerHl.sizeDelta = V(0, 5);
            Img(headerHl, null, ColA(1, 1, 1, 0.15f)).raycastTarget = false;

            // Header bottom shadow
            var headerSh = R(header.gameObject, "HeaderShadow");
            headerSh.anchorMin = V(0, 0); headerSh.anchorMax = V(1, 0);
            headerSh.pivot = V(0.5f, 1); headerSh.sizeDelta = V(0, 43);
            var headerShImg = headerSh.gameObject.AddComponent<Image>();
            headerShImg.sprite = ThemeConfig.CreateGradientSprite(ColA(0, 0, 0, 0.5f), Color.clear);
            headerShImg.raycastTarget = false;

            var sep = R(header.gameObject, "GoldSeparator");
            sep.anchorMin = V(0, 0); sep.anchorMax = V(1, 0);
            sep.pivot = V(0.5f, 0); sep.sizeDelta = V(0, GoldSepHeight);
            Img(sep, null, Col("#7a5510")).raycastTarget = false;

            // Content area inside header — below notch, above bottom edge
            var headerContent = R(header.gameObject, "HeaderContent");
            headerContent.anchorMin = V(0, 0); headerContent.anchorMax = V(1, 1);
            headerContent.offsetMin = V(0, 0);
            headerContent.offsetMax = V(0, -notchPad);

            // Title — inside safe content area
            var title = Txt(headerContent.gameObject, "Settings", 103f, Color.white, 0.6f, 0.25f);
            title.characterSpacing = -4f;

            // Close button — inside safe content area
            var closeRoot = R(headerContent.gameObject, "CloseButton");
            closeRoot.anchorMin = V(1, 0.5f); closeRoot.anchorMax = V(1, 0.5f);
            closeRoot.pivot = V(1, 0.5f);
            closeRoot.anchoredPosition = V(-CloseButtonMargin, 0);
            closeRoot.sizeDelta = V(CloseButtonSize, CloseButtonSize);

            // Close: gold ring (full size, Simple — not Sliced for circles)
            var closeGold = R(closeRoot.gameObject, "GoldRing");
            closeGold.anchorMin = V(0, 0); closeGold.anchorMax = V(1, 1);
            Img(closeGold, UIShapeUtils.WhiteCircle(64), Col("#e0c040")).raycastTarget = false;

            // Close: red ring (inset 8px from gold = 130×130)
            var closeRed = R(closeRoot.gameObject, "RedRing");
            closeRed.anchorMin = V(0.5f, 0.5f); closeRed.anchorMax = V(0.5f, 0.5f);
            closeRed.sizeDelta = V(130, 130);
            Img(closeRed, UIShapeUtils.WhiteCircle(64), Col("#cc2020")).raycastTarget = false;

            // Close: red face (inset 8px from ring = 114×114 — CSS: padding 3px)
            var closeFace = R(closeRoot.gameObject, "RedFace");
            closeFace.anchorMin = V(0.5f, 0.5f); closeFace.anchorMax = V(0.5f, 0.5f);
            closeFace.sizeDelta = V(114, 114);
            var closeFaceImg = closeFace.gameObject.AddComponent<Image>();
            closeFaceImg.sprite = UIShapeUtils.WhiteCircle(64);
            closeFaceImg.color = Col("#d42828");
            closeFaceImg.raycastTarget = false;
            closeFace.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            // Close: gloss — full face height, soft gradient fade (circle → transparent)
            var closeGloss = R(closeFace.gameObject, "Gloss");
            closeGloss.anchorMin = V(0.1f, 0f); closeGloss.anchorMax = V(0.9f, 1f);
            closeGloss.offsetMin = V(0, 0); closeGloss.offsetMax = V(0, 0);
            var closeGlossImg = closeGloss.gameObject.AddComponent<Image>();
            closeGlossImg.sprite = ThemeConfig.CreateGradientSprite(ColA(1, 1, 1, 0.2f), Color.clear);
            closeGlossImg.raycastTarget = false;

            // Close: X label — manually created, small rect centered on face
            var xGo = new GameObject("XLabel");
            xGo.transform.SetParent(closeFace, false);
            var xRect = xGo.AddComponent<RectTransform>();
            xRect.anchorMin = V(0.2f, 0.2f); xRect.anchorMax = V(0.8f, 0.8f);
            xRect.offsetMin = V(0, 0); xRect.offsetMax = V(0, 0);
            var xLabel = xGo.AddComponent<TextMeshProUGUI>();
            xLabel.text = "X";
            xLabel.fontSize = 46f;
            xLabel.fontStyle = FontStyles.Bold;
            xLabel.alignment = TextAlignmentOptions.Center;
            xLabel.color = Color.white;
            xLabel.font = ThemeConfig.GetFont();
            xLabel.overflowMode = TextOverflowModes.Overflow;
            xLabel.raycastTarget = false;
            var xMat = new Material(xLabel.fontSharedMaterial);
            xMat.SetFloat("_FaceDilate", 0.8f);
            xMat.EnableKeyword("OUTLINE_ON");
            xMat.SetFloat("_OutlineWidth", 0.4f);
            xMat.SetColor("_OutlineColor", Color.black);
            xLabel.fontMaterial = xMat;

            // Close: button
            Img(closeRoot, null, Color.clear);
            var closeBtn = closeRoot.gameObject.AddComponent<Button>();
            closeRoot.gameObject.AddComponent<ButtonBounce>();
            closeBtn.onClick.AddListener(() => screen.OnClose());

            // Scroll area
            var scrollGo = R(go, "ScrollArea");
            scrollGo.anchorMin = V(0, 0); scrollGo.anchorMax = V(1, 1);
            scrollGo.offsetMin = V(0, 0); scrollGo.offsetMax = V(0, -HeaderHeight);

            var viewport = R(scrollGo.gameObject, "Viewport");
            viewport.anchorMin = V(0, 0); viewport.anchorMax = V(1, 1);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = R(viewport.gameObject, "Content");
            content.anchorMin = V(0, 1); content.anchorMax = V(1, 1);
            content.pivot = V(0.5f, 1);

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = CardGap;
            vlg.padding = new RectOffset((int)ContentPaddingLR, (int)ContentPaddingLR, (int)ContentPaddingTop, (int)ContentPaddingBottom);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childAlignment = TextAnchor.UpperCenter;

            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            // ===== NOTIFICATION CARD =====
            var notifCard = CreateCard(content.gameObject, "NotificationCard");
            notifCard.AddComponent<LayoutElement>().preferredHeight = CardPadding * 2 + ToggleHeight + 20f;

            var notifRow = R(notifCard, "NotifRow");
            notifRow.anchorMin = V(0, 0); notifRow.anchorMax = V(1, 1);
            notifRow.offsetMin = V(CardPadding, CardPadding);
            notifRow.offsetMax = V(-CardPadding, -CardPadding);

            // Bell icon from Resources
            var bellIcon = R(notifRow.gameObject, "BellIcon");
            bellIcon.anchorMin = V(0, 0.5f); bellIcon.anchorMax = V(0, 0.5f);
            bellIcon.pivot = V(0, 0.5f); bellIcon.sizeDelta = V(59, 59);
            var bellImg = bellIcon.gameObject.AddComponent<Image>();
            bellImg.sprite = LoadWhiteIcon("Icons/bell-icon-white");
            bellImg.preserveAspect = true;
            bellImg.color = Color.white;
            bellImg.raycastTarget = false;

            // Notifications text — bold + black border
            var notifTxt = Txt(notifRow.gameObject, "Notifications:", 59f, Color.white, 0.5f, 0.35f);
            notifTxt.characterSpacing = -5f;
            notifTxt.alignment = TextAlignmentOptions.MidlineLeft;
            notifTxt.rectTransform.anchorMin = V(0, 0);
            notifTxt.rectTransform.anchorMax = V(1, 1);
            notifTxt.rectTransform.offsetMin = V(80, 0);
            notifTxt.rectTransform.offsetMax = V(-ToggleWidth - 20, 0);

            // Toggle switch
            var toggleRoot = R(notifRow.gameObject, "ToggleSwitch");
            toggleRoot.anchorMin = V(1, 0.5f); toggleRoot.anchorMax = V(1, 0.5f);
            toggleRoot.pivot = V(1, 0.5f);
            toggleRoot.sizeDelta = V(ToggleWidth, ToggleHeight);

            var trackBorder = R(toggleRoot.gameObject, "TrackBorder");
            trackBorder.anchorMin = V(0, 0); trackBorder.anchorMax = V(1, 1);
            trackBorder.offsetMin = V(-CardBorderWidth, -CardBorderWidth);
            trackBorder.offsetMax = V(CardBorderWidth, CardBorderWidth);
            screen._trackBorderImage = ImgSliced(trackBorder, UIShapeUtils.WhitePill(256, 128), Color.white);
            screen._trackBorderImage.raycastTarget = false;

            var trackFill = R(toggleRoot.gameObject, "Track");
            trackFill.anchorMin = V(0, 0); trackFill.anchorMax = V(1, 1);
            screen._trackImage = ImgSliced(trackFill, UIShapeUtils.WhitePill(256, 128), Color.white);
            screen._trackImage.raycastTarget = false;

            // Toggle label — bold + moderate border
            screen._toggleLabel = Txt(toggleRoot.gameObject, "ON", 38f, Color.white, 0.6f, 0.5f);
            screen._toggleLabel.alignment = TextAlignmentOptions.MidlineLeft;
            screen._toggleLabel.rectTransform.offsetMin = V(32f, 0f);
            screen._toggleLabel.rectTransform.offsetMax = V(-32f, 0f);

            // Knob
            var knob = R(toggleRoot.gameObject, "Knob");
            knob.anchorMin = V(0.5f, 0.5f); knob.anchorMax = V(0.5f, 0.5f);
            knob.sizeDelta = V(KnobSize, KnobSize);
            screen._knobRect = knob;
            screen._knobImage = ImgSliced(knob, UIShapeUtils.WhiteCircle(64), Color.white);
            screen._knobImage.raycastTarget = false;

            // Knob border ring (3px bevel)
            var knobBorder = R(knob.gameObject, "KnobBorder");
            knobBorder.anchorMin = V(0, 0); knobBorder.anchorMax = V(1, 1);
            knobBorder.offsetMin = V(-3, -3); knobBorder.offsetMax = V(3, 3);
            ImgSliced(knobBorder, UIShapeUtils.WhiteCircle(64), ColA(1, 1, 1, 0.5f)).raycastTarget = false;
            knobBorder.SetAsFirstSibling();

            // Toggle button
            Img(toggleRoot, null, Color.clear);
            var toggleBtn = toggleRoot.gameObject.AddComponent<Button>();
            toggleBtn.onClick.AddListener(() => screen.OnNotifToggle());
            toggleRoot.gameObject.AddComponent<ButtonBounce>();

            // ===== AUDIO CARD =====
            var audioCard = CreateCard(content.gameObject, "AudioCard");
            audioCard.AddComponent<LayoutElement>().preferredHeight = CardPadding * 2 + AudioButtonSize + 54 + 27;

            var audioGrid = R(audioCard, "AudioGrid");
            audioGrid.anchorMin = V(0, 0); audioGrid.anchorMax = V(1, 1);
            audioGrid.offsetMin = V(CardPadding, CardPadding);
            audioGrid.offsetMax = V(-CardPadding, -CardPadding);
            var audioHlg = audioGrid.gameObject.AddComponent<HorizontalLayoutGroup>();
            audioHlg.childAlignment = TextAnchor.MiddleCenter;
            audioHlg.childForceExpandWidth = true;
            audioHlg.childForceExpandHeight = false;
            audioHlg.spacing = 0;

            screen._soundStrike = CreateAudioItem(audioGrid.gameObject, "Sound", "Icons/icon-sfx-on", () => screen.OnToggleSFX());
            screen._hapticStrike = CreateAudioItem(audioGrid.gameObject, "Haptic", "Icons/icon-vibration", () => screen.OnToggleHaptic(), 0f, 80f);
            screen._musicStrike = CreateAudioItem(audioGrid.gameObject, "Music", "Icons/icon-music-on", () => screen.OnToggleMusic());

            // ===== SAVE CARD =====
            var saveCard = CreateCard(content.gameObject, "SaveCard");
            saveCard.AddComponent<LayoutElement>().preferredHeight = CardPadding * 2 + 65 + 43 + 160;

            var saveInner = R(saveCard, "Inner");
            saveInner.anchorMin = V(0, 0); saveInner.anchorMax = V(1, 1);
            saveInner.offsetMin = V(CardPadding, CardPadding);
            saveInner.offsetMax = V(-CardPadding, -CardPadding);
            var saveVlg = saveInner.gameObject.AddComponent<VerticalLayoutGroup>();
            saveVlg.childAlignment = TextAnchor.MiddleCenter;
            saveVlg.childForceExpandWidth = true;
            saveVlg.childForceExpandHeight = false;
            saveVlg.spacing = 43;

            // Save title
            var saveTitleGo = new GameObject("SaveTitle");
            saveTitleGo.transform.SetParent(saveInner, false);
            saveTitleGo.AddComponent<RectTransform>().sizeDelta = V(0, 65);
            saveTitleGo.AddComponent<LayoutElement>().preferredHeight = 65;
            var saveTitle = saveTitleGo.AddComponent<TextMeshProUGUI>();
            saveTitle.text = "Save Your Progress";
            saveTitle.fontSize = 56;
            saveTitle.fontStyle = FontStyles.Bold;
            saveTitle.alignment = TextAlignmentOptions.Center;
            saveTitle.color = Color.white;
            saveTitle.font = ThemeConfig.GetFont();
            saveTitle.characterSpacing = -3f;
            saveTitle.raycastTarget = false;
            var saveMat = new Material(saveTitle.fontSharedMaterial);
            saveMat.SetFloat("_FaceDilate", 1.0f);
            saveMat.EnableKeyword("OUTLINE_ON");
            saveMat.SetFloat("_OutlineWidth", 0.4f);
            saveMat.SetColor("_OutlineColor", Color.black);
            saveTitle.fontMaterial = saveMat;

            CreateGoogleButton(saveInner.gameObject, screen);

            // ===== SUPPORT BUTTON =====
            var supportWrapper = new GameObject("SupportWrapper");
            supportWrapper.transform.SetParent(content, false);
            supportWrapper.AddComponent<RectTransform>();
            supportWrapper.AddComponent<LayoutElement>().preferredHeight = 160;
            var supportHlg = supportWrapper.AddComponent<HorizontalLayoutGroup>();
            supportHlg.childAlignment = TextAnchor.MiddleCenter;
            supportHlg.childForceExpandWidth = false;
            supportHlg.childForceExpandHeight = true;

            CreateSupportButton(supportWrapper, screen);

            // ===== LEGAL ROW =====
            var legalRow = new GameObject("LegalRow");
            legalRow.transform.SetParent(content, false);
            legalRow.AddComponent<RectTransform>();
            legalRow.AddComponent<LayoutElement>().preferredHeight = 110;
            var legalHlg = legalRow.AddComponent<HorizontalLayoutGroup>();
            legalHlg.spacing = LegalGap;
            legalHlg.padding = new RectOffset((int)LegalPaddingLR, (int)LegalPaddingLR, 0, 0);
            legalHlg.childForceExpandWidth = true;
            legalHlg.childForceExpandHeight = true;
            legalHlg.childAlignment = TextAnchor.MiddleCenter;

            CreateLegalButton(legalRow, "Terms", () => { HapticUtils.TryVibrate(); Application.OpenURL("https://juicesort.com/terms"); });
            CreateLegalButton(legalRow, "Privacy", () => { HapticUtils.TryVibrate(); Application.OpenURL("https://juicesort.com/privacy"); });

            return go;
        }

        #endregion

        #region Card Container

        private static GameObject CreateCard(GameObject parent, string name)
        {
            var cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent.transform, false);
            cardGo.AddComponent<RectTransform>();

            // Border ring
            var border = R(cardGo, "Border");
            border.anchorMin = V(0, 0); border.anchorMax = V(1, 1);
            border.offsetMin = V(-CardBorderWidth, -CardBorderWidth);
            border.offsetMax = V(CardBorderWidth, CardBorderWidth);
            ImgSliced(border, UIShapeUtils.WhiteRoundedRect(CardRadius + 4, 64), Col("#1e1862")).raycastTarget = false;

            // Fill
            var fill = R(cardGo, "Fill");
            fill.anchorMin = V(0, 0); fill.anchorMax = V(1, 1);
            ImgSliced(fill, UIShapeUtils.WhiteRoundedRect(CardRadius, 64), Col("#2c2585")).raycastTarget = false;

            // Top shadow (h=8)
            var sh = R(cardGo, "TopShadow");
            sh.anchorMin = V(0, 1); sh.anchorMax = V(1, 1);
            sh.pivot = V(0.5f, 1); sh.sizeDelta = V(0, 8);
            Img(sh, null, ColA(0, 0, 0, 0.35f)).raycastTarget = false;

            // Bottom highlight (h=5)
            var hl = R(cardGo, "BottomHighlight");
            hl.anchorMin = V(0, 0); hl.anchorMax = V(1, 0);
            hl.pivot = V(0.5f, 0); hl.sizeDelta = V(0, 5);
            Img(hl, null, ColA(1, 1, 1, 0.04f)).raycastTarget = false;

            return cardGo;
        }

        #endregion

        #region 3D Button Builder

        private static GameObject Create3DButton(GameObject parent, string name,
            float w, float h,
            Color goldColor, Color borderColor, Color faceColorTop, Color faceColorBottom,
            int goldRadius, int borderRadius, int faceRadius,
            float glossAlpha,
            UnityEngine.Events.UnityAction onClick)
        {
            var btnRoot = new GameObject(name);
            btnRoot.transform.SetParent(parent.transform, false);
            btnRoot.AddComponent<RectTransform>();
            var le = btnRoot.AddComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = h;

            int sprSize = Mathf.Max(64, goldRadius * 4);

            // Layer 1: Gold ring (FULL button size)
            var goldRing = R(btnRoot, "GoldRing");
            goldRing.anchorMin = V(0, 0); goldRing.anchorMax = V(1, 1);
            ImgSliced(goldRing, UIShapeUtils.WhiteRoundedRect(goldRadius, sprSize), goldColor).raycastTarget = false;

            // Layer 2: Color border (inset 8px sides/top, 11px bottom — CSS: padding 3px, bottom 4px)
            var colorBorder = R(btnRoot, "ColorBorder");
            colorBorder.anchorMin = V(0, 0); colorBorder.anchorMax = V(1, 1);
            colorBorder.offsetMin = V(8, 11); colorBorder.offsetMax = V(-8, -8);
            ImgSliced(colorBorder, UIShapeUtils.WhiteRoundedRect(borderRadius, sprSize), borderColor).raycastTarget = false;

            // Layer 3: Color face (inset 5px from border, 8px bottom)
            // Uses rounded rect sprite + Mask to clip gradient child to rounded shape
            var colorFace = R(btnRoot, "ColorFace");
            colorFace.anchorMin = V(0, 0); colorFace.anchorMax = V(1, 1);
            colorFace.offsetMin = V(13, 19); colorFace.offsetMax = V(-13, -13);
            var faceImg = ImgSliced(colorFace, UIShapeUtils.WhiteRoundedRect(faceRadius, sprSize), Color.white);
            faceImg.raycastTarget = false;
            colorFace.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            // Gradient fill inside masked face
            var faceGrad = R(colorFace.gameObject, "FaceGradient");
            faceGrad.anchorMin = V(0, 0); faceGrad.anchorMax = V(1, 1);
            var faceGradImg = faceGrad.gameObject.AddComponent<Image>();
            faceGradImg.sprite = ThemeConfig.CreateGradientSprite(faceColorTop, faceColorBottom);
            faceGradImg.raycastTarget = false;

            // Gloss overlay (top 45% of face, smooth gradient)
            AddGloss(colorFace.gameObject, glossAlpha);

            // Transparent image for raycast + Button + ButtonBounce
            Img(btnRoot.GetComponent<RectTransform>(), null, Color.clear);
            var btn = btnRoot.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(onClick);
            btnRoot.AddComponent<ButtonBounce>();

            return colorFace.gameObject;
        }

        #endregion

        #region Audio Item

        private static GameObject CreateAudioItem(GameObject parent, string label, string iconPath,
            UnityEngine.Events.UnityAction onClick, float btnSize = 0f, float iconSize = 0f)
        {
            var item = new GameObject($"{label}Item");
            item.transform.SetParent(parent.transform, false);
            item.AddComponent<RectTransform>();
            var itemVlg = item.AddComponent<VerticalLayoutGroup>();
            itemVlg.childAlignment = TextAnchor.UpperCenter;
            itemVlg.childForceExpandWidth = false;
            itemVlg.childForceExpandHeight = false;
            itemVlg.spacing = AudioItemSpacing;

            // Label
            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(item.transform, false);
            lblGo.AddComponent<RectTransform>();
            lblGo.AddComponent<LayoutElement>().preferredHeight = 54;
            var lbl = lblGo.AddComponent<TextMeshProUGUI>();
            lbl.text = label;
            lbl.fontSize = 54;
            lbl.fontStyle = FontStyles.Bold;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.color = Color.white;
            lbl.font = ThemeConfig.GetFont();
            lbl.characterSpacing = -5f;
            lbl.raycastTarget = false;
            var lblMat = new Material(lbl.fontSharedMaterial);
            lblMat.SetFloat("_FaceDilate", 0.5f);
            lblMat.EnableKeyword("OUTLINE_ON");
            lblMat.SetFloat("_OutlineWidth", 0.5f);
            lblMat.SetColor("_OutlineColor", Color.black);
            lbl.fontMaterial = lblMat;

            // 3D button (green + gold ring) — face gradient: bright top → dark bottom
            float size = btnSize > 0 ? btnSize : AudioButtonSize;
            var face = Create3DButton(item, $"{label}Button",
                size, size,
                Col("#e0c040"), Col("#1a7820"), Col("#5ee85e"), Col("#209420"),
                AudioGoldRadius, AudioBorderRadius, AudioFaceRadius,
                0.55f, onClick);

            float icoSz = iconSize > 0 ? iconSize : AudioIconSize;

            // Icon shadow (offset down+right for 3D look)
            var iconShadow = R(face, "IconShadow");
            iconShadow.anchorMin = V(0.5f, 0.5f); iconShadow.anchorMax = V(0.5f, 0.5f);
            iconShadow.sizeDelta = V(icoSz, icoSz);
            iconShadow.anchoredPosition = V(2, -3);
            var shadowImg = iconShadow.gameObject.AddComponent<Image>();
            shadowImg.sprite = LoadWhiteIcon(iconPath);
            shadowImg.preserveAspect = true;
            shadowImg.color = ColA(0, 0, 0, 0.35f);
            shadowImg.raycastTarget = false;

            // Icon on face
            var icon = R(face, "Icon");
            icon.anchorMin = V(0.5f, 0.5f); icon.anchorMax = V(0.5f, 0.5f);
            icon.sizeDelta = V(icoSz, icoSz);
            var iconImg = icon.gameObject.AddComponent<Image>();
            iconImg.sprite = LoadWhiteIcon(iconPath);
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            // Strike line
            var strike = R(face, "StrikeLine");
            strike.anchorMin = V(0.5f, 0.5f); strike.anchorMax = V(0.5f, 0.5f);
            strike.sizeDelta = V(StrikeWidth, StrikeHeight);
            strike.localRotation = Quaternion.Euler(0, 0, -45f);
            ImgSliced(strike, UIShapeUtils.WhiteRoundedRect(3, 16), Col("#d02020")).raycastTarget = false;

            return strike.gameObject;
        }

        #endregion

        #region Google Button

        private static void CreateGoogleButton(GameObject parent, SettingsScreen screen)
        {
            var face = Create3DButton(parent, "GoogleButton",
                0, 160,
                Col("#e0c040"), Col("#1870a8"), Col("#60d8ff"), Col("#0878c0"),
                GoogleGoldRadius, GoogleBorderRadius, GoogleFaceRadius,
                0.55f, () => { HapticUtils.TryVibrate(); Debug.Log("[Settings] Google Sign-In clicked"); });

            // Content layout on face
            var faceContent = R(face, "Content");
            faceContent.anchorMin = V(0, 0); faceContent.anchorMax = V(1, 1);
            var hlg = faceContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = GoogleContentGap;
            hlg.padding = new RectOffset((int)GoogleFacePaddingLR, (int)GoogleFacePaddingLR, (int)GoogleFacePaddingTB, (int)GoogleFacePaddingTB);

            // Google logo
            var logoBg = new GameObject("LogoBg");
            logoBg.transform.SetParent(faceContent, false);
            logoBg.AddComponent<RectTransform>();
            var logoBgLE = logoBg.AddComponent<LayoutElement>();
            logoBgLE.preferredWidth = GoogleLogoSize;
            logoBgLE.preferredHeight = GoogleLogoSize;
            logoBgLE.minWidth = GoogleLogoSize;
            logoBgLE.minHeight = GoogleLogoSize;
            Img(logoBg.GetComponent<RectTransform>(), UIShapeUtils.WhiteCircle(64), Color.white).raycastTarget = false;

            // Google logo from Resources
            var gChild = new GameObject("GoogleLogo");
            gChild.transform.SetParent(logoBg.transform, false);
            var gR = gChild.AddComponent<RectTransform>();
            gR.anchorMin = V(0.15f, 0.15f); gR.anchorMax = V(0.85f, 0.85f);
            gR.offsetMin = V(0, 0); gR.offsetMax = V(0, 0);
            var gImg = gChild.AddComponent<Image>();
            var googleLogo = Resources.Load<Sprite>("Icons/google-logo");
            if (googleLogo != null) gImg.sprite = googleLogo;
            gImg.preserveAspect = true;
            gImg.raycastTarget = false;

            // Google text
            var txtGo = new GameObject("GoogleText");
            txtGo.transform.SetParent(faceContent, false);
            txtGo.AddComponent<RectTransform>();
            txtGo.AddComponent<LayoutElement>().preferredHeight = 51;
            var googleTxt = txtGo.AddComponent<TextMeshProUGUI>();
            googleTxt.text = "Sign in with Google";
            googleTxt.fontSize = 44;
            googleTxt.fontStyle = FontStyles.Bold;
            googleTxt.alignment = TextAlignmentOptions.MidlineLeft;
            googleTxt.color = Color.white;
            googleTxt.font = ThemeConfig.GetFont();
            googleTxt.characterSpacing = -3f;
            googleTxt.raycastTarget = false;
            var gMat = new Material(googleTxt.fontSharedMaterial);
            gMat.SetFloat("_FaceDilate", 0.7f);
            gMat.EnableKeyword("OUTLINE_ON");
            gMat.SetFloat("_OutlineWidth", 0.5f);
            gMat.SetColor("_OutlineColor", Color.black);
            googleTxt.fontMaterial = gMat;
        }

        #endregion

        #region Support Button

        private static void CreateSupportButton(GameObject parent, SettingsScreen screen)
        {
            var face = Create3DButton(parent, "SupportButton",
                (1080 - ContentPaddingLR * 2) * 0.72f, 160,
                Col("#e0c040"), Col("#1a8820"), Col("#6ef848"), Col("#1c8c0c"),
                SupportGoldRadius, SupportBorderRadius, SupportFaceRadius,
                0.5f, () => { HapticUtils.TryVibrate(); Application.OpenURL("mailto:support@juicesort.com"); });

            var faceContent = R(face, "Content");
            faceContent.anchorMin = V(0, 0); faceContent.anchorMax = V(1, 1);
            var hlg = faceContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = SupportContentGap;
            hlg.padding = new RectOffset((int)SupportFacePaddingLR, (int)SupportFacePaddingLR, (int)SupportFacePaddingTB, (int)SupportFacePaddingTB);

            var iconGo = new GameObject("SupportIcon");
            iconGo.transform.SetParent(faceContent, false);
            iconGo.AddComponent<RectTransform>();
            var iconLE = iconGo.AddComponent<LayoutElement>();
            iconLE.preferredWidth = SupportIconSize; iconLE.preferredHeight = SupportIconSize;
            iconLE.minWidth = SupportIconSize; iconLE.minHeight = SupportIconSize;
            var supportIconImg = Img(iconGo.GetComponent<RectTransform>(), LoadWhiteIcon("Icons/support-icon"), Color.white);
            supportIconImg.preserveAspect = true;
            supportIconImg.raycastTarget = false;

            // Support text
            var txtGo = new GameObject("SupportText");
            txtGo.transform.SetParent(faceContent, false);
            txtGo.AddComponent<RectTransform>();
            txtGo.AddComponent<LayoutElement>().preferredHeight = 59;
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            txt.text = "Support"; txt.fontSize = 59; txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.color = Color.white; txt.font = ThemeConfig.GetFont();
            txt.raycastTarget = false;
            var sMat = new Material(txt.fontSharedMaterial);
            sMat.SetFloat("_FaceDilate", 0.5f);
            sMat.EnableKeyword("OUTLINE_ON");
            sMat.SetFloat("_OutlineWidth", 0.4f);
            sMat.SetColor("_OutlineColor", Color.black);
            txt.fontMaterial = sMat;
        }

        #endregion

        #region Legal Buttons

        private static void CreateLegalButton(GameObject parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var btnRoot = new GameObject($"{label}Button");
            btnRoot.transform.SetParent(parent.transform, false);
            btnRoot.AddComponent<RectTransform>();

            int sprSize = Mathf.Max(64, LegalOuterRadius * 4);

            // Purple outer
            var outer = R(btnRoot, "PurpleOuter");
            outer.anchorMin = V(0, 0); outer.anchorMax = V(1, 1);
            ImgSliced(outer, UIShapeUtils.WhiteRoundedRect(LegalOuterRadius, sprSize), Col("#6050b0")).raycastTarget = false;

            // Purple border (inset 8/11 — CSS: padding 3px, bottom 4px)
            var border = R(btnRoot, "PurpleBorder");
            border.anchorMin = V(0, 0); border.anchorMax = V(1, 1);
            border.offsetMin = V(8, 11); border.offsetMax = V(-8, -8);
            ImgSliced(border, UIShapeUtils.WhiteRoundedRect(LegalBorderRadius, sprSize), Col("#483890")).raycastTarget = false;

            // Purple face (inset 5/8 from border — CSS: padding 2px, bottom 3px)
            var face = R(btnRoot, "PurpleFace");
            face.anchorMin = V(0, 0); face.anchorMax = V(1, 1);
            face.offsetMin = V(13, 19); face.offsetMax = V(-13, -13);
            var faceImgLegal = ImgSliced(face, UIShapeUtils.WhiteRoundedRect(LegalFaceRadius, sprSize), Color.white);
            faceImgLegal.raycastTarget = false;
            face.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            var faceGradLegal = R(face.gameObject, "FaceGradient");
            faceGradLegal.anchorMin = V(0, 0); faceGradLegal.anchorMax = V(1, 1);
            var faceGradLegalImg = faceGradLegal.gameObject.AddComponent<Image>();
            faceGradLegalImg.sprite = ThemeConfig.CreateGradientSprite(Col("#9080d4"), Col("#5040a0"));
            faceGradLegalImg.raycastTarget = false;

            // Gloss
            AddGloss(face.gameObject, 0.3f);

            // Label
            var txt = Txt(face.gameObject, label, 54f, Color.white, 0.5f, 0.5f);
            txt.rectTransform.offsetMin = V(LegalFacePaddingLR, LegalFacePaddingTB);
            txt.rectTransform.offsetMax = V(-LegalFacePaddingLR, -LegalFacePaddingTB);

            // Button
            Img(btnRoot.GetComponent<RectTransform>(), null, Color.clear);
            var btn = btnRoot.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            btnRoot.AddComponent<ButtonBounce>();
        }

        #endregion

        #region Helpers

        private static Vector2 V(float x, float y) => new Vector2(x, y);

        private static RectTransform R(GameObject p, string n)
        {
            var g = new GameObject(n);
            g.transform.SetParent(p.transform, false);
            var r = g.AddComponent<RectTransform>();
            r.anchorMin = V(0, 0); r.anchorMax = V(1, 1);
            r.offsetMin = V(0, 0); r.offsetMax = V(0, 0);
            return r;
        }

        private static Image Img(RectTransform rt, Sprite sprite, Color color)
        {
            var img = rt.gameObject.AddComponent<Image>();
            if (sprite != null) img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static Image ImgSliced(RectTransform rt, Sprite sprite, Color color)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            return img;
        }

        private static void AddGloss(GameObject parent, float alpha, float bottom = 0f, float hInset = 0f)
        {
            // Full-height gradient: white at top → transparent at ~55% → fully transparent at bottom
            // This eliminates the hard edge — gradient sprite handles the smooth fade
            var gloss = R(parent, "Gloss");
            gloss.anchorMin = V(hInset, bottom);
            gloss.anchorMax = V(1f - hInset, 1);
            gloss.offsetMin = V(0, 0); gloss.offsetMax = V(0, 0);
            var img = gloss.gameObject.AddComponent<Image>();
            img.sprite = ThemeConfig.CreateGradientSprite(ColA(1, 1, 1, alpha), Color.clear);
            img.raycastTarget = false;
        }

        private static TextMeshProUGUI Txt(GameObject p, string txt, float sz, Color c,
            float faceDilate = 0.5f, float outlineW = 0.3f)
        {
            var g = new GameObject("T");
            g.transform.SetParent(p.transform, false);
            var r = g.AddComponent<RectTransform>();
            r.anchorMin = V(0, 0); r.anchorMax = V(1, 1);
            r.offsetMin = V(0, 0); r.offsetMax = V(0, 0);
            var t = g.AddComponent<TextMeshProUGUI>();
            t.text = txt;
            t.fontSize = sz;
            t.alignment = TextAlignmentOptions.Center;
            t.color = c;
            t.font = ThemeConfig.GetFont();
            t.fontStyle = FontStyles.Bold;
            t.raycastTarget = false;
            var mat = new Material(t.fontSharedMaterial);
            mat.SetFloat("_FaceDilate", faceDilate);
            mat.EnableKeyword("OUTLINE_ON");
            mat.SetFloat("_OutlineWidth", outlineW);
            mat.SetColor("_OutlineColor", Color.black);
            t.fontMaterial = mat;
            return t;
        }

        private static Color Col(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        private static Color ColA(float r, float g, float b, float a)
        {
            return new Color(r, g, b, a);
        }

        private static readonly System.Collections.Generic.Dictionary<string, Sprite> _whiteIconCache = new();

        private static Sprite LoadWhiteIcon(string resourcePath)
        {
            if (_whiteIconCache.TryGetValue(resourcePath, out var cached)) return cached;
            var srcTex = Resources.Load<Texture2D>(resourcePath);
            if (srcTex == null)
            {
                var srcSprite = Resources.Load<Sprite>(resourcePath);
                if (srcSprite != null) srcTex = srcSprite.texture;
            }
            if (srcTex == null) return null;

            // Read pixels and force all non-transparent to white
            var rt = RenderTexture.GetTemporary(srcTex.width, srcTex.height, 0);
            Graphics.Blit(srcTex, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var whiteTex = new Texture2D(srcTex.width, srcTex.height, TextureFormat.RGBA32, false);
            whiteTex.ReadPixels(new Rect(0, 0, srcTex.width, srcTex.height), 0, 0);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            var pixels = whiteTex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.01f)
                    pixels[i] = new Color(1f, 1f, 1f, pixels[i].a);
            }
            whiteTex.SetPixels(pixels);
            whiteTex.Apply();

            var sprite = Sprite.Create(whiteTex, new Rect(0, 0, whiteTex.width, whiteTex.height), new Vector2(0.5f, 0.5f));
            _whiteIconCache[resourcePath] = sprite;
            return sprite;
        }

        #endregion
    }
}
