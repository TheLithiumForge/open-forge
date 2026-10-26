using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed partial class RouteIncludeApplicationTests
{
    [Fact(DisplayName = "Folder Include creates absent settings with prepared recovery and converges"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    public async Task FolderAbsentSettings()
    {
        using var workspace = RouteInclusionFolderFixture.Create(false);
        File.Delete(workspace.Files.Combine(WorkspaceSettingsDefinitions.RelativePath));
        var request = RouteInclusionFolderFixture.Request(workspace);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(request, RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        var settings = Assert.Single(plan.RecoveryTargets, item => item.Change.LogicalPath == workspace.Files.Combine(WorkspaceSettingsDefinitions.RelativePath));
        Assert.True(settings.RequiresRecovery);
        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Complete, outcome.Status);
        Assert.True(outcome.Verified);
        Assert.Equal(RouteInclusionRecoveryState.Removed, outcome.Recovery);
        Assert.Equal([RouteInclusionFolderFixture.Folder], RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        Assert.True((await new RouteInclusionPlanBuilder().BuildAsync(request, RouteInclusionFolderFixture.Token)).Outcome.NoOp);
    }

    [Theory(DisplayName = "Folder application compares intended registrations and retained initial expectations"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task FolderCompletePlanEquivalence(bool loseInitialFacts)
    {
        using var workspace = RouteInclusionFolderFixture.Create(false);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        var changed = loseInitialFacts ? plan with { Target = plan.Target with { Expectations = [] } }
            : plan with { IntendedLayout = plan.IntendedLayout.WithOutsideRoutes([]) };
        var before = workspace.Files.SnapshotHashes();
        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(changed, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Blocked, outcome.Status);
        Assert.Equal(RouteInclusionFindingCode.TargetChanged, outcome.Code);
        Assert.All(outcome.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Folder Include applies its complete intended layout and becomes a verified no-op"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task FolderApplyAndNoOp(bool custom, bool anchor)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, anchor);
        var anchorBytes = anchor ? workspace.Bytes(RouteInclusionFolderFixture.Catalogue) : null;
        var request = RouteInclusionFolderFixture.Request(workspace);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(request, RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Complete, outcome.Status);
        Assert.True(outcome.Verified);
        Assert.Equal(RouteInclusionRecoveryState.Removed, outcome.Recovery);
        Assert.Null(outcome.RecoveryPath);
        Assert.All(outcome.Effects, effect => Assert.Equal(RouteInclusionEffectState.Verified, effect.State));
        Assert.Equal([RouteInclusionFolderFixture.Folder], RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).Roots);
        Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).AllowInstallPaths);
        if (anchorBytes is not null) Assert.Equal(anchorBytes, workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
        var after = workspace.Files.SnapshotHashes();
        var repeated = await new RouteInclusionPlanBuilder().BuildAsync(request, RouteInclusionFolderFixture.Token);
        Assert.True(repeated.Outcome.NoOp);
        Assert.True(repeated.Outcome.Verified);
        Assert.Empty(repeated.Outcome.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes());
        Assert.Equal(plan.IntendedLayout.OutsideRoutes, repeated.Plan!.IntendedLayout.OutsideRoutes);
    }

    [Theory(DisplayName = "Folder application retains initial settings, directory and anchor facts before any effect"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, 0), InlineData(false, 1), InlineData(false, 2)]
    [InlineData(true, 0), InlineData(true, 1), InlineData(true, 2)]
    public async Task FolderStaleFacts(bool custom, int drift)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        if (drift == 0) workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            workspace.Read(WorkspaceSettingsDefinitions.RelativePath) + "\n ");
        if (drift == 1) Directory.Delete(workspace.Files.Combine(RouteInclusionFolderFixture.Folder));
        if (drift == 2) workspace.Files.WriteText(RouteInclusionFolderFixture.Catalogue, RouteInclusionFolderFixture.AuthoredCatalogue);
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInclusionFindingCode.TargetChanged, result.Code);
        Assert.All(result.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.Null(result.RecoveryPath);
    }

    [Theory(DisplayName = "An omitted registered folder row stays omitted until explicit Include restores it"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task FolderExplicitReinclude(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, true);
        var operation = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder };
        Assert.True((await operation.ExecuteAsync(request, RouteInclusionFolderFixture.Token)).Outcome.Verified);
        RouteInclusionFolderFixture.Rows(workspace, MarkdownEntriesSectionReader.EmptyEntry);
        var bytes = workspace.Bytes(RouteInclusionFolderFixture.Catalogue);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        Assert.True(build.Outcome.RegisteredBefore);
        Assert.Equal(RouteInclusionEffectKind.LoaderEntries, Assert.Single(build.Outcome.Effects).Kind);
        Assert.True((await operation.ExecuteAsync(request, RouteInclusionFolderFixture.Token)).Outcome.Verified);
        var layout = build.Plan!.IntendedLayout;
        Assert.Contains(RouteInclusionFolderFixture.Catalogue,
            SourceLoaderEntriesParser.Parse(layout, workspace.Read(layout.LoaderPath)).Destinations.Select(item => item.CanonicalPath));
        Assert.Equal(bytes, workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
    }
}
