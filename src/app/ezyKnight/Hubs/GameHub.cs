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
        private const int MaxX = 500;
        private const int MaxY = 500;
        

        public Task Join(string name, string color)
        {
            if (MvcApplication.Players.Any(x => x.Value.Name == name))
            {
                return Clients.Caller.addChatMessage("Player with that name already exsists");
            }

            MvcApplication.Players.Add(Context.ConnectionId, new Player() { Id = Context.ConnectionId, Name = name, Color = color });
            Groups.Add(Context.ConnectionId, "Players");
            return Clients.Group("Players").joined(MvcApplication.Players.ToArray());
        }

        public Task SendPlayers()
        {
            return Clients.Group("Players").joined(MvcApplication.Players.ToArray());
        }

        public Task Send(string message)
        {
            return Clients.OthersInGroup("Players").addChatMessage(MvcApplication.Players[Context.ConnectionId].Name + " -> " + message);
        }

        public Task Move(int x, int y)
        {
            if (!MvcApplication.Players.ContainsKey(Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }
            var player = MvcApplication.Players[Context.ConnectionId];

            if (MvcApplication.Players.Any(p => p.Value.X == x && p.Value.Y == y))
                return Clients.Caller.collision(player);

            if (x < 0 || y < 0 || y > MaxY || x > MaxX)
                return Clients.Caller.collision(player);

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