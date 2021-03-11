using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Moq;
using Xunit;
using AutoFixture.Xunit2;
using Sitecore.Trekroner.Hosts;
using System;

namespace Sitecore.Trekroner.Tests.HostsWriterTests
{
    public class HostsWriterTests
    {
        [Theory, AutoData]
        public async Task WriteHosts_AddsHostEntries(string fileName, string ipAddress, string[] hosts, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(true);
            var hostsWriter = new HostsWriter(filesystem.Object);
            var hostEntries = new HostsEntry[]
            {
                new HostsEntry
                {
                    IpAddress = ipAddress,
                    Hosts = hosts
                }
            };

            await hostsWriter.WriteHosts(fileName, hostEntries, sourceIdentifier);

            var expectedValue = $"{ipAddress}\t{string.Join(' ', hosts)}\t#{sourceIdentifier}";
            filesystem.Verify(f => f.File.AppendAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x =>
                    x.ToArray()[0] == expectedValue
                ),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Theory, AutoData]
        public async Task WriteHosts_AddsMultipleHostEntries(string fileName, HostsEntry[] entries, string sourceIdentifer)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(true);
            var hostsWriter = new HostsWriter(filesystem.Object);

            await hostsWriter.WriteHosts(fileName, entries, sourceIdentifer);

            filesystem.Verify(f => f.File.AppendAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x => x.Count() == entries.Length),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Theory, AutoData]
        public async Task WriteHosts_ThrowsIfFileDoesNotExist(string fileName, HostsEntry entry, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(false);
            var hostsWriter = new HostsWriter(filesystem.Object);

            Task result() => hostsWriter.WriteHosts(fileName, new[] { entry }, sourceIdentifier);

            var exception = await Assert.ThrowsAsync<HostsWriterException>(result);
            Assert.Equal(HostsWriterError.FileDoesNotExist, exception.HostsWriterError);
        }

        [Theory, AutoData]
        public async Task WriteHosts_CreatesAndDeletesBackup(string fileName, HostsEntry entry, string sourceIdentifier, string backupExtension)
        {
            await Test_CreatesAndDeletesBackup(
                fileName,
                backupExtension,
                (hostsWriter) => hostsWriter.WriteHosts(fileName, new[] { entry }, sourceIdentifier, backupExtension)
            );
        }

        [Theory, AutoData]
        public async Task WriteHosts_DoesNotDeleteBackupOnError(string fileName, HostsEntry entry, string sourceIdentifier, string backupExtension)
        {
            await Test_DoesNotDeleteBackupOnError(
                fileName,
                backupExtension,
                (hostsWriter) => hostsWriter.WriteHosts(fileName, new[] { entry }, sourceIdentifier, backupExtension)
            );
        }

        [Theory, AutoData]
        public async Task RemoveAll_RemovesSourceIdentifier(string fileName, string keepHost, string removeHost1, string removeHost2, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(true);
            filesystem.Setup(f => f.File.ReadAllLinesAsync(It.Is<string>(x => x == fileName), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new[]
            {
                $"127.0.0.1 {keepHost}",
                $"127.0.0.1 {removeHost1} #{sourceIdentifier}",
                $"127.0.0.1 {removeHost2} #{sourceIdentifier}"
            }));
            var hostsWriter = new HostsWriter(filesystem.Object);

            await hostsWriter.RemoveAll(fileName, sourceIdentifier);

            filesystem.Verify(f => f.File.WriteAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x => x.Single().EndsWith(keepHost)),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Theory, AutoData]
        public async Task RemoveAll_ThrowsIfFileDoesNotExist(string fileName, HostsEntry entry, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(false);
            var hostsWriter = new HostsWriter(filesystem.Object);

            Task result() => hostsWriter.RemoveAll(fileName,sourceIdentifier);

            var exception = await Assert.ThrowsAsync<HostsWriterException>(result);
            Assert.Equal(HostsWriterError.FileDoesNotExist, exception.HostsWriterError);
        }
        
        [Theory, AutoData]
        public async Task RemoveAll_CreatesAndDeletesBackup(string fileName, string sourceIdentifier, string backupExtension)
        {
            await Test_CreatesAndDeletesBackup(
                fileName,
                backupExtension,
                (hostsWriter) => hostsWriter.RemoveAll(fileName, sourceIdentifier, backupExtension)
            );
        }

        [Theory, AutoData]
        public async Task RemoveAll_DoesNotDeleteBackupOnError(string fileName, string sourceIdentifier, string backupExtension)
        {
            await Test_DoesNotDeleteBackupOnError(
                fileName,
                backupExtension,
                (hostsWriter) => hostsWriter.RemoveAll(fileName, sourceIdentifier, backupExtension)
            );
        }

        private async Task Test_CreatesAndDeletesBackup(string fileName, string backupExtension, Func<HostsWriter,Task> action)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(true);
            var hostsWriter = new HostsWriter(filesystem.Object);

            await action(hostsWriter);

            filesystem.Verify(f => f.File.Copy(
                It.Is<string>(x => x == fileName),
                It.Is<string>(x => x.EndsWith(backupExtension))
            ), Times.Once);
            filesystem.Verify(f => f.File.Delete(
                It.Is<string>(x => x.EndsWith(backupExtension))
            ), Times.Once);
        }

        private async Task Test_DoesNotDeleteBackupOnError(string fileName, string backupExtension, Func<HostsWriter, Task> action)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(true);
            // For Write
            filesystem
                .Setup(f => f.File.AppendAllLinesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            // For Remove
            filesystem
                .Setup(f => f.File.WriteAllLinesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            var hostsWriter = new HostsWriter(filesystem.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() => action(hostsWriter));
            filesystem.Verify(f => f.File.Copy(
                It.Is<string>(x => x == fileName),
                It.Is<string>(x => x.EndsWith(backupExtension))
            ), Times.Once);
            filesystem.Verify(f => f.File.Delete(
                It.Is<string>(x => x.EndsWith(backupExtension))
            ), Times.Never);
        }

    }
}
