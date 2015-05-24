using ForestSolver;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests
{
    [TestFixture]
    public class ForestTests
    {
        [Test]
        public void LifeAddHp()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Life();
            field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.AreEqual(3, keeper.Hp);
        }

        [Test]
        public void LifeTransformation()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Life();
            field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.IsTrue(field[0, 1].GetType() == typeof(Path));
        }

        [Test]
        public void CanMoveToLife()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Life();
            var ans = field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.IsTrue(ans);
            Assert.AreEqual(0,keeper.Position.X);
            Assert.AreEqual(1, keeper.Position.Y);
        }

        [Test]
        public void CanMoveToPath()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Path();
            var ans = field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.IsTrue(ans);
            Assert.AreEqual(0, keeper.Position.X);
            Assert.AreEqual(1, keeper.Position.Y);
        }

        [Test]
        public void CanMoveToTrap()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Trap();
            var ans = field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.IsTrue(ans);
            Assert.AreEqual(0, keeper.Position.X);
            Assert.AreEqual(1, keeper.Position.Y);
        }

        [Test]
        public void TrapSubtractHp()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Trap();
            field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.AreEqual(1, keeper.Hp);
        }

        [Test]
        public void CanMoveToWall()
        {
            var keeper = new ForestKeeper("aa", new Point(0, 0), new Point(10, 10), 2, 'A');
            var field = new ICell[2, 2];
            field[0, 1] = new Wall();
            var ans = field[0, 1].MakeTurn(keeper, new Point(0, 1), ref field[0, 1]);
            Assert.IsFalse(ans);
            Assert.AreEqual(0, keeper.Position.X);
            Assert.AreEqual(0, keeper.Position.Y);
        }

        [Test]
        public void SimpleMap()
        {
            RunAi("test1.txt", new Point(3, 3), new Point(1, 1));
        }

        [Test]
        public void HardMap()
        {
            RunAi("test2.txt", new Point(2, 3), new Point(10, 3));
        }

        [Test]
        public void SampleMap()
        {
            RunAi("test3.txt", new Point(3, 3), new Point(1, 1));
        }

        [Test]
        public void Map()
        {
            RunAi("test5.txt", new Point(8, 1), new Point(1, 8));
        }

        private static void RunAi(string sourse, Point target, Point keeperPosition)
        {
            string source = sourse;
            var map = FileReader.GetField(source);
            var forest = new Forest(map, 0);
            ForestKeeper keeper = forest.MakeNewKeeper("Thranduil", 'A', keeperPosition, target, 2);
            var keeperAi = new KeeperAi(keeper);
            while (keeper.Position != keeper.Destination)
            {
                keeperAi.Go(forest.Move);
            }
            Assert.AreEqual(keeper.Position, new Point(target.Y, target.X));
        }
    }
}
