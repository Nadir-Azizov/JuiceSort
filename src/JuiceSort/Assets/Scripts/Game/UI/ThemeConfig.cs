using UnityEngine;
using TMPro;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Game.UI
{
    /// <summary>
    /// Single source of truth for all visual constants.
    /// Provides mood-aware color retrieval for the Tropical Fresh aesthetic.
    /// </summary>
    public static class ThemeConfig
    {
        /// <summary>
        /// Current mood — set by GameplayManager when loading a level.
        /// Screens and components read this for mood-aware styling.
        /// </summary>
        public static LevelMood CurrentMood { get; set; } = LevelMood.Morning;

        // --- Font Size Hierarchy (at 1080×1920 reference) ---
        public const float FontSizeTitle = 72f;
        public const float FontSizeHeader = 38f;
        public const float FontSizeBody = 30f;
        public const float FontSizeSecondary = 22f;
        public const float FontSizeSmall = 18f;

        // --- Font Access ---
        private static TMP_FontAsset _fontRegular;

        public static TMP_FontAsset GetFont()
        {
            if (_fontRegular == null)
            {
                _fontRegular = Resources.Load<TMP_FontAsset>("Fonts/Nunito-Regular SDF");
                if (_fontRegular == null)
                {
                    Debug.LogWarning("[ThemeConfig] Font 'Fonts/Nunito-Regular SDF' not found. Using TMP default font. Generate SDF asset in Unity Editor.");
                    _fontRegular = TMP_Settings.defaultFontAsset;
                }
            }
            return _fontRegular;
        }

        /// <summary>
        /// Returns the same font asset as GetFont(). Use TMPro's fontStyle = FontStyles.Bold
        /// on the TextMeshProUGUI component for bold rendering (Nunito is a variable weight font).
        /// </summary>
        public static TMP_FontAsset GetFontBold()
        {
            return GetFont();
        }

        // --- Drink Colors (Tropical Fresh palette) ---

        public static Color GetDrinkColor(DrinkColor drinkColor)
        {
            switch (drinkColor)
            {
                case DrinkColor.MangoAmber:
                    return new Color(0.95f, 0.65f, 0.12f);   // warm golden-orange
                case DrinkColor.DeepBerry:
                    return new Color(0.55f, 0.08f, 0.35f);   // luxurious purple-red
                case DrinkColor.TropicalTeal:
                    return new Color(0.1f, 0.72f, 0.65f);    // ocean blue-green
                case DrinkColor.WatermelonRose:
                    return new Color(0.92f, 0.42f, 0.52f);   // warm soft pink
                case DrinkColor.LimeGold:
                    return new Color(0.6f, 0.82f, 0.15f);    // sunlit green
                case DrinkColor.None:
                default:
                    return new Color(0f, 0f, 0f, 0f);
            }
        }

        // --- Mood-Aware Colors ---

        public static Color GetColor(ThemeColorType type)
        {
            return GetColor(CurrentMood, type);
        }

        public static Color GetColor(LevelMood mood, ThemeColorType type)
        {
            if (mood == LevelMood.Morning)
                return GetMorningColor(type);
            else
                return GetNightColor(type);
        }

        private static Color GetMorningColor(ThemeColorType type)
        {
            switch (type)
            {
                case ThemeColorType.Background:
                    return new Color(0.95f, 0.85f, 0.6f);        // warm golden
                case ThemeColorType.ContainerIdle:
                    return new Color(0.55f, 0.48f, 0.38f, 0.7f); // darker container body
                case ThemeColorType.ContainerSelected:
                    return new Color(1.0f, 0.88f, 0.3f, 0.85f);  // golden glow
                case ThemeColorType.ButtonPrimary:
                    return new Color(0.25f, 0.6f, 0.35f, 0.92f); // fresh green
                case ThemeColorType.ButtonSecondary:
                    return new Color(0.5f, 0.42f, 0.3f, 0.88f);  // warm brown
                case ThemeColorType.TextPrimary:
                    return new Color(0.95f, 0.92f, 0.85f);       // warm white
                case ThemeColorType.TextSecondary:
                    return new Color(0.7f, 0.62f, 0.5f);         // warm muted
                case ThemeColorType.StarGold:
                    return new Color(1.0f, 0.82f, 0.15f);        // bright gold
                case ThemeColorType.EmptySlot:
                    return new Color(1f, 0.95f, 0.88f, 0.5f); // visible light warm
                case ThemeColorType.Overlay:
                    return new Color(0.08f, 0.05f, 0.02f, 0.78f); // warm dark
                default:
                    return Color.white;
            }
        }

        private static Color GetNightColor(ThemeColorType type)
        {
            switch (type)
            {
                case ThemeColorType.Background:
                    return new Color(0.1f, 0.12f, 0.22f);        // deep blue
                case ThemeColorType.ContainerIdle:
                    return new Color(0.2f, 0.22f, 0.35f, 0.7f); // darker container body
                case ThemeColorType.ContainerSelected:
                    return new Color(1.0f, 0.82f, 0.3f, 0.85f);  // golden glow (same)
                case ThemeColorType.ButtonPrimary:
                    return new Color(0.22f, 0.45f, 0.55f, 0.92f); // cool teal
                case ThemeColorType.ButtonSecondary:
                    return new Color(0.35f, 0.28f, 0.42f, 0.88f); // cool purple
                case ThemeColorType.TextPrimary:
                    return new Color(0.9f, 0.88f, 0.92f);        // cool white
                case ThemeColorType.TextSecondary:
                    return new Color(0.6f, 0.58f, 0.68f);        // cool muted
                case ThemeColorType.StarGold:
                    return new Color(1.0f, 0.78f, 0.2f);         // amber gold
                case ThemeColorType.EmptySlot:
                    return new Color(0.5f, 0.52f, 0.62f, 0.45f); // visible cool light
                case ThemeColorType.Overlay:
                    return new Color(0.02f, 0.03f, 0.08f, 0.82f); // cool dark
                default:
                    return Color.white;
            }
        }

        // --- Gradient helpers for backgrounds ---

        public static Color GetBackgroundGradientTop(LevelMood mood)
        {
            return mood == LevelMood.Morning
                ? new Color(0.95f, 0.78f, 0.45f)  // golden sunrise
                : new Color(0.08f, 0.1f, 0.25f);   // deep twilight
        }

        public static Color GetBackgroundGradientBottom(LevelMood mood)
        {
            return mood == LevelMood.Morning
                ? new Color(0.98f, 0.88f, 0.72f)  // soft peach
                : new Color(0.2f, 0.12f, 0.28f);   // warm purple
        }

        // --- Gradient Texture Helpers ---

        private static Sprite _cachedMorningGradient;
        private static Sprite _cachedNightGradient;

        public static Texture2D CreateGradientTexture(Color top, Color bottom, int height = 256)
        {
            var tex = new Texture2D(1, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Creates an uncached gradient sprite. Callers are responsible for destroying
        /// the returned sprite's texture (sprite.texture) when it is no longer needed
        /// to avoid texture memory leaks.
        /// </summary>
        public static Sprite CreateGradientSprite(Color top, Color bottom, int height = 128)
        {
            var tex = CreateGradientTexture(top, bottom, height);
            return Sprite.Create(tex, new Rect(0, 0, 1, height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Returns a cached gradient sprite for the given mood.
        /// Shared across all UI screens to avoid duplicate texture allocations.
        /// </summary>
        public static Sprite CreateGradientSprite(LevelMood mood)
        {
            if (mood == LevelMood.Morning)
            {
                if (_cachedMorningGradient == null)
                    _cachedMorningGradient = CreateGradientSprite(
                        GetBackgroundGradientTop(LevelMood.Morning),
                        GetBackgroundGradientBottom(LevelMood.Morning));
                return _cachedMorningGradient;
            }
            else
            {
                if (_cachedNightGradient == null)
                    _cachedNightGradient = CreateGradientSprite(
                        GetBackgroundGradientTop(LevelMood.Night),
                        GetBackgroundGradientBottom(LevelMood.Night));
                return _cachedNightGradient;
            }
        }
    }
}
