using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Trekroner.Hosts;

namespace Sitecore.Trekroner.Hosts
{
    public interface IHostsWriter
    {
        Task WriteHosts(IEnumerable<HostsEntry> hosts);
        Task RemoveAll();
    }
}
