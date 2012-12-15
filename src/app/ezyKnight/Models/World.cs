using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using ezyKnight.Events;

namespace ezyKnight.Models
{
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
            if (events.Count <= 0)
            {

                return;
            }

            for (int i = events.Count - 1; i > -1; i--)
            {
                var @event = events[i];
                if (@event.ShouldExecute(e.SignalTime))
                {
                    Debug.WriteLine("Executed");
                    events.RemoveAt(i);
                    @event.Execute(e.SignalTime);

                }
            }
        }

        public static void RemovePlayer(Player player)
        {
            _world.players.Remove(player);
        }
    }
}