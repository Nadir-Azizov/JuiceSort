using System;
using System.Collections;
using UnityEngine;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Cork/cap animation that drops onto a bottle when it becomes fully sorted.
    /// Includes drop+bounce and confetti burst effects.
    /// </summary>
    public class BottleCapAnimation : MonoBehaviour
    {
        private SpriteRenderer _capRenderer;
        private Coroutine _animCoroutine;
        private Vector3 _restPosition;

        private const float DropHeight = 0.5f;
        private const float DropDuration = 0.3f;
        private const int ConfettiCount = 8;
        private const float ConfettiFadeDuration = 0.5f;
        private const float ConfettiSpeed = 1.5f;
        private const float ConfettiGravity = 3f;

        // Cached sprite
        private static Sprite _cachedCapSprite;

        /// <summary>
        /// Plays the cap drop + bounce + confetti animation.
        /// </summary>
        public void PlayCapClose(Action onComplete = null)
        {
            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);

            _animCoroutine = StartCoroutine(AnimateCapDrop(onComplete));
        }

        /// <summary>
        /// Hides the cap instantly (for undo/reset).
        /// </summary>
        public void HideCap()
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }

            if (_capRenderer != null)
            {
                _capRenderer.enabled = false;
                transform.localPosition = _restPosition;
            }
        }

        public bool IsVisible => _capRenderer != null && _capRenderer.enabled;

        private IEnumerator AnimateCapDrop(Action onComplete)
        {
            _capRenderer.enabled = true;

            // Start above rest position
            Vector3 startPos = _restPosition + new Vector3(0f, DropHeight, 0f);
            transform.localPosition = startPos;

            // Drop with EaseOutBounce
            float elapsed = 0f;
            while (elapsed < DropDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBounce(Mathf.Clamp01(elapsed / DropDuration));
                transform.localPosition = Vector3.Lerp(startPos, _restPosition, t);
                yield return null;
            }
            transform.localPosition = _restPosition;

            // Confetti burst on landing
            SpawnConfetti();

            _animCoroutine = null;
            onComplete?.Invoke();
        }

        private void SpawnConfetti()
        {
            // Get the bottle's top color for confetti
            var parentView = GetComponentInParent<BottleContainerView>();
            Color confettiColor = Color.white;
            if (parentView != null && parentView.Data != null)
            {
                var topColor = parentView.Data.GetTopColor();
                if (topColor != DrinkColor.None)
                    confettiColor = ThemeConfig.GetDrinkColor(topColor);
            }

            for (int i = 0; i < ConfettiCount; i++)
            {
                var particleGo = new GameObject($"Confetti_{i}");
                particleGo.transform.SetParent(transform.parent, false);
                particleGo.transform.localPosition = transform.localPosition;

                var sr = particleGo.AddComponent<SpriteRenderer>();
                sr.sprite = LoadCapSprite(); // reuse small square sprite
                sr.sortingOrder = 4;
                sr.color = confettiColor;
                particleGo.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.3f, 0.6f);

                // Random outward velocity
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ConfettiSpeed;
                velocity.y += UnityEngine.Random.Range(0.5f, 1.5f); // bias upward

                // Start fade+gravity coroutine on parent (confetti GO gets destroyed)
                StartCoroutine(AnimateConfettiParticle(particleGo, velocity));
            }
        }

        private IEnumerator AnimateConfettiParticle(GameObject particle, Vector2 velocity)
        {
            var sr = particle.GetComponent<SpriteRenderer>();
            Color startColor = sr.color;
            float elapsed = 0f;

            while (elapsed < ConfettiFadeDuration)
            {
                elapsed += Time.deltaTime;

                // Apply velocity + gravity
                velocity.y -= ConfettiGravity * Time.deltaTime;
                particle.transform.localPosition += new Vector3(velocity.x, velocity.y, 0f) * Time.deltaTime;

                // Fade out
                float alpha = Mathf.Lerp(1f, 0f, elapsed / ConfettiFadeDuration);
                sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                yield return null;
            }

            Destroy(particle);
        }

        private static float EaseOutBounce(float t)
        {
            if (t < 1f / 2.75f)
                return 7.5625f * t * t;
            if (t < 2f / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }

        private static Sprite LoadCapSprite()
        {
            if (_cachedCapSprite == null)
            {
                // Procedural small rounded-ish square
                int size = 8;
                var tex = new Texture2D(size, size);
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        tex.SetPixel(x, y, Color.white);
                tex.Apply();
                _cachedCapSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
            }
            return _cachedCapSprite;
        }

        /// <summary>
        /// Creates a BottleCapAnimation as a child of the bottle.
        /// </summary>
        public static BottleCapAnimation Create(Transform parent, float spriteWidth, float spriteHeight)
        {
            var go = new GameObject("BottleCap");
            go.transform.SetParent(parent, false);

            // Position at bottle mouth: top of sprite, in the headroom zone
            float capY = spriteHeight * 0.42f; // just above the 80% visual fill line
            go.transform.localPosition = new Vector3(0f, capY, 0f);

            // Scale cap to ~1/4 bottle width
            float capScale = spriteWidth * 0.3f;
            go.transform.localScale = new Vector3(capScale, capScale * 0.5f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadCapSprite();
            sr.sortingOrder = 3;
            sr.color = new Color(0.55f, 0.35f, 0.2f, 1f); // cork brown
            sr.maskInteraction = SpriteMaskInteraction.None; // cap sits on top, not masked
            sr.enabled = false; // hidden until completion

            var cap = go.AddComponent<BottleCapAnimation>();
            cap._capRenderer = sr;
            cap._restPosition = go.transform.localPosition;

            return cap;
        }
    }
}
