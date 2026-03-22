using UnityEngine;

namespace JuiceSort.Game.Economy
{
    /// <summary>
    /// Coin economy configuration. Plain C# class with defaults.
    /// BootLoader uses new GameObject + AddComponent (not prefab), so [SerializeField] won't populate.
    /// Can be promoted to ScriptableObject later when CoinManager moves to a prefab.
    /// </summary>
    public class CoinConfig
    {
        public int InitialBalance { get; set; } = 0;
        public int BaseLevelReward { get; set; } = 75;
        public float MoveEfficiencyBonusPercent { get; set; } = 0.25f;
        public int StreakBonus3 { get; set; } = 200;
        public int StreakBonus5 { get; set; } = 500;
        public int[] UndoCosts { get; set; } = { 100, 200, 300 };
        public int[] ExtraBottleCosts { get; set; } = { 500, 900 };
        public int AdRewardAmount { get; set; } = 250;

        /// <summary>
        /// Get undo cost for the Nth use in a level (0-indexed). Caps at last value.
        /// </summary>
        public int GetUndoCost(int useIndex)
        {
            if (UndoCosts == null || UndoCosts.Length == 0) return 100;
            return UndoCosts[Mathf.Min(useIndex, UndoCosts.Length - 1)];
        }

        /// <summary>
        /// Get extra bottle cost for the Nth use in a level (0-indexed). Caps at last value.
        /// </summary>
        public int GetExtraBottleCost(int useIndex)
        {
            if (ExtraBottleCosts == null || ExtraBottleCosts.Length == 0) return 500;
            return ExtraBottleCosts[Mathf.Min(useIndex, ExtraBottleCosts.Length - 1)];
        }

        public static CoinConfig Default()
        {
            return new CoinConfig();
        }
    }
}
