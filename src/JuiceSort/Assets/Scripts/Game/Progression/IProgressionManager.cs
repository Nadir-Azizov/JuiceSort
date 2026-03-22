using System.Collections.Generic;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Progression tracking service interface.
    /// In Game assembly (not Core) because it references Game types (LevelRecord, LevelDefinition).
    /// </summary>
    public interface IProgressionManager
    {
        void CompleteLevelWithStars(int levelNumber, int stars, LevelDefinition definition);
        int GetStarRating(int levelNumber);
        int GetTotalStars();
        LevelRecord GetLevelRecord(int levelNumber);
        List<LevelRecord> GetAllLevelRecords();
        int CurrentLevel { get; }
        int HighestCompletedLevel { get; }
        bool IsLevelCompleted(int levelNumber);
        bool IsAtBatchGate(int levelNumber);
        bool CanPassBatchGate();
        int GetCurrentBatchStars();
        int GetBatchRequiredStars();
        bool SoundEnabled { get; set; }
        bool MusicEnabled { get; set; }

        /// <summary>
        /// Saves all progression + coin data to persistent storage.
        /// </summary>
        void AutoSave();
    }
}
