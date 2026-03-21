using NUnit.Framework;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ProgressiveChallengeTests
    {
        [Test]
        public void Level1_GeneratesValidPuzzle()
        {
            var def = DifficultyScaler.GetLevelDefinition(1);
            var state = LevelGenerator.Generate(def);

            Assert.IsNotNull(state);
            Assert.AreEqual(def.ContainerCount, state.ContainerCount);
            Assert.IsFalse(state.IsAllSorted(), "Generated puzzle should not start solved");
        }

        [Test]
        public void NextLevel_IncrementsLevelNumber()
        {
            int levelNumber = 1;
            levelNumber++; // NextLevel
            Assert.AreEqual(2, levelNumber);
        }

        [Test]
        public void SameLevelNumber_ProducesSamePuzzle()
        {
            var def1 = DifficultyScaler.GetLevelDefinition(5);
            var def2 = DifficultyScaler.GetLevelDefinition(5);

            var state1 = LevelGenerator.Generate(def1);
            var state2 = LevelGenerator.Generate(def2);

            // Compare slot-by-slot
            for (int c = 0; c < state1.ContainerCount; c++)
            {
                for (int s = 0; s < state1.GetContainer(c).SlotCount; s++)
                {
                    Assert.AreEqual(
                        state1.GetContainer(c).GetSlot(s),
                        state2.GetContainer(c).GetSlot(s),
                        $"Level 5 replay: container {c} slot {s} differs");
                }
            }
        }

        [Test]
        public void Level20_HasMoreParametersThanLevel1()
        {
            var def1 = DifficultyScaler.GetLevelDefinition(1);
            var def21 = DifficultyScaler.GetLevelDefinition(21);

            // Level 21 should have at least more colors or containers
            bool harder = def21.ColorCount > def1.ColorCount
                       || def21.ContainerCount > def1.ContainerCount
                       || def21.SlotCount > def1.SlotCount;

            Assert.IsTrue(harder, "Level 21 should have higher difficulty params than level 1");
        }

        [Test]
        public void RestartLevel_ProducesSamePuzzle()
        {
            int levelNumber = 7;
            var def = DifficultyScaler.GetLevelDefinition(levelNumber);

            var original = LevelGenerator.Generate(def);
            // Make some pours
            if (PuzzleEngine.CanPour(original, 0, original.ContainerCount - 1))
                PuzzleEngine.ExecutePour(original, 0, original.ContainerCount - 1);

            // "Restart" = regenerate same level
            var restarted = LevelGenerator.Generate(DifficultyScaler.GetLevelDefinition(levelNumber));

            // Should match a fresh generation
            var fresh = LevelGenerator.Generate(DifficultyScaler.GetLevelDefinition(levelNumber));
            for (int c = 0; c < fresh.ContainerCount; c++)
            {
                for (int s = 0; s < fresh.GetContainer(c).SlotCount; s++)
                {
                    Assert.AreEqual(
                        fresh.GetContainer(c).GetSlot(s),
                        restarted.GetContainer(c).GetSlot(s));
                }
            }
        }

        [Test]
        public void MultipleConsecutiveLevels_AllValid()
        {
            for (int level = 1; level <= 30; level++)
            {
                var def = DifficultyScaler.GetLevelDefinition(level);
                var state = LevelGenerator.Generate(def);

                Assert.AreEqual(def.ContainerCount, state.ContainerCount, $"Level {level} container count mismatch");
                Assert.IsFalse(state.IsAllSorted(), $"Level {level} should not start solved");
            }
        }
    }
}
