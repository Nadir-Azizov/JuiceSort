using System.Collections.Generic;
using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Estimates optimal move count for a level.
    /// Tries PuzzleSolver with increasing depth limits, falls back to scaled heuristic.
    /// Caches results per level number to avoid re-solving on replay.
    /// </summary>
    public static class OptimalMoveEstimator
    {
        private const int SolverDepthLimit = 50;
        private const int MaxCacheSize = 200;
        private static readonly Dictionary<int, int> _cache = new Dictionary<int, int>();

        /// <summary>
        /// Returns estimated optimal move count for the given level.
        /// </summary>
        public static int Estimate(int levelNumber, LevelDefinition definition)
        {
            if (_cache.TryGetValue(levelNumber, out int cached))
                return cached;

            int estimate = TrySolver(definition);

            // Evict oldest entries if cache grows too large (mobile memory)
            if (_cache.Count >= MaxCacheSize)
                _cache.Clear();

            _cache[levelNumber] = estimate;
            return estimate;
        }

        private static int TrySolver(LevelDefinition definition)
        {
            var state = LevelGenerator.Generate(definition);
            var result = PuzzleSolver.Solve(state, SolverDepthLimit);

            if (result.IsSolvable && result.MoveCount > 0)
                return result.MoveCount;

            // Scaled heuristic fallback: base estimate from puzzle complexity
            // Lower multiplier produces tighter thresholds so stars aren't given away
            int baseEstimate = definition.ColorCount * (definition.SlotCount - 1);
            int complexityBonus = (definition.ColorCount - 3) * 2;
            return baseEstimate + complexityBonus;
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
