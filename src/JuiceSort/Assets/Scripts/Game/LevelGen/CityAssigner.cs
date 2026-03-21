namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// Assigns a city and mood to each level.
    /// Deterministic: same level number always gets the same city and mood.
    /// Independent from DifficultyScaler — called separately.
    /// </summary>
    public static class CityAssigner
    {
        public static CityData AssignCity(int levelNumber)
        {
            int cityIndex = (levelNumber - 1) % CityDatabase.CityCount;
            return CityDatabase.Cities[cityIndex];
        }

        public static LevelMood AssignMood(int levelNumber)
        {
            // Alternate morning/night based on level number
            return levelNumber % 2 == 1 ? LevelMood.Morning : LevelMood.Night;
        }
    }
}
