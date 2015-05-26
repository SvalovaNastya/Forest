using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ForestSolver;
using ForestSolverPackages;

namespace Client
{
    class ClientConnection
    {
        private readonly ClientWorker clientWorker;
        private readonly TcpClient server;
        private readonly string name;
        private bool isGameOver;
        private readonly IPAddress address;
        private readonly int port;

        public ClientConnection(ClientWorker clientWorker, string name, IPAddress address, int port)
        {
            this.clientWorker = clientWorker;
            server = new TcpClient();
            this.name = name;
            isGameOver = false;
            this.address = address;
            this.port = port;
        }

        public void Begin()
        {
            Initialize();
            RunGame();
        }

        private void Initialize()
        {
            server.Connect(address, port);
            var stream = server.GetStream();
            var helloPacket = new Hello
            {
                IsVisualizator = false,
                Name = name
            };
            JSon.Write(helloPacket, stream);
            var clientInfo = JSon.Read<ClientInfo>(stream);
            clientWorker.Initialise(clientInfo, name);
        }

        private void RunGame()
        {
            while (!isGameOver)
                try
                {
                    clientWorker.Move(TryMove);
                }
                catch (Exception)
                {
                    server.Close();
                    break;
                }
        }


        private bool TryMove(ForestKeeper keeper, DeltaPoint point)
        {
            var stream = server.GetStream();
            var directoins = new Dictionary<DeltaPoint, int>
            {
                { DeltaPoint.GoUp(), 3 }, 
                { DeltaPoint.GoRight(), 2 }, 
                { DeltaPoint.GoDown(), 1 }, 
                { DeltaPoint.GoLeft(), 0 }
            };
            var move = new Move {Direction = directoins[point]};
            JSon.Write(move, stream);
            Console.WriteLine(directoins[point]);
            var resultInfo = JSon.Read<MoveResultInfo>(stream);
            if (resultInfo.Result == 2)
            {
                isGameOver = true;
            }
            if (resultInfo.Result == 0)
                clientWorker.ChangePosition(point);
            Console.WriteLine(resultInfo.Result == 0);
            return resultInfo.Result == 0;
        }

    }
}
