using System;
using System.Collections;
using UnityEngine;
using JuiceSort.Game.Layout;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// World-space board using BottleContainerView (SpriteRenderer + shader).
    /// Replaces the old Canvas-based PuzzleBoardView for bottle rendering.
    /// </summary>
    public class BottleBoardView : MonoBehaviour
    {
        private BottleContainerView[] _containerViews;
        private LayoutConfig _layoutConfig;

        // Animation constants
        private const float RelayoutDuration = 0.3f;
        private const float PopInDuration = 0.2f;

        public float BottleSpriteHeight => _layoutConfig != null ? _layoutConfig.BottleSpriteHeight : 10.17f;

        public event Action<int> OnContainerTapped;

        public void Initialize(PuzzleState puzzleState)
        {
            int count = puzzleState.ContainerCount;
            _containerViews = new BottleContainerView[count];
            _layoutConfig = LayoutConfig.Default();

            var cam = Camera.main;
            float camOrthoSize = cam != null ? cam.orthographicSize : 5f;
            float camAspect = cam != null ? cam.aspect : 9f / 16f;

            var layout = ResponsiveLayoutManager.CalculateLayout(count, camOrthoSize, camAspect, _layoutConfig);

            // Position board at play area center
            transform.position = new Vector3(
                cam != null ? cam.transform.position.x : 0f,
                layout.BoardY,
                0f
            );

            for (int i = 0; i < count; i++)
            {
                var pos = layout.Positions[i];
                var containerData = puzzleState.GetContainer(i);
                var view = BottleContainerView.Create(transform, containerData, i, pos.x, pos.y, layout.Scale);
                view.OnTapped += HandleContainerTapped;
                _containerViews[i] = view;
            }

            Debug.Log($"[BottleBoardView] Created {count} bottles. Scale={layout.Scale:F3}, rows={layout.RowCount}, boardY={layout.BoardY:F2}");
        }

        private void OnDestroy()
        {
            if (_containerViews == null)
                return;

            for (int i = 0; i < _containerViews.Length; i++)
            {
                if (_containerViews[i] != null)
                    _containerViews[i].OnTapped -= HandleContainerTapped;
            }
        }

        private void HandleContainerTapped(int containerIndex)
        {
            OnContainerTapped?.Invoke(containerIndex);
        }

        public BottleContainerView GetContainerView(int index)
        {
            if (_containerViews == null || index < 0 || index >= _containerViews.Length)
                return null;
            return _containerViews[index];
        }

        /// <summary>
        /// Rebinds all containers to a new puzzle state (for undo/restart).
        /// </summary>
        public void RebindPuzzle(PuzzleState puzzleState)
        {
            for (int i = 0; i < _containerViews.Length && i < puzzleState.ContainerCount; i++)
            {
                _containerViews[i].SetData(puzzleState.GetContainer(i));
            }
        }

        /// <summary>
        /// Recalculates layout for the given bottle count using ResponsiveLayoutManager.
        /// </summary>
        private BottleLayout RecalculateLayout(int bottleCount)
        {
            if (_layoutConfig == null)
                _layoutConfig = LayoutConfig.Default();

            var cam = Camera.main;
            float camOrthoSize = cam != null ? cam.orthographicSize : 5f;
            float camAspect = cam != null ? cam.aspect : 9f / 16f;

            return ResponsiveLayoutManager.CalculateLayout(bottleCount, camOrthoSize, camAspect, _layoutConfig);
        }

        /// <summary>
        /// Dynamically adds a container with animated re-layout.
        /// Existing bottles animate to new positions (0.3s), new bottle pops in (0.2s).
        /// Calls onComplete when all animations finish.
        /// </summary>
        public void AddContainerView(ContainerData data, int index, Action onComplete = null)
        {
            int newCount = _containerViews.Length + 1;
            var layout = RecalculateLayout(newCount);

            // Update board position
            var cam = Camera.main;
            transform.position = new Vector3(
                cam != null ? cam.transform.position.x : 0f,
                layout.BoardY,
                0f
            );

            // Create the new bottle at its target position but with scale = 0 (invisible)
            var newPos = layout.Positions[_containerViews.Length];
            var view = BottleContainerView.Create(transform, data, index, newPos.x, newPos.y, 0f);
            view.OnTapped += HandleContainerTapped;

            // Expand the array
            var newViews = new BottleContainerView[newCount];
            for (int i = 0; i < _containerViews.Length; i++)
                newViews[i] = _containerViews[i];
            newViews[_containerViews.Length] = view;
            _containerViews = newViews;

            // Start animated re-layout
            StartCoroutine(AnimateRelayout(layout, view, onComplete));

            Debug.Log($"[BottleBoardView] Adding bottle (animated). Total={newCount}, scale={layout.Scale:F3}, rows={layout.RowCount}");
        }

        /// <summary>
        /// Animates existing bottles to new positions/scales, then pops in the new bottle.
        /// </summary>
        private IEnumerator AnimateRelayout(BottleLayout layout, BottleContainerView newBottle, Action onComplete)
        {
            int existingCount = _containerViews.Length - 1; // Last one is the new bottle

            // Capture current state of existing bottles
            var fromPositions = new Vector3[existingCount];
            var fromScales = new Vector3[existingCount];
            for (int i = 0; i < existingCount; i++)
            {
                fromPositions[i] = _containerViews[i].transform.localPosition;
                fromScales[i] = _containerViews[i].transform.localScale;
            }

            // Phase 1: Animate existing bottles to new positions (0.3s, all concurrent)
            float elapsed = 0f;
            Vector3 targetScale = new Vector3(layout.Scale, layout.Scale, 1f);

            while (elapsed < RelayoutDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / RelayoutDuration));

                for (int i = 0; i < existingCount; i++)
                {
                    var targetPos = new Vector3(layout.Positions[i].x, layout.Positions[i].y, 0f);
                    _containerViews[i].transform.localPosition = Vector3.Lerp(fromPositions[i], targetPos, t);
                    _containerViews[i].transform.localScale = Vector3.Lerp(fromScales[i], targetScale, t);
                }

                yield return null;
            }

            // Snap to final positions and update rest state
            for (int i = 0; i < existingCount; i++)
            {
                _containerViews[i].transform.localPosition = new Vector3(layout.Positions[i].x, layout.Positions[i].y, 0f);
                _containerViews[i].transform.localScale = targetScale;
                _containerViews[i].UpdateRestState();
            }

            // Phase 2: Pop-in new bottle (0.2s, EaseOutBack)
            elapsed = 0f;
            while (elapsed < PopInDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(Mathf.Clamp01(elapsed / PopInDuration));
                float s = layout.Scale * t;
                newBottle.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            // Snap to final scale and update rest state
            newBottle.transform.localScale = targetScale;
            newBottle.UpdateRestState();

            onComplete?.Invoke();
        }

        public int ContainerCount => _containerViews != null ? _containerViews.Length : 0;

        public void SetAllSparklesEnabled(bool enabled)
        {
            if (_containerViews == null) return;
            for (int i = 0; i < _containerViews.Length; i++)
            {
                if (_containerViews[i] != null)
                    _containerViews[i].SetSparklesEnabled(enabled);
            }
        }

        public static BottleBoardView Create(PuzzleState puzzleState)
        {
            var go = new GameObject("BottleBoard");
            var board = go.AddComponent<BottleBoardView>();
            board.Initialize(puzzleState);
            return board;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
