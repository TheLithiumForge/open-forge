using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal sealed class ExtensionRemoveRecoveryApplication(
    RecoveryBundleStore recoveryStore,
    RecoveryBundleCatalogue recoveryCatalogue,
    RecoveryBundleDeletionGuard recoveryDeletionGuard)
{
    private readonly RecoveryBundleStore _recoveryStore = recoveryStore;
    private readonly RecoveryBundleCatalogue _recoveryCatalogue = recoveryCatalogue;
    private readonly RecoveryBundleDeletionGuard _recoveryDeletionGuard = recoveryDeletionGuard;

    internal ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        ExtensionRemovePlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
        => _recoveryStore.PrepareAsync(
            RecoveryBundleInput.Create(
                plan.Request.Workspace,
                ExtensionRemoveDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Extension,
                    RecoveryBundleOperation.Remove,
                    plan.Request.Workspace),
                operationId,
                plan.Effects
                    .Select(effect => effect.RecoveryTarget)
                    .Where(target => target is not null)
                    .Cast<RecoveryBundleTarget>()
                    .Append(plan.LifecycleRecoveryTarget)
                    .Where(target => target is not null)
                    .Cast<RecoveryBundleTarget>()
                    .ToArray()),
            cancellationToken);

    internal async ValueTask<ExtensionRemoveRecoveryCleanup> CleanupAsync(
        ExtensionRemovePlan plan,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        if (preparation is null)
        {
            return new ExtensionRemoveRecoveryCleanup(
                new ExtensionRemoveRecovery(
                    ExtensionRemoveRecoveryState.NotRequired,
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
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove recovery cleanup was interrupted before deletion.");
        }
        catch (Exception)
        {
            return Failure(
                preparation,
                ExtensionRemoveFindingCode.RecoveryFailed,
                "Extension Remove recovery cleanup failed unexpectedly.",
                residualPath: null);
        }

        var candidate = catalogue.State == RecoveryBundleCatalogueState.Available
            ? catalogue.Candidates.SingleOrDefault(value => Matches(value, preparation))
            : null;
        if (candidate is null)
        {
            return catalogue.State == RecoveryBundleCatalogueState.Cancelled
                ? Retained(
                    preparation,
                    ExtensionRemoveFindingCode.Interrupted,
                    "Extension Remove recovery cleanup was interrupted before deletion.")
                : Failure(
                    preparation,
                    ExtensionRemoveFindingCode.RecoveryFailed,
                    catalogue.Cause
                        ?? "The prepared Extension Remove recovery bundle is unavailable for cleanup.",
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
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove recovery cleanup was interrupted.",
                residualPath: null);
        }
        catch (Exception)
        {
            return Failure(
                preparation,
                ExtensionRemoveFindingCode.RecoveryFailed,
                "Extension Remove recovery cleanup failed unexpectedly.",
                residualPath: null);
        }

        if (deletion.State == RecoveryBundleDeletionState.Deleted)
        {
            return new ExtensionRemoveRecoveryCleanup(
                new ExtensionRemoveRecovery(
                    ExtensionRemoveRecoveryState.Removed,
                    ProtectedPaths(preparation),
                    residualPath: null),
                Finding: null);
        }

        var residual = deletion.ResidualPath ?? preparation.BundlePath;
        if (deletion.State == RecoveryBundleDeletionState.Failed
            && deletion.Disposition == RecoveryBundleDisposition.Retained)
        {
            return new ExtensionRemoveRecoveryCleanup(
                new ExtensionRemoveRecovery(
                    ExtensionRemoveRecoveryState.Retained,
                    ProtectedPaths(preparation),
                    residual),
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.RecoveryArtifactRetained,
                    deletion.Cause
                        ?? "The verified Extension Remove recovery bundle remains after cleanup.",
                    residual));
        }

        return Failure(
            preparation,
            deletion.State == RecoveryBundleDeletionState.Cancelled
                ? ExtensionRemoveFindingCode.Interrupted
                : ExtensionRemoveFindingCode.RecoveryFailed,
            deletion.Cause ?? "The Extension Remove recovery artifact disposition is unsafe or unknown.",
            deletion.ResidualPath);
    }

    internal static bool Matches(
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

    private static ExtensionRemoveRecoveryCleanup Failure(
        RecoveryBundlePreparation preparation,
        ExtensionRemoveFindingCode code,
        string cause,
        string? residualPath)
        => new(
            new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.Unknown,
                ProtectedPaths(preparation),
                residualPath),
            new ExtensionRemoveFinding(code, cause, residualPath));

    private static ExtensionRemoveRecoveryCleanup Retained(
        RecoveryBundlePreparation preparation,
        ExtensionRemoveFindingCode code,
        string cause)
        => new(
            new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.Retained,
                ProtectedPaths(preparation),
                preparation.BundlePath),
            new ExtensionRemoveFinding(code, cause, preparation.BundlePath));

    private static IReadOnlyList<string> ProtectedPaths(
        RecoveryBundlePreparation preparation)
        => preparation.Entries
            .OrderBy(entry => entry.Ordinal)
            .Select(entry => entry.TargetPath)
            .ToArray();
}

internal sealed record ExtensionRemoveRecoveryCleanup(
    ExtensionRemoveRecovery Recovery,
    ExtensionRemoveFinding? Finding);
