using System;
using System.Collections.Generic;
using System.Linq;

namespace ForestSolver
{
    public enum MyColour
    {
        White,
        Black

    }
    public class CellWithColor
    {
        public ICell Cell { get; private set; }
        public MyColour Color { get; set; }

        public CellWithColor(ICell cell, MyColour color)
        {
            Cell = cell;
            Color = color;
        }

    }
    public class KeeperAi
    {
        private readonly ExpansibleList<CellWithColor> field;
        private readonly ForestKeeper keeper;
        private readonly Point target;

        public KeeperAi(ForestKeeper keeper)
        {
            field = new ExpansibleList<CellWithColor>(() => new CellWithColor(new Path(), MyColour.White));
            target = keeper.Destination;
            field[target.X, target.Y] = new CellWithColor(new Path(), MyColour.White);
            field[keeper.Position.X, keeper.Position.Y] = new CellWithColor(new Path(), MyColour.White);
            this.keeper = keeper;
        }

        public void Go(Func<ForestKeeper, DeltaPoint , bool> tryMove)
        {
            Point position = keeper.Position;
            DeltaPoint deltaPoint = BFS();
            if (!tryMove(keeper, deltaPoint))
            {
                var newPosition = position.Add(deltaPoint);
                field[newPosition.X, newPosition.Y] = new CellWithColor(new Wall(), MyColour.White);
            }
        }

        private DeltaPoint BFS()
        {
            var queue = new Queue<Point>();
            queue.Enqueue(target);

            for(int i = 0; i < field.RowCount; i++)
                for (int j = 0; j < field.ColumnCount; j++)
                    field[i,j].Color = MyColour.White;
            field[target.X, target.Y].Color = MyColour.Black;
            var neighbours = new[] {DeltaPoint.GoDown(), DeltaPoint.GoLeft(), DeltaPoint.GoRight(), DeltaPoint.GoUp()};
            while (queue.Count() != 0)
            {
                var position = queue.Dequeue();
                foreach (var neighbour in neighbours)
                {
                    var newPosition = position.Add(neighbour);
                    if (newPosition.X < 0 || newPosition.Y < 0)
                        continue;
                    if (newPosition == keeper.Position)
                        return neighbour.Reverse();
                    if (newPosition.X >= field.RowCount || newPosition.Y >= field.ColumnCount)
                        field[newPosition.X, newPosition.Y] = new CellWithColor(new Path(), MyColour.White);
                    if (field[newPosition.X, newPosition.Y].Color == MyColour.White &&
                        field[newPosition.X, newPosition.Y].Cell.GetType() != typeof (Wall))
                    {
                        queue.Enqueue(newPosition);
                        field[newPosition.X, newPosition.Y].Color = MyColour.Black;
                    }
                }
            }
            throw new Exception("невозможно прийти");
        }
    }

    public class ExpansibleList<T>
    {
        private readonly List<List<T>> arr;
        private readonly Func<T> defaultElement;
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        public ExpansibleList(Func<T> defaultElement)
        {
            this.defaultElement = defaultElement;
            arr = new List<List<T>>();
            RowCount = 0;
            ColumnCount = 0;
        }

        public T this[int i, int j]
        {
            get
            {
                if (i < 0 || j < 0)
                    throw new IndexOutOfRangeException("the index is negative");
                if (i >= RowCount || j >= ColumnCount)
                    return defaultElement();
                return arr[i][j];
            }
            set
            {
                if (i < 0 || j < 0)
                    throw new IndexOutOfRangeException("the index is negative");
                while (RowCount <= i)
                {
                    arr.Add(new List<T>());
                    RowCount++;
                }
                foreach (var row in arr)
                    while (row.Count <= j || row.Count < ColumnCount)
                    {
                        row.Add(defaultElement());
                    }
                if (ColumnCount <= j)
                    ColumnCount = j + 1;
                arr[i][j] = value;
            }
        }
    }
}
