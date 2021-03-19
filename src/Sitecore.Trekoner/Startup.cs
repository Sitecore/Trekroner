using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;
using Microsoft.ReverseProxy.Abstractions;
using System.Net;
using Sitecore.Trekroner.Hosts;
using System.Threading;
using Sitecore.Trekroner.Services;
using Sitecore.Trekroner.Proxy;

namespace Sitecore.Trekroner
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFileSystem, FileSystem>();
            services.AddScoped<IServiceConfigurationProvider, ServiceConfigurationProvider>();
            services.AddScoped<HostsWriter, HostsWriter>();
            services.AddHostedService<HostsWriterService>();

            var proxyConfiguration = Configuration.GetSection(ProxyConfiguration.Key).Get<ProxyConfiguration>();
            var serviceConfiguration = new ServiceConfigurationProvider().GetConfiguration();

            var routes = serviceConfiguration.Services.Select(x => new ProxyRoute()
            {
                RouteId = $"route-{x.Name}",
                ClusterId = $"cluster-{x.Name}",
                Match = new ProxyMatch
                {
                    Hosts = new[] { $"{x.Name}.{proxyConfiguration.Domain}" }
                }
            }).ToArray();
            var clusters = serviceConfiguration.Services.Select(x => new Cluster()
            {
                Id = $"cluster-{x.Name}",
                Destinations = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase)
                {
                    { $"destination-{x.Name}", new Destination() { Address = x.InternalUrl.ToString() } }
                }
            }).ToArray();

            services.AddReverseProxy()
                .LoadFromMemory(routes, clusters);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
            });
        }

        private class HostsWriterService : IHostedService
        {
            private readonly HostsWriterConfiguration WriterConfiguration;
            private readonly ProxyConfiguration ProxyConfiguration;
            private readonly ServiceConfiguration ServiceConfiguration;
            private readonly HostsWriter HostsWriter;

            public HostsWriterService(IConfiguration configuration, IServiceConfigurationProvider serviceConfigurationProvider, HostsWriter hostsWriter)
            {
                WriterConfiguration = configuration.GetSection(HostsWriterConfiguration.Key).Get<HostsWriterConfiguration>();
                ProxyConfiguration = configuration.GetSection(ProxyConfiguration.Key).Get<ProxyConfiguration>();
                ServiceConfiguration = serviceConfigurationProvider.GetConfiguration();
                HostsWriter = hostsWriter;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {

                var currentIp = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToString();
                var hostsEntries = ServiceConfiguration.Services.Select(x => new HostsEntry
                {
                    IpAddress = currentIp,
                    Hosts = new[] { $"{x.Name}.{ProxyConfiguration.Domain}" }
                });
                Console.WriteLine($"Adding hosts entries for {currentIp}");
                await HostsWriter.WriteHosts(hostsEntries, WriterConfiguration);
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                await HostsWriter.RemoveAll(WriterConfiguration);
            }
        }
    }
}
