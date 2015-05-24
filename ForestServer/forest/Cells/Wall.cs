namespace ForestSolver
{
    public class Wall : ICell
    {
        public bool MakeTurn(ForestKeeper keeper, Point position, ref ICell cell)
        {
            cell = this;
            return false;
        }
    }
}
