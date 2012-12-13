using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;

namespace ezyKnight.Server.Hubs
{
    public class Values
    {
        public Values()
        {

        }

        public Values(int value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }

    public class Spell
    {
        public string Name { get; set; }
        public int Range { get; set; }
        public int Speed { get; set; }
        public Values Damage { get; set; }
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Class { get; set; }
        public Orientation Orientation { get; set; }
        public bool IsDead = false;
        public Values Health = new Values(100, 100);

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
        public Values Attack(Spell spell)
        {
            switch (Orientation)
            {
                case Orientation.Right:
                    return new Values(X + spell.Range, Y);
                case Orientation.Left:
                    return new Values(X - spell.Range, Y);
                case Orientation.Up:
                    return new Values(X, Y - spell.Range);
                case Orientation.Down:
                    return new Values(X, Y + spell.Range);
                default:
                    return null;
            }
        }

        public void Attacked(int damage)
        {
            Health.Value1 -= damage;

            if (Health.Value1 <= 0)
                IsDead = true;

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
            if (Program.Players.Any(x => x.Value.Name == name))
            {
                return Clients.Caller.addChatMessage("Player with that name already exsists");
            }

            var rndClass = new Random().Next(0, 4);

            Program.Players.Add(Context.ConnectionId, new Player() { Id = Context.ConnectionId, Name = name, Class = userClass });
            Groups.Add(Context.ConnectionId, "Players");
            return Clients.Group("Players").joined(Program.Players.ToArray());
        }

        public Task SendPlayers()
        {
            return Clients.Group("Players").joined(Program.Players.ToArray());
        }

        public Task Send(string message)
        {
            return Clients.OthersInGroup("Players").addChatMessage(string.Format("[{0}] {1} -> {2}", DateTime.Now, Program.Players[Context.ConnectionId].Name, message));
        }

        public Task Attack(int spellId)
        {
            if (!Program.Players.ContainsKey(Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }

            //Hard coding one spell now.
            var spell = new Spell() { Damage = new Values(10, 100), Name = "Melee", Range = 26, Speed = 100 };
            var player = Program.Players[Context.ConnectionId];

            if(player.IsDead)
                return Clients.Caller.addChatMessage("You are DEAD!");
            if (!player.HasSpell(spell))
                return Clients.Caller.addChatMessage("You dont have that spell");



            var attackCords = player.Attack(spell);

            var enemy = Program.Players.FirstOrDefault(x => x.Value.X == attackCords.Value1 && x.Value.Y == attackCords.Value2);

            if (enemy.Value == null)
                return Clients.Caller.addChatMessage("You missed with " + spell.Name);

            //TODO: Make some random shit here.
            enemy.Value.Attacked(spell.Damage.Value2);


            return Clients.Group("Players").joined(Program.Players.ToArray());
        }

        public Task Move(int x, int y)
        {
            if (!Program.Players.ContainsKey(Context.ConnectionId))
            {
                return Clients.Caller.addChatMessage("Player hasnt joined");
            }
            var player = Program.Players[Context.ConnectionId];

            if (Program.Players.Any(p => p.Value.X == x && p.Value.Y == y))
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
            if (Program.Players.ContainsKey(Context.ConnectionId))
                Program.Players.Remove(Context.ConnectionId);
            return Clients.Group("Players").joined(Program.Players.ToArray());
        }
    }
}