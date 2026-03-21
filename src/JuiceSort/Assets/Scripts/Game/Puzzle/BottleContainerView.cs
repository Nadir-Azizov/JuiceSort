using System;
using System.Collections;
using UnityEngine;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// SpriteRenderer-based container view using SpriteMask for liquid clipping.
    /// Renders each liquid slot as an equal-height colored sprite inside the bottle mask.
    /// </summary>
    public class BottleContainerView : MonoBehaviour
    {
        private SpriteRenderer _frameRenderer;
        private SpriteMask _mask;
        private SpriteRenderer[] _slotRenderers;
        private ContainerData _data;
        private ContainerState _state;
        private int _containerIndex;

        // Selection animation state
        private Coroutine _selectionCoroutine;
        private Vector3 _restLocalPosition;
        private Vector3 _baseScale;

        // Selection animation constants
        private const float SelectLiftHeight = 0.25f;
        private const float SelectScaleBump = 1.08f;
        private const float SelectAnimDuration = 0.15f;
        private const float DeselectAnimDuration = 0.12f;

        private static readonly Color SelectedFrameColor = new Color(1f, 0.88f, 0.3f, 1f);
        private static readonly Color IdleFrameColor = new Color(1f, 1f, 1f, 0.9f);

        // Cached assets
        private static Sprite _cachedMaskSprite;
        private static Sprite _cachedFrameSprite;
        private static Sprite _cachedWhiteSprite;

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

        private static Sprite LoadWhiteSprite()
        {
            if (_cachedWhiteSprite == null)
            {
                var tex = new Texture2D(4, 4);
                var pixels = new Color[16];
                for (int i = 0; i < 16; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
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
            CaptureRestState();
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
            StopSelectionAnim();
            _selectionCoroutine = StartCoroutine(AnimateSelect());
        }

        public void Deselect()
        {
            _state = ContainerState.Idle;
            StopSelectionAnim();
            _selectionCoroutine = StartCoroutine(AnimateDeselect());
        }

        /// <summary>
        /// Instantly resets to idle visual state without animation (used by pour animator return).
        /// </summary>
        public void ResetVisualState()
        {
            StopSelectionAnim();
            _state = ContainerState.Idle;
            transform.localPosition = _restLocalPosition;
            transform.localScale = _baseScale;
            if (_frameRenderer != null)
                _frameRenderer.color = IdleFrameColor;
        }

        private void StopSelectionAnim()
        {
            if (_selectionCoroutine != null)
            {
                StopCoroutine(_selectionCoroutine);
                _selectionCoroutine = null;
            }
        }

        private IEnumerator AnimateSelect()
        {
            Vector3 fromPos = transform.localPosition;
            Vector3 toPos = _restLocalPosition + new Vector3(0f, SelectLiftHeight, 0f);
            Vector3 toScale = _baseScale * SelectScaleBump;

            float elapsed = 0f;
            while (elapsed < SelectAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(Mathf.Clamp01(elapsed / SelectAnimDuration));
                transform.localPosition = Vector3.Lerp(fromPos, toPos, t);
                transform.localScale = Vector3.Lerp(_baseScale, toScale, t);
                if (_frameRenderer != null)
                    _frameRenderer.color = Color.Lerp(IdleFrameColor, SelectedFrameColor, t);
                yield return null;
            }
            transform.localPosition = toPos;
            transform.localScale = toScale;
            if (_frameRenderer != null)
                _frameRenderer.color = SelectedFrameColor;

            _selectionCoroutine = null;
        }

        private IEnumerator AnimateDeselect()
        {
            Vector3 fromPos = transform.localPosition;
            Vector3 fromScale = transform.localScale;
            Color fromColor = _frameRenderer != null ? _frameRenderer.color : IdleFrameColor;

            float elapsed = 0f;
            while (elapsed < DeselectAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / DeselectAnimDuration));
                transform.localPosition = Vector3.Lerp(fromPos, _restLocalPosition, t);
                transform.localScale = Vector3.Lerp(fromScale, _baseScale, t);
                if (_frameRenderer != null)
                    _frameRenderer.color = Color.Lerp(fromColor, IdleFrameColor, t);
                yield return null;
            }
            transform.localPosition = _restLocalPosition;
            transform.localScale = _baseScale;
            if (_frameRenderer != null)
                _frameRenderer.color = IdleFrameColor;

            _selectionCoroutine = null;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        public void Refresh()
        {
            if (_data == null || _slotRenderers == null)
                return;

            int slotCount = _data.SlotCount;

            for (int i = 0; i < _slotRenderers.Length && i < slotCount; i++)
            {
                var drinkColor = _data.GetSlot(i);
                if (drinkColor != DrinkColor.None)
                {
                    _slotRenderers[i].color = ThemeConfig.GetDrinkColor(drinkColor);
                    _slotRenderers[i].enabled = true;
                }
                else
                {
                    _slotRenderers[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// Shows or hides a specific slot renderer (used by PourAnimator).
        /// </summary>
        public void SetSlotVisible(int slotIndex, bool visible)
        {
            if (_slotRenderers != null && slotIndex >= 0 && slotIndex < _slotRenderers.Length)
                _slotRenderers[slotIndex].enabled = visible;
        }

        /// <summary>
        /// Sets a slot's color and makes it visible (used by PourAnimator for target fill).
        /// </summary>
        public void SetSlotColorAndShow(int slotIndex, DrinkColor drinkColor)
        {
            if (_slotRenderers != null && slotIndex >= 0 && slotIndex < _slotRenderers.Length)
            {
                _slotRenderers[slotIndex].color = ThemeConfig.GetDrinkColor(drinkColor);
                _slotRenderers[slotIndex].enabled = true;
            }
        }

        /// <summary>
        /// Captures the rest position and scale. Must be called after the bottle is positioned.
        /// </summary>
        private void CaptureRestState()
        {
            _restLocalPosition = transform.localPosition;
            _baseScale = transform.localScale;
        }

        private void OnMouseDown()
        {
            OnTapped?.Invoke(_containerIndex);
        }

        public static BottleContainerView Create(Transform parent, ContainerData data, int containerIndex, float xPosition)
        {
            var maskSprite = LoadMaskSprite();
            var frameSprite = LoadFrameSprite();
            var whiteSprite = LoadWhiteSprite();

            var go = new GameObject($"Bottle_{containerIndex}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(xPosition, 0f, 0f);

            // Scale the bottle
            float bottleScale = 0.18f;
            go.transform.localScale = new Vector3(bottleScale, bottleScale, 1f);

            // SpriteMask clips liquid sprites to the bottle shape
            var mask = go.AddComponent<SpriteMask>();
            mask.sprite = maskSprite;

            // Compute slot geometry from the mask sprite bounds
            float spriteWidth = maskSprite != null ? maskSprite.bounds.size.x : 5f;
            float spriteHeight = maskSprite != null ? maskSprite.bounds.size.y : 10f;

            int slotCount = data.SlotCount;
            float slotHeight = spriteHeight / slotCount;
            // Bottom of sprite is at -spriteHeight/2
            float bottomY = -spriteHeight / 2f;

            // Create one colored sprite per slot (bottom-up: slot 0 = bottom)
            var slotRenderers = new SpriteRenderer[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                var slotGo = new GameObject($"Slot_{i}");
                slotGo.transform.SetParent(go.transform, false);

                float slotCenterY = bottomY + slotHeight * i + slotHeight / 2f;
                slotGo.transform.localPosition = new Vector3(0f, slotCenterY, 0f);
                // Scale the white sprite (1x1 unit) to fill the slot
                slotGo.transform.localScale = new Vector3(spriteWidth, slotHeight, 1f);

                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = whiteSprite;
                sr.sortingOrder = 0;
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                slotRenderers[i] = sr;
            }

            // Frame renderer: glass outline on top (not masked)
            var frameGo = new GameObject("Frame");
            frameGo.transform.SetParent(go.transform, false);
            frameGo.transform.localPosition = Vector3.zero;
            frameGo.transform.localScale = Vector3.one;

            var frameRenderer = frameGo.AddComponent<SpriteRenderer>();
            frameRenderer.sprite = frameSprite;
            frameRenderer.sortingOrder = 2;
            frameRenderer.color = new Color(1f, 1f, 1f, 0.9f);
            frameRenderer.maskInteraction = SpriteMaskInteraction.None;

            // Collider for tap detection
            var collider = go.AddComponent<BoxCollider2D>();
            if (maskSprite != null)
                collider.size = maskSprite.bounds.size;
            else
                collider.size = new Vector2(5f, 10f);

            var view = go.AddComponent<BottleContainerView>();
            view._frameRenderer = frameRenderer;
            view._mask = mask;
            view._slotRenderers = slotRenderers;
            view.Initialize(data, containerIndex);

            return view;
        }
    }
}
