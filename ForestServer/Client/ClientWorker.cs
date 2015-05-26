using System;
using ForestSolver;
using ForestSolverPackages;

namespace Client
{
    class ClientWorker
    {
        private KeeperAi ai;
        private ForestKeeper keeper;

        public void Initialise(ClientInfo info, string name)
        {
            keeper = new ForestKeeper(name, ForestSolver.Point.ConvertFromNetPoint(info.StartPosition).Reverse(),
                ForestSolver.Point.ConvertFromNetPoint(info.Target).Reverse(), info.Hp, 1);
            ai = new KeeperAi(keeper);
        }

        public void Move(Func<ForestKeeper, DeltaPoint, bool> tryMove)
        {
//            tryMove(keeper, DeltaPoint.GoDown());
            ai.Go(tryMove);
        }

        public void ChangePosition(DeltaPoint point)
        {
            keeper.Position = keeper.Position.Add(point);
        }
    }
}
