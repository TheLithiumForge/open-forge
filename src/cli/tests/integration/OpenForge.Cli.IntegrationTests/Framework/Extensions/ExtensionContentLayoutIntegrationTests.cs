using System.Text;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions;

[Trait("Feature", "extension-content"), Trait("Evidence", "Integration")]
public sealed class ExtensionContentLayoutIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact]
    public async Task ReadsContentFilesWithExactSourceAndDestinationCoordinates()
    {
        using var consumer = TemporaryWorkspace.Create("content-consumer");
        using var source = TemporaryWorkspace.Create("content-package");
        source.CreateFile("extension.json", """{"id":"team","name":"Team","description":"Test package","version":"1.0.0","dependencies":[]}""");
        source.CreateFile("content/.agents/notes/a.txt", "internal bytes\n");
        source.CreateFile("content/.apm/agents/reviewer.md", "external bytes\n");
        var before = source.SnapshotHashes();
        var workspace = new CliWorkspace(consumer.Path, consumer.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        var result = await new ExtensionSourceReader(new PhysicalPathResolver()).ReadAsync(
            workspace, source.Path, TestContext.Current.CancellationToken);

        Assert.Equal(ExtensionSourceReadState.Complete, result.State);
        var package = Assert.Single(result.Packages);
        Assert.Equal(2, package.Payload.Count);
        var external = Assert.Single(package.Payload, value => value.TargetPath == ".apm/agents/reviewer.md");
        Assert.Equal("content/.apm/agents/reviewer.md", external.Path);
        Assert.Equal(ExtensionPackageFileReadState.Available, external.State);
        var bytes = Assert.IsType<ReadOnlyMemory<byte>>(external.Bytes);
        Assert.Equal("external bytes\n", Encoding.UTF8.GetString(bytes.Span));
        Assert.Equal(before, source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task UnavailableCatalogueClassificationCannotFallThroughToPackageSuccess()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This source-enumeration boundary requires Unix directory permissions.");
            return;
        }

        using var consumer = TemporaryWorkspace.Create("source-enumeration-consumer");
        using var source = TemporaryWorkspace.Create("source-enumeration-package");
        source.CreateFile("extension.json", """{"id":"team","name":"Team","description":"Test package","version":"1.0.0","dependencies":[]}""");
        source.CreateFile("child/extension.json", """{"id":"child","name":"Child","description":"Test package","version":"1.0.0","dependencies":[]}""");
        var original = File.GetUnixFileMode(source.Path);
        var before = source.SnapshotHashes();
        var workspace = new CliWorkspace(consumer.Path, consumer.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        try
        {
            File.SetUnixFileMode(source.Path, UnixFileMode.UserExecute);

            var result = await new ExtensionSourceReader(new PhysicalPathResolver()).ReadAsync(
                workspace, source.Path, TestContext.Current.CancellationToken);

            Assert.Equal(ExtensionSourceReadState.Unavailable, result.State);
            Assert.Empty(result.Packages);
        }
        finally
        {
            File.SetUnixFileMode(source.Path, original);
        }
        Assert.Equal(before, source.SnapshotHashes());
    }

}
