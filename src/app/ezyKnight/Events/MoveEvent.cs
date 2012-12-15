using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using ezyKnight.Hubs;
using ezyKnight.Models;

namespace ezyKnight.Events
{
    public class MoveEvent : IEvent
    {
        private readonly Player _player;
        private readonly int _x;
        private readonly int _y;
        private const int MaxX = 960;
        private const int MaxY = 422;

        public MoveEvent(Player player,int x,int y)
        {
            _player = player;
            _player.IsMoving = true;
            _x = x;
            _y = y;
            ExecuteTime = DateTime.UtcNow;
        }

        public bool ShouldExecute(DateTime tick)
        {
            return (tick > ExecuteTime);

        }

        public void Execute(DateTime tick)
        {
            Executed = true;

            if(!_player.IsMoving)
                return;

            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            if (World.GetPlayers().Any(p => p.X == _x && p.Y == _y)){
                context.Clients.Client(_player.ConnectionId).collision(_player);
                return;
            }

            if (_x < 0 || _y < 0 || _y > MaxY || _x > MaxX){
                context.Clients.Client(_player.ConnectionId).collision(_player);
                return;
            }

            if (_player.IsDead){
                context.Clients.Client(_player.ConnectionId).addChatMessage("You are DEAD!");
                return;
            }
            if (!_player.IsMoving)
                return;
            _player.MoveTo(_x, _y);
            context.Clients.Group("Players").moved(_player);
        }

        public bool Executed { get; private set; }
        public DateTime ExecuteTime { get; private set; }
    }
}