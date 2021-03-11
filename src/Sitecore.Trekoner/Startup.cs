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

            var routes = new[]
            {
                new ProxyRoute()
                {
                    RouteId = "route1",
                    ClusterId = "cluster1",
                    Match = new ProxyMatch
                    {
                        Path = "{**catch-all}"
                    }
                }
            };
            var clusters = new[]
            {
                new Cluster()
                {
                    Id = "cluster1",
                    Destinations = new Dictionary<string, Destination>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "destination1", new Destination() { Address = "http://sample/" } }
                    }
                }
            };

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
