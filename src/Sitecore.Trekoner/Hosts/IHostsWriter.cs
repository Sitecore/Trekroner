using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Trekroner.Hosts;

namespace Sitecore.Trekroner.Hosts
{
    interface IHostsWriter
    {
        Task WriteHosts(IEnumerable<HostsEntry> hosts, HostsWriterConfiguration config);
        Task RemoveAll(HostsWriterConfiguration config);
    }
}
