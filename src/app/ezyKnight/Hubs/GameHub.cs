using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace ezyKnight.Hubs
{
    public class Player
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }



    public class GameHub : Hub
    {
        public static IDictionary<string, Player> Players = new Dictionary<string, Player>();
        private string Motd = "Welcome to EzyKnight";

        public Task Join(string name)
        {
            if (Players.Any(x => x.Value.Name == name))
            {
                return Clients.Caller.addChatMessage("Player with that name already exsists");
            }

            Players.Add(Context.ConnectionId, new Player() { Name = name });
            return Groups.Add(Context.ConnectionId, "Players");
        }

        public Task Send(string message)
        {
            return Clients.OthersInGroup("Players").addChatMessage(Players[Context.ConnectionId].Name + " -> " + message);
        }

        public override Task OnConnected()
        {
            return Clients.Group("Players").joined(Context.ConnectionId);
        }
    }
}