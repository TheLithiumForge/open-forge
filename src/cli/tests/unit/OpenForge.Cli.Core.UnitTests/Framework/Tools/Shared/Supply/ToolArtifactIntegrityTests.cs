using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-supply"), Trait("Evidence", "Unit")]
public sealed class ToolArtifactIntegrityTests
{
    [Fact(DisplayName = "Artifact keys use the exact SHA-256 or SHA-512 digest")]
    public void ReadsDigestKeys()
    {
        byte[] payload = [1, 127, 255];
        var sha256 = SHA256.HashData(payload);
        var sha512 = SHA512.HashData(payload);
        var hex = Convert.ToHexStringLower(sha256);
        Assert.True(ToolArtifactIntegrity.IsValid(ToolArtifactIntegrity.Sha256Prefix + hex));
        Assert.Equal(hex, ToolArtifactIntegrity.Key(ToolArtifactIntegrity.Sha256Prefix + hex));
        var sri = ToolArtifactIntegrity.Sha512Prefix + Convert.ToBase64String(sha512);
        Assert.True(ToolArtifactIntegrity.IsValid(sri));
        Assert.Equal(Convert.ToHexStringLower(sha512), ToolArtifactIntegrity.Key(sri));
    }

    [Theory(DisplayName = "Artifact integrity rejects unsupported, truncated, uppercase and whitespace forms")]
    [InlineData("algorithm")]
    [InlineData("short-sha256")]
    [InlineData("upper-sha256")]
    [InlineData("short-sha512")]
    [InlineData("space-sha512")]
    public void RejectsMalformedIntegrity(string situation)
    {
        var sha = Convert.ToHexStringLower(SHA256.HashData([1, 2, 3]));
        var sri = Convert.ToBase64String(SHA512.HashData([1, 2, 3]));
        var value = situation switch
        {
            "algorithm" => "unsupported:" + sha,
            "short-sha256" => ToolArtifactIntegrity.Sha256Prefix + sha[..^1],
            "upper-sha256" => ToolArtifactIntegrity.Sha256Prefix + sha.ToUpperInvariant(),
            "short-sha512" => ToolArtifactIntegrity.Sha512Prefix + sri[..^1],
            "space-sha512" => ToolArtifactIntegrity.Sha512Prefix + " " + sri,
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        Assert.False(ToolArtifactIntegrity.IsValid(value));
        Assert.Throws<ArgumentException>(() => ToolArtifactIntegrity.Key(value));
    }
}
