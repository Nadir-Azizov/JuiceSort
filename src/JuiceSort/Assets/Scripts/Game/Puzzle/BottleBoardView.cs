using System;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// World-space board using BottleContainerView (SpriteRenderer + shader).
    /// Replaces the old Canvas-based PuzzleBoardView for bottle rendering.
    /// </summary>
    public class BottleBoardView : MonoBehaviour
    {
        private BottleContainerView[] _containerViews;

        public event Action<int> OnContainerTapped;

        public void Initialize(PuzzleState puzzleState)
        {
            int count = puzzleState.ContainerCount;
            _containerViews = new BottleContainerView[count];

            // Get camera info to fit bottles on screen
            var cam = Camera.main;
            float camHeight = cam != null ? cam.orthographicSize * 2f : 10f;
            float camWidth = camHeight * Screen.width / Screen.height;

            // Bottle dimensions (mask sprite ~510x1017px at 100PPU, scaled 0.18)
            float bottleWorldWidth = 0.92f;
            float bottleWorldHeight = 1.83f;

            // Use 80% of screen width for bottles, leave margins
            float usableWidth = camWidth * 0.85f;
            float bottleSpacing = usableWidth / count;

            // Cap spacing so bottles don't overlap
            if (bottleSpacing < bottleWorldWidth * 1.1f)
                bottleSpacing = bottleWorldWidth * 1.1f;

            float totalWidth = (count - 1) * bottleSpacing;
            float startX = -totalWidth / 2f;

            // Position board: center horizontally, lower portion of screen
            float camBottom = cam != null ? cam.transform.position.y - cam.orthographicSize : -5f;
            float boardY = camBottom + bottleWorldHeight * 0.6f;
            transform.position = new Vector3(
                cam != null ? cam.transform.position.x : 0f,
                boardY,
                0f
            );

            for (int i = 0; i < count; i++)
            {
                float x = startX + i * bottleSpacing;
                var containerData = puzzleState.GetContainer(i);
                var view = BottleContainerView.Create(transform, containerData, i, x);
                view.OnTapped += HandleContainerTapped;
                _containerViews[i] = view;
            }

            Debug.Log($"[BottleBoardView] Created {count} bottles. CamSize={camHeight}, spacing={bottleSpacing:F2}, boardY={boardY:F2}");
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
        /// Dynamically adds a container (for extra bottle).
        /// </summary>
        public void AddContainerView(ContainerData data, int index)
        {
            float bottleSpacing = _containerViews.Length <= 5 ? 1.2f : 0.9f;
            float scale = _containerViews.Length <= 5 ? 0.8f : 0.6f;

            int newCount = _containerViews.Length + 1;
            float totalWidth = (newCount - 1) * bottleSpacing * scale;
            float x = -totalWidth / 2f + (_containerViews.Length) * bottleSpacing * scale;

            var view = BottleContainerView.Create(transform, data, index, x);
            view.transform.localScale = new Vector3(scale, scale, 1f);
            view.OnTapped += HandleContainerTapped;

            var newViews = new BottleContainerView[newCount];
            for (int i = 0; i < _containerViews.Length; i++)
                newViews[i] = _containerViews[i];
            newViews[_containerViews.Length] = view;
            _containerViews = newViews;
        }

        public int ContainerCount => _containerViews != null ? _containerViews.Length : 0;

        public static BottleBoardView Create(PuzzleState puzzleState)
        {
            var go = new GameObject("BottleBoard");
            var board = go.AddComponent<BottleBoardView>();
            board.Initialize(puzzleState);
            return board;
        }
    }
}
