using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallRecoveryOperation(
    RecoveryBundleStore recoveryStore,
    RecoveryBundleCatalogue recoveryCatalogue,
    RecoveryBundleDeletionGuard recoveryDeletionGuard)
{
    private readonly RecoveryBundleStore _recoveryStore = recoveryStore;
    private readonly RecoveryBundleCatalogue _recoveryCatalogue = recoveryCatalogue;
    private readonly RecoveryBundleDeletionGuard _recoveryDeletionGuard = recoveryDeletionGuard;

    internal ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        InstallPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
    {
        var input = RecoveryBundleInput.Create(
            plan.Request.Workspace,
            InstallDefinitions.CommandIdentity,
            operationId,
            plan.RecoveryTargets);
        return _recoveryStore.PrepareAsync(input, cancellationToken);
    }

    internal async ValueTask<InstallRecoveryCleanupResult> CleanupAsync(
        InstallRecoveryCleanupRequest request,
        CancellationToken cancellationToken)
    {
        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await _recoveryCatalogue.ReadAsync(
                    request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                code: InstallFindingCode.Interrupted,
                cause: "Install recovery cleanup was interrupted.",
                residualPath: null,
                state: InstallRecoveryState.Unknown);
        }
        catch (Exception)
        {
            return Failed(
                code: InstallFindingCode.RecoveryFailed,
                cause: "Install recovery cleanup failed unexpectedly.",
                residualPath: null,
                state: InstallRecoveryState.Unknown);
        }

        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return ReadCatalogueBoundary(catalogue);
        }

        var candidates = catalogue.Candidates
            .Where(candidate => MatchesPreparation(
                candidate,
                request.Preparation))
            .ToArray();
        if (candidates.Length != 1)
        {
            return Failed(
                code: InstallFindingCode.RecoveryFailed,
                cause: "The prepared Install recovery bundle no longer has one exact recognized identity.",
                residualPath: null,
                state: InstallRecoveryState.Unknown);
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await _recoveryDeletionGuard.DeleteAsync(
                    request.Lease,
                    candidates[0],
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failed(
                code: InstallFindingCode.Interrupted,
                cause: "Install recovery cleanup was interrupted.",
                residualPath: null,
                state: InstallRecoveryState.Unknown);
        }
        catch (Exception)
        {
            return Failed(
                code: InstallFindingCode.RecoveryFailed,
                cause: "Install recovery cleanup failed unexpectedly.",
                residualPath: null,
                state: InstallRecoveryState.Unknown);
        }

        return ReadDeletion(deletion);
    }

    private static InstallRecoveryCleanupResult ReadCatalogueBoundary(
        RecoveryBundleCatalogueResult catalogue)
    {
        return catalogue.State switch
        {
            RecoveryBundleCatalogueState.Cancelled => Failed(
                code: InstallFindingCode.Interrupted,
                cause: catalogue.Cause
                    ?? "The prepared Install recovery bundle could not be re-read for cleanup.",
                residualPath: null,
                state: InstallRecoveryState.Unknown),
            RecoveryBundleCatalogueState.Unavailable => Failed(
                code: InstallFindingCode.RecoveryFailed,
                cause: catalogue.Cause
                    ?? "The prepared Install recovery bundle could not be re-read for cleanup.",
                residualPath: null,
                state: InstallRecoveryState.Unknown),
            RecoveryBundleCatalogueState.Available => throw new ArgumentException(
                "An available recovery catalogue is not a cleanup boundary.",
                nameof(catalogue)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(catalogue),
                catalogue.State,
                "The recovery catalogue state is not defined."),
        };
    }

    internal static InstallRecoveryCleanupResult ReadDeletion(
        RecoveryBundleDeletionResult deletion)
    {
        return deletion.State switch
        {
            RecoveryBundleDeletionState.Deleted => new InstallRecoveryCleanupResult
            {
                Finding = null,
                Recovery = InstallApplicationResultFactory.Recovery(
                    InstallRecoveryState.Removed),
            },
            RecoveryBundleDeletionState.Failed
                when deletion.Disposition == RecoveryBundleDisposition.Retained => Failed(
                    code: InstallFindingCode.RecoveryArtifactRetained,
                    cause: deletion.Cause
                        ?? "The verified Install recovery artifact remains after cleanup.",
                    residualPath: deletion.ResidualPath,
                    state: InstallRecoveryState.Retained),
            RecoveryBundleDeletionState.Failed
                or RecoveryBundleDeletionState.Blocked => Failed(
                    code: InstallFindingCode.RecoveryFailed,
                    cause: deletion.Cause
                        ?? "The Install recovery artifact disposition is unsafe or unknown.",
                    residualPath: deletion.ResidualPath,
                    state: ReadDisposition(deletion.Disposition)),
            RecoveryBundleDeletionState.Cancelled => Failed(
                code: InstallFindingCode.Interrupted,
                cause: "Install recovery cleanup was interrupted.",
                residualPath: deletion.ResidualPath,
                state: ReadDisposition(deletion.Disposition)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(deletion),
                deletion.State,
                "The recovery deletion state is not defined."),
        };
    }

    private static InstallRecoveryState ReadDisposition(
        RecoveryBundleDisposition disposition)
    {
        return disposition switch
        {
            RecoveryBundleDisposition.Retained => InstallRecoveryState.Retained,
            RecoveryBundleDisposition.Removed
                or RecoveryBundleDisposition.Unknown => InstallRecoveryState.Unknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(disposition),
                disposition,
                "The recovery bundle disposition is not defined."),
        };
    }

    private static InstallRecoveryCleanupResult Failed(
        InstallFindingCode code,
        string cause,
        string? residualPath,
        InstallRecoveryState state)
        => new()
        {
            Finding = new InstallFinding(code, cause),
            Recovery = InstallApplicationResultFactory.Recovery(
                state,
                residualPath),
        };

    private static bool MatchesPreparation(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
    {
        if (candidate.Kind != RecoveryBundleCandidateKind.Final
            || candidate.Integrity != RecoveryBundleIntegrity.Verified
            || candidate.Verified is not { } verified)
        {
            return false;
        }

        return PhysicalIdentityTracker.PathComparer.Equals(
                candidate.Path,
                preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(
                verified.WorkspaceKey,
                preparation.WorkspaceKey,
                StringComparison.Ordinal)
            && string.Equals(
                verified.Command,
                preparation.Command,
                StringComparison.Ordinal)
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);
    }
}
