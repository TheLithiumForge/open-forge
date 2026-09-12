using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateRecoveryIdentityIntegrationTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedAttribution(bool changed)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-recovery-identity");
        var plan = Assert.IsType<RouteUpdatePlan>(
            (await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request())).Plan);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteUpdateRecoveryPreparer.PrepareAsync(
            new RouteUpdateRecoveryPreparationInput { Plan = plan, OperationId = operationId.ToString("D") },
            TestContext.Current.CancellationToken);
        Assert.Equal(RouteUpdateRecoveryPreparationState.Prepared, prepared.State);
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

            var result = await RouteUpdateRecoveryCompleter.CompleteAsync(
                new RouteUpdateRecoveryCompletionInput { Plan = plan, Preparation = preparation, Lease = lease },
                TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(RouteUpdateRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(RouteUpdateFindingCode.RecoveryFailed, result.FindingCode);
                Assert.Null(result.Recovery.ResidualPath);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(RouteUpdateRecoveryState.Removed, result.Recovery.State);
                Assert.Null(result.FindingCode);
                Assert.Null(result.Recovery.ResidualPath);
            }
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
