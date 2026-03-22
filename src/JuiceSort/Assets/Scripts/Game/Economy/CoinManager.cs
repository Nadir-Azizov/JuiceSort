using System;
using UnityEngine;
using JuiceSort.Core;
using JuiceSort.Game.Save;
using JuiceSort.Game.Progression;

namespace JuiceSort.Game.Economy
{
    /// <summary>
    /// Manages coin balance for boosters. Registered as ICoinManager in Service Locator.
    /// Saves via ProgressionManager to avoid race conditions with shared SaveData.
    /// </summary>
    public class CoinManager : MonoBehaviour, ICoinManager
    {
        private int _balance;
        private int _streakCount;
        private CoinConfig _config;

        public event Action<int> OnBalanceChanged;

        public CoinConfig Config => _config;
        public int StreakCount => _streakCount;

        private void Awake()
        {
            _config = CoinConfig.Default();
            _balance = _config.InitialBalance;
        }

        /// <summary>
        /// Loads coin balance from save. Called in Start to ensure SaveManager is available.
        /// </summary>
        private void Start()
        {
            if (Services.TryGet<ISaveManager>(out var saveManager) && saveManager.HasSave())
            {
                string json = saveManager.LoadJson();
                if (!string.IsNullOrEmpty(json))
                {
                    var saveData = JsonUtility.FromJson<SaveData>(json);
                    if (saveData != null)
                    {
                        _balance = saveData.coinBalance;
                        _streakCount = saveData.consecutiveWinStreak;
                        Debug.Log($"[CoinManager] Loaded balance: {_balance} coins, streak: {_streakCount}");
                        return;
                    }
                }
            }

            Debug.Log($"[CoinManager] No save found, starting with {_balance} coins");
        }

        public int GetBalance()
        {
            return _balance;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            _balance += amount;
            Debug.Log($"[CoinManager] Added {amount} coins. Balance: {_balance}");

            OnBalanceChanged?.Invoke(_balance);
            TriggerSave();
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;

            if (_balance < amount)
            {
                Debug.Log($"[CoinManager] Insufficient coins. Need {amount}, have {_balance}");
                return false;
            }

            _balance -= amount;
            Debug.Log($"[CoinManager] Spent {amount} coins. Balance: {_balance}");

            OnBalanceChanged?.Invoke(_balance);
            TriggerSave();
            return true;
        }

        public void IncrementStreak()
        {
            _streakCount++;
            Debug.Log($"[CoinManager] Streak incremented to {_streakCount}");
        }

        public void ResetStreak()
        {
            _streakCount = 0;
            Debug.Log("[CoinManager] Streak reset to 0");
            TriggerSave();
        }

        /// <summary>
        /// Saves via ProgressionManager to avoid race condition with shared SaveData JSON.
        /// </summary>
        private void TriggerSave()
        {
            if (Services.TryGet<IProgressionManager>(out var progression))
                progression.AutoSave();
        }
    }
}
