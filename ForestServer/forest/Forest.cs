using System;
using System.Collections.Generic;

namespace ForestSolver
{
    public class Forest
    {
        public readonly List<ForestKeeper> Keepers;
        public readonly ICell[,] Field;
        public readonly int FogOfWar;

        public Forest(ICell[,] field, int fogOfWar)
        {
            Field = field;
            FogOfWar = fogOfWar;
            Keepers = new List<ForestKeeper>();
        }

        public bool Move(ForestKeeper keeper, DeltaPoint deltas)
        {
            var newPosition = keeper.Position.Add(deltas);
            foreach (var keep in Keepers)
                if (newPosition == keep.Position && keeper.Id != keep.Id)
                    return false;
            var canMove = Field[newPosition.X, newPosition.Y].MakeTurn(keeper, newPosition, ref Field[newPosition.X, newPosition.Y]);
            if (keeper.Hp <= 0)
                Keepers.Remove(keeper);
            return canMove;
        }

        public ForestKeeper MakeNewKeeper(string name, int id, Point position, Point destination, int hp)
        {
            var keeper = new ForestKeeper(name, new Point(position.Y - 1, position.X), new Point(destination.Y, destination.X), hp, id);
            Keepers.Add(keeper);
            if (!Move(keeper, DeltaPoint.GoRight()))
                throw new Exception("нельзя сюда поставить");
            return keeper;
        }
    }
}
