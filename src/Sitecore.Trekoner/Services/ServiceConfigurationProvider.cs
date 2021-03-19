using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Trekroner.Services
{
    public class ServiceConfigurationProvider : IServiceConfigurationProvider
    {
        public ServiceConfiguration GetConfiguration()
        {
            return new ServiceConfiguration
            {
                Services = new[]
                {
                    new Service
                    {
                        Name = "cm",
                        InternalUrl = new Uri("http://cm/")
                    },
                    new Service
                    {
                        Name = "id",
                        InternalUrl = new Uri("http://id/")
                    },
                    new Service
                    {
                        Name = "xconnect",
                        InternalUrl = new Uri("http://xconnect/")
                    },
                }
            };
        }
    }
}
