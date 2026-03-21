namespace JuiceSort.Game.LevelGen
{
    public struct SolveResult
    {
        public bool IsSolvable;
        public int MoveCount;

        public SolveResult(bool isSolvable, int moveCount)
        {
            IsSolvable = isSolvable;
            MoveCount = moveCount;
        }
    }
}
