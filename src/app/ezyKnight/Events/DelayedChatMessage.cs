using System;
using Microsoft.AspNet.SignalR;
using ezyKnight.Hubs;

namespace ezyKnight.Events
{
    public class DelayedChatMessage : IEvent
    {
        public DateTime ExecuteTime { get; private set; }
        public DelayedChatMessage(int delay)
        {
            ExecuteTime = DateTime.Now.AddMilliseconds(delay);
        }

        public string Message { get; set; }

        public bool ShouldExecute(DateTime tick)
        {
            return tick > ExecuteTime;
        }

        public void Execute(DateTime tick)
        {
            Executed = true;
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients.Group("Players").addChatMessage(string.Format("[{0}] {1} -> {2}", DateTime.Now, "Event trigged chat message", Message));
        }

        public bool Executed { get; private set; }
    }
}