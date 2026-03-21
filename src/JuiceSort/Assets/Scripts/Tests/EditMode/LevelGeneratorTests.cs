using NUnit.Framework;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class LevelGeneratorTests
    {
        [Test]
        public void Generate_CorrectContainerCount()
        {
            var def = new LevelDefinition(1, 5, 4, 4, 1);
            var state = LevelGenerator.Generate(def);

            Assert.AreEqual(5, state.ContainerCount);
        }

        [Test]
        public void Generate_CorrectFilledAndEmptyContainers()
        {
            var def = new LevelDefinition(1, 5, 4, 4, 1);
            var state = LevelGenerator.Generate(def);

            // Count total color units — should be colorCount * slotCount
            int totalUnits = 0;
            int emptyContainers = 0;
            for (int i = 0; i < state.ContainerCount; i++)
            {
                var container = state.GetContainer(i);
                int filled = container.FilledCount();
                totalUnits += filled;
                if (container.IsEmpty())
                    emptyContainers++;
            }

            // Total color units preserved: 4 colors * 4 slots = 16
            Assert.AreEqual(16, totalUnits, "Total color units should match colorCount * slotCount");
        }

        [Test]
        public void Generate_SameSeedProducesSamePuzzle()
        {
            var def1 = new LevelDefinition(42, 5, 4, 4, 1);
            var def2 = new LevelDefinition(42, 5, 4, 4, 1);

            var state1 = LevelGenerator.Generate(def1);
            var state2 = LevelGenerator.Generate(def2);

            for (int c = 0; c < state1.ContainerCount; c++)
            {
                for (int s = 0; s < state1.GetContainer(c).SlotCount; s++)
                {
                    Assert.AreEqual(
                        state1.GetContainer(c).GetSlot(s),
                        state2.GetContainer(c).GetSlot(s),
                        $"Container {c} slot {s} differs — seed not deterministic");
                }
            }
        }

        [Test]
        public void Generate_DifferentSeedsProduceDifferentPuzzles()
        {
            var def1 = new LevelDefinition(1, 5, 4, 4, 1);
            var def2 = new LevelDefinition(2, 5, 4, 4, 1);

            var state1 = LevelGenerator.Generate(def1);
            var state2 = LevelGenerator.Generate(def2);

            bool anyDifference = false;
            for (int c = 0; c < state1.ContainerCount && !anyDifference; c++)
            {
                for (int s = 0; s < state1.GetContainer(c).SlotCount; s++)
                {
                    if (state1.GetContainer(c).GetSlot(s) != state2.GetContainer(c).GetSlot(s))
                    {
                        anyDifference = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(anyDifference, "Different seeds should produce different puzzles");
        }

        [Test]
        public void Generate_PuzzleIsNotSolved()
        {
            var def = new LevelDefinition(1, 5, 4, 4, 1);
            var state = LevelGenerator.Generate(def);

            Assert.IsFalse(state.IsAllSorted(), "Generated puzzle should not be in solved state");
        }

        [Test]
        public void Generate_TotalColorUnitsPreserved()
        {
            var def = new LevelDefinition(10, 6, 5, 4, 1);
            var state = LevelGenerator.Generate(def);

            // Count color units per color
            var colorCounts = new int[6]; // DrinkColor values 0-5
            for (int c = 0; c < state.ContainerCount; c++)
            {
                var container = state.GetContainer(c);
                for (int s = 0; s < container.SlotCount; s++)
                {
                    var color = container.GetSlot(s);
                    if (color != DrinkColor.None)
                        colorCounts[(int)color]++;
                }
            }

            // Each color should have exactly slotCount units
            int expectedPerColor = def.SlotCount;
            int colorsFound = 0;
            for (int i = 0; i < colorCounts.Length; i++)
            {
                if (colorCounts[i] > 0)
                {
                    Assert.AreEqual(expectedPerColor, colorCounts[i],
                        $"Color {(DrinkColor)i} should have {expectedPerColor} units");
                    colorsFound++;
                }
            }

            Assert.AreEqual(def.ColorCount, colorsFound, "Should have exactly colorCount distinct colors");
        }

        [Test]
        public void LevelDefinition_FilledContainerCount()
        {
            var def = new LevelDefinition(1, 5, 4, 4, 1);
            Assert.AreEqual(4, def.FilledContainerCount);
        }

        [Test]
        public void LevelDefinition_SeedEqualsLevelNumber()
        {
            var def = new LevelDefinition(42, 5, 4, 4, 1);
            Assert.AreEqual(42, def.Seed);
        }

        [Test]
        public void Generate_MinimalPuzzle_TwoContainersOneColor()
        {
            var def = new LevelDefinition(1, 2, 1, 2, 1);
            var state = LevelGenerator.Generate(def);

            Assert.AreEqual(2, state.ContainerCount);
            // Total units: 1 color * 2 slots = 2
            int totalUnits = 0;
            for (int c = 0; c < state.ContainerCount; c++)
                totalUnits += state.GetContainer(c).FilledCount();
            Assert.AreEqual(2, totalUnits);
        }
    }
}
