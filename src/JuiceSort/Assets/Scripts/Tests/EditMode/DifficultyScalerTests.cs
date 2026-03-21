using NUnit.Framework;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class DifficultyScalerTests
    {
        [Test]
        public void Level1_MinimumParameters()
        {
            var def = DifficultyScaler.GetLevelDefinition(1);

            Assert.AreEqual(1, def.LevelNumber);
            Assert.AreEqual(3, def.ColorCount, "Level 1 should have 3 colors");
            Assert.AreEqual(4, def.SlotCount, "Level 1 should have 4 slots");
            Assert.GreaterOrEqual(def.ContainerCount, def.ColorCount + 1, "Should have more containers than colors");
            Assert.AreEqual(1, def.EmptyContainerCount);
        }

        [Test]
        public void Level20_ColorCountIncreases()
        {
            var def1 = DifficultyScaler.GetLevelDefinition(1);
            var def21 = DifficultyScaler.GetLevelDefinition(21);

            Assert.Greater(def21.ColorCount, def1.ColorCount, "Level 21 should have more colors than level 1");
        }

        [Test]
        public void Level100_SlotCountIncreases()
        {
            var def1 = DifficultyScaler.GetLevelDefinition(1);
            var def101 = DifficultyScaler.GetLevelDefinition(101);

            Assert.Greater(def101.SlotCount, def1.SlotCount, "Level 101 should have more slots than level 1");
        }

        [Test]
        public void Parameters_NeverBelowMinimums()
        {
            for (int level = 1; level <= 200; level++)
            {
                var def = DifficultyScaler.GetLevelDefinition(level);

                Assert.GreaterOrEqual(def.ColorCount, 3, $"Level {level} color count below minimum");
                Assert.GreaterOrEqual(def.SlotCount, 4, $"Level {level} slot count below minimum");
                Assert.GreaterOrEqual(def.ContainerCount, def.ColorCount + 1, $"Level {level} needs more containers than colors");
                Assert.GreaterOrEqual(def.EmptyContainerCount, 1, $"Level {level} needs at least 1 empty");
            }
        }

        [Test]
        public void Parameters_ClampedAtMaximums()
        {
            var def = DifficultyScaler.GetLevelDefinition(1000);

            Assert.LessOrEqual(def.ColorCount, 5, "Colors should not exceed 5 (DrinkColor enum limit)");
            Assert.LessOrEqual(def.SlotCount, 6, "Slots should not exceed 6");
            Assert.LessOrEqual(def.ContainerCount, 11, "Containers should not exceed max + empty");
        }

        [Test]
        public void ProgressiveIncrease_ParamsNeverDecrease()
        {
            var prev = DifficultyScaler.GetLevelDefinition(1);

            for (int level = 2; level <= 200; level++)
            {
                var curr = DifficultyScaler.GetLevelDefinition(level);

                Assert.GreaterOrEqual(curr.ColorCount, prev.ColorCount, $"Colors decreased at level {level}");
                Assert.GreaterOrEqual(curr.SlotCount, prev.SlotCount, $"Slots decreased at level {level}");
                Assert.GreaterOrEqual(curr.ContainerCount, prev.ContainerCount, $"Containers decreased at level {level}");

                prev = curr;
            }
        }

        [Test]
        public void Seed_EqualsLevelNumber()
        {
            var def = DifficultyScaler.GetLevelDefinition(42);
            Assert.AreEqual(42, def.Seed);
        }
    }
}
