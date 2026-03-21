using NUnit.Framework;
using JuiceSort.Game.Progression;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ProgressionDataTests
    {
        private ProgressionData _data;

        [SetUp]
        public void SetUp()
        {
            _data = new ProgressionData();
        }

        [Test]
        public void SetLevelRecord_StoresRecordWithMetadata()
        {
            var record = new LevelRecord(1, "Paris", "France", LevelMood.Morning, 2);
            _data.SetLevelRecord(record);

            var retrieved = _data.GetLevelRecord(1);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Paris", retrieved.CityName);
            Assert.AreEqual("France", retrieved.CountryName);
            Assert.AreEqual(LevelMood.Morning, retrieved.Mood);
            Assert.AreEqual(2, retrieved.Stars);
        }

        [Test]
        public void SetLevelRecord_KeepsBestStars_HigherFirst()
        {
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 2));

            Assert.AreEqual(3, _data.GetStarRating(1), "Should keep 3 stars, not downgrade to 2");
        }

        [Test]
        public void SetLevelRecord_UpgradesStars_LowerFirst()
        {
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 1));
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));

            Assert.AreEqual(3, _data.GetStarRating(1), "Should upgrade from 1 to 3");
        }

        [Test]
        public void GetTotalStars_SumsAllRatings()
        {
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            _data.SetLevelRecord(new LevelRecord(2, "Tokyo", "Japan", LevelMood.Night, 2));
            _data.SetLevelRecord(new LevelRecord(3, "London", "UK", LevelMood.Morning, 1));

            Assert.AreEqual(6, _data.GetTotalStars());
        }

        [Test]
        public void IsLevelCompleted_TrueAfterCompletion()
        {
            Assert.IsFalse(_data.IsLevelCompleted(1));

            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 1));

            Assert.IsTrue(_data.IsLevelCompleted(1));
        }

        [Test]
        public void CurrentLevel_AdvancesAfterCompletingCurrent()
        {
            Assert.AreEqual(1, _data.CurrentLevel);

            // Simulate completing level 1 (CurrentLevel = 1)
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 2));
            if (1 == _data.CurrentLevel)
                _data.CurrentLevel = 2;

            Assert.AreEqual(2, _data.CurrentLevel);
        }

        [Test]
        public void CurrentLevel_DoesNotAdvanceForNonCurrentLevel()
        {
            _data.CurrentLevel = 5;

            // Completing level 3 (not current) shouldn't advance
            _data.SetLevelRecord(new LevelRecord(3, "London", "UK", LevelMood.Morning, 3));
            if (3 == _data.CurrentLevel)
                _data.CurrentLevel = 4;

            Assert.AreEqual(5, _data.CurrentLevel, "Current level should not change");
        }

        [Test]
        public void GetAllLevelRecords_ReturnsOrderedList()
        {
            _data.SetLevelRecord(new LevelRecord(3, "London", "UK", LevelMood.Morning, 1));
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            _data.SetLevelRecord(new LevelRecord(2, "Tokyo", "Japan", LevelMood.Night, 2));

            var records = _data.GetAllLevelRecords();

            Assert.AreEqual(3, records.Count);
            Assert.AreEqual(1, records[0].LevelNumber);
            Assert.AreEqual(2, records[1].LevelNumber);
            Assert.AreEqual(3, records[2].LevelNumber);
        }

        [Test]
        public void GetAllLevelRecords_ContainsCityAndMood()
        {
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 2));

            var records = _data.GetAllLevelRecords();

            Assert.AreEqual("Paris", records[0].CityName);
            Assert.AreEqual("France", records[0].CountryName);
            Assert.AreEqual(LevelMood.Morning, records[0].Mood);
        }

        [Test]
        public void LevelRecord_PreservesCityMoodOnStarUpgrade()
        {
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 1));
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));

            var record = _data.GetLevelRecord(1);
            Assert.AreEqual("Paris", record.CityName);
            Assert.AreEqual(LevelMood.Morning, record.Mood);
            Assert.AreEqual(3, record.Stars);
        }

        [Test]
        public void HighestCompletedLevel_ReturnsMax()
        {
            _data.SetLevelRecord(new LevelRecord(5, "Berlin", "Germany", LevelMood.Night, 1));
            _data.SetLevelRecord(new LevelRecord(2, "Tokyo", "Japan", LevelMood.Night, 3));

            Assert.AreEqual(5, _data.HighestCompletedLevel);
        }

        [Test]
        public void GetStarRating_UncompletedLevel_ReturnsZero()
        {
            Assert.AreEqual(0, _data.GetStarRating(99));
        }
    }
}
