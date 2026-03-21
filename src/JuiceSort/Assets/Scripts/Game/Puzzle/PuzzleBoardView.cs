using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Manages the visual layout of all containers on the puzzle board.
    /// Creates a UI Canvas with responsive scaling and arranges containers horizontally.
    /// </summary>
    public class PuzzleBoardView : MonoBehaviour
    {
        private ContainerView[] _containerViews;
        private Canvas _canvas;
        private Transform _containerAreaTransform;

        /// <summary>
        /// Fired when any container is tapped. Passes the container index.
        /// </summary>
        public event Action<int> OnContainerTapped;

        /// <summary>
        /// Fired when empty background space is tapped.
        /// </summary>
        public event Action OnBackgroundTapped;

        /// <summary>
        /// Creates the full board UI and renders the puzzle state.
        /// </summary>
        public void Initialize(PuzzleState puzzleState)
        {
            CreateCanvas();
            CreateBackgroundTapCatcher();
            CreateContainerLayout(puzzleState);
        }

        private void CreateCanvas()
        {
            // Canvas setup
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 0;

            // Canvas Scaler: Scale With Screen Size, reference 1080x1920, match 0.5
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            // Graphic Raycaster for touch input
            gameObject.AddComponent<GraphicRaycaster>();
            // EventSystem created globally by BootLoader
        }

        private void CreateBackgroundTapCatcher()
        {
            var bgGo = new GameObject("BackgroundTapCatcher");
            bgGo.transform.SetParent(transform, false);

            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0f);
            bgImage.raycastTarget = true;

            var catcher = bgGo.AddComponent<BackgroundTapCatcher>();
            catcher.OnTapped += () => OnBackgroundTapped?.Invoke();
        }

        private void CreateContainerLayout(PuzzleState puzzleState)
        {
            // Container area: positioned in lower ~60% of screen
            var containerArea = new GameObject("ContainerArea");
            containerArea.transform.SetParent(transform, false);
            _containerAreaTransform = containerArea.transform;

            var areaRect = containerArea.AddComponent<RectTransform>();
            areaRect.anchorMin = new Vector2(0f, 0f);
            areaRect.anchorMax = new Vector2(1f, 0.6f);
            areaRect.offsetMin = new Vector2(40f, 40f);
            areaRect.offsetMax = new Vector2(-40f, -20f);

            // Horizontal layout for containers
            var layout = containerArea.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 16f;
            layout.padding = new RectOffset(16, 16, 16, 16);

            // Spawn container views
            _containerViews = new ContainerView[puzzleState.ContainerCount];
            for (int i = 0; i < puzzleState.ContainerCount; i++)
            {
                var containerData = puzzleState.GetContainer(i);
                var containerView = ContainerView.Create(containerArea.transform, containerData, i);
                _containerViews[i] = containerView;

                // Subscribe to container tap events
                containerView.OnTapped += HandleContainerTapped;

                // Set preferred height for containers
                var layoutElement = containerView.gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredHeight = 300f;
                layoutElement.flexibleWidth = 1f;
            }
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

        /// <summary>
        /// Rebinds all container views to a new puzzle state.
        /// Used by undo and restart.
        /// </summary>
        public void RebindPuzzle(PuzzleState puzzleState)
        {
            for (int i = 0; i < _containerViews.Length; i++)
            {
                _containerViews[i].SetData(puzzleState.GetContainer(i));
            }
        }

        /// <summary>
        /// Dynamically adds a new container to the existing layout.
        /// Used by extra bottle mechanic — no full board rebuild needed.
        /// </summary>
        public void AddContainerView(ContainerData data, int index)
        {
            var containerView = ContainerView.Create(_containerAreaTransform, data, index);
            containerView.OnTapped += HandleContainerTapped;

            var layoutElement = containerView.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 300f;
            layoutElement.flexibleWidth = 1f;

            // Resize array
            var newViews = new ContainerView[_containerViews.Length + 1];
            for (int i = 0; i < _containerViews.Length; i++)
                newViews[i] = _containerViews[i];
            newViews[_containerViews.Length] = containerView;
            _containerViews = newViews;
        }

        public ContainerView GetContainerView(int index)
        {
            return _containerViews[index];
        }

        public int ContainerCount => _containerViews != null ? _containerViews.Length : 0;

        /// <summary>
        /// Creates the PuzzleBoardView on a new GameObject.
        /// </summary>
        public static PuzzleBoardView Create(PuzzleState puzzleState)
        {
            var go = new GameObject("PuzzleBoard");
            var board = go.AddComponent<PuzzleBoardView>();
            board.Initialize(puzzleState);
            return board;
        }
    }
}
