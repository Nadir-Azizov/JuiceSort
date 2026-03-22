using UnityEngine;

namespace JuiceSort.Game.Layout
{
    /// <summary>
    /// Pure static utility that calculates responsive bottle positions and scale.
    /// No MonoBehaviour dependencies — fully unit-testable.
    /// </summary>
    public static class ResponsiveLayoutManager
    {
        /// <summary>
        /// Calculate layout for the given bottle count and camera parameters.
        /// </summary>
        /// <param name="bottleCount">Total number of bottles to lay out.</param>
        /// <param name="camOrthoSize">Camera.main.orthographicSize (half-height in world units).</param>
        /// <param name="camAspect">Camera aspect ratio (width/height).</param>
        /// <param name="config">Layout configuration with margins, reserves, scale bounds.</param>
        /// <returns>BottleLayout with positions, scale, and row info.</returns>
        public static BottleLayout CalculateLayout(int bottleCount, float camOrthoSize, float camAspect, LayoutConfig config)
        {
            if (config == null)
                config = LayoutConfig.Default();

            var layout = new BottleLayout();

            if (bottleCount <= 0)
            {
                layout.Positions = new Vector3[0];
                layout.Scale = config.MaxScale;
                layout.RowCount = 0;
                layout.TopRowCount = 0;
                layout.BottomRowCount = 0;
                layout.BoardY = 0f;
                return layout;
            }

            float camHeight = camOrthoSize * 2f;
            float camWidth = camHeight * camAspect;

            // Usable area after reserves and margins
            float usableWidth = camWidth * (1f - 2f * config.HorizontalMargin);
            float usableHeight = camHeight - config.TopReserve - config.BottomReserve;

            // Determine row count
            bool twoRows = bottleCount >= config.RowThreshold;
            layout.RowCount = twoRows ? 2 : 1;

            // Row distribution: top row gets extra on odd count
            int topCount, bottomCount;
            if (twoRows)
            {
                topCount = (bottleCount + 1) / 2;
                bottomCount = bottleCount - topCount;
            }
            else
            {
                topCount = bottleCount;
                bottomCount = 0;
            }

            layout.TopRowCount = topCount;
            layout.BottomRowCount = bottomCount;

            // Calculate scale: fit widest row within usable width
            int widestRowCount = Mathf.Max(topCount, bottomCount);
            float scale = CalculateScale(widestRowCount, usableWidth, usableHeight, twoRows, config);
            layout.Scale = scale;

            // Bottle world dimensions at this scale
            float bottleW = config.BottleSpriteWidth * scale;
            float bottleH = config.BottleSpriteHeight * scale;

            // Calculate board Y: center the play area between reserves
            // Play area center is offset from camera center because reserves are asymmetric
            float playAreaBottom = -camOrthoSize + config.BottomReserve;
            float playAreaTop = camOrthoSize - config.TopReserve;
            float playAreaCenterY = (playAreaBottom + playAreaTop) / 2f;
            layout.BoardY = playAreaCenterY;

            // Build positions (local to board, so relative to BoardY)
            layout.Positions = new Vector3[bottleCount];

            if (twoRows)
            {
                // Row gap: split usable height between two rows
                float rowGap = Mathf.Max(bottleH * 0.2f, config.MinSpacing);
                float topRowY = (bottleH + rowGap) / 2f;
                float bottomRowY = -(bottleH + rowGap) / 2f;

                // Ensure rows fit in usable height
                float totalRowHeight = bottleH * 2f + rowGap;
                if (totalRowHeight > usableHeight)
                {
                    // Reduce gap to fit
                    rowGap = usableHeight - bottleH * 2f;
                    if (rowGap < 0f) rowGap = 0f;
                    topRowY = (bottleH + rowGap) / 2f;
                    bottomRowY = -(bottleH + rowGap) / 2f;
                }

                LayoutRow(layout.Positions, 0, topCount, topRowY, bottleW, config.MinSpacing);
                LayoutRow(layout.Positions, topCount, bottomCount, bottomRowY, bottleW, config.MinSpacing);
            }
            else
            {
                LayoutRow(layout.Positions, 0, topCount, 0f, bottleW, config.MinSpacing);
            }

            return layout;
        }

        private static float CalculateScale(int widestRowCount, float usableWidth, float usableHeight, bool twoRows, LayoutConfig config)
        {
            // Scale based on width: all bottles in widest row must fit
            // Total width = count * bottleW + (count-1) * minSpacing
            // bottleW = spriteWidth * scale
            // count * spriteWidth * scale + (count-1) * minSpacing <= usableWidth
            // scale <= (usableWidth - (count-1) * minSpacing) / (count * spriteWidth)
            float widthScale = float.MaxValue;
            if (widestRowCount > 0)
            {
                float availableForBottles = usableWidth - (widestRowCount - 1) * config.MinSpacing;
                if (availableForBottles > 0)
                    widthScale = availableForBottles / (widestRowCount * config.BottleSpriteWidth);
                else
                    widthScale = config.MinScale;
            }

            // Scale based on height: rows must fit in usable height
            float heightScale = float.MaxValue;
            if (twoRows)
            {
                // Two rows of bottles + gap must fit in usable height
                // 2 * spriteHeight * scale + minSpacing <= usableHeight
                float availableForBottles = usableHeight - config.MinSpacing;
                if (availableForBottles > 0)
                    heightScale = availableForBottles / (2f * config.BottleSpriteHeight);
                else
                    heightScale = config.MinScale;
            }
            else
            {
                // Single row: bottle height must fit in usable height
                if (usableHeight > 0)
                    heightScale = usableHeight / config.BottleSpriteHeight;
                else
                    heightScale = config.MinScale;
            }

            float scale = Mathf.Min(widthScale, heightScale);
            scale = Mathf.Clamp(scale, config.MinScale, config.MaxScale);
            return scale;
        }

        private static void LayoutRow(Vector3[] positions, int startIndex, int count, float rowY, float bottleWorldWidth, float minSpacing)
        {
            if (count <= 0) return;

            // Spacing between bottle centers
            float spacing = bottleWorldWidth + minSpacing;
            float totalWidth = (count - 1) * spacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                float x = startX + i * spacing;
                positions[startIndex + i] = new Vector3(x, rowY, 0f);
            }
        }
    }
}
