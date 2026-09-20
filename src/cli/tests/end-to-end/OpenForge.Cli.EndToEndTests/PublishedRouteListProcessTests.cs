using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using static OpenForge.Cli.EndToEndTests.Shared.PublishedProcess.PublishedProcessTestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteListProcessTests
{
    [Fact(DisplayName = "Published route list emits one structured result without mutating the workspace"),
     Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListEmitsStructuredReadOnlyResult()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
            working.Path,
            ["route", "list", "--format=json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("route list", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var resultBody = document.RootElement.GetProperty("data");
        Assert.Equal(1, resultBody.GetProperty("depth").GetInt32());
        Assert.Contains(resultBody.GetProperty("rows").EnumerateArray(), row => row.GetProperty("id").GetString() == "workspace-defined");
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published Route List selects one exact path at depth zero in compact view"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListExactPathUsesZeroDepthCompactView()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            ["route", "list", ".agents/root/_root.md", "--depth=0", "--detail=minimal"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("Status:", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Coverage:", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("  root", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/root/_root.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("root/child", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route List rejects invalid depth with typed JSON and no writes"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListInvalidDepthUsesTypedJson()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes, ["route", "list", "--depth=-1", "--format=json"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("route-list.invalid-depth", document.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("next").ValueKind);
    }

}
