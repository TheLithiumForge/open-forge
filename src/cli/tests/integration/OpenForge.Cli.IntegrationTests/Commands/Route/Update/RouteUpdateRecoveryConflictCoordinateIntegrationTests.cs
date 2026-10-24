using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateRecoveryConflictCoordinateIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Update keeps an observed recovery candidate coordinate while unrelated work proceeds"), Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    [InlineData("valid-final")]
    [InlineData("draft")]
    [InlineData("malformed-final")]
    public async Task ExistingRecoveryCandidateIsWarnedWithoutBlocking(
        string candidateKind)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-recovery-coordinate-{candidateKind}");
        var candidate = await SeedCandidateAsync(workspace.Workspace, candidateKind);
        var before = workspace.SnapshotHashes();
        try
        {
            var result = await workspace.ExecuteAsync(
                workspace.Request(),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(0, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
            Assert.DoesNotContain(result.Findings, item => item.Code == RouteUpdateFindingCode.RecoveryConflict);
            var report = RouteUpdateReportSelector.Select(result, new CliSelection(CliDetail.Standard, null));
            if (candidateKind == "valid-final")
            {
                Assert.Empty(result.RecoveryHistory.Paths);
                Assert.DoesNotContain(report.Findings, item => item.Subject.Path == candidate.Path);
            }
            else
            {
                Assert.Contains(candidate.Path, result.RecoveryHistory.Paths);
                Assert.Contains(report.Findings, item => item.Code == RouteUpdateDefinitions.RecoveryHistoryCode
                    && item.Subject.Path == candidate.Path && item.Severity == CliSeverity.Warning);
            }
            Assert.NotEqual(candidate.Path, result.Recovery.ResidualPath);
            Assert.NotEmpty(result.Effects);
            Assert.Equal(RouteUpdateVerificationState.Verified, result.Verification);
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
    [Fact(DisplayName = "Route Update prepares its own bundle while preserving history added after planning"), Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task HeldLeasePreparationPreservesLaterHistory()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-recovery-coordinate-held-lease");
        EnsureRecoveryPayload(workspace.Workspace);
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request());
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.NotEmpty(plan.RecoveryTargets);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var candidate = await SeedCandidateAsync(workspace.Workspace, "valid-final");
        workspace.TrackRecoveryPath(candidate.Path);
        var before = workspace.SnapshotHashes();

        var planBuilder = RouteUpdateIntegrationWorkspace.CreatePlanBuilder();
        var preparer = new RouteUpdateApplicationPreparer(
            new RouteUpdatePlanRevalidator(planBuilder, new RouteUpdatePlanEquivalence()),
            new MutationRevalidator(new FileExpectationValidator(new PhysicalPathResolver())));
        var preparation = await preparer.PrepareAsync(
            new RouteUpdateApplicationPipelineInput
            {
                Plan = plan,
                Lease = lease,
                OperationId = operationId,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteUpdateApplicationPreparationState.Ready, preparation.State);
        Assert.NotNull(preparation.RecoveryPreparation);
        Assert.NotEqual(candidate.Path, preparation.RecoveryPreparation.BundlePath);
        RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation.RecoveryPreparation.BundlePath);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            candidate.Bytes,
            await File.ReadAllBytesAsync(candidate.Path, TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Managed")]
    [Theory(DisplayName = "Route Update maps stored blocked preparation only when a residual coordinate exists"), Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StoredBlockedPreparationUsesOnlyItsObservedResidualPath(bool hasResidualPath)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-stored-blocked-coordinate");
        EnsureRecoveryPayload(workspace.Workspace);
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request());
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.NotEmpty(plan.RecoveryTargets);
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
                "update",
                "observed.bundle");
        }

        var stored = RecoveryBundlePreparationResult.Blocked(cause, candidatePath, failure);
        var mapped = RouteUpdateRecoveryResultProjector.FromPreparation(stored);
        var validation = MutationValidationResult.Valid();
        var boundary = RouteUpdateApplicationPreparer.RecoveryBoundary(plan, mapped, validation);

        Assert.Same(failure, stored.Failure);
        Assert.Equal(cause, stored.Cause);
        Assert.Equal(cause, mapped.Cause);
        Assert.Null(boundary.RecoveryPreparation);
        Assert.Same(validation, boundary.Validation);
        Assert.Equal(candidatePath, boundary.Recovery.ResidualPath);

        var expectedState = RouteUpdateApplicationPreparationState.Incomplete;
        var expectedCode = RouteUpdateFindingCode.RecoveryUnavailable;
        if (hasResidualPath)
        {
            expectedState = RouteUpdateApplicationPreparationState.Blocked;
            expectedCode = RouteUpdateFindingCode.RecoveryConflict;
        }

        Assert.Equal(expectedState, boundary.State);
        var finding = Assert.IsType<RouteUpdateFinding>(boundary.Finding);
        Assert.Equal(expectedCode, finding.Code);
        var expectedStatus = CliSemanticStatus.Incomplete;
        if (hasResidualPath)
        {
            expectedStatus = CliSemanticStatus.Blocked;
        }

        Assert.Equal(expectedStatus, finding.Status);
        Assert.Equal(cause, finding.Cause);
        var expectedTarget = plan.Preview.Target.Path ?? plan.Preview.Target.Requested;
        if (candidatePath is not null)
        {
            expectedTarget = candidatePath;
        }

        Assert.Equal(expectedTarget, finding.Target);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async ValueTask<(string Path, byte[] Bytes)> SeedCandidateAsync(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace,
        string candidateKind)
    {
        if (candidateKind == "valid-final")
        {
            var payloadPath = EnsureRecoveryPayload(workspace);
            var payloadBytes = "prior recovery target bytes"u8.ToArray();
            var snapshot = FileStateSnapshot.File(
                payloadPath,
                payloadPath,
                payloadBytes);
            var prepared = await RecoveryBundleStore.PrepareAsync(
                RecoveryBundleInput.Create(
                    workspace,
                    RouteUpdateDefinitions.CommandIdentity,
                    RecoveryBundleAttribution.Create(
                        RecoveryBundleProducer.Route,
                        RecoveryBundleOperation.Update,
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

    private static string EnsureRecoveryPayload(
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace)
    {
        var path = Path.Combine(
            workspace.PhysicalRoot,
            RouteUpdateIntegrationWorkspace.TargetPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path));
        return path;
    }
}
