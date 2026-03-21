using UnityEngine;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Maps DrinkColor enum values to Unity Colors.
    /// Delegates to ThemeConfig for the Tropical Fresh palette.
    /// </summary>
    public static class DrinkColorMap
    {
        public static Color GetColor(DrinkColor drinkColor)
        {
            return ThemeConfig.GetDrinkColor(drinkColor);
        }
    }
}
