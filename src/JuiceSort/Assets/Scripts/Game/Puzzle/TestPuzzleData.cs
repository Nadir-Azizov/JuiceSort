using JuiceSort.Core;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Provides a hand-crafted solvable test puzzle for development.
    /// 4 containers, 4 slots each, 4 colors, 1 empty container.
    /// This class will be removed once procedural generation (Epic 2) is implemented.
    /// </summary>
    public static class TestPuzzleData
    {
        public static PuzzleState CreateTestPuzzle()
        {
            int slots = GameConstants.DefaultSlotsPerContainer;

            // Container 0: [WatermelonRose, MangoAmber, TropicalTeal, DeepBerry] (bottom → top)
            var container0 = new ContainerData(new[]
            {
                DrinkColor.WatermelonRose,
                DrinkColor.MangoAmber,
                DrinkColor.TropicalTeal,
                DrinkColor.DeepBerry
            });

            // Container 1: [TropicalTeal, DeepBerry, WatermelonRose, MangoAmber] (bottom → top)
            var container1 = new ContainerData(new[]
            {
                DrinkColor.TropicalTeal,
                DrinkColor.DeepBerry,
                DrinkColor.WatermelonRose,
                DrinkColor.MangoAmber
            });

            // Container 2: [DeepBerry, WatermelonRose, MangoAmber, TropicalTeal] (bottom → top)
            var container2 = new ContainerData(new[]
            {
                DrinkColor.DeepBerry,
                DrinkColor.WatermelonRose,
                DrinkColor.MangoAmber,
                DrinkColor.TropicalTeal
            });

            // Container 3: Empty (maneuvering space)
            var container3 = new ContainerData(slots);

            return new PuzzleState(new[]
            {
                container0,
                container1,
                container2,
                container3
            });
        }
    }
}
