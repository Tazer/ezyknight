using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;

namespace ezyKnight.Hubs
{
    public class Spell
    {
        public string Name { get; set; }
        public int Range { get; set; }
        public int Speed { get; set; }
        public Tuple<int, int> Damage { get; set; }
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Class { get; set; }
        public Orientation Orientation { get; set; }

        public bool HasSpell(Spell spell)
        {
            //Handle logic here.
            return true;
        }

        public void MoveTo(int x, int y)
        {
            if (x > this.X)
                Orientation = Orientation.Right;

            if (x < this.X)
                Orientation = Orientation.Left;

            if (y < this.Y)
                Orientation = Orientation.Up;

            if (y > this.Y)
                Orientation = Orientation.Down;


            this.X = x;
            this.Y = y;
        }
        public Tuple<int, int> Attack(Spell spell)
        {
            switch (Orientation)
            {
                case Orientation.Right:
                    return Tuple.Create(X + spell.Range, Y);
                case Orientation.Left:
                    return Tuple.Create(X - spell.Range, Y);
                case Orientation.Up:
                    return Tuple.Create(X, Y - spell.Range);
                case Orientation.Down:
                    return Tuple.Create(X, Y + spell.Range);
            }
        }
    }

    public enum Orientation
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }



    public class GameHub : Hub
    {
        private const int MaxX = 960;
        private const int MaxY = 422;


        public Task Join(string name, int userClass)
        {
            if (MvcApplication.Players.Any(x => x.Value.Name == name))
            {
                return Clients.Caller.addChatMessage("Player with that name already exsists");
            }

            var rndClass = new Random().Next(0, 4);

            MvcApplication.Players.Add(Context.ConnectionId, new Player() { Id = Context.ConnectionId, Name = name, Class = userClass });
            Groups.Add(Context.ConnectionId, "Players");
            return Clients.Group("Players").joined(MvcApplication.Players.ToArray());
        }

        public Task SendPlayers()
        {
            return Clients.Group("Players").joined(MvcApplication.Players.ToArray());
        }

        public Task Send(string message)
        {
            return Clients.OthersInGroup("Players").addChatMessage(string.Format("[{0}] {1} -> {2}", DateTime.Now, MvcApplication.Players[Context.ConnectionId].Name, message));
        }

        public Task Attack(int spellId)
        {
            if (!MvcApplication.Players.ContainsKey(Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }

            //Hard coding one spell now.
            var spell = new Spell() {Damage = Tuple.Create(10, 100), Name = "Melee",Range = 26,Speed = 100};
            var player = MvcApplication.Players[Context.ConnectionId];

            if(!player.HasSpell(spell))
                return Clients.Caller.addChatMessage("You dont have that spell");



            var attackCords = player.Attack(spell);


            return Clients.Group("Players").joined(MvcApplication.Players.ToArray());
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

            player.MoveTo(x, y);
            return Clients.Group("Players").moved(player);
        }

        public override Task OnConnected()
        {
            return Clients.Group("Players").newconnection(Context.ConnectionId);
        }

        public override Task OnDisconnected()
        {
            if (MvcApplication.Players.ContainsKey(Context.ConnectionId))
                MvcApplication.Players.Remove(Context.ConnectionId);
            return Clients.Group("Players").joined(MvcApplication.Players.ToArray());
        }
    }
}