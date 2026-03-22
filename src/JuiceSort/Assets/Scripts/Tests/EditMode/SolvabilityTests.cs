using NUnit.Framework;
using JuiceSort.Game.Puzzle;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class SolvabilityTests
    {
        [Test]
        public void TestPuzzle_IsSolvable()
        {
            var state = TestPuzzleData.CreateTestPuzzle();
            var result = PuzzleSolver.Solve(state);

            Assert.IsTrue(result.IsSolvable, "Hand-crafted test puzzle should be solvable");
            Assert.Greater(result.MoveCount, 0);
        }

        [Test]
        public void GeneratedLevel1_IsSolvable()
        {
            var def = DifficultyScaler.GetLevelDefinition(1);
            var state = LevelGenerator.Generate(def);
            var result = PuzzleSolver.Solve(state);

            Assert.IsTrue(result.IsSolvable, "Generated level 1 should be solvable");
        }

        [Test]
        public void GeneratedLevels_1_5_10_20_AllSolvable()
        {
            int[] levels = { 1, 5, 10, 20 };

            foreach (int level in levels)
            {
                var def = DifficultyScaler.GetLevelDefinition(level);
                var state = LevelGenerator.Generate(def);
                var result = PuzzleSolver.Solve(state);

                Assert.IsTrue(result.IsSolvable, $"Level {level} should be solvable");
                Assert.Greater(result.MoveCount, 0, $"Level {level} should require moves");
            }
        }

        [Test]
        public void Solver_ReturnsPositiveMoveCount()
        {
            var def = DifficultyScaler.GetLevelDefinition(3);
            var state = LevelGenerator.Generate(def);
            var result = PuzzleSolver.Solve(state);

            Assert.IsTrue(result.IsSolvable);
            Assert.Greater(result.MoveCount, 0, "Solvable puzzle should require at least 1 move");
        }

        [Test]
        public void Solver_IdentifiesUnsolvableState()
        {
            // Create an intentionally unsolvable puzzle:
            // Two containers, each with different colors, no empty space to maneuver
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(new[] { DrinkColor.DeepBerry, DrinkColor.MangoAmber })
            };
            var state = new PuzzleState(containers);

            var result = PuzzleSolver.Solve(state, maxDepth: 50);

            Assert.IsFalse(result.IsSolvable, "Puzzle with no empty space should be unsolvable");
        }

        [Test]
        public void AlreadySolvedPuzzle_ZeroMoves()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.MangoAmber }),
                new ContainerData(new[] { DrinkColor.DeepBerry, DrinkColor.DeepBerry }),
                new ContainerData(2)
            };
            var state = new PuzzleState(containers);

            var result = PuzzleSolver.Solve(state);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(0, result.MoveCount, "Already solved puzzle needs 0 moves");
        }

        [Test]
        public void BatchTest_20GeneratedPuzzles_AllSolvable()
        {
            for (int level = 1; level <= 20; level++)
            {
                var def = DifficultyScaler.GetLevelDefinition(level);
                var state = LevelGenerator.Generate(def);
                var result = PuzzleSolver.Solve(state);

                Assert.IsTrue(result.IsSolvable, $"Level {level} should be solvable (batch test)");
            }
        }

        [Test]
        public void HighDifficultyLevels_AllSolvable()
        {
            // Test across all difficulty tiers:
            // Level 41+ = 5 colors (max), Level 101+ = 5 slots, Level 201+ = 6 slots (max)
            int[] levels = { 30, 41, 50, 75, 101, 150 };

            foreach (int level in levels)
            {
                var def = DifficultyScaler.GetLevelDefinition(level);
                var state = LevelGenerator.Generate(def);
                var result = PuzzleSolver.Solve(state);

                Assert.IsTrue(result.IsSolvable, $"Level {level} (colors={def.ColorCount}, slots={def.SlotCount}) should be solvable");
            }
        }

        [Test]
        public void Generate_EnsuresRequiredEmptyContainers()
        {
            for (int level = 1; level <= 50; level++)
            {
                var def = DifficultyScaler.GetLevelDefinition(level);
                var state = LevelGenerator.Generate(def);

                int emptyCount = 0;
                for (int i = 0; i < state.ContainerCount; i++)
                {
                    if (state.GetContainer(i).IsEmpty())
                        emptyCount++;
                }

                Assert.GreaterOrEqual(emptyCount, def.EmptyContainerCount,
                    $"Level {level} should have at least {def.EmptyContainerCount} empty containers, got {emptyCount}");
            }
        }
    }
}
