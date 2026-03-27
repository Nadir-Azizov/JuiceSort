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
        /// Destroys all cached textures and sprites. Call during scene cleanup or when
        /// procedural UI is fully torn down to reclaim GPU memory.
        /// </summary>
        public static void ClearCache()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.texture != null)
                        Object.Destroy(kvp.Value.texture);
                    Object.Destroy(kvp.Value);
                }
            }
            _cache.Clear();
        }

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
        /// Creates a radial gradient circle sprite with off-center highlight.
        /// Used for coin icons per §6.1.
        /// </summary>
        public static Sprite RadialGradientCircle(int size, Color centerColor, Color edgeColor,
            float centerX = 0.5f, float centerY = 0.5f)
        {
            string key = $"radgrad_{size}_{ColorKey(centerColor)}_{ColorKey(edgeColor)}_{centerX}_{centerY}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float cxPx = (size - 1) * centerX;
            float cyPx = (size - 1) * centerY;
            float radius = (size - 1) / 2f;
            // Max distance from the gradient center to any edge of the circle
            float maxDist = radius + Mathf.Sqrt(
                (cxPx - (size - 1) / 2f) * (cxPx - (size - 1) / 2f) +
                (cyPx - (size - 1) / 2f) * (cyPx - (size - 1) / 2f));

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Circle SDF for anti-aliasing
                    float dx = x - (size - 1) / 2f;
                    float dy = y - (size - 1) / 2f;
                    float circleDist = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    float circleAlpha = circleDist < -2f ? 1f : (circleDist > 0f ? 0f : Mathf.Clamp01(-circleDist / 2f));

                    // Radial gradient from offset center
                    float gx = x - cxPx;
                    float gy = y - cyPx;
                    float gradDist = Mathf.Sqrt(gx * gx + gy * gy);
                    float t = Mathf.Clamp01(gradDist / maxDist);
                    Color c = Color.Lerp(centerColor, edgeColor, t);

                    tex.SetPixel(x, y, new Color(c.r, c.g, c.b, c.a * circleAlpha));
                }
            }

            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Baked coin gradient per §6.1: 128×128, center #FFE866 at (38%, 32%), edge #F5A623.
        /// </summary>
        public static Sprite CoinGradient(int size = 128)
        {
            return RadialGradientCircle(size,
                new Color(1f, 0.91f, 0.40f),   // #FFE866
                new Color(0.96f, 0.65f, 0.14f), // #F5A623
                0.38f, 0.68f); // CSS (38%,32%) → texture Y is flipped: 1-0.32=0.68
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

        /// <summary>
        /// White gear icon sprite — 8-tooth gear with center hole.
        /// </summary>
        public static Sprite WhiteGear(int size = 128)
        {
            string key = $"gear_{size}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = (size - 1) / 2f;
            float outerR = size * 0.44f;
            float innerR = size * 0.30f;
            float holeR = size * 0.14f;
            int teeth = 8;
            float toothHalfAngle = Mathf.PI / teeth * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    float sectorAngle = Mathf.Repeat(angle + Mathf.PI, Mathf.PI * 2f / teeth) - Mathf.PI / teeth;
                    bool inTooth = Mathf.Abs(sectorAngle) < toothHalfAngle;
                    float edgeR = inTooth ? outerR : innerR;
                    float sdf = dist - edgeR;
                    float holeSdf = holeR - dist;
                    float combinedSdf = Mathf.Max(sdf, holeSdf);
                    float alpha = Mathf.Clamp01(-combinedSdf / 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// White circular arrow (restart) icon — thick open ring with bold arrowhead at top.
        /// </summary>
        public static Sprite WhiteRestartArrow(int size = 128)
        {
            string key = $"restart_v3_{size}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float ctr = (size - 1) / 2f;
            float ringR = size * 0.32f;
            float thick = size * 0.08f;
            // Gap at top (from ~330° to ~30° in standard math coords, i.e. top-right)
            // Ring goes clockwise from gapEnd to gapStart (most of the circle)
            float gapCenter = 90f * Mathf.Deg2Rad; // top
            float gapHalf = 35f * Mathf.Deg2Rad;
            float gapStart = gapCenter - gapHalf; // ~55°
            float gapEnd = gapCenter + gapHalf;   // ~125°
            // Arrowhead tip at the clockwise end of the arc (gapStart side = right-top)
            // Arrow points clockwise (tangent direction at gapStart)
            float tipAngle = gapStart;
            float tipX = ctr + Mathf.Cos(tipAngle) * ringR;
            float tipY = ctr + Mathf.Sin(tipAngle) * ringR;
            // Arrow direction: tangent pointing clockwise = perpendicular inward
            float arrowDir = tipAngle + Mathf.PI * 0.5f; // clockwise tangent
            float arrowLen = size * 0.22f;
            float arrowW = size * 0.18f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - ctr;
                    float dy = y - ctr;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    // Ring SDF — exclude gap region
                    float ringSdf = Mathf.Abs(dist - ringR) - thick;
                    // Normalize angle to check if in gap
                    float a = Mathf.Repeat(angle, Mathf.PI * 2f);
                    float gs = Mathf.Repeat(gapStart, Mathf.PI * 2f);
                    float ge = Mathf.Repeat(gapEnd, Mathf.PI * 2f);
                    bool inGap = (gs < ge) ? (a >= gs && a <= ge) : (a >= gs || a <= ge);
                    if (inGap) ringSdf = 999f;

                    // Arrowhead triangle — tip at arc end, pointing along tangent
                    float adx = x - tipX;
                    float ady = y - tipY;
                    float along = adx * Mathf.Cos(arrowDir) + ady * Mathf.Sin(arrowDir);
                    float across = Mathf.Abs(-adx * Mathf.Sin(arrowDir) + ady * Mathf.Cos(arrowDir));
                    float arrowSdf = (along < 0 || along > arrowLen)
                        ? 999f
                        : across - arrowW * (1f - along / arrowLen);

                    float sdf = Mathf.Min(ringSdf, arrowSdf);
                    float alpha = Mathf.Clamp01(-sdf / 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// White undo (back-curved arrow) icon — leftward curved arrow.
        /// </summary>
        public static Sprite WhiteUndoArrow(int size = 128)
        {
            string key = $"undo_{size}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float c = (size - 1) / 2f;
            float arcCenterX = c + size * 0.05f;
            float arcCenterY = c - size * 0.02f;
            float arcR = size * 0.28f;
            float thickness = size * 0.09f;
            // Arc from ~150° to ~390° (open on left side)
            float arcStart = 140f * Mathf.Deg2Rad;
            float arcEnd = 400f * Mathf.Deg2Rad;
            // Arrowhead at arc start (pointing left-up)
            float tipAngle = arcStart;
            float tipX = arcCenterX + Mathf.Cos(tipAngle) * arcR;
            float tipY = arcCenterY + Mathf.Sin(tipAngle) * arcR;
            float arrowLen = size * 0.20f;
            float arrowWidth = size * 0.16f;
            float arrowDir = tipAngle + Mathf.PI / 2f; // tangent direction

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - arcCenterX;
                    float dy = y - arcCenterY;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    float normAngle = Mathf.Repeat(angle - arcStart + Mathf.PI * 4f, Mathf.PI * 2f);
                    float arcSpan = arcEnd - arcStart;
                    bool inArc = normAngle <= arcSpan;
                    float arcSdf = inArc ? Mathf.Abs(dist - arcR) - thickness : 999f;
                    // Arrowhead
                    float adx = x - tipX;
                    float ady = y - tipY;
                    float along = adx * Mathf.Cos(arrowDir) + ady * Mathf.Sin(arrowDir);
                    float across = Mathf.Abs(-adx * Mathf.Sin(arrowDir) + ady * Mathf.Cos(arrowDir));
                    float arrowSdf = (along < 0 || along > arrowLen) ? 999f : across - arrowWidth * (1f - along / arrowLen);
                    float sdf = Mathf.Min(arcSdf, arrowSdf);
                    float alpha = Mathf.Clamp01(-sdf / 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// White vibration/phone icon — phone outline with motion lines.
        /// </summary>
        public static Sprite WhiteVibration(int size = 128)
        {
            string key = $"vibration_{size}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float c = (size - 1) / 2f;
            // Phone body — rounded rect in center
            float phoneW = size * 0.30f;
            float phoneH = size * 0.55f;
            float phoneR = size * 0.06f;
            float thick = size * 0.06f;
            // Motion arcs on left and right
            float arcR1 = size * 0.30f;
            float arcR2 = size * 0.40f;
            float arcThick = size * 0.05f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Phone outline SDF
                    float px = Mathf.Abs(x - c) - (phoneW / 2f - phoneR);
                    float py = Mathf.Abs(y - c) - (phoneH / 2f - phoneR);
                    float phoneDist = Mathf.Sqrt(Mathf.Max(px, 0f) * Mathf.Max(px, 0f) +
                                                  Mathf.Max(py, 0f) * Mathf.Max(py, 0f)) +
                                      Mathf.Min(Mathf.Max(px, py), 0f) - phoneR;
                    float phoneOutline = Mathf.Abs(phoneDist) - thick;

                    // Motion lines (arcs on left and right)
                    float dx = x - c;
                    float dy = y - c;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    // Left arc: angle around PI (150°-210°)
                    bool leftArc = Mathf.Abs(angle - Mathf.PI) < 0.52f || Mathf.Abs(angle + Mathf.PI) < 0.52f;
                    // Right arc: angle around 0 (-30°-30°)
                    bool rightArc = Mathf.Abs(angle) < 0.52f;

                    float arc1Sdf = (leftArc || rightArc) ? Mathf.Abs(dist - arcR1) - arcThick : 999f;
                    float arc2Sdf = (leftArc || rightArc) ? Mathf.Abs(dist - arcR2) - arcThick : 999f;

                    float sdf = Mathf.Min(phoneOutline, Mathf.Min(arc1Sdf, arc2Sdf));
                    float alpha = Mathf.Clamp01(-sdf / 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// White exit/door icon — door with arrow pointing out.
        /// </summary>
        public static Sprite WhiteExitDoor(int size = 128)
        {
            string key = $"exit_v4_{size}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float c = (size - 1) / 2f;
            float thick = size * 0.15f;
            // Door frame (left side open)
            float doorL = size * 0.15f;
            float doorR = size * 0.55f;
            float doorT = size * 0.82f;
            float doorB = size * 0.18f;
            // Arrow
            float arrowY = c;
            float arrowL = size * 0.45f;
            float arrowR = size * 0.88f;
            float arrowHeadLen = size * 0.15f;
            float arrowHeadW = size * 0.12f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float sdf = 999f;

                    // Door frame — 3 sides (top, right, bottom)
                    // Top bar
                    if (x >= doorL && x <= doorR)
                    {
                        float d = Mathf.Abs(y - doorT) - thick * 0.5f;
                        sdf = Mathf.Min(sdf, d);
                    }
                    // Bottom bar
                    if (x >= doorL && x <= doorR)
                    {
                        float d = Mathf.Abs(y - doorB) - thick * 0.5f;
                        sdf = Mathf.Min(sdf, d);
                    }
                    // Left bar (back wall)
                    if (y >= doorB && y <= doorT)
                    {
                        float d = Mathf.Abs(x - doorL) - thick * 0.5f;
                        sdf = Mathf.Min(sdf, d);
                    }

                    // Arrow shaft
                    if (x >= arrowL && x <= arrowR - arrowHeadLen)
                    {
                        float d = Mathf.Abs(y - arrowY) - thick * 0.4f;
                        sdf = Mathf.Min(sdf, d);
                    }
                    // Arrow head (triangle)
                    float ax = x - (arrowR - arrowHeadLen);
                    float ay = y - arrowY;
                    if (ax >= 0 && ax <= arrowHeadLen)
                    {
                        float halfW = arrowHeadW * (1f - ax / arrowHeadLen);
                        float d = Mathf.Abs(ay) - halfW;
                        sdf = Mathf.Min(sdf, d);
                    }

                    float alpha = Mathf.Clamp01(-sdf / 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            _cache[key] = sprite;
            return sprite;
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

        /// <summary>
        /// Baked beveled button sprite — frame + face + gradient in ONE texture.
        /// No overlapping layers = no defect lines.
        /// </summary>
        public static Sprite BeveledButton(int size, int outerRadius, int bevel,
            Color frameColor, Color faceTop, Color faceBottom)
        {
            string key = $"bevel_{size}_{outerRadius}_{bevel}_{ColorKey(frameColor)}_{ColorKey(faceTop)}_{ColorKey(faceBottom)}";
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float outerR = Mathf.Min(outerRadius, size / 2f);
            float innerR = Mathf.Max(outerR - bevel * 0.6f, 2f);

            for (int y = 0; y < size; y++)
            {
                float t = (float)y / Mathf.Max(1, size - 1);
                Color faceRow = Color.Lerp(faceBottom, faceTop, t);

                for (int x = 0; x < size; x++)
                {
                    // Outer rounded rect SDF
                    float outerDist = RoundedRectSDF(x, y, size, size, outerR);
                    float outerAlpha = outerDist < -2f ? 1f : (outerDist > 0f ? 0f : Mathf.Clamp01(-outerDist / 2f));

                    if (outerAlpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Inner face rect SDF (inset by bevel)
                    float hW = (size - 1) / 2f - bevel;
                    float hH = (size - 1) / 2f - bevel;
                    float cx = Mathf.Abs(x - (size - 1) / 2f) - (hW - innerR);
                    float cy = Mathf.Abs(y - (size - 1) / 2f) - (hH - innerR);
                    float innerDist = Mathf.Sqrt(Mathf.Max(cx, 0f) * Mathf.Max(cx, 0f) +
                                                  Mathf.Max(cy, 0f) * Mathf.Max(cy, 0f)) +
                                      Mathf.Min(Mathf.Max(cx, cy), 0f) - innerR;

                    Color pixel;
                    if (innerDist < -1.5f)
                    {
                        // Inside face — gradient
                        pixel = faceRow;
                    }
                    else if (innerDist > 0.5f)
                    {
                        // Frame area
                        pixel = frameColor;
                    }
                    else
                    {
                        // Anti-aliased edge between frame and face
                        float blend = Mathf.Clamp01((-innerDist + 0.5f) / 2f);
                        pixel = Color.Lerp(frameColor, faceRow, blend);
                    }

                    pixel.a *= outerAlpha;
                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            int border = Mathf.Min(outerRadius + bevel + 2, size / 2);
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            _cache[key] = sprite;
            return sprite;
        }

        private static string ColorKey(Color c)
        {
            return $"{Mathf.RoundToInt(c.r * 255)}_{Mathf.RoundToInt(c.g * 255)}_{Mathf.RoundToInt(c.b * 255)}_{Mathf.RoundToInt(c.a * 255)}";
        }
    }
}
