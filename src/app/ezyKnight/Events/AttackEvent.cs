using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using ezyKnight.Hubs;
using ezyKnight.Models;

namespace ezyKnight.Events
{
    public class AttackEvent : IEvent
    {
        private readonly Player _player;
        private readonly Spell _spell;
        private readonly List<Values> _attackHere = new List<Values>();
        public AttackEvent(Player player, Spell spell)
        {
            _player = player;
            _spell = spell;
            _attackHere = _spell.GetHitArea(_player.X, _player.Y, _player.Orientation);
        }

        public bool ShouldExecute(DateTime tick)
        {
            return _attackHere.Any(x => tick > x.Delay);
        }

        public void Execute(DateTime tick)
        {

            if (_attackHere.All(x => x.Executed))
                Executed = true;

            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            foreach (var attackCord in _attackHere.Where(x => tick > x.Delay && !x.Executed))
            {
                attackCord.Executed = true;
                var enemy = World.GetPlayers().FirstOrDefault(x => x.X == attackCord.Value1 && x.Y == attackCord.Value2);
                if (enemy == null)
                {
                    context.Clients.Client(_player.ConnectionId).addChatMessage("You missed with " + _spell.Name);
                    return;
                }

                //TODO: Make some random shit here.
                enemy.Attacked(_spell.Damage.Value2);

            }

            context.Clients.Group("Players").joined(World.GetPlayers().ToArray());
        }

        public bool Executed { get; private set; }
        public DateTime ExecuteTime { get; private set; }
    }
}