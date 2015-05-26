using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ForestSolver;
using ForestSolverPackages;
using Point = ForestSolverPackages.Point;

namespace Server
{
    public class PlayerBot
    {
        public readonly TcpClient Client;
        public readonly ForestKeeper Keeper;
        public PlayerBot(TcpClient client, ForestKeeper keeper)
        {
            Client = client;
            Keeper = keeper;
        }
    }
    class ServersConnection
    {
        private readonly TcpListener listener;
        private readonly ServerWorker serverWorker;
        private TcpClient visualizer;
        private readonly Dictionary<TcpClient, string> clients;
        private readonly List<PlayerBot> players;

        public ServersConnection(ServerWorker serverWorker, IPAddress address, int port)
        {
            this.serverWorker = serverWorker;
            ThreadPool.SetMinThreads(8, 8);
            listener = new TcpListener(address, port);
            clients = new Dictionary<TcpClient, string>();
            players = new List<PlayerBot>();
        }

        public void Start()
        {
            listener.Start();
            Begin();
            foreach (var client in clients)
            {
                InitializeClient(client.Key, client.Value);
            }
            InitializeVisualizer();
            RunGame();
        }

        private void InitializeVisualizer()
        {
            var playersForVis =
                players.Select(
                    x =>
                        new Player(x.Keeper.Id, x.Keeper.Name, x.Keeper.Position.ConvertToNetPoint(),
                            x.Keeper.Destination.ConvertToNetPoint(), x.Keeper.Hp)).ToArray();
            var map = serverWorker.GetMap();
            var worldInfo = new WorldInfo {Map = map, Players = playersForVis};
            JSon.Write(worldInfo, visualizer.GetStream());
        }

        private TcpClient WaitClientConnect(out Hello packet)
        {
            var client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            packet = JSon.Read<Hello>(stream, 100000);
            return client;
        }

        private void Begin()
        {
            int paticipantsCount = 0;
            while (paticipantsCount < serverWorker.MaxPaticipants || visualizer == null)
            {
                Hello packet;
                try
                {
                    TcpClient client = WaitClientConnect(out packet);
                    if (packet == null)
                        continue;
                    if (packet.IsVisualizator)
                        visualizer = client;
                    else
                    {
                        if (paticipantsCount < serverWorker.MaxPaticipants)
                            clients.Add(client, packet.Name);
                        paticipantsCount++;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void InitializeClient(TcpClient client, string name)
        {
            var rr = CreateClientInfo(name);
            var clientInfo = rr.Item1;
            var keeper = rr.Item2;
            JSon.Write(clientInfo, client.GetStream());
            players.Add(new PlayerBot(client, keeper));
        }

        private void RunGame()
        {
            var visStream = visualizer.GetStream();
            while (true)
            {
                var ans = JSon.Read<Answer>(visStream);
                if (ans.AnswerCode == 0)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        var playerBot = players[i];
                        RunOneStep(playerBot);
                        players[i] = playerBot;
                    }
                    if (players.Count == 0)
                    {
                        serverWorker.IsOver = true;
                    }
                }
                serverWorker.CheckForHp(players);
                var lastMoveInfo = CreateLastMoveInfo();
                JSon.Write(lastMoveInfo, visStream);
                if (serverWorker.IsOver)
                {
                    foreach (var player in players)
                    {
                        player.Client.Close();
                    }
                    listener.Stop();
                    break;
                }
            }
        }

        private LastMoveInfo CreateLastMoveInfo()
        {
            var changedPositions = players
                .Select(x => Tuple.Create(x.Keeper.Id, x.Keeper.Position.ConvertToNetPoint(), x.Keeper.Hp))
                .ToArray();
            var changedCells = serverWorker.GetChangedCells(changedPositions);
            var lastMoveInfo = new LastMoveInfo
            {
                ChangedCells = changedCells,
                GameOver = serverWorker.IsOver,
                PlayersChangedPosition = changedPositions,
                WinnerId = serverWorker.winnerId
            };
            return lastMoveInfo;
        }

        private void RunOneStep(PlayerBot player)
        {
            try
            {
                var move = JSon.Read<Move>(player.Client.GetStream());
                var res = 1;
//                Console.WriteLine(move.Direction);
                if (serverWorker.Move(move.Direction, player.Keeper))
                    res = 0;
                if (serverWorker.IsOver)
                    res = 2;
                var visibleMap = serverWorker.GetVisibleMap(player.Keeper);
                var result = new MoveResultInfo
                {
                    Result = res,
                    VisibleMap = visibleMap
                };
                JSon.Write(result, player.Client.GetStream());
            }
            catch (Exception)
            {
                players.Remove(player);
                player.Client.Close();
            }
        }

        private Tuple<ClientInfo, ForestKeeper> CreateClientInfo(string name)
        {
            var a = serverWorker.AddClient(name);
            var player = a.Item1;
            var keeper = a.Item2;
            var visibleMap = serverWorker.GetVisibleMap(keeper);
            var clientInfo = new ClientInfo
            {
                MapSize = new Point(serverWorker.Forest.Field.GetLength(0), serverWorker.Forest.Field.GetLength(1)),
                Hp = player.Hp,
                StartPosition = player.StartPosition,
                Target = player.Target,
                VisibleMap = visibleMap
            };
            return Tuple.Create(clientInfo, keeper);
        }
    }
}
