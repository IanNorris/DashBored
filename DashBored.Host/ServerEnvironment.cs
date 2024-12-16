using DashBored.Host.Models;
using DashBored.PluginApi;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace DashBored.Host
{
    public class ServerEnvironment : IPluginServerEnvironment
    {
        public ServerEnvironment(IServer server)
        {
            _server = server;
        }

        public string GetListenAddress()
        {
            var listenAddress = _server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault(a => a.StartsWith("https://"));

            if (listenAddress == null)
            {
                listenAddress = _server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault(a => a.StartsWith("http://"));
            }

            if (!listenAddress.EndsWith('/'))
            {
                listenAddress += "/";
            }

            listenAddress = listenAddress.Replace("[::]", "localhost");

            return listenAddress;
        }

        private IServer _server;
    }
}
