using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ReverseProxy.Abstractions;

namespace Sitecore.Trekroner.Proxy
{
    public class YarpConfigurationBuilder
    {
        public IReadOnlyList<ProxyRoute> GetRoutes(ProxyConfiguration proxyConfig)
        {
            return proxyConfig?.Services?.Values.Select(x => new ProxyRoute()
            {
                RouteId = $"route-{x.Name}",
                ClusterId = $"cluster-{x.Name}",
                Match = new ProxyMatch
                {
                    Hosts = new[] { $"{x.Name}.{proxyConfig.DefaultDomain}" }
                }
            }).ToArray();
        }

        public IReadOnlyList<Cluster> GetClusters(ProxyConfiguration proxyConfig)
        {
            return proxyConfig?.Services?.Values.Select(x => new Cluster()
            {
                Id = $"cluster-{x.Name}",
                Destinations = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase)
                {
                    { $"destination-{x.Name}", new Destination() { Address = GetDestination(x) } }
                }
            }).ToArray();
        }

        private string GetDestination(ServiceConfiguration service)
        {
            var builder = new UriBuilder("http", service.Name);
            if (service.TargetPort.HasValue)
            {
                builder.Port = service.TargetPort.Value;
            }
            Console.WriteLine($"Added destination {builder}");
            return builder.ToString();
        }
    }
}
