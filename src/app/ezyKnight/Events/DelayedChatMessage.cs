using System;
using Microsoft.AspNet.SignalR;

namespace ezyKnight.Hubs
{
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
}