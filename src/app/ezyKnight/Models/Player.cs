using System.Collections.Generic;

namespace ezyKnight.Models
{
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
        public bool IsMoving = false;
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

        public void Attacked(int damage)
        {
            Health.Value1 -= damage;

            if (Health.Value1 <= 0)
                IsDead = true;

        }
    }
}