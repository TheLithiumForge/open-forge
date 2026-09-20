using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

public sealed class RouteInitRecoveryIdentityIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedAttribution(bool changed)
    {
        using var temporary = TemporaryWorkspace.Create("route-init-recovery-identity");
        var workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var target = temporary.CreateFile("target.md", "prior"u8.ToArray());
        var before = FileStateSnapshot.File(logicalPath: target, physicalPath: target, bytes: "prior"u8);
        var change = PlannedFileChange.Replace(before.Expectation, "intended"u8);
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create("route-init-recovery-identity-locks");
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, RouteInitDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace: workspace,
                command: RouteInitDefinitions.CommandIdentity,
                attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Route, RecoveryBundleOperation.Init, workspace),
                operationId: operationId,
                targets: [RecoveryBundleTarget.Create(change, before)]),
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var observed = await RecoveryFinalIdentityFixture.ObserveAsync(workspace, preparation);
            var changedAttribution = RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair, workspace);
            Assert.NotEqual(preparation.Attribution, changedAttribution);
            if (changed)
            {
                await observed.ChangeAttributionAsync(changedAttribution);
            }

            var bundleBytes = await observed.AssertAdmissionAsync(
                preparation.Command,
                changed ? changedAttribution : preparation.Attribution);

            var result = await RouteInitRecoveryLifecycle.DeleteAsync(lease, preparation, TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(RouteInitRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(RouteInitFindingCode.RecoveryFailed, result.FindingCode);
                Assert.Equal(preparation.BundlePath, result.Recovery.ResidualPath);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
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
