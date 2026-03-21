using UnityEngine;
using UnityEngine.UI;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Visual representation of a single slot within a container.
    /// Uses a UI Image to display the drink color.
    /// </summary>
    public class SlotView : MonoBehaviour
    {
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void SetColor(DrinkColor drinkColor)
        {
            if (_image == null)
                _image = GetComponent<Image>();

            if (drinkColor == DrinkColor.None)
            {
                _image.color = ThemeConfig.GetColor(ThemeColorType.EmptySlot);
            }
            else
            {
                _image.color = DrinkColorMap.GetColor(drinkColor);
            }
        }
    }
}
