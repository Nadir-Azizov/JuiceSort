using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Detects taps on empty space (background behind containers).
    /// Fires OnTapped when the background is clicked/tapped.
    /// </summary>
    public class BackgroundTapCatcher : MonoBehaviour, IPointerClickHandler
    {
        public event Action OnTapped;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnTapped?.Invoke();
        }
    }
}
