using System;
using System.Collections.Generic;
using UnityEngine;

namespace JuiceSort.Game.UI
{
    /// <summary>
    /// Manages screen transitions by showing/hiding Canvas GameObjects.
    /// Registered as a service in Service Locator.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        private readonly Dictionary<GameFlowState, GameObject> _screens = new Dictionary<GameFlowState, GameObject>();
        private GameFlowState _currentState;
        private GameObject _currentScreen;

        public GameFlowState CurrentState => _currentState;

        /// <summary>
        /// Fired when screen state changes. Passes new state.
        /// </summary>
        public event Action<GameFlowState> OnStateChanged;

        public void RegisterScreen(GameFlowState state, GameObject screen)
        {
            _screens[state] = screen;
            screen.SetActive(false);
        }

        public void TransitionTo(GameFlowState state)
        {
            if (_currentScreen != null)
                _currentScreen.SetActive(false);

            _currentState = state;

            if (_screens.TryGetValue(state, out var screen))
            {
                screen.SetActive(true);
                _currentScreen = screen;
            }
            else
            {
                _currentScreen = null;
                Debug.LogWarning($"[ScreenManager] No screen registered for state {state}");
            }

            OnStateChanged?.Invoke(state);
            Debug.Log($"[ScreenManager] Transitioned to {state}");
        }

        /// <summary>
        /// Shows an overlay screen without hiding the current one.
        /// Used for LevelComplete overlay on top of Playing.
        /// </summary>
        public void ShowOverlay(GameFlowState state)
        {
            _currentState = state;

            if (_screens.TryGetValue(state, out var screen))
            {
                screen.SetActive(true);
            }

            OnStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Hides an overlay without affecting the underlying screen.
        /// </summary>
        public void HideOverlay(GameFlowState state)
        {
            if (_screens.TryGetValue(state, out var screen))
            {
                screen.SetActive(false);
            }
        }

        public GameObject GetScreen(GameFlowState state)
        {
            return _screens.TryGetValue(state, out var screen) ? screen : null;
        }
    }
}
