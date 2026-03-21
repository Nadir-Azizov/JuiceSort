namespace JuiceSort.Game.LevelGen
{
    public class CityData
    {
        public string CityName { get; }
        public string CountryName { get; }

        public CityData(string cityName, string countryName)
        {
            CityName = cityName;
            CountryName = countryName;
        }
    }
}
