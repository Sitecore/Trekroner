using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Microsoft.ReverseProxy.Abstractions;
using Microsoft.ReverseProxy.Service;
using Moq;
using Sitecore.Trekroner.Hosts;
using Sitecore.Trekroner.Net;
using Sitecore.Trekroner.Services;
using Xunit;

namespace Sitecore.Trekroner.Tests.Services
{
    public class HostsWriterServiceTests
    {
        [Theory, AutoData]
        public async Task StartAsync_WritesCurrentIp(IPAddress ip, string host)
        {
            var ipResolver = new Mock<IIpAddressResolver>();
            ipResolver.Setup(x => x.GetCurrentIp()).Returns(ip);
            var proxyConfigProvider = GetProxyConfigProvider(new[] {
                new ProxyRoute
                {
                    Match = new ProxyMatch
                    {
                        Hosts = new[] { host }
                    }
                }
            });
            var hostsWriter = new Mock<IHostsWriter>();
            var hostsWriterService = new HostsWriterService(proxyConfigProvider.Object, hostsWriter.Object, ipResolver.Object);

            await hostsWriterService.StartAsync(CancellationToken.None);

            hostsWriter.Verify(x => x.WriteHosts(
                It.Is<IEnumerable<HostsEntry>>(y => y.Single().IpAddress == ip.ToString())), Times.Once);
        }

        [Theory, AutoData]
        public async Task StartAsync_AddsAllRoutes(IPAddress ip, string host1, string host2)
        {
            var ipResolver = new Mock<IIpAddressResolver>();
            ipResolver.Setup(x => x.GetCurrentIp()).Returns(ip);
            var proxyConfigProvider = GetProxyConfigProvider(new[] {
                new ProxyRoute
                {
                    Match = new ProxyMatch
                    {
                        Hosts = new[] { host1 }
                    }
                },
                new ProxyRoute
                {
                    Match = new ProxyMatch
                    {
                        Hosts = new[] { host2 }
                    }
                }
            });
            var hostsWriter = new Mock<IHostsWriter>();
            var hostsWriterService = new HostsWriterService(proxyConfigProvider.Object, hostsWriter.Object, ipResolver.Object);

            await hostsWriterService.StartAsync(CancellationToken.None);

            hostsWriter.Verify(x => x.WriteHosts(
                It.Is<IEnumerable<HostsEntry>>(
                    y => y.Count(z => z.Hosts.Single() == host1) == 1
                        && y.Count(z => z.Hosts.Single() == host2) == 1
                )), Times.Once);
        }

        [Theory, AutoData]
        public async Task StartAsync_AddsAllHosts(IPAddress ip, string host1, string host2)
        {
            var ipResolver = new Mock<IIpAddressResolver>();
            ipResolver.Setup(x => x.GetCurrentIp()).Returns(ip);
            var proxyConfigProvider = GetProxyConfigProvider(new[] {
                new ProxyRoute
                {
                    Match = new ProxyMatch
                    {
                        Hosts = new[] { host1, host2 }
                    }
                }
            });
            var hostsWriter = new Mock<IHostsWriter>();
            var hostsWriterService = new HostsWriterService(proxyConfigProvider.Object, hostsWriter.Object, ipResolver.Object);

            await hostsWriterService.StartAsync(CancellationToken.None);

            hostsWriter.Verify(x => x.WriteHosts(
                It.Is<IEnumerable<HostsEntry>>(
                    y => y.Single().Hosts.Count(z => z == host1) == 1
                        && y.Single().Hosts.Count(z => z == host2) == 1
                )), Times.Once);
        }

        [Theory, AutoData]
        public async Task StartAsync_RemovesBeforeWrites(IPAddress ip, string host)
        {
            var ipResolver = new Mock<IIpAddressResolver>();
            ipResolver.Setup(x => x.GetCurrentIp()).Returns(ip);
            var proxyConfigProvider = GetProxyConfigProvider(new[] {
                new ProxyRoute
                {
                    Match = new ProxyMatch
                    {
                        Hosts = new[] { host }
                    }
                }
            });
            var removeCalled = false;
            var writeCalledAfter = false;
            var hostsWriter = new Mock<IHostsWriter>();
            hostsWriter.Setup(x => x.RemoveAll()).Callback(() => removeCalled = true);
            hostsWriter.Setup(x => x.WriteHosts(It.IsAny<IEnumerable<HostsEntry>>())).Callback(() => writeCalledAfter = removeCalled);
            var hostsWriterService = new HostsWriterService(proxyConfigProvider.Object, hostsWriter.Object, ipResolver.Object);

            await hostsWriterService.StartAsync(CancellationToken.None);

            Assert.True(writeCalledAfter);
        }

        [Theory, AutoData]
        public async Task StopAsync_Removes(IPAddress ip, string host)
        {
            var ipResolver = new Mock<IIpAddressResolver>();
            ipResolver.Setup(x => x.GetCurrentIp()).Returns(ip);
            var proxyConfigProvider = GetProxyConfigProvider(new[] {
                new ProxyRoute
                {
                    Match = new ProxyMatch
                    {
                        Hosts = new[] { host }
                    }
                }
            });
            var hostsWriter = new Mock<IHostsWriter>();
            var hostsWriterService = new HostsWriterService(proxyConfigProvider.Object, hostsWriter.Object, ipResolver.Object);

            await hostsWriterService.StopAsync(CancellationToken.None);

            hostsWriter.Verify(x => x.RemoveAll(), Times.Once);
        }

        private Mock<IProxyConfigProvider> GetProxyConfigProvider(ProxyRoute[] routes)
        {
            var proxyConfig = new Mock<IProxyConfig>();
            proxyConfig.Setup(x => x.Routes).Returns(routes);
            var proxyConfigProvider = new Mock<IProxyConfigProvider>();
            proxyConfigProvider.Setup(x => x.GetConfig()).Returns(proxyConfig.Object);
            return proxyConfigProvider;
        }
    }
}
