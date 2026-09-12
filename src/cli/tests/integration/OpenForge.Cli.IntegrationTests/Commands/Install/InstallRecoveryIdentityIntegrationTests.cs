using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallRecoveryIdentityIntegrationTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedAttribution(bool changed)
    {
        using var temporary = TemporaryWorkspace.Create("install-recovery-identity");
        var workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var target = temporary.CreateFile("target.md", "prior"u8.ToArray());
        var before = FileStateSnapshot.File(logicalPath: target, physicalPath: target, bytes: "prior"u8);
        var change = PlannedFileChange.Replace(before.Expectation, "intended"u8);
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create("install-recovery-identity-locks");
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, InstallDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace: workspace,
                command: InstallDefinitions.CommandIdentity,
                attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Install, workspace),
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

            var result = await InstallRecoveryOperation.CleanupAsync(
                new InstallRecoveryCleanupRequest { Workspace = workspace, Lease = lease, Preparation = preparation },
                TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(InstallRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(InstallFindingCode.RecoveryFailed, result.Finding?.Code);
                Assert.Null(result.Recovery.ResidualPath);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(InstallRecoveryState.Removed, result.Recovery.State);
                Assert.Null(result.Finding?.Code);
                Assert.Null(result.Recovery.ResidualPath);
            }
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
