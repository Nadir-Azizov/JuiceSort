using JuiceSort.Game.Puzzle;

namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// Generates solvable puzzles using reverse-from-solved algorithm with seeded PRNG.
    /// </summary>
    public static class LevelGenerator
    {
        private static readonly DrinkColor[] ColorPalette =
        {
            DrinkColor.MangoAmber,
            DrinkColor.DeepBerry,
            DrinkColor.TropicalTeal,
            DrinkColor.WatermelonRose,
            DrinkColor.LimeGold
        };

        public static PuzzleState Generate(LevelDefinition definition)
        {
            // Step 1: Create ALL containers including empties
            var containers = new ContainerData[definition.ContainerCount];

            for (int i = 0; i < definition.FilledContainerCount; i++)
            {
                var slots = new DrinkColor[definition.SlotCount];
                var color = ColorPalette[i % ColorPalette.Length];
                for (int s = 0; s < definition.SlotCount; s++)
                    slots[s] = color;
                containers[i] = new ContainerData(slots);
            }

            for (int i = definition.FilledContainerCount; i < definition.ContainerCount; i++)
                containers[i] = new ContainerData(definition.SlotCount);

            var state = new PuzzleState(containers);

            // Step 2: Scramble using ALL containers (including empties for maneuvering)
            // Use raw pours (no color matching) to mix thoroughly
            Scramble(state, definition);

            // Step 3: Ensure at least EmptyContainerCount containers are empty at the end
            // by consolidating liquid back into fewer containers if needed
            EnsureEmptyContainers(state, definition);

            return state;
        }

        private static void Scramble(PuzzleState state, LevelDefinition definition)
        {
            var rng = new System.Random(definition.Seed);
            int containerCount = definition.ContainerCount;

            // Many raw pours to thoroughly mix colors
            int totalPours = definition.ColorCount * definition.SlotCount * 4;
            int maxAttempts = totalPours * 8;
            int successfulPours = 0;

            for (int i = 0; i < maxAttempts && successfulPours < totalPours; i++)
            {
                int source = rng.Next(containerCount);
                int target = rng.Next(containerCount);

                if (source == target)
                    continue;

                var src = state.GetContainer(source);
                var tgt = state.GetContainer(target);

                if (src.IsEmpty() || tgt.IsFull())
                    continue;

                var color = src.RemoveTop();
                tgt.AddToTop(color);
                successfulPours++;
            }
        }

        /// <summary>
        /// After scrambling, ensure we have the required number of empty containers.
        /// Move liquid from nearly-empty containers into others to free up containers.
        /// </summary>
        private static void EnsureEmptyContainers(PuzzleState state, LevelDefinition definition)
        {
            int emptyNeeded = definition.EmptyContainerCount;
            int containerCount = definition.ContainerCount;

            // Count current empties
            int emptyCount = 0;
            for (int i = 0; i < containerCount; i++)
            {
                if (state.GetContainer(i).IsEmpty())
                    emptyCount++;
            }

            // If we already have enough empties, done
            if (emptyCount >= emptyNeeded)
                return;

            // Find containers with least liquid and move their contents elsewhere
            for (int attempts = 0; attempts < containerCount * 10 && emptyCount < emptyNeeded; attempts++)
            {
                // Find container with fewest filled slots (but not empty)
                int minIndex = -1;
                int minFilled = int.MaxValue;
                for (int i = 0; i < containerCount; i++)
                {
                    var c = state.GetContainer(i);
                    int filled = c.FilledCount();
                    if (filled > 0 && filled < minFilled)
                    {
                        minFilled = filled;
                        minIndex = i;
                    }
                }

                if (minIndex < 0)
                    break;

                // Move all liquid from this container to others
                var source = state.GetContainer(minIndex);
                while (!source.IsEmpty())
                {
                    bool moved = false;
                    for (int t = 0; t < containerCount; t++)
                    {
                        if (t == minIndex)
                            continue;
                        var target = state.GetContainer(t);
                        if (target.IsFull())
                            continue;

                        var color = source.RemoveTop();
                        target.AddToTop(color);
                        moved = true;
                        break;
                    }
                    if (!moved)
                        break;
                }

                if (source.IsEmpty())
                    emptyCount++;
            }
        }
    }
}
