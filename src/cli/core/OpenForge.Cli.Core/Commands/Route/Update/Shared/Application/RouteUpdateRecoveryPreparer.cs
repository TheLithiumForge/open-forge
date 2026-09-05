using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed class RouteUpdateRecoveryPreparer(
    RecoveryBundleCatalogue catalogue,
    RecoveryBundleStore store)
{
    private readonly RecoveryBundleCatalogue _catalogue = catalogue;
    private readonly RecoveryBundleStore _store = store;

    internal async ValueTask<RouteUpdateRecoveryPreparationResult> PrepareAsync(
        RouteUpdateRecoveryPreparationInput input,
        CancellationToken cancellationToken)
    {
        if (input.Plan.RecoveryTargets.IsEmpty)
        {
            return RouteUpdateRecoveryResultProjector.NotRequired();
        }

        if (!Guid.TryParseExact(input.OperationId, "D", out var operationId)
            || operationId == Guid.Empty)
        {
            return RouteUpdateRecoveryResultProjector.InvalidOperationId();
        }

        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await _catalogue.ReadAsync(
                    input.Plan.Request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteUpdateRecoveryResultProjector.Cancelled(residualPath: null);
        }
        catch (Exception)
        {
            return RouteUpdateRecoveryResultProjector.CatalogueFailed(
                "The Route Update recovery catalogue failed unexpectedly.");
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return RouteUpdateRecoveryResultProjector.Cancelled(residualPath: null);
        }

        if (catalogue.State == RecoveryBundleCatalogueState.Unavailable)
        {
            return RouteUpdateRecoveryResultProjector.CatalogueUnavailable(
                catalogue.Cause ?? "The Route Update recovery catalogue is unavailable.");
        }

        if (!catalogue.Candidates.IsEmpty)
        {
            return RouteUpdateRecoveryResultProjector.Conflict(
                catalogue.Candidates[0].Path,
                "An existing recovery candidate conflicts with this Route Update application.");
        }

        RecoveryBundlePreparationResult result;
        try
        {
            result = await _store.PrepareAsync(
                    RecoveryBundleInput.Create(
                        input.Plan.Request.Workspace,
                        RouteUpdateDefinitions.CommandIdentity,
                        RecoveryBundleAttribution.Create(
                            RecoveryBundleProducer.Route,
                            RecoveryBundleOperation.Update,
                            input.Plan.Request.Workspace),
                        operationId,
                        input.Plan.RecoveryTargets),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteUpdateRecoveryResultProjector.StorageInterrupted();
        }
        catch (Exception)
        {
            return RouteUpdateRecoveryResultProjector.CatalogueFailed(
                "Route Update recovery preparation failed unexpectedly after storage entry.");
        }

        return RouteUpdateRecoveryResultProjector.FromPreparation(result);
    }
}
