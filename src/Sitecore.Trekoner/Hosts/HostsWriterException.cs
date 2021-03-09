using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Trekroner.Hosts
{
    public class HostsWriterException : Exception
    {
        public HostsWriterException(string message) : base(message)
        {
        }

        public HostsWriterError HostsWriterError { get; init; }
    }

    public enum HostsWriterError
    {
        FileDoesNotExist
    }
}
