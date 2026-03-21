using UnityEngine;

namespace JuiceSort.Game.Effects
{
    /// <summary>
    /// Ambient warm bokeh particles that drift slowly upward in the background.
    /// Creates a subtle tropical atmosphere effect. Uses a fixed sprite pool.
    /// </summary>
    public class FloatingLights : MonoBehaviour
    {
        private SpriteRenderer[] _particles;
        private float[] _speeds;
        private float[] _phases;
        private float[] _alphas;
        private float _screenTop;
        private float _screenBottom;
        private float _screenLeft;
        private float _screenRight;

        private const int ParticleCount = 8;
        private const float MinSpeed = 0.4f;
        private const float MaxSpeed = 0.6f;
        private const float MinSize = 0.08f;
        private const float MaxSize = 0.16f;
        private const float MinAlpha = 0.1f;
        private const float MaxAlpha = 0.3f;
        private const float SwayAmplitude = 0.1f;
        private const float SwaySpeed = 0.8f;
        private const float EdgeFadeZone = 0.15f; // Fraction of screen height to fade in/out

        private static readonly Color LightColor = new Color(1f, 0.9f, 0.6f);

        public static FloatingLights Create()
        {
            var go = new GameObject("FloatingLights");
            DontDestroyOnLoad(go);
            var fl = go.AddComponent<FloatingLights>();
            fl.Initialize();
            return fl;
        }

        private void Initialize()
        {
            UpdateScreenBounds();

            var sprite = CreateBokehSprite();
            _particles = new SpriteRenderer[ParticleCount];
            _speeds = new float[ParticleCount];
            _phases = new float[ParticleCount];
            _alphas = new float[ParticleCount];

            for (int i = 0; i < ParticleCount; i++)
            {
                var go = new GameObject($"Light_{i}");
                go.transform.SetParent(transform, false);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = -5; // Behind containers, in front of background
                sr.maskInteraction = SpriteMaskInteraction.None;

                _particles[i] = sr;
                _speeds[i] = Random.Range(MinSpeed, MaxSpeed);
                _phases[i] = Random.Range(0f, Mathf.PI * 2f);
                _alphas[i] = Random.Range(MinAlpha, MaxAlpha);

                // Stagger initial positions across full screen height
                float startY = Mathf.Lerp(_screenBottom, _screenTop, (float)i / ParticleCount);
                float startX = Random.Range(_screenLeft, _screenRight);
                float size = Random.Range(MinSize, MaxSize);

                go.transform.position = new Vector3(startX, startY, 5f); // Z=5 behind bottles
                go.transform.localScale = Vector3.one * size;
                sr.color = new Color(LightColor.r, LightColor.g, LightColor.b, _alphas[i]);
            }
        }

        private void Update()
        {
            if (_particles == null) return;

            float screenHeight = _screenTop - _screenBottom;
            float fadeZone = screenHeight * EdgeFadeZone;

            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] == null) continue;

                var tf = _particles[i].transform;
                var pos = tf.position;

                // Move upward
                pos.y += _speeds[i] * Time.deltaTime;

                // Horizontal sway
                pos.x += Mathf.Cos(Time.time * SwaySpeed + _phases[i]) * SwayAmplitude * Time.deltaTime;

                // Recycle at top
                if (pos.y > _screenTop + 0.5f)
                {
                    pos.y = _screenBottom - 0.3f;
                    pos.x = Random.Range(_screenLeft, _screenRight);
                    _speeds[i] = Random.Range(MinSpeed, MaxSpeed);
                    _phases[i] = Random.Range(0f, Mathf.PI * 2f);
                    _alphas[i] = Random.Range(MinAlpha, MaxAlpha);

                    float size = Random.Range(MinSize, MaxSize);
                    tf.localScale = Vector3.one * size;
                }

                tf.position = pos;

                // Fade near screen edges — fade in at bottom, fade out at top
                float edgeAlpha = 1f;
                float distFromBottom = pos.y - _screenBottom;
                float distFromTop = _screenTop - pos.y;
                if (distFromBottom < fadeZone)
                    edgeAlpha = Mathf.Clamp01(distFromBottom / fadeZone);
                else if (distFromTop < fadeZone)
                    edgeAlpha = Mathf.Clamp01(distFromTop / fadeZone);

                _particles[i].color = new Color(LightColor.r, LightColor.g, LightColor.b, _alphas[i] * edgeAlpha);
            }
        }

        private void UpdateScreenBounds()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                _screenTop = cam.transform.position.y + cam.orthographicSize;
                _screenBottom = cam.transform.position.y - cam.orthographicSize;
                float halfWidth = cam.orthographicSize * cam.aspect;
                _screenLeft = cam.transform.position.x - halfWidth;
                _screenRight = cam.transform.position.x + halfWidth;
            }
            else
            {
                _screenTop = 5f;
                _screenBottom = -5f;
                _screenLeft = -3f;
                _screenRight = 3f;
            }
        }

        private static Sprite CreateBokehSprite()
        {
            // Soft circular gradient
            int size = 16;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;

            float center = (size - 1) / 2f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha = alpha * alpha; // Soft falloff
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
