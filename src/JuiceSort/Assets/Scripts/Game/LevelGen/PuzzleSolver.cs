using System.Collections.Generic;
using System.Text;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// DFS solver with depth limit for puzzle solvability verification.
    /// Finds *a* solution, not necessarily the optimal (shortest) one.
    /// Used for testing/verification — not called at runtime during gameplay.
    /// </summary>
    public static class PuzzleSolver
    {
        private const int DefaultMaxDepth = 200;

        public static SolveResult Solve(PuzzleState state, int maxDepth = DefaultMaxDepth)
        {
            var visited = new HashSet<string>();
            return DFS(state, 0, maxDepth, visited);
        }

        private static SolveResult DFS(PuzzleState state, int depth, int maxDepth, HashSet<string> visited)
        {
            if (state.IsAllSorted())
                return new SolveResult(true, depth);

            if (depth >= maxDepth)
                return new SolveResult(false, -1);

            string hash = HashState(state);
            if (visited.Contains(hash))
                return new SolveResult(false, -1);

            visited.Add(hash);

            int containerCount = state.ContainerCount;
            for (int source = 0; source < containerCount; source++)
            {
                if (state.GetContainer(source).IsEmpty())
                    continue;

                for (int target = 0; target < containerCount; target++)
                {
                    if (source == target)
                        continue;

                    if (!PuzzleEngine.CanPour(state, source, target))
                        continue;

                    var clone = state.Clone();
                    PuzzleEngine.ExecutePour(clone, source, target);

                    var result = DFS(clone, depth + 1, maxDepth, visited);
                    if (result.IsSolvable)
                        return result;
                }
            }

            return new SolveResult(false, -1);
        }

        /// <summary>
        /// Creates a hash string for puzzle state to detect duplicates.
        /// Each container's slots are encoded as digit sequences.
        /// </summary>
        private static string HashState(PuzzleState state)
        {
            var sb = new StringBuilder();
            for (int c = 0; c < state.ContainerCount; c++)
            {
                var container = state.GetContainer(c);
                for (int s = 0; s < container.SlotCount; s++)
                {
                    sb.Append((int)container.GetSlot(s));
                    sb.Append(',');
                }
                sb.Append('|');
            }
            return sb.ToString();
        }
    }
}
