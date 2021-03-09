using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Trekroner.HostsWriter
{
    interface IHostsWriter
    {
        void WriteHosts(string filePath, IEnumerable<HostsEntry> hosts, string sourceIdentifier);
        void RemoveAll(string filePath, string sourceIdentifier);
    }
}
