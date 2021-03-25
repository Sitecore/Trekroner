using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Xunit2;
using Sitecore.Trekroner.Proxy;
using Xunit;

namespace Sitecore.Trekroner.Tests.Proxy
{
    public class YarpConfigurationBuilderTests
    {
        [Theory, AutoData]
        public void GetRoutes_CreatesRouteForEachService(Dictionary<string, ServiceConfiguration> services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);

            Assert.Equal(services.Count, routes.Count);
        }

        [Theory, AutoData]
        public void GetRoutes_CreatesUniqueRouteIds(Dictionary<string, ServiceConfiguration> services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);

            Assert.Equal(routes.Count, routes.Select(x => x.RouteId).Distinct().Count());
        }

        [Theory, AutoData]
        public void GetRoutes_SetsHostWithDefaultDomain(string domain, ServiceConfiguration service)
        {
            var proxyConfiguration = new ProxyConfiguration
            {
                DefaultDomain = domain,
                Services = new Dictionary<string,ServiceConfiguration> { { service.Name, service } }
            };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);

            Assert.Equal($"{service.Name}.{domain}", routes.FirstOrDefault().Match.Hosts.FirstOrDefault());
        }

        [Theory, AutoData]
        public void GetClusters_CreatesClusterForEveryService(Dictionary<string, ServiceConfiguration> services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(services.Count, clusters.Count);
        }

        [Theory, AutoData]
        public void GetClusters_CreatesUniqueClusterIds(Dictionary<string, ServiceConfiguration> services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(clusters.Count, clusters.Select(x => x.Id).Distinct().Count());
        }

        [Theory, AutoData]
        public void GetClusters_CreatesUniqueDestinationKeys(Dictionary<string, ServiceConfiguration> services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);
            var destinations = clusters.SelectMany(x => x.Destinations);

            Assert.Equal(destinations.Count(), destinations.Select(x => x.Key).Distinct().Count());
        }

        [Fact]
        public void GetClusters_CreatesDestinationUrlWithServiceName()
        {
            var fixture = new Fixture();
            var service = fixture.Build<ServiceConfiguration>().Without(x => x.TargetPort).Create();
            var proxyConfiguration = new ProxyConfiguration { Services = new Dictionary<string, ServiceConfiguration> { { service.Name, service } } };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(
                $"http://{service.Name}/",
                clusters.FirstOrDefault().Destinations.FirstOrDefault().Value.Address
            );
        }

        [Theory, AutoData]
        public void GetClusters_CreatesDestinationUrlWithServiceNameAndPort(ServiceConfiguration service)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = new Dictionary<string, ServiceConfiguration> { { service.Name, service } } };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(
                $"http://{service.Name}:{service.TargetPort}/",
                clusters.FirstOrDefault().Destinations.FirstOrDefault().Value.Address
            );
        }

        [Theory, AutoData]
        public void GetRoutes_GetClusters_MatchClusterIds(ServiceConfiguration service)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = new Dictionary<string, ServiceConfiguration> { { service.Name, service } } };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);
            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(routes.FirstOrDefault().ClusterId, clusters.FirstOrDefault().Id);
        }
    }
}
