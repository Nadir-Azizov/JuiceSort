using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.Economy
{
    /// <summary>
    /// Pure static utility that calculates coin rewards for level completion.
    /// Reward = base * difficulty multiplier + efficiency bonus (star-based).
    /// </summary>
    public static class CoinRewardCalculator
    {
        /// <summary>
        /// Calculate coin reward for completing a level.
        /// </summary>
        /// <param name="stars">Star rating earned (1, 2, or 3).</param>
        /// <param name="definition">Level definition with difficulty info.</param>
        /// <param name="config">Coin config with reward values.</param>
        /// <returns>Total coin reward (rounded down to int).</returns>
        public static int CalculateReward(int stars, LevelDefinition definition, CoinConfig config)
        {
            if (config == null)
                config = CoinConfig.Default();

            // Difficulty multiplier: scales with color count (3 colors = 1.0x, each extra color +0.15x)
            float difficultyMultiplier = 1.0f + (definition.ColorCount - 3) * 0.15f;
            if (difficultyMultiplier < 1.0f)
                difficultyMultiplier = 1.0f;

            float baseDifficultyReward = config.BaseLevelReward * difficultyMultiplier;

            // Efficiency bonus based on stars
            float efficiencyBonus = 0f;
            if (stars >= 3)
                efficiencyBonus = baseDifficultyReward * config.MoveEfficiencyBonusPercent;
            else if (stars == 2)
                efficiencyBonus = baseDifficultyReward * config.MoveEfficiencyBonusPercent * 0.5f;
            // 1 star = no bonus

            return (int)(baseDifficultyReward + efficiencyBonus);
        }
    }
}
