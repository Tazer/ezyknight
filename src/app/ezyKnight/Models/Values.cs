using System;

namespace ezyKnight.Models
{
    public class Values
    {
        public Values()
        {
            Delay = DateTime.Now;
        }

        public Values(int value1, int value2)
        {
            Value1 = value1;
            Value2 = value2;
            Delay = DateTime.Now;
        }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public DateTime Delay { get; set; }
        public bool Executed { get; set; }
    }
}