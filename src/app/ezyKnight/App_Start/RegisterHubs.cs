using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
[assembly: PreApplicationStartMethod(typeof(ezyKnight.RegisterHubs), "Start")]
namespace ezyKnight
{
    public static class RegisterHubs
    {
        public static void Start()
        {
            // Register the default hubs route: ~/signalr/hubs
            RouteTable.Routes.MapHubs();
        }
    }
}