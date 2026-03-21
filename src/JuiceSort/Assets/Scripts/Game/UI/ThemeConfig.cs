using UnityEngine;
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
    }
}
