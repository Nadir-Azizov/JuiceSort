using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    public class HubScreen : MonoBehaviour
    {
        private TextMeshProUGUI _levelButtonText;
        private TextMeshProUGUI _coinText;
        private int _currentLevel;

        // Nav bar state
        private RectTransform[] _navIcons;
        private Image[] _navImages;
        private GameObject[] _navGlows;
        private int _activeTab = 2; // Home default
        private Coroutine[] _navAnimCoroutines;

        // Play button state
        private RectTransform _playButtonFace;
        private Vector2 _playButtonBasePos;
        private CanvasGroup _playGlowCG;
        private RectTransform _playGlowRT;

        // Animation coroutines
        private Coroutine _playPulseCoroutine;
        private Coroutine _playGlowCoroutine;
        private Coroutine _playShimmerCoroutine;
        private Coroutine _goldShimmerCoroutine;
        private Coroutine _navGlowCoroutine;

        // Events
        public event Action<int> OnTabChanged;

        // Track runtime-created textures so we only destroy those (never Resources-loaded assets)
        private readonly List<Texture2D> _runtimeTextures = new List<Texture2D>();

        // Cached colors
        static readonly Color INACTIVE_TINT = new Color(0.55f, 0.55f, 0.55f, 0.7f);
    static readonly Color ACTIVE_TINT = Color.white;
        static readonly Color GOLD = new Color(0.784f, 0.659f, 0.306f, 1f); // #C8A84E
        static readonly Color GOLD_SEMI = new Color(0.784f, 0.659f, 0.306f, 0.35f);

        void OnEnable() { if (_levelButtonText != null) Refresh(); }
        void Start() { Refresh(); }

        void OnDestroy()
        {
            StopAllCoroutines();

            // Only destroy runtime-created textures — never Resources-loaded assets
            foreach (var tex in _runtimeTextures)
            {
                if (tex != null) Destroy(tex);
            }
            _runtimeTextures.Clear();

            // Clean up cloned TMPro materials
            foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.fontMaterial != null && tmp.fontMaterial != tmp.font?.material)
                    Destroy(tmp.fontMaterial);
            }
        }

        public void Refresh()
        {
            if (!Services.TryGet<IProgressionManager>(out var p)) return;
            _currentLevel = p.CurrentLevel;
            if (_levelButtonText != null) _levelButtonText.text = $"<b>Level {_currentLevel}</b>";
            ThemeConfig.CurrentMood = CityAssigner.AssignMood(_currentLevel);
            if (Services.TryGet<ICoinManager>(out var c) && _coinText != null)
                _coinText.text = $"<b>{c.GetBalance().ToString("N0")}</b>";
        }

        void Play()
        {
            if (!Services.TryGet<GameplayManager>(out var g)) return;
            if (!Services.TryGet<ScreenManager>(out var s)) return;
            g.StartLevel(_currentLevel); s.TransitionTo(GameFlowState.Playing);
        }
        void GoRoadmap() { if (Services.TryGet<ScreenManager>(out var s)) s.TransitionTo(GameFlowState.Roadmap); }
        void GoSettings() { if (Services.TryGet<ScreenManager>(out var s)) s.TransitionTo(GameFlowState.Settings); }

        // ================================================================
        // TAB SWITCHING
        // ================================================================

        void OnTabClicked(int index)
        {
            if (index == _activeTab) return;
            if (index < 0 || index >= _navIcons.Length) return;

            // Stop any running animation on both tabs
            if (_navAnimCoroutines[_activeTab] != null) StopCoroutine(_navAnimCoroutines[_activeTab]);
            if (_navAnimCoroutines[index] != null) StopCoroutine(_navAnimCoroutines[index]);

            // Old tab — shrink with undershoot
            _navAnimCoroutines[_activeTab] = StartCoroutine(AnimateShrinkOut(_activeTab));

            // New tab — bounce up with overshoot
            _navAnimCoroutines[index] = StartCoroutine(AnimateBounceIn(index));

            _activeTab = index;
            OnTabChanged?.Invoke(index);
            // TODO: wire non-Home tabs to screens when implemented
        }

        // ================================================================
        // KEYFRAME INTERPOLATION
        // ================================================================

        // Evaluates a keyframe curve at time t (0-1) using linear interpolation between keys
        static float EvalKeyframes(float t, float[] times, float[] values)
        {
            if (t <= times[0]) return values[0];
            if (t >= times[times.Length - 1]) return values[values.Length - 1];
            for (int i = 0; i < times.Length - 1; i++)
            {
                if (t <= times[i + 1])
                {
                    float seg = (t - times[i]) / (times[i + 1] - times[i]);
                    return Mathf.Lerp(values[i], values[i + 1], seg);
                }
            }
            return values[values.Length - 1];
        }

        // ACTIVATE keyframes: 1.0 → 1.65 → 1.30 → 1.52 → 1.42 → 1.45
        static readonly float[] BOUNCE_TIMES  = { 0f, 0.25f, 0.45f, 0.70f, 0.85f, 1f };
        static readonly float[] BOUNCE_SCALES = { 1.0f, 1.65f, 1.30f, 1.52f, 1.42f, 1.45f };

        // DEACTIVATE keyframes: 1.45 → 0.85 → 1.05 → 1.0
        static readonly float[] SHRINK_TIMES  = { 0f, 0.50f, 0.75f, 1f };
        static readonly float[] SHRINK_SCALES = { 1.45f, 0.85f, 1.05f, 1.0f };

        // ================================================================
        // ANIMATION COROUTINES
        // ================================================================

        // NEW TAB: Bounce up with overshoot keyframes (0.45s)
        // 1.0 → 1.65 (overshoot!) → 1.30 → 1.52 → 1.42 → 1.45 (settle)
        IEnumerator AnimateBounceIn(int index)
        {
            float duration = 0.45f;
            float elapsed = 0f;
            var icon = _navIcons[index];
            var img = _navImages[index];
            Color startColor = img.color;

            if (_navGlows[index] != null) _navGlows[index].SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Scale from keyframe table — visible overshoot to 1.65
                float scale = EvalKeyframes(t, BOUNCE_TIMES, BOUNCE_SCALES);
                icon.localScale = Vector3.one * scale;

                // Color — linear over 0.3s
                float colorT = Mathf.Clamp01(elapsed / 0.3f);
                img.color = Color.Lerp(startColor, ACTIVE_TINT, colorT);

                yield return null;
            }
            icon.localScale = Vector3.one * 1.45f;
            img.color = ACTIVE_TINT;
        }

        // OLD TAB: Shrink with undershoot keyframes (0.25s)
        // 1.45 → 0.85 (undershoot!) → 1.05 → 1.0 (settle)
        IEnumerator AnimateShrinkOut(int index)
        {
            float duration = 0.25f;
            float elapsed = 0f;
            var icon = _navIcons[index];
            var img = _navImages[index];
            Color startColor = img.color;

            if (_navGlows[index] != null) _navGlows[index].SetActive(false);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Scale from keyframe table — undershoot to 0.85
                float scale = EvalKeyframes(t, SHRINK_TIMES, SHRINK_SCALES);
                icon.localScale = Vector3.one * scale;

                // Color
                img.color = Color.Lerp(startColor, INACTIVE_TINT, t);

                yield return null;
            }
            icon.localScale = Vector3.one;
            img.color = INACTIVE_TINT;
        }

        IEnumerator AnimatePlayPulse(RectTransform button)
        {
            while (true)
            {
                float t = Time.time;
                float scale = Mathf.Lerp(1f, 1.02f, (Mathf.Sin(t * Mathf.PI / 1.5f) + 1f) * 0.5f);
                button.localScale = Vector3.one * scale;
                yield return null;
            }
        }

        IEnumerator AnimatePlayGlow(CanvasGroup cg, RectTransform rt)
        {
            while (true)
            {
                float t = Time.time;
                float sine = (Mathf.Sin(t * Mathf.PI / 1.25f) + 1f) * 0.5f;
                cg.alpha = Mathf.Lerp(0.35f, 0.7f, sine);  // HTML: 0.5 -> 1.0 (toned down for mobile)
                float s = Mathf.Lerp(1f, 1.1f, sine);   // HTML: scale 1 -> 1.1
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        IEnumerator AnimatePlayShimmer(RectTransform shimmer, float buttonWidth, float shimmerWidth)
        {
            // HTML: animation 2.5s, sweep from -50% to 120% in first 35%, hold rest
            var startPos = new Vector2(-shimmerWidth, 0);
            float endX = buttonWidth * 1.2f;
            while (true)
            {
                shimmer.anchoredPosition = startPos;
                float elapsed = 0f;
                float sweepDuration = 0.875f; // 35% of 2.5s
                while (elapsed < sweepDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / sweepDuration);
                    float ease = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
                    float x = Mathf.Lerp(-shimmerWidth, endX, ease);
                    shimmer.anchoredPosition = new Vector2(x, 0);
                    yield return null;
                }
                // Hold at end for remaining time, then restart (total ~2.5s cycle)
                yield return new WaitForSeconds(1.625f);
            }
        }

        IEnumerator AnimateGoldShimmer(RectTransform shimmer, float barWidth, float shimmerWidth)
        {
            var startPos = new Vector2(-shimmerWidth, 0);
            while (true)
            {
                shimmer.anchoredPosition = startPos;
                float elapsed = 0f;
                while (elapsed < 5f)
                {
                    elapsed += Time.deltaTime;
                    float x = Mathf.Lerp(-shimmerWidth, barWidth, elapsed / 5f);
                    shimmer.anchoredPosition = new Vector2(x, 0);
                    yield return null;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator AnimateNavGlow()
        {
            while (true)
            {
                float t = Time.time;
                // Pulse alpha of active tab's glow
                if (_navGlows != null && _activeTab >= 0 && _activeTab < _navGlows.Length && _navGlows[_activeTab] != null)
                {
                    var glowImg = _navGlows[_activeTab].GetComponent<Image>();
                    if (glowImg != null)
                    {
                        float navT = Mathf.Repeat(t * 0.5f, 1f); // 2s cycle
                        float ease = navT < 0.5f
                            ? 4f * navT * navT * navT
                            : 1f - Mathf.Pow(-2f * navT + 2f, 3f) / 2f;
                        float navA = Mathf.Lerp(0.15f, 0.5f, ease);
                        glowImg.color = new Color(1f, 0.84f, 0f, navA);
                    }
                }
                yield return null;
            }
        }

        // ================================================================
        // PLAY BUTTON PRESS FEEDBACK
        // ================================================================

        void OnPlayPointerDown()
        {
            if (_playPulseCoroutine != null) { StopCoroutine(_playPulseCoroutine); _playPulseCoroutine = null; }
            _playButtonFace.localScale = Vector3.one;
            _playButtonFace.anchoredPosition = _playButtonBasePos + new Vector2(0, -6f);
        }

        void OnPlayPointerUp()
        {
            _playButtonFace.anchoredPosition = _playButtonBasePos;
            _playPulseCoroutine = StartCoroutine(AnimatePlayPulse(_playButtonFace));
        }

        // ================================================================
        // FACTORY: Create()
        // ================================================================

        public static GameObject Create()
        {
            var go = new GameObject("HubScreen");
            var cv = go.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay; cv.sortingOrder = 10;
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080f, 1920f);
            sc.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            var hub = go.AddComponent<HubScreen>();

            // ===== BACKGROUND (aspect-fill / cover) =====
            var bgR = R(go, "BG");
            bgR.anchorMin = V(0, 0); bgR.anchorMax = V(1, 1);
            var bgRaw = bgR.gameObject.AddComponent<RawImage>();

            var bgTex2D = Resources.Load<Texture2D>("Backgrounds/hub_background");
            if (bgTex2D != null)
            {
                bgRaw.texture = bgTex2D;
            }
            else
            {
                // Fallback: dark blue gradient
                var fbTex = new Texture2D(1, 512, TextureFormat.RGBA32, false);
                fbTex.wrapMode = TextureWrapMode.Clamp; fbTex.filterMode = FilterMode.Bilinear;
                Color cTop = new Color(0.02f, 0.02f, 0.12f);
                Color cBot = new Color(0.08f, 0.12f, 0.35f);
                for (int y = 0; y < 512; y++)
                    fbTex.SetPixel(0, y, Color.Lerp(cBot, cTop, (float)y / 511f));
                fbTex.Apply();
                bgRaw.texture = fbTex;
                hub._runtimeTextures.Add(fbTex);
            }

            bgR.gameObject.AddComponent<AspectFillScaler>();

            var safe = R(go, "Safe"); ApplySafeArea(safe);

            // ===== TOP BAR =====
            var top = R(safe.gameObject, "Top");
            top.anchorMin = V(0,1); top.anchorMax = V(1,1);
            top.pivot = V(0.5f,1); top.sizeDelta = V(0,140);

            // COIN PILL — matches GameplayHUD night mode
            var pill = R(top.gameObject, "Pill");
            pill.anchorMin = V(0,0.5f); pill.anchorMax = V(0,0.5f);
            pill.pivot = V(0,0.5f); pill.anchoredPosition = V(28,0); pill.sizeDelta = V(250,72);
            // Fill
            var pillFill = R(pill.gameObject, "Fill");
            pillFill.anchorMin = V(0,0); pillFill.anchorMax = V(1,1);
            var pillFillI = pillFill.gameObject.AddComponent<Image>();
            pillFillI.sprite = UIShapeUtils.WhiteRoundedRect(60, 128);
            pillFillI.type = Image.Type.Sliced;
            pillFillI.color = new Color(0f, 0f, 0f, 0.4f);
            pillFillI.raycastTarget = false;
            // Border ring
            var pillBrd = R(pill.gameObject, "BorderRing");
            pillBrd.anchorMin = V(0,0); pillBrd.anchorMax = V(1,1);
            var brdRect = pillBrd.GetComponent<RectTransform>();
            brdRect.offsetMin = V(-4.5f, -4.5f); brdRect.offsetMax = V(4.5f, 4.5f);
            var pillBrdI = pillBrd.gameObject.AddComponent<Image>();
            pillBrdI.sprite = UIShapeUtils.WhiteRoundedRect(60, 128);
            pillBrdI.type = Image.Type.Sliced;
            pillBrdI.color = new Color(1f, 1f, 1f, 0.45f);
            pillBrdI.raycastTarget = false;
            pillBrd.transform.SetAsFirstSibling();
            // Outer glow
            var pillGlow = R(pill.gameObject, "Glow");
            var pillGlowRect = pillGlow.GetComponent<RectTransform>();
            pillGlowRect.anchorMin = V(0,0); pillGlowRect.anchorMax = V(1,1);
            pillGlowRect.offsetMin = V(-30,-30); pillGlowRect.offsetMax = V(30,30);
            var pillGlowI = pillGlow.gameObject.AddComponent<Image>();
            pillGlowI.sprite = UIShapeUtils.Glow(128, Color.white, 0.5f);
            pillGlowI.color = new Color(1f, 0.784f, 0.196f, 0.1f);
            pillGlowI.raycastTarget = false;
            pillGlow.transform.SetAsFirstSibling();

            // Coin icon — radial gradient with border ring
            var coinC = R(pill.gameObject, "Coin");
            coinC.anchorMin = V(0,0.5f); coinC.anchorMax = V(0,0.5f);
            coinC.pivot = V(0,0.5f); coinC.anchoredPosition = V(10,0); coinC.sizeDelta = V(57,57);
            // Border ring behind
            var coinBrd = R(coinC.gameObject, "BorderRing");
            coinBrd.anchorMin = V(0,0); coinBrd.anchorMax = V(1,1);
            var coinBrdRect = coinBrd.GetComponent<RectTransform>();
            coinBrdRect.offsetMin = V(-6,-6); coinBrdRect.offsetMax = V(6,6);
            var coinBrdI = coinBrd.gameObject.AddComponent<Image>();
            coinBrdI.sprite = UIShapeUtils.WhiteCircle(64);
            coinBrdI.color = new Color(0.80f, 0.53f, 0f); // #CC8800
            coinBrdI.raycastTarget = false;
            // Gradient fill
            var coinFillGo = R(coinC.gameObject, "Fill");
            coinFillGo.anchorMin = V(0,0); coinFillGo.anchorMax = V(1,1);
            var coinFillI = coinFillGo.gameObject.AddComponent<Image>();
            coinFillI.sprite = UIShapeUtils.CoinGradient(128);
            coinFillI.color = Color.white;
            coinFillI.raycastTarget = false;
            // "$" symbol
            var coinSym = Txt(coinC.gameObject, "<b>$</b>", 24, new Color(0.545f, 0.412f, 0.078f));

            // Coin amount
            hub._coinText = Txt(pill.gameObject, "<b>0</b>", 51, new Color(1f, 0.878f, 0.4f));
            hub._coinText.alignment = TextAlignmentOptions.MidlineLeft;
            hub._coinText.enableAutoSizing = true;
            hub._coinText.fontSizeMin = 24f;
            hub._coinText.fontSizeMax = 51f;
            hub._coinText.extraPadding = true;
            var coinMat = new Material(hub._coinText.fontSharedMaterial);
            coinMat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.5f);
            hub._coinText.fontMaterial = coinMat;
            hub._coinText.rectTransform.anchorMin = V(0,0); hub._coinText.rectTransform.anchorMax = V(1,1);
            hub._coinText.rectTransform.offsetMin = V(76,0); hub._coinText.rectTransform.offsetMax = V(-12,0);

            // SETTINGS — matches GameplayHUD gear button (night mode)
            var setGo = new GameObject("Set");
            setGo.transform.SetParent(top.gameObject.transform, false);
            var setR = setGo.AddComponent<RectTransform>();
            setR.anchorMin = V(1,0.5f); setR.anchorMax = V(1,0.5f);
            setR.pivot = V(1,0.5f); setR.anchoredPosition = V(-28,0);
            setR.sizeDelta = V(108, 108);
            // Invisible raycast
            var setImg = setGo.AddComponent<Image>();
            setImg.color = Color.clear;
            // 3D shadow layer
            var setShadow = R(setGo, "Shadow");
            var setShadowRect = setShadow.GetComponent<RectTransform>();
            setShadowRect.anchorMin = V(0,0); setShadowRect.anchorMax = V(1,1);
            setShadowRect.offsetMin = V(0,-8); setShadowRect.offsetMax = V(0,0);
            var setShadowI = setShadow.gameObject.AddComponent<Image>();
            setShadowI.sprite = UIShapeUtils.WhiteRoundedRect(30, 90);
            setShadowI.type = Image.Type.Sliced;
            setShadowI.color = new Color(0.18f, 0.12f, 0.35f, 0.65f);
            setShadowI.raycastTarget = false;
            setShadow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            // Border ring
            var setBrd = R(setGo, "BorderRing");
            var setBrdRect = setBrd.GetComponent<RectTransform>();
            setBrdRect.anchorMin = V(0,0); setBrdRect.anchorMax = V(1,1);
            setBrdRect.offsetMin = V(-4.5f,-4.5f); setBrdRect.offsetMax = V(4.5f,4.5f);
            var setBrdI = setBrd.gameObject.AddComponent<Image>();
            setBrdI.sprite = UIShapeUtils.WhiteRoundedRect(30, 90);
            setBrdI.type = Image.Type.Sliced;
            setBrdI.color = new Color(1f, 1f, 1f, 0.4f);
            setBrdI.raycastTarget = false;
            setBrd.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            // Fill
            var setFill = R(setGo, "Fill");
            setFill.anchorMin = V(0,0); setFill.anchorMax = V(1,1);
            var setFillI = setFill.gameObject.AddComponent<Image>();
            setFillI.sprite = UIShapeUtils.WhiteRoundedRect(30, 90);
            setFillI.type = Image.Type.Sliced;
            setFillI.color = new Color(0.294f, 0.216f, 0.529f, 0.65f);
            setFillI.raycastTarget = false;
            setFill.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            // Glow
            var setGlow = R(setGo, "Glow");
            var setGlowRect = setGlow.GetComponent<RectTransform>();
            setGlowRect.anchorMin = V(0,0); setGlowRect.anchorMax = V(1,1);
            setGlowRect.offsetMin = V(-24,-24); setGlowRect.offsetMax = V(24,24);
            var setGlowI = setGlow.gameObject.AddComponent<Image>();
            setGlowI.sprite = UIShapeUtils.Glow(128, Color.white, 0.5f);
            setGlowI.color = new Color(0.549f, 0.392f, 0.863f, 0.15f);
            setGlowI.raycastTarget = false;
            setGlow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            setGlow.transform.SetAsFirstSibling();
            // Gear icon
            var setIcon = R(setGo, "GearIcon");
            setIcon.anchorMin = V(0.15f,0.15f); setIcon.anchorMax = V(0.85f,0.85f);
            var setIconI = setIcon.gameObject.AddComponent<Image>();
            setIconI.sprite = UIShapeUtils.WhiteGear(128);
            setIconI.color = new Color(1f, 1f, 1f, 0.85f);
            setIconI.raycastTarget = false;
            // Button + bounce
            setGo.AddComponent<Button>().onClick.AddListener(() => { TryHaptic(); hub.GoSettings(); });
            setGo.AddComponent<ButtonBounce>();

            // ROADMAP — PNG icon button
            var mapGo = SpriteBtn(safe.gameObject, "Map", "Icons/icon_worldmap", 108);
            var mapR = mapGo.GetComponent<RectTransform>();
            mapR.anchorMin = V(1,1); mapR.anchorMax = V(1,1);
            mapR.pivot = V(1,1); mapR.anchoredPosition = V(-28,-180);
            var mapRootImg = mapGo.GetComponent<Image>();
            var mapIconGo = R(mapGo, "Icon");
            mapIconGo.anchorMin = V(0,0); mapIconGo.anchorMax = V(1,1);
            var mapIconI = mapIconGo.gameObject.AddComponent<Image>();
            mapIconI.sprite = mapRootImg.sprite;
            mapIconI.preserveAspect = true;
            mapIconI.color = Color.white;
            mapIconI.raycastTarget = false;
            mapRootImg.sprite = null;
            mapRootImg.color = Color.clear;
            mapRootImg.raycastTarget = true;
            // Fill
            var mapFill = R(mapGo, "Fill");
            mapFill.anchorMin = V(0,0); mapFill.anchorMax = V(1,1);
            var mapFillI = mapFill.gameObject.AddComponent<Image>();
            mapFillI.sprite = UIShapeUtils.WhiteRoundedRect(30, 90);
            mapFillI.type = Image.Type.Sliced;
            mapFillI.color = new Color(0.294f, 0.216f, 0.529f, 0.65f);
            mapFillI.raycastTarget = false;
            mapFill.transform.SetAsFirstSibling();
            // Border ring
            var mapBrd = R(mapGo, "BorderRing");
            var mapBrdRect = mapBrd.GetComponent<RectTransform>();
            mapBrdRect.anchorMin = V(0,0); mapBrdRect.anchorMax = V(1,1);
            mapBrdRect.offsetMin = V(-4.5f,-4.5f); mapBrdRect.offsetMax = V(4.5f,4.5f);
            var mapBrdI = mapBrd.gameObject.AddComponent<Image>();
            mapBrdI.sprite = UIShapeUtils.WhiteRoundedRect(30, 90);
            mapBrdI.type = Image.Type.Sliced;
            mapBrdI.color = new Color(1f, 1f, 1f, 0.4f);
            mapBrdI.raycastTarget = false;
            mapBrd.transform.SetAsFirstSibling();
            mapIconGo.transform.SetAsLastSibling();
            mapGo.GetComponent<Button>().onClick.AddListener(() => { TryHaptic(); hub.GoRoadmap(); });

            // ===== TAP AREA =====
            var tap = Img(go, "Tap", V(0,0.13f), V(1,0.87f));
            tap.color = new Color(0,0,0,0);
            var tapBtn = tap.gameObject.AddComponent<Button>();
            tapBtn.transition = Selectable.Transition.None;
            tapBtn.onClick.AddListener(() => hub.GoRoadmap());

            // ===== NAV BAR =====
            NavBar(go, hub);

            // ===== PLAY BUTTON =====
            PlayButton(go, hub);

            // ===== START ANIMATIONS =====
            hub._navGlowCoroutine = hub.StartCoroutine(hub.AnimateNavGlow());

            return go;
        }

        // ================================================================
        // NAV BAR
        // ================================================================

        static void NavBar(GameObject root, HubScreen hub)
        {
            hub._navIcons = new RectTransform[5];
            hub._navImages = new Image[5];
            hub._navGlows = new GameObject[5];
            hub._navAnimCoroutines = new Coroutine[5];

            var bar = R(root, "Nav");
            bar.anchorMin = V(0, 0); bar.anchorMax = V(1, 0);
            bar.pivot = V(0.5f, 0); bar.sizeDelta = V(0, 200);

            // Gradient background: top #4A2D8A -> bottom #251060
            var navBg = R(bar.gameObject, "NavBg");
            navBg.anchorMin = V(0, 0); navBg.anchorMax = V(1, 1);
            navBg.offsetMin = Vector2.zero; navBg.offsetMax = Vector2.zero;
            var navBgI = navBg.gameObject.AddComponent<Image>();
            navBgI.sprite = ThemeConfig.CreateGradientSprite(
                new Color(0.290f, 0.176f, 0.541f), // #4A2D8A top
                new Color(0.145f, 0.063f, 0.376f)); // #251060 bottom
            navBgI.type = Image.Type.Simple;
            navBgI.raycastTarget = false;
            navBg.transform.SetAsFirstSibling();

            // Gold top border — 3.5px, #C8A84E
            var goldLine = R(bar.gameObject, "GoldBorder");
            goldLine.anchorMin = V(0, 1); goldLine.anchorMax = V(1, 1);
            goldLine.pivot = V(0.5f, 0.5f);
            goldLine.anchoredPosition = V(0, 0);
            goldLine.sizeDelta = V(0, 3.5f);
            var goldLineI = goldLine.gameObject.AddComponent<Image>();
            goldLineI.color = GOLD;
            goldLineI.raycastTarget = false;

            // Gold shimmer band (child of gold border, clipped by RectMask2D)
            var goldMask = goldLine.gameObject.AddComponent<RectMask2D>();
            var shimmer = R(goldLine.gameObject, "Shimmer");
            shimmer.anchorMin = V(0, 0); shimmer.anchorMax = V(0, 1);
            shimmer.pivot = V(0, 0.5f);
            shimmer.sizeDelta = V(120, 0); // shimmer band width
            shimmer.anchoredPosition = V(-120, 0);
            var shimmerI = shimmer.gameObject.AddComponent<Image>();
            shimmerI.sprite = UIShapeUtils.Glow(64, Color.white, 0.8f);
            shimmerI.color = new Color(1f, 1f, 1f, 0.6f);
            shimmerI.raycastTarget = false;
            hub._goldShimmerCoroutine = hub.StartCoroutine(hub.AnimateGoldShimmer(shimmer, 1080f, 120f));

            // Gold glow behind border line
            var goldGlow = R(bar.gameObject, "GoldGlow");
            goldGlow.anchorMin = V(0.05f, 0.92f); goldGlow.anchorMax = V(0.95f, 1.05f);
            var goldGlowI = goldGlow.gameObject.AddComponent<Image>();
            goldGlowI.sprite = UIShapeUtils.Glow(128, new Color(1f, 0.8f, 0.2f, 0.15f), 0.5f);
            goldGlowI.raycastTarget = false;
            goldGlow.transform.SetAsFirstSibling();

            // Nav items + separators
            string[] labels = { "Shop", "Leaders", "Home", "Teams", "Collect" };
            string[] icoFallback = { "\u2B50", "\u265B", "\u2665", "\u2726", "\u2666" };
            string[] icoRes = {
                "Icons/NavBar/icon_shop",
                "Icons/NavBar/icon_leaderboard",
                "Icons/NavBar/icon_home",
                "Icons/NavBar/icon_teams",
                "Icons/NavBar/icon_collections"
            };
            // Fallback to old flat icons if new ones not found
            string[] icoResFallback = {
                "Icons/flat_icon_shop",
                "Icons/flat_icon_leaderboard",
                "Icons/flat_icon_home",
                "Icons/flat_icon_teams",
                "Icons/flat_icon_collections"
            };
            Color[] fallbackColors = {
                new Color(1f, 0.8f, 0.2f),
                new Color(1f, 0.6f, 0.2f),
                new Color(0.6f, 0.92f, 1f),
                new Color(0.4f, 1f, 0.55f),
                new Color(1f, 0.5f, 0.7f)
            };

            float tabW = 1f / 5f;
            for (int i = 0; i < 5; i++)
            {
                bool isActive = (i == 2); // Home tab

                // Separator before this tab (skip first)
                if (i > 0)
                {
                    var sep = R(bar.gameObject, $"Sep_{i - 1}_{i}");
                    sep.anchorMin = V(tabW * i - 0.001f, 0);
                    sep.anchorMax = V(tabW * i + 0.001f, 1);
                    sep.offsetMin = V(0, 14); sep.offsetMax = V(0, -14);
                    sep.sizeDelta = new Vector2(1.5f, sep.sizeDelta.y);
                    var sepI = sep.gameObject.AddComponent<Image>();
                    sepI.color = GOLD_SEMI;
                    sepI.raycastTarget = false;
                }

                // Nav item container
                var item = R(bar.gameObject, $"N{labels[i]}");
                item.anchorMin = V(tabW * i, 0); item.anchorMax = V(tabW * (i + 1), 1);
                item.offsetMin = V(4, 28); item.offsetMax = V(-4, -8);

                // Gold glow behind icon (active only)
                var gl = R(item.gameObject, "Gl");
                gl.anchorMin = V(-0.1f, 0.1f); gl.anchorMax = V(1.1f, 0.95f);
                var glI = gl.gameObject.AddComponent<Image>();
                glI.sprite = UIShapeUtils.Glow(64, new Color(1f, 0.84f, 0f, 0.4f), 0.55f);
                glI.raycastTarget = false;
                gl.transform.SetAsFirstSibling();
                gl.gameObject.SetActive(isActive);
                hub._navGlows[i] = gl.gameObject;

                // Icon — direct PNG, no background box
                var iconRT = R(item.gameObject, "Ico");
                iconRT.anchorMin = V(0.1f, 0.05f); iconRT.anchorMax = V(0.9f, 0.95f);
                var iconImg = iconRT.gameObject.AddComponent<Image>();
                iconImg.raycastTarget = false;
                iconImg.preserveAspect = true;

                // Load icon: new NavBar/ path -> old flat_ path -> text fallback
                Sprite iconSprite = LoadIconSprite(icoRes[i]);
                if (iconSprite == null) iconSprite = LoadIconSprite(icoResFallback[i]);

                if (iconSprite != null)
                {
                    iconImg.sprite = iconSprite;
                }
                else
                {
                    iconImg.color = Color.clear;
                    Txt(item.gameObject, $"<b>{icoFallback[i]}</b>", 36, fallbackColors[i]);
                }

                // Initial state: active = 1.45x white, inactive = 1.0x dimmed
                iconRT.localScale = isActive ? Vector3.one * 1.45f : Vector3.one;
                iconImg.color = isActive ? ACTIVE_TINT : INACTIVE_TINT;

                hub._navIcons[i] = iconRT;
                hub._navImages[i] = iconImg;

                // Invisible raycast + button
                var btnImg = item.gameObject.AddComponent<Image>();
                btnImg.color = Color.clear;
                var btn = item.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                int tabIndex = i;
                btn.onClick.AddListener(() => { TryHaptic(); hub.OnTabClicked(tabIndex); });
            }
        }

        static Sprite LoadIconSprite(string resPath)
        {
            var spr = Resources.Load<Sprite>(resPath);
            if (spr != null) return spr;
            var tex = Resources.Load<Texture2D>(resPath);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), V(0.5f, 0.5f));
            return null;
        }

        // ================================================================
        // PLAY BUTTON
        // ================================================================

        static void PlayButton(GameObject root, HubScreen hub)
        {
            // Position play button in the center-bottom area
            var cen = R(root, "Cen");
            cen.anchorMin = V(0, 0.15f); cen.anchorMax = V(1, 0.85f);

            float btnBottom = 0.04f;
            float btnTop = 0.16f;

            // Glow behind button — soft green radial, very faded
            var glowGo = R(cen.gameObject, "PlayGlow");
            glowGo.anchorMin = V(0.10f, btnBottom - 0.02f);
            glowGo.anchorMax = V(0.90f, btnTop + 0.02f);
            var glowImg = glowGo.gameObject.AddComponent<Image>();
            glowImg.sprite = UIShapeUtils.Glow(128, new Color(0.31f, 0.82f, 0.39f, 0.35f), 0.3f);
            glowImg.raycastTarget = false;
            hub._playGlowCG = glowGo.gameObject.AddComponent<CanvasGroup>();
            hub._playGlowCG.alpha = 0.5f;
            hub._playGlowRT = glowGo;

            // === BUTTON FACE ===
            // Single pill-shaped sprite with gradient. No separate shadow/border layers,
            // so transparent pill corners have nothing behind them to bleed through.
            var face = R(cen.gameObject, "PlayFace");
            face.anchorMin = V(0.20f, btnBottom);
            face.anchorMax = V(0.80f, btnTop);

            // Face image: rounded-rect pill with 4-stop green gradient
            var faceI = face.gameObject.AddComponent<Image>();
            faceI.sprite = hub.BakeGradientPillTracked(600, 180, 90,
                new Color(0.40f, 0.93f, 0.47f),  // #66ee77 top
                new Color(0.27f, 0.87f, 0.33f),  // #44dd55
                new Color(0.20f, 0.80f, 0.27f),  // #33cc44
                new Color(0.16f, 0.72f, 0.24f)); // #28b83d bottom
            faceI.type = Image.Type.Sliced;

            // Mask clips children (gloss, shimmer, etc.) to pill shape
            var maskImg = face.gameObject.AddComponent<Mask>();
            maskImg.showMaskGraphic = true;

            // Drop shadow via Unity's Shadow component — duplicates mesh below, same shape
            var shadow = face.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.086f, 0.420f, 0.157f, 0.8f); // #166b28
            shadow.effectDistance = new Vector2(0, -6f);

            // Button + bounce + press feedback
            var faceBtn = face.gameObject.AddComponent<Button>();
            faceBtn.transition = Selectable.Transition.None;
            faceBtn.onClick.AddListener(() => { TryHaptic(); hub.Play(); });
            face.gameObject.AddComponent<ButtonBounce>();

            var pressFeedback = face.gameObject.AddComponent<PlayButtonPressFeedback>();
            pressFeedback.Init(hub);

            hub._playButtonFace = face;
            hub._playButtonBasePos = face.anchoredPosition;

            // Top highlight — fades from white to transparent
            var gloss = R(face.gameObject, "Gloss");
            gloss.anchorMin = V(0, 0.65f); gloss.anchorMax = V(1, 0.95f);
            var glossI = gloss.gameObject.AddComponent<Image>();
            glossI.sprite = ThemeConfig.CreateGradientSprite(
                new Color(1f, 1f, 1f, 0.3f),
                new Color(1f, 1f, 1f, 0f));
            glossI.type = Image.Type.Simple;
            glossI.raycastTarget = false;

            // Inset bottom shadow — subtle dark at bottom
            var insetBot = R(face.gameObject, "InsetBot");
            insetBot.anchorMin = V(0, 0f); insetBot.anchorMax = V(1, 0.2f);
            var insetBotI = insetBot.gameObject.AddComponent<Image>();
            insetBotI.sprite = ThemeConfig.CreateGradientSprite(
                new Color(0f, 0f, 0f, 0f),
                new Color(0f, 0f, 0f, 0.1f));
            insetBotI.type = Image.Type.Simple;
            insetBotI.raycastTarget = false;

            // Shimmer sweep inside RectMask2D
            var shimmerMask = R(face.gameObject, "ShimmerMask");
            shimmerMask.anchorMin = V(0, 0); shimmerMask.anchorMax = V(1, 1);
            shimmerMask.gameObject.AddComponent<RectMask2D>();

            var shimmerBand = R(shimmerMask.gameObject, "ShimmerBand");
            shimmerBand.anchorMin = V(0, 0); shimmerBand.anchorMax = V(0, 1);
            shimmerBand.pivot = V(0.5f, 0.5f);
            shimmerBand.sizeDelta = V(300, 0);
            shimmerBand.anchoredPosition = V(-300, 0);
            var shimmerBandI = shimmerBand.gameObject.AddComponent<Image>();
            shimmerBandI.sprite = UIShapeUtils.Glow(64, Color.white, 0.3f);
            shimmerBandI.color = new Color(1f, 1f, 1f, 0.25f);
            shimmerBandI.raycastTarget = false;

            // Level text
            hub._levelButtonText = Txt(face.gameObject, "<b>Level 1</b>", 56, Color.white);
            hub._levelButtonText.extraPadding = true;
            hub._levelButtonText.raycastTarget = false;
            var mat = new Material(hub._levelButtonText.fontSharedMaterial);
            mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.3f);
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f);
            mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.white);
            hub._levelButtonText.fontMaterial = mat;

            // === DEBUG: Log play button configuration ===
            hub.StartCoroutine(LogPlayButtonDebugSimple(face, faceI));

            // Start play button animations
            hub._playPulseCoroutine = hub.StartCoroutine(hub.AnimatePlayPulse(face));
            hub._playGlowCoroutine = hub.StartCoroutine(hub.AnimatePlayGlow(hub._playGlowCG, hub._playGlowRT));
            hub._playShimmerCoroutine = hub.StartCoroutine(hub.AnimatePlayShimmer(shimmerBand, 600f, 300f));
        }

        // ================================================================
        // PLAY BUTTON DEBUG LOGGING
        // ================================================================

        static IEnumerator LogPlayButtonDebugSimple(RectTransform face, Image faceI)
        {
            yield return null; // wait one frame for layout

            var tag = "[PlayButton]";
            var sb = new StringBuilder();
            sb.AppendLine($"{tag} ===== PLAY BUTTON DEBUG DUMP =====");

            // Face
            sb.AppendLine($"{tag} FACE: ImageType={faceI.type}, Color={faceI.color}, " +
                $"RectSize={face.rect.width:F1}x{face.rect.height:F1}, " +
                $"AnchoredPos={face.anchoredPosition}");
            var s = faceI.sprite;
            if (s != null)
            {
                sb.AppendLine($"{tag}   Sprite: tex={s.texture.width}x{s.texture.height}, " +
                    $"border=L{s.border.x} B{s.border.y} R{s.border.z} T{s.border.w}");
                var tex = s.texture;
                int midY = tex.height / 2;
                sb.AppendLine($"{tag}   EdgePixels(y={midY}): " +
                    $"x=0 a={tex.GetPixel(0, midY).a:F3}, " +
                    $"x=1 a={tex.GetPixel(1, midY).a:F3}, " +
                    $"x=3 a={tex.GetPixel(3, midY).a:F3}, " +
                    $"x=10 a={tex.GetPixel(10, midY).a:F3}");
            }

            // Mask + Shadow
            var mask = face.GetComponent<Mask>();
            var shad = face.GetComponent<Shadow>();
            sb.AppendLine($"{tag} MASK: present={mask != null}, showGraphic={mask?.showMaskGraphic}");
            sb.AppendLine($"{tag} SHADOW: present={shad != null}" +
                (shad != null ? $", color={shad.effectColor}, dist={shad.effectDistance}" : ""));

            // All children
            sb.AppendLine($"{tag} Children ({face.childCount}):");
            for (int i = 0; i < face.childCount; i++)
            {
                var ch = face.GetChild(i);
                var rt = ch.GetComponent<RectTransform>();
                var img = ch.GetComponent<Image>();
                sb.AppendLine($"{tag}   [{i}] {ch.name}: " +
                    $"anchors=({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})-({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}), " +
                    $"size={rt.rect.width:F1}x{rt.rect.height:F1}" +
                    (img != null ? $", imgType={img.type}, color={img.color}" : ""));
            }

            // Sibling layers (parent = Cen)
            var parent = face.parent;
            sb.AppendLine($"{tag} Sibling layers under '{parent.name}' ({parent.childCount}):");
            for (int i = 0; i < parent.childCount; i++)
            {
                var sib = parent.GetChild(i);
                var sibImg = sib.GetComponent<Image>();
                var sibRT = sib.GetComponent<RectTransform>();
                sb.AppendLine($"{tag}   [{i}] {sib.name}: " +
                    $"size={sibRT.rect.width:F1}x{sibRT.rect.height:F1}, " +
                    $"pos={sibRT.anchoredPosition}" +
                    (sibImg != null ? $", color={sibImg.color}, type={sibImg.type}" : ""));
            }

            sb.AppendLine($"{tag} ===== END DEBUG DUMP =====");
            UnityEngine.Debug.Log(sb.ToString());
        }

        // ================================================================
        // PLAY BUTTON PRESS FEEDBACK COMPONENT
        // ================================================================

        class PlayButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            private HubScreen _hub;

            public void Init(HubScreen hub) { _hub = hub; }

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_hub != null) _hub.OnPlayPointerDown();
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                if (_hub != null) _hub.OnPlayPointerUp();
            }
        }

        // ================================================================
        // UTILITY METHODS
        // ================================================================

        static void TryHaptic() => HapticUtils.TryVibrate();

        static GameObject SpriteBtn(GameObject par, string n, string resPath, float sz)
        {
            var rt = R(par, n); rt.sizeDelta = V(sz, sz);
            var img = rt.gameObject.AddComponent<Image>();
            var spr = Resources.Load<Sprite>(resPath);
            if (spr == null)
            {
                var tex = Resources.Load<Texture2D>(resPath);
                if (tex != null)
                    spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), V(0.5f, 0.5f));
            }
            if (spr != null) { img.sprite = spr; img.preserveAspect = true; }
            else { img.color = new Color(0.5f, 0.25f, 0.9f, 1f); Debug.LogError($"[HubScreen] Failed to load icon: {resPath}"); }
            rt.gameObject.AddComponent<Button>();
            rt.gameObject.AddComponent<ButtonBounce>();
            return rt.gameObject;
        }

        // === TEXT with rich text <b> for REAL bold ===
        static TextMeshProUGUI Txt(GameObject p, string txt, float sz, Color c)
        {
            return Txt(p, txt, sz, c, V(0,0), V(1,1));
        }
        static TextMeshProUGUI Txt(GameObject p, string txt, float sz, Color c, Vector2 mn, Vector2 mx)
        {
            var g = new GameObject("T"); g.transform.SetParent(p.transform, false);
            var r = g.AddComponent<RectTransform>();
            r.anchorMin = mn; r.anchorMax = mx;
            r.offsetMin = V(0,0); r.offsetMax = V(0,0);
            var t = g.AddComponent<TextMeshProUGUI>();
            t.text = txt;
            t.fontSize = sz;
            t.alignment = TextAlignmentOptions.Center;
            t.color = c;
            t.font = ThemeConfig.GetFont();
            t.richText = true;
            t.fontStyle = FontStyles.Bold;
            t.fontWeight = FontWeight.Heavy;
            t.outlineWidth = 0.25f;
            t.outlineColor = new Color32(
                (byte)(c.r*255), (byte)(c.g*255), (byte)(c.b*255), (byte)(c.a*200));
            return t;
        }

        // === BAKE GRADIENT INTO PILL SHAPE ===
        Sprite BakeGradientPillTracked(int w, int h, int radius, Color c0, Color c1, Color c2, Color c3)
        {
            var sprite = BakeGradientPill(w, h, radius, c0, c1, c2, c3);
            _runtimeTextures.Add(sprite.texture);
            return sprite;
        }
        static Sprite BakeGradientPill(int w, int h, int radius, Color c0, Color c1, Color c2, Color c3)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float r = Mathf.Min(radius, Mathf.Min(w, h) / 2f);
            float se = 3f;
            // Extend pill shape beyond texture edges by 'se' so the AA fringe
            // falls outside the visible area — all texture pixels are fully opaque
            // along the straight edges, corners keep smooth antialiasing.
            float halfW = (w - 1) / 2f + se;
            float halfH = (h - 1) / 2f + se;

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                Color rowColor;
                if      (t > 0.7f) rowColor = Color.Lerp(c1, c0, (t - 0.7f) / 0.3f);
                else if (t > 0.4f) rowColor = Color.Lerp(c2, c1, (t - 0.4f) / 0.3f);
                else               rowColor = Color.Lerp(c3, c2, t / 0.4f);

                for (int x = 0; x < w; x++)
                {
                    float cx = Mathf.Max(Mathf.Abs(x - (w-1)/2f) - (halfW - r), 0f);
                    float cy = Mathf.Max(Mathf.Abs(y - (h-1)/2f) - (halfH - r), 0f);
                    float dist = Mathf.Sqrt(cx*cx + cy*cy) - r;
                    float alpha = dist < -se ? 1f : (dist > 0f ? 0f : -dist / se);
                    tex.SetPixel(x, y, new Color(rowColor.r, rowColor.g, rowColor.b, alpha));
                }
            }
            tex.Apply();
            int b = Mathf.CeilToInt(r);
            int bv = Mathf.Min(b, h / 2);
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), 100f,
                0, SpriteMeshType.FullRect, new Vector4(b, bv, b, bv));
        }

        // === HELPERS ===
        static Vector2 V(float x, float y) => new Vector2(x, y);
        static RectTransform R(GameObject p, string n)
        {
            var g = new GameObject(n); g.transform.SetParent(p.transform, false);
            var r = g.AddComponent<RectTransform>();
            r.anchorMin = V(0,0); r.anchorMax = V(1,1);
            r.offsetMin = V(0,0); r.offsetMax = V(0,0);
            return r;
        }
        static Image Img(GameObject p, string n, Vector2 mn, Vector2 mx)
        {
            var r = R(p, n); r.anchorMin = mn; r.anchorMax = mx;
            return r.gameObject.AddComponent<Image>();
        }
        static void ApplySafeArea(RectTransform r)
        {
            var s = Screen.safeArea; int w = Screen.width, h = Screen.height;
            if (w <= 0 || h <= 0) return;
            r.anchorMin = V(s.x/w, s.y/h);
            r.anchorMax = V(s.xMax/w, s.yMax/h);
            r.offsetMin = V(0,0); r.offsetMax = V(0,0);
        }
    }
}
