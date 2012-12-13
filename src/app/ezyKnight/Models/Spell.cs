using System.Collections.Generic;

namespace ezyKnight.Models
{
    public abstract class Spell
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Range { get; set; }
        public int Speed { get; set; }
        public Values Damage { get; set; }
        public abstract List<Values> GetHitArea(int x, int y, Orientation orientation);
    }

    public class Melee : Spell
    {
        public override List<Values> GetHitArea(int x, int y, Orientation orientation)
        {
            var values = new List<Values>();
            switch (orientation)
            {
                case Orientation.Right:
                    values.Add(new Values(x + this.Range, y));
                    break;
                case Orientation.Left:
                    values.Add(new Values(x - this.Range, y));
                    break;
                case Orientation.Up:
                    values.Add(new Values(x, y - this.Range));
                    break;
                case Orientation.Down:
                    values.Add(new Values(x, y + this.Range));
                    break;
                default:
                    break;
            }
            return values;
        }
    }
}