using NUnit.Framework;
using JuiceSort.Game.Puzzle;

namespace JuiceSort.Tests.EditMode
{
    [TestFixture]
    public class UndoStackTests
    {
        [Test]
        public void Push_Pop_ReturnsLastPushed()
        {
            var stack = new UndoStack(3);
            var state = CreateSimpleState(DrinkColor.MangoAmber);

            stack.Push(state);
            var popped = stack.Pop();

            Assert.IsNotNull(popped);
            Assert.AreEqual(DrinkColor.MangoAmber, popped.GetContainer(0).GetTopColor());
        }

        [Test]
        public void Pop_EmptyStack_ReturnsNull()
        {
            var stack = new UndoStack(3);
            Assert.IsNull(stack.Pop());
        }

        [Test]
        public void Count_TracksCorrectly()
        {
            var stack = new UndoStack(3);

            Assert.AreEqual(0, stack.Count);

            stack.Push(CreateSimpleState(DrinkColor.MangoAmber));
            Assert.AreEqual(1, stack.Count);

            stack.Push(CreateSimpleState(DrinkColor.DeepBerry));
            Assert.AreEqual(2, stack.Count);

            stack.Pop();
            Assert.AreEqual(1, stack.Count);
        }

        [Test]
        public void Push_AtCapacity_DropsOldest()
        {
            var stack = new UndoStack(3);

            stack.Push(CreateSimpleState(DrinkColor.MangoAmber));    // oldest
            stack.Push(CreateSimpleState(DrinkColor.DeepBerry));
            stack.Push(CreateSimpleState(DrinkColor.TropicalTeal));
            Assert.AreEqual(3, stack.Count);

            // Push 4th — oldest (MangoAmber) is dropped
            stack.Push(CreateSimpleState(DrinkColor.WatermelonRose));
            Assert.AreEqual(3, stack.Count);

            // Pop should return in reverse order: Rose, Teal, Berry
            Assert.AreEqual(DrinkColor.WatermelonRose, stack.Pop().GetContainer(0).GetTopColor());
            Assert.AreEqual(DrinkColor.TropicalTeal, stack.Pop().GetContainer(0).GetTopColor());
            Assert.AreEqual(DrinkColor.DeepBerry, stack.Pop().GetContainer(0).GetTopColor());
            Assert.IsNull(stack.Pop(), "MangoAmber was dropped — stack should be empty");
        }

        [Test]
        public void Clear_RemovesAll()
        {
            var stack = new UndoStack(3);
            stack.Push(CreateSimpleState(DrinkColor.MangoAmber));
            stack.Push(CreateSimpleState(DrinkColor.DeepBerry));

            stack.Clear();

            Assert.AreEqual(0, stack.Count);
            Assert.IsNull(stack.Pop());
        }

        [Test]
        public void SequentialPushPop_LIFO()
        {
            var stack = new UndoStack(3);

            stack.Push(CreateSimpleState(DrinkColor.MangoAmber));
            stack.Push(CreateSimpleState(DrinkColor.DeepBerry));
            stack.Push(CreateSimpleState(DrinkColor.TropicalTeal));

            Assert.AreEqual(DrinkColor.TropicalTeal, stack.Pop().GetContainer(0).GetTopColor());
            Assert.AreEqual(DrinkColor.DeepBerry, stack.Pop().GetContainer(0).GetTopColor());
            Assert.AreEqual(DrinkColor.MangoAmber, stack.Pop().GetContainer(0).GetTopColor());
        }

        // --- Undo integration tests ---

        [Test]
        public void Undo_RestoresPreviousState()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry }),
                new ContainerData(2)
            };
            var state = new PuzzleState(containers);
            var stack = new UndoStack(3);

            // Save snapshot, then pour
            stack.Push(state.Clone());
            PuzzleEngine.ExecutePour(state, 0, 1);

            Assert.AreEqual(1, state.GetContainer(0).FilledCount());
            Assert.AreEqual(1, state.GetContainer(1).FilledCount());

            // Undo
            var restored = stack.Pop();
            Assert.AreEqual(2, restored.GetContainer(0).FilledCount());
            Assert.AreEqual(0, restored.GetContainer(1).FilledCount());
        }

        [Test]
        public void Undo_MoveCounterDecrements()
        {
            var stack = new UndoStack(3);
            int moveCount = 0;

            var state = CreateSimpleState(DrinkColor.MangoAmber);
            stack.Push(state.Clone());
            moveCount++;

            // Undo
            stack.Pop();
            moveCount--;

            Assert.AreEqual(0, moveCount);
        }

        [Test]
        public void Undo_EmptyStack_NoChange()
        {
            var stack = new UndoStack(3);
            int moveCount = 5;

            var snapshot = stack.Pop();
            if (snapshot != null)
                moveCount--;

            Assert.AreEqual(5, moveCount, "Move count should not change on empty undo");
        }

        [Test]
        public void Undo_ThreePoursThenThreeUndos_RestoresOriginal()
        {
            var containers = new[]
            {
                new ContainerData(new[] { DrinkColor.MangoAmber, DrinkColor.DeepBerry, DrinkColor.TropicalTeal }),
                new ContainerData(3)
            };
            var originalState = new PuzzleState(containers);
            var state = originalState.Clone();
            var stack = new UndoStack(3);

            // 3 pours
            for (int i = 0; i < 3; i++)
            {
                stack.Push(state.Clone());
                PuzzleEngine.ExecutePour(state, 0, 1);
            }

            Assert.IsTrue(state.GetContainer(0).IsEmpty());
            Assert.AreEqual(3, state.GetContainer(1).FilledCount());

            // 3 undos
            for (int i = 0; i < 3; i++)
            {
                state = stack.Pop();
            }

            Assert.AreEqual(3, state.GetContainer(0).FilledCount());
            Assert.AreEqual(0, state.GetContainer(1).FilledCount());
        }

        [Test]
        public void Undo_Overflow_OldestLost()
        {
            var stack = new UndoStack(3);

            // Push 4 states — first one gets dropped
            stack.Push(CreateSimpleState(DrinkColor.MangoAmber));
            stack.Push(CreateSimpleState(DrinkColor.DeepBerry));
            stack.Push(CreateSimpleState(DrinkColor.TropicalTeal));
            stack.Push(CreateSimpleState(DrinkColor.WatermelonRose));

            // Can only undo 3 times
            Assert.IsNotNull(stack.Pop());
            Assert.IsNotNull(stack.Pop());
            Assert.IsNotNull(stack.Pop());
            Assert.IsNull(stack.Pop(), "4th undo should fail — oldest was dropped");
        }

        private PuzzleState CreateSimpleState(DrinkColor color)
        {
            return new PuzzleState(new[]
            {
                new ContainerData(new[] { color })
            });
        }
    }
}
