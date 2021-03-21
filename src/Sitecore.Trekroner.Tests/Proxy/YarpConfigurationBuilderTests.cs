using System.Linq;
using AutoFixture.Xunit2;
using Sitecore.Trekroner.Proxy;
using Xunit;

namespace Sitecore.Trekroner.Tests.Proxy
{
    public class YarpConfigurationBuilderTests
    {
        [Theory, AutoData]
        public void GetRoutes_CreatesRouteForEachService(ServiceConfiguration[] services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);

            Assert.Equal(services.Length, routes.Count);
        }

        [Theory, AutoData]
        public void GetRoutes_CreatesUniqueRouteIds(ServiceConfiguration[] services)
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
                Services = new[] { service }
            };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);

            Assert.Equal($"{service.Name}.{domain}", routes.FirstOrDefault().Match.Hosts.FirstOrDefault());
        }

        [Theory, AutoData]
        public void GetClusters_CreatesClusterForEveryService(ServiceConfiguration[] services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(services.Length, clusters.Count);
        }

        [Theory, AutoData]
        public void GetClusters_CreatesUniqueClusterIds(ServiceConfiguration[] services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(clusters.Count, clusters.Select(x => x.Id).Distinct().Count());
        }

        [Theory, AutoData]
        public void GetClusters_CreatesUniqueDestinationKeys(ServiceConfiguration[] services)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = services };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);
            var destinations = clusters.SelectMany(x => x.Destinations);

            Assert.Equal(destinations.Count(), destinations.Select(x => x.Key).Distinct().Count());
        }

        [Theory, AutoData]
        public void GetClusters_CreatesDestinationUrlWithServiceName(ServiceConfiguration service)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = new[] { service } };
            var builder = new YarpConfigurationBuilder();

            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(
                $"http://{service.Name}",
                clusters.FirstOrDefault().Destinations.FirstOrDefault().Value.Address
            );
        }

        [Theory, AutoData]
        public void GetRoutes_GetClusters_MatchClusterIds(ServiceConfiguration service)
        {
            var proxyConfiguration = new ProxyConfiguration { Services = new[] { service } };
            var builder = new YarpConfigurationBuilder();

            var routes = builder.GetRoutes(proxyConfiguration);
            var clusters = builder.GetClusters(proxyConfiguration);

            Assert.Equal(routes.FirstOrDefault().ClusterId, clusters.FirstOrDefault().Id);
        }
    }
}
