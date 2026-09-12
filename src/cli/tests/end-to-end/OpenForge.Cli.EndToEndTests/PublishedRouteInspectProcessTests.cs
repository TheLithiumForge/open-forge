using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInspectProcessTests
{
    [Fact(DisplayName = "Published Route Inspect JSON exposes the complete typed identity and profile graph"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task JsonResultExposesCompleteTypedGraph()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root", "--workspace", working.Path, "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(
            "root",
            root.GetProperty("result").GetProperty("selection").GetProperty("requestedReference").GetString());

        var identity = root.GetProperty("result").GetProperty("identity");
        Assert.Equal(".agents/root/_root.md", identity.GetProperty("path").GetString());
        var layers = identity.GetProperty("physicalLayers").EnumerateArray().ToArray();
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/_root.overwrite.md"],
            layers.Select(layer => layer.GetProperty("workspaceRelativePath").GetString()));
        Assert.Equal(["base", "overwrite"], layers.Select(layer => layer.GetProperty("role").GetString()));

        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Published Route Inspect exact-path attention status uses stdout without an invented next action"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task ExactPathAttentionStatusHasNoInventedNext()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", ".agents/root/collision.md", "--view=compact"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Status: requires attention", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published redirected Route Inspect blocks a human collision without prompting"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task BlockedCollisionUsesExactNextWording()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateAttention();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "root/collision", "--view=compact"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.DoesNotContain("matches more than one source", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Choose a source by number or exact path", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: blocked", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect \".agents/root/collision.md\"", result.StandardError, StringComparison.Ordinal);
    }

}
