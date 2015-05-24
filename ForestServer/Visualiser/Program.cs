using System.Net;
using ForestSolver;

namespace Visualiser
{
    static class Program
    {
        static void Main()
        {
            var visualiserWorker = new VisualiserWorker(new ConsoleBlackAndWhiteVisualizer());
            var visualiser = new VisualiserConnection(visualiserWorker, IPAddress.Parse("127.0.0.1"), 20000);
            visualiser.Start();
        }
    }
}
