using System;
using System.Collections;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Coroutine-based pour animation. Source bottle lifts, moves to hover above
    /// target, tilts progressively based on liquid depth while lerping fill levels,
    /// then returns smoothly to rest position.
    /// </summary>
    public static class PourAnimator
    {
        // --- Timing constants (tunable) ---
        private const float LiftDuration = 0.12f;
        private const float MoveDuration = 0.18f;
        private const float BasePourDuration = 0.25f;
        private const float PourDurationPerSlot = 0.05f; // extra time per slot poured
        private const float ReturnDuration = 0.25f;
        private const float InitialTiltDuration = 0.08f;

        // --- Movement constants ---
        private const float HoverGap = 0.15f; // small gap between source bottom and target top

        // --- Progressive tilt curve (fill ratio → tilt angle) ---
        // fillRatio 1.0 (full)  → 15°
        // fillRatio 0.75        → 30°
        // fillRatio 0.5         → 55°
        // fillRatio 0.25        → 80°
        // fillRatio 0.0 (empty) → 105°
        private const float TiltAtFull = 50f;
        private const float TiltAtEmpty = 105f;

        // --- Stream position constants ---
        // Bottle mouth is at ~42% of sprite height from pivot (top of visual fill zone)
        private const float MouthOffsetRatio = 0.42f;
        // MaxVisualFill from shader — target surface = fillRatio * this * spriteHeight
        private const float MaxVisualFill = 0.80f;
        // Small offset above target surface so stream lands ON the liquid
        private const float SurfaceLandingOffset = 0.05f;
        // Bottom of liquid area relative to bottle pivot (sprite center)
        private const float BottleBottomRatio = -0.42f;
        // Converts bottle tilt degrees to shader _LiquidTilt value.
        // Single variable: liquid always syncs with bottle rotation.
        private const float LiquidTiltPerDegree = 0.75f;

        // Band calculation
        private const int MaxBands = 6;

        /// <summary>
        /// Runs the full pour animation with movement, progressive tilt, and smooth fills.
        /// bottleWorldHeight: the scaled height of a bottle in world units (for clearance).
        /// sourceDataSnapshot/targetDataSnapshot: pre-pour data clones (before ExecutePour).
        /// </summary>
        public static IEnumerator Animate(
            BottleContainerView source,
            BottleContainerView target,
            int pourCount,
            DrinkColor pourColor,
            ContainerData sourceDataSnapshot,
            ContainerData targetDataSnapshot,
            float bottleWorldHeight,
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
            Vector3 targetLocalPos = targetTf.localPosition;

            // Tilt direction: toward target
            float dx = targetLocalPos.x - originalPos.x;
            float direction = dx < 0f ? 1f : -1f;

            // Pre-pour fill ratio for tilt start angle
            int slotCount = sourceDataSnapshot.SlotCount;
            int prePourFilled = sourceDataSnapshot.FilledCount();
            int postPourFilled = prePourFilled - pourCount;
            float prePourRatio = (float)prePourFilled / Mathf.Max(1, slotCount);
            float postPourRatio = (float)Mathf.Max(0, postPourFilled) / Mathf.Max(1, slotCount);

            // Pour duration scales with pour count for natural feel
            float pourDuration = BasePourDuration + pourCount * PourDurationPerSlot;

            // Compute bottle dimensions early (needed for hover offset and stream)
            float bottleScale = sourceTf.localScale.x;
            float spriteHeight = bottleWorldHeight / Mathf.Max(0.01f, bottleScale);

            // Compute initial tilt angle (needed for hover X offset)
            float startTiltAngle = FillRatioToTiltAngle(prePourRatio);

            // --- Pre-compute band states ---
            var srcBandsBefore = ComputeBands(sourceDataSnapshot);
            var tgtBandsBefore = ComputeBands(targetDataSnapshot);
            var srcBandsAfter = ComputeBandsAfterSourcePour(sourceDataSnapshot, pourCount);
            var tgtBandsAfter = ComputeBandsAfterTargetPour(targetDataSnapshot, pourCount, pourColor);

            // --- Phase 1: Lift source so its bottom clears the target's top ---
            float halfH = bottleWorldHeight * 0.5f;
            float targetTop = Mathf.Max(originalPos.y, targetLocalPos.y) + halfH;
            float liftY = targetTop + halfH + HoverGap;
            Vector3 liftedPos = new Vector3(originalPos.x, liftY, originalPos.z);
            yield return LerpPosition(sourceTf, originalPos, liftedPos, LiftDuration);

            // --- Phase 2: Move so MOUTH (not center) aligns with target X after tilt ---
            // When tilted by startTiltAngle, the mouth swings from center by:
            //   mouthSwingX = sin(tiltZ_radians) * mouthLocalY * bottleScale
            // We offset the bottle center so mouth lands at target.x
            float tiltZRad = startTiltAngle * direction * Mathf.Deg2Rad;
            float mouthLocalY = spriteHeight * MouthOffsetRatio;
            float mouthSwingX = Mathf.Sin(tiltZRad) * mouthLocalY * bottleScale;
            // Offset center so that center + mouth rotation offset = target.x
            // mouth_world_x = center.x - sin(tiltZ) * mouthY * scale
            // center.x = target.x + sin(tiltZ) * mouthY * scale
            float hoverX = targetLocalPos.x + mouthSwingX * 1.1f; // multiplier to exaggerate the offset for better visual alignment
            Vector3 hoverPos = new Vector3(hoverX, liftY, originalPos.z);
            yield return LerpPosition(sourceTf, liftedPos, hoverPos, MoveDuration, EaseInOutCubic);

            // --- Phase 3: Progressive tilt + smooth fill lerps + stream ---
            Quaternion startTiltRot = Quaternion.Euler(0f, 0f, startTiltAngle * direction);

            // Quick initial tilt to start angle (small rotation before pour begins)
            yield return LerpRotation(sourceTf, originalRot, startTiltRot, InitialTiltDuration);

            // Set initial liquid tilt to match bottle rotation
            if (srcController != null)
            {
                srcController.SetLiquidTilt(startTiltAngle * -direction * LiquidTiltPerDegree);
            }

            // Pre-pour target fill ratio for surface tracking
            int tgtSlotCount = targetDataSnapshot.SlotCount;
            int tgtPrePourFilled = targetDataSnapshot.FilledCount();
            int tgtPostPourFilled = tgtPrePourFilled + pourCount;
            float tgtPreFillRatio = (float)tgtPrePourFilled / Mathf.Max(1, tgtSlotCount);
            float tgtPostFillRatio = (float)Mathf.Min(tgtPostPourFilled, tgtSlotCount) / Mathf.Max(1, tgtSlotCount);

            // Pour ratio for stream width
            float pourRatio = (float)pourCount / Mathf.Max(1, slotCount);

            // Calculate initial stream positions
            Vector3 streamSourcePos = GetBottleMouthWorldPos(sourceTf, spriteHeight);
            Vector3 streamTargetPos = GetTargetSurfaceWorldPos(targetTf, spriteHeight, tgtPreFillRatio);

            // Start pour stream VFX
            if (pourStream != null)
            {
                Color streamColor = UI.ThemeConfig.GetDrinkColor(pourColor);
                pourStream.StartStream(streamSourcePos, streamTargetPos, streamColor, pourRatio);
            }

            if (srcController != null && tgtController != null)
            {
                // Setup band colors from snapshots
                SetupSourceColors(srcController, sourceDataSnapshot);
                SetupTargetColors(tgtController, targetDataSnapshot, pourCount, pourColor);

                // Set layer counts
                int srcLayerCount = Mathf.Max(CountActiveBands(srcBandsBefore), CountActiveBands(srcBandsAfter));
                int tgtLayerCount = Mathf.Max(CountActiveBands(tgtBandsBefore), CountActiveBands(tgtBandsAfter));
                srcController.SetLayerCount(srcLayerCount);
                tgtController.SetLayerCount(tgtLayerCount);

                float elapsed = 0f;
                while (elapsed < pourDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = EaseOutCubic(Mathf.Clamp01(elapsed / pourDuration));

                    // Lerp source bands
                    for (int b = 0; b < MaxBands; b++)
                    {
                        float srcFill = Mathf.Lerp(srcBandsBefore.fills[b], srcBandsAfter.fills[b], t);
                        srcController.SetFillAmount(b, srcFill);
                    }

                    // Lerp target bands
                    for (int b = 0; b < MaxBands; b++)
                    {
                        float tgtFill = Mathf.Lerp(tgtBandsBefore.fills[b], tgtBandsAfter.fills[b], t);
                        tgtController.SetFillAmount(b, tgtFill);
                    }

                    // Progressive tilt: angle increases as source empties
                    float currentFillRatio = Mathf.Lerp(prePourRatio, postPourRatio, t);
                    float currentTiltAngle = FillRatioToTiltAngle(currentFillRatio);
                    sourceTf.localRotation = Quaternion.Euler(0f, 0f, currentTiltAngle * direction);

                    // Liquid tilt: driven by same angle as bottle rotation
                    srcController.SetLiquidTilt(currentTiltAngle * -direction * LiquidTiltPerDegree);

                    // Update stream positions: mouth tracks tilt, target tracks fill level
                    if (pourStream != null)
                    {
                        streamSourcePos = GetBottleMouthWorldPos(sourceTf, spriteHeight);
                        float currentTgtFillRatio = Mathf.Lerp(tgtPreFillRatio, tgtPostFillRatio, t);
                        streamTargetPos = GetTargetSurfaceWorldPos(targetTf, spriteHeight, currentTgtFillRatio);
                        pourStream.UpdatePositions(streamSourcePos, streamTargetPos);
                    }

                    yield return null;
                }
            }

            // Stop pour stream
            if (pourStream != null)
                pourStream.StopStream();

            // Wobble target after receiving liquid
            if (tgtController != null)
                tgtController.TriggerWobble(0.03f);

            // Mid-pour callback (sound, etc.)
            onMidPour?.Invoke();

            // Small pause at peak
            yield return new WaitForSeconds(0.04f);

            // --- Phase 4: Return — un-tilt → move back → descend ---

            // Capture final tilt for un-tilt lerp
            Quaternion finalTiltRot = sourceTf.localRotation;

            // Un-tilt back to upright and reset liquid tilt
            yield return LerpRotation(sourceTf, finalTiltRot, originalRot, ReturnDuration * 0.3f);
            if (srcController != null)
                srcController.SetLiquidTilt(0f);

            // Move horizontally back to original X, staying at lift height
            Vector3 returnHoverPos = new Vector3(originalPos.x, liftY, originalPos.z);
            yield return LerpPosition(sourceTf, hoverPos, returnHoverPos, ReturnDuration * 0.35f, EaseInOutCubic);

            // Descend to original position
            yield return LerpPosition(sourceTf, returnHoverPos, originalPos, ReturnDuration * 0.35f);

            // Final refresh to snap to data-driven state
            source.Refresh();
            target.Refresh();

            onComplete?.Invoke();
        }

        // --- Progressive tilt mapping ---

        /// <summary>
        /// Maps source fill ratio to tilt angle using smooth interpolation.
        /// Full bottle (1.0) → 15°, Empty bottle (0.0) → 105°.
        /// Uses quadratic curve for natural acceleration at lower fill levels.
        /// </summary>
        private static float FillRatioToTiltAngle(float fillRatio)
        {
            // Invert: empty = 0 fill ratio → max tilt
            float emptyRatio = 1f - Mathf.Clamp01(fillRatio);
            // Quadratic curve: accelerates tilt as bottle empties
            float curved = emptyRatio * emptyRatio;
            // For mostly-full bottles, use a gentler curve
            float blended = Mathf.Lerp(emptyRatio, curved, 0.6f);
            return Mathf.Lerp(TiltAtFull, TiltAtEmpty, blended);
        }

        // --- Stream position helpers ---

        /// <summary>
        /// Gets the bottle mouth position in world space, accounting for current tilt.
        /// The mouth is at the top of the bottle sprite (MouthOffsetRatio from center).
        /// TransformPoint rotates the local offset with the bottle's current rotation.
        /// </summary>
        private static Vector3 GetBottleMouthWorldPos(Transform bottleTf, float spriteHeight)
        {
            // Local offset: top of bottle relative to pivot (center)
            // spriteHeight is at scale 1.0; TransformPoint applies the transform's scale automatically
            Vector3 localMouth = new Vector3(0f, spriteHeight * MouthOffsetRatio, 0f);
            return bottleTf.TransformPoint(localMouth);
        }

        /// <summary>
        /// Gets the target bottle's current liquid surface position in world space.
        /// Surface height = bottom of liquid area + fillRatio * maxVisualFill * spriteHeight.
        /// Target bottle is not tilted, so we can use a simple Y offset.
        /// </summary>
        private static Vector3 GetTargetSurfaceWorldPos(Transform targetTf, float spriteHeight, float fillRatio)
        {
            // Liquid area starts at BottleBottomRatio from center, fills up by fillRatio * MaxVisualFill * spriteHeight
            float surfaceLocalY = spriteHeight * BottleBottomRatio + fillRatio * MaxVisualFill * spriteHeight + SurfaceLandingOffset;
            Vector3 localSurface = new Vector3(0f, surfaceLocalY, 0f);
            return targetTf.TransformPoint(localSurface);
        }

        // --- Band computation helpers (unchanged from 10.8) ---

        private struct BandState
        {
            public float[] fills;
            public BandState(int size) { fills = new float[size]; }
        }

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

        private static BandState ComputeBandsAfterSourcePour(ContainerData data, int pourCount)
        {
            var state = new BandState(MaxBands);
            int slotCount = data.SlotCount;

            int filledCount = data.FilledCount();
            int remainingFilled = filledCount - pourCount;
            if (remainingFilled <= 0) return state;

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

                int effectiveRun = Mathf.Min(runLength, remainingFilled - slotsProcessed);
                state.fills[bandIndex] = (float)effectiveRun / slotCount;
                bandIndex++;
                slotsProcessed += effectiveRun;
                i += runLength;
            }

            return state;
        }

        private static BandState ComputeBandsAfterTargetPour(ContainerData data, int pourCount, DrinkColor pourColor)
        {
            var state = new BandState(MaxBands);
            int slotCount = data.SlotCount;
            int bandIndex = 0;

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

            if (pourCount > 0 && bandIndex < MaxBands)
            {
                float pourFill = (float)pourCount / slotCount;

                if (bandIndex > 0 && lastColor == pourColor)
                {
                    state.fills[bandIndex - 1] += pourFill;
                }
                else
                {
                    state.fills[bandIndex] = pourFill;
                }
            }

            ClampFillSum(ref state);
            return state;
        }

        private static void SetupTargetColors(LiquidMaterialController controller, ContainerData data, int pourCount, DrinkColor pourColor)
        {
            int slotCount = data.SlotCount;
            int bandIndex = 0;
            DrinkColor lastColor = DrinkColor.None;

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

            if (pourCount > 0 && lastColor != pourColor && bandIndex < MaxBands)
            {
                controller.SetBandColor(bandIndex, UI.ThemeConfig.GetDrinkColor(pourColor));
            }
        }

        private static void SetupSourceColors(LiquidMaterialController controller, ContainerData data)
        {
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

                controller.SetBandColor(bandIndex, UI.ThemeConfig.GetDrinkColor(color));
                bandIndex++;
                i += runLength;
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

        private static void ClampFillSum(ref BandState state)
        {
            float sum = 0f;
            for (int i = 0; i < state.fills.Length; i++)
                sum += state.fills[i];

            if (sum > 1f)
            {
                float scale = 1f / sum;
                for (int i = 0; i < state.fills.Length; i++)
                    state.fills[i] *= scale;
            }
        }

        // --- Lerp helpers ---

        private static IEnumerator LerpPosition(Transform tf, Vector3 from, Vector3 to, float duration)
        {
            return LerpPosition(tf, from, to, duration, EaseOutCubic);
        }

        private static IEnumerator LerpPosition(Transform tf, Vector3 from, Vector3 to, float duration, Func<float, float> easing)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easing(Mathf.Clamp01(elapsed / duration));
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

        private static float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}
