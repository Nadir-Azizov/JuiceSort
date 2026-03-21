namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Fixed-capacity stack for puzzle state snapshots.
    /// When full, oldest snapshot is silently dropped.
    /// Uses circular buffer — no allocations during push/pop.
    /// </summary>
    public class UndoStack
    {
        private readonly PuzzleState[] _buffer;
        private readonly int _capacity;
        private int _head; // next write position
        private int _count;

        public int Count => _count;
        public int Capacity => _capacity;

        public UndoStack(int capacity)
        {
            _capacity = capacity;
            _buffer = new PuzzleState[capacity];
            _head = 0;
            _count = 0;
        }

        /// <summary>
        /// Pushes a snapshot. If at capacity, oldest is silently dropped.
        /// </summary>
        public void Push(PuzzleState snapshot)
        {
            _buffer[_head] = snapshot;
            _head = (_head + 1) % _capacity;

            if (_count < _capacity)
                _count++;
        }

        /// <summary>
        /// Returns and removes the most recent snapshot, or null if empty.
        /// </summary>
        public PuzzleState Pop()
        {
            if (_count == 0)
                return null;

            _head = (_head - 1 + _capacity) % _capacity;
            _count--;

            var snapshot = _buffer[_head];
            _buffer[_head] = null;
            return snapshot;
        }

        public void Clear()
        {
            for (int i = 0; i < _capacity; i++)
                _buffer[i] = null;

            _head = 0;
            _count = 0;
        }
    }
}
