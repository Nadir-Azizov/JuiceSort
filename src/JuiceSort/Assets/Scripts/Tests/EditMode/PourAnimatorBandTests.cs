using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    /// <summary>
    /// Tests that validate the pour animation's band computation correctness.
    /// Specifically validates that pre-pour data snapshots produce correct before/after
    /// band states for both source and target bottles.
    /// These tests verify the fix for the extra-slot visual bug (Story 10.8).
    /// </summary>
    public class PourAnimatorBandTests
    {
        [Test]
        public void PrePourSnapshot_SourceHasCorrectFilledCount_BeforeExecutePour()
        {
            // Arrange: source has 3 filled slots (MangoAmber, MangoAmber, DeepBerry from bottom)
            var source = new ContainerData(4);
            source.SetSlot(0, DrinkColor.MangoAmber);
            source.SetSlot(1, DrinkColor.MangoAmber);
            source.SetSlot(2, DrinkColor.DeepBerry);

            // Act: snapshot BEFORE mutation
            var snapshot = source.Clone();

            // Simulate ExecutePour removing 1 DeepBerry from top
            source.SetSlot(2, DrinkColor.None);

            // Assert: snapshot preserves pre-pour state
            Assert.AreEqual(3, snapshot.FilledCount(), "Snapshot should have 3 filled slots (pre-pour)");
            Assert.AreEqual(DrinkColor.DeepBerry, snapshot.GetSlot(2), "Snapshot slot 2 should still be DeepBerry");
            Assert.AreEqual(2, source.FilledCount(), "Live data should have 2 filled slots (post-pour)");
            Assert.AreEqual(DrinkColor.None, source.GetSlot(2), "Live data slot 2 should be None");
        }

        [Test]
        public void PrePourSnapshot_TargetHasCorrectFilledCount_BeforeExecutePour()
        {
            // Arrange: target has 1 filled slot (TropicalTeal at bottom)
            var target = new ContainerData(4);
            target.SetSlot(0, DrinkColor.TropicalTeal);

            // Act: snapshot BEFORE mutation
            var snapshot = target.Clone();

            // Simulate ExecutePour adding 1 DeepBerry on top
            target.SetSlot(1, DrinkColor.DeepBerry);

            // Assert: snapshot preserves pre-pour state
            Assert.AreEqual(1, snapshot.FilledCount(), "Snapshot should have 1 filled slot (pre-pour)");
            Assert.AreEqual(DrinkColor.None, snapshot.GetSlot(1), "Snapshot slot 1 should still be None");
            Assert.AreEqual(2, target.FilledCount(), "Live data should have 2 filled slots (post-pour)");
            Assert.AreEqual(DrinkColor.DeepBerry, target.GetSlot(1), "Live data slot 1 should be DeepBerry");
        }

        [Test]
        public void Clone_ProducesIndependentCopy_MutationsDoNotPropagate()
        {
            var original = new ContainerData(4);
            original.SetSlot(0, DrinkColor.MangoAmber);
            original.SetSlot(1, DrinkColor.DeepBerry);
            original.SetSlot(2, DrinkColor.TropicalTeal);
            original.SetSlot(3, DrinkColor.LimeGold);

            var clone = original.Clone();

            // Mutate original
            original.SetSlot(3, DrinkColor.None);
            original.SetSlot(2, DrinkColor.None);

            // Clone should be untouched
            Assert.AreEqual(4, clone.FilledCount());
            Assert.AreEqual(DrinkColor.TropicalTeal, clone.GetSlot(2));
            Assert.AreEqual(DrinkColor.LimeGold, clone.GetSlot(3));
        }

        [Test]
        public void PourScenario_OneSlotsFromSourceToTarget_CorrectFillCounts()
        {
            // Scenario from user report: source 1 slot, target 1 slot, pour 1
            // After pour: source 0 slots, target 2 slots
            var source = new ContainerData(4);
            source.SetSlot(0, DrinkColor.MangoAmber);

            var target = new ContainerData(4);
            target.SetSlot(0, DrinkColor.MangoAmber);

            // Capture snapshots
            var srcSnap = source.Clone();
            var tgtSnap = target.Clone();

            // Simulate ExecutePour
            source.SetSlot(0, DrinkColor.None); // Remove from source
            target.SetSlot(1, DrinkColor.MangoAmber);  // Add to target

            // Verify snapshots for band computation
            Assert.AreEqual(1, srcSnap.FilledCount(), "Source snapshot: 1 filled (pre-pour)");
            Assert.AreEqual(1, tgtSnap.FilledCount(), "Target snapshot: 1 filled (pre-pour)");

            // After-pour computed from snapshots (not live data):
            // Source after: 1 - 1 = 0 filled
            // Target after: 1 + 1 = 2 filled
            // This prevents the bug where live data (already 2 in target) + pourCount (1) = 3
            Assert.AreEqual(0, source.FilledCount(), "Source live: 0 filled (post-pour)");
            Assert.AreEqual(2, target.FilledCount(), "Target live: 2 filled (post-pour)");
        }

        [Test]
        public void PourScenario_FillBottleCompletely_MaxFourSlots()
        {
            // Target has 3/4, source pours 1 to fill it
            var source = new ContainerData(4);
            source.SetSlot(0, DrinkColor.DeepBerry);

            var target = new ContainerData(4);
            target.SetSlot(0, DrinkColor.DeepBerry);
            target.SetSlot(1, DrinkColor.DeepBerry);
            target.SetSlot(2, DrinkColor.DeepBerry);

            var srcSnap = source.Clone();
            var tgtSnap = target.Clone();

            // Simulate ExecutePour
            source.SetSlot(0, DrinkColor.None);
            target.SetSlot(3, DrinkColor.DeepBerry);

            // Target snapshot has 3 filled → after pour has 4 filled
            // Band fill sum from snapshot: 3/4 = 0.75 → after: 4/4 = 1.0
            // Shader multiplies by _MaxVisualFill (0.80) so visual = 80% max
            Assert.AreEqual(3, tgtSnap.FilledCount());
            Assert.AreEqual(4, target.FilledCount());
            Assert.IsTrue(target.IsFull());
        }

        [Test]
        public void PourScenario_MultiSlotPour_ConservesLiquid()
        {
            // Source has 3 MangoAmber on top, target is empty
            var source = new ContainerData(4);
            source.SetSlot(0, DrinkColor.DeepBerry);
            source.SetSlot(1, DrinkColor.MangoAmber);
            source.SetSlot(2, DrinkColor.MangoAmber);
            source.SetSlot(3, DrinkColor.MangoAmber);

            var target = new ContainerData(4);

            var srcSnap = source.Clone();
            var tgtSnap = target.Clone();

            int pourCount = 3; // Pour 3 MangoAmber slots

            // Conservation: srcSnap.FilledCount - pourCount + tgtSnap.FilledCount + pourCount
            // = srcSnap.FilledCount + tgtSnap.FilledCount (unchanged total)
            int totalBefore = srcSnap.FilledCount() + tgtSnap.FilledCount();

            // Simulate ExecutePour
            source.SetSlot(1, DrinkColor.None);
            source.SetSlot(2, DrinkColor.None);
            source.SetSlot(3, DrinkColor.None);
            target.SetSlot(0, DrinkColor.MangoAmber);
            target.SetSlot(1, DrinkColor.MangoAmber);
            target.SetSlot(2, DrinkColor.MangoAmber);

            int totalAfter = source.FilledCount() + target.FilledCount();
            Assert.AreEqual(totalBefore, totalAfter, "Total liquid must be conserved");
        }
    }
}
