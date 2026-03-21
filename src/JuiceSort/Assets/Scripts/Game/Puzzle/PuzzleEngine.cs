namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Pure C# puzzle logic. No MonoBehaviour, no Unity dependencies.
    /// All methods operate on PuzzleState data directly.
    /// </summary>
    public static class PuzzleEngine
    {
        /// <summary>
        /// Validates whether a pour from source to target is allowed.
        /// Rules: source not empty, target not full, target empty OR top colors match.
        /// </summary>
        public static bool CanPour(PuzzleState state, int sourceIndex, int targetIndex)
        {
            var source = state.GetContainer(sourceIndex);
            var target = state.GetContainer(targetIndex);

            if (source.IsEmpty())
                return false;

            if (target.IsFull())
                return false;

            if (target.IsEmpty())
                return true;

            return source.GetTopColor() == target.GetTopColor();
        }

        /// <summary>
        /// Executes a pour: moves all consecutive same-color units from source top
        /// into target, limited by available empty slots. Returns true if at least
        /// one unit was poured, false if validation fails.
        /// </summary>
        public static bool ExecutePour(PuzzleState state, int sourceIndex, int targetIndex)
        {
            if (!CanPour(state, sourceIndex, targetIndex))
                return false;

            var source = state.GetContainer(sourceIndex);
            var target = state.GetContainer(targetIndex);

            int available = source.GetTopColorCount();
            int emptySlots = target.SlotCount - target.FilledCount();
            int pourCount = available < emptySlots ? available : emptySlots;

            for (int i = 0; i < pourCount; i++)
            {
                var color = source.RemoveTop();
                target.AddToTop(color);
            }

            return true;
        }

        /// <summary>
        /// Adds an extra empty container to the puzzle.
        /// Returns a NEW PuzzleState with expanded container array.
        /// </summary>
        public static PuzzleState AddExtraContainer(PuzzleState state, int slotCount)
        {
            var newContainers = new ContainerData[state.ContainerCount + 1];
            for (int i = 0; i < state.ContainerCount; i++)
            {
                newContainers[i] = state.GetContainer(i);
            }
            newContainers[state.ContainerCount] = new ContainerData(slotCount);
            return new PuzzleState(newContainers);
        }
    }
}
