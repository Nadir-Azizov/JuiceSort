using UnityEngine;
using System.Collections.Generic;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Generates procedural UI shape sprites at runtime.
    /// Rounded rectangles, circles, pills, and soft shadows — no external assets needed.
    /// All sprites are cached by key to avoid duplicate texture allocations.
    /// </summary>
    public static class UIShapeUtils
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Creates a rounded rectangle sprite with optional gradient and soft edges.
        /// </summary>
        /// <param name="width">Texture width in pixels</param>
        /// <param name="height">Texture height in pixels</param>
        /// <param name="radius">Corner radius in pixels</param>
        /// <param name="colorTop">Top gradient color</param>
        /// <param name="colorBottom">Bottom gradient color (same as top for solid)</param>
        /// <param name="softEdge">Pixel width of anti-aliased edge (0 = sharp)</param>
        public static Sprite RoundedRect(int width, int height, int radius,
            Color colorTop, Color colorBottom, float softEdge = 2f)
        {
            string key = $"rr_{width}_{height}_{radius}_{ColorKey(colorTop)}_{ColorKey(colorBottom)}_{softEdge}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float r = Mathf.Min(radius, Mathf.Min(width, height) / 2f);

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / Mathf.Max(1, height - 1);
                Color rowColor = Color.Lerp(colorBottom, colorTop, t);

                for (int x = 0; x < width; x++)
                {
                    float dist = RoundedRectSDF(x, y, width, height, r);
                    float alpha;

                    if (dist < -softEdge)
                        alpha = 1f;
                    else if (dist > 0f)
                        alpha = 0f;
                    else
                        alpha = Mathf.Clamp01(-dist / Mathf.Max(0.01f, softEdge));

                    tex.SetPixel(x, y, new Color(rowColor.r, rowColor.g, rowColor.b, rowColor.a * alpha));
                }
            }

            tex.Apply();

            // Use 9-slice borders so the sprite scales without distorting corners
            int border = Mathf.Min(radius + 2, Mathf.Min(width, height) / 2);
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));

            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Solid color rounded rectangle.
        /// </summary>
        public static Sprite RoundedRect(int width, int height, int radius, Color color, float softEdge = 2f)
        {
            return RoundedRect(width, height, radius, color, color, softEdge);
        }

        /// <summary>
        /// Creates a circle sprite.
        /// </summary>
        public static Sprite Circle(int diameter, Color color, float softEdge = 2f)
        {
            string key = $"circle_{diameter}_{ColorKey(color)}_{softEdge}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = (diameter - 1) / 2f;
            float r = center;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    float alpha;

                    if (dist < -softEdge)
                        alpha = 1f;
                    else if (dist > 0f)
                        alpha = 0f;
                    else
                        alpha = Mathf.Clamp01(-dist / Mathf.Max(0.01f, softEdge));

                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, diameter, diameter),
                new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Creates a soft glow/shadow sprite (radial gradient from center).
        /// </summary>
        public static Sprite Glow(int size, Color color, float falloff = 0.5f)
        {
            string key = $"glow_{size}_{ColorKey(color)}_{falloff}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = (size - 1) / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - Mathf.Pow(dist, falloff));

                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// White rounded rectangle for tinting via Image.color.
        /// Most efficient — one cached sprite, tint per-use.
        /// </summary>
        public static Sprite WhiteRoundedRect(int radius = 24, int size = 64)
        {
            return RoundedRect(size, size, radius, Color.white, Color.white, 2f);
        }

        /// <summary>
        /// White circle for tinting via Image.color.
        /// </summary>
        public static Sprite WhiteCircle(int diameter = 64)
        {
            return Circle(diameter, Color.white, 2f);
        }

        /// <summary>
        /// White pill shape (fully rounded ends).
        /// </summary>
        public static Sprite WhitePill(int width = 128, int height = 48)
        {
            int radius = height / 2;
            return RoundedRect(width, height, radius, Color.white, Color.white, 2f);
        }

        // --- Signed Distance Function for rounded rectangle ---
        private static float RoundedRectSDF(int px, int py, int w, int h, float r)
        {
            // Distance from point to rounded rectangle boundary
            // Negative = inside, Positive = outside
            float cx = Mathf.Max(Mathf.Abs(px - (w - 1) / 2f) - ((w - 1) / 2f - r), 0f);
            float cy = Mathf.Max(Mathf.Abs(py - (h - 1) / 2f) - ((h - 1) / 2f - r), 0f);
            return Mathf.Sqrt(cx * cx + cy * cy) - r;
        }

        private static string ColorKey(Color c)
        {
            return $"{(int)(c.r * 255)}_{(int)(c.g * 255)}_{(int)(c.b * 255)}_{(int)(c.a * 255)}";
        }
    }
}
