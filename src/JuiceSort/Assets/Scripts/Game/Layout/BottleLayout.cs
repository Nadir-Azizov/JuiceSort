using UnityEngine;

namespace JuiceSort.Game.Layout
{
    /// <summary>
    /// Result of a layout calculation — positions, scale, and row info for all bottles.
    /// </summary>
    public struct BottleLayout
    {
        /// <summary>World-space local positions for each bottle (relative to board center).</summary>
        public Vector3[] Positions;

        /// <summary>Uniform scale applied to all bottles.</summary>
        public float Scale;

        /// <summary>Number of rows (1 or 2).</summary>
        public int RowCount;

        /// <summary>Number of bottles in the top row.</summary>
        public int TopRowCount;

        /// <summary>Number of bottles in the bottom row (0 if single row).</summary>
        public int BottomRowCount;

        /// <summary>Y offset for the board transform (world-space).</summary>
        public float BoardY;
    }
}
