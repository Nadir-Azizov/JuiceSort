using System;
using System.Collections;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Plays a diagonal white shimmer sweep across a completed bottle.
    /// Attaches as a child of BottleContainerView and uses the same SpriteMask for clipping.
    /// </summary>
    public class CompletionShimmer : MonoBehaviour
    {
        private SpriteRenderer _shimmerRenderer;
        private bool _isPlaying;

        private const float SweepDuration = 0.4f;
        private const float FadeDuration = 0.2f;
        private const float ShimmerAlpha = 0.45f;

        /// <summary>
        /// Creates and attaches a CompletionShimmer to a bottle.
        /// </summary>
        public static CompletionShimmer Create(Transform bottleParent, float spriteWidth, float spriteHeight)
        {
            var go = new GameObject("CompletionShimmer");
            go.transform.SetParent(bottleParent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(spriteWidth * 0.4f, spriteHeight, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateShimmerSprite();
            sr.sortingOrder = 1; // Between liquid (0) and frame (2)
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            sr.color = new Color(1f, 1f, 1f, 0f); // Start invisible
            sr.enabled = false;

            var shimmer = go.AddComponent<CompletionShimmer>();
            shimmer._shimmerRenderer = sr;

            return shimmer;
        }

        /// <summary>
        /// Plays the shimmer sweep animation. Calls onComplete when finished.
        /// </summary>
        public void Play(Action onComplete = null)
        {
            if (_isPlaying) return;
            StartCoroutine(AnimateShimmer(onComplete));
        }

        public bool IsPlaying => _isPlaying;

        private IEnumerator AnimateShimmer(Action onComplete)
        {
            _isPlaying = true;
            _shimmerRenderer.enabled = true;

            // Sweep from left to right by moving the shimmer sprite horizontally
            // The shimmer is narrow (40% width) and sweeps across the bottle
            float startX = -1.2f; // Start off-left
            float endX = 1.2f;   // End off-right

            // Phase 1: Sweep with fade-in at start
            float elapsed = 0f;
            while (elapsed < SweepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SweepDuration);

                // Move horizontally
                transform.localPosition = new Vector3(
                    Mathf.Lerp(startX, endX, t),
                    0f, 0f
                );

                // Fade in quickly at start, hold during sweep
                float alpha = t < 0.15f
                    ? Mathf.Lerp(0f, ShimmerAlpha, t / 0.15f)
                    : ShimmerAlpha;

                _shimmerRenderer.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            // Phase 2: Fade out
            elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / FadeDuration));
                float alpha = Mathf.Lerp(ShimmerAlpha, 0f, t);
                _shimmerRenderer.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            _shimmerRenderer.color = new Color(1f, 1f, 1f, 0f);
            _shimmerRenderer.enabled = false;
            transform.localPosition = Vector3.zero;
            _isPlaying = false;

            onComplete?.Invoke();
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        private static Sprite CreateShimmerSprite()
        {
            // Create a vertical gradient stripe — white in center, transparent at edges
            int w = 8;
            int h = 4;
            var tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Horizontal gradient: peak in center, fade at edges
                    float nx = (float)x / (w - 1);
                    float edgeFade = 1f - Mathf.Abs(nx - 0.5f) * 2f;
                    edgeFade = Mathf.Clamp01(edgeFade);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, edgeFade));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
