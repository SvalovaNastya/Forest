using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestSolver
{
    public class Path : ICell
    {
        public bool MakeTurn(ForestKeeper keeper, Point position, ref ICell cell)
        {
            keeper.Position = position;
            cell = this;
            return true;
        }
    }
}
