namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Calculates star rating based on move efficiency.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public static class StarCalculator
    {
        private const float ThreeStar = 1.2f;
        private const float TwoStar = 1.5f;

        /// <summary>
        /// Returns 1, 2, or 3 stars based on move count relative to estimated optimal.
        /// </summary>
        public static int CalculateStars(int moveCount, int estimatedOptimal)
        {
            if (estimatedOptimal <= 0)
                return 1;

            if (moveCount <= estimatedOptimal * ThreeStar)
                return 3;

            if (moveCount <= estimatedOptimal * TwoStar)
                return 2;

            return 1;
        }

        /// <summary>
        /// Returns star text with filled/empty symbols: "★★☆", "★☆☆", etc.
        /// </summary>
        public static string GetStarText(int stars)
        {
            switch (stars)
            {
                case 3: return "\u2605\u2605\u2605";
                case 2: return "\u2605\u2605\u2606";
                case 1: return "\u2605\u2606\u2606";
                default: return "\u2606\u2606\u2606";
            }
        }
    }
}
