using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal static class RouteUpdateRecoveryCompleter
{
    internal static async ValueTask<RouteUpdateRecoveryCompletionResult> CompleteAsync(
        RouteUpdateRecoveryCompletionInput input,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteUpdateRecoveryResultProjector.Retained(
                input.Preparation,
                "Recovery cleanup was interrupted; the verified artifact was retained.");
        }

        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await RecoveryBundleCatalogue.ReadAsync(
                    input.Plan.Request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteUpdateRecoveryResultProjector.Retained(
                input.Preparation,
                "Recovery cleanup was interrupted; the verified artifact was retained.");
        }
        catch (Exception)
        {
            return RouteUpdateRecoveryResultProjector.Unknown(
                "The recovery catalogue failed during cleanup.");
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return RouteUpdateRecoveryResultProjector.Retained(
                input.Preparation,
                "Recovery cleanup was interrupted; the verified artifact was retained.");
        }

        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return RouteUpdateRecoveryResultProjector.Unknown(
                catalogue.Cause ?? "The recovery catalogue is unavailable during cleanup.");
        }

        var candidates = catalogue.Candidates
            .Where(candidate => RecoveryBundleIdentity.Matches(candidate, input.Preparation))
            .ToArray();
        if (candidates.Length != 1)
        {
            return RouteUpdateRecoveryResultProjector.Unknown(
                "The exact prepared Route Update recovery final could not be selected for cleanup.");
        }

        RecoveryBundleDeletionResult deletion;
        try
        {
            deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                    input.Lease,
                    candidates[0],
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteUpdateRecoveryResultProjector.Retained(
                input.Preparation,
                "Recovery cleanup was interrupted; the verified artifact was retained.");
        }
        catch (Exception)
        {
            return RouteUpdateRecoveryResultProjector.Unknown(
                "Recovery cleanup failed unexpectedly.");
        }

        return RouteUpdateRecoveryResultProjector.FromDeletion(input.Preparation, deletion);
    }
}
