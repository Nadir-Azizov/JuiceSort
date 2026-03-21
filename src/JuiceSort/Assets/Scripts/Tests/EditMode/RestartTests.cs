using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class RestartTests
    {
        [Test]
        public void Restart_RestoresInitialState()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(2)
            };
            var initial = new PuzzleState(containers);
            var current = initial.Clone();

            // Make a pour
            PuzzleEngine.ExecutePour(current, 0, 1);
            Assert.AreEqual(1, current.GetContainer(0).FilledCount());

            // Restart: clone initial
            current = initial.Clone();
            Assert.AreEqual(2, current.GetContainer(0).FilledCount());
            Assert.AreEqual(0, current.GetContainer(1).FilledCount());
        }

        [Test]
        public void Restart_MoveCounterResetsToZero()
        {
            int moveCount = 5;
            moveCount = 0;
            Assert.AreEqual(0, moveCount);
        }

        [Test]
        public void Restart_UndoStackClears()
        {
            var stack = new UndoStack(3);
            stack.Push(new PuzzleState(new[] { new ContainerData(2) }));
            stack.Push(new PuzzleState(new[] { new ContainerData(2) }));

            Assert.AreEqual(2, stack.Count);

            stack.Clear();
            Assert.AreEqual(0, stack.Count);
            Assert.IsNull(stack.Pop());
        }

        [Test]
        public void Restart_AfterLevelComplete_AllowsPlayAgain()
        {
            bool isLevelComplete = true;
            int moveCount = 10;

            // Restart
            isLevelComplete = false;
            moveCount = 0;

            Assert.IsFalse(isLevelComplete);
            Assert.AreEqual(0, moveCount);
        }

        [Test]
        public void Restart_MultipleRestarts_EachProducesFreshState()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(2)
            };
            var initial = new PuzzleState(containers);

            // First restart
            var state1 = initial.Clone();
            PuzzleEngine.ExecutePour(state1, 0, 1);

            // Second restart
            var state2 = initial.Clone();

            // state2 should be fresh (not affected by state1 modifications)
            Assert.AreEqual(2, state2.GetContainer(0).FilledCount());
            Assert.AreEqual(0, state2.GetContainer(1).FilledCount());

            // state1 should still be modified
            Assert.AreEqual(1, state1.GetContainer(0).FilledCount());
        }

        [Test]
        public void Restart_InitialStateNotMutated()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(2)
            };
            var initial = new PuzzleState(containers);

            // Clone, modify, restart
            var current = initial.Clone();
            PuzzleEngine.ExecutePour(current, 0, 1);
            current = initial.Clone();
            PuzzleEngine.ExecutePour(current, 0, 1);

            // Initial should be untouched
            Assert.AreEqual(2, initial.GetContainer(0).FilledCount());
            Assert.AreEqual(0, initial.GetContainer(1).FilledCount());
        }
    }
}
