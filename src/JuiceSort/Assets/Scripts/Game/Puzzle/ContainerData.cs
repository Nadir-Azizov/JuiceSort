using System;

namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Pure C# data model for a single container.
    /// Slots are indexed bottom-up: index 0 = bottom, index N-1 = top.
    /// </summary>
    public class ContainerData
    {
        private readonly DrinkColor[] _slots;

        public int SlotCount => _slots.Length;

        public ContainerData(int slotCount)
        {
            _slots = new DrinkColor[slotCount];
        }

        public ContainerData(DrinkColor[] slots)
        {
            _slots = new DrinkColor[slots.Length];
            Array.Copy(slots, _slots, slots.Length);
        }

        public DrinkColor GetSlot(int index)
        {
            return _slots[index];
        }

        public void SetSlot(int index, DrinkColor color)
        {
            _slots[index] = color;
        }

        /// <summary>
        /// Returns the color of the topmost non-empty slot, or None if empty.
        /// </summary>
        public DrinkColor GetTopColor()
        {
            for (int i = _slots.Length - 1; i >= 0; i--)
            {
                if (_slots[i] != DrinkColor.None)
                    return _slots[i];
            }
            return DrinkColor.None;
        }

        /// <summary>
        /// Returns the index of the topmost non-empty slot, or -1 if empty.
        /// </summary>
        public int GetTopIndex()
        {
            for (int i = _slots.Length - 1; i >= 0; i--)
            {
                if (_slots[i] != DrinkColor.None)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns how many consecutive slots from the top share the same color.
        /// </summary>
        public int GetTopColorCount()
        {
            var topColor = GetTopColor();
            if (topColor == DrinkColor.None)
                return 0;

            int count = 0;
            for (int i = _slots.Length - 1; i >= 0; i--)
            {
                if (_slots[i] == topColor)
                    count++;
                else if (_slots[i] != DrinkColor.None)
                    break;
            }
            return count;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != DrinkColor.None)
                    return false;
            }
            return true;
        }

        public bool IsFull()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == DrinkColor.None)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// A container is sorted if it is completely empty OR completely full with one color.
        /// Partially filled containers are NOT sorted — all slots must be filled with the same color.
        /// </summary>
        public bool IsSorted()
        {
            // Completely empty = sorted
            if (IsEmpty())
                return true;

            // Must be completely full
            if (!IsFull())
                return false;

            // All slots must be the same color
            var firstColor = _slots[0];
            for (int i = 1; i < _slots.Length; i++)
            {
                if (_slots[i] != firstColor)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// A completed container is full with one color (sorted and non-empty).
        /// Completed containers are locked — cannot be selected, poured from, or poured into.
        /// </summary>
        public bool IsCompleted()
        {
            return !IsEmpty() && IsSorted();
        }

        /// <summary>
        /// Returns the number of filled (non-None) slots.
        /// </summary>
        public int FilledCount()
        {
            int count = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != DrinkColor.None)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Returns the index of the lowest empty slot (scanning bottom-up), or -1 if full.
        /// </summary>
        public int GetFirstEmptyIndex()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == DrinkColor.None)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Removes the topmost non-empty color and returns it.
        /// Returns None if container is empty.
        /// </summary>
        public DrinkColor RemoveTop()
        {
            int topIndex = GetTopIndex();
            if (topIndex < 0)
                return DrinkColor.None;

            var color = _slots[topIndex];
            _slots[topIndex] = DrinkColor.None;
            return color;
        }

        /// <summary>
        /// Places color in the lowest empty slot.
        /// Returns true if successful, false if container is full.
        /// </summary>
        public bool AddToTop(DrinkColor color)
        {
            int emptyIndex = GetFirstEmptyIndex();
            if (emptyIndex < 0)
                return false;

            _slots[emptyIndex] = color;
            return true;
        }

        public ContainerData Clone()
        {
            return new ContainerData(_slots);
        }
    }
}
