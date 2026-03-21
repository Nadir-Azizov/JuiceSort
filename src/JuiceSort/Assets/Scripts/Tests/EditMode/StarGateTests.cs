using NUnit.Framework;
using JuiceSort.Core;
using JuiceSort.Game.Progression;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class StarGateTests
    {
        private ProgressionData _data;

        [SetUp]
        public void SetUp()
        {
            _data = new ProgressionData();
        }

        [Test]
        public void IsAtBatchGate_Level50_IsGate()
        {
            Assert.IsTrue(50 % GameConstants.LevelsPerBatch == 0);
        }

        [Test]
        public void IsAtBatchGate_Level100_IsGate()
        {
            Assert.IsTrue(100 % GameConstants.LevelsPerBatch == 0);
        }

        [Test]
        public void IsAtBatchGate_Level25_NotGate()
        {
            Assert.IsFalse(25 % GameConstants.LevelsPerBatch == 0);
        }

        [Test]
        public void BatchRequiredStars_Is120()
        {
            int required = (int)(GameConstants.LevelsPerBatch * GameConstants.StarsPerLevel * GameConstants.StarGatePercent);
            Assert.AreEqual(120, required);
        }

        [Test]
        public void GetBatchStarCount_SumsCorrectly()
        {
            // Add some levels in batch 1 (levels 1-50)
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            _data.SetLevelRecord(new LevelRecord(2, "Tokyo", "Japan", LevelMood.Night, 2));
            _data.SetLevelRecord(new LevelRecord(3, "London", "UK", LevelMood.Morning, 1));

            Assert.AreEqual(6, _data.GetBatchStarCount(1, GameConstants.LevelsPerBatch));
        }

        [Test]
        public void GatePass_EnoughStars()
        {
            // Fill batch 1 with enough stars (120 needed)
            for (int i = 1; i <= 50; i++)
            {
                _data.SetLevelRecord(new LevelRecord(i, "City", "Country", LevelMood.Morning, 3));
            }

            int batchStars = _data.GetBatchStarCount(1, GameConstants.LevelsPerBatch);
            int required = (int)(GameConstants.LevelsPerBatch * GameConstants.StarsPerLevel * GameConstants.StarGatePercent);

            Assert.GreaterOrEqual(batchStars, required, "150 stars should pass 120 requirement");
        }

        [Test]
        public void GateBlock_InsufficientStars()
        {
            // Fill batch 1 with only 1 star each (50 stars total, need 120)
            for (int i = 1; i <= 50; i++)
            {
                _data.SetLevelRecord(new LevelRecord(i, "City", "Country", LevelMood.Morning, 1));
            }

            int batchStars = _data.GetBatchStarCount(1, GameConstants.LevelsPerBatch);
            int required = (int)(GameConstants.LevelsPerBatch * GameConstants.StarsPerLevel * GameConstants.StarGatePercent);

            Assert.Less(batchStars, required, "50 stars should fail 120 requirement");
        }

        [Test]
        public void GateRecheck_AfterImprovement_Passes()
        {
            // Start with 1 star each (50 total)
            for (int i = 1; i <= 50; i++)
            {
                _data.SetLevelRecord(new LevelRecord(i, "City", "Country", LevelMood.Morning, 1));
            }

            int required = (int)(GameConstants.LevelsPerBatch * GameConstants.StarsPerLevel * GameConstants.StarGatePercent);
            Assert.Less(_data.GetBatchStarCount(1, GameConstants.LevelsPerBatch), required);

            // Improve 35 levels from 1→3 (adds 70 stars: 50 + 70 = 120)
            for (int i = 1; i <= 35; i++)
            {
                _data.SetLevelRecord(new LevelRecord(i, "City", "Country", LevelMood.Morning, 3));
            }

            Assert.GreaterOrEqual(_data.GetBatchStarCount(1, GameConstants.LevelsPerBatch), required, "After improvement should pass gate");
        }

        [Test]
        public void GetBatchStarCount_Batch2_OnlyCountsBatch2Levels()
        {
            // Batch 1 levels
            _data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            // Batch 2 levels (51-100)
            _data.SetLevelRecord(new LevelRecord(51, "Dubai", "UAE", LevelMood.Night, 2));

            Assert.AreEqual(3, _data.GetBatchStarCount(1, GameConstants.LevelsPerBatch));
            Assert.AreEqual(2, _data.GetBatchStarCount(2, GameConstants.LevelsPerBatch));
        }
    }
}
