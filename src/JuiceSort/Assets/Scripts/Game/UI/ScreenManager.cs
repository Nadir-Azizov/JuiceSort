using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JuiceSort.Game.UI
{
    /// <summary>
    /// Manages screen transitions with crossfade + slide animations.
    /// Registered as a service in Service Locator.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        private readonly Dictionary<GameFlowState, GameObject> _screens = new Dictionary<GameFlowState, GameObject>();
        private readonly Dictionary<GameFlowState, CanvasGroup> _canvasGroups = new Dictionary<GameFlowState, CanvasGroup>();
        private GameFlowState _currentState;
        private GameObject _currentScreen;
        private bool _isTransitioning;
        private bool _hasEverTransitioned;
        private Coroutine _transitionCoroutine;

        private const float FadeDuration = 0.3f;
        private const float SlideOffset = 20f;

        public GameFlowState CurrentState => _currentState;

        /// <summary>
        /// Fired when screen state changes. Passes new state.
        /// </summary>
        public event Action<GameFlowState> OnStateChanged;

        public void RegisterScreen(GameFlowState state, GameObject screen)
        {
            _screens[state] = screen;

            // Ensure CanvasGroup exists for transition animation
            var cg = screen.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = screen.AddComponent<CanvasGroup>();
            _canvasGroups[state] = cg;

            screen.SetActive(false);
        }

        public void TransitionTo(GameFlowState state)
        {
            if (_isTransitioning) return;

            bool hasOutgoing = _currentScreen != null;
            bool hasIncoming = _screens.ContainsKey(state);

            // First screen on app launch — show instantly, no animation
            if (!hasOutgoing && !_hasEverTransitioned)
            {
                _hasEverTransitioned = true;
                _currentState = state;
                if (hasIncoming)
                {
                    var screen = _screens[state];
                    screen.SetActive(true);
                    SetCanvasGroupVisible(state, true);
                    _currentScreen = screen;
                }
                OnStateChanged?.Invoke(state);
                Debug.Log($"[ScreenManager] Transitioned to {state} (instant)");
                return;
            }

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(TransitionCoroutine(_currentState, state));
        }

        private IEnumerator TransitionCoroutine(GameFlowState outgoingState, GameFlowState incomingState)
        {
            _isTransitioning = true;

            try
            {
                var outgoing = _screens.TryGetValue(outgoingState, out var outGo) ? outGo : null;
                _screens.TryGetValue(incomingState, out var incoming);

                CanvasGroup outCg = null;
                RectTransform outRt = null;
                Vector2 outOriginalPos = Vector2.zero;

                if (outgoing != null)
                {
                    _canvasGroups.TryGetValue(outgoingState, out outCg);
                    outRt = outgoing.GetComponent<RectTransform>();
                    if (outRt != null)
                        outOriginalPos = outRt.anchoredPosition;
                }

                // Phase 1: Fade out outgoing screen
                if (outgoing != null && outCg != null)
                {
                    float elapsed = 0f;
                    while (elapsed < FadeDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = EaseInOutCubic(Mathf.Clamp01(elapsed / FadeDuration));
                        outCg.alpha = 1f - t;
                        if (outRt != null)
                            outRt.anchoredPosition = outOriginalPos + new Vector2(0f, SlideOffset * t);
                        yield return null;
                    }
                    outCg.alpha = 0f;
                    outgoing.SetActive(false);
                    // Reset position
                    if (outRt != null)
                        outRt.anchoredPosition = outOriginalPos;
                }

                // Phase 2: Fade in incoming screen (skip if no screen registered, e.g. Playing state)
                if (incoming != null)
                {
                    _canvasGroups.TryGetValue(incomingState, out var inCg);
                    var inRt = incoming.GetComponent<RectTransform>();
                    Vector2 inOriginalPos = inRt != null ? inRt.anchoredPosition : Vector2.zero;

                    if (inCg != null)
                    {
                        inCg.alpha = 0f;
                        inCg.blocksRaycasts = false;
                    }
                    if (inRt != null)
                        inRt.anchoredPosition = inOriginalPos + new Vector2(0f, -SlideOffset);

                    incoming.SetActive(true);

                    if (inCg != null)
                    {
                        float elapsed = 0f;
                        while (elapsed < FadeDuration)
                        {
                            elapsed += Time.unscaledDeltaTime;
                            float t = EaseInOutCubic(Mathf.Clamp01(elapsed / FadeDuration));
                            inCg.alpha = t;
                            if (inRt != null)
                                inRt.anchoredPosition = Vector2.Lerp(
                                    inOriginalPos + new Vector2(0f, -SlideOffset),
                                    inOriginalPos, t);
                            yield return null;
                        }
                        inCg.alpha = 1f;
                        inCg.blocksRaycasts = true;
                    }
                    if (inRt != null)
                        inRt.anchoredPosition = inOriginalPos;

                    _currentScreen = incoming;
                }
                else
                {
                    // No incoming screen (e.g. transitioning to gameplay) — just clear current
                    _currentScreen = null;
                }

                _currentState = incomingState;
                OnStateChanged?.Invoke(incomingState);
                Debug.Log($"[ScreenManager] Transitioned to {incomingState}");
            }
            finally
            {
                _isTransitioning = false;
                _transitionCoroutine = null;
            }
        }

        /// <summary>
        /// Shows an overlay screen with a fade-in (no slide).
        /// Used for LevelComplete overlay on top of Playing.
        /// </summary>
        public void ShowOverlay(GameFlowState state)
        {
            _currentState = state;

            if (_screens.TryGetValue(state, out var screen))
            {
                screen.SetActive(true);

                if (_canvasGroups.TryGetValue(state, out var cg))
                    StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, FadeDuration));
            }

            OnStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Hides an overlay with a fade-out.
        /// </summary>
        public void HideOverlay(GameFlowState state)
        {
            if (_screens.TryGetValue(state, out var screen))
            {
                if (_canvasGroups.TryGetValue(state, out var cg))
                {
                    StartCoroutine(FadeCanvasGroup(cg, 1f, 0f, FadeDuration, () =>
                    {
                        screen.SetActive(false);
                        cg.alpha = 1f; // Reset for next show
                    }));
                }
                else
                {
                    screen.SetActive(false);
                }
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, Action onComplete = null)
        {
            cg.alpha = from;
            // Block input during fade animation; final state set after completion
            cg.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseInOutCubic(Mathf.Clamp01(elapsed / duration));
                cg.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            cg.alpha = to;
            cg.blocksRaycasts = to > 0.5f; // Re-enable input only if fading in

            onComplete?.Invoke();
        }

        public GameObject GetScreen(GameFlowState state)
        {
            return _screens.TryGetValue(state, out var screen) ? screen : null;
        }

        private void SetCanvasGroupVisible(GameFlowState state, bool visible)
        {
            if (_canvasGroups.TryGetValue(state, out var cg))
            {
                cg.alpha = visible ? 1f : 0f;
                cg.blocksRaycasts = visible;
            }
        }

        private static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}
