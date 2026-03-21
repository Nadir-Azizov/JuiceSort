using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Visual representation of a single container.
    /// Handles rendering, selection state, and tap detection.
    /// </summary>
    public class ContainerView : MonoBehaviour, IPointerClickHandler
    {

        private SlotView[] _slotViews;
        private ContainerData _data;
        private Image _bodyImage;
        private ContainerState _state;
        private int _containerIndex;

        public ContainerState State => _state;
        public bool IsSelected => _state == ContainerState.Selected;
        public int ContainerIndex => _containerIndex;
        public ContainerData Data => _data;

        /// <summary>
        /// Fired when this container is tapped. Passes the container index.
        /// </summary>
        public event Action<int> OnTapped;

        /// <summary>
        /// Creates the container visual hierarchy and populates it with data.
        /// Must be called once after instantiation.
        /// </summary>
        public void Initialize(ContainerData data, int containerIndex = 0)
        {
            _data = data;
            _containerIndex = containerIndex;
            _state = ContainerState.Idle;
            CreateSlotViews(data.SlotCount);
            Refresh();
        }

        /// <summary>
        /// Replaces the data reference and refreshes visuals.
        /// Used by undo and restart to rebind views to new state.
        /// </summary>
        public void SetData(ContainerData data)
        {
            _data = data;
            Refresh();
        }

        public void Select()
        {
            _state = ContainerState.Selected;
            UpdateHighlight();
        }

        public void Deselect()
        {
            _state = ContainerState.Idle;
            UpdateHighlight();
        }

        /// <summary>
        /// Updates visuals to match current data state.
        /// </summary>
        public void Refresh()
        {
            if (_data == null || _slotViews == null)
                return;

            for (int i = 0; i < _data.SlotCount; i++)
            {
                _slotViews[i].SetColor(_data.GetSlot(i));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnTapped?.Invoke(_containerIndex);
        }

        private void UpdateHighlight()
        {
            if (_bodyImage == null)
                return;

            switch (_state)
            {
                case ContainerState.Selected:
                    _bodyImage.color = ThemeConfig.GetColor(ThemeColorType.ContainerSelected);
                    break;
                case ContainerState.Idle:
                default:
                    _bodyImage.color = ThemeConfig.GetColor(ThemeColorType.ContainerIdle);
                    break;
            }
        }

        private void CreateSlotViews(int slotCount)
        {
            _slotViews = new SlotView[slotCount];

            // Container body outline
            _bodyImage = GetComponent<Image>();
            if (_bodyImage == null)
                _bodyImage = gameObject.AddComponent<Image>();

            _bodyImage.color = ThemeConfig.GetColor(ThemeColorType.ContainerIdle);

            // Add vertical layout for slots (bottom to top)
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 3f;
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.reverseArrangement = false;

            // Add dark outline to container body
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.08f, 0.05f, 0.9f);
            outline.effectDistance = new Vector2(2f, 2f);

            // Create slot views bottom-up (index 0 = bottom, created first)
            for (int i = 0; i < slotCount; i++)
            {
                var slotGo = new GameObject($"Slot_{i}");
                slotGo.transform.SetParent(transform, false);

                var slotImage = slotGo.AddComponent<Image>();

                // Dark border on each slot for visibility
                var slotOutline = slotGo.AddComponent<Outline>();
                slotOutline.effectColor = new Color(0.08f, 0.06f, 0.04f, 0.7f);
                slotOutline.effectDistance = new Vector2(1.5f, 1.5f);

                var slotView = slotGo.AddComponent<SlotView>();

                _slotViews[i] = slotView;
            }
        }

        /// <summary>
        /// Creates a ContainerView programmatically with all required components.
        /// </summary>
        public static ContainerView Create(Transform parent, ContainerData data, int containerIndex = 0)
        {
            var go = new GameObject("Container");
            go.transform.SetParent(parent, false);

            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(120f, 300f);

            var containerView = go.AddComponent<ContainerView>();
            containerView.Initialize(data, containerIndex);

            return containerView;
        }
    }
}
