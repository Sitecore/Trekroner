using System.Net;

namespace Sitecore.Trekroner.Net
{
    public interface IIpAddressResolver
    {
        IPAddress GetCurrentIp();
    }
}
