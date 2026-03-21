using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Reusable tactile bounce effect for UI buttons.
    /// Scales down on press, springs back on release with overshoot.
    /// </summary>
    public class ButtonBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Coroutine _animCoroutine;
        private Vector3 _originalScale;
        private bool _isPressed;

        private const float PressScale = 0.92f;
        private const float SpringScale = 1.05f;
        private const float PressDuration = 0.06f;
        private const float ReleaseDuration = 0.15f;
        private const float CancelDuration = 0.1f;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            StopAnim();
            _animCoroutine = StartCoroutine(AnimateScale(
                transform.localScale,
                _originalScale * PressScale,
                PressDuration,
                EaseInCubic));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;
            StopAnim();
            _animCoroutine = StartCoroutine(AnimateRelease());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;
            StopAnim();
            _animCoroutine = StartCoroutine(AnimateScale(
                transform.localScale,
                _originalScale,
                CancelDuration,
                EaseOutCubic));
        }

        private IEnumerator AnimateRelease()
        {
            // Spring to overshoot
            yield return AnimateScale(
                transform.localScale,
                _originalScale * SpringScale,
                ReleaseDuration * 0.6f,
                EaseOutCubic);

            // Settle to rest
            yield return AnimateScale(
                transform.localScale,
                _originalScale,
                ReleaseDuration * 0.4f,
                EaseOutCubic);

            _animCoroutine = null;
        }

        private IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration, System.Func<float, float> easing)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easing(Mathf.Clamp01(elapsed / duration));
                transform.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }
            transform.localScale = to;
        }

        private void StopAnim()
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }
        }

        private static float EaseInCubic(float t)
        {
            return t * t * t;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }
    }
}
