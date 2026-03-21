using System;
using UnityEngine;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// SpriteRenderer-based container view using the Bottle Fill Shader Graph.
    /// Renders liquid as colored bands inside a bottle mask, with a glass frame overlay.
    /// </summary>
    public class BottleContainerView : MonoBehaviour
    {
        private SpriteRenderer _fillRenderer;   // mask sprite + fill shader
        private SpriteRenderer _frameRenderer;  // glass outline overlay
        private ContainerData _data;
        private ContainerState _state;
        private int _containerIndex;
        private MaterialPropertyBlock _propBlock;

        // Shader property IDs (cached for performance)
        private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");
        private static readonly int SpriteHeightId = Shader.PropertyToID("_SpriteHeight");
        private static readonly int BandScaleId = Shader.PropertyToID("_BandScale");
        private static readonly int BandCountId = Shader.PropertyToID("_BandCount");
        private static readonly int BandColor1Id = Shader.PropertyToID("_BandColor1");
        private static readonly int BandColor2Id = Shader.PropertyToID("_BandColor2");
        private static readonly int BandColor3Id = Shader.PropertyToID("_BandColor3");
        private static readonly int BandColor4Id = Shader.PropertyToID("_BandColor4");

        // Cached assets
        private static Sprite _cachedMaskSprite;
        private static Sprite _cachedFrameSprite;
        private static Material _cachedFillMaterial;

        public ContainerState State => _state;
        public bool IsSelected => _state == ContainerState.Selected;
        public int ContainerIndex => _containerIndex;
        public ContainerData Data => _data;

        public event Action<int> OnTapped;

        private static Sprite LoadMaskSprite()
        {
            if (_cachedMaskSprite == null)
            {
                var sprites = Resources.LoadAll<Sprite>("Bottles/bottle_mask");
                if (sprites != null && sprites.Length > 0)
                    _cachedMaskSprite = sprites[0];
                else
                    Debug.LogError("[BottleContainerView] Failed to load bottle_mask sprite from Resources/Bottles/");
            }
            return _cachedMaskSprite;
        }

        private static Sprite LoadFrameSprite()
        {
            if (_cachedFrameSprite == null)
            {
                var sprites = Resources.LoadAll<Sprite>("Bottles/bottle_frame");
                if (sprites != null && sprites.Length > 0)
                    _cachedFrameSprite = sprites[0];
                else
                    Debug.LogError("[BottleContainerView] Failed to load bottle_frame sprite from Resources/Bottles/");
            }
            return _cachedFrameSprite;
        }

        private static Material LoadFillMaterial()
        {
            if (_cachedFillMaterial == null)
            {
                _cachedFillMaterial = Resources.Load<Material>("Bottles/BottleFillShaderGraph");
                if (_cachedFillMaterial == null)
                    Debug.LogError("[BottleContainerView] Failed to load BottleFillShaderGraph material from Resources/Bottles/");
            }
            return _cachedFillMaterial;
        }

        public void Initialize(ContainerData data, int containerIndex)
        {
            _data = data;
            _containerIndex = containerIndex;
            _state = ContainerState.Idle;
            _propBlock = new MaterialPropertyBlock();
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
            if (_data == null || _fillRenderer == null || _propBlock == null)
                return;

            int slotCount = _data.SlotCount;
            int filledCount = _data.FilledCount();

            // Fill amount: proportion of slots filled
            float fillAmount = slotCount > 0 ? (float)filledCount / slotCount : 0f;

            // Compute sprite height for the shader
            float spriteHeight = 0f;
            if (_fillRenderer.sprite != null)
                spriteHeight = _fillRenderer.sprite.bounds.size.y * _fillRenderer.transform.lossyScale.y;

            _fillRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(FillAmountId, fillAmount);
            _propBlock.SetFloat(SpriteHeightId, spriteHeight);
            _propBlock.SetFloat(BandScaleId, 1f);
            _propBlock.SetFloat(BandCountId, Mathf.Clamp(filledCount, 1, 4));

            // Map slots to band colors
            // Shader: BandColor1 = topmost, BandColor4 = bottom
            // Container: slot 0 = bottom, slot N-1 = top
            // So we reverse: slot[N-1] → BandColor1, slot[N-2] → BandColor2, etc.
            var bandColorIds = new[] { BandColor1Id, BandColor2Id, BandColor3Id, BandColor4Id };
            int bandIndex = 0;

            // Gather filled colors from top to bottom
            for (int i = slotCount - 1; i >= 0 && bandIndex < 4; i--)
            {
                var drinkColor = _data.GetSlot(i);
                if (drinkColor != DrinkColor.None)
                {
                    _propBlock.SetColor(bandColorIds[bandIndex], ThemeConfig.GetDrinkColor(drinkColor));
                    bandIndex++;
                }
            }

            // Fill remaining bands with transparent
            for (; bandIndex < 4; bandIndex++)
                _propBlock.SetColor(bandColorIds[bandIndex], new Color(0, 0, 0, 0));

            _fillRenderer.SetPropertyBlock(_propBlock);
        }

        private void UpdateHighlight()
        {
            if (_frameRenderer != null)
            {
                _frameRenderer.color = _state == ContainerState.Selected
                    ? new Color(1f, 0.88f, 0.3f, 1f)   // golden glow
                    : new Color(1f, 1f, 1f, 0.9f);      // normal glass
            }
        }

        private void OnMouseDown()
        {
            OnTapped?.Invoke(_containerIndex);
        }

        public static BottleContainerView Create(Transform parent, ContainerData data, int containerIndex, float xPosition)
        {
            var maskSprite = LoadMaskSprite();
            var frameSprite = LoadFrameSprite();
            var fillMaterial = LoadFillMaterial();

            var go = new GameObject($"Bottle_{containerIndex}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(xPosition, 0f, 0f);

            // Scale the bottle to a reasonable world-space size
            // Mask sprite is ~510x1017 pixels at 100 PPU = ~5.1 x 10.17 units
            // We want bottles about 0.9 x 1.8 units, so scale ~0.18
            float bottleScale = 0.18f;
            go.transform.localScale = new Vector3(bottleScale, bottleScale, 1f);

            // Fill renderer: mask sprite with shader material
            var fillRenderer = go.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = maskSprite;
            if (fillMaterial != null)
                fillRenderer.sharedMaterial = fillMaterial;
            fillRenderer.sortingOrder = 0;

            // Frame renderer: glass outline on top
            var frameGo = new GameObject("Frame");
            frameGo.transform.SetParent(go.transform, false);
            frameGo.transform.localPosition = Vector3.zero;
            frameGo.transform.localScale = Vector3.one;

            var frameRenderer = frameGo.AddComponent<SpriteRenderer>();
            frameRenderer.sprite = frameSprite;
            frameRenderer.sortingOrder = 1;
            frameRenderer.color = new Color(1f, 1f, 1f, 0.9f);

            // Collider for tap detection
            var collider = go.AddComponent<BoxCollider2D>();
            if (maskSprite != null)
                collider.size = maskSprite.bounds.size;
            else
                collider.size = new Vector2(5f, 10f);

            var view = go.AddComponent<BottleContainerView>();
            view._fillRenderer = fillRenderer;
            view._frameRenderer = frameRenderer;
            view.Initialize(data, containerIndex);

            return view;
        }
    }
}
