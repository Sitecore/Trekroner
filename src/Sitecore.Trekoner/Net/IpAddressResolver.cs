using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sitecore.Trekroner.Net
{
    public class IpAddressResolver : IIpAddressResolver
    {
        public IPAddress GetCurrentIp()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
