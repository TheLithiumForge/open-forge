using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal static class ExtensionRemoveRecoveryApplication
{
    internal static ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        ExtensionRemoveExecutionPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
        => RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                plan.Content.Request.Workspace,
                ExtensionRemoveDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Extension,
                    RecoveryBundleOperation.Remove,
                    plan.Content.Request.Workspace),
                operationId,
                plan.RecoveryTargets),
            cancellationToken);

    internal static async ValueTask<ExtensionRemoveRecoveryCleanup> CleanupAsync(
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
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
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
            ? catalogue.Candidates.SingleOrDefault(value => RecoveryBundleIdentity.Matches(value, preparation))
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
            deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
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

    private static string[] ProtectedPaths(
        RecoveryBundlePreparation preparation)
        => [.. preparation.Entries
            .OrderBy(entry => entry.Ordinal)
            .Select(entry => entry.TargetPath)];
}
