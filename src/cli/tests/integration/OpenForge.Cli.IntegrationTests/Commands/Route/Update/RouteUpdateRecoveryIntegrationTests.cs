using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateRecoveryIntegrationTests
{
    [Fact(DisplayName = "Route Update recovery prepares one external bundle and rejects deterministic collision")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task RecoveryCollisionStopsBeforeApplication()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-recovery-collision");
        var build = await workspace.BuildPlanAsync(workspace.Request());
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        var recovery = RouteUpdateIntegrationWorkspace.CreateRecoveryServices();
        var input = new RouteUpdateRecoveryPreparationInput
        {
            Plan = plan,
            OperationId = Guid.NewGuid().ToString("D"),
        };

        var prepared = await recovery.Preparer.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundlePreparation>(
            prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var collision = await recovery.Preparer.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteUpdateRecoveryPreparationState.Prepared, prepared.State);
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, prepared.Recovery.State);
        Assert.Equal(RouteUpdateRecoveryPreparationState.Blocked, collision.State);
        Assert.Equal(RouteUpdateRecoveryState.Retained, collision.Recovery.State);
        Assert.Equal(preparation.BundlePath, collision.Recovery.ResidualPath);
    }

    [Fact(DisplayName = "Route Update completion retains a known recovery artifact when interrupted")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task InterruptedCleanupReportsRetainedArtifact()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-recovery-retained");
        var plan = Assert.IsType<RouteUpdatePlan>(
            (await workspace.BuildPlanAsync(workspace.Request())).Plan);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var recovery = RouteUpdateIntegrationWorkspace.CreateRecoveryServices();
        var prepared = await recovery.Preparer.PrepareAsync(
            new RouteUpdateRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId.ToString("D"),
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundlePreparation>(
            prepared.Preparation);
        workspace.TrackRecovery(preparation);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var completion = await recovery.Completer.CompleteAsync(
            new RouteUpdateRecoveryCompletionInput
            {
                Plan = plan,
                Preparation = preparation,
                Lease = lease,
            },
            cancellation.Token);

        Assert.Equal(RouteUpdateRecoveryState.Retained, completion.Recovery.State);
        Assert.Equal(preparation.BundlePath, completion.Recovery.ResidualPath);
        Assert.Equal(RouteUpdateFindingCode.RecoveryArtifactRetained, completion.FindingCode);
        Assert.True(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Update completion reports unknown recovery disposition when identity changes")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task ChangedRecoveryIdentityIsUnknown()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-recovery-unknown");
        var plan = Assert.IsType<RouteUpdatePlan>(
            (await workspace.BuildPlanAsync(workspace.Request())).Plan);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var recovery = RouteUpdateIntegrationWorkspace.CreateRecoveryServices();
        var prepared = await recovery.Preparer.PrepareAsync(
            new RouteUpdateRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId.ToString("D"),
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundlePreparation>(
            prepared.Preparation);
        workspace.ReplaceRecoveryWithDirectory(preparation);

        var completion = await recovery.Completer.CompleteAsync(
            new RouteUpdateRecoveryCompletionInput
            {
                Plan = plan,
                Preparation = preparation,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteUpdateRecoveryState.Unknown, completion.Recovery.State);
        Assert.Null(completion.Recovery.ResidualPath);
        Assert.Equal(RouteUpdateFindingCode.RecoveryFailed, completion.FindingCode);
    }
}
