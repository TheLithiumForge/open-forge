using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;
using static OpenForge.Cli.EndToEndTests.Shared.PublishedProcess.PublishedProcessTestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteListProcessTests
{
    private const string DefaultRoute = "root";
    private const string WorkspaceDefinedRoute = "workspace-defined";
    private const string DefaultCatalogue = PublishedTokens.Sources.AgentsRoot + "/" + DefaultRoute + "/_" + DefaultRoute + PublishedTokens.Layout.MarkdownExtension;
    private const string ZeroDepth = "0";
    private const string InvalidDepth = "-1";

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
            [PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);

        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, result.ExitCode);
        Assert.Empty(result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.RouteList.CommandIdentity, document.RootElement.GetProperty(PublishedTokens.Json.Command).GetString());
        Assert.Equal(PublishedTokens.Statuses.Complete, document.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        var resultBody = document.RootElement.GetProperty(PublishedTokens.Json.Data);
        Assert.Equal(PublishedTokens.RouteList.DefaultDepth, resultBody.GetProperty(PublishedTokens.RouteListData.Depth).GetInt32());
        Assert.Contains(resultBody.GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray(), row => row.GetProperty(PublishedTokens.Json.Id).GetString() == WorkspaceDefinedRoute);
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published Route List selects one exact path at depth zero in compact view"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListExactPathUsesZeroDepthCompactView()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            [PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, DefaultCatalogue,
                PublishedTokens.RouteList.Depth, ZeroDepth, PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.MinimalDetail]);

        Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, result.ExitCode);
        Assert.Empty(result.StandardError);
        CommandOutputSnapshot.MatchSnapshot(result.StandardOutput, nameof(PublishedRouteListExactPathUsesZeroDepthCompactView));
    }

    [Fact(DisplayName = "Published Route List rejects invalid depth with typed JSON and no writes"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListInvalidDepthUsesTypedJson()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes,
            [PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, PublishedTokens.RouteList.Depth, InvalidDepth,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);

        Assert.Equal(PublishedTokens.Statuses.InvalidExitCode, result.ExitCode);
        Assert.Empty(result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Invalid, document.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
        Assert.Equal(PublishedTokens.RouteList.InvalidDepthCode, document.RootElement.GetProperty(PublishedTokens.Json.Findings)[0].GetProperty(PublishedTokens.Json.Code).GetString());
        CommandOutputSnapshot.MatchSnapshot(CommandOutputNormalization.ReplaceDelimitedPath(result.StandardOutput, workspace.Path, "<workspace>"),
            nameof(PublishedRouteListInvalidDepthUsesTypedJson));
    }

}
