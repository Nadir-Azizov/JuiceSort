namespace JuiceSort.Game.Economy
{
    /// <summary>
    /// Pure logic for calculating streak bonus based on current streak count.
    /// </summary>
    public static class StreakBonusCalculator
    {
        public static int GetStreakBonus(int streakCount, CoinConfig config)
        {
            if (config == null)
                config = CoinConfig.Default();

            if (streakCount == 3)
                return config.StreakBonus3;
            if (streakCount == 5)
                return config.StreakBonus5;

            return 0;
        }
    }
}
