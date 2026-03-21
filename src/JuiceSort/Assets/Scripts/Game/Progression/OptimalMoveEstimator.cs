using System.Collections.Generic;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Estimates optimal move count for a level.
    /// Tries PuzzleSolver with tight depth limit, falls back to heuristic.
    /// Caches results per level number to avoid re-solving on replay.
    /// </summary>
    public static class OptimalMoveEstimator
    {
        private const int SolverDepthLimit = 30;
        private static readonly Dictionary<int, int> _cache = new Dictionary<int, int>();

        /// <summary>
        /// Returns estimated optimal move count for the given level.
        /// </summary>
        public static int Estimate(int levelNumber, LevelDefinition definition)
        {
            if (_cache.TryGetValue(levelNumber, out int cached))
                return cached;

            int estimate = TrySolver(levelNumber, definition);
            _cache[levelNumber] = estimate;
            return estimate;
        }

        private static int TrySolver(int levelNumber, LevelDefinition definition)
        {
            var state = LevelGenerator.Generate(definition);
            var result = PuzzleSolver.Solve(state, SolverDepthLimit);

            if (result.IsSolvable && result.MoveCount > 0)
                return result.MoveCount;

            // Heuristic fallback
            return definition.ColorCount * definition.SlotCount * 2;
        }

        /// <summary>
        /// Clears the cache. Call when difficulty config changes.
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
