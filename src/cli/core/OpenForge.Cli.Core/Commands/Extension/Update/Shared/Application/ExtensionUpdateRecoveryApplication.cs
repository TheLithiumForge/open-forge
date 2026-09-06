using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;

internal sealed class ExtensionUpdateRecoveryApplication(
    RecoveryBundleStore recoveryStore,
    RecoveryBundleCatalogue recoveryCatalogue,
    RecoveryBundleDeletionGuard recoveryDeletionGuard)
{
    private readonly RecoveryBundleStore _recoveryStore = recoveryStore;
    private readonly RecoveryBundleCatalogue _recoveryCatalogue = recoveryCatalogue;
    private readonly RecoveryBundleDeletionGuard _recoveryDeletionGuard = recoveryDeletionGuard;

    internal ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        ExtensionUpdatePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
        => _recoveryStore.PrepareAsync(
            RecoveryBundleInput.Create(
                plan.Request.Workspace,
                ExtensionUpdateDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Extension,
                    RecoveryBundleOperation.Update,
                    plan.Request.Workspace),
                operationId,
                plan.RecoveryTargets),
            cancellationToken);

    internal async ValueTask<ExtensionUpdateRecoveryCleanup> CleanupAsync(
        ExtensionUpdatePlan plan,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        if (preparation is null)
        {
            return new ExtensionUpdateRecoveryCleanup(
                new ExtensionUpdateRecovery(
                    ExtensionUpdateRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Finding: null);
        }

        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await _recoveryCatalogue.ReadAsync(
                plan.Request.Workspace,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Retained(
                preparation,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery cleanup was interrupted before deletion.");
        }
        catch (Exception)
        {
            return Failure(
                preparation,
                ExtensionUpdateFindingCode.RecoveryFailed,
                "Extension Update recovery cleanup failed unexpectedly.",
                residualPath: null);
        }

        var candidate = catalogue.State == RecoveryBundleCatalogueState.Available
            ? catalogue.Candidates.SingleOrDefault(value => Matches(value, preparation))
            : null;
        if (candidate is null)
        {
            if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
            {
                return Retained(
                    preparation,
                    ExtensionUpdateFindingCode.Interrupted,
                    catalogue.Cause ?? "Extension Update recovery cleanup was interrupted before deletion.");
            }

            return Failure(
                preparation,
                ExtensionUpdateFindingCode.RecoveryFailed,
                catalogue.Cause ?? "The prepared Extension Update recovery bundle is unavailable for cleanup.",
                residualPath: null);
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await _recoveryDeletionGuard.DeleteAsync(
                lease,
                candidate,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(
                preparation,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery cleanup was interrupted.",
                residualPath: null);
        }
        catch (Exception)
        {
            return Failure(
                preparation,
                ExtensionUpdateFindingCode.RecoveryFailed,
                "Extension Update recovery cleanup failed unexpectedly.",
                residualPath: null);
        }

        if (deletion.State == RecoveryBundleDeletionState.Deleted)
        {
            return new ExtensionUpdateRecoveryCleanup(
                new ExtensionUpdateRecovery(
                    ExtensionUpdateRecoveryState.Removed,
                    ProtectedPaths(preparation),
                    residualPath: null),
                Finding: null);
        }

        var residual = deletion.ResidualPath ?? preparation.BundlePath;
        if (deletion.State == RecoveryBundleDeletionState.Failed
            && deletion.Disposition == RecoveryBundleDisposition.Retained)
        {
            return new ExtensionUpdateRecoveryCleanup(
                new ExtensionUpdateRecovery(
                    ExtensionUpdateRecoveryState.Retained,
                    ProtectedPaths(preparation),
                    residual),
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.RecoveryArtifactRetained,
                    deletion.Cause ?? "The verified Extension Update recovery bundle remains after cleanup.",
                    residual));
        }

        var state = deletion.Disposition == RecoveryBundleDisposition.Retained
            ? ExtensionUpdateRecoveryState.Retained
            : ExtensionUpdateRecoveryState.Unknown;
        return new ExtensionUpdateRecoveryCleanup(
            new ExtensionUpdateRecovery(
                state,
                ProtectedPaths(preparation),
                state == ExtensionUpdateRecoveryState.Retained ? residual : deletion.ResidualPath),
            new ExtensionUpdateFinding(
                deletion.State == RecoveryBundleDeletionState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : ExtensionUpdateFindingCode.RecoveryFailed,
                deletion.Cause ?? "The Extension Update recovery artifact disposition is unsafe or unknown.",
                deletion.ResidualPath));
    }

    private static ExtensionUpdateRecoveryCleanup Failure(
        RecoveryBundlePreparation preparation,
        ExtensionUpdateFindingCode code,
        string cause,
        string? residualPath)
        => new(
            new ExtensionUpdateRecovery(
                ExtensionUpdateRecoveryState.Unknown,
                ProtectedPaths(preparation),
                residualPath),
            new ExtensionUpdateFinding(code, cause, residualPath));

    private static ExtensionUpdateRecoveryCleanup Retained(
        RecoveryBundlePreparation preparation,
        ExtensionUpdateFindingCode code,
        string cause)
        => new(
            new ExtensionUpdateRecovery(
                ExtensionUpdateRecoveryState.Retained,
                ProtectedPaths(preparation),
                preparation.BundlePath),
            new ExtensionUpdateFinding(code, cause, preparation.BundlePath));

    private static IReadOnlyList<string> ProtectedPaths(RecoveryBundlePreparation preparation)
        => preparation.Entries.OrderBy(entry => entry.Ordinal).Select(entry => entry.TargetPath).ToArray();

    private static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
    {
        if (candidate.Kind != RecoveryBundleCandidateKind.Final
            || candidate.Integrity != RecoveryBundleIntegrity.Verified
            || candidate.Verified is not { } verified)
        {
            return false;
        }

        return PhysicalIdentityTracker.PathComparer.Equals(candidate.Path, preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(verified.WorkspaceKey, preparation.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(verified.Command, preparation.Command, StringComparison.Ordinal)
            && verified.Attribution == preparation.Attribution
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);
    }
}

internal sealed record ExtensionUpdateRecoveryCleanup(
    ExtensionUpdateRecovery Recovery,
    ExtensionUpdateFinding? Finding);
