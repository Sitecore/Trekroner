using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;
using Microsoft.ReverseProxy.Abstractions;
using System.Net;
using Sitecore.Trekroner.Hosts;
using System.Threading;
using Sitecore.Trekroner.Proxy;
using Microsoft.ReverseProxy.Service;
using Sitecore.Trekroner.Services;
using Sitecore.Trekroner.Net;

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
            services.AddScoped<IHostsWriter, HostsWriter>();
            services.AddScoped<IIpAddressResolver, IpAddressResolver>();
            services.AddHostedService<HostsWriterService>();

            var proxyConfiguration = Configuration.GetSection(ProxyConfiguration.Key).Get<ProxyConfiguration>();
            var yarpBuilder = new YarpConfigurationBuilder();
            var routes = yarpBuilder.GetRoutes(proxyConfiguration);
            var clusters = yarpBuilder.GetClusters(proxyConfiguration);

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
    }
}
