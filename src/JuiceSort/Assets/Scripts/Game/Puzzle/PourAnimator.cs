using System;
using System.Collections;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Coroutine-based pour animation. Lifts source bottle, tilts toward target,
    /// smoothly lerps liquid fill levels between bottles, then returns source to rest.
    /// </summary>
    public static class PourAnimator
    {
        // Timing
        private const float LiftDuration = 0.15f;
        private const float TiltDuration = 0.12f;
        private const float PourLerpDuration = 0.25f;
        private const float ReturnDuration = 0.2f;
        private const float LiftHeight = 0.4f;

        // Dynamic tilt range
        private const float MinTiltAngle = 15f;
        private const float MaxTiltAngle = 35f;

        // Band calculation
        private const int MaxBands = 6;

        /// <summary>
        /// Runs the full pour animation sequence with smooth liquid fill lerps.
        /// Call from a MonoBehaviour via StartCoroutine.
        /// onMidPour is invoked after visual transfer completes (apply game logic here).
        /// onComplete is invoked when the animation fully finishes.
        /// </summary>
        public static IEnumerator Animate(
            BottleContainerView source,
            BottleContainerView target,
            int pourCount,
            DrinkColor pourColor,
            int sourceTopIndex,
            int targetFirstEmpty,
            PourStreamVFX pourStream,
            Action onMidPour,
            Action onComplete)
        {
            var sourceTf = source.transform;
            var targetTf = target.transform;
            var srcController = source.LiquidController;
            var tgtController = target.LiquidController;

            Vector3 originalPos = sourceTf.localPosition;
            Quaternion originalRot = sourceTf.localRotation;

            // Dynamic tilt: more liquid = steeper tilt
            float pourRatio = (float)pourCount / Mathf.Max(1, source.Data.SlotCount);
            float tiltAngle = Mathf.Lerp(MinTiltAngle, MaxTiltAngle, pourRatio);

            // Determine tilt direction: tilt toward target
            float dx = targetTf.position.x - sourceTf.position.x;
            float direction = dx < 0f ? 1f : -1f;
            float tiltZ = tiltAngle * direction;

            // --- Pre-compute band states before and after pour ---
            // Source: before pour (current visual state) → after pour (less liquid)
            var srcBandsBefore = ComputeBands(source.Data);
            // Target: before pour (current visual state) → after pour (more liquid)
            var tgtBandsBefore = ComputeBands(target.Data);

            // Simulate post-pour state for visual target
            // Source loses pourCount slots from top, target gains pourCount slots
            var srcBandsAfter = ComputeBandsAfterSourcePour(source.Data, pourCount);
            var tgtBandsAfter = ComputeBandsAfterTargetPour(target.Data, pourCount, pourColor);

            // --- Phase 1: Lift source bottle up ---
            Vector3 liftedPos = originalPos + new Vector3(0f, LiftHeight, 0f);
            yield return LerpPosition(sourceTf, originalPos, liftedPos, LiftDuration);

            // --- Phase 2: Tilt source toward target ---
            Quaternion tiltedRot = Quaternion.Euler(0f, 0f, tiltZ);
            yield return LerpRotation(sourceTf, originalRot, tiltedRot, TiltDuration);

            // --- Phase 3: Smooth liquid transfer via fill lerps ---

            // Start pour stream VFX (if provided)
            Vector3 streamSourcePos = sourceTf.position + new Vector3(0f, LiftHeight * 0.5f, 0f);
            Vector3 streamTargetPos = targetTf.position + new Vector3(0f, LiftHeight * 0.3f, 0f);
            if (pourStream != null)
            {
                Color streamColor = UI.ThemeConfig.GetDrinkColor(pourColor);
                pourStream.StartStream(streamSourcePos, streamTargetPos, streamColor);
            }

            if (srcController != null && tgtController != null)
            {
                // Set target band colors to post-pour state BEFORE lerp starts.
                SetupTargetColors(tgtController, target.Data, pourCount, pourColor);

                // Set layer counts to max of before/after so all bands are active during lerp
                int srcLayerCount = Mathf.Max(CountActiveBands(srcBandsBefore), CountActiveBands(srcBandsAfter));
                int tgtLayerCount = Mathf.Max(CountActiveBands(tgtBandsBefore), CountActiveBands(tgtBandsAfter));
                srcController.SetLayerCount(srcLayerCount);
                tgtController.SetLayerCount(tgtLayerCount);

                float elapsed = 0f;
                while (elapsed < PourLerpDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / PourLerpDuration));

                    // Lerp source bands from before to after
                    for (int b = 0; b < MaxBands; b++)
                    {
                        float srcFill = Mathf.Lerp(srcBandsBefore.fills[b], srcBandsAfter.fills[b], t);
                        srcController.SetFillAmount(b, srcFill);
                    }

                    // Lerp target bands from before to after
                    for (int b = 0; b < MaxBands; b++)
                    {
                        float tgtFill = Mathf.Lerp(tgtBandsBefore.fills[b], tgtBandsAfter.fills[b], t);
                        tgtController.SetFillAmount(b, tgtFill);
                    }

                    // Update stream positions (source moves during tilt)
                    if (pourStream != null)
                    {
                        streamSourcePos = sourceTf.position + sourceTf.up * LiftHeight * 0.3f;
                        pourStream.UpdatePositions(streamSourcePos, streamTargetPos);
                    }

                    yield return null;
                }
            }

            // Stop pour stream before return phase
            if (pourStream != null)
                pourStream.StopStream();

            // Wobble target bottle after receiving liquid
            if (tgtController != null)
                tgtController.TriggerWobble(0.03f);

            // Invoke mid-pour callback (game logic: ExecutePour already done, just a sync point)
            onMidPour?.Invoke();

            // Small pause at peak
            yield return new WaitForSeconds(0.05f);

            // --- Phase 4: Return source to original position ---
            yield return LerpRotation(sourceTf, tiltedRot, originalRot, ReturnDuration * 0.5f);
            yield return LerpPosition(sourceTf, liftedPos, originalPos, ReturnDuration * 0.5f);

            // Final refresh to ensure data-driven state is correct
            source.Refresh();
            target.Refresh();

            onComplete?.Invoke();
        }

        // --- Band computation helpers ---

        private struct BandState
        {
            public float[] fills;
            public BandState(int size) { fills = new float[size]; }
        }

        /// <summary>
        /// Computes contiguous color bands from container data (same algorithm as LiquidMaterialController).
        /// Returns fill heights as fractions of total slot count.
        /// </summary>
        private static BandState ComputeBands(ContainerData data)
        {
            var state = new BandState(MaxBands);
            int slotCount = data.SlotCount;
            int bandIndex = 0;

            int i = 0;
            while (i < slotCount && bandIndex < MaxBands)
            {
                var color = data.GetSlot(i);
                if (color == DrinkColor.None) break;

                int runLength = 1;
                while (i + runLength < slotCount && data.GetSlot(i + runLength) == color)
                    runLength++;

                state.fills[bandIndex] = (float)runLength / slotCount;
                bandIndex++;
                i += runLength;
            }

            return state;
        }

        /// <summary>
        /// Computes source band state after losing pourCount slots from the top.
        /// </summary>
        private static BandState ComputeBandsAfterSourcePour(ContainerData data, int pourCount)
        {
            var state = new BandState(MaxBands);
            int slotCount = data.SlotCount;

            // Count filled slots
            int filledCount = data.FilledCount();
            int remainingFilled = filledCount - pourCount;
            if (remainingFilled <= 0) return state; // all poured out

            int bandIndex = 0;
            int i = 0;
            int slotsProcessed = 0;
            while (i < slotCount && bandIndex < MaxBands && slotsProcessed < remainingFilled)
            {
                var color = data.GetSlot(i);
                if (color == DrinkColor.None) break;

                int runLength = 1;
                while (i + runLength < slotCount && data.GetSlot(i + runLength) == color)
                    runLength++;

                // Clamp run to remaining slots
                int effectiveRun = Mathf.Min(runLength, remainingFilled - slotsProcessed);
                state.fills[bandIndex] = (float)effectiveRun / slotCount;
                bandIndex++;
                slotsProcessed += effectiveRun;
                i += runLength;
            }

            return state;
        }

        /// <summary>
        /// Computes target band state after gaining pourCount slots of pourColor on top.
        /// </summary>
        private static BandState ComputeBandsAfterTargetPour(ContainerData data, int pourCount, DrinkColor pourColor)
        {
            var state = new BandState(MaxBands);
            int slotCount = data.SlotCount;
            int bandIndex = 0;

            // Existing bands first
            int i = 0;
            DrinkColor lastColor = DrinkColor.None;
            while (i < slotCount && bandIndex < MaxBands)
            {
                var color = data.GetSlot(i);
                if (color == DrinkColor.None) break;

                int runLength = 1;
                while (i + runLength < slotCount && data.GetSlot(i + runLength) == color)
                    runLength++;

                state.fills[bandIndex] = (float)runLength / slotCount;
                lastColor = color;
                bandIndex++;
                i += runLength;
            }

            // Add poured liquid on top
            if (pourCount > 0 && bandIndex < MaxBands)
            {
                float pourFill = (float)pourCount / slotCount;

                // Merge with top band if same color
                if (bandIndex > 0 && lastColor == pourColor)
                {
                    state.fills[bandIndex - 1] += pourFill;
                }
                else
                {
                    state.fills[bandIndex] = pourFill;
                }
            }

            return state;
        }

        /// <summary>
        /// Sets target controller's band colors to reflect post-pour state.
        /// New bands get the pour color so they're visible when fill lerps up from 0.
        /// </summary>
        private static void SetupTargetColors(LiquidMaterialController controller, ContainerData data, int pourCount, DrinkColor pourColor)
        {
            int slotCount = data.SlotCount;
            int bandIndex = 0;
            DrinkColor lastColor = DrinkColor.None;

            // Set existing band colors
            int i = 0;
            while (i < slotCount && bandIndex < MaxBands)
            {
                var color = data.GetSlot(i);
                if (color == DrinkColor.None) break;

                int runLength = 1;
                while (i + runLength < slotCount && data.GetSlot(i + runLength) == color)
                    runLength++;

                controller.SetBandColor(bandIndex, UI.ThemeConfig.GetDrinkColor(color));
                lastColor = color;
                bandIndex++;
                i += runLength;
            }

            // Set color for the new incoming band (if it's a new band, not merged)
            if (pourCount > 0 && lastColor != pourColor && bandIndex < MaxBands)
            {
                controller.SetBandColor(bandIndex, UI.ThemeConfig.GetDrinkColor(pourColor));
            }
        }

        private static int CountActiveBands(BandState state)
        {
            int count = 0;
            for (int i = 0; i < state.fills.Length; i++)
            {
                if (state.fills[i] > 0f) count++;
                else break;
            }
            return count;
        }

        // --- Lerp helpers (unchanged) ---

        private static IEnumerator LerpPosition(Transform tf, Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                tf.localPosition = Vector3.Lerp(from, to, t);
                yield return null;
            }
            tf.localPosition = to;
        }

        private static IEnumerator LerpRotation(Transform tf, Quaternion from, Quaternion to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                tf.localRotation = Quaternion.Lerp(from, to, t);
                yield return null;
            }
            tf.localRotation = to;
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }
    }
}
