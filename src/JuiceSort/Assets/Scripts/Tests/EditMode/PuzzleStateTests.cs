using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class PuzzleStateTests
    {
        [Test]
        public void Constructor_CreatesStateWithCorrectContainerCount()
        {
            var containers = new[]
            {
                new ContainerData(4),
                new ContainerData(4),
                new ContainerData(4)
            };

            var state = new PuzzleState(containers);
            Assert.AreEqual(3, state.ContainerCount);
        }

        [Test]
        public void GetContainer_ReturnsCorrectContainer()
        {
            var container0 = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.MangoAmber
            });
            var container1 = new ContainerData(4);

            var state = new PuzzleState(new[] { container0, container1 });

            Assert.IsTrue(state.GetContainer(0).IsFull());
            Assert.IsTrue(state.GetContainer(1).IsEmpty());
        }

        [Test]
        public void IsAllSorted_AllContainersSorted_ReturnsTrue()
        {
            var containers = new[]
            {
                new ContainerData(new[]
                {
                    DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                    DrinkColor.MangoAmber, DrinkColor.MangoAmber
                }),
                new ContainerData(new[]
                {
                    DrinkColor.DeepBerry, DrinkColor.DeepBerry,
                    DrinkColor.DeepBerry, DrinkColor.DeepBerry
                }),
                new ContainerData(4) // empty = sorted
            };

            var state = new PuzzleState(containers);
            Assert.IsTrue(state.IsAllSorted());
        }

        [Test]
        public void IsAllSorted_OneUnsortedContainer_ReturnsFalse()
        {
            var containers = new[]
            {
                new ContainerData(new[]
                {
                    DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                    DrinkColor.MangoAmber, DrinkColor.MangoAmber
                }),
                new ContainerData(new[]
                {
                    DrinkColor.DeepBerry, DrinkColor.MangoAmber,
                    DrinkColor.DeepBerry, DrinkColor.None
                }),
                new ContainerData(4)
            };

            var state = new PuzzleState(containers);
            Assert.IsFalse(state.IsAllSorted());
        }

        [Test]
        public void IsAllSorted_AllEmpty_ReturnsTrue()
        {
            var containers = new[]
            {
                new ContainerData(4),
                new ContainerData(4)
            };

            var state = new PuzzleState(containers);
            Assert.IsTrue(state.IsAllSorted());
        }

        [Test]
        public void Clone_CreatesIndependentCopy()
        {
            var containers = new[]
            {
                new ContainerData(new[]
                {
                    DrinkColor.MangoAmber, DrinkColor.DeepBerry,
                    DrinkColor.None, DrinkColor.None
                }),
                new ContainerData(4)
            };

            var original = new PuzzleState(containers);
            var clone = original.Clone();

            // Verify clone has same data
            Assert.AreEqual(original.ContainerCount, clone.ContainerCount);
            Assert.AreEqual(
                original.GetContainer(0).GetSlot(0),
                clone.GetContainer(0).GetSlot(0));

            // Modify clone and verify original is unchanged
            clone.GetContainer(0).SetSlot(0, DrinkColor.TropicalTeal);
            Assert.AreEqual(DrinkColor.MangoAmber, original.GetContainer(0).GetSlot(0));
            Assert.AreEqual(DrinkColor.TropicalTeal, clone.GetContainer(0).GetSlot(0));
        }

        [Test]
        public void TestPuzzleData_CreateTestPuzzle_ReturnsSolvableState()
        {
            var puzzle = TestPuzzleData.CreateTestPuzzle();

            Assert.AreEqual(4, puzzle.ContainerCount);

            // First 3 containers should be full
            Assert.IsTrue(puzzle.GetContainer(0).IsFull());
            Assert.IsTrue(puzzle.GetContainer(1).IsFull());
            Assert.IsTrue(puzzle.GetContainer(2).IsFull());

            // Last container should be empty
            Assert.IsTrue(puzzle.GetContainer(3).IsEmpty());

            // Puzzle should NOT be solved initially
            Assert.IsFalse(puzzle.IsAllSorted());
        }

        [Test]
        public void TestPuzzleData_CreateTestPuzzle_HasFourDistinctColors()
        {
            var puzzle = TestPuzzleData.CreateTestPuzzle();
            var colors = new System.Collections.Generic.HashSet<DrinkColor>();

            for (int c = 0; c < puzzle.ContainerCount; c++)
            {
                var container = puzzle.GetContainer(c);
                for (int s = 0; s < container.SlotCount; s++)
                {
                    var color = container.GetSlot(s);
                    if (color != DrinkColor.None)
                        colors.Add(color);
                }
            }

            Assert.AreEqual(4, colors.Count, "Test puzzle should use exactly 4 distinct colors");
        }
    }
}
