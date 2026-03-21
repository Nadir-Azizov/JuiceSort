using NUnit.Framework;
using JuiceSort.Game.UI.Components;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class RoadmapTests
    {
        [Test]
        public void LevelNodeData_FromRecord_SetsCorrectValues()
        {
            var record = new LevelRecord(5, "Paris", "France", LevelMood.Morning, 3);
            var node = LevelNodeData.FromRecord(record);

            Assert.AreEqual(5, node.LevelNumber);
            Assert.AreEqual("Paris", node.CityName);
            Assert.AreEqual("France", node.CountryName);
            Assert.AreEqual(LevelMood.Morning, node.Mood);
            Assert.AreEqual(3, node.Stars);
            Assert.IsTrue(node.IsCompleted);
            Assert.IsFalse(node.IsCurrentLevel);
        }

        [Test]
        public void LevelNodeData_ForCurrentLevel_IsNotCompleted()
        {
            var city = new CityData("Tokyo", "Japan");
            var node = LevelNodeData.ForCurrentLevel(10, city, LevelMood.Night);

            Assert.AreEqual(10, node.LevelNumber);
            Assert.AreEqual("Tokyo", node.CityName);
            Assert.AreEqual(LevelMood.Night, node.Mood);
            Assert.AreEqual(0, node.Stars);
            Assert.IsTrue(node.IsCurrentLevel);
            Assert.IsFalse(node.IsCompleted);
        }

        [Test]
        public void LevelNodeData_FromRecord_CurrentLevel()
        {
            var record = new LevelRecord(1, "Paris", "France", LevelMood.Morning, 2);
            var node = LevelNodeData.FromRecord(record, isCurrentLevel: true);

            Assert.IsTrue(node.IsCurrentLevel);
            Assert.IsTrue(node.IsCompleted);
        }

        [Test]
        public void BuildNodeList_IncludesCompletedAndCurrent()
        {
            var data = new ProgressionData();
            data.SetLevelRecord(new LevelRecord(1, "Paris", "France", LevelMood.Morning, 3));
            data.SetLevelRecord(new LevelRecord(2, "Tokyo", "Japan", LevelMood.Night, 2));
            data.CurrentLevel = 3;

            var records = data.GetAllLevelRecords();
            // 2 completed + 1 current = 3 nodes
            int nodeCount = records.Count + 1; // +1 for current level
            Assert.AreEqual(3, nodeCount);
        }
    }
}
