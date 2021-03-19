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
using Sitecore.Trekroner.Configuration;
using System.Net;
using Sitecore.Trekroner.Hosts;
using System.Threading;

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
            services.AddScoped<HostsWriter, HostsWriter>();
            services.AddHostedService<HostsWriterService>();

            var configuration = new TrekronerProxyConfiguration
            {
                Domain = "trekroner.test",
                Services = new[]
                {
                    "cm",
                    "id",
                    "xconnect"
                }
            };
            services.AddSingleton(configuration);

            var routes = configuration.Services.Select(x => new ProxyRoute()
            {
                RouteId = $"route-{x}",
                ClusterId = $"cluster-{x}",
                Match = new ProxyMatch
                {
                    Hosts = new[] { $"{x}.{configuration.Domain}" }
                }
            }).ToArray();
            var clusters = configuration.Services.Select(x => new Cluster()
            {
                Id = $"cluster-{x}",
                Destinations = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase)
                {
                    { $"destination-{x}", new Destination() { Address = $"http://{x}/" } }
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
            private readonly TrekronerProxyConfiguration ProxyConfiguration;
            private readonly HostsWriter HostsWriter;

            public HostsWriterService(TrekronerProxyConfiguration proxyConfiguration, HostsWriter hostsWriter)
            {
                ProxyConfiguration = proxyConfiguration;
                HostsWriter = hostsWriter;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                var currentIp = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToString();
                var hostsEntries = ProxyConfiguration.Services.Select(x => new HostsEntry
                {
                    IpAddress = currentIp,
                    Hosts = new[] { $"{x}.{ProxyConfiguration.Domain}" }
                });
                Console.WriteLine($"Adding hosts entries for {currentIp}");
                await HostsWriter.WriteHosts("c:\\driversetc\\hosts", hostsEntries, "trekroner", ".trekroner.bak");
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                await HostsWriter.RemoveAll("c:\\driversetc\\hosts", "trekroner", ".trekroner.bak");
            }
        }
    }
}
