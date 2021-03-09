using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Trekroner.Hosts;

namespace Sitecore.Trekroner.Hosts
{
    interface IHostsWriter
    {
        Task WriteHosts(string filePath, IEnumerable<HostsEntry> hosts, string sourceIdentifier);
        Task RemoveAll(string filePath, string sourceIdentifier);
    }
}
