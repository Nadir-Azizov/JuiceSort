using System.Collections.Generic;
using System.Linq;

namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Runtime progression state. Tracks completed levels with city/mood/star metadata.
    /// Plain C# — not serialized directly (SaveData handles persistence).
    /// </summary>
    public class ProgressionData
    {
        private readonly Dictionary<int, LevelRecord> _records = new Dictionary<int, LevelRecord>();

        public int CurrentLevel { get; set; } = 1;
        public bool SoundEnabled { get; set; } = true;
        public bool MusicEnabled { get; set; } = true;

        public void SetLevelRecord(LevelRecord record)
        {
            if (_records.TryGetValue(record.LevelNumber, out var existing))
            {
                existing.TryUpgradeStars(record.Stars);
            }
            else
            {
                _records[record.LevelNumber] = record;
            }
        }

        public int GetStarRating(int levelNumber)
        {
            return _records.TryGetValue(levelNumber, out var record) ? record.Stars : 0;
        }

        public int GetTotalStars()
        {
            int total = 0;
            foreach (var record in _records.Values)
                total += record.Stars;
            return total;
        }

        public LevelRecord GetLevelRecord(int levelNumber)
        {
            return _records.TryGetValue(levelNumber, out var record) ? record : null;
        }

        public List<LevelRecord> GetAllLevelRecords()
        {
            var list = new List<LevelRecord>(_records.Values);
            list.Sort((a, b) => a.LevelNumber.CompareTo(b.LevelNumber));
            return list;
        }

        public bool IsLevelCompleted(int levelNumber)
        {
            return _records.ContainsKey(levelNumber);
        }

        public int HighestCompletedLevel
        {
            get
            {
                int max = 0;
                foreach (var key in _records.Keys)
                {
                    if (key > max)
                        max = key;
                }
                return max;
            }
        }

        public int CompletedLevelCount => _records.Count;

        /// <summary>
        /// Returns total stars earned for levels in a specific batch.
        /// Batch 1 = levels 1-50, Batch 2 = levels 51-100, etc.
        /// </summary>
        public int GetBatchStarCount(int batchNumber, int levelsPerBatch)
        {
            int startLevel = (batchNumber - 1) * levelsPerBatch + 1;
            int endLevel = batchNumber * levelsPerBatch;
            int total = 0;

            for (int level = startLevel; level <= endLevel; level++)
            {
                total += GetStarRating(level);
            }
            return total;
        }
    }
}
