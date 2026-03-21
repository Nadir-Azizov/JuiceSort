using System;
using System.Collections.Generic;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;

namespace JuiceSort.Game.Save
{
    [Serializable]
    public class SaveData
    {
        public int currentLevel = 1;
        public SavedLevelRecord[] levelRecords = Array.Empty<SavedLevelRecord>();
        public bool soundEnabled = true;
        public bool musicEnabled = true;

        public static SaveData FromProgressionData(ProgressionData data)
        {
            var saveData = new SaveData();
            saveData.currentLevel = data.CurrentLevel;
            saveData.soundEnabled = data.SoundEnabled;
            saveData.musicEnabled = data.MusicEnabled;

            var records = data.GetAllLevelRecords();
            saveData.levelRecords = new SavedLevelRecord[records.Count];
            for (int i = 0; i < records.Count; i++)
            {
                saveData.levelRecords[i] = SavedLevelRecord.FromLevelRecord(records[i]);
            }

            return saveData;
        }

        public ProgressionData ToProgressionData()
        {
            var data = new ProgressionData();
            data.CurrentLevel = currentLevel;
            data.SoundEnabled = soundEnabled;
            data.MusicEnabled = musicEnabled;

            if (levelRecords != null)
            {
                foreach (var saved in levelRecords)
                {
                    data.SetLevelRecord(saved.ToLevelRecord());
                }
            }

            return data;
        }
    }

    [Serializable]
    public class SavedLevelRecord
    {
        public int levelNumber;
        public string cityName;
        public string countryName;
        public int mood; // 0 = Morning, 1 = Night
        public int stars;

        public static SavedLevelRecord FromLevelRecord(LevelRecord record)
        {
            return new SavedLevelRecord
            {
                levelNumber = record.LevelNumber,
                cityName = record.CityName,
                countryName = record.CountryName,
                mood = (int)record.Mood,
                stars = record.Stars
            };
        }

        public LevelRecord ToLevelRecord()
        {
            return new LevelRecord(levelNumber, cityName, countryName, (LevelMood)mood, stars);
        }
    }
}
