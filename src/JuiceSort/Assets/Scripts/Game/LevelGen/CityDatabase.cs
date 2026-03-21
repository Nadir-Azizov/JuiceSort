namespace JuiceSort.Game.LevelGen
{
    /// <summary>
    /// Static list of 38 world cities for level theming.
    /// Visual metadata only — no gameplay effect.
    /// </summary>
    public static class CityDatabase
    {
        public static readonly CityData[] Cities =
        {
            new CityData("Paris", "France"),
            new CityData("Tokyo", "Japan"),
            new CityData("New York", "USA"),
            new CityData("Rio de Janeiro", "Brazil"),
            new CityData("Sydney", "Australia"),
            new CityData("Cairo", "Egypt"),
            new CityData("London", "UK"),
            new CityData("Bangkok", "Thailand"),
            new CityData("Rome", "Italy"),
            new CityData("Istanbul", "Turkey"),
            new CityData("Dubai", "UAE"),
            new CityData("Barcelona", "Spain"),
            new CityData("Mumbai", "India"),
            new CityData("San Francisco", "USA"),
            new CityData("Berlin", "Germany"),
            new CityData("Amsterdam", "Netherlands"),
            new CityData("Cape Town", "South Africa"),
            new CityData("Buenos Aires", "Argentina"),
            new CityData("Seoul", "South Korea"),
            new CityData("Mexico City", "Mexico"),
            new CityData("Vienna", "Austria"),
            new CityData("Prague", "Czech Republic"),
            new CityData("Lisbon", "Portugal"),
            new CityData("Singapore", "Singapore"),
            new CityData("Marrakech", "Morocco"),
            new CityData("Havana", "Cuba"),
            new CityData("Athens", "Greece"),
            new CityData("Stockholm", "Sweden"),
            new CityData("Dublin", "Ireland"),
            new CityData("Vancouver", "Canada"),
            new CityData("Montreal", "Canada"),
            new CityData("Kyoto", "Japan"),
            new CityData("Florence", "Italy"),
            new CityData("Santorini", "Greece"),
            new CityData("Bali", "Indonesia"),
            new CityData("Reykjavik", "Iceland"),
            new CityData("Cartagena", "Colombia"),
            new CityData("Zanzibar", "Tanzania")
        };

        public static int CityCount => Cities.Length;
    }
}
