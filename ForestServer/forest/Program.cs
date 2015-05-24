using System;
using System.Threading;

namespace ForestSolver
{
    static class Program
    {
        static void Main()
        {
            const string source = "input.txt";
            var map = FileReader.GetField(source);
            var forest = new Forest(map, 0);
            var visualizer = new ConsoleBlackAndWhiteVisualizer();
            var keeper = forest.MakeNewKeeper("Thranduil", 'A', new Point(2, 1), new Point(3, 3), 2);
            var keeperAi = new KeeperAi(keeper);
            visualizer.DrawForest(forest);
            while (keeper.Position != keeper.Destination)
            {
                keeperAi.Go(forest.Move);
                Thread.Sleep(1000);
                Console.Clear();
                visualizer.DrawForest(forest);
            }
            Console.WriteLine("цель достигнута");
        }
    }
}
