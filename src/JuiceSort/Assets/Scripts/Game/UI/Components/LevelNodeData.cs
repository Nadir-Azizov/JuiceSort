using JuiceSort.Game.LevelGen;
using JuiceSort.Game.Progression;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Data for rendering a level node in roadmap or gate screen.
    /// </summary>
    public class LevelNodeData
    {
        public int LevelNumber { get; }
        public string CityName { get; }
        public string CountryName { get; }
        public LevelMood Mood { get; }
        public int Stars { get; }
        public bool IsCurrentLevel { get; }
        public bool IsCompleted { get; }

        public LevelNodeData(int levelNumber, string cityName, string countryName, LevelMood mood, int stars, bool isCurrentLevel, bool isCompleted)
        {
            LevelNumber = levelNumber;
            CityName = cityName;
            CountryName = countryName;
            Mood = mood;
            Stars = stars;
            IsCurrentLevel = isCurrentLevel;
            IsCompleted = isCompleted;
        }

        public static LevelNodeData FromRecord(LevelRecord record, bool isCurrentLevel = false)
        {
            return new LevelNodeData(
                record.LevelNumber,
                record.CityName,
                record.CountryName,
                record.Mood,
                record.Stars,
                isCurrentLevel,
                true
            );
        }

        public static LevelNodeData ForCurrentLevel(int levelNumber, CityData city, LevelMood mood)
        {
            return new LevelNodeData(
                levelNumber,
                city.CityName,
                city.CountryName,
                mood,
                0,
                true,
                false
            );
        }
    }
}
