using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Models;

public sealed class FrameworkPayloadAssetTests
{
    [Fact(DisplayName = "Framework payload assets own exact bytes and lowercase SHA-256"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void AssetsOwnExactBytesAndHash()
    {
        var expected = Encoding.UTF8.GetBytes("loader bytes\n");
        var source = expected.ToArray();

        var asset = FrameworkPayloadAsset.Create(".agents/loader.md", source);
        source[0] = (byte)'X';

        Assert.Equal(".agents/loader.md", asset.Path);
        Assert.Equal(expected.Length, asset.ByteLength);
        Assert.Equal(expected, asset.Bytes);
        Assert.Equal(Hash(expected), asset.Sha256);
        Assert.Equal(asset.ByteLength, asset.Bytes.Length);
    }

    [Theory(DisplayName = "Framework payload assets reject noncanonical paths"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("/")]
    [InlineData(".agents")]
    [InlineData(".agents/")]
    [InlineData(".agents//loader.md")]
    [InlineData(".agents/./loader.md")]
    [InlineData(".agents/../AGENTS.md")]
    [InlineData(".agents\\loader.md")]
    [InlineData("agents/loader.md")]
    [InlineData("AGENTS.md/child")]
    [InlineData("CLAUDE.md/child")]
    [InlineData(".agents/\u0001invalid.md")]
    public void AssetsRejectNoncanonicalPaths(string path)
    {
        Assert.Throws<ArgumentException>(() => FrameworkPayloadAsset.Create(path, "bytes"u8));
    }

    [Theory(DisplayName = "Framework payload assets admit only exact roots and canonical agent descendants"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    [InlineData("AGENTS.md")]
    [InlineData("CLAUDE.md")]
    [InlineData(".agents/loader.md")]
    [InlineData(".agents/guidance/adaptive-collaboration.md")]
    public void AssetsAdmitCanonicalPaths(string path)
    {
        var asset = FrameworkPayloadAsset.Create(path, "bytes"u8);

        Assert.Equal(path, asset.Path);
    }

    private static string Hash(ReadOnlySpan<byte> bytes)
        => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
