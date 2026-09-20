using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Models;

public sealed class FrameworkPayloadTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework payload inventory is ordinal and stable for shuffled input"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void InventoryIsOrdinalAndStableForShuffledInput()
    {
        FrameworkPayloadAsset[] assets =
        [
            Asset(".agents/loader.md", "loader\n"),
            Asset("CLAUDE.md", "claude\n"),
            Asset(".agents/guidance/é.md", "guidance\n"),
            Asset("AGENTS.md", "agents\n"),
        ];

        var forward = FrameworkPayload.Create(assets);
        var shuffled = FrameworkPayload.Create([assets[2], assets[0], assets[3], assets[1]]);

        string[] expectedPaths =
        [
            ".agents/guidance/é.md",
            ".agents/loader.md",
            "AGENTS.md",
            "CLAUDE.md",
        ];
        Assert.Equal(expectedPaths, forward.Assets.Select(asset => asset.Path));
        Assert.Equal(expectedPaths, shuffled.Assets.Select(asset => asset.Path));
        Assert.Equal(forward.InventoryFingerprint, shuffled.InventoryFingerprint);
        Assert.Equal(InventoryFingerprintOracle(assets), forward.InventoryFingerprint);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework payload inventory formula uses exact UTF-8 tab fields and literal LF"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void InventoryFingerprintMatchesIndependentFormula()
    {
        FrameworkPayloadAsset[] assets =
        [
            Asset("AGENTS.md", "one"),
            Asset("CLAUDE.md", "two\r\n"),
            Asset(".agents/loader.md", "three\n"),
        ];

        var payload = FrameworkPayload.Create(assets);

        Assert.Equal(InventoryFingerprintOracle(assets), payload.InventoryFingerprint);
        Assert.DoesNotContain("\r", InventoryManifestOracle(assets), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework payload rejects duplicate paths and missing required anchors"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void PayloadRejectsDuplicatesAndMissingAnchors()
    {
        var agents = Asset("AGENTS.md", "agents");
        var claude = Asset("CLAUDE.md", "claude");
        var loader = Asset(".agents/loader.md", "loader");

        Assert.Throws<ArgumentException>(() => FrameworkPayload.Create([agents, claude, loader, Asset("AGENTS.md", "duplicate")]));
        Assert.Throws<ArgumentException>(() => FrameworkPayload.Create([claude, loader]));
        Assert.Throws<ArgumentException>(() => FrameworkPayload.Create([agents, loader]));
        Assert.Throws<ArgumentException>(() => FrameworkPayload.Create([agents, claude]));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework payload lookup is exact ordinal and returns owned assets"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void FindUsesExactOrdinalIdentity()
    {
        var agents = Asset("AGENTS.md", "agents");
        var claude = Asset("CLAUDE.md", "claude");
        var loader = Asset(".agents/loader.md", "loader");
        var payload = FrameworkPayload.Create([loader, claude, agents]);

        Assert.Same(loader, payload.Find(".agents/loader.md"));
        Assert.Null(payload.Find(".agents/Loader.md"));
        Assert.Null(payload.Find("agents.md"));
        Assert.Null(payload.Find(".agents\\loader.md"));
    }

    private static FrameworkPayloadAsset Asset(string path, string content)
        => FrameworkPayloadAsset.Create(path, Encoding.UTF8.GetBytes(content));

    private static string InventoryFingerprintOracle(IEnumerable<FrameworkPayloadAsset> assets)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(InventoryManifestOracle(assets))));

    private static string InventoryManifestOracle(IEnumerable<FrameworkPayloadAsset> assets)
    {
        var builder = new StringBuilder();
        foreach (var asset in assets.OrderBy(asset => asset.Path, StringComparer.Ordinal))
        {
            builder.Append(CultureInfo.InvariantCulture, $"{asset.Path}\t{asset.ByteLength}\t{asset.Sha256}\n");
        }

        return builder.ToString();
    }
}
