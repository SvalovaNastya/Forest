using System;
using System.Collections.Generic;
using Server;
using ForestSolver;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ServwrWorkerTests
    {
        private ICell[,] field;
        private ServerWorker worker;
        private ForestKeeper keeper;

        [TestFixtureSetUp]
        public void SetUp()
        {
            field = FileReader.GetField("input.txt");
            
            keeper = new ForestKeeper("a", new Point(2, 2), new Point(4, 4), 2, 'f');
            var forest = new Forest(field, 2);
            forest.Keepers.Add(keeper);
            worker = new ServerWorker(forest, new List<Tuple<Point, Point>>());
        }

        [Test]
        public void CorrectGetVisibleMap()
        {
            keeper.Position = new Point(2, 2);
            var ansMap = new[,]
            {
                {2, 2, 2, 2, 2},
                {2, 3, 1, 4, 2},
                {2, 1, 5, 1, 2},
                {2, 1, 1, 1, 2},
                {2, 2, 2, 2, 2}
            };
            var ans = worker.GetVisibleMap(keeper);
            Assert.AreEqual(ansMap, ans);
        }

        [Test]
        public void CorrectGetVisibleMapOutField()
        {
            keeper.Position = new Point(1,1);
            var ansMap = new[,]
            {
                {0, 0, 0, 0, 0},
                {0, 2, 2, 2, 2},
                {0, 2, 5, 1, 4},
                {0, 2, 1, 1, 1},
                {0, 2, 1, 1, 1}
            };
            var ans = worker.GetVisibleMap(keeper);
            Assert.AreEqual(ansMap, ans);
        }
    }
}
