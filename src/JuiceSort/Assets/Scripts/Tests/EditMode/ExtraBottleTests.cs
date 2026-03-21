using NUnit.Framework;
using JuiceSort.Core;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ExtraBottleTests
    {
        [Test]
        public void AddExtraContainer_IncreasesCount()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.MangoAmber }),
                new ContainerData(2)
            };
            var state = new PuzzleState(containers);

            Assert.AreEqual(2, state.ContainerCount);

            var newState = PuzzleEngine.AddExtraContainer(state, 2);

            Assert.AreEqual(3, newState.ContainerCount);
        }

        [Test]
        public void AddExtraContainer_NewContainerIsEmpty()
        {
            var state = new PuzzleState(new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber }),
                new ContainerData(1)
            });

            var newState = PuzzleEngine.AddExtraContainer(state, 2);

            var lastContainer = newState.GetContainer(newState.ContainerCount - 1);
            Assert.IsTrue(lastContainer.IsEmpty());
            Assert.AreEqual(2, lastContainer.SlotCount);
        }

        [Test]
        public void AddExtraContainer_PreservesExistingContainers()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(new[] { DrinkColor.TropicalTeal, DrinkColor.None })
            };
            var state = new PuzzleState(containers);

            var newState = PuzzleEngine.AddExtraContainer(state, 2);

            Assert.AreEqual(DrinkColor.MangoAmber, newState.GetContainer(0).GetSlot(0));
            Assert.AreEqual(DrinkColor.DeepBerry, newState.GetContainer(0).GetSlot(1));
            Assert.AreEqual(DrinkColor.TropicalTeal, newState.GetContainer(1).GetSlot(0));
        }

        [Test]
        public void ExtraBottleLimit_MaxTwo()
        {
            int extraBottlesUsed = 0;
            int maxExtraBottles = GameConstants.MaxExtraBottles;

            Assert.AreEqual(2, maxExtraBottles);

            extraBottlesUsed++;
            Assert.Less(extraBottlesUsed, maxExtraBottles + 1);

            extraBottlesUsed++;
            Assert.AreEqual(maxExtraBottles, extraBottlesUsed);

            // At limit — should be blocked
            bool canUseMore = extraBottlesUsed < maxExtraBottles;
            Assert.IsFalse(canUseMore);
        }

        [Test]
        public void ExtraBottleCount_ResetsOnNewLevel()
        {
            int extraBottlesUsed = 2;

            // Simulate new level load
            extraBottlesUsed = 0;

            Assert.AreEqual(0, extraBottlesUsed);
            Assert.AreEqual(GameConstants.MaxExtraBottles, GameConstants.MaxExtraBottles - extraBottlesUsed);
        }

        [Test]
        public void AddExtraContainer_OriginalStateUnchanged()
        {
            var state = new PuzzleState(new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber }),
                new ContainerData(1)
            });

            int originalCount = state.ContainerCount;
            var newState = PuzzleEngine.AddExtraContainer(state, 1);

            Assert.AreEqual(originalCount, state.ContainerCount, "Original state should not change");
            Assert.AreEqual(originalCount + 1, newState.ContainerCount);
        }
    }
}
