using UnityEngine;
using UnityEngine.UI;

namespace JuiceSort.Game.UI.Components
{
    /// <summary>
    /// Scales a RawImage to fill its parent while preserving aspect ratio.
    /// Crops overflow edges (center-aligned). Like CSS background-size: cover.
    /// Attach to any RawImage that should fill the screen without stretching.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class AspectFillScaler : MonoBehaviour
    {
        private RawImage _rawImage;
        private RectTransform _rectTransform;
        private RectTransform _parentRect;

        void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = transform.parent.GetComponent<RectTransform>();
        }

        void Start()
        {
            ApplyAspectFill();
        }

        public void ApplyAspectFill()
        {
            if (_rawImage.texture == null) return;

            float imageAspect = (float)_rawImage.texture.width / _rawImage.texture.height;

            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);

            float parentWidth = _parentRect.rect.width;
            float parentHeight = _parentRect.rect.height;
            float parentAspect = parentWidth / parentHeight;

            float width, height;

            if (imageAspect > parentAspect)
            {
                // Image is wider — match height, crop sides
                height = parentHeight;
                width = height * imageAspect;
            }
            else
            {
                // Image is taller — match width, crop top/bottom
                width = parentWidth;
                height = width / imageAspect;
            }

            _rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
