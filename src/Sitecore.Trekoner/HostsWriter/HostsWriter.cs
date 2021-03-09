using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Sitecore.Trekroner.HostsWriter
{
    public class HostsWriter : IHostsWriter
    {
        private readonly IFileSystem FileSystem;

        public HostsWriter(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public async void WriteHosts(string filePath, IEnumerable<HostsEntry> entries, string sourceIdentifier)
        {
            var hostLines = entries.Select(entry => $"{entry.IpAddress}\t{string.Join(' ', entry.Hosts)}\t#{sourceIdentifier}");
            await FileSystem.File.AppendAllLinesAsync(filePath, hostLines);
        }

        public async void RemoveAll(string filePath, string sourceIdentifier)
        {
            IEnumerable<string> hostLines = await FileSystem.File.ReadAllLinesAsync(filePath);
            hostLines = hostLines.Where(line => !line.EndsWith($"#{sourceIdentifier}"));
            await FileSystem.File.WriteAllLinesAsync(filePath, hostLines);
        }
    }
}
