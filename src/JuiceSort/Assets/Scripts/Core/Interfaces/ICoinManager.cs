using System;

namespace JuiceSort.Core
{
    /// <summary>
    /// Coin economy service interface. Manages coin balance for boosters (undo, extra bottle).
    /// </summary>
    public interface ICoinManager
    {
        /// <summary>Current coin balance.</summary>
        int GetBalance();

        /// <summary>Add coins to balance (level rewards, ad rewards). Triggers OnBalanceChanged and auto-save.</summary>
        void AddCoins(int amount);

        /// <summary>Spend coins if sufficient balance. Returns false if insufficient. Triggers OnBalanceChanged and auto-save on success.</summary>
        bool SpendCoins(int amount);

        /// <summary>Current consecutive win streak count.</summary>
        int StreakCount { get; }

        /// <summary>Increments win streak by 1. Does not auto-save — caller should trigger save (e.g., via AddCoins).</summary>
        void IncrementStreak();

        /// <summary>Resets win streak to 0 and auto-saves.</summary>
        void ResetStreak();

        /// <summary>Fired whenever balance changes. Parameter is the new balance.</summary>
        event Action<int> OnBalanceChanged;
    }
}
