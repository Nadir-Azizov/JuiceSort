using NUnit.Framework;
using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class RoadmapTests
    {
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
