using System;

namespace ezyKnight.Events
{
    public interface IEvent
    {
        bool ShouldExecute(DateTime tick);
        void Execute(DateTime tick);
        bool Executed { get; }
        DateTime ExecuteTime { get; }
    }
}