using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveRecoveryIdentityIntegrationTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedAttribution(bool changed)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-recovery-identity");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var workspaceBefore = workspace.SnapshotHashes();
            var observed = await RecoveryFinalIdentityFixture.ObserveAsync(workspace.Workspace, preparation);
            var changedAttribution = RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair, workspace.Workspace);
            Assert.NotEqual(preparation.Attribution, changedAttribution);
            if (changed)
            {
                await observed.ChangeAttributionAsync(changedAttribution);
            }

            var bundleBytes = await observed.AssertAdmissionAsync(
                preparation.Command,
                changed ? changedAttribution : preparation.Attribution);
            Assert.Equal(workspaceBefore, workspace.SnapshotHashes());

            var result = await RouteMoveRecoveryLifecycle.DeleteExactAsync(
                new RouteMoveRecoveryDeletionInput
                {
                    Held = new RouteMoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
                    Preparation = preparation,
                },
                TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(RouteMoveRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(RouteMoveFindingCode.RecoveryFailed, result.Finding?.Code);
                Assert.Null(result.Recovery.ResidualPath);
                Assert.Equal(CliSemanticStatus.Failed, result.Finding?.Status);
                Assert.Equal(plan.Preview.Recovery.ProtectedPaths, result.Recovery.ProtectedPaths);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(RouteMoveRecoveryState.Removed, result.Recovery.State);
                Assert.Null(result.Finding?.Code);
                Assert.Null(result.Recovery.ResidualPath);
                Assert.Equal(plan.Preview.Recovery.ProtectedPaths, result.Recovery.ProtectedPaths);
            }
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
