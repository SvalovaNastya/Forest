namespace ForestSolver
{
    public interface ICell
    {
        bool MakeTurn(ForestKeeper keeper, Point position, ref ICell cell);
    }
}
