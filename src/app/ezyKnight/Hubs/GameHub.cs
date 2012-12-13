using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;
using ezyKnight.Events;
using ezyKnight.Models;

namespace ezyKnight.Hubs
{
    public class GameHub : Hub
    {
        private const int MaxX = 960;
        private const int MaxY = 422;


        public Task Join(string name, int userClass)
        {
            if (World.GetPlayers().Any(x => x.Name == name))
            {
                return Clients.Caller.addChatMessage("Player with that name already exsists");
            }

            var rndClass = new Random().Next(0, 4);

            World.AddPlayer(new Player() { Id = Context.ConnectionId, ConnectionId = Context.ConnectionId, Name = name, Class = userClass });
            Groups.Add(Context.ConnectionId, "Players");
            return Clients.Group("Players").joined(World.GetPlayers().ToArray());
        }

        public Task SendPlayers()
        {
            return Clients.Group("Players").joined(World.GetPlayers().ToArray());
        }

        public Task Send(string message)
        {
            var player = World.GetPlayers().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            World.AddEvent(new DelayedChatMessage(2000) { Message = "IM DELAYED!" });
            return Clients.OthersInGroup("Players").addChatMessage(string.Format("[{0}] {1} -> {2}", DateTime.Now, player.Name, message));
        }

        public Task Attack(int spellId)
        {
            if (!World.GetPlayers().Any(p => p.ConnectionId == Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }

            //Hard coding one spell now.
            var spell = new Melee() { Damage = new Values(10, 100), Name = "Melee", Range = 26, Speed = 100 };
            var player = World.GetPlayers().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (player.IsDead)
                return Clients.Caller.addChatMessage("You are DEAD!");
            if (!player.HasSpell(spell))
                return Clients.Caller.addChatMessage("You dont have that spell");

            World.AddEvent(new AttackEvent(player,spell));




            return Clients.Group("Players").joined(World.GetPlayers().ToArray());
        }

        public Task Move(int x, int y)
        {
            if (!World.GetPlayers().Any(p => p.ConnectionId == Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }
            var player = World.GetPlayers().FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

            if (World.GetPlayers().Any(p => p.X == x && p.Y == y))
                return Clients.Caller.collision(player);

            if (x < 0 || y < 0 || y > MaxY || x > MaxX)
                return Clients.Caller.collision(player);

            if (player.IsDead)
                return Clients.Caller.addChatMessage("You are DEAD!");

            player.MoveTo(x, y);
            return Clients.Group("Players").moved(player);
        }

        public override Task OnConnected()
        {
            return Clients.Group("Players").newconnection(Context.ConnectionId);
        }

        public override Task OnDisconnected()
        {
            var player = World.GetPlayers().FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

            if (player != null)
                World.RemovePlayer(player);
            return Clients.Group("Players").joined(World.GetPlayers());
        }
    }
}