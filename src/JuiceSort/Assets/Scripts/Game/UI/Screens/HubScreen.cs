using System.Collections;
using UnityEngine;
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
        private Image _playGlowImage;
        private Image _navGlowImage;
        private int _currentLevel;

        void OnEnable() { if (_levelButtonText != null) Refresh(); }
        void Start() { Refresh(); }

        void OnDestroy()
        {
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                if (img.sprite != null && !img.sprite.name.StartsWith("Unity"))
                {
                    if (img.sprite.texture != null) Destroy(img.sprite.texture);
                    Destroy(img.sprite);
                }
            }
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

            // ===== BACKGROUND (hub_background.png from Resources) =====
            var bg = Img(go, "BG", V(0,0), V(1,1));
            bg.preserveAspect = false;
            var bgSprite = Resources.Load<Sprite>("Backgrounds/hub_background");
            if (bgSprite != null)
            {
                bg.sprite = bgSprite;
            }
            else
            {
                var bgTex2D = Resources.Load<Texture2D>("Backgrounds/hub_background");
                if (bgTex2D != null)
                {
                    bg.sprite = Sprite.Create(bgTex2D,
                        new Rect(0, 0, bgTex2D.width, bgTex2D.height), V(0.5f, 0.5f));
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
                    bg.sprite = Sprite.Create(fbTex, new Rect(0, 0, 1, 512), V(0.5f, 0.5f));
                }
            }
            bg.type = Image.Type.Simple;

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
            setGo.AddComponent<Button>().onClick.AddListener(() => hub.GoSettings());
            setGo.AddComponent<ButtonBounce>();

            // ROADMAP — PNG icon button
            var mapGo = SpriteBtn(safe.gameObject, "Map", "Icons/icon_worldmap", 108);
            var mapR = mapGo.GetComponent<RectTransform>();
            mapR.anchorMin = V(1,1); mapR.anchorMax = V(1,1);
            mapR.pivot = V(1,1); mapR.anchoredPosition = V(-28,-180);
            mapGo.GetComponent<Button>().onClick.AddListener(() => hub.GoRoadmap());

            // ===== TAP AREA =====
            var tap = Img(go, "Tap", V(0,0.13f), V(1,0.87f));
            tap.color = new Color(0,0,0,0);
            var tapBtn = tap.gameObject.AddComponent<Button>();
            tapBtn.transition = Selectable.Transition.None;
            tapBtn.onClick.AddListener(() => hub.GoRoadmap());

            // ===== CENTER =====
            var cen = R(go, "Cen");
            cen.anchorMin = V(0,0.15f); cen.anchorMax = V(1,0.85f);

            // ===== PLAY BUTTON =====

            // Glow — kept inside button bounds, no overflow
            var glow = R(cen.gameObject, "Glow");
            glow.anchorMin = V(0.18f,0.04f); glow.anchorMax = V(0.82f,0.17f);
            hub._playGlowImage = glow.gameObject.AddComponent<Image>();
            hub._playGlowImage.sprite = UIShapeUtils.Glow(128, new Color(0.2f,1f,0.3f,0.3f), 0.7f);
            hub._playGlowImage.raycastTarget = false;

            // 3D BOTTOM EDGE (darker green, offset down + right)
            var edge3 = R(cen.gameObject, "Edge3");
            edge3.anchorMin = V(0.185f, 0.015f); edge3.anchorMax = V(0.825f, 0.155f);
            var edge3I = edge3.gameObject.AddComponent<Image>();
            edge3I.sprite = BakeGradientPill(600, 180, 90,
                new Color(0.08f, 0.45f, 0.12f, 1f),
                new Color(0.05f, 0.32f, 0.08f, 1f),
                new Color(0.04f, 0.28f, 0.06f, 1f),
                new Color(0.03f, 0.22f, 0.05f, 1f));
            edge3I.type = Image.Type.Simple;
            edge3I.raycastTarget = false;

            // 3D MIDDLE EDGE (medium dark green, smaller offset)
            var edge2 = R(cen.gameObject, "Edge2");
            edge2.anchorMin = V(0.183f, 0.025f); edge2.anchorMax = V(0.823f, 0.165f);
            var edge2I = edge2.gameObject.AddComponent<Image>();
            edge2I.sprite = BakeGradientPill(600, 180, 90,
                new Color(0.1f, 0.52f, 0.15f, 1f),
                new Color(0.07f, 0.4f, 0.1f, 1f),
                new Color(0.06f, 0.35f, 0.08f, 1f),
                new Color(0.05f, 0.3f, 0.07f, 1f));
            edge2I.type = Image.Type.Simple;
            edge2I.raycastTarget = false;

            // PLAY BUTTON (main face) — exact CSS gradient
            var pb = R(cen.gameObject, "Play");
            pb.anchorMin = V(0.18f, 0.04f); pb.anchorMax = V(0.82f, 0.16f);
            var pbI = pb.gameObject.AddComponent<Image>();
            Color g0 = new Color(0.40f, 0.93f, 0.47f); // #66ee77
            Color g1 = new Color(0.27f, 0.87f, 0.33f); // #44dd55
            Color g2 = new Color(0.20f, 0.80f, 0.27f); // #33cc44
            Color g3 = new Color(0.16f, 0.72f, 0.24f); // #28b83d
            pbI.sprite = BakeGradientPill(600, 180, 90, g0, g1, g2, g3);
            pbI.type = Image.Type.Simple;
            pb.gameObject.AddComponent<Button>().onClick.AddListener(() => hub.Play());
            pb.gameObject.AddComponent<ButtonBounce>();

            // Subtle white tint at top only (thin, inside button, no overflow)
            var topTint = R(pb.gameObject, "TopTint");
            topTint.anchorMin = V(0.1f, 0.7f); topTint.anchorMax = V(0.9f, 0.95f);
            var topTintI = topTint.gameObject.AddComponent<Image>();
            topTintI.sprite = UIShapeUtils.WhitePill(400, 30);
            topTintI.type = Image.Type.Simple;
            topTintI.color = new Color(1f, 1f, 1f, 0.12f);
            topTintI.raycastTarget = false;

            // Shine dot left
            var shL = R(pb.gameObject, "ShL");
            shL.anchorMin = V(0.08f, 0.55f); shL.anchorMax = V(0.18f, 0.85f);
            var shLI = shL.gameObject.AddComponent<Image>();
            shLI.sprite = UIShapeUtils.WhiteCircle(32);
            shLI.color = new Color(1f, 1f, 1f, 0.1f);
            shLI.raycastTarget = false;

            // Shine dot right
            var shR = R(pb.gameObject, "ShR");
            shR.anchorMin = V(0.82f, 0.55f); shR.anchorMax = V(0.92f, 0.85f);
            var shRI = shR.gameObject.AddComponent<Image>();
            shRI.sprite = UIShapeUtils.WhiteCircle(32);
            shRI.color = new Color(1f, 1f, 1f, 0.08f);
            shRI.raycastTarget = false;

            // Level text — MASSIVE and 3X BOLD
            hub._levelButtonText = Txt(pb.gameObject, "<b>Level 1</b>", 56, Color.white);
            // Force bold through material — this works even if font atlas has no bold data
            hub._levelButtonText.extraPadding = true;
            var mat = new Material(hub._levelButtonText.fontSharedMaterial);
            mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0.3f);  // thicken glyphs
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f); // outline adds more thickness
            mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.white);
            hub._levelButtonText.fontMaterial = mat;


            // ===== NAV BAR =====
            NavBar(go, hub);

            hub.StartCoroutine(hub.Anim());
            return go;
        }

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

        // === WHITE BORDER ===
        static void WBorder(GameObject go, float alpha)
        {
            var b = R(go, "Brd");
            b.anchorMin = V(-0.03f,-0.03f); b.anchorMax = V(1.03f,1.03f);
            var i = b.gameObject.AddComponent<Image>();
            i.sprite = UIShapeUtils.WhiteRoundedRect(24, 68);
            i.type = Image.Type.Sliced;
            i.color = new Color(1,1,1,alpha);
            i.raycastTarget = false;
            b.transform.SetAsFirstSibling();
        }

        // === NAV BAR ===
        static void NavBar(GameObject root, HubScreen hub)
        {
            var bar = R(root, "Nav");
            bar.anchorMin = V(0,0); bar.anchorMax = V(1,0);
            bar.pivot = V(0.5f,0); bar.sizeDelta = V(0, 200);

            // Semi-transparent background
            var navBg = R(bar.gameObject, "NavBg");
            navBg.anchorMin = V(0, 0); navBg.anchorMax = V(1, 1);
            var navBgI = navBg.gameObject.AddComponent<Image>();
            navBgI.color = new Color(0.04f, 0.06f, 0.12f, 0.75f);
            navBgI.raycastTarget = false;
            navBg.transform.SetAsFirstSibling();

            // Top border line — purple accent #5533AA
            var navTopLine = R(bar.gameObject, "NavTopLine");
            navTopLine.anchorMin = V(0, 1); navTopLine.anchorMax = V(1, 1);
            navTopLine.pivot = V(0.5f, 1); navTopLine.sizeDelta = V(0, 3);
            var navTopLineI = navTopLine.gameObject.AddComponent<Image>();
            navTopLineI.color = new Color(0.333f, 0.2f, 0.667f, 1f);
            navTopLineI.raycastTarget = false;

            // GOLD TOP BORDER — thick shiny line
            var goldLine = R(bar.gameObject, "GoldBorder");
            goldLine.anchorMin = V(0, 1); goldLine.anchorMax = V(1, 1);
            goldLine.pivot = V(0.5f, 1); goldLine.sizeDelta = V(0, 6);
            var goldLineI = goldLine.gameObject.AddComponent<Image>();
            goldLineI.sprite = UIShapeUtils.RoundedRect(400, 6, 3,
                new Color(1f, 0.85f, 0.3f, 0.9f),
                new Color(1f, 0.7f, 0.15f, 0.7f));
            goldLineI.type = Image.Type.Simple;
            goldLineI.raycastTarget = false;

            // Gold glow behind border line
            var goldGlow = R(bar.gameObject, "GoldGlow");
            goldGlow.anchorMin = V(0.05f, 0.92f); goldGlow.anchorMax = V(0.95f, 1.05f);
            var goldGlowI = goldGlow.gameObject.AddComponent<Image>();
            goldGlowI.sprite = UIShapeUtils.Glow(128, new Color(1f, 0.8f, 0.2f, 0.15f), 0.5f);
            goldGlowI.raycastTarget = false;
            goldGlow.transform.SetAsFirstSibling();

            string[] lbl = {"Shop","Leaders","Home","Teams","Collect"};
            // Fallback text icons if PNGs not loaded
            string[] icoText = { "\u2B50", "\u265B", "\u2665", "\u2726", "\u2666" };
            // PNG icon names in Resources/Icons/
            string[] icoRes = { "Icons/flat_icon_shop", "Icons/flat_icon_leaderboard", "Icons/flat_icon_home", "Icons/flat_icon_teams", "Icons/flat_icon_collections" };
            Color[] clr = {
                new Color(1f, 0.8f, 0.2f),        // gold
                new Color(1f, 0.6f, 0.2f),         // orange
                new Color(0.6f, 0.92f, 1f),        // cyan
                new Color(0.4f, 1f, 0.55f),         // green
                new Color(1f, 0.5f, 0.7f)           // pink
            };

            // Use anchors (0-1) so all 5 fit evenly, no overflow
            float w = 1f / 5f;
            for (int i = 0; i < 5; i++)
            {
                NI(bar.gameObject, lbl[i], icoText[i], icoRes[i], clr[i],
                    V(w * i, 0), V(w * (i + 1), 1), i == 2, hub);
            }
        }

        static void NI(GameObject par, string label, string iconText, string iconRes, Color ic,
            Vector2 aMin, Vector2 aMax, bool act, HubScreen hub)
        {
            var item = R(par, $"N{label}");
            item.anchorMin = aMin; item.anchorMax = aMax;
            // Active icon raised UP (popped out of bar), inactive stays normal
            item.offsetMin = V(4, act ? 35 : 16);
            item.offsetMax = V(-4, act ? 10 : -8);

            // Icon container — HUGE, fills almost entire item
            var ibg = R(item.gameObject, "IBg");
            ibg.anchorMin = V(0f, 0.08f); ibg.anchorMax = V(1f, 0.98f);

            // NO background color — just the PNG icon directly
            // Active glow behind icon
            if (act)
            {
                var gl = R(item.gameObject, "Gl");
                gl.anchorMin = V(-0.1f, 0.2f); gl.anchorMax = V(1.1f, 0.95f);
                var glI = gl.gameObject.AddComponent<Image>();
                glI.sprite = UIShapeUtils.Glow(64, new Color(0.5f, 0.3f, 1f, 0.35f), 0.55f);
                glI.raycastTarget = false;
                gl.transform.SetAsFirstSibling();
                hub._navGlowImage = glI;
            }

            // ICON — load PNG, fallback to text
            var iconSprite = Resources.Load<Sprite>(iconRes);
            if (iconSprite == null)
            {
                // Try loading as Texture2D and create sprite
                var iconTex = Resources.Load<Texture2D>(iconRes);
                if (iconTex != null)
                    iconSprite = Sprite.Create(iconTex,
                        new Rect(0, 0, iconTex.width, iconTex.height),
                        V(0.5f, 0.5f));
            }

            if (iconSprite != null)
            {
                var icoImg = R(ibg.gameObject, "Ico");
                icoImg.anchorMin = V(0.05f, 0.05f); icoImg.anchorMax = V(0.95f, 0.95f);
                var icoImgC = icoImg.gameObject.AddComponent<Image>();
                icoImgC.sprite = iconSprite;
                icoImgC.preserveAspect = true;
                icoImgC.raycastTarget = false;
                // All icons show original colors — no dimming
            }
            else
            {
                // Fallback: text icon
                Txt(ibg.gameObject, $"<b>{iconText}</b>", 36, ic);
            }

            // No SOON badges, no labels — icons only

            // Active golden dot
            if (act)
            {
                var dot = R(item.gameObject, "Dot");
                dot.anchorMin = V(0.4f, 0.01f); dot.anchorMax = V(0.6f, 0.06f);
                var dI = dot.gameObject.AddComponent<Image>();
                dI.sprite = UIShapeUtils.WhiteCircle(16);
                dI.color = new Color(1, 0.85f, 0.3f); dI.raycastTarget = false;
            }
        }

        // === ANIMATION ===
        IEnumerator Anim()
        {
            while (true)
            {
                float t = Time.time;
                if (_playGlowImage != null)
                    _playGlowImage.color = new Color(1,1,1,
                        Mathf.Lerp(0.25f,0.6f,(Mathf.Sin(t*2f)+1)*0.5f));
                // Nav glow pulse (cubic-bezier-like easing)
                if (_navGlowImage != null)
                {
                    float navT = Mathf.Repeat(t * 0.5f, 1f); // 2s cycle
                    float ease = navT < 0.5f
                        ? 4f * navT * navT * navT  // ease in
                        : 1f - Mathf.Pow(-2f * navT + 2f, 3f) / 2f; // ease out
                    float navA = Mathf.Lerp(0.15f, 0.45f, ease);
                    _navGlowImage.color = new Color(0.5f, 0.3f, 1f, navA);
                }
                yield return null;
            }
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
            t.richText = true; // enable <b> tags
            t.fontStyle = FontStyles.Bold;
            t.fontWeight = FontWeight.Heavy;
            t.outlineWidth = 0.25f; // extra thickness
            t.outlineColor = new Color32(
                (byte)(c.r*255), (byte)(c.g*255), (byte)(c.b*255), (byte)(c.a*200));
            return t;
        }

        // === BAKE GRADIENT INTO PILL SHAPE (no white overlay needed) ===
        static Sprite BakeGradientPill(int w, int h, int radius, Color c0, Color c1, Color c2, Color c3)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float r = Mathf.Min(radius, Mathf.Min(w, h) / 2f);
            float se = 3f; // soft edge

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1); // 0=bottom, 1=top
                // 4-stop gradient: c3(0%) → c2(40%) → c1(70%) → c0(100%)
                Color rowColor;
                if      (t > 0.7f) rowColor = Color.Lerp(c1, c0, (t - 0.7f) / 0.3f);
                else if (t > 0.4f) rowColor = Color.Lerp(c2, c1, (t - 0.4f) / 0.3f);
                else               rowColor = Color.Lerp(c3, c2, t / 0.4f);

                for (int x = 0; x < w; x++)
                {
                    // SDF for rounded rect
                    float cx = Mathf.Max(Mathf.Abs(x - (w-1)/2f) - ((w-1)/2f - r), 0f);
                    float cy = Mathf.Max(Mathf.Abs(y - (h-1)/2f) - ((h-1)/2f - r), 0f);
                    float dist = Mathf.Sqrt(cx*cx + cy*cy) - r;
                    float alpha = dist < -se ? 1f : (dist > 0f ? 0f : -dist / se);
                    tex.SetPixel(x, y, new Color(rowColor.r, rowColor.g, rowColor.b, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), 100f);
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
