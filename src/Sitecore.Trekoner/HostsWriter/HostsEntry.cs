using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Trekroner.HostsWriter
{
    public class HostsEntry
    {
        public string IpAddress { get; set; }
        public IEnumerable<string> Hosts { get; set; }
    }
}
