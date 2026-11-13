using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools;

[Trait("Boundary", "Filesystem"), Trait("Feature", "tool-catalogue"), Trait("Evidence", "Integration")]
public sealed class ToolCatalogueIntegrationTests
{
    [Fact(DisplayName = "The production Tool catalogue is empty and creates no workspace state")]
    public async Task BundledCatalogueIsEmpty()
    {
        using var workspace = TemporaryWorkspace.Create("tool-empty-catalogue");
        var before = workspace.SnapshotHashes();
        var result = await new ToolCatalogueReader().ReadAsync(null, TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Complete, result.State);
        Assert.Empty(result.Definitions);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Catalogue defaults and exact pins do not depend on array order")]
    public async Task SelectsExactPins()
    {
        using var workspace = TemporaryWorkspace.Create("tool-exact-pins");
        var first = ToolFixtureSource.ManifestBytes();
        var secondNode = ToolFixtureSource.Manifest();
        secondNode["version"] = "2.0.0+fixture";
        var second = Encoding.UTF8.GetBytes(secondNode.ToJsonString());
        workspace.WriteBytes("first.json", first);
        workspace.WriteBytes("second.json", second);
        var catalogue = Catalogue(first, second);
        workspace.WriteText("catalogue.json", catalogue.ToJsonString());
        var reader = Reviewed(first, second);
        var before = workspace.SnapshotHashes();
        var read = await reader.ReadAsync(workspace.Combine("catalogue.json"), TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Complete, read.State);
        Assert.Equal(2, read.Definitions.Count);
        Assert.Equal(ToolFixtureSource.Version, ToolCatalogueReader.Select(read, ToolFixtureSource.Id, null).Definition?.Manifest.Version);
        Assert.Equal("2.0.0+fixture", ToolCatalogueReader.Select(read, ToolFixtureSource.Id, "2.0.0+fixture").Definition?.Manifest.Version);
        Assert.Equal(ToolSourceState.Invalid, ToolCatalogueReader.Select(read, ToolFixtureSource.Id, "2.0.0").State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Malformed catalogues cannot select a pin or escape their source root")]
    [InlineData("digest")]
    [InlineData("identity")]
    [InlineData("default")]
    [InlineData("duplicate")]
    [InlineData("escape")]
    [InlineData("link")]
    public async Task RejectsCatalogueFailure(string kind)
    {
        using var workspace = TemporaryWorkspace.Create("tool-invalid-catalogue");
        var first = ToolFixtureSource.ManifestBytes();
        var second = ToolFixtureSource.Manifest();
        second["version"] = "2.0.0+fixture";
        var secondBytes = Encoding.UTF8.GetBytes(second.ToJsonString());
        workspace.WriteBytes("first.json", first);
        workspace.WriteBytes("second.json", secondBytes);
        var catalogue = Catalogue(first, secondBytes);
        switch (kind)
        {
            case "digest": catalogue["tools"]![0]!["versions"]![0]!["sha256"] = new string('0', 64); break;
            case "identity": catalogue["tools"]![0]!["id"] = "other-tool"; break;
            case "default": catalogue["tools"]![0]!["defaultVersion"] = "3.0.0"; break;
            case "duplicate": catalogue["tools"]![0]!["versions"]![0]!["path"] = "first.json"; break;
            case "escape": catalogue["tools"]![0]!["versions"]![0]!["path"] = "../escape.json"; break;
            case "link":
                workspace.CreateFileSymbolicLink("linked.json", workspace.Combine("first.json"));
                catalogue["tools"]![0]!["versions"]![0]!["path"] = "linked.json";
                break;
        }
        workspace.WriteText("catalogue.json", catalogue.ToJsonString());
        var result = await Reviewed(first, secondBytes).ReadAsync(workspace.Combine("catalogue.json"), TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Invalid, result.State);
    }

    [Fact(DisplayName = "Unreadable required manifest bytes give unavailable coverage without fallback")]
    public async Task MissingMemberIsUnavailable()
    {
        using var workspace = TemporaryWorkspace.Create("tool-unavailable-source");
        var bytes = ToolFixtureSource.ManifestBytes();
        workspace.WriteText("catalogue.json", Catalogue(bytes, bytes).ToJsonString());
        var result = await new ToolCatalogueReader().ReadAsync(workspace.Combine("catalogue.json"), TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Unavailable, result.State);
        Assert.Empty(result.Definitions);
    }

    [Fact(DisplayName = "Release support checks the exact manifest digest and compiled revision")]
    public async Task UnreviewedPinIsBlocked()
    {
        using var workspace = TemporaryWorkspace.Create("tool-unreviewed-source");
        workspace.WriteBytes(ToolFixtureSource.ManifestName, ToolFixtureSource.ManifestBytes());
        var read = await new ToolCatalogueReader().ReadAsync(workspace.Combine(ToolFixtureSource.ManifestName), TestContext.Current.CancellationToken);
        var selected = ToolCatalogueReader.Select(read, ToolFixtureSource.Id, null);
        Assert.Equal(ToolSourceState.Blocked, selected.State);
        Assert.NotNull(selected.Definition);
        var missingAdapter = new ToolCatalogueReader(new([new(ToolFixtureSource.Id, ToolFixtureSource.Version, ToolCatalogueReader.Hash(ToolFixtureSource.ManifestBytes()), 1)], []));
        var unsupported = await missingAdapter.ReadAsync(workspace.Combine(ToolFixtureSource.ManifestName), TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Blocked, ToolCatalogueReader.Select(unsupported, ToolFixtureSource.Id, null).State);
    }

    [Fact(DisplayName = "Synthetic artifacts are local immutable bytes matching the manifest hash")]
    public void ArtifactFixtureHashMatches()
    {
        using var workspace = TemporaryWorkspace.Create("tool-local-artifact");
        var artifact = ToolFixtureSource.ArtifactBytes();
        workspace.WriteBytes(ToolFixtureSource.ArtifactName, artifact);
        var expected = ToolFixtureSource.Manifest()["artifacts"]!["package"]!["integrity"]?.GetValue<string>();
        Assert.Equal($"sha256:{Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(workspace.Combine(ToolFixtureSource.ArtifactName))))}", expected);
    }

    private static ToolCatalogueReader Reviewed(params byte[][] manifests)
    {
        var parser = new ToolCatalogueReader();
        var definitions = manifests.Select(bytes => parser.Definition(bytes, ToolFixtureSource.ManifestName, true)).ToArray();
        return new(new ToolReleaseSupport(definitions.Select(definition => new ToolReviewedPin(definition.Manifest.Id,
            definition.Manifest.Version, definition.Sha256, definition.Manifest.Adapter.Revision)), [new("codegraph-1.6", 1)]));
    }

    private static JsonObject Catalogue(byte[] first, byte[] second) => new()
    {
        ["schema"] = ToolVocabulary.ManifestSchema,
        ["tools"] = new JsonArray(new JsonObject
        {
            ["id"] = ToolFixtureSource.Id,
            ["defaultVersion"] = ToolFixtureSource.Version,
            ["versions"] = new JsonArray(
                new JsonObject { ["version"] = "2.0.0+fixture", ["path"] = "second.json", ["sha256"] = ToolCatalogueReader.Hash(second) },
                new JsonObject { ["version"] = ToolFixtureSource.Version, ["path"] = "first.json", ["sha256"] = ToolCatalogueReader.Hash(first) }),
        }),
    };
}
