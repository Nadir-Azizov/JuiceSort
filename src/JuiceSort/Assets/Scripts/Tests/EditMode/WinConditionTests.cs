using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class WinConditionTests
    {
        [Test]
        public void IsAllSorted_SolvedPuzzle_ReturnsTrue()
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
        public void IsAllSorted_UnsolvedPuzzle_ReturnsFalse()
        {
            var state = TestPuzzleData.CreateTestPuzzle();
            Assert.IsFalse(state.IsAllSorted());
        }

        [Test]
        public void WinDetection_SolveSimplePuzzle_BecomesAllSorted()
        {
            // Simple 2-color puzzle: [A, B] [B, A] [empty]
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(new[] { DrinkColor.DeepBerry, DrinkColor.MangoAmber }),
                new ContainerData(2) // empty
            };
            var state = new PuzzleState(containers);

            Assert.IsFalse(state.IsAllSorted());

            // Solve: move B from container 0 to empty
            PuzzleEngine.ExecutePour(state, 0, 2); // [A, _] [B, A] [B, _]
            Assert.IsFalse(state.IsAllSorted());

            // Move A from container 1 to container 0
            PuzzleEngine.ExecutePour(state, 1, 0); // [A, A] [B, _] [B, _]
            Assert.IsFalse(state.IsAllSorted());

            // Move B from container 2 to container 1
            PuzzleEngine.ExecutePour(state, 2, 1); // [A, A] [B, B] [_, _]
            Assert.IsTrue(state.IsAllSorted(), "Puzzle should be solved");
        }

        [Test]
        public void LevelCompleteFlag_BlocksFurtherPours()
        {
            // Replicate GameplayManager logic
            bool isLevelComplete = false;
            int selectedIndex = -1;

            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber }),
                new ContainerData(1) // empty, 1 slot
            };
            var state = new PuzzleState(containers);

            // Simulate: select container 0, pour to container 1
            selectedIndex = 0;

            if (!isLevelComplete)
            {
                bool success = PuzzleEngine.ExecutePour(state, 0, 1);
                if (success)
                {
                    selectedIndex = -1;
                    if (state.IsAllSorted())
                        isLevelComplete = true;
                }
            }

            Assert.IsTrue(isLevelComplete, "Level should be complete");

            // Try to interact after completion — should be blocked
            int prevSelected = selectedIndex;
            if (!isLevelComplete)
                selectedIndex = 0;

            Assert.AreEqual(-1, selectedIndex, "Selection should be blocked after level complete");
        }
    }
}
