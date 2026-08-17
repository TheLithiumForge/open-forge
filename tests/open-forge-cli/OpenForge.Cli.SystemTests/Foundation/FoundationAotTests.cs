using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Foundation;

namespace OpenForge.Cli.SystemTests.Foundation;

public sealed class FoundationAotTests
{
    [Fact(DisplayName = "Native AOT executes the complete internal foundation probe")]
    [Trait("Feature", "foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ExecutesFoundationProbe()
    {
        using var workspace = TemporaryWorkspace.Create();
        const string markdown = "# Native AOT\n";
        var request = new FoundationRequest(workspace.Path, markdown, "name: native-aot\n");

        var result = await FoundationProbe.RunAsync(request, TestContext.Current.CancellationToken);

        var succeeded = Assert.IsType<FoundationSucceeded>(result);
        var expectedBytes = Encoding.UTF8.GetBytes(markdown);
        var expectedHash = Convert.ToHexString(SHA256.HashData(expectedBytes)).ToLowerInvariant();
        Assert.Equal(1, succeeded.Report.Snapshot.SchemaVersion);
        Assert.Equal(1, succeeded.Report.Snapshot.MarkdownHeadingCount);
        Assert.Equal("native-aot", succeeded.Report.Snapshot.YamlName);
        Assert.Equal(expectedBytes.Length, succeeded.Report.Snapshot.FileByteCount);
        Assert.Equal(expectedHash, succeeded.Report.Snapshot.FileSha256);
        Assert.True(succeeded.Report.Snapshot.ExclusiveLockObserved);
        Assert.Equal(
            expectedBytes,
            await File.ReadAllBytesAsync(
                Path.Combine(workspace.Path, "foundation-probe.md"),
                TestContext.Current.CancellationToken));
        Assert.Equal(
            $$"""{"schemaVersion":1,"markdownHeadingCount":1,"yamlName":"native-aot","fileByteCount":{{expectedBytes.Length}},"fileSha256":"{{expectedHash}}","exclusiveLockObserved":true}""",
            succeeded.Report.Json);
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
                $"open-forge-foundation-aot-{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            return new TemporaryWorkspace(path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
