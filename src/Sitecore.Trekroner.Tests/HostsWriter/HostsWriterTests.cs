using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Moq;
using Xunit;
using Target = Sitecore.Trekroner.HostsWriter.HostsWriter;
using HostsEntry = Sitecore.Trekroner.HostsWriter.HostsEntry;
using AutoFixture.Xunit2;

namespace Sitecore.Trekroner.Tests.HostsWriter
{
    public class HostsWriterTests
    {
        [Theory, AutoData]
        public void WriteHosts_AddsHostEntries(string fileName, string ipAddress, string[] hosts, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.AppendAllLinesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Verifiable();
            var hostsWriter = new Target(filesystem.Object);
            var hostEntries = new HostsEntry[]
            {
                new HostsEntry
                {
                    IpAddress = ipAddress,
                    Hosts = hosts
                }
            };

            hostsWriter.WriteHosts(fileName, hostEntries, sourceIdentifier);

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
        public void WriteHosts_AddsMultipleHostEntries(string fileName, HostsEntry[] entries, string sourceIdentifer)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.AppendAllLinesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Verifiable();
            var hostsWriter = new Target(filesystem.Object);

            hostsWriter.WriteHosts(fileName, entries, sourceIdentifer);

            filesystem.Verify(f => f.File.AppendAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x => x.Count() == entries.Length),
                It.IsAny<CancellationToken>()
            ));
        }

        [Theory, AutoData]
        public void RemoveAll_RemovesSourceIdentifier(string fileName, string keepHost, string removeHost1, string removeHost2, string sourceIdentifier)
        {
            var filesystem = new Mock<IFileSystem>();
            filesystem.Setup(f => f.File.ReadAllLinesAsync(It.Is<string>(x => x == fileName), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new[]
            {
                $"127.0.0.1 {keepHost}",
                $"127.0.0.1 {removeHost1} #{sourceIdentifier}",
                $"127.0.0.1 {removeHost2} #{sourceIdentifier}"
            }));
            filesystem.Setup(f => f.File.WriteAllLinesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>())).Verifiable();
            var hostsWriter = new Target(filesystem.Object);

            hostsWriter.RemoveAll(fileName, sourceIdentifier);

            filesystem.Verify(f => f.File.WriteAllLinesAsync(
                It.Is<string>(x => x == fileName),
                It.Is<IEnumerable<string>>(x => x.Single().EndsWith(keepHost)),
                It.IsAny<CancellationToken>()
            ));
        }
    }
}
