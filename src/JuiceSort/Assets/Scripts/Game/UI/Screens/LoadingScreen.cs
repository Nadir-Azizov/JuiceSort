using UnityEngine;
using UnityEngine.UI;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// PNG-based loading/splash screen shown on app launch before the Hub.
    /// Displays loading_background.png full-screen as a bridge between
    /// Unity splash and Hub screen, then signals readiness for transition.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        /// <summary>Set to true once the screen is ready to transition away.</summary>
        public bool IsReady { get; private set; }

        void Start()
        {
            // No minimum delay — Unity splash already provides branded moment.
            // Screen stays visible until BootLoader transitions to Hub.
            IsReady = true;
        }

        public static GameObject Create()
        {
            var go = new GameObject("LoadingScreen");
            var cv = go.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            cv.sortingOrder = 100; // above everything during boot
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080f, 1920f);
            sc.matchWidthOrHeight = 0.5f;
            go.AddComponent<LoadingScreen>();

            // ===== FULL-SCREEN BACKGROUND IMAGE =====
            var bgGo = new GameObject("BG", typeof(RectTransform));
            bgGo.transform.SetParent(go.transform, false);
            var rt = bgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = bgGo.AddComponent<Image>();
            img.preserveAspect = false;

            // Try Sprite first, then Texture2D, then fallback gradient
            var bgSprite = Resources.Load<Sprite>("Backgrounds/loading_background");
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
            }
            else
            {
                var bgTex = Resources.Load<Texture2D>("Backgrounds/loading_background");
                if (bgTex != null)
                {
                    img.sprite = Sprite.Create(bgTex,
                        new Rect(0, 0, bgTex.width, bgTex.height),
                        new Vector2(0.5f, 0.5f));
                }
                else
                {
                    // Fallback: warm sunset gradient
                    var tex = new Texture2D(1, 256, TextureFormat.RGBA32, false);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.filterMode = FilterMode.Bilinear;
                    var top = new Color(0.95f, 0.55f, 0.25f);
                    var bot = new Color(0.85f, 0.25f, 0.35f);
                    for (int y = 0; y < 256; y++)
                        tex.SetPixel(0, y, Color.Lerp(bot, top, y / 255f));
                    tex.Apply();
                    img.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 256),
                        new Vector2(0.5f, 0.5f));
                }
            }
            img.type = Image.Type.Simple;

            return go;
        }
    }
}
