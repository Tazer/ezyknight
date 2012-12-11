using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using ezyKnight.Hubs;

namespace ezyKnight.Tests
{
    [TestFixture]
    public class PlayerTest
    {
        [Test]
        public void CheckOrintationCorrect()
        {
            var player = new Player();
            player.X = 0;
            player.Y = 10;

            player.MoveTo(10,10);

            Assert.AreEqual(Orientation.Right,player.Orientation);

            player.MoveTo(0,10);

            Assert.AreEqual(Orientation.Left, player.Orientation);

            player.MoveTo(0,0);

            Assert.AreEqual(Orientation.Up, player.Orientation);

            player.MoveTo(0,10);

            Assert.AreEqual(Orientation.Down, player.Orientation);
        }
    }
}