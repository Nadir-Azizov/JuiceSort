using UnityEngine;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Manages gameplay background using a world-space SpriteRenderer.
    /// Positioned behind bottles (negative Z or low sorting order).
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        private SpriteRenderer _bgRenderer;

        public void SetBackground(string cityName, LevelMood mood)
        {
            float cityHueShift = GetCityHueShift(cityName);

            var topColor = ThemeConfig.GetBackgroundGradientTop(mood);
            var bottomColor = ThemeConfig.GetBackgroundGradientBottom(mood);

            topColor = ShiftHue(topColor, cityHueShift);
            bottomColor = ShiftHue(bottomColor, cityHueShift);

            _bgRenderer.color = Color.Lerp(topColor, bottomColor, 0.4f);
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

            // Create a large white sprite for background
            var tex = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

            var bgRenderer = go.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = sprite;
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

            return mgr;
        }
    }
}
