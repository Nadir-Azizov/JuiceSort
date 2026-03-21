using System;
using System.Collections;
using UnityEngine;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Coroutine-based pour animation. Lifts source bottle, tilts toward target,
    /// animates liquid transfer slot-by-slot, then returns source to rest.
    /// </summary>
    public static class PourAnimator
    {
        // Timing
        private const float LiftDuration = 0.15f;
        private const float TiltDuration = 0.12f;
        private const float SlotTransferDelay = 0.08f;
        private const float ReturnDuration = 0.2f;
        private const float LiftHeight = 0.4f;
        private const float TiltAngle = 25f;

        /// <summary>
        /// Runs the full pour animation sequence.
        /// Call from a MonoBehaviour via StartCoroutine.
        /// onMidPour is invoked after liquid visuals transfer (apply game logic here).
        /// onComplete is invoked when the animation fully finishes.
        /// </summary>
        /// <param name="sourceTopIndex">Top filled slot index of source BEFORE ExecutePour was called.</param>
        /// <param name="targetFirstEmpty">First empty slot index of target BEFORE ExecutePour was called.</param>
        public static IEnumerator Animate(
            BottleContainerView source,
            BottleContainerView target,
            int pourCount,
            DrinkColor pourColor,
            int sourceTopIndex,
            int targetFirstEmpty,
            Action onMidPour,
            Action onComplete)
        {
            var sourceTf = source.transform;
            var targetTf = target.transform;

            Vector3 originalPos = sourceTf.localPosition;
            Quaternion originalRot = sourceTf.localRotation;

            // Determine tilt direction: tilt toward target
            float dx = targetTf.position.x - sourceTf.position.x;
            float direction = dx < 0f ? 1f : -1f;
            float tiltZ = TiltAngle * direction;

            // --- Phase 1: Lift source bottle up ---
            Vector3 liftedPos = originalPos + new Vector3(0f, LiftHeight, 0f);
            yield return LerpPosition(sourceTf, originalPos, liftedPos, LiftDuration);

            // --- Phase 2: Tilt source toward target ---
            Quaternion tiltedRot = Quaternion.Euler(0f, 0f, tiltZ);
            yield return LerpRotation(sourceTf, originalRot, tiltedRot, TiltDuration);

            // --- Phase 3: Transfer liquid slot by slot ---
            for (int i = 0; i < pourCount; i++)
            {
                int srcSlot = sourceTopIndex - i;
                int tgtSlot = targetFirstEmpty + i;

                if (srcSlot >= 0)
                    source.SetSlotVisible(srcSlot, false);

                if (tgtSlot < target.Data.SlotCount)
                    target.SetSlotColorAndShow(tgtSlot, pourColor);

                yield return new WaitForSeconds(SlotTransferDelay);
            }

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

        /// <summary>
        /// Ease-out cubic: fast start, smooth deceleration.
        /// </summary>
        private static float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }
    }
}
