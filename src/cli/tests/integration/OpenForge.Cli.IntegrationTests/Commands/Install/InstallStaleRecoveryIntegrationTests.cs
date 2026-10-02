using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallStaleRecoveryIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install-stale-recovery"), Trait("Evidence", "Integration")]
    public async Task SupersededBytesDoNotBypassAnUnsafeTargetOrUntrustedDraft(bool draft)
    {
        using var workspace = InstallOperationWorkspace.Create("install-recovery-boundary");
        var cancellation = TestContext.Current.CancellationToken;
        var first = workspace.Combine("first.md");
        var second = workspace.Combine("second.md");
        var prior = "prior"u8.ToArray();
        workspace.WriteText("first.md", "prior");
        workspace.WriteText("second.md", "prior");
        var firstBefore = FileStateSnapshot.File(first, first, prior);
        var secondBefore = FileStateSnapshot.File(second, second, prior);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace.Workspace,
                InstallDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Install, workspace.Workspace),
                Guid.NewGuid(),
                [
                    RecoveryBundleTarget.Create(PlannedFileChange.Replace(firstBefore.Expectation, "intended"u8), firstBefore),
                    RecoveryBundleTarget.Create(PlannedFileChange.Replace(secondBefore.Expectation, "intended"u8), secondBefore),
                ]),
            cancellation);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var bundleBytes = await File.ReadAllBytesAsync(preparation.BundlePath, cancellation);
        await File.WriteAllTextAsync(first, "new authored state", cancellation);
        var retainedPath = preparation.BundlePath;
        if (draft)
        {
            var root = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None)
                ?? throw new InvalidOperationException("The prepared recovery store must exist.");
            retainedPath = RecoveryBundlePathIdentity.DraftPath(root, workspace.PhysicalPath, preparation.OperationId);
            File.Move(preparation.BundlePath, retainedPath);
        }
        else
        {
            File.Delete(second);
            Directory.CreateDirectory(second);
        }

        try
        {
            var beforeInstall = workspace.SnapshotHashes();
            var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
            var result = await operation.ExecuteAsync(workspace.Request(), cancellation);

            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.RecoveryConflict);
            Assert.Equal(beforeInstall, workspace.SnapshotHashes());
            Assert.Empty(result.Facts.Effects);
            Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(retainedPath, cancellation));
        }
        finally
        {
            if (!draft)
            {
                Directory.Delete(second);
                await File.WriteAllBytesAsync(second, prior, cancellation);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install-stale-recovery"), Trait("Evidence", "Integration")]
    public async Task ImmediateWorkspaceChangeInvalidatesAnOldRecoveryBlock(bool removeTarget)
    {
        using var workspace = InstallOperationWorkspace.Create("install-stale-recovery");
        var cancellation = TestContext.Current.CancellationToken;
        var target = workspace.Combine("AGENTS.md");
        var prior = "# Branch A\n\nOld authored context.\n"u8.ToArray();
        var intended = "# Branch A\n\nOld proposed replacement.\n"u8.ToArray();
        var current = "# Branch B\n\nCurrent authored context.\n"u8.ToArray();
        workspace.WriteText("AGENTS.md", System.Text.Encoding.UTF8.GetString(prior));
        var before = FileStateSnapshot.File(target, target, prior);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace.Workspace,
                InstallDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Install, workspace.Workspace),
                Guid.NewGuid(),
                [RecoveryBundleTarget.Create(PlannedFileChange.Replace(before.Expectation, intended), before)]),
            cancellation);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var bundleBytes = await File.ReadAllBytesAsync(preparation.BundlePath, cancellation);
        await File.WriteAllBytesAsync(target, intended, cancellation);
        var catalogue = await RecoveryBundleCatalogue.ReadAsync(workspace.Workspace, cancellation);
        var candidate = Assert.Single(catalogue.Candidates);
        var resolver = new PhysicalPathResolver();
        var oldObservation = await RecoveryEntrySetObserver.ReadAsync(resolver, workspace.Workspace, candidate, cancellation);
        Assert.Equal(RecoveryBundleTargetComparisonState.Intended, Assert.Single(oldObservation.Entries).State);

        if (removeTarget)
        {
            File.Delete(target);
        }
        else
        {
            await File.WriteAllBytesAsync(target, current, cancellation);
        }

        var currentObservation = await RecoveryEntrySetObserver.ReadAsync(resolver, workspace.Workspace, candidate, cancellation);
        Assert.Equal(RecoveryBundleTargetComparisonState.Third, Assert.Single(currentObservation.Entries).State);
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        var result = await operation.ExecuteAsync(workspace.Request(), cancellation);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, cancellation));
        if (!removeTarget)
        {
            var authored = await File.ReadAllTextAsync(target, cancellation);
            Assert.Contains("Current authored context.", authored, StringComparison.Ordinal);
            Assert.DoesNotContain("Old authored context.", authored, StringComparison.Ordinal);
        }

        var repeat = await operation.ExecuteAsync(workspace.Request(), cancellation);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Facts.Effects);
        Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, cancellation));
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install-stale-recovery"), Trait("Evidence", "Integration")]
    public async Task CompletedOrRolledBackRecoveryRemainsHistory(bool completed)
    {
        using var workspace = InstallOperationWorkspace.Create("install-resolved-recovery");
        var cancellation = TestContext.Current.CancellationToken;
        var target = workspace.Combine("previous-operation.md");
        var prior = "prior"u8.ToArray();
        var intended = "intended"u8.ToArray();
        workspace.WriteText("previous-operation.md", System.Text.Encoding.UTF8.GetString(prior));
        var before = FileStateSnapshot.File(target, target, prior);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace.Workspace,
                InstallDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Install, workspace.Workspace),
                Guid.NewGuid(),
                [RecoveryBundleTarget.Create(PlannedFileChange.Replace(before.Expectation, intended), before)]),
            cancellation);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var bundleBytes = await File.ReadAllBytesAsync(preparation.BundlePath, cancellation);
        var current = completed ? intended : prior;
        await File.WriteAllBytesAsync(target, current, cancellation);

        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        var result = await operation.ExecuteAsync(workspace.Request(), cancellation);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(current, await File.ReadAllBytesAsync(target, cancellation));
        Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, cancellation));
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install-stale-recovery"), Trait("Evidence", "Integration")]
    public async Task PartialRecoveryBlocksUntilItsCurrentSnapshotIsSuperseded(bool superseded)
    {
        using var workspace = InstallOperationWorkspace.Create("install-partial-recovery");
        var cancellation = TestContext.Current.CancellationToken;
        var first = workspace.Combine("first.md");
        var second = workspace.Combine("second.md");
        var prior = "prior"u8.ToArray();
        var intended = "intended"u8.ToArray();
        var authored = "new authored state"u8.ToArray();
        workspace.WriteText("first.md", System.Text.Encoding.UTF8.GetString(prior));
        workspace.WriteText("second.md", System.Text.Encoding.UTF8.GetString(prior));
        var firstBefore = FileStateSnapshot.File(first, first, prior);
        var secondBefore = FileStateSnapshot.File(second, second, prior);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                workspace.Workspace,
                InstallDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Install, workspace.Workspace),
                Guid.NewGuid(),
                [
                    RecoveryBundleTarget.Create(PlannedFileChange.Replace(firstBefore.Expectation, intended), firstBefore),
                    RecoveryBundleTarget.Create(PlannedFileChange.Replace(secondBefore.Expectation, intended), secondBefore),
                ]),
            cancellation);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var bundleBytes = await File.ReadAllBytesAsync(preparation.BundlePath, cancellation);
        await File.WriteAllBytesAsync(first, intended, cancellation);
        var secondCurrent = superseded ? authored : prior;
        await File.WriteAllBytesAsync(second, secondCurrent, cancellation);
        var beforeInstall = workspace.SnapshotHashes();

        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        var result = await operation.ExecuteAsync(workspace.Request(), cancellation);

        if (superseded)
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.RecoveryConflict);
            Assert.Equal(beforeInstall, workspace.SnapshotHashes());
            Assert.Empty(result.Facts.Effects);
        }

        Assert.Equal(intended, await File.ReadAllBytesAsync(first, cancellation));
        Assert.Equal(secondCurrent, await File.ReadAllBytesAsync(second, cancellation));
        Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, cancellation));
    }
}
