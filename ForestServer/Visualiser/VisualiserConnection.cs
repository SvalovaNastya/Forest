using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ForestSolver;
using ForestSolverPackages;

namespace Visualiser
{
    internal class VisualiserConnection
    {
        private readonly TcpClient server;
        private readonly VisualiserWorker worker;
        private Player[] players;
        private int[,] map;
        private readonly IPAddress address;
        private readonly int port;

        public VisualiserConnection(VisualiserWorker worker, IPAddress address, int port)
        {
            server = new TcpClient();
            this.worker = worker;
            this.address = address;
            this.port = port;
        }

        public void Start()
        {
            Initialise();
            GetAllClients();
            RunGame();
        }

        private void GetAllClients()
        {
            var stream = server.GetStream();
            var worldInfo = JSon.Read<WorldInfo>(stream);
            players = worldInfo.Players;
            map = worldInfo.Map;
        }

        private void RunGame()
        {
            while (true)
            {
                var stream = server.GetStream();
                var ans = new Answer {AnswerCode = 0};
                JSon.Write(ans, stream);
                var lastMoveInfo = JSon.Read<LastMoveInfo>(stream);
                ApplyLastMoveInfo(lastMoveInfo);
                worker.Draw(map, players);
                if (lastMoveInfo.GameOver)
                    break;
                Thread.Sleep(500);
            }
        }

        private void ApplyLastMoveInfo(LastMoveInfo lastMoveInfo)
        {
            foreach (var changedCell in lastMoveInfo.ChangedCells)
            {
                map[changedCell.Item1.X, changedCell.Item1.Y] = changedCell.Item2;
            }
            foreach (var player in lastMoveInfo.PlayersChangedPosition)
            {
                var p = players.FirstOrDefault(x => x.Id == player.Item1);
                p.StartPosition = player.Item2;
                p.Hp = player.Item3;
            }
        }

        private void Initialise()
        {
            server.Connect(address, port);
            var stream = server.GetStream();
            var helloPacket = new Hello
            {
                IsVisualizator = true,
            };
            JSon.Write(helloPacket, stream);
        }
    }
}
