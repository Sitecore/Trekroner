using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Sitecore.Trekroner.Hosts
{
    public class HostsWriter : IHostsWriter
    {
        private readonly IFileSystem FileSystem;
        private readonly HostsWriterConfiguration Configuration;

        public HostsWriter(IFileSystem fileSystem, IConfiguration configuration)
        {
            FileSystem = fileSystem;
            Configuration = configuration.GetSection(HostsWriterConfiguration.Key).Get<HostsWriterConfiguration>()
                ?? new HostsWriterConfiguration();
        }

        public async Task WriteHosts(IEnumerable<HostsEntry> entries)
        {
            if (!FileSystem.File.Exists(Configuration.FilePath))
            {
                throw new HostsWriterException($"Cannot write hosts to {Configuration.FilePath}: File does not exist")
                {
                    HostsWriterError = HostsWriterError.FileDoesNotExist
                };
            }

            var existingText = await FileSystem.File.ReadAllTextAsync(Configuration.FilePath);
            var addNewLine = existingText.Length > 0 && !existingText.EndsWith(Environment.NewLine);

            var hostLines = new List<string>();
            if (addNewLine)
            {
                hostLines.Add(Environment.NewLine);
            }
            hostLines.AddRange(
                entries.Select(entry => $"{entry.IpAddress}\t{string.Join(' ', entry.Hosts)}\t#{Configuration.SourceIdentifier}")
            );

            await WithBackup(Configuration.FilePath, Configuration.BackupExtension,
                () => FileSystem.File.AppendAllLinesAsync(Configuration.FilePath, hostLines)
            );
        }

        public async Task RemoveAll()
        {
            if (!FileSystem.File.Exists(Configuration.FilePath))
            {
                throw new HostsWriterException($"Cannot remove hosts from {Configuration.FilePath}: File does not exist")
                {
                    HostsWriterError = HostsWriterError.FileDoesNotExist
                };
            }

            IEnumerable<string> hostLines = await FileSystem.File.ReadAllLinesAsync(Configuration.FilePath);
            hostLines = hostLines.Where(line => !line.EndsWith($"#{Configuration.SourceIdentifier}"));

            await WithBackup(Configuration.FilePath, Configuration.BackupExtension,
                () => FileSystem.File.WriteAllLinesAsync(Configuration.FilePath, hostLines)
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
