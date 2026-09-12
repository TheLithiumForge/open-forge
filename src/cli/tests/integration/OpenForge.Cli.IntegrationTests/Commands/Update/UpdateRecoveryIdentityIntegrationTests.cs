using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdateRecoveryIdentityIntegrationTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedCommand(bool changed)
    {
        using var temporary = TemporaryWorkspace.Create("update-recovery-identity");
        var workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var target = temporary.CreateFile("target.md", "prior"u8.ToArray());
        var before = FileStateSnapshot.File(logicalPath: target, physicalPath: target, bytes: "prior"u8);
        var change = PlannedFileChange.Replace(before.Expectation, "intended"u8);
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create("update-recovery-identity-locks");
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, UpdateDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace: workspace,
                command: UpdateDefinitions.CommandIdentity,
                attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Update, workspace),
                operationId: operationId,
                targets: [RecoveryBundleTarget.Create(change, before)]),
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var observed = await RecoveryFinalIdentityFixture.ObserveAsync(workspace, preparation);
            const string changedCommand = "index";
            Assert.NotEqual(changedCommand, preparation.Command);
            if (changed)
            {
                await observed.ChangeCommandAsync(changedCommand);
            }

            var bundleBytes = await observed.AssertAdmissionAsync(
                changed ? changedCommand : preparation.Command,
                preparation.Attribution);
            string[] protectedPaths = ["target.md"];

            var result = await UpdateRecoveryOperation.CleanupAsync(lease, preparation, protectedPaths, TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            Assert.Equal(protectedPaths, result.Recovery.ProtectedPaths);
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(UpdateRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(UpdateFindingCode.RecoveryFailed, result.Finding?.Code);
                Assert.Null(result.Recovery.ResidualPath);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(UpdateRecoveryState.Removed, result.Recovery.State);
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
