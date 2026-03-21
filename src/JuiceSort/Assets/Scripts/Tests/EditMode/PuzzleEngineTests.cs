using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class PuzzleEngineTests
    {
        // --- ContainerData.RemoveTop tests ---

        [Test]
        public void RemoveTop_FullContainer_RemovesTopColor()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.TropicalTeal,
                DrinkColor.WatermelonRose
            });

            var removed = container.RemoveTop();

            Assert.AreEqual(DrinkColor.WatermelonRose, removed);
            Assert.AreEqual(DrinkColor.None, container.GetSlot(3));
            Assert.AreEqual(DrinkColor.TropicalTeal, container.GetTopColor());
        }

        [Test]
        public void RemoveTop_PartiallyFilled_RemovesCorrectSlot()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            var removed = container.RemoveTop();

            Assert.AreEqual(DrinkColor.DeepBerry, removed);
            Assert.AreEqual(DrinkColor.None, container.GetSlot(1));
            Assert.AreEqual(1, container.FilledCount());
        }

        [Test]
        public void RemoveTop_EmptyContainer_ReturnsNone()
        {
            var container = new ContainerData(4);

            var removed = container.RemoveTop();

            Assert.AreEqual(DrinkColor.None, removed);
            Assert.IsTrue(container.IsEmpty());
        }

        [Test]
        public void RemoveTop_SingleItem_LeavesEmpty()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.None,
                DrinkColor.None,
                DrinkColor.None
            });

            var removed = container.RemoveTop();

            Assert.AreEqual(DrinkColor.MangoAmber, removed);
            Assert.IsTrue(container.IsEmpty());
        }

        // --- ContainerData.AddToTop tests ---

        [Test]
        public void AddToTop_EmptyContainer_PlacesAtBottom()
        {
            var container = new ContainerData(4);

            bool result = container.AddToTop(DrinkColor.MangoAmber);

            Assert.IsTrue(result);
            Assert.AreEqual(DrinkColor.MangoAmber, container.GetSlot(0));
            Assert.AreEqual(1, container.FilledCount());
        }

        [Test]
        public void AddToTop_PartiallyFilled_PlacesInLowestEmpty()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            bool result = container.AddToTop(DrinkColor.TropicalTeal);

            Assert.IsTrue(result);
            Assert.AreEqual(DrinkColor.TropicalTeal, container.GetSlot(2));
            Assert.AreEqual(3, container.FilledCount());
        }

        [Test]
        public void AddToTop_FullContainer_ReturnsFalse()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.TropicalTeal,
                DrinkColor.WatermelonRose
            });

            bool result = container.AddToTop(DrinkColor.LimeGold);

            Assert.IsFalse(result);
            Assert.AreEqual(DrinkColor.WatermelonRose, container.GetSlot(3));
        }

        // --- ContainerData.GetFirstEmptyIndex tests ---

        [Test]
        public void GetFirstEmptyIndex_EmptyContainer_ReturnsZero()
        {
            var container = new ContainerData(4);
            Assert.AreEqual(0, container.GetFirstEmptyIndex());
        }

        [Test]
        public void GetFirstEmptyIndex_PartiallyFilled_ReturnsCorrectIndex()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.AreEqual(2, container.GetFirstEmptyIndex());
        }

        [Test]
        public void GetFirstEmptyIndex_FullContainer_ReturnsNegativeOne()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.TropicalTeal,
                DrinkColor.WatermelonRose
            });

            Assert.AreEqual(-1, container.GetFirstEmptyIndex());
        }

        // --- PuzzleEngine.ExecutePour tests ---

        [Test]
        public void ExecutePour_SingleTopColor_MovesOneUnit()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.DeepBerry,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.TropicalTeal, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            bool result = PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.IsTrue(result);
            Assert.AreEqual(DrinkColor.MangoAmber, source.GetTopColor(), "Source should have MangoAmber after DeepBerry removed");
            Assert.AreEqual(1, source.FilledCount());
            Assert.AreEqual(DrinkColor.DeepBerry, target.GetSlot(1), "Target should receive DeepBerry in slot 1");
            Assert.AreEqual(2, target.FilledCount());
        }

        [Test]
        public void ExecutePour_IntoEmptyContainer_ColorGoesToSlotZero()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, target });

            bool result = PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.IsTrue(result);
            Assert.AreEqual(DrinkColor.MangoAmber, target.GetSlot(0));
            Assert.IsTrue(source.IsEmpty());
        }

        [Test]
        public void ExecutePour_FromEmptySource_ReturnsFalse()
        {
            var source = new ContainerData(4);
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, target });

            bool result = PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.IsFalse(result);
            Assert.IsTrue(source.IsEmpty());
            Assert.IsTrue(target.IsEmpty());
        }

        [Test]
        public void ExecutePour_ToFullTarget_ReturnsFalse()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.DeepBerry, DrinkColor.DeepBerry,
                DrinkColor.DeepBerry, DrinkColor.DeepBerry
            });
            var state = new PuzzleState(new[] { source, target });

            bool result = PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.IsFalse(result);
            Assert.AreEqual(1, source.FilledCount(), "Source should be unchanged");
            Assert.AreEqual(4, target.FilledCount(), "Target should be unchanged");
        }

        [Test]
        public void ExecutePour_MultipleConsecutiveSameColor_PoursAll()
        {
            // Source has 3 consecutive TropicalTeal on top, target is empty
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.TropicalTeal,
                DrinkColor.TropicalTeal, DrinkColor.TropicalTeal
            });
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, target });

            PuzzleEngine.ExecutePour(state, 0, 1);

            // All 3 TropicalTeal should pour in one move
            Assert.AreEqual(1, source.FilledCount(), "Only MangoAmber should remain");
            Assert.AreEqual(DrinkColor.MangoAmber, source.GetTopColor());
            Assert.AreEqual(3, target.FilledCount());
            Assert.AreEqual(DrinkColor.TropicalTeal, target.GetSlot(0));
            Assert.AreEqual(DrinkColor.TropicalTeal, target.GetSlot(1));
            Assert.AreEqual(DrinkColor.TropicalTeal, target.GetSlot(2));
        }

        [Test]
        public void ExecutePour_MultiPour_LimitedByTargetSpace()
        {
            // Source has 3 consecutive Mango, target has 1 empty slot
            var source = new ContainerData(new[]
            {
                DrinkColor.DeepBerry, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.MangoAmber
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            PuzzleEngine.ExecutePour(state, 0, 1);

            // Only 1 should pour (limited by target space)
            Assert.AreEqual(3, source.FilledCount(), "Source should still have 3 filled");
            Assert.AreEqual(DrinkColor.MangoAmber, source.GetTopColor());
            Assert.IsTrue(target.IsFull(), "Target should be full");
        }

        [Test]
        public void ExecutePour_TwoConsecutive_PoursTwoIntoEmptyTarget()
        {
            // Source has 2 consecutive DeepBerry on top
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.TropicalTeal,
                DrinkColor.DeepBerry, DrinkColor.DeepBerry
            });
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, target });

            PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.AreEqual(2, source.FilledCount());
            Assert.AreEqual(DrinkColor.TropicalTeal, source.GetTopColor());
            Assert.AreEqual(2, target.FilledCount());
            Assert.AreEqual(DrinkColor.DeepBerry, target.GetSlot(0));
            Assert.AreEqual(DrinkColor.DeepBerry, target.GetSlot(1));
        }

        // --- Move counter logic test ---

        [Test]
        public void MoveCounter_IncreasesOnlyOnSuccessfulPour()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var emptySource = new ContainerData(4);
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, emptySource, target });

            int moveCount = 0;

            // Successful pour
            if (PuzzleEngine.ExecutePour(state, 0, 2))
                moveCount++;
            Assert.AreEqual(1, moveCount);

            // Failed pour (empty source)
            if (PuzzleEngine.ExecutePour(state, 1, 2))
                moveCount++;
            Assert.AreEqual(1, moveCount, "Failed pour should not increment");
        }

        // --- CanPour color validation tests ---

        [Test]
        public void CanPour_MatchingTopColors_ReturnsTrue()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.DeepBerry,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.TropicalTeal, DrinkColor.DeepBerry,
                DrinkColor.None, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            Assert.IsTrue(PuzzleEngine.CanPour(state, 0, 1));
        }

        [Test]
        public void CanPour_EmptyTarget_ReturnsTrue()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, target });

            Assert.IsTrue(PuzzleEngine.CanPour(state, 0, 1));
        }

        [Test]
        public void CanPour_MismatchedColors_ReturnsFalse()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.DeepBerry,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.TropicalTeal, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            Assert.IsFalse(PuzzleEngine.CanPour(state, 0, 1));
        }

        [Test]
        public void CanPour_EmptySource_ReturnsFalse()
        {
            var source = new ContainerData(4);
            var target = new ContainerData(4);
            var state = new PuzzleState(new[] { source, target });

            Assert.IsFalse(PuzzleEngine.CanPour(state, 0, 1));
        }

        [Test]
        public void CanPour_FullTarget_ReturnsFalse()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.MangoAmber
            });
            var state = new PuzzleState(new[] { source, target });

            Assert.IsFalse(PuzzleEngine.CanPour(state, 0, 1));
        }

        [Test]
        public void ExecutePour_MismatchedColors_ReturnsFalseNoStateChange()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.DeepBerry,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.TropicalTeal, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            bool result = PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.IsFalse(result);
            Assert.AreEqual(2, source.FilledCount(), "Source unchanged");
            Assert.AreEqual(1, target.FilledCount(), "Target unchanged");
            Assert.AreEqual(DrinkColor.DeepBerry, source.GetTopColor());
            Assert.AreEqual(DrinkColor.TropicalTeal, target.GetTopColor());
        }

        [Test]
        public void ExecutePour_MatchingColors_Succeeds()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.TropicalTeal,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.TropicalTeal, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            bool result = PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.IsTrue(result);
            Assert.AreEqual(1, source.FilledCount());
            Assert.AreEqual(2, target.FilledCount());
            Assert.AreEqual(DrinkColor.TropicalTeal, target.GetSlot(1));
        }

        // --- Slot validation edge case tests (Story 1.5) ---

        [Test]
        public void CanPour_NearlyFullTargetMatchingColor_ReturnsTrue()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            Assert.IsTrue(PuzzleEngine.CanPour(state, 0, 1), "1 empty slot + matching color should allow pour");
        }

        [Test]
        public void CanPour_NearlyFullTargetMismatchedColor_ReturnsFalse()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.DeepBerry, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            Assert.IsFalse(PuzzleEngine.CanPour(state, 0, 1), "1 empty slot but mismatched color should reject");
        }

        [Test]
        public void CanPour_FullTargetMatchingColor_ReturnsFalse()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.MangoAmber
            });
            var state = new PuzzleState(new[] { source, target });

            Assert.IsFalse(PuzzleEngine.CanPour(state, 0, 1), "Full target should reject even with matching color");
        }

        [Test]
        public void ExecutePour_FillToCapacity_LastPourSucceedsNextFails()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.None, DrinkColor.None
            });
            var target = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.None
            });
            var state = new PuzzleState(new[] { source, target });

            // Pour to fill target (1 empty slot, matching color)
            bool fill = PuzzleEngine.ExecutePour(state, 0, 1);
            Assert.IsTrue(fill, "Pour into last empty slot should succeed");
            Assert.IsTrue(target.IsFull(), "Target should now be full");

            // Next pour should fail (target full)
            bool overflow = PuzzleEngine.ExecutePour(state, 0, 1);
            Assert.IsFalse(overflow, "Pour into full target should fail");
            Assert.AreEqual(1, source.FilledCount(), "Source should be unchanged after failed pour");
        }

        [Test]
        public void SlotValidation_FailedPour_PreservesSourceSelection()
        {
            var source = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.None,
                DrinkColor.None, DrinkColor.None
            });
            var fullTarget = new ContainerData(new[]
            {
                DrinkColor.MangoAmber, DrinkColor.MangoAmber,
                DrinkColor.MangoAmber, DrinkColor.MangoAmber
            });
            var state = new PuzzleState(new[] { source, fullTarget });

            // Replicate GameplayManager logic: pour fails → source stays selected
            int selectedIndex = 0;
            bool success = PuzzleEngine.ExecutePour(state, 0, 1);
            if (success)
                selectedIndex = -1;

            Assert.IsFalse(success);
            Assert.AreEqual(0, selectedIndex, "Source should remain selected after failed pour");
        }
    }
}
