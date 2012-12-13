using System;

namespace ezyKnight.Hubs
{
    public interface IEvent
    {
        bool ShouldExecute(DateTime tick);
        void Execute();
        bool Executed { get; }
    }
}