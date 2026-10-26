using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveRecoveryConflictCoordinateIntegrationTests
{
    private const string SourceReference = "guidance/application";
    private const string DestinationTarget = RouteMoveIntegrationWorkspace.ApplicationCategoryDestination;

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Move keeps an observed recovery candidate coordinate while unrelated work proceeds"), Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    [InlineData("valid-final")]
    [InlineData("draft")]
    [InlineData("malformed-final")]
    public async Task ExistingRecoveryCandidateIsWarnedWithoutBlocking(
        string candidateKind)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            $"route-move-recovery-coordinate-{candidateKind}");
        _ = await workspace.BuildApplicationPlanAsync();
        var candidate = await SeedCandidateAsync(workspace.Workspace, candidateKind);
        workspace.TrackRecoveryPath(candidate.Path);
        var before = workspace.SnapshotHashes();
        try
        {
            var result = await RouteMoveOperationFactory.Create(workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.Request(
                        SourceReference,
                        DestinationTarget,
                        RouteMoveMode.Apply),
                    TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(0, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
            Assert.DoesNotContain(result.Findings, item => item.Code == RouteMoveFindingCode.RecoveryConflict);
            var report = RouteMoveReportSelector.Select(result, new CliSelection(CliDetail.Standard, null));
            if (candidateKind == "valid-final")
            {
                Assert.Empty(result.RecoveryHistory.Paths);
                Assert.DoesNotContain(report.Findings, item => item.Subject.Path == candidate.Path);
            }
            else
            {
                Assert.Contains(candidate.Path, result.RecoveryHistory.Paths);
                Assert.Contains(report.Findings, item => item.Code == RouteMoveDefinitions.RecoveryHistoryCode
                    && item.Subject.Path == candidate.Path && item.Severity == CliSeverity.Warning);
            }
            Assert.NotEqual(candidate.Path, result.Recovery.ResidualPath);
            Assert.NotEmpty(result.Effects);
            Assert.Equal(RouteMoveVerificationState.Verified, result.Verification);
            Assert.Equal(
                candidate.Bytes,
                await File.ReadAllBytesAsync(candidate.Path, TestContext.Current.CancellationToken));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(candidate.Path);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move prepares its own bundle while preserving history added after planning"), Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task HeldLeasePreparationPreservesLaterHistory()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-coordinate-held-lease");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var candidate = await SeedCandidateAsync(workspace.Workspace, "valid-final");
        workspace.TrackRecoveryPath(candidate.Path);
        var before = workspace.SnapshotHashes();

        var preparation = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveHeldApplication(plan, operationId, lease),
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveRecoveryPreparationState.Prepared, preparation.State);
        Assert.NotNull(preparation.Preparation);
        Assert.NotEqual(candidate.Path, preparation.Preparation.BundlePath);
        RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.Preparation.BundlePath);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            candidate.Bytes,
            await File.ReadAllBytesAsync(candidate.Path, TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Managed")]
    [Theory(DisplayName = "Route Move maps stored blocked preparation only when a residual coordinate exists"), Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StoredBlockedPreparationUsesOnlyItsObservedResidualPath(bool hasResidualPath)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-stored-blocked-coordinate");
        var plan = await workspace.BuildApplicationPlanAsync();
        var before = workspace.SnapshotHashes();
        var cause = "The recovery archive publication failed after candidate inspection.";
        var failure = new FilesystemFailure(
            FilesystemFailureKind.InputOutput,
            "The recovery archive could not be published.");
        string? candidatePath = null;
        if (hasResidualPath)
        {
            candidatePath = Path.Combine(
                Path.GetTempPath(),
                "task64-observed-recovery-candidate",
                "move",
                "observed.bundle");
        }

        var stored = RecoveryBundlePreparationResult.Blocked(cause, candidatePath, failure);
        var mapped = RouteMoveRecoveryLifecycle.FromStored(plan, stored);
        var progress = RouteMoveApplicationProgressProjector.PreparationBoundary(plan, mapped);
        var finding = Assert.Single(progress.Findings);

        Assert.Same(failure, stored.Failure);
        Assert.Equal(cause, stored.Cause);
        Assert.Null(mapped.Preparation);
        Assert.Equal(candidatePath, mapped.Recovery.ResidualPath);
        Assert.Equal(cause, finding.Cause);

        if (hasResidualPath)
        {
            Assert.Equal(RouteMoveRecoveryPreparationState.Blocked, mapped.State);
            Assert.Equal(RouteMoveFindingCode.RecoveryConflict, finding.Code);
            Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
            Assert.Equal(candidatePath, finding.Target);
        }
        else
        {
            Assert.Equal(RouteMoveRecoveryPreparationState.Incomplete, mapped.State);
            Assert.Equal(RouteMoveFindingCode.RecoveryUnavailable, finding.Code);
            Assert.Equal(CliSemanticStatus.Incomplete, finding.Status);
            Assert.Null(mapped.Recovery.ResidualPath);
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async ValueTask<(string Path, byte[] Bytes)> SeedCandidateAsync(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace,
        string candidateKind)
    {
        if (candidateKind == "valid-final")
        {
            var payloadPath = Path.Combine(
                workspace.PhysicalRoot,
                RouteMoveIntegrationWorkspace.ApplicationCategoryPath.Replace('/', Path.DirectorySeparatorChar));
            var payloadBytes = await File.ReadAllBytesAsync(
                payloadPath,
                TestContext.Current.CancellationToken);
            var snapshot = FileStateSnapshot.File(payloadPath, payloadPath, payloadBytes);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(
                    workspace,
                    RouteMoveDefinitions.CommandIdentity,
                    RecoveryBundleAttribution.Create(
                        RecoveryBundleProducer.Route,
                        RecoveryBundleOperation.Move,
                        workspace),
                    Guid.NewGuid(),
                    [RecoveryBundleTarget.Create(
                        PlannedFileChange.Delete(snapshot.Expectation),
                        snapshot)]),
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            return (
                preparation.BundlePath,
                await File.ReadAllBytesAsync(
                    preparation.BundlePath,
                    TestContext.Current.CancellationToken));
        }

        var storeRoot = TestDataHome.LocalRecoveryStoreRoot(workspace.PhysicalRoot);
        var operationId = Guid.NewGuid();
        string path;
        if (candidateKind == "draft")
        {
            path = RecoveryBundlePathIdentity.DraftPath(storeRoot, workspace.PhysicalRoot, operationId);
        }
        else if (candidateKind == "malformed-final")
        {
            path = RecoveryBundlePathIdentity.FinalPath(storeRoot, workspace.PhysicalRoot, operationId);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(candidateKind), candidateKind, null);
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        byte[] bytes;
        if (candidateKind == "draft")
        {
            bytes = "incomplete recovery draft"u8.ToArray();
        }
        else if (candidateKind == "malformed-final")
        {
            bytes = "not a recovery ZIP"u8.ToArray();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(candidateKind), candidateKind, null);
        }
        await File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
        return (path, bytes);
    }
}
