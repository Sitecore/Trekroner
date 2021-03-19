using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.Trekroner.Hosts
{
    public class HostsWriterConfiguration
    {
        public static readonly string Key = "HostsWriter";

        public string FilePath { get; init; } = "c:\\diversetc\\hosts";
        public string SourceIdentifier { get; init; } = nameof(HostsWriter);
        public string BackupExtension { get; init; }
    }
}
