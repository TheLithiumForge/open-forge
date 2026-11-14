using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-supply"), Trait("Evidence", "Unit")]
public sealed class ToolArtifactIntegrityTests
{
    [Fact(DisplayName = "Artifact SHA-256 pins have an exact hexadecimal digest")]
    public void ReadsDigest()
    {
        var digest = Convert.ToHexStringLower(SHA256.HashData([1, 127, 255]));
        Assert.True(ToolArtifactIntegrity.IsSha256(digest));
        Assert.True(ToolArtifactIntegrity.IsSha256(digest.ToUpperInvariant()));
        Assert.False(ToolArtifactIntegrity.IsSha256(digest[..^1]));
        Assert.False(ToolArtifactIntegrity.IsSha256($" {digest}"));
        Assert.False(ToolArtifactIntegrity.IsSha256(null));
    }

    [Theory(DisplayName = "Direct sources reject insecure and unapproved locations")]
    [InlineData("https://github.com/example/sample/releases/download/v2.4.3/sample.zip", true)]
    [InlineData("https://nodejs.org/dist/v2.4.3/sample.zip", true)]
    [InlineData("http://github.com/example/sample/releases/download/v2.4.3/sample.zip", false)]
    [InlineData("https://user:password@github.com/example/sample/releases/download/v2.4.3/sample.zip", false)]
    [InlineData("https://example.com/sample.zip", false)]
    [InlineData("https://github.com/example/sample/releases/download/v2.4.2/sample.zip", false)]
    public void ChecksSource(string url, bool official)
    {
        var asset = new ToolReleaseAsset
        {
            Version = "2.4.3",
            Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
            Url = new(url),
            Archive = ToolArchiveKind.Zip,
            EntryRelativePath = "sample",
            StripComponents = 0,
            MaximumBytes = 100,
        };
        Assert.Equal(official, ToolArtifactSource.IsOfficial(asset));
    }
}
