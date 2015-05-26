using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using ForestSolver;

namespace Server
{
    static class Program
    {
        static void Main()
        {
//            string source = "input.txt";
            const string patSourse = "config.xml";
            var serializer = new XmlSerializer(typeof(Config));
            var config = (Config)serializer.Deserialize(File.OpenRead(patSourse));
            var map = FileReader.GetField(config.Filename);
            var paticipants = config.Points.Select(x => Tuple.Create(new Point(x.StartPointX, x.StartPointY), new Point(x.TargetX, x.TargetY), x.Hp)).ToList();
//            var paticipants = FileReader.GetAllPaticipants(patSourse);
            var forest = new Forest(map, 0);
            var serverWorker = new ServerWorker(forest, paticipants);
            var server = new ServersConnection(serverWorker, IPAddress.Parse("127.0.0.1"), 20000);
            server.Start();
        }
    }
}
