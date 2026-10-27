using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed partial class RouteExcludePlanningTests
{
    [Theory(DisplayName = "Folder Exclude preview removes every matching row and only its directory registration"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task FolderPreviewKeepsUnrelatedState(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, true);
        workspace.Files.CreateDirectory(RouteInclusionFolderFixture.OtherFolder);
        var bytes = workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath);
        bytes = WorkspaceSettingsCodec.AddOutsideRoute(bytes, RouteInclusionFolderFixture.Folder)!;
        bytes = WorkspaceSettingsCodec.AddOutsideRoute(bytes, RouteInclusionFolderFixture.OtherFolder)!;
        bytes = WorkspaceSettingsCodec.AddRoot(bytes, RouteInclusionFolderFixture.Catalogue)!;
        workspace.Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath, bytes);
        var destination = RouteInclusionFolderFixture.Destination(workspace);
        RouteInclusionFolderFixture.Rows(workspace, $"- [Before](../before.md) - #Docs\n- [One]({destination}) - #Docs\n- [Two]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag}\n- [After](../after.md) - #Other");
        var before = workspace.Files.SnapshotHashes();
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace, exclude: true) with { DryRun = true }, RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        Assert.Equal(CliSemanticStatus.Complete, build.Outcome.Status);
        Assert.True(build.Outcome.RegisteredBefore);
        Assert.False(build.Outcome.RegisteredAfter);
        Assert.Null(build.Outcome.After);
        Assert.Equal(RouteInclusionFolderFixture.OtherFolder, Assert.Single(plan.IntendedLayout.OutsideRoutes).DirectoryPath);
        Assert.Equal(2, build.Outcome.Effects.Length);
        var loader = Assert.Single(build.Outcome.Effects, effect => effect.Kind == RouteInclusionEffectKind.LoaderEntries);
        var destinations = SourceLoaderEntriesParser.Parse(plan.IntendedLayout, loader.After).Destinations;
        Assert.Equal(2, destinations.Count);
        Assert.DoesNotContain(destinations, row => row.CanonicalPath == RouteInclusionFolderFixture.Catalogue);
        var settings = Assert.Single(plan.Changes, change => change.LogicalPath == workspace.Files.Combine(WorkspaceSettingsDefinitions.RelativePath));
        Assert.Equal([RouteInclusionFolderFixture.Catalogue], WorkspaceSettingsCodec.Read(settings.IntendedBytes.AsMemory()).Document!.Roots);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }
}
