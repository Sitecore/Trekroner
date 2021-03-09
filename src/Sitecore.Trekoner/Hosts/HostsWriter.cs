using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Trekroner.Hosts;

namespace Sitecore.Trekroner.Hosts
{
    public class HostsWriter : IHostsWriter
    {
        private readonly IFileSystem FileSystem;

        public HostsWriter(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public async Task WriteHosts(string filePath, IEnumerable<HostsEntry> entries, string sourceIdentifier)
        {
            if (!FileSystem.File.Exists(filePath))
            {
                throw new HostsWriterException($"Cannot write hosts to {filePath}: File does not exist")
                {
                    HostsWriterError = HostsWriterError.FileDoesNotExist
                };
            }
            var hostLines = entries.Select(entry => $"{entry.IpAddress}\t{string.Join(' ', entry.Hosts)}\t#{sourceIdentifier}");
            await FileSystem.File.AppendAllLinesAsync(filePath, hostLines);
        }

        public async Task RemoveAll(string filePath, string sourceIdentifier)
        {
            IEnumerable<string> hostLines = await FileSystem.File.ReadAllLinesAsync(filePath);
            hostLines = hostLines.Where(line => !line.EndsWith($"#{sourceIdentifier}"));
            await FileSystem.File.WriteAllLinesAsync(filePath, hostLines);
        }
    }
}
