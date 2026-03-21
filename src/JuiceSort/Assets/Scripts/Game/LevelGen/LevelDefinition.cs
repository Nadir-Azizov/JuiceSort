namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// Plain C# data class holding level generation parameters and theme metadata.
    /// </summary>
    public class LevelDefinition
    {
        public int LevelNumber { get; }
        public int ContainerCount { get; }
        public int ColorCount { get; }
        public int SlotCount { get; }
        public int EmptyContainerCount { get; }
        public int Seed { get; }

        public int ShuffleMultiplier { get; }

        // Theme metadata (populated by CityAssigner, independent from difficulty)
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public LevelMood Mood { get; set; }

        public LevelDefinition(int levelNumber, int containerCount, int colorCount, int slotCount, int emptyContainerCount, int shuffleMultiplier = 3)
        {
            LevelNumber = levelNumber;
            ContainerCount = containerCount;
            ColorCount = colorCount;
            SlotCount = slotCount;
            EmptyContainerCount = emptyContainerCount;
            ShuffleMultiplier = shuffleMultiplier;
            Seed = levelNumber;
        }

        /// <summary>
        /// Number of containers that hold colors (total - empty).
        /// </summary>
        public int FilledContainerCount => ContainerCount - EmptyContainerCount;
    }
}
