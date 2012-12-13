using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace ezyKnight.Hubs
{
    public interface IEvent
    {
        bool ShouldExecute(DateTime tick);
        void Execute();
        bool Executed { get; }
    }
    public class World
    {
        private static World _world;
        private bool _stop;
        private Timer gameTimer;
        private readonly IList<Player> players = new List<Player>();
        private readonly List<IEvent> events = new List<IEvent>();
        public static void Spawn()
        {
            _world = new World();
            new Task(_world.Loop).Start();
        }
        public static void AddPlayer(Player player)
        {
            _world.players.Add(player);
        }
        public static IList<Player> GetPlayers()
        {
            return _world.players;
        }
        public static void AddEvent(IEvent @event)
        {
            _world.events.Insert(0, @event);
        }

        public void Loop()
        {
            _world.gameTimer = new Timer(30) { AutoReset = true };
            _world.gameTimer.Elapsed += gameTimer_Elapsed;
            _world.gameTimer.Start();
        }

        void gameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //HAndle all shit here.
            _world.gameTimer.Stop();
            if (events.Count <= 0)
            {
                _world.gameTimer.Start();
                return;
            }

            for (int i = events.Count - 1; i > -1; i--)
            {
                var @event = events[i];
                if (@event.ShouldExecute(e.SignalTime))
                {
                    Debug.WriteLine("Executed");
                    @event.Execute();
                    events.RemoveAt(i);
                }
            }
            _world.gameTimer.Start();
            //foreach (var @event in events.Where(x => !x.Executed))
            //{
            //    if (@event.ShouldExecute(e.SignalTime))
            //    {
            //        Debug.WriteLine("Executed");
            //        @event.Execute();
            //    }

            //}
        }

        public static void RemovePlayer(Player player)
        {
            _world.players.Remove(player);
        }
    }
    public class DelayedChatMessage : IEvent
    {
        public DateTime Created { get; private set; }
        public DateTime ExecuteTime { get; private set; }
        public DelayedChatMessage(int delay)
        {
            Created = DateTime.Now;
            ExecuteTime = Created.AddMilliseconds(delay);
        }

        public string Message { get; set; }

        public bool ShouldExecute(DateTime tick)
        {
            return tick > ExecuteTime;
        }

        public void Execute()
        {
            Executed = true;
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients.Group("Players").addChatMessage(string.Format("[{0}] {1} -> {2}", DateTime.Now, "Event trigged chat message", Message));
        }

        public bool Executed { get; private set; }
    }

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
        public string ConnectionId { get; set; }
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
            var spell = new Spell() { Damage = new Values(10, 100), Name = "Melee", Range = 26, Speed = 100 };
            var player = World.GetPlayers().FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (player.IsDead)
                return Clients.Caller.addChatMessage("You are DEAD!");
            if (!player.HasSpell(spell))
                return Clients.Caller.addChatMessage("You dont have that spell");



            var attackCords = player.Attack(spell);

            var enemy = World.GetPlayers().FirstOrDefault(x => x.X == attackCords.Value1 && x.Y == attackCords.Value2);

            if (enemy == null)
                return Clients.Caller.addChatMessage("You missed with " + spell.Name);

            //TODO: Make some random shit here.
            enemy.Attacked(spell.Damage.Value2);


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