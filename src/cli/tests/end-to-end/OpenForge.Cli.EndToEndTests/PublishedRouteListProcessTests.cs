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
            ["route", "list", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("route list", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var resultBody = document.RootElement.GetProperty("result");
        Assert.Equal(1, resultBody.GetProperty("requestedDepth").GetInt32());
        Assert.Contains(resultBody.GetProperty("rows").EnumerateArray(), row => row.GetProperty("id").GetString() == "workspace-defined");
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published Route List selects one exact path at depth zero in compact view"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListExactPathUsesZeroDepthCompactView()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            ["route", "list", ".agents/root/_root.md", "--depth=0", "--view=compact"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("result=complete  coverage=complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("requestedDepth=0 effectiveDepth=0", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("root  .agents/root/_root.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("root/child", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route List rejects invalid depth with typed JSON and no writes"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListInvalidDepthUsesTypedJson()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes, ["route", "list", "--depth=-1", "--json"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("route-list.invalid-depth", document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal("open-forge route list --help", document.RootElement.GetProperty("next").GetProperty("command").GetString());
    }

}
