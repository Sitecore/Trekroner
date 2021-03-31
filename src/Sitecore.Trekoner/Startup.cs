using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;
using Sitecore.Trekroner.Hosts;
using Sitecore.Trekroner.Proxy;
using Sitecore.Trekroner.HostedServices;
using Sitecore.Trekroner.Net;
using Microsoft.ReverseProxy.Abstractions.Config;
using Sitecore.Trekroner.Protos;

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
                .LoadFromMemory(routes, clusters)
                .AddTransforms(builderContext =>
                {
                    builderContext.AddXForwarded();
                    builderContext.UseOriginalHost = true;
                });

            services.AddRazorPages();
            services.AddGrpc();
            services.AddGrpcReflection();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHsts();
            

            var proxyConfiguration = Configuration.GetSection(ProxyConfiguration.Key).Get<ProxyConfiguration>();
            app.MapWhen(x => x.Request.Host.Host.Equals(proxyConfiguration.DefaultDomain), statusApp =>
            {
                statusApp.UseRouting();
                statusApp.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapGrpcService<ContainerOperationsService>();
                    endpoints.MapGrpcReflectionService();
                });
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
            });
        }
    }
}
