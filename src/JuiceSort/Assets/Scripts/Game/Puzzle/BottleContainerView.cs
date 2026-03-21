using System;
using UnityEngine;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// SpriteRenderer-based container view.
    /// Uses simple colored slots (shader integration done later in Unity Editor).
    /// Touch detection via OnMouseDown (requires Collider2D).
    /// </summary>
    public class BottleContainerView : MonoBehaviour
    {
        private SpriteRenderer[] _slotRenderers;
        private SpriteRenderer _bgRenderer;
        private ContainerData _data;
        private ContainerState _state;
        private int _containerIndex;

        public ContainerState State => _state;
        public bool IsSelected => _state == ContainerState.Selected;
        public int ContainerIndex => _containerIndex;
        public ContainerData Data => _data;

        public event Action<int> OnTapped;

        private static Sprite _cachedWhiteSprite;

        private static Sprite GetWhiteSprite()
        {
            if (_cachedWhiteSprite == null)
            {
                var tex = new Texture2D(4, 4);
                for (int x = 0; x < 4; x++)
                    for (int y = 0; y < 4; y++)
                        tex.SetPixel(x, y, Color.white);
                tex.Apply();
                _cachedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return _cachedWhiteSprite;
        }

        public void Initialize(ContainerData data, int containerIndex)
        {
            _data = data;
            _containerIndex = containerIndex;
            _state = ContainerState.Idle;
            Refresh();
        }

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

        public void Refresh()
        {
            if (_data == null || _slotRenderers == null)
                return;

            for (int i = 0; i < _data.SlotCount && i < _slotRenderers.Length; i++)
            {
                var color = _data.GetSlot(i);
                if (color == DrinkColor.None)
                    _slotRenderers[i].color = new Color(0.92f, 0.9f, 0.85f, 0.35f);
                else
                    _slotRenderers[i].color = ThemeConfig.GetDrinkColor(color);
            }
        }

        private void UpdateHighlight()
        {
            if (_bgRenderer != null)
            {
                _bgRenderer.color = _state == ContainerState.Selected
                    ? new Color(1f, 0.88f, 0.3f, 0.95f) // golden
                    : new Color(0.25f, 0.22f, 0.18f, 0.85f); // dark container body
            }
        }

        private void OnMouseDown()
        {
            OnTapped?.Invoke(_containerIndex);
        }

        public static BottleContainerView Create(Transform parent, ContainerData data, int containerIndex, float xPosition)
        {
            var whiteSprite = GetWhiteSprite();
            int slotCount = data.SlotCount;

            var go = new GameObject($"Bottle_{containerIndex}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(xPosition, 0f, 0f);

            // Container body (dark background)
            var bgRenderer = go.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = whiteSprite;
            bgRenderer.color = new Color(0.25f, 0.22f, 0.18f, 0.85f);
            bgRenderer.sortingOrder = 0;
            // Scale: width=0.9 units, height=2.2 units
            go.transform.localScale = new Vector3(0.9f, 2.2f, 1f);

            // Slot renderers (colored bars, bottom to top)
            var slotRenderers = new SpriteRenderer[slotCount];
            float usableHeight = 0.9f; // fraction of container height
            float slotHeight = usableHeight / slotCount;
            float gap = 0.015f;
            float startY = -usableHeight / 2f;

            for (int i = 0; i < slotCount; i++)
            {
                var slotGo = new GameObject($"Slot_{i}");
                slotGo.transform.SetParent(go.transform, false);

                float yPos = startY + (i * slotHeight) + (slotHeight / 2f);
                slotGo.transform.localPosition = new Vector3(0f, yPos, -0.01f);
                slotGo.transform.localScale = new Vector3(0.85f, slotHeight - gap, 1f);

                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = whiteSprite;
                sr.sortingOrder = 1;
                slotRenderers[i] = sr;
            }

            // Collider for tap detection (in local space of the scaled GO)
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);

            var view = go.AddComponent<BottleContainerView>();
            view._slotRenderers = slotRenderers;
            view._bgRenderer = bgRenderer;
            view.Initialize(data, containerIndex);

            return view;
        }
    }
}
