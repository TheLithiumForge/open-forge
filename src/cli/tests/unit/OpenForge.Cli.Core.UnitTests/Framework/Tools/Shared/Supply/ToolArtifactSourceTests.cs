using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Manifest;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-supply"), Trait("Evidence", "Unit")]
public sealed class ToolArtifactSourceTests
{
    [Fact(DisplayName = "Per-artifact URLs can name a private Node runtime and registry package in one recipe")]
    public void KeepsIndependentSources()
    {
        var json = ToolFixtureSource.Manifest();
        var artifacts = Assert.IsType<JsonObject>(json["artifacts"]);
        var package = Assert.IsType<JsonObject>(artifacts["package"]);
        package["url"] = $"https://{ToolArtifactSource.RegistryHost}/fixture/-/fixture-{ToolFixtureSource.Version}.tgz";
        var node = package.DeepClone().AsObject();
        node["file"] = "node.zip";
        node["url"] = $"https://{ToolArtifactSource.NodeHost}/dist/v{ToolFixtureSource.Version}/node.zip";
        artifacts["node"] = node;
        var runtime = Assert.IsType<JsonObject>(json["runtime"]);
        runtime["kind"] = "locked-package";
        runtime["version"] = ToolFixtureSource.Version;
        runtime["extraDownloads"] = new JsonArray("node");
        foreach (var target in Assert.IsType<JsonObject>(json["targets"]).Select(pair => Assert.IsType<JsonObject>(pair.Value)))
            target["lock"] = new JsonObject { ["artifacts"] = new JsonArray("node", "package"), ["closureComplete"] = true };
        var manifest = Read(json);
        Assert.Equal(ToolArtifactSource.RegistryHost, ToolArtifactSource.Resolve(manifest, manifest.Artifacts["package"]).Host);
        Assert.Equal(ToolArtifactSource.NodeHost, ToolArtifactSource.Resolve(manifest, manifest.Artifacts["node"]).Host);
        Assert.True(ToolArtifactSource.IsOfficial(manifest, manifest.Artifacts["package"]));
        Assert.True(ToolArtifactSource.IsOfficial(manifest, manifest.Artifacts["node"]));
        Assert.False(ToolArtifactSource.IsOfficial(manifest with { Runtime = manifest.Runtime with { Version = "2.0.0" } }, manifest.Artifacts["node"]));
    }

    [Theory(DisplayName = "Artifact overrides reject non-HTTPS, credentials, queries and fragments")]
    [InlineData("http://example.invalid/asset.zip")]
    [InlineData("https://name:password@example.invalid/asset.zip")]
    [InlineData("https://example.invalid/asset.zip?pin=changed")]
    [InlineData("https://example.invalid/asset.zip#member")]
    public void RejectsUnsafeOverrides(string url)
    {
        var json = ToolFixtureSource.Manifest();
        var artifacts = Assert.IsType<JsonObject>(json["artifacts"]);
        Assert.IsType<JsonObject>(artifacts["package"])["url"] = url;
        Assert.Throws<JsonException>(() => Read(json));
    }

    [Fact(DisplayName = "Native supply accepts only the exact source project's tagged GitHub release path")]
    public void ChecksNativeSource()
    {
        var manifest = Read(ToolFixtureSource.Manifest());
        var artifact = manifest.Artifacts["package"];
        manifest = manifest with { Source = manifest.Source with { Repository = $"https://{ToolArtifactSource.GitHubHost}/fixture/{manifest.Id}" } };
        var release = $"{manifest.Source.Repository.TrimEnd('/')}/releases/download/{Uri.EscapeDataString(manifest.Source.Tag)}/{artifact.File}";
        Assert.True(ToolArtifactSource.IsOfficial(manifest, artifact with { Url = release }));
        Assert.False(ToolArtifactSource.IsOfficial(manifest, artifact with { Url = release.Replace("/releases/download/", "/archive/", StringComparison.Ordinal) }));
        Assert.False(ToolArtifactSource.IsOfficial(manifest, artifact with { Url = $"https://mirror.invalid/{artifact.File}" }));
        Assert.False(ToolArtifactSource.IsOfficial(manifest, artifact with { Url = new UriBuilder(release) { Port = 8443 }.Uri.AbsoluteUri }));
    }

    [Fact(DisplayName = "Absent artifact overrides keep the manifest's existing download base and file")]
    public void KeepsBaseCompatibility()
    {
        var manifest = Read(ToolFixtureSource.Manifest());
        var artifact = manifest.Artifacts["package"];
        Assert.Null(artifact.Url);
        Assert.Equal(new Uri($"{manifest.DownloadBase.TrimEnd('/')}/{artifact.File}"), ToolArtifactSource.Resolve(manifest, artifact));
    }

    private static ToolManifest Read(JsonObject json)
        => new ToolCatalogueReader().Definition(System.Text.Encoding.UTF8.GetBytes(json.ToJsonString()), ToolFixtureSource.ManifestName, true).Manifest;
}
