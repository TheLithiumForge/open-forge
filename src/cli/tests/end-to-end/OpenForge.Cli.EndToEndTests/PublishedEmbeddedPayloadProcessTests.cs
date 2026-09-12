using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedEmbeddedPayloadProcessTests
{
    [Fact(DisplayName = "Relocated published artifact reaches its embedded Install payload without workspace or lock writes"), Trait("Feature", "published-artifact"), Trait("Evidence", "EndToEnd")]
    public async Task RelocatedArtifactReachesEmbeddedPayload()
    {
        var target = PublishedExecutableTarget.Discover();
        using var relocated = RelocatedPublishedInstallLayout.Create(target);
        using var workspace = PublishedInstallWorkspace.Create();
        var before = workspace.SnapshotState();
        var result = await relocated.RunAsync(workspace.Path, ["install", "--automatic", "--dry-run", "--json"], workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var plan = document.RootElement.GetProperty("result");
        Assert.Equal("dry-run", plan.GetProperty("mode").GetString());
        Assert.Equal(PublishedInstallWorkspace.EmbeddedPayloadPaths.Count + 2, plan.GetProperty("source").GetProperty("assetCount").GetInt32());
        Assert.Contains(plan.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("path").GetString() == ".agents/loader.md");
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Relocated published artifact lists the source-derived embedded Extension catalogue without writes"), Trait("Feature", "published-artifact"), Trait("Evidence", "EndToEnd")]
    public async Task RelocatedArtifactReachesEmbeddedExtensions()
    {
        var target = PublishedExecutableTarget.Discover();
        using var relocated = RelocatedPublishedInstallLayout.Create(target);
        using var workspace = PublishedInstallWorkspace.Create();
        var before = workspace.SnapshotState();
        var result = await relocated.RunAsync(workspace.Path,
            ["extension", "list", "--available", "--json"], workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var catalogue = document.RootElement.GetProperty("result");
        Assert.Equal("embedded-catalogue", catalogue.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal(ExtensionCatalogueSource.PackageIds,
            catalogue.GetProperty("available").EnumerateArray()
                .Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoLockInfrastructure();
    }
}
