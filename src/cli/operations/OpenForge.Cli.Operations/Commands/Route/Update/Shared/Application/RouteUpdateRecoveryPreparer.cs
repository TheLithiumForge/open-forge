using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal static class RouteUpdateRecoveryPreparer
{
    internal static async ValueTask<RouteUpdateRecoveryPreparationResult> PrepareAsync(
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

        RecoveryBundlePreparationResult result;
        try
        {
            result = await RecoveryBundleStore.PrepareAsync(
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
