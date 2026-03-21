using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class ContainerDataTests
    {
        [Test]
        public void Constructor_WithSlotCount_CreatesEmptyContainer()
        {
            var container = new ContainerData(4);

            Assert.AreEqual(4, container.SlotCount);
            Assert.IsTrue(container.IsEmpty());
            Assert.IsFalse(container.IsFull());
        }

        [Test]
        public void Constructor_WithSlots_CopiesDataCorrectly()
        {
            var slots = new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry, DrinkColor.None, DrinkColor.None };
            var container = new ContainerData(slots);

            Assert.AreEqual(DrinkColor.MangoAmber, container.GetSlot(0));
            Assert.AreEqual(DrinkColor.DeepBerry, container.GetSlot(1));
            Assert.AreEqual(DrinkColor.None, container.GetSlot(2));
        }

        [Test]
        public void GetTopColor_EmptyContainer_ReturnsNone()
        {
            var container = new ContainerData(4);
            Assert.AreEqual(DrinkColor.None, container.GetTopColor());
        }

        [Test]
        public void GetTopColor_PartiallyFilled_ReturnsTopNonEmptyColor()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.AreEqual(DrinkColor.DeepBerry, container.GetTopColor());
        }

        [Test]
        public void GetTopColor_FullContainer_ReturnsTopColor()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.TropicalTeal,
                DrinkColor.WatermelonRose
            });

            Assert.AreEqual(DrinkColor.WatermelonRose, container.GetTopColor());
        }

        [Test]
        public void GetTopIndex_EmptyContainer_ReturnsNegativeOne()
        {
            var container = new ContainerData(4);
            Assert.AreEqual(-1, container.GetTopIndex());
        }

        [Test]
        public void GetTopIndex_PartiallyFilled_ReturnsCorrectIndex()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.AreEqual(1, container.GetTopIndex());
        }

        [Test]
        public void GetTopColorCount_EmptyContainer_ReturnsZero()
        {
            var container = new ContainerData(4);
            Assert.AreEqual(0, container.GetTopColorCount());
        }

        [Test]
        public void GetTopColorCount_SingleTopColor_ReturnsOne()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.AreEqual(1, container.GetTopColorCount());
        }

        [Test]
        public void GetTopColorCount_MultipleConsecutiveTopColors_ReturnsCount()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.DeepBerry,
                DrinkColor.None
            });

            Assert.AreEqual(2, container.GetTopColorCount());
        }

        [Test]
        public void IsEmpty_AllNone_ReturnsTrue()
        {
            var container = new ContainerData(4);
            Assert.IsTrue(container.IsEmpty());
        }

        [Test]
        public void IsEmpty_HasContents_ReturnsFalse()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.None,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.IsFalse(container.IsEmpty());
        }

        [Test]
        public void IsFull_AllFilled_ReturnsTrue()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.TropicalTeal,
                DrinkColor.WatermelonRose
            });

            Assert.IsTrue(container.IsFull());
        }

        [Test]
        public void IsFull_HasEmptySlot_ReturnsFalse()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.IsFalse(container.IsFull());
        }

        [Test]
        public void IsSorted_EmptyContainer_ReturnsTrue()
        {
            var container = new ContainerData(4);
            Assert.IsTrue(container.IsSorted());
        }

        [Test]
        public void IsSorted_AllSameColor_ReturnsTrue()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.MangoAmber,
                DrinkColor.MangoAmber,
                DrinkColor.MangoAmber
            });

            Assert.IsTrue(container.IsSorted());
        }

        [Test]
        public void IsSorted_PartiallySameColor_ReturnsTrue()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.MangoAmber,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.IsTrue(container.IsSorted());
        }

        [Test]
        public void IsSorted_MixedColors_ReturnsFalse()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.MangoAmber,
                DrinkColor.None
            });

            Assert.IsFalse(container.IsSorted());
        }

        [Test]
        public void FilledCount_EmptyContainer_ReturnsZero()
        {
            var container = new ContainerData(4);
            Assert.AreEqual(0, container.FilledCount());
        }

        [Test]
        public void FilledCount_PartiallyFilled_ReturnsCorrectCount()
        {
            var container = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            Assert.AreEqual(2, container.FilledCount());
        }

        [Test]
        public void Clone_CreatesIndependentCopy()
        {
            var original = new ContainerData(new[]
            {
                DrinkColor.MangoAmber,
                DrinkColor.DeepBerry,
                DrinkColor.None,
                DrinkColor.None
            });

            var clone = original.Clone();

            // Verify clone has same data
            Assert.AreEqual(original.GetSlot(0), clone.GetSlot(0));
            Assert.AreEqual(original.GetSlot(1), clone.GetSlot(1));
            Assert.AreEqual(original.SlotCount, clone.SlotCount);

            // Modify clone and verify original is unchanged
            clone.SetSlot(0, DrinkColor.TropicalTeal);
            Assert.AreEqual(DrinkColor.MangoAmber, original.GetSlot(0));
            Assert.AreEqual(DrinkColor.TropicalTeal, clone.GetSlot(0));
        }

        [Test]
        public void SetSlot_UpdatesValue()
        {
            var container = new ContainerData(4);
            container.SetSlot(0, DrinkColor.MangoAmber);

            Assert.AreEqual(DrinkColor.MangoAmber, container.GetSlot(0));
            Assert.IsFalse(container.IsEmpty());
        }
    }
}
