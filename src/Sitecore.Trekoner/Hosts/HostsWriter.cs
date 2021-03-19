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

        public async Task WriteHosts(IEnumerable<HostsEntry> entries, HostsWriterConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentException($"{nameof(config)} cannot be null");
            }

            if (!FileSystem.File.Exists(config.FilePath))
            {
                throw new HostsWriterException($"Cannot write hosts to {config.FilePath}: File does not exist")
                {
                    HostsWriterError = HostsWriterError.FileDoesNotExist
                };
            }

            var hostLines = new List<string>();
            hostLines.Add(Environment.NewLine);
            hostLines.AddRange(
                entries.Select(entry => $"{entry.IpAddress}\t{string.Join(' ', entry.Hosts)}\t#{config.SourceIdentifier}")
            );

            await WithBackup(config.FilePath, config.BackupExtension,
                () => FileSystem.File.AppendAllLinesAsync(config.FilePath, hostLines)
            );
        }

        public async Task RemoveAll(HostsWriterConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentException($"{nameof(config)} cannot be null");
            }

            if (!FileSystem.File.Exists(config.FilePath))
            {
                throw new HostsWriterException($"Cannot remove hosts from {config.FilePath}: File does not exist")
                {
                    HostsWriterError = HostsWriterError.FileDoesNotExist
                };
            }

            IEnumerable<string> hostLines = await FileSystem.File.ReadAllLinesAsync(config.FilePath);
            hostLines = hostLines.Where(line => !line.EndsWith($"#{config.SourceIdentifier}"));

            await WithBackup(config.FilePath, config.BackupExtension,
                () => FileSystem.File.WriteAllLinesAsync(config.FilePath, hostLines)
            );
        }

        private async Task WithBackup(string filePath, string backupExtension, Func<Task> action)
        {
            var backupFile = !string.IsNullOrEmpty(backupExtension) ? filePath + backupExtension : null;
            if (backupFile != null)
            {
                FileSystem.File.Copy(filePath, backupFile);
            }

            await action();

            // no error handling  -- we don't want to execute this if the file write fails
            if (backupFile != null)
            {
                FileSystem.File.Delete(backupFile);
            }
        }
    }
}
