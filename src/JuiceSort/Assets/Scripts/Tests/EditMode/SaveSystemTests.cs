using NUnit.Framework;
using UnityEngine;
using JuiceSort.Game.Save;
using JuiceSort.Game.Progression;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class SaveSystemTests
    {
        [Test]
        public void SaveData_JsonRoundTrip_PreservesValues()
        {
            var saveData = new SaveData
            {
                currentLevel = 5,
                levelRecords = new[]
                {
                    new SavedLevelRecord { levelNumber = 1, cityName = "Paris", countryName = "France", mood = 0, stars = 3 },
                    new SavedLevelRecord { levelNumber = 2, cityName = "Tokyo", countryName = "Japan", mood = 1, stars = 2 }
                },
                soundEnabled = true,
                musicEnabled = false
            };

            string json = JsonUtility.ToJson(saveData);
            var loaded = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(5, loaded.currentLevel);
            Assert.AreEqual(2, loaded.levelRecords.Length);
            Assert.AreEqual("Paris", loaded.levelRecords[0].cityName);
            Assert.AreEqual("Tokyo", loaded.levelRecords[1].cityName);
            Assert.AreEqual(3, loaded.levelRecords[0].stars);
            Assert.AreEqual(1, loaded.levelRecords[1].mood); // Night
            Assert.IsTrue(loaded.soundEnabled);
            Assert.IsFalse(loaded.musicEnabled);
        }

        [Test]
        public void SaveData_FromProgressionData_ConvertsCorrectly()
        {
            var data = new ProgressionData();
            data.CurrentLevel = 3;
            data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            data.SetLevelRecord(new LevelRecord(2, "Tokyo", "Japan", LevelMood.Night, 2));

            var saveData = SaveData.FromProgressionData(data);

            Assert.AreEqual(3, saveData.currentLevel);
            Assert.AreEqual(2, saveData.levelRecords.Length);
            Assert.AreEqual("Paris", saveData.levelRecords[0].cityName);
            Assert.AreEqual(0, saveData.levelRecords[0].mood); // Morning = 0
            Assert.AreEqual(1, saveData.levelRecords[1].mood); // Night = 1
        }

        [Test]
        public void SaveData_ToProgressionData_ConvertsCorrectly()
        {
            var saveData = new SaveData
            {
                currentLevel = 5,
                levelRecords = new[]
                {
                    new SavedLevelRecord { levelNumber = 1, cityName = "Paris", countryName = "France", mood = 0, stars = 3 },
                    new SavedLevelRecord { levelNumber = 4, cityName = "Rio", countryName = "Brazil", mood = 1, stars = 1 }
                }
            };

            var data = saveData.ToProgressionData();

            Assert.AreEqual(5, data.CurrentLevel);
            Assert.AreEqual(3, data.GetStarRating(1));
            Assert.AreEqual(1, data.GetStarRating(4));
            Assert.IsTrue(data.IsLevelCompleted(1));
            Assert.IsTrue(data.IsLevelCompleted(4));
            Assert.IsFalse(data.IsLevelCompleted(2));
            Assert.AreEqual(4, data.GetTotalStars());
        }

        [Test]
        public void SaveData_LevelRecords_PreserveCityMoodStars()
        {
            var data = new ProgressionData();
            data.SetLevelRecord(new LevelRecord(7, "Berlin", "Germany", LevelMood.Night, 2));

            var saveData = SaveData.FromProgressionData(data);
            string json = JsonUtility.ToJson(saveData);
            var loaded = JsonUtility.FromJson<SaveData>(json);
            var restored = loaded.ToProgressionData();

            var record = restored.GetLevelRecord(7);
            Assert.IsNotNull(record);
            Assert.AreEqual("Berlin", record.CityName);
            Assert.AreEqual("Germany", record.CountryName);
            Assert.AreEqual(LevelMood.Night, record.Mood);
            Assert.AreEqual(2, record.Stars);
        }

        [Test]
        public void SaveData_EmptyProgression_HandledCorrectly()
        {
            var data = new ProgressionData();
            var saveData = SaveData.FromProgressionData(data);

            Assert.AreEqual(1, saveData.currentLevel);
            Assert.AreEqual(0, saveData.levelRecords.Length);
        }

        [Test]
        public void SaveData_NullRecords_FreshStart()
        {
            var saveData = new SaveData { currentLevel = 1, levelRecords = null };
            var data = saveData.ToProgressionData();

            Assert.AreEqual(1, data.CurrentLevel);
            Assert.AreEqual(0, data.GetTotalStars());
        }

        [Test]
        public void SaveData_CoinAndStreak_SurviveJsonRoundTrip()
        {
            var data = new ProgressionData();
            data.CurrentLevel = 10;
            data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));

            var saveData = SaveData.FromProgressionData(data, coinBalance: 1500, consecutiveWinStreak: 4);

            string json = JsonUtility.ToJson(saveData);
            var loaded = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(1500, loaded.coinBalance, "Coin balance should survive JSON round-trip");
            Assert.AreEqual(4, loaded.consecutiveWinStreak, "Win streak should survive JSON round-trip");
            Assert.AreEqual(10, loaded.currentLevel);
            Assert.AreEqual(1, loaded.levelRecords.Length);
        }

        [Test]
        public void SaveData_SettingsFlags_SurviveJsonRoundTrip()
        {
            var data = new ProgressionData();
            data.SoundEnabled = false;
            data.MusicEnabled = false;

            var saveData = SaveData.FromProgressionData(data);
            string json = JsonUtility.ToJson(saveData);
            var loaded = JsonUtility.FromJson<SaveData>(json);
            var restored = loaded.ToProgressionData();

            Assert.IsFalse(restored.SoundEnabled, "SoundEnabled=false should survive round-trip");
            Assert.IsFalse(restored.MusicEnabled, "MusicEnabled=false should survive round-trip");
        }
    }
}
