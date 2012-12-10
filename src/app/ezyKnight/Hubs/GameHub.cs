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
        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }



    public class GameHub : Hub
    {
        public static IDictionary<string, Player> Players = new Dictionary<string, Player>();
        private string Motd = "Welcome to EzyKnight";

        public Task Join(string name, string color)
        {
            if (Players.Any(x => x.Value.Name == name))
            {
                return Clients.Caller.addChatMessage("Player with that name already exsists");
            }

            Players.Add(Context.ConnectionId, new Player() { Id = Context.ConnectionId, Name = name, Color = color });
            Groups.Add(Context.ConnectionId, "Players");
            return Clients.Group("Players").joined(Players.ToArray());
        }

        public Task Send(string message)
        {
            return Clients.OthersInGroup("Players").addChatMessage(Players[Context.ConnectionId].Name + " -> " + message);
        }

        public Task Move(int x, int y)
        {
            if (!Players.ContainsKey(Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }

            if (Players.Any(p => p.Value.X == x && p.Value.Y == y))
                return Clients.Caller.collision();

            var player = Players[Context.ConnectionId];
            player.X = x;
            player.Y = y;
            return Clients.Group("Players").moved(player);
        }

        public override Task OnConnected()
        {
            return Clients.Group("Players").newconnection(Context.ConnectionId);
        }
    }
}