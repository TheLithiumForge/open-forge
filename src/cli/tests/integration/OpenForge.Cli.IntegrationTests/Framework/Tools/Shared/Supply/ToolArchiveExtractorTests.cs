using System.Formats.Tar;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-supply"), Trait("Evidence", "Integration")]
public sealed class ToolArchiveExtractorTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Verified local archives extract contained members and record their exact hashes")]
    [InlineData("zip")]
    [InlineData("tar.gz")]
    [InlineData("file")]
    public async Task ExtractsVerifiedBytes(string format)
    {
        using var fixture = new ToolArtifactFixture();
        var request = fixture.Create(format);
        var before = fixture.Temporary.SnapshotHashes();
        var plan = await ToolArchiveExtractor.ReadAsync(request, Token);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        Assert.Equal(ToolArtifactFixture.Payload.Length * (format == "file" ? 1 : 2), plan.ExpandedBytes);
        var result = await ToolArchiveExtractor.ExtractAsync(request, Token);
        Assert.Equal(format == "file" ? 1 : 2, result.Files.Length);
        foreach (var file in result.Files)
        {
            var actual = await File.ReadAllBytesAsync(Path.Combine(fixture.OutputPath, file.Path), Token);
            Assert.Equal(ToolArtifactFixture.Payload, actual);
            Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(actual)), file.Sha256);
        }
    }

    [Theory(DisplayName = "Unsafe archive destinations block before creating any extracted member")]
    [InlineData("traversal")]
    [InlineData("absolute")]
    [InlineData("backslash")]
    [InlineData("duplicate")]
    [InlineData("case-collision")]
    [InlineData("file-parent")]
    public async Task RejectsUnsafeDestinations(string situation)
    {
        using var fixture = new ToolArtifactFixture();
        string[] names = situation switch
        {
            "traversal" => [ToolArtifactFixture.Wrapper + "/../escape.bin"],
            "absolute" => [Path.GetFullPath(fixture.Temporary.Combine("escape.bin"))],
            "backslash" => [ToolArtifactFixture.Wrapper + "\\escape.bin"],
            "duplicate" => [ToolArtifactFixture.Wrapper + "/same", ToolArtifactFixture.Wrapper + "/same"],
            "case-collision" => [ToolArtifactFixture.Wrapper + "/Same", ToolArtifactFixture.Wrapper + "/same"],
            "file-parent" => [ToolArtifactFixture.Wrapper + "/parent", ToolArtifactFixture.Wrapper + "/parent/child"],
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        var request = fixture.Create("zip", names);
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAsync<InvalidDataException>(async () => await ToolArchiveExtractor.ExtractAsync(request, Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Archive symbolic and hard links are rejected without creating them")]
    [InlineData(TarEntryType.SymbolicLink)]
    [InlineData(TarEntryType.HardLink)]
    public async Task RejectsLinks(TarEntryType type)
    {
        using var fixture = new ToolArtifactFixture();
        var request = fixture.Create("tar.gz", [ToolArtifactFixture.Wrapper + "/link"], type);
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAsync<InvalidDataException>(async () => await ToolArchiveExtractor.ExtractAsync(request, Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Artifact size and hash mismatches block before archive interpretation")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RejectsDifferentBytes(bool size)
    {
        using var fixture = new ToolArtifactFixture();
        var request = fixture.Create("zip");
        var artifact = size ? request.Artifact with { Bytes = request.Artifact.Bytes + 1 }
            : request.Artifact with { Integrity = ToolArtifactIntegrity.Sha256Prefix + new string('0', SHA256.HashSizeInBytes * 2) };
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAsync<InvalidDataException>(async () => await ToolArchiveExtractor.ExtractAsync(request with { Artifact = artifact }, Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "SHA-512 SRI verifies local package bytes without contacting a registry")]
    public async Task VerifiesRegistryIntegrity()
    {
        using var fixture = new ToolArtifactFixture();
        var request = fixture.Create("file");
        var sri = ToolArtifactIntegrity.Sha512Prefix + Convert.ToBase64String(SHA512.HashData(ToolArtifactFixture.Payload));
        await ToolArtifactIntegrity.VerifyAsync(request.ArchivePath, ToolArtifactFixture.Payload.Length, sri, Token);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.OutputPath));
    }
}
