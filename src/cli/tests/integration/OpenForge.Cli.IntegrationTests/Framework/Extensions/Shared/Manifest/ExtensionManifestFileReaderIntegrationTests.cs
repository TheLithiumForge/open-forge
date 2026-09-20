using OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;
using System.Text;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Extensions.Shared.Manifest;

public sealed class ExtensionManifestFileReaderIntegrationTests
{
    private const string ValidManifest = """{"id":"toolkit","name":"Toolkit","description":"A toolkit.","version":"1.2.3","dependencies":["base"]}""";

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File manifest reader preserves package facts without changing source bytes"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Integration")]
    public async Task ReadsPackageFacts()
    {
        using var source = TemporaryWorkspace.Create("manifest-file");
        source.CreateFile("extension.json", ValidManifest);
        var before = source.SnapshotHashes();

        var outcome = await ReadAsync(source.Combine("extension.json"), TestContext.Current.CancellationToken);

        var package = Assert.IsType<ExtensionManifestFileReadSuccess>(outcome).Package;
        Assert.Equal("toolkit", package.Id);
        Assert.Equal("Toolkit", package.Name);
        Assert.Equal("A toolkit.", package.Description);
        Assert.Equal("1.2.3", package.Version);
        Assert.Equal(["base"], package.Dependencies);
        Assert.Equal("toolkit/extension.json", package.ManifestPath);
        var file = Assert.Single(package.Payload);
        Assert.Equal("content/note.txt", file.Path);
        Assert.Equal(ExtensionPackageFileReadState.Missing, file.State);
        Assert.Equal(before, source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "File manifest reader retains malformed package and dependency conflict outcomes"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Integration")]
    [InlineData("syntax")]
    [InlineData("encoding")]
    [InlineData("required")]
    [InlineData("identity")]
    [InlineData("dependency")]
    public static async Task ClassifiesMalformedManifest(string scenario)
    {
        using var source = TemporaryWorkspace.Create("invalid-manifest-file");
        var path = source.Combine("extension.json");
        var content = scenario switch
        {
            "syntax" => Encoding.UTF8.GetBytes("{"),
            "encoding" => [0xff],
            "required" => Encoding.UTF8.GetBytes("{}"),
            "identity" => Encoding.UTF8.GetBytes(ValidManifest.Replace("toolkit", "Invalid_ID", StringComparison.Ordinal)),
            "dependency" => Encoding.UTF8.GetBytes(ValidManifest.Replace("[\"base\"]", "[\"base\",\"base\"]", StringComparison.Ordinal)),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        source.CreateFile("extension.json", content);
        var before = source.SnapshotHashes();

        var outcome = await ReadAsync(path, TestContext.Current.CancellationToken);

        var failure = Assert.IsType<ExtensionManifestFileReadFailure>(outcome).Result;
        Assert.Equal(ExtensionSourceReadState.Invalid, failure.State);
        Assert.Equal(path, failure.Identity);
        Assert.Null(failure.Kind);
        Assert.Empty(failure.Packages);
        if (scenario == "dependency")
        {
            Assert.Equal(ExtensionSourceFailureKind.DependencyConflict, failure.FailureKind);
            Assert.Equal("The Extension manifest has conflicting dependencies: Extension dependencies cannot contain duplicate stable IDs.", failure.Cause);
        }
        else
        {
            Assert.Equal(ExtensionSourceFailureKind.PackageInvalid, failure.FailureKind);
            Assert.StartsWith("The Extension manifest is invalid: ", failure.Cause);
        }
        if (scenario == "encoding")
        {
            Assert.Equal(
                "The Extension manifest is invalid: Unable to translate bytes [FF] at index 0 from specified code page to Unicode.",
                failure.Cause);
        }
        Assert.Equal(before, source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "File manifest reader classifies missing inaccessible and cancelled file observations"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Integration")]
    [InlineData("missing")]
    [InlineData("directory")]
    [InlineData("cancelled")]
    public static async Task ClassifiesFileFailures(string scenario)
    {
        using var source = TemporaryWorkspace.Create("manifest-file-failure");
        source.CreateFile("extension.json", ValidManifest);
        var path = scenario switch
        {
            "missing" => source.Combine("missing/extension.json"),
            "directory" => source.Path,
            "cancelled" => source.Combine("extension.json"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        using var cancellation = new CancellationTokenSource();
        if (scenario == "cancelled")
        {
            cancellation.Cancel();
        }
        var before = source.SnapshotHashes();

        var outcome = await ReadAsync(path, cancellation.Token);

        var failure = Assert.IsType<ExtensionManifestFileReadFailure>(outcome).Result;
        Assert.Equal(scenario == "cancelled" ? ExtensionSourceReadState.Cancelled : ExtensionSourceReadState.Unavailable, failure.State);
        Assert.Equal(ExtensionSourceFailureKind.None, failure.FailureKind);
        Assert.Equal(path, failure.Identity);
        Assert.Null(failure.Kind);
        Assert.Empty(failure.Packages);
        Assert.False(string.IsNullOrWhiteSpace(failure.Cause));
        if (scenario == "cancelled")
        {
            Assert.Equal("Extension source inspection was interrupted.", failure.Cause);
        }
        Assert.Equal(before, source.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Full file reads validate manifest coordinates before file observation"), Trait("Feature", "extension-manifest"), Trait("Evidence", "Integration")]
    [InlineData("")]
    [InlineData(" ")]
    public static async Task RejectsInvalidManifestPathBeforeRead(string manifestPath)
    {
        using var source = TemporaryWorkspace.Create("manifest-ingress");

        await Assert.ThrowsAsync<ArgumentException>(async () => await ExtensionManifestFileReader.ReadAsync(
            source.Combine("missing.json"), manifestPath, [], TestContext.Current.CancellationToken));

        Assert.Empty(source.SnapshotHashes());
    }

    private static ValueTask<ExtensionManifestFileReadOutcome> ReadAsync(string path, CancellationToken cancellationToken)
        => ExtensionManifestFileReader.ReadAsync(
            path,
            "toolkit/extension.json",
            [ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot { Path = "content/note.txt", State = ExtensionPackageFileReadState.Missing })],
            cancellationToken);
}
