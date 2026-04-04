using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// SpriteRenderer-based map content manager for the roadmap screen.
    /// Creates and manages islands, stepping stones, badges, stars, ocean background,
    /// and current-level glow. Handles object pooling for off-screen nodes.
    /// </summary>
    public class RoadmapMapView : MonoBehaviour
    {
        private Camera _camera;
        private Transform _mapRoot;
        private List<RoadmapNodeData> _nodes = new List<RoadmapNodeData>();

        // Island GameObjects indexed by level number
        private readonly Dictionary<int, GameObject> _islandGOs = new Dictionary<int, GameObject>();
        // Stone segment parents: key = lower level number, value = (GO, upper level number)
        private readonly Dictionary<int, (GameObject go, int upperLevel)> _stoneSegments
            = new Dictionary<int, (GameObject, int)>();

        // Runtime-generated textures (must be destroyed to avoid leaks)
        private Texture2D _oceanTexture;
        private Texture2D _circleTexture;
        private Texture2D _starTexture;
        private Sprite _circleSprite;
        private Sprite _starSprite;
        private Sprite _oceanSprite;

        // Stone PNG sprites (loaded from Resources)
        private Sprite[] _stoneSprites;
        private GameObject _oceanQuad;
        private GameObject _glowGO;

        // Cliff decoration container (scrolls with map at 1:1)
        private GameObject _cliffsParentGO;

        // Pooling state
        private float _poolingMargin;

        private const int RoadmapLayer = 8; // Layer slot 8 in TagManager
        private const string SortingLayerSky = "RoadmapSky";
        private const string SortingLayerStones = "RoadmapStones";
        private const string SortingLayerIslands = "RoadmapIslands";

        public void Initialize(Camera camera, Transform mapRoot)
        {
            _camera = camera;
            _mapRoot = mapRoot;
            _poolingMargin = _camera.orthographicSize * RoadmapConfig.PoolingMarginScreens;

            GenerateRuntimeTextures();
        }

        public void BuildMap(List<RoadmapNodeData> nodes)
        {
            ClearMap();
            _nodes = nodes;

            CreateOceanBackground();

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                CreateIslandNode(node);

                // Create stepping stones between consecutive islands
                if (i > 0)
                    CreateStoneSegment(_nodes[i - 1], node);
            }

            CreateCliffDecorations();
            UpdatePooling();
        }

        public void UpdatePooling()
        {
            if (_camera == null) return;

            float camY = _camera.transform.position.y;
            float margin = _camera.orthographicSize * RoadmapConfig.PoolingMarginScreens;

            foreach (var kvp in _islandGOs)
            {
                if (kvp.Value == null) continue;
                float nodeY = kvp.Value.transform.position.y;
                bool visible = Mathf.Abs(nodeY - camY) < margin;
                kvp.Value.SetActive(visible);
            }

            // Stone segments stay active if either connected island is in range
            foreach (var kvp in _stoneSegments)
            {
                var (segGO, upperLevel) = kvp.Value;
                if (segGO == null) continue;
                int lowerLevel = kvp.Key;

                float lowerY = float.MaxValue;
                float upperY = float.MaxValue;
                if (_islandGOs.TryGetValue(lowerLevel, out var lowerGO) && lowerGO != null)
                    lowerY = lowerGO.transform.position.y;
                if (_islandGOs.TryGetValue(upperLevel, out var upperGO) && upperGO != null)
                    upperY = upperGO.transform.position.y;

                bool visible = Mathf.Abs(lowerY - camY) < margin || Mathf.Abs(upperY - camY) < margin;
                segGO.SetActive(visible);
            }
        }

        public void UpdateOceanPosition()
        {
            // Ocean quad is now parented to camera — no manual repositioning needed
        }

        // Cliffs are parented to _mapRoot — they scroll at 1:1 with the camera, no parallax needed.

        private void GenerateRuntimeTextures()
        {
            // Circle texture for glow and badges
            _circleTexture = CreateCircleTexture(64);
            _circleSprite = Sprite.Create(_circleTexture,
                new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);

            // Star texture for badges
            _starTexture = CreateStarTexture(64);
            _starSprite = Sprite.Create(_starTexture,
                new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f);

            // Load 8 stone PNG sprites for path variety
            _stoneSprites = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                _stoneSprites[i] = RoadmapConfig.LoadSprite($"Roadmap/Stones/stone_{i + 1}");
                if (_stoneSprites[i] == null)
                {
                    Debug.LogWarning($"[RoadmapMapView] Stone sprite stone_{i + 1} not found, using fallback circle");
                    _stoneSprites[i] = _circleSprite;
                }
            }
        }

        private void CreateOceanBackground()
        {
            // Gradient texture: 8px wide, 256px tall. All columns identical.
            // 2px border on each side filled with edge color to prevent bilinear
            // filtering from blending with transparent pixels at sprite edges.
            const int texW = 8;
            const int texH = 256;
            _oceanTexture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            _oceanTexture.wrapMode = TextureWrapMode.Clamp;
            _oceanTexture.filterMode = FilterMode.Bilinear;

            Color[] stops = {
                RoadmapConfig.OceanBottom,   // 0% — bottom
                RoadmapConfig.OceanMidBot,   // 25%
                RoadmapConfig.OceanMid,      // 50%
                RoadmapConfig.OceanMidTop,   // 75%
                RoadmapConfig.OceanTop        // 100% — top
            };

            for (int y = 0; y < texH; y++)
            {
                float t = y / (float)(texH - 1);
                float segT = t * 4f;
                int seg = Mathf.Min((int)segT, 3);
                float localT = segT - seg;
                Color c = Color.Lerp(stops[seg], stops[seg + 1], localT);
                for (int x = 0; x < texW; x++)
                    _oceanTexture.SetPixel(x, y, c);
            }
            _oceanTexture.Apply();

            // Use full texture as sprite — no inset needed since all edge pixels
            // contain valid gradient colors (not transparent)
            _oceanSprite = Sprite.Create(_oceanTexture,
                new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), 100f);

            _oceanQuad = new GameObject("OceanBackground");
            _oceanQuad.layer = RoadmapLayer;
            _oceanQuad.transform.SetParent(_camera.transform, false);
            _oceanQuad.transform.localPosition = new Vector3(0f, 0f, 20f);

            var sr = _oceanQuad.AddComponent<SpriteRenderer>();
            sr.sprite = _oceanSprite;
            sr.sortingLayerName = SortingLayerSky;
            sr.sortingOrder = 0;
            sr.drawMode = SpriteDrawMode.Simple;

            // Scale to fill viewport + 40% padding (generous to guarantee no edge visible)
            float camHeight = _camera.orthographicSize * 2f;
            float aspect = _camera.aspect > 0.01f ? _camera.aspect : (float)Screen.width / Screen.height;
            float camWidth = camHeight * aspect;
            float spriteWorldW = _oceanSprite.bounds.size.x;
            float spriteWorldH = _oceanSprite.bounds.size.y;
            float scaleX = (camWidth / spriteWorldW) * 1.4f;
            float scaleY = (camHeight / spriteWorldH) * 1.4f;
            _oceanQuad.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        private void CreateIslandNode(RoadmapNodeData node)
        {
            var islandGO = new GameObject($"Island_{node.LevelNumber}");
            islandGO.layer = RoadmapLayer;
            islandGO.transform.SetParent(_mapRoot, false);
            islandGO.transform.position = new Vector3(node.WorldPosition.x, node.WorldPosition.y, 0f);

            // Island sprite
            var sr = islandGO.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = SortingLayerIslands;
            sr.sortingOrder = 0;

            if (node.IslandSprite != null)
            {
                sr.sprite = node.IslandSprite;
                sr.flipX = node.FlipX;
            }
            else
            {
                Debug.LogWarning($"[RoadmapMapView] Missing sprite for level {node.LevelNumber}");
                return;
            }

            // Scale: PNGs are larger than spec (1024px or 1254px), need corrective scale
            // to reach target world sizes (2.6, 3.8, 2.3 units at 100 PPU)
            float targetSize = node.IsBoss ? RoadmapConfig.BossIslandSize : RoadmapConfig.NormalIslandSize;
            if (node.State == RoadmapLevelState.Locked && !node.IsBoss)
                targetSize = 2.3f; // locked island native size

            float actualSize = sr.sprite.bounds.size.x; // actual world size from sprite at PPU
            float baseScale = targetSize / actualSize;

            float scale = baseScale;
            if (node.State == RoadmapLevelState.Current)
                scale *= RoadmapConfig.CurrentLevelScale;

            islandGO.transform.localScale = new Vector3(scale, scale, 1f);

            // Boss locked tint
            if (node.State == RoadmapLevelState.Locked && node.IsBoss)
                sr.color = RoadmapConfig.BossLockedTint;

            // Collider for tap detection (in local space, so use actual sprite bounds)
            var collider = islandGO.AddComponent<BoxCollider2D>();
            collider.size = sr.sprite.bounds.size;

            // Marker for tap identification
            var marker = islandGO.AddComponent<RoadmapIslandMarker>();
            marker.LevelNumber = node.LevelNumber;
            marker.State = node.State;

            // Current level glow
            if (node.State == RoadmapLevelState.Current)
                CreateGlow(islandGO.transform, node, scale);

            // WorldSpace Canvas for badge + stars
            CreateBadgeAndStars(islandGO.transform, node, targetSize, scale);

            _islandGOs[node.LevelNumber] = islandGO;
        }

        private void CreateGlow(Transform islandTransform, RoadmapNodeData node, float parentScale)
        {
            _glowGO = new GameObject("Glow");
            _glowGO.layer = RoadmapLayer;
            _glowGO.transform.SetParent(islandTransform, false);
            _glowGO.transform.localPosition = Vector3.zero;

            var sr = _glowGO.AddComponent<SpriteRenderer>();
            sr.sprite = _circleSprite;
            sr.sortingLayerName = SortingLayerIslands;
            sr.sortingOrder = -1; // behind island

            // Glow color with more visible alpha
            var glowColor = RoadmapConfig.GlowColor;
            glowColor.a = 0.85f;
            sr.color = glowColor;

            // Glow = 1.3x the target island size (in world units)
            float targetIslandSize = node.IsBoss ? RoadmapConfig.BossIslandSize : RoadmapConfig.NormalIslandSize;
            float glowWorldSize = targetIslandSize * RoadmapConfig.CurrentLevelScale * 1.3f;
            // Circle sprite is 64px at 100PPU = 0.64 world units
            // Parent GO is already at parentScale, so local scale = desired world size / (0.64 * parentScale)
            float localScale = glowWorldSize / (0.64f * parentScale);
            _glowGO.transform.localScale = new Vector3(localScale, localScale, 1f);
        }

        private void CreateBadgeAndStars(Transform islandTransform, RoadmapNodeData node, float islandSize, float parentScale)
        {
            // WorldSpace Canvas — child of island so it scrolls with it
            var canvasGO = new GameObject("BadgeCanvas");
            canvasGO.layer = RoadmapLayer;
            canvasGO.transform.SetParent(islandTransform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = SortingLayerIslands;
            canvas.sortingOrder = 10;

            // Canvas size 200x400 (height 400 so badge at y≈-148 is within bounds ±200)
            var canvasRect = canvasGO.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200f, 400f);
            // Compensate for parent scale so 100 canvas px = 1 world unit
            float canvasLocalScale = 0.01f / parentScale;
            canvasRect.localScale = new Vector3(canvasLocalScale, canvasLocalScale, 1f);
            canvasRect.localPosition = Vector3.zero;

            // Badge position: below island center-bottom edge
            // islandSize is the target world size (e.g., 2.6). Badge is BadgeOffsetY below the bottom edge.
            float halfIslandSize = islandSize / 2f;
            float badgeWorldY = -halfIslandSize + RoadmapConfig.BadgeOffsetY;
            // Convert world offset to canvas pixels: effective world scale = 0.01, so px = world / 0.01
            float badgePxY = badgeWorldY * 100f;

            bool isBoss = node.IsBoss;
            bool isCurrent = node.State == RoadmapLevelState.Current;
            float badgeSize = (isBoss || isCurrent) ? 90f : 80f;
            float fontSize = (isBoss || isCurrent) ? 38f : 32f;

            // Badge background — circular using runtime circle sprite
            var badgeGO = new GameObject("Badge");
            badgeGO.layer = RoadmapLayer;
            badgeGO.transform.SetParent(canvasGO.transform, false);
            var badgeRect = badgeGO.AddComponent<RectTransform>();
            badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);
            badgeRect.anchoredPosition = new Vector2(0f, badgePxY);

            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.sprite = _circleSprite; // runtime-generated white circle, tinted by color
            badgeImg.color = GetBadgeColor(node);

            // Border via Outline component on the circle
            Color borderColor = node.State == RoadmapLevelState.Locked
                ? new Color(1f, 1f, 1f, 0.12f)
                : new Color(1f, 1f, 1f, 0.5f);
            var outline = badgeGO.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(2f, 2f);

            // Drop shadow below badge
            var shadow = badgeGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(0f, -3f);

            // Badge text (level number or lock icon)
            var textGO = new GameObject("Text");
            textGO.layer = RoadmapLayer;
            textGO.transform.SetParent(badgeGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.font = ThemeConfig.GetFontBold();
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            tmp.text = node.LevelNumber.ToString();
            if (node.State == RoadmapLevelState.Locked)
            {
                tmp.color = new Color(1f, 1f, 1f, 0.45f);
            }

            // Stars (only for completed and current levels)
            if (node.State != RoadmapLevelState.Locked)
            {
                float starsWorldY = badgeWorldY + RoadmapConfig.StarsOffsetY;
                float starsPxY = starsWorldY * 100f;
                CreateStars(canvasGO.transform, node.StarsEarned, starsPxY);
            }
        }

        private void CreateStars(Transform canvasTransform, int starsEarned, float yPosition)
        {
            float starSize = 22f;
            float gap = 3f;
            float totalWidth = starSize * 3f + gap * 2f;
            float startX = -totalWidth / 2f + starSize / 2f;

            for (int i = 0; i < 3; i++)
            {
                var starGO = new GameObject($"Star_{i}");
                starGO.layer = RoadmapLayer;
                starGO.transform.SetParent(canvasTransform, false);
                var rect = starGO.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(starSize, starSize);
                rect.anchoredPosition = new Vector2(startX + i * (starSize + gap), yPosition);

                var img = starGO.AddComponent<Image>();
                img.sprite = _starSprite;
                img.color = i < starsEarned ? RoadmapConfig.StarEarnedColor : RoadmapConfig.StarEmptyColor;
            }
        }

        private Color GetBadgeColor(RoadmapNodeData node)
        {
            if (node.State == RoadmapLevelState.Locked)
                return RoadmapConfig.BadgeLockedColor;
            if (node.State == RoadmapLevelState.Current)
                return RoadmapConfig.BadgeCurrentColor;
            if (node.IsBoss)
                return RoadmapConfig.BadgeBossColor;
            return RoadmapConfig.BadgeCompletedColor;
        }

        private void CreateCliffDecorations()
        {
            if (_nodes.Count == 0 || _camera == null) return;

            var cliffsGO = new GameObject("CliffsParent");
            cliffsGO.layer = RoadmapLayer;
            cliffsGO.transform.SetParent(_mapRoot, false);
            _cliffsParentGO = cliffsGO;

            string[] cliffPaths = { "Roadmap/Decorations/cliff_left", "Roadmap/Decorations/cliff_right" };

            float mapBottom = _nodes[0].WorldPosition.y;
            float mapTop = _nodes[_nodes.Count - 1].WorldPosition.y;
            float screenEdgeX = _camera.orthographicSize * _camera.aspect;

            int index = 0;
            for (float y = mapBottom + 6f; y < mapTop; y += 16f)
            {
                bool isLeft = index % 2 == 0;
                string path = isLeft ? cliffPaths[0] : cliffPaths[1];
                var sprite = RoadmapConfig.LoadSprite(path);

                if (sprite == null)
                {
                    Debug.LogWarning($"[RoadmapMapView] Cliff sprite not found: {path}");
                    index++;
                    continue;
                }

                var cliffGO = new GameObject(isLeft ? "CliffLeft" : "CliffRight");
                cliffGO.layer = RoadmapLayer;
                cliffGO.transform.SetParent(cliffsGO.transform, false);

                var sr = cliffGO.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = SortingLayerStones;
                sr.sortingOrder = -5;

                // Position partially off-screen on edges
                float cliffX = isLeft ? (-screenEdgeX + 0.5f) : (screenEdgeX - 0.5f);
                cliffGO.transform.position = new Vector3(cliffX, y, 0f);

                // Scale to ~2.5 world units wide
                float targetWidth = 2.5f;
                float spriteWidth = sprite.bounds.size.x;
                float scale = targetWidth / spriteWidth;
                cliffGO.transform.localScale = new Vector3(scale, scale, 1f);

                sr.color = new Color(1f, 1f, 1f, 1f);

                index++;
            }
        }

        private void CreateStoneSegment(RoadmapNodeData lower, RoadmapNodeData upper)
        {
            var segGO = new GameObject($"Stones_{lower.LevelNumber}_{upper.LevelNumber}");
            segGO.layer = RoadmapLayer;
            segGO.transform.SetParent(_mapRoot, false);

            // Island half-sizes (radius from center to edge)
            float lowerHalf = lower.IsBoss ? RoadmapConfig.BossIslandSize / 2f : RoadmapConfig.NormalIslandSize / 2f;
            float upperHalf = upper.IsBoss ? RoadmapConfig.BossIslandSize / 2f : RoadmapConfig.NormalIslandSize / 2f;

            if (lower.State == RoadmapLevelState.Locked && !lower.IsBoss)
                lowerHalf = 2.3f / 2f;
            if (upper.State == RoadmapLevelState.Locked && !upper.IsBoss)
                upperHalf = 2.3f / 2f;

            // Direction vector from lower island center to upper island center
            Vector2 lowerCenter = lower.WorldPosition;
            Vector2 upperCenter = upper.WorldPosition;
            Vector2 dir = (upperCenter - lowerCenter).normalized;

            // P0: edge of lower island, pointing toward upper island
            Vector2 P0 = lowerCenter + dir * lowerHalf;
            // P3: edge of upper island, pointing toward lower island
            Vector2 P3 = upperCenter - dir * upperHalf;

            // Gentle arc control points
            Vector2 P1 = new Vector2(Mathf.Lerp(P0.x, P3.x, 0.33f), Mathf.Lerp(P0.y, P3.y, 0.28f));
            Vector2 P2 = new Vector2(Mathf.Lerp(P0.x, P3.x, 0.67f), Mathf.Lerp(P0.y, P3.y, 0.72f));

            // Determine tint based on whether the path is unlocked
            bool unlocked = lower.State != RoadmapLevelState.Locked && upper.State != RoadmapLevelState.Locked;
            Color stoneColor = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);

            // Fewer stones for boss paths (closer islands)
            bool isBossPath = (lower.LevelNumber % 10 == 0) || (upper.LevelNumber % 10 == 0);
            int stoneCount = isBossPath ? 3 : RoadmapConfig.StonesPerSegment;
            for (int i = 0; i < stoneCount; i++)
            {
                float t = 0.08f + (i / (float)(stoneCount - 1)) * 0.84f;
                Vector2 pos = CubicBezier(P0, P1, P2, P3, t);

                var stoneGO = new GameObject($"Stone_{i}");
                stoneGO.layer = RoadmapLayer;
                stoneGO.transform.SetParent(segGO.transform, false);
                stoneGO.transform.position = new Vector3(pos.x, pos.y, 0f);

                // Deterministic random for variant and scale
                var rng = new System.Random(lower.LevelNumber * 100 + i);
                int variantIndex = rng.Next(0, _stoneSprites.Length);

                var sr = stoneGO.AddComponent<SpriteRenderer>();
                sr.sprite = _stoneSprites[variantIndex];
                sr.sortingLayerName = SortingLayerStones;
                sr.sortingOrder = 0;
                sr.color = stoneColor;

                // Stone PNGs are 128px at 100 PPU = 1.28 world units native
                // Scale to ~0.38 with slight random variation (0.35 to 0.41)
                float scaleVar = 0.38f + (float)(rng.NextDouble() * 0.06 - 0.03);
                stoneGO.transform.localScale = new Vector3(scaleVar, scaleVar, 1f);
            }

            _stoneSegments[lower.LevelNumber] = (segGO, upper.LevelNumber);
        }

        private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            return uu * u * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + tt * t * p3;
        }

        private static Texture2D CreateCircleTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= radius)
                    {
                        float edge = Mathf.Clamp01((radius - dist) * 2f); // soft edge
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, edge));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateStarTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = size / 2f;
            float outerRadius = center - 2f;
            float innerRadius = outerRadius * 0.38f;

            // Pre-calculate star polygon vertices (5 outer + 5 inner = 10 points)
            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI / 2f + i * Mathf.PI / 5f; // start from top
                float r = (i % 2 == 0) ? outerRadius : innerRadius;
                points[i] = new Vector2(
                    center + r * Mathf.Cos(angle),
                    center + r * Mathf.Sin(angle)
                );
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (PointInPolygon(new Vector2(x, y), points))
                        tex.SetPixel(x, y, Color.white);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return tex;
        }

        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }

        public void ClearMap()
        {
            foreach (var kvp in _islandGOs)
            {
                if (kvp.Value != null)
                    Object.Destroy(kvp.Value);
            }
            _islandGOs.Clear();

            foreach (var kvp in _stoneSegments)
            {
                if (kvp.Value.go != null)
                    Object.Destroy(kvp.Value.go);
            }
            _stoneSegments.Clear();

            if (_cliffsParentGO != null)
            {
                Object.Destroy(_cliffsParentGO);
                _cliffsParentGO = null;
            }

            if (_oceanQuad != null)
            {
                Object.Destroy(_oceanQuad);
                _oceanQuad = null;
            }

            _glowGO = null;
        }

        private void OnDestroy()
        {
            ClearMap();

            if (_oceanTexture != null) Object.Destroy(_oceanTexture);
            if (_circleTexture != null) Object.Destroy(_circleTexture);
            if (_starTexture != null) Object.Destroy(_starTexture);
            if (_oceanSprite != null) Object.Destroy(_oceanSprite);
            if (_circleSprite != null) Object.Destroy(_circleSprite);
            if (_starSprite != null) Object.Destroy(_starSprite);
        }
    }
}
