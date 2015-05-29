using System.Net;
using System.Threading.Tasks;

namespace Client
{
    static class Program
    {
        static void Main()
        {
            var client = new ClientConnection(new ClientWorker(), "Simple bot", IPAddress.Parse("127.0.0.1"), 20000);
            client.Begin();
//            var task = Task.Run(() => client.Begin());
//            var clientq = new ClientConnection(new ClientWorker(), "t", IPAddress.Parse("127.0.0.1"), 20000);
//            var task2 = Task.Run(() => clientq.Begin());
//            Task.WaitAll(new[] {task, task2});
        }
    }
}
