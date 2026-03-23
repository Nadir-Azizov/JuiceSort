using System.Collections;
using UnityEngine;
using JuiceSort.Game.UI;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Manages per-bottle shader parameters for the LiquidFill shader.
    /// Converts ContainerData slots into contiguous color bands and sets
    /// shader uniforms via material instance. Updates only on state change.
    /// </summary>
    public class LiquidMaterialController : MonoBehaviour
    {
        private const int MaxBands = 6;
        private const float DefaultMaxVisualFill = 0.80f;
        private const float DimmedMultiplier = 0.7f;

        private Material _material;
        private SpriteRenderer _renderer;
        private int _slotCount;

        // Reusable arrays to avoid per-frame allocations
        private readonly float[] _fillArray = new float[MaxBands];
        private readonly Vector4[] _colorArray = new Vector4[MaxBands];

        /// <summary>
        /// Initializes the controller with a SpriteRenderer and creates a runtime material instance.
        /// </summary>
        public void Initialize(SpriteRenderer renderer, int slotCount)
        {
            _renderer = renderer;
            _slotCount = slotCount;

            var shader = Shader.Find("JuiceSort/LiquidFill");
            if (shader == null)
            {
                Debug.LogError("[LiquidMaterialController] JuiceSort/LiquidFill shader not found. Ensure LiquidFill.shader exists in Assets/Art/Shaders/");
                return;
            }

            _material = new Material(shader);
            _material.SetFloat("_MaxVisualFill", DefaultMaxVisualFill);
            _material.SetFloat("_DimMultiplier", 1f);
            _renderer.material = _material;
        }

        /// <summary>
        /// Converts slot data into contiguous color bands and updates shader uniforms.
        /// Consecutive same-color slots are merged into a single band.
        /// </summary>
        public void SetLayers(ContainerData data)
        {
            if (_material == null) return;

            int slotCount = data.SlotCount;
            int bandIndex = 0;

            // Walk slots bottom-to-top, merge same-color runs into bands
            int i = 0;
            while (i < slotCount && bandIndex < MaxBands)
            {
                var color = data.GetSlot(i);
                if (color == DrinkColor.None)
                    break; // empty slot = end of liquid

                // Count consecutive slots of same color
                int runLength = 1;
                while (i + runLength < slotCount && data.GetSlot(i + runLength) == color)
                    runLength++;

                // Band height = (runLength / totalSlots) — shader scales by _MaxVisualFill
                _fillArray[bandIndex] = (float)runLength / slotCount;
                Color c = ThemeConfig.GetDrinkColor(color);
                _colorArray[bandIndex] = new Vector4(c.r, c.g, c.b, c.a);

                bandIndex++;
                i += runLength;
            }

            // Clear unused bands
            for (int b = bandIndex; b < MaxBands; b++)
            {
                _fillArray[b] = 0f;
                _colorArray[b] = Vector4.zero;
            }

            _material.SetFloatArray("_FillLevels", _fillArray);
            _material.SetVectorArray("_LayerColors", _colorArray);
            _material.SetInt("_LayerCount", bandIndex);
        }

        /// <summary>
        /// Sets the dimming multiplier for completed bottles.
        /// </summary>
        public void SetDimmed(bool dimmed)
        {
            if (_material == null) return;
            _material.SetFloat("_DimMultiplier", dimmed ? DimmedMultiplier : 1f);
        }

        /// <summary>
        /// Sets a specific band's fill amount (for animated fills in Story 10.2).
        /// </summary>
        public void SetFillAmount(int bandIndex, float fill)
        {
            if (_material == null || bandIndex < 0 || bandIndex >= MaxBands) return;
            _fillArray[bandIndex] = fill;
            _material.SetFloatArray("_FillLevels", _fillArray);
        }

        /// <summary>
        /// Sets a specific band's color (for pour animation — sets new band color before fill lerp).
        /// </summary>
        public void SetBandColor(int bandIndex, Color color)
        {
            if (_material == null || bandIndex < 0 || bandIndex >= MaxBands) return;
            _colorArray[bandIndex] = new Vector4(color.r, color.g, color.b, color.a);
            _material.SetVectorArray("_LayerColors", _colorArray);
        }

        /// <summary>
        /// Sets the layer count (number of active bands in the shader).
        /// </summary>
        public void SetLayerCount(int count)
        {
            if (_material == null) return;
            _material.SetInt("_LayerCount", Mathf.Clamp(count, 0, MaxBands));
        }

        /// <summary>
        /// Sets the wobble X parameter directly.
        /// </summary>
        public void SetWobble(float wobbleX)
        {
            if (_material == null) return;
            _material.SetFloat("_WobbleX", wobbleX);
        }

        /// <summary>
        /// Sets the liquid tilt for gravity simulation during pour.
        /// Positive = liquid pools toward x=1 (right), Negative = toward x=0 (left).
        /// Set to 0 when bottle is upright.
        /// </summary>
        public void SetLiquidTilt(float tilt)
        {
            if (_material == null) return;
            _material.SetFloat("_LiquidTilt", tilt);
        }

        /// <summary>
        /// Sets the liquid Y-axis tilt for gravity simulation during pour.
        /// Positive = liquid pools toward y=1 (top in UV), Negative = toward y=0 (bottom in UV).
        /// Combined with SetLiquidTilt, enables correct gravity in any rotation.
        /// Set to 0 when bottle is upright.
        /// </summary>
        public void SetLiquidTiltY(float tiltY)
        {
            if (_material == null) return;
            _material.SetFloat("_LiquidTiltY", tiltY);
        }

        /// <summary>
        /// Sets the inner glow color and intensity for the liquid surface.
        /// </summary>
        public void SetGlow(Color glowColor, float intensity)
        {
            if (_material == null) return;
            _material.SetColor("_GlowColor", glowColor);
            _material.SetFloat("_GlowIntensity", intensity);
        }

        // Wobble animation state
        private Coroutine _wobbleCoroutine;
        private const float WobbleFrequency = 4f;
        private const float WobbleDecay = 5f;
        private const float WobbleDuration = 0.5f;

        /// <summary>
        /// Triggers a damped wobble oscillation on the liquid surface.
        /// Stops any existing wobble before starting a new one.
        /// </summary>
        public void TriggerWobble(float amplitude)
        {
            if (_material == null) return;
            StopWobble();
            _wobbleCoroutine = StartCoroutine(AnimateWobble(amplitude));
        }

        /// <summary>
        /// Stops any active wobble coroutine and snaps wobble to zero.
        /// </summary>
        public void StopWobble()
        {
            if (_wobbleCoroutine != null)
            {
                StopCoroutine(_wobbleCoroutine);
                _wobbleCoroutine = null;
            }
            if (_material != null)
            {
                _material.SetFloat("_WobbleX", 0f);
                _material.SetFloat("_WobbleZ", 0f);
            }
        }

        private IEnumerator AnimateWobble(float amplitude)
        {
            float elapsed = 0f;
            while (elapsed < WobbleDuration)
            {
                elapsed += Time.deltaTime;
                float wobble = Mathf.Sin(elapsed * WobbleFrequency * 2f * Mathf.PI)
                             * amplitude
                             * Mathf.Exp(-WobbleDecay * elapsed);
                _material.SetFloat("_WobbleX", wobble);
                yield return null;
            }

            // Snap to zero
            _material.SetFloat("_WobbleX", 0f);
            _material.SetFloat("_WobbleZ", 0f);
            _wobbleCoroutine = null;
        }

        /// <summary>
        /// Resets all shader parameters to default state.
        /// </summary>
        public void ResetToDefault()
        {
            if (_material == null) return;

            for (int i = 0; i < MaxBands; i++)
            {
                _fillArray[i] = 0f;
                _colorArray[i] = Vector4.zero;
            }

            _material.SetFloatArray("_FillLevels", _fillArray);
            _material.SetVectorArray("_LayerColors", _colorArray);
            _material.SetInt("_LayerCount", 0);
            _material.SetFloat("_DimMultiplier", 1f);
            _material.SetFloat("_WobbleX", 0f);
            _material.SetFloat("_WobbleZ", 0f);
            _material.SetFloat("_LiquidTilt", 0f);
            _material.SetFloat("_LiquidTiltY", 0f);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }
    }
}