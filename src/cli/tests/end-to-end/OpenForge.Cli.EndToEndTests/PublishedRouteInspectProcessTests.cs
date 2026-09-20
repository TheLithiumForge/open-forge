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
            ["route", "inspect", "root", "--workspace", working.Path, "--format=json", "--detail=full"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;

        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route inspect", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var data = root.GetProperty("data");
        Assert.Equal("root", data.GetProperty("selection").GetProperty("requested").GetString());

        Assert.Equal(".agents/root/_root.md", data.GetProperty("path").GetString());
        var layers = data.GetProperty("layers").EnumerateArray().ToArray();
        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/_root.overwrite.md"],
            layers.Select(layer => layer.GetProperty("path").GetString()));
        Assert.Equal(["base", "overwrite"], layers.Select(layer => layer.GetProperty("kind").GetString()));

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
            ["route", "inspect", ".agents/root/collision.md", "--detail=minimal"]);

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("root/collision  .agents/root/collision.md", result.StandardOutput, StringComparison.Ordinal);
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
            ["route", "inspect", "root/collision", "--detail=minimal"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains(
            "Cannot inspect root/collision: root/collision matches more than one source. Use the exact path.",
            result.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect \".agents/root/collision.md\"", result.StandardError, StringComparison.Ordinal);
    }

}
