using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
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

    internal static async ValueTask<ExtensionRemoveRecoveryCleanup> VerifyRetainedAsync(
        ExtensionRemovePlan plan,
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
                "Extension Remove recovery verification was interrupted before retention was verified.");
        }
        catch (Exception)
        {
            return Failure(
                preparation,
                ExtensionRemoveFindingCode.RecoveryFailed,
                "Extension Remove recovery verification failed unexpectedly.",
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
                    "Extension Remove recovery verification was interrupted before retention was verified.")
                : Failure(
                    preparation,
                    ExtensionRemoveFindingCode.RecoveryFailed,
                    catalogue.Cause
                        ?? "The prepared Extension Remove recovery bundle is unavailable for verification.",
                    residualPath: null);
        }

        return new ExtensionRemoveRecoveryCleanup(
            new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.Retained,
                ProtectedPaths(preparation),
                preparation.BundlePath),
            Finding: null);
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
