namespace JuiceSort.Game.Layout
{
    /// <summary>
    /// Configuration for responsive bottle layout.
    /// Plain C# class with sensible defaults — can be promoted to ScriptableObject later.
    /// </summary>
    public class LayoutConfig
    {
        /// <summary>Fraction of camera width reserved as margin on each side (0.075 = 7.5%).</summary>
        public float HorizontalMargin { get; set; } = 0.075f;

        /// <summary>Bottle count at which layout switches from 1 row to 2 rows.</summary>
        public int RowThreshold { get; set; } = 7;

        /// <summary>World units reserved at the top of screen for HUD.</summary>
        public float TopReserve { get; set; } = 1.5f;

        /// <summary>World units reserved at the bottom of screen for HUD/buttons.</summary>
        public float BottomReserve { get; set; } = 2.0f;

        /// <summary>Minimum bottle scale (prevents bottles from becoming too tiny).</summary>
        public float MinScale { get; set; } = 0.10f;

        /// <summary>Maximum bottle scale (prevents bottles from becoming oversized).</summary>
        public float MaxScale { get; set; } = 0.20f;

        /// <summary>Minimum gap between bottle edges in world units.</summary>
        public float MinSpacing { get; set; } = 0.15f;

        /// <summary>Bottle sprite width at scale 1.0 (mask sprite ~510px at 100PPU = 5.1 units).</summary>
        public float BottleSpriteWidth { get; set; } = 5.1f;

        /// <summary>Bottle sprite height at scale 1.0 (mask sprite ~1017px at 100PPU = 10.17 units).</summary>
        public float BottleSpriteHeight { get; set; } = 10.17f;

        public static LayoutConfig Default()
        {
            return new LayoutConfig();
        }
    }
}
