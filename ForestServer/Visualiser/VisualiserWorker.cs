using System;
using System.Collections.Generic;
using ForestSolver;
using ForestSolverPackages;

namespace Visualiser
{
    class VisualiserWorker
    {
        private readonly Dictionary<int, Func<ICell>> cellsNum;
        private readonly IVisualizer visualizer;

        public VisualiserWorker(IVisualizer visualizer)
        {
            cellsNum = new Dictionary<int, Func<ICell>>
            {
                {1, () => new Path()},
                {2, () => new Wall()},
                {3, () => new Trap()},
                {4, () => new Life()}
            };
            this.visualizer = visualizer;
        }

        public void Draw(int[,] map, Player[] players)
        {
            var field = new ICell[map.GetLength(0), map.GetLength(1)];
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                    field[i,j] = cellsNum[map[i, j]]();
            }
            var forest = new Forest(field, 0);
            foreach (var player in players)
            {
                forest.MakeNewKeeper(player.Nick, (char)player.Id, ForestSolver.Point.ConvertFromNetPoint(player.StartPosition).Reverse(), ForestSolver.Point.ConvertFromNetPoint(player.Target).Reverse(), player.Hp);
            }
            visualizer.DrawForest(forest);
        }
    }
}
