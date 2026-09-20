using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallRecoveryIdentityIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedAttribution(bool changed)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-recovery-identity");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-recovery-identity-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", OpenForgeDocumentSeed.Metadata("Toolkit", ["Extension"], "# Toolkit\n")));
        workspace.CreateOccupant(".agents/toolkit.md", "plain occupant\n");
        var resolver = new PhysicalPathResolver();
        var planBuild = await new ExtensionInstallPlanner(
            ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionInstallWording.Selection(),
            ExtensionInteractionTestFactory.UnavailableInstallConfirmation,
            static paths => new CliConfirmQuestion(ExtensionInstallWording.ReplaceExisting(paths)),
            resolver).BuildAsync(
                new ExtensionInstallRequest(
                    workspace.Workspace,
                    ExtensionInstallMode.Apply,
                    ["toolkit"],
                    all: false,
                    source.Path,
                    force: true,
                    automatic: true,
                    allowInteraction: false),
                TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionInstallPlan>(planBuild.Plan);
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create("extension-install-recovery-identity-locks");
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, ExtensionInstallDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var prepared = await ExtensionInstallRecoveryOperation.PrepareAsync(
            new ExtensionInstallExecutionPlan(plan, new(
                Observation: null,
                Result: WorkspacePermissionResult.NotEvaluated with { Decision = WorkspacePermissionDecision.NotRequired },
                Change: null,
                RecoveryTarget: null,
                Failure: null)),
            operationId,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var candidateSnapshotKey = $"recovery-candidate:{preparation.BundlePath}";
            var workspaceBefore = workspace.Snapshot().Where(item => item.Key != candidateSnapshotKey).ToArray();
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
            Assert.Equal(workspaceBefore, workspace.Snapshot().Where(item => item.Key != candidateSnapshotKey));

            var result = await ExtensionInstallRecoveryOperation.CleanupAsync(
                new ExtensionInstallRecoveryCleanupRequest(plan, lease, preparation),
                TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            Assert.Equal(workspaceBefore, workspace.Snapshot().Where(item => item.Key != candidateSnapshotKey));
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(ExtensionInstallRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(ExtensionInstallFindingCode.RecoveryFailed, result.Finding?.Code);
                Assert.Null(result.Recovery.ResidualPath);
                Assert.Equal(preparation.Entries.OrderBy(entry => entry.Ordinal).Select(entry => entry.TargetPath), result.Recovery.ProtectedPaths);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(ExtensionInstallRecoveryState.Removed, result.Recovery.State);
                Assert.Null(result.Finding);
                Assert.Null(result.Recovery.ResidualPath);
                Assert.Equal(preparation.Entries.OrderBy(entry => entry.Ordinal).Select(entry => entry.TargetPath), result.Recovery.ProtectedPaths);
            }
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
