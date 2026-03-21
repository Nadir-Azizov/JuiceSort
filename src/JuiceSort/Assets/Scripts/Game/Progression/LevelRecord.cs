using JuiceSort.Game.LevelGen;

namespace JuiceSort.Game.Progression
{
    /// <summary>
    /// Per-level completion data for roadmap display.
    /// Stores level number, city theme, mood, and star rating.
    /// </summary>
    public class LevelRecord
    {
        public int LevelNumber { get; }
        public string CityName { get; }
        public string CountryName { get; }
        public LevelMood Mood { get; }
        public int Stars { get; private set; }

        public LevelRecord(int levelNumber, string cityName, string countryName, LevelMood mood, int stars)
        {
            LevelNumber = levelNumber;
            CityName = cityName;
            CountryName = countryName;
            Mood = mood;
            Stars = stars;
        }

        /// <summary>
        /// Updates stars only if the new rating is better.
        /// Returns true if stars were upgraded.
        /// </summary>
        public bool TryUpgradeStars(int newStars)
        {
            if (newStars > Stars)
            {
                Stars = newStars;
                return true;
            }
            return false;
        }
    }
}
