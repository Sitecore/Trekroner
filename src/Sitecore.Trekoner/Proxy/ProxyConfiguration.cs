using System.Collections.Generic;

namespace Sitecore.Trekroner.Proxy
{
    public class ProxyConfiguration
    {
        public static readonly string Key = "Proxy";

        public string DefaultDomain { get; set; }
        public IDictionary<string,ServiceConfiguration> Services { get; set; }
    }
}
