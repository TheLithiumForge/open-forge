using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

public sealed partial class RouteInclusionRecoveryHistoryIntegrationTests
{
    [Theory(DisplayName = "Unavailable recovery prevents every folder effect")]
    [InlineData(false), InlineData(true)]
    public async Task FolderRecoveryPreparationFailure(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var root = WorkspaceRuntimePaths.RecoveryRoot(workspace.Workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(root)!);
        await File.WriteAllTextAsync(root, "Blocked recovery storage", Token);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        var before = workspace.Files.SnapshotHashes();
        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, Token);
        Assert.Equal(CliSemanticStatus.Incomplete, outcome.Status);
        Assert.Equal(RouteInclusionFindingCode.RecoveryUnavailable, outcome.Code);
        Assert.All(outcome.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.False(File.Exists(workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue)));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "An interrupted folder apply retains truthful unstarted effects")]
    [InlineData(false), InlineData(true)]
    public async Task FolderInterruptedBeforeEffects(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        var before = workspace.Files.SnapshotHashes();
        using var interrupted = new CancellationTokenSource();
        interrupted.Cancel();
        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, interrupted.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, outcome.Status);
        Assert.All(outcome.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Null(outcome.RecoveryPath);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "Partial folder application retains complete verified recovery and actual effect receipts")]
    [InlineData(false), InlineData(true)]
    public async Task FolderPartialFailure(bool custom)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        var loader = workspace.Bytes(RouteInclusionFolderFixture.Loader(workspace));
        var settings = workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath);
        using var held = File.Open(workspace.Files.Combine(RouteInclusionFolderFixture.Loader(workspace)), FileMode.Open, FileAccess.Read, FileShare.Read);
        var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, Token);
        Assert.Equal(CliSemanticStatus.Failed, outcome.Status);
        Assert.False(outcome.Verified);
        Assert.Equal(RouteInclusionEffectState.Verified, outcome.Effects[0].State);
        Assert.All(outcome.Effects.Skip(1), effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Equal(plan.Changes[0].IntendedBytes.ToArray(), workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
        Assert.Equal(loader, workspace.Bytes(RouteInclusionFolderFixture.Loader(workspace)));
        Assert.Equal(settings, workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath));
        Assert.Equal(RouteInclusionRecoveryState.Retained, outcome.Recovery);
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(workspace.Workspace, Token);
        var candidate = Assert.Single(catalogue.Candidates);
        Assert.Equal(outcome.RecoveryPath, candidate.Path);
        var verified = Assert.IsType<RecoveryBundleVerifiedRead>(candidate.Verified);
        Assert.Equal(plan.Changes.Select(change => Path.GetRelativePath(workspace.Files.Path, change.LogicalPath).Replace('\\', '/')),
            verified.Entries.Select(entry => entry.TargetPath));
    }
}
