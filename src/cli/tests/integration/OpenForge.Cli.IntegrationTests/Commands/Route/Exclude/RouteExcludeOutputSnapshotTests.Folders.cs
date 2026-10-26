using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Presentation.Route.Exclude;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed partial class RouteExcludeOutputSnapshotTests
{
    [Fact(DisplayName = "Folder Exclude preview, application, no-op and stale failure retain text and JSON facts"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    public async Task FolderJourney()
    {
        using var workspace = RouteInclusionFolderFixture.Create(false);
        var token = RouteInclusionFolderFixture.Token;
        Assert.True((await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder }, token)).Outcome.Verified);
        var request = new RouteExcludeRequest { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder };
        var operation = new RouteExcludeOperation(workspace.Locks.StoreRoot);
        var renderers = CommandOutputRenderers<RouteExcludeResult>.From(RouteExcludePresentation.Rendering);
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        var before = workspace.Files.SnapshotHashes();
        var preview = await operation.ExecuteAsync(request with { DryRun = true }, token);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        renderers.MatchDetails(preview, "folder-preview", snapshotCollector: captures);
        var applied = await operation.ExecuteAsync(request, token);
        Assert.True(applied.Outcome.Verified);
        renderers.MatchDetails(applied, "folder-applied", snapshotCollector: captures);
        var repeated = await operation.ExecuteAsync(request, token);
        Assert.True(repeated.Outcome.NoOp);
        renderers.MatchDetails(repeated, "folder-no-op", snapshotCollector: captures);
        Assert.True((await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder }, token)).Outcome.Verified);
        var planned = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace, true), token);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Read(WorkspaceSettingsDefinitions.RelativePath) + "\n ");
        var failed = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(Assert.IsType<RouteInclusionPlan>(planned.Plan), token);
        Assert.Equal(CliSemanticStatus.Blocked, failed.Status);
        renderers.MatchDetails(new RouteExcludeResult { Workspace = workspace.Workspace, Requested = request.File, Outcome = failed }, "folder-failure", snapshotCollector: captures);
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }
}
