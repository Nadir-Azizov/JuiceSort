using System.Collections;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Spawns subtle sparkle particles on glass containers when idle.
    /// Uses a shared static pool to limit total active sparkles across all bottles.
    /// </summary>
    public class GlassSparkle : MonoBehaviour
    {
        private ContainerData _data;
        private float _spriteWidth;
        private float _spriteHeight;
        private bool _enabled = true;
        private Coroutine _loopCoroutine;

        private const float MinInterval = 2f;
        private const float MaxInterval = 3f;
        private const float FadeInDuration = 0.3f;
        private const float HoldDuration = 0.2f;
        private const float FadeOutDuration = 0.3f;
        private const float SparkleAlpha = 0.6f;
        private const int MaxActiveSparkles = 8;

        // Shared pool
        private static SpriteRenderer[] _pool;
        private static int _activeCount;
        private static Sprite _sparkleSprite;

        public void Initialize(ContainerData data, float spriteWidth, float spriteHeight)
        {
            _data = data;
            _spriteWidth = spriteWidth;
            _spriteHeight = spriteHeight;
            EnsurePool();
            _loopCoroutine = StartCoroutine(SparkleLoop());
        }

        public void SetData(ContainerData data)
        {
            _data = data;
        }

        public void SetSparklesEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        private void OnDestroy()
        {
            if (_loopCoroutine != null)
                StopCoroutine(_loopCoroutine);
        }

        private IEnumerator SparkleLoop()
        {
            while (true)
            {
                float wait = Random.Range(MinInterval, MaxInterval);
                yield return new WaitForSeconds(wait);

                if (!_enabled || _data == null || _data.IsEmpty() || _activeCount >= MaxActiveSparkles)
                    continue;

                // Random position within bottle bounds
                float x = Random.Range(-_spriteWidth * 0.4f, _spriteWidth * 0.4f);
                float y = Random.Range(-_spriteHeight * 0.4f, _spriteHeight * 0.4f);
                Vector3 localPos = new Vector3(x, y, 0f);

                StartCoroutine(AnimateSparkle(localPos));
            }
        }

        private IEnumerator AnimateSparkle(Vector3 localPos)
        {
            var sr = AcquireSparkle();
            if (sr == null) yield break;

            sr.transform.SetParent(transform, false);
            sr.transform.localPosition = localPos;
            sr.transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
            sr.color = new Color(1f, 1f, 1f, 0f);
            sr.enabled = true;

            // Fade in
            float elapsed = 0f;
            while (elapsed < FadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeInDuration);
                sr.color = new Color(1f, 1f, 1f, t * SparkleAlpha);
                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(HoldDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < FadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeOutDuration);
                sr.color = new Color(1f, 1f, 1f, (1f - t) * SparkleAlpha);
                yield return null;
            }

            ReleaseSparkle(sr);
        }

        private static void EnsurePool()
        {
            if (_pool != null) return;

            _sparkleSprite = CreateSparkleSprite();
            _pool = new SpriteRenderer[MaxActiveSparkles];
            _activeCount = 0;

            for (int i = 0; i < MaxActiveSparkles; i++)
            {
                var go = new GameObject($"Sparkle_{i}");
                go.hideFlags = HideFlags.HideAndDontSave;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _sparkleSprite;
                sr.sortingOrder = 1;
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                sr.enabled = false;
                _pool[i] = sr;
            }
        }

        private static SpriteRenderer AcquireSparkle()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                if (_pool[i] != null && !_pool[i].enabled)
                {
                    _activeCount++;
                    return _pool[i];
                }
            }
            return null;
        }

        private static void ReleaseSparkle(SpriteRenderer sr)
        {
            sr.enabled = false;
            _activeCount = Mathf.Max(0, _activeCount - 1);
        }

        private static Sprite CreateSparkleSprite()
        {
            // Small diamond shape
            int size = 8;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;

            float center = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) / center;
                    float dy = Mathf.Abs(y - center) / center;
                    float dist = dx + dy; // Diamond distance
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha *= alpha; // Sharper falloff
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
        }
    }
}
