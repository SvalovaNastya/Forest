using System;
using System.Collections.Generic;
using System.Linq;
using ForestSolver;
using ForestSolverPackages;
using Point = ForestSolver.Point;

namespace Server
{
    public class ServerWorker
    {
        public readonly Forest Forest;
        private readonly List<Tuple<Point, Point>> patFirstPos;
        public readonly int MaxPaticipants;
        private int nextId;
        private readonly List<ForestKeeper> keepers;
        public bool IsOver;
        public int winnerId;
        private readonly Dictionary<Type, int> cellsNum = new Dictionary<Type, int>
            {
                {typeof (Path), 1},
                {typeof (Wall), 2},
                {typeof (Trap), 3},
                {typeof (Life), 4}
            };

        public ServerWorker(Forest forest, List<Tuple<Point, Point>> patFirstPos)
        {
            Forest = forest;
            this.patFirstPos = patFirstPos;
            nextId = 0; 
            MaxPaticipants = patFirstPos.Count;
            keepers = new List<ForestKeeper>();
            IsOver = false;
            winnerId = 0;
        }

        public void CheckForHp(List<PlayerBot> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Keeper.Hp <= 0)
                {
                    players[i].Client.Close();
                    players.RemoveAt(i);
                    i--;
                }
            }
        }

        public Tuple<Player, ForestKeeper> AddClient(string name)
        {
            var startPosition = patFirstPos[0].Item1;
            var destination = patFirstPos[0].Item2;
            var keeper = Forest.MakeNewKeeper(name, nextId, startPosition, destination, 2);
            keepers.Add(keeper);
            var player = new Player(nextId, name, startPosition.ConvertToNetPoint(), destination.ConvertToNetPoint(), 2);
            patFirstPos.RemoveAt(0);
            nextId = (char)(nextId + 1);
            return Tuple.Create(player, keeper);
        }

        public bool Move(int direction, ForestKeeper keeper)
        {
            var dicts = new Dictionary<int, Func<DeltaPoint>>
            {
                {3, DeltaPoint.GoUp},
                {2, DeltaPoint.GoRight},
                {1, DeltaPoint.GoDown},
                {0, DeltaPoint.GoLeft}
            };
            var canMove = Forest.Move(keeper, dicts[direction]());
            var dest = keeper.Destination;
            if (keeper.Position == new Point(dest.X, dest.Y))
            {
                IsOver = true;
                winnerId = keeper.Id;
                Console.WriteLine(winnerId);
            }
            return canMove;
        }

        public int[,] GetVisibleMap(ForestKeeper keeper)
        {
            var visibleMap = new int[Forest.FogOfWar * 2 + 1, Forest.FogOfWar * 2 + 1];
            for (int i = 0; i < visibleMap.GetLength(0); i++)
                for (int j = 0; j < visibleMap.GetLength(1); j++)
                {
                    if (keeper.Position.X - Forest.FogOfWar + i < 0 ||
                        keeper.Position.X - Forest.FogOfWar + i >= Forest.Field.GetLength(0) ||
                        keeper.Position.Y - Forest.FogOfWar + j < 0 ||
                        keeper.Position.Y - Forest.FogOfWar + j >= Forest.Field.GetLength(1))
                        visibleMap[i, j] = 0;
                    else
                        visibleMap[i, j] = cellsNum[
                            Forest.Field[
                                i + keeper.Position.X - Forest.FogOfWar,
                                j + keeper.Position.Y - Forest.FogOfWar].GetType()];
                    foreach (var keep in Forest.Keepers)
                        if (keep.Position ==
                            new Point(i + keeper.Position.X - Forest.FogOfWar, j + keeper.Position.Y - Forest.FogOfWar))
                            visibleMap[i, j] = 5;
                }
            return visibleMap;
        }

        public int[,] GetMap()
        {
            var map = new int[Forest.Field.GetLength(0), Forest.Field.GetLength(1)];
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    map[i, j] = cellsNum[Forest.Field[i, j].GetType()];
            return map;
        }

        public Tuple<ForestSolverPackages.Point, int>[] GetChangedCells(Tuple<int, ForestSolverPackages.Point, int>[] changedPositions)
        {
            return
                changedPositions.Select(
                    x => Tuple.Create(x.Item2, cellsNum[Forest.Field[x.Item2.X, x.Item2.Y].GetType()])).ToArray();
        }
    }
}
