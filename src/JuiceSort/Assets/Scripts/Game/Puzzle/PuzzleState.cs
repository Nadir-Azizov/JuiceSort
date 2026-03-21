namespace JuiceSort.Game.Puzzle
{
    /// <summary>
    /// Pure C# data model for the entire puzzle board state.
    /// </summary>
    public class PuzzleState
    {
        private readonly ContainerData[] _containers;

        public int ContainerCount => _containers.Length;

        public PuzzleState(ContainerData[] containers)
        {
            _containers = containers;
        }

        public ContainerData GetContainer(int index)
        {
            return _containers[index];
        }

        /// <summary>
        /// Win condition: all containers are sorted (each holds only one color or is empty).
        /// </summary>
        public bool IsAllSorted()
        {
            for (int i = 0; i < _containers.Length; i++)
            {
                if (!_containers[i].IsSorted())
                    return false;
            }
            return true;
        }

        public PuzzleState Clone()
        {
            var clonedContainers = new ContainerData[_containers.Length];
            for (int i = 0; i < _containers.Length; i++)
            {
                clonedContainers[i] = _containers[i].Clone();
            }
            return new PuzzleState(clonedContainers);
        }
    }
}
