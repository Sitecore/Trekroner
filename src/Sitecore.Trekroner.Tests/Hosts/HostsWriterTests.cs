using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Moq;
using Xunit;
using AutoFixture.Xunit2;
using Sitecore.Trekroner.Hosts;

namespace Sitecore.Trekroner.Tests.HostsWriterTests
{
    public class HostsWriterTests
    {
        [Theory, AutoData]
        public async Task WriteHosts_AddsHostEntries(string fileName, string ipAddress, string[] hosts, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.AppendAllLinesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Verifiable();
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
            ));
        }

        [Theory, AutoData]
        public async Task WriteHosts_AddsMultipleHostEntries(string fileName, HostsEntry[] entries, string sourceIdentifer)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.Exists(It.Is<string>(x => x == fileName))).Returns(true);
            filesystem.Setup(f => f.File.AppendAllLinesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Verifiable();
            var hostsWriter = new HostsWriter(filesystem.Object);

            await hostsWriter.WriteHosts(fileName, entries, sourceIdentifer);

            filesystem.Verify(f => f.File.AppendAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x => x.Count() == entries.Length),
                It.IsAny<CancellationToken>()
            ));
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

        // TODO: file backup tests
        // TODO: file locked tests

        [Theory, AutoData]
        public async Task RemoveAll_RemovesSourceIdentifier(string fileName, string keepHost, string removeHost1, string removeHost2, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.ReadAllLinesAsync(It.Is<string>(x => x == fileName), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new[]
            {
                $"127.0.0.1 {keepHost}",
                $"127.0.0.1 {removeHost1} #{sourceIdentifier}",
                $"127.0.0.1 {removeHost2} #{sourceIdentifier}"
            }));
            filesystem.Setup(f => f.File.WriteAllLinesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Verifiable();
            var hostsWriter = new HostsWriter(filesystem.Object);

            await hostsWriter.RemoveAll(fileName, sourceIdentifier);

            filesystem.Verify(f => f.File.WriteAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x => x.Single().EndsWith(keepHost)),
                It.IsAny<CancellationToken>()
            ));
        }

        // TODO: RemoveAll if file doesn't exist
        // TODO: File backup tests
        // TODO: File locked tests

    }
}
