using System.Collections;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Renders a visible liquid stream between bottles during a pour.
    /// Uses a LineRenderer with a quadratic bezier arc.
    /// Reusable single instance — activate on StartStream, deactivate on StopStream.
    /// </summary>
    public class PourStreamVFX : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private Material _material;
        private bool _isActive;
        private Coroutine _fadeCoroutine;

        private const int PointCount = 10;
        private const float StartWidth = 0.08f;
        private const float EndWidth = 0.05f;
        private const float ArcHeight = 0.3f;
        private const float FadeDuration = 0.1f;

        // Pre-allocated array to avoid per-frame allocation
        private readonly Vector3[] _points = new Vector3[PointCount];

        /// <summary>
        /// Shows the stream between source and target positions with the given color.
        /// Call UpdatePositions() each frame to track moving bottles.
        /// </summary>
        public void StartStream(Vector3 sourcePos, Vector3 targetPos, Color color)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _isActive = true;
            gameObject.SetActive(true);

            // Set color with full alpha
            color.a = 1f;
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;

            UpdateCurve(sourcePos, targetPos);
        }

        /// <summary>
        /// Updates stream curve positions. Call each frame during pour while stream is active.
        /// </summary>
        public void UpdatePositions(Vector3 sourcePos, Vector3 targetPos)
        {
            if (!_isActive) return;
            UpdateCurve(sourcePos, targetPos);
        }

        /// <summary>
        /// Fades and hides the stream.
        /// </summary>
        public void StopStream()
        {
            if (!_isActive) return;
            _isActive = false;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOut());
        }

        private void UpdateCurve(Vector3 start, Vector3 end)
        {
            // Quadratic bezier: control point above midpoint for natural arc
            Vector3 mid = (start + end) * 0.5f;
            Vector3 controlPoint = mid + new Vector3(0f, ArcHeight, 0f);

            for (int i = 0; i < PointCount; i++)
            {
                float t = (float)i / (PointCount - 1);
                // Quadratic bezier: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
                float u = 1f - t;
                _points[i] = u * u * start + 2f * u * t * controlPoint + t * t * end;
            }

            _lineRenderer.positionCount = PointCount;
            _lineRenderer.SetPositions(_points);
        }

        private IEnumerator FadeOut()
        {
            Color startColor = _lineRenderer.startColor;
            float elapsed = 0f;

            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / FadeDuration);
                Color c = startColor;
                c.a = alpha;
                _lineRenderer.startColor = c;
                _lineRenderer.endColor = c;
                yield return null;
            }

            gameObject.SetActive(false);
            _fadeCoroutine = null;
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }

        public static PourStreamVFX Create()
        {
            var go = new GameObject("PourStreamVFX");
            go.SetActive(false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = PointCount;
            lr.startWidth = StartWidth;
            lr.endWidth = EndWidth;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 4;
            lr.sortingOrder = 3; // above bottles and liquid

            // Simple unlit material — tracked for cleanup
            var mat = new Material(Shader.Find("Sprites/Default"));
            lr.material = mat;
            lr.startColor = Color.white;
            lr.endColor = Color.white;

            var vfx = go.AddComponent<PourStreamVFX>();
            vfx._lineRenderer = lr;
            vfx._material = mat;

            return vfx;
        }
    }
}
