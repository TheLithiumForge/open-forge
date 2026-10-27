using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Presentation.Route.Include;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed partial class RouteIncludeOutputSnapshotTests
{
    [Fact(DisplayName = "Folder Include preview, application, no-op and stale failure retain text and JSON facts"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    public async Task FolderJourney()
    {
        using var workspace = RouteInclusionFolderFixture.Create(false);
        var token = RouteInclusionFolderFixture.Token;

        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder };
        var operation = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var renderers = CommandOutputRenderers<RouteIncludeResult>.From(RouteIncludePresentation.Rendering,
            normalizeText: text => text.Replace("\\r\\n", "\\n", StringComparison.Ordinal),
            normalizeJson: text => text.Replace("\\r\\n", "\\n", StringComparison.Ordinal));
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        var authored = workspace.Bytes("guide.md");
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var preview = await operation.ExecuteAsync(request with { DryRun = true }, token);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        renderers.MatchDetails(preview, "folder-preview", snapshotCollector: captures);
        var applied = await operation.ExecuteAsync(request, token);
        Assert.True(applied.Outcome.Verified);
        Assert.Equal([RouteInclusionFolderFixture.Folder], RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).Roots);
        Assert.Equal(authored, workspace.Bytes("guide.md"));
        renderers.MatchDetails(applied, "folder-applied", snapshotCollector: captures);
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeated = await operation.ExecuteAsync(request, token);
        Assert.True(repeated.Outcome.NoOp);
        Assert.True(repeated.Outcome.Verified);
        Assert.Empty(repeated.Outcome.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
        renderers.MatchDetails(repeated, "folder-no-op", snapshotCollector: captures);

        var planned = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace, false) with { Description = "Replacement policy" }, token);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Read(WorkspaceSettingsDefinitions.RelativePath) + "\n ");
        var stale = workspace.Files.SnapshotHashes(includeRuntime: true);
        var failed = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(Assert.IsType<RouteInclusionPlan>(planned.Plan), token);
        Assert.Equal(CliSemanticStatus.Blocked, failed.Status);
        Assert.Equal(stale, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.Equal(authored, workspace.Bytes("guide.md"));
        renderers.MatchDetails(new RouteIncludeResult { Workspace = workspace.Workspace, Requested = request.File, Outcome = failed }, "folder-failure", snapshotCollector: captures);
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }
}
