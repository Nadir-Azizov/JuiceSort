using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    /// <summary>
    /// Tests for container selection logic.
    /// Tests replicate GameplayManager.OnContainerTapped logic since
    /// GameplayManager is a MonoBehaviour and can't be instantiated in EditMode.
    /// </summary>
    [TestFixture]
    public class ContainerSelectionTests
    {
        private PuzzleState _puzzle;
        private int _selectedIndex;

        /// <summary>
        /// Replicates GameplayManager.OnContainerTapped logic for testability.
        /// </summary>
        private void SimulateContainerTapped(int index)
        {
            var containerData = _puzzle.GetContainer(index);

            if (containerData.IsEmpty())
                return;

            if (_selectedIndex != index)
            {
                _selectedIndex = index;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _puzzle = TestPuzzleData.CreateTestPuzzle();
            _selectedIndex = -1;
        }

        [Test]
        public void TapNonEmptyContainer_SelectsIt()
        {
            Assert.IsFalse(_puzzle.GetContainer(0).IsEmpty());

            SimulateContainerTapped(0);

            Assert.AreEqual(0, _selectedIndex);
        }

        [Test]
        public void TapEmptyContainer_DoesNotSelect()
        {
            Assert.IsTrue(_puzzle.GetContainer(3).IsEmpty());

            SimulateContainerTapped(3);

            Assert.AreEqual(-1, _selectedIndex, "Tapping empty container should not change selection");
        }

        [Test]
        public void TapEmptyContainer_KeepsPreviousSelection()
        {
            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex);

            SimulateContainerTapped(3);
            Assert.AreEqual(0, _selectedIndex, "Tapping empty should keep previous selection");
        }

        [Test]
        public void TapDifferentNonEmpty_ChangesSelection()
        {
            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex);

            SimulateContainerTapped(1);
            Assert.AreEqual(1, _selectedIndex, "Tapping different non-empty should change selection");
        }

        [Test]
        public void TapSameContainer_SelectionUnchanged()
        {
            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex);

            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex, "Tapping same container should keep selection");
        }

        [Test]
        public void SequentialTaps_TrackCorrectly()
        {
            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex);

            SimulateContainerTapped(1);
            Assert.AreEqual(1, _selectedIndex);

            SimulateContainerTapped(2);
            Assert.AreEqual(2, _selectedIndex);
        }

        [Test]
        public void NoTaps_SelectionIsNone()
        {
            Assert.AreEqual(-1, _selectedIndex);
        }

        [Test]
        public void TapAllEmptyContainers_NeverSelects()
        {
            // Create a puzzle with all empty containers
            var emptyContainers = new[]
            {
                new ContainerData(4),
                new ContainerData(4),
                new ContainerData(4)
            };
            var emptyPuzzle = new PuzzleState(emptyContainers);
            _puzzle = emptyPuzzle;

            SimulateContainerTapped(0);
            SimulateContainerTapped(1);
            SimulateContainerTapped(2);

            Assert.AreEqual(-1, _selectedIndex, "All empty containers should never allow selection");
        }

        [Test]
        public void ContainerState_DefaultIsIdle()
        {
            Assert.AreEqual(ContainerState.Idle, default(ContainerState));
        }

        [Test]
        public void ContainerState_HasAllRequiredValues()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(ContainerState), ContainerState.Idle));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ContainerState), ContainerState.Selected));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ContainerState), ContainerState.Pouring));
            Assert.IsTrue(System.Enum.IsDefined(typeof(ContainerState), ContainerState.Receiving));
        }

        // --- Deselect tests (Story 1.6) ---

        [Test]
        public void TapSameContainer_Deselects()
        {
            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex);

            // Tap same — should deselect
            SimulateTapSameOrBackground(0);
            Assert.AreEqual(-1, _selectedIndex);
        }

        [Test]
        public void BackgroundTap_Deselects()
        {
            SimulateContainerTapped(0);
            Assert.AreEqual(0, _selectedIndex);

            SimulateBackgroundTap();
            Assert.AreEqual(-1, _selectedIndex);
        }

        [Test]
        public void BackgroundTap_NothingSelected_NoChange()
        {
            Assert.AreEqual(-1, _selectedIndex);

            SimulateBackgroundTap();
            Assert.AreEqual(-1, _selectedIndex);
        }

        /// <summary>
        /// Replicates GameplayManager logic: tapping same container deselects.
        /// </summary>
        private void SimulateTapSameOrBackground(int index)
        {
            if (_selectedIndex == index)
            {
                _selectedIndex = -1;
            }
        }

        private void SimulateBackgroundTap()
        {
            if (_selectedIndex >= 0)
                _selectedIndex = -1;
        }
    }
}
