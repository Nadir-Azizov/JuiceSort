using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Game.UI.Components;

namespace JuiceSort.Game.UI.Screens
{
    /// <summary>
    /// PNG-based loading/splash screen shown on app launch before the Hub.
    /// Displays loading_background.png full-screen for a minimum duration,
    /// then signals readiness for transition to Hub.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        /// <summary>Set to true once the minimum display time has elapsed.</summary>
        public bool IsReady { get; private set; }

        private float _elapsed;
        private const float MinDisplaySeconds = 2f;

        void Update()
        {
            if (IsReady) return;
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= MinDisplaySeconds)
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

            // ===== FULL-SCREEN BACKGROUND (aspect-fill / cover) =====
            var bgGo = new GameObject("BG", typeof(RectTransform));
            bgGo.transform.SetParent(go.transform, false);
            var rt = bgGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var raw = bgGo.AddComponent<RawImage>();

            var bgTex = Resources.Load<Texture2D>("Backgrounds/loading_background");
            if (bgTex != null)
            {
                raw.texture = bgTex;
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
                raw.texture = tex;
            }

            bgGo.AddComponent<AspectFillScaler>();

            return go;
        }
    }
}
