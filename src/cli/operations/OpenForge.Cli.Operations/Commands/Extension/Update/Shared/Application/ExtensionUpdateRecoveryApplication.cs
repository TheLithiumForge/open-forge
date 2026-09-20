using OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;

internal static class ExtensionUpdateRecoveryApplication
{
    internal static ValueTask<RecoveryBundlePreparationResult> PrepareAsync(
        ExtensionUpdateExecutionPlan plan,
        Guid operationId,
        CancellationToken cancellationToken)
        => RecoveryBundleStore.PrepareAsync(
            RecoveryBundleInput.Create(
                plan.Content.Request.Workspace,
                ExtensionUpdateDefinitions.CommandIdentity,
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Extension,
                    RecoveryBundleOperation.Update,
                    plan.Content.Request.Workspace),
                operationId,
                plan.RecoveryTargets),
            cancellationToken);

    internal static async ValueTask<ExtensionUpdateRecoveryCleanup> VerifyRetainedAsync(
        ExtensionUpdatePlan plan,
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
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
                plan.Request.Workspace,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Retained(
                preparation,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery verification was interrupted before retention was verified.");
        }
        catch (Exception)
        {
            return Failure(
                preparation,
                ExtensionUpdateFindingCode.RecoveryFailed,
                "Extension Update recovery verification failed unexpectedly.",
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
                    ExtensionUpdateFindingCode.Interrupted,
                    "Extension Update recovery verification was interrupted before retention was verified.")
                : Failure(
                    preparation,
                    ExtensionUpdateFindingCode.RecoveryFailed,
                    catalogue.Cause
                        ?? "The prepared Extension Update recovery bundle is unavailable for verification.",
                    residualPath: null);
        }

        return new ExtensionUpdateRecoveryCleanup(
            new ExtensionUpdateRecovery(
                ExtensionUpdateRecoveryState.Retained,
                ProtectedPaths(preparation),
                preparation.BundlePath),
            Finding: null);
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

    private static string[] ProtectedPaths(
        RecoveryBundlePreparation preparation)
        => [.. preparation.Entries
            .OrderBy(entry => entry.Ordinal)
            .Select(entry => entry.TargetPath)];
}
