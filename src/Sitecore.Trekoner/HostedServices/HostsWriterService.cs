using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.ReverseProxy.Service;
using Sitecore.Trekroner.Hosts;
using Sitecore.Trekroner.Net;

namespace Sitecore.Trekroner.HostedServices
{
    public class HostsWriterService : IHostedService
    {
        private readonly IProxyConfigProvider YarpConfigurationProvider;
        private readonly IHostsWriter HostsWriter;
        private readonly IIpAddressResolver IpResolver;

        public HostsWriterService(
            IProxyConfigProvider yarpConfigurationProvider,
            IHostsWriter hostsWriter,
            IIpAddressResolver ipResolver)
        {
            YarpConfigurationProvider = yarpConfigurationProvider;
            HostsWriter = hostsWriter;
            IpResolver = ipResolver;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var currentIp = IpResolver.GetCurrentIp().ToString();
            var hostsEntries = YarpConfigurationProvider.GetConfig().Routes.Select(x => new HostsEntry
            {
                IpAddress = currentIp,
                Hosts = x.Match.Hosts
            });
            Console.WriteLine($"Adding hosts entries for {currentIp}");
            // remove existing in case of unclean shutdown
            await HostsWriter.RemoveAll();
            await HostsWriter.WriteHosts(hostsEntries);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await HostsWriter.RemoveAll();
        }
    }
}
