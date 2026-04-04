using UnityEngine;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Constants and layout configuration for the roadmap screen.
    /// All values in world units (100 Pixels Per Unit at 1080×1920 reference).
    /// </summary>
    public static class RoadmapConfig
    {
        // --- Layout ---
        /// <summary>Vertical spacing between normal levels (world units).</summary>
        public const float NormalSpacing = 3.3f;
        /// <summary>Vertical spacing before/after boss levels (world units).</summary>
        public const float BossSpacing = 4.0f;
        /// <summary>Horizontal offset from center for left/right zigzag (world units).</summary>
        public const float ZigzagOffsetX = 2.6f;

        // --- Island Scales ---
        /// <summary>Scale for the current (active) level island.</summary>
        public const float CurrentLevelScale = 1.35f;
        /// <summary>Scale for locked normal islands.</summary>
        public const float LockedScale = 0.88f;
        /// <summary>Boss island base size in world units.</summary>
        public const float BossIslandSize = 3.8f;
        /// <summary>Normal island base size in world units.</summary>
        public const float NormalIslandSize = 2.6f;

        // --- Scroll ---
        /// <summary>World units below level 1 for scroll bottom bound.</summary>
        public const float ScrollPaddingBottom = -7.0f;
        /// <summary>World units above last unlocked level for scroll top bound.</summary>
        public const float ScrollPaddingTop = 2.0f;
        /// <summary>Momentum decay factor per frame (0-1, lower = faster stop).</summary>
        public const float ScrollDecay = 0.92f;
        /// <summary>Minimum velocity before stopping scroll (world units/sec).</summary>
        public const float ScrollMinVelocity = 0.01f;
        /// <summary>Duration of auto-scroll animation in seconds.</summary>
        public const float AutoScrollDuration = 0.8f;

        // --- Badge ---
        /// <summary>Badge offset below island center-bottom (world units).</summary>
        public const float BadgeOffsetY = -0.18f;
        /// <summary>Stars offset above badge (world units).</summary>
        public const float StarsOffsetY = 0.08f;

        // --- Stepping Stones ---
        /// <summary>Number of stones per path segment between islands.</summary>
        public const int StonesPerSegment = 5;

        // --- Object Pooling ---
        /// <summary>Number of screen heights beyond viewport to keep nodes active.</summary>
        public const float PoolingMarginScreens = 2.0f;

        // --- Colors ---
        public static readonly Color BadgeCompletedColor = new Color(0.76f, 0.16f, 0.66f, 1f);     // #C22AA8
        public static readonly Color BadgeBossColor = new Color(0.91f, 0.66f, 0f, 1f);              // #E8A800
        public static readonly Color BadgeCurrentColor = new Color(0.91f, 0.33f, 0.82f, 1f);        // #E855D0
        public static readonly Color BadgeLockedColor = new Color(0.42f, 0.47f, 0.54f, 1f);         // #6A7789
        public static readonly Color StarEarnedColor = new Color(1f, 0.84f, 0f, 1f);                // #FFD700
        public static readonly Color StarEmptyColor = new Color(0.33f, 0.38f, 0.46f, 0.5f);         // #556075 @ 50%
        public static readonly Color GlowColor = new Color(0.91f, 0.33f, 0.82f, 0.25f);             // purple glow
        public static readonly Color BossLockedTint = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        public static readonly Color StoneUnlockedColor = new Color(0.67f, 0.61f, 0.53f, 0.75f);
        public static readonly Color StoneLockedColor = new Color(0.31f, 0.39f, 0.47f, 0.5f);

        // --- Ocean Gradient ---
        public static readonly Color OceanTop = HexColor("#87CEEB");
        public static readonly Color OceanMidTop = HexColor("#4DD8FF");
        public static readonly Color OceanMid = HexColor("#0E94C8");
        public static readonly Color OceanMidBot = HexColor("#0A7DB0");
        public static readonly Color OceanBottom = HexColor("#064A68");

        // --- Header ---
        public static readonly Color HeaderBgColor = new Color(0.03f, 0.20f, 0.29f, 0.95f);         // rgba(8,50,75,0.95)

        // --- Resource Paths ---
        public const string IslandAPath = "Roadmap/Islands/island_A";
        public const string IslandBPath = "Roadmap/Islands/island_B";
        public const string IslandCPath = "Roadmap/Islands/island_C";
        public const string IslandDPath = "Roadmap/Islands/island_D";
        public const string IslandEPath = "Roadmap/Islands/island_E";
        public const string IslandLockedPath = "Roadmap/Islands/island_locked";

        /// <summary>
        /// Gets the island sprite for a given level number and state.
        /// Follows the A→B→D→E rotation pattern. Boss = C. Locked = island_locked.
        /// </summary>
        public static Sprite GetIslandSprite(int levelNumber, RoadmapLevelState state)
        {
            if (state == RoadmapLevelState.Locked && levelNumber % 10 != 0)
                return LoadSprite(IslandLockedPath);

            if (levelNumber % 10 == 0)
                return LoadSprite(IslandCPath);

            int variant = (levelNumber - 1) % 4;
            string path = variant switch
            {
                0 => IslandAPath,
                1 => IslandBPath,
                2 => IslandDPath,
                3 => IslandEPath,
                _ => IslandAPath
            };
            return LoadSprite(path);
        }

        /// <summary>
        /// Whether the island should be flipped horizontally for visual variety.
        /// Every other usage of the same variant gets flipped.
        /// </summary>
        public static bool ShouldFlip(int levelNumber)
        {
            if (levelNumber % 10 == 0) return false; // boss never flips
            int cyclePos = (levelNumber - 1) / 4;
            return cyclePos % 2 == 1;
        }

        /// <summary>
        /// Calculates the world position for a given level number.
        /// Odd levels go LEFT, even go RIGHT, boss goes CENTER.
        /// </summary>
        public static Vector2 GetNodePosition(int levelNumber)
        {
            bool isBoss = levelNumber % 10 == 0;
            float y = CalculateY(levelNumber);
            float x;

            if (isBoss)
            {
                x = 0f;
            }
            else
            {
                x = (levelNumber % 2 == 1) ? -ZigzagOffsetX : ZigzagOffsetX;
            }

            return new Vector2(x, y);
        }

        /// <summary>
        /// Calculates the cumulative Y position for a level based on spacing rules.
        /// </summary>
        private static float CalculateY(int levelNumber)
        {
            float y = 0f;
            for (int i = 2; i <= levelNumber; i++)
            {
                bool prevIsBoss = (i - 1) % 10 == 0;
                bool currIsBoss = i % 10 == 0;
                y += (prevIsBoss || currIsBoss) ? BossSpacing : NormalSpacing;
            }
            return y;
        }

        // --- Sprite Cache ---
        private static readonly System.Collections.Generic.Dictionary<string, Sprite> _spriteCache
            = new System.Collections.Generic.Dictionary<string, Sprite>();

        /// <summary>
        /// Loads a sprite from Resources with caching.
        /// Falls back to Texture2D → Sprite.Create if the asset isn't imported as Sprite type.
        /// </summary>
        public static Sprite LoadSprite(string resourcePath)
        {
            if (_spriteCache.TryGetValue(resourcePath, out var cached) && cached != null)
                return cached;

            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                var tex = Resources.Load<Texture2D>(resourcePath);
                if (tex != null)
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 100f);
            }

            if (sprite != null)
                _spriteCache[resourcePath] = sprite;

            return sprite;
        }

        private static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
