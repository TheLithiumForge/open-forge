using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Foundation;

namespace OpenForge.Cli.Tests.Foundation;

public sealed class FoundationProbeTests
{
    [Fact(DisplayName = "Foundation rejects a relative workspace before filesystem access")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "Unit")]
    public async Task RejectsRelativeWorkspace()
    {
        var request = new FoundationRequest("relative", "# Heading\n", "name: forge\n");

        var result = await FoundationProbe.RunAsync(request, TestContext.Current.CancellationToken);

        var rejected = Assert.IsType<FoundationRejected>(result);
        Assert.Equal(FoundationFailureKind.InvalidInput, rejected.Failure.Kind);
    }

    [Fact(DisplayName = "Foundation proves parser serialization filesystem and lock boundaries")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ProvesFoundationBoundaries()
    {
        using var workspace = TemporaryWorkspace.Create();
        const string markdown = "# One\r\n\r\n## Two\r\n";
        const string yaml = "name: forge\n";
        var request = new FoundationRequest(workspace.Path, markdown, yaml);

        var result = await FoundationProbe.RunAsync(request, TestContext.Current.CancellationToken);

        var succeeded = Assert.IsType<FoundationSucceeded>(result);
        var expectedBytes = Encoding.UTF8.GetBytes(markdown);
        var expectedHash = Convert.ToHexString(SHA256.HashData(expectedBytes)).ToLowerInvariant();
        Assert.Equal(1, succeeded.Report.Snapshot.SchemaVersion);
        Assert.Equal(2, succeeded.Report.Snapshot.MarkdownHeadingCount);
        Assert.Equal("forge", succeeded.Report.Snapshot.YamlName);
        Assert.Equal(expectedBytes.Length, succeeded.Report.Snapshot.FileByteCount);
        Assert.Equal(expectedHash, succeeded.Report.Snapshot.FileSha256);
        Assert.True(succeeded.Report.Snapshot.ExclusiveLockObserved);
        Assert.Equal(
            expectedBytes,
            await File.ReadAllBytesAsync(
                Path.Combine(workspace.Path, "foundation-probe.md"),
                TestContext.Current.CancellationToken));
        Assert.Equal(
            $$"""{"schemaVersion":1,"markdownHeadingCount":2,"yamlName":"forge","fileByteCount":{{expectedBytes.Length}},"fileSha256":"{{expectedHash}}","exclusiveLockObserved":true}""",
            succeeded.Report.Json);
    }

    [Fact(DisplayName = "Foundation rejects invalid YAML without writing the probe file")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "Integration")]
    public async Task RejectsInvalidYamlWithoutWriting()
    {
        using var workspace = TemporaryWorkspace.Create();
        var request = new FoundationRequest(workspace.Path, "# Heading\n", "name: [\n");

        var result = await FoundationProbe.RunAsync(request, TestContext.Current.CancellationToken);

        var rejected = Assert.IsType<FoundationRejected>(result);
        Assert.Equal(FoundationFailureKind.InvalidInput, rejected.Failure.Kind);
        Assert.False(File.Exists(Path.Combine(workspace.Path, "foundation-probe.md")));
    }

    [Fact(DisplayName = "Foundation rejects an empty YAML document without writing the probe file")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "Integration")]
    public async Task RejectsEmptyYamlWithoutWriting()
    {
        using var workspace = TemporaryWorkspace.Create();
        var request = new FoundationRequest(workspace.Path, "# Heading\n", string.Empty);

        var result = await FoundationProbe.RunAsync(request, TestContext.Current.CancellationToken);

        var rejected = Assert.IsType<FoundationRejected>(result);
        Assert.Equal(FoundationFailureKind.InvalidInput, rejected.Failure.Kind);
        Assert.False(File.Exists(Path.Combine(workspace.Path, "foundation-probe.md")));
    }

    [Fact(DisplayName = "Foundation reports a filesystem subject that is not a directory")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ReportsNonDirectoryWorkspace()
    {
        using var workspace = TemporaryWorkspace.Create();
        var filePath = Path.Combine(workspace.Path, "not-a-directory");
        await File.WriteAllTextAsync(filePath, "occupied", TestContext.Current.CancellationToken);
        var request = new FoundationRequest(filePath, "# Heading\n", "name: forge\n");

        var result = await FoundationProbe.RunAsync(request, TestContext.Current.CancellationToken);

        var rejected = Assert.IsType<FoundationRejected>(result);
        Assert.Equal(FoundationFailureKind.Filesystem, rejected.Failure.Kind);
        Assert.Equal(
            "occupied",
            await File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken));
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryWorkspace Create()
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"open-forge-foundation-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return new TemporaryWorkspace(path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
