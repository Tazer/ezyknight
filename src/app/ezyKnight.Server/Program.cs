using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ezyKnight.Server.Hubs;

namespace ezyKnight.Server
{
    public class Program
    {
        public static IDictionary<string, Player> Players = new Dictionary<string, Player>();
        static void Main(string[] args)
        {
            string url = "http://localhost:8081/";
            var server = new Microsoft.AspNet.SignalR.Hosting.Self.Server(url);

            // Map the default hub url (/signalr)
            server.MapHubs();

            // Start the server
            server.Start();

            Console.WriteLine("Server running on {0}", url);

            // Keep going until somebody hits 'x'
            while (true)
            {
                ConsoleKeyInfo ki = Console.ReadKey(true);
                if (ki.Key == ConsoleKey.X)
                {
                    break;
                }
            }
        }
    }
}
