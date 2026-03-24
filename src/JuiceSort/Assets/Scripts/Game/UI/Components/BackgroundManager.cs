using UnityEngine;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Manages gameplay background using a world-space SpriteRenderer.
    /// Renders a smooth vertical gradient instead of a flat color.
    /// Positioned behind bottles (negative Z or low sorting order).
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        private SpriteRenderer _bgRenderer;
        private Texture2D _gradientTexture;
        private Sprite _gradientSprite;
        private Texture2D _initialTexture;
        private Sprite _initialSprite;

        public void SetBackground(string cityName, LevelMood mood)
        {
            if (_bgRenderer == null) return;

            float cityHueShift = GetCityHueShift(cityName);

            var topColor = ShiftHue(ThemeConfig.GetBackgroundGradientTop(mood), cityHueShift);
            var bottomColor = ShiftHue(ThemeConfig.GetBackgroundGradientBottom(mood), cityHueShift);

            // Destroy old texture and sprite to prevent leaks
            if (_gradientTexture != null)
                Destroy(_gradientTexture);
            if (_gradientSprite != null)
                Destroy(_gradientSprite);

            _gradientTexture = ThemeConfig.CreateGradientTexture(topColor, bottomColor);
            _gradientSprite = Sprite.Create(
                _gradientTexture,
                new Rect(0, 0, 1, _gradientTexture.height),
                new Vector2(0.5f, 0.5f),
                1f);

            _bgRenderer.sprite = _gradientSprite;
            _bgRenderer.color = Color.white; // Don't tint — gradient is baked into texture
        }

        private void OnDestroy()
        {
            if (_gradientTexture != null)
                Destroy(_gradientTexture);
            if (_gradientSprite != null)
                Destroy(_gradientSprite);
            if (_initialTexture != null)
                Destroy(_initialTexture);
            if (_initialSprite != null)
                Destroy(_initialSprite);
        }

        private float GetCityHueShift(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
                return 0f;

            int hash = 0;
            for (int i = 0; i < cityName.Length; i++)
                hash = hash * 31 + cityName[i];

            return (hash % 160 - 80) / 1000f;
        }

        private Color ShiftHue(Color color, float shift)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            h = Mathf.Repeat(h + shift, 1f);
            return Color.HSVToRGB(h, s, v);
        }

        public static BackgroundManager Create()
        {
            var go = new GameObject("BackgroundManager");

            // Create a minimal white sprite for initial display
            var initTex = new Texture2D(1, 1);
            initTex.SetPixel(0, 0, Color.white);
            initTex.Apply();
            var initSprite = Sprite.Create(initTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            var bgRenderer = go.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = initSprite;
            bgRenderer.color = ThemeConfig.GetColor(LevelMood.Morning, ThemeColorType.Background);
            bgRenderer.sortingOrder = -10; // behind everything

            // Scale to fill screen
            var cam = Camera.main;
            if (cam != null)
            {
                float camHeight = cam.orthographicSize * 2f;
                float camWidth = camHeight * Screen.width / Screen.height;
                go.transform.localScale = new Vector3(camWidth + 2f, camHeight + 2f, 1f);
                go.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 1f);
            }
            else
            {
                go.transform.localScale = new Vector3(20f, 20f, 1f);
                go.transform.position = new Vector3(0f, 0f, 1f);
            }

            var mgr = go.AddComponent<BackgroundManager>();
            mgr._bgRenderer = bgRenderer;
            mgr._initialTexture = initTex;
            mgr._initialSprite = initSprite;

            return mgr;
        }
    }
}
