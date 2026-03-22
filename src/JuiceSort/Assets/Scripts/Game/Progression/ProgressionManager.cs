using System.Collections.Generic;
using UnityEngine;
using JuiceSort.Core;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Save;

namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Manages player progression. Registered as IProgressionManager in Service Locator.
    /// Created programmatically by BootLoader.
    /// </summary>
    public class ProgressionManager : MonoBehaviour, IProgressionManager
    {
        private ProgressionData _data;

        public int CurrentLevel => _data.CurrentLevel;
        public int HighestCompletedLevel => _data.HighestCompletedLevel;

        public bool SoundEnabled
        {
            get => _data.SoundEnabled;
            set { _data.SoundEnabled = value; AutoSave(); }
        }

        public bool MusicEnabled
        {
            get => _data.MusicEnabled;
            set { _data.MusicEnabled = value; AutoSave(); }
        }

        private void Awake()
        {
            _data = new ProgressionData();
        }

        /// <summary>
        /// Loads saved progression. Called in Start to ensure SaveManager is registered first.
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
                        _data = saveData.ToProgressionData();
                        Debug.Log($"[ProgressionManager] Loaded save: level {_data.CurrentLevel}, {_data.GetTotalStars()} stars");
                        return;
                    }
                }
                Debug.LogWarning("[ProgressionManager] Save data corrupted, starting fresh.");
            }
        }

        public void CompleteLevelWithStars(int levelNumber, int stars, LevelDefinition definition)
        {
            var record = new LevelRecord(
                levelNumber,
                definition.CityName ?? "",
                definition.CountryName ?? "",
                definition.Mood,
                stars
            );

            _data.SetLevelRecord(record);

            // Advance current level if completing the current one
            if (levelNumber == _data.CurrentLevel)
            {
                _data.CurrentLevel = levelNumber + 1;
            }

            Debug.Log($"[ProgressionManager] Level {levelNumber} completed with {stars} stars. Total: {_data.GetTotalStars()} stars. Current: {_data.CurrentLevel}");

            // Auto-save
            AutoSave();
        }

        /// <summary>
        /// Saves all progression + coin data. Called internally and by CoinManager.
        /// </summary>
        public void AutoSave()
        {
            if (Services.TryGet<ISaveManager>(out var saveManager))
            {
                int coinBalance = 0;
                int streakCount = 0;
                if (Services.TryGet<ICoinManager>(out var coinMgr))
                {
                    coinBalance = coinMgr.GetBalance();
                    streakCount = coinMgr.StreakCount;
                }

                var saveData = SaveData.FromProgressionData(_data, coinBalance, streakCount);
                string json = JsonUtility.ToJson(saveData);
                saveManager.Save(json);
            }
        }

        public int GetStarRating(int levelNumber)
        {
            return _data.GetStarRating(levelNumber);
        }

        public int GetTotalStars()
        {
            return _data.GetTotalStars();
        }

        public LevelRecord GetLevelRecord(int levelNumber)
        {
            return _data.GetLevelRecord(levelNumber);
        }

        public List<LevelRecord> GetAllLevelRecords()
        {
            return _data.GetAllLevelRecords();
        }

        public bool IsLevelCompleted(int levelNumber)
        {
            return _data.IsLevelCompleted(levelNumber);
        }

        public bool IsAtBatchGate(int levelNumber)
        {
            return levelNumber % GameConstants.LevelsPerBatch == 0;
        }

        public bool CanPassBatchGate()
        {
            return GetCurrentBatchStars() >= GetBatchRequiredStars();
        }

        public int GetCurrentBatchStars()
        {
            int batchNumber = GetCurrentBatchNumber();
            return _data.GetBatchStarCount(batchNumber, GameConstants.LevelsPerBatch);
        }

        private int GetCurrentBatchNumber()
        {
            int batch = (_data.CurrentLevel - 1) / GameConstants.LevelsPerBatch;
            return batch < 1 ? 1 : batch;
        }

        public int GetBatchRequiredStars()
        {
            return (int)(GameConstants.LevelsPerBatch * GameConstants.StarsPerLevel * GameConstants.StarGatePercent);
        }

        /// <summary>
        /// Loads progression from existing data (used by save system).
        /// </summary>
        public void LoadFromData(ProgressionData data)
        {
            _data = data;
        }

        /// <summary>
        /// Returns current data for save system serialization.
        /// </summary>
        public ProgressionData GetData()
        {
            return _data;
        }
    }
}
