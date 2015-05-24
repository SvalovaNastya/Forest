namespace ForestSolver
{
    public class Trap : ICell
    {
        public bool MakeTurn(ForestKeeper keeper, Point position, ref ICell cell)
        {
            keeper.Hp -= 1;
            keeper.Position = position;
            cell = this;
            return true;
        }
    }
}
