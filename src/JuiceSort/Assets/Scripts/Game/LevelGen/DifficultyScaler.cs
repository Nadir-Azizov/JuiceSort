using System;

namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// Maps level number to difficulty parameters.
    /// Can use a DifficultyConfig ScriptableObject or built-in defaults.
    /// </summary>
    public static class DifficultyScaler
    {
        // Default values (used when no config is provided)
        private const int DefaultBaseColorCount = 3;
        private const int DefaultColorsPerStep = 20;
        private const int DefaultMaxColors = 5;
        private const int DefaultBaseExtraContainers = 1;
        private const int DefaultContainersPerStep = 10;
        private const int DefaultMaxContainers = 10;
        private const int DefaultBaseSlotCount = 4;
        private const int DefaultSlotsPerStep = 100;
        private const int DefaultMaxSlots = 6;
        private const int DefaultEmptyContainers = 2;

        /// <summary>
        /// Gets level definition using built-in default scaling.
        /// </summary>
        public static LevelDefinition GetLevelDefinition(int levelNumber)
        {
            int colorCount = Math.Min(DefaultBaseColorCount + (levelNumber - 1) / DefaultColorsPerStep, DefaultMaxColors);
            int slotCount = Math.Min(DefaultBaseSlotCount + (levelNumber - 1) / DefaultSlotsPerStep, DefaultMaxSlots);

            // Filled containers = one per color. Total = filled + empty.
            int filledContainers = colorCount;
            int totalContainers = filledContainers + DefaultEmptyContainers;

            return new LevelDefinition(
                levelNumber,
                totalContainers,
                colorCount,
                slotCount,
                DefaultEmptyContainers
            );
        }

        /// <summary>
        /// Gets level definition using a ScriptableObject config for tweakable values.
        /// </summary>
        public static LevelDefinition GetLevelDefinition(int levelNumber, DifficultyConfig config)
        {
            int colorCount = Math.Min(config.BaseColorCount + (levelNumber - 1) / config.ColorsPerLevelStep, config.MaxColors);
            int slotCount = Math.Min(config.BaseSlotCount + (levelNumber - 1) / config.SlotsPerLevelStep, config.MaxSlots);

            int filledContainers = colorCount;
            int totalContainers = filledContainers + config.EmptyContainerCount;

            return new LevelDefinition(
                levelNumber,
                totalContainers,
                colorCount,
                slotCount,
                config.EmptyContainerCount,
                config.ShuffleMultiplier
            );
        }
    }
}
