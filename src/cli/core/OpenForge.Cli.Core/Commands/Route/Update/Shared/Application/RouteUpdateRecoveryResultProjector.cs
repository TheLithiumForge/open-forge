using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal static class RouteUpdateRecoveryResultProjector
{
    internal static RouteUpdateRecoveryPreparationResult NotRequired()
        => Preparation(
            RouteUpdateRecoveryPreparationState.NotRequired,
            preparation: null,
            RouteUpdateRecovery.NotCreated(),
            cause: null);

    internal static RouteUpdateRecoveryPreparationResult InvalidOperationId()
        => Preparation(
            RouteUpdateRecoveryPreparationState.Failed,
            preparation: null,
            RouteUpdateRecovery.Unknown(residualPath: null),
            "Route Update recovery preparation requires one canonical operation ID.");

    internal static RouteUpdateRecoveryPreparationResult CatalogueFailed(string cause)
        => Preparation(
            RouteUpdateRecoveryPreparationState.Failed,
            preparation: null,
            RouteUpdateRecovery.Unknown(residualPath: null),
            cause);

    internal static RouteUpdateRecoveryPreparationResult CatalogueUnavailable(string cause)
        => Preparation(
            RouteUpdateRecoveryPreparationState.Incomplete,
            preparation: null,
            RouteUpdateRecovery.Unknown(residualPath: null),
            cause);

    internal static RouteUpdateRecoveryPreparationResult Conflict(
        string residualPath,
        string cause)
        => Preparation(
            RouteUpdateRecoveryPreparationState.Blocked,
            preparation: null,
            RouteUpdateRecovery.Retained(residualPath),
            cause);

    internal static RouteUpdateRecoveryPreparationResult Cancelled(string? residualPath)
        => Preparation(
            RouteUpdateRecoveryPreparationState.Cancelled,
            preparation: null,
            Residual(residualPath),
            "Route Update recovery preparation was interrupted.");

    internal static RouteUpdateRecoveryPreparationResult StorageInterrupted()
        => Preparation(
            RouteUpdateRecoveryPreparationState.Cancelled,
            preparation: null,
            RouteUpdateRecovery.Unknown(residualPath: null),
            "Route Update recovery preparation was interrupted after storage entry.");

    internal static RouteUpdateRecoveryPreparationResult FromPreparation(
        RecoveryBundlePreparationResult result)
        => result.State switch
        {
            RecoveryBundlePreparationState.Prepared when result.Preparation is { } verified =>
                Preparation(
                    RouteUpdateRecoveryPreparationState.Prepared,
                    verified,
                    RouteUpdateRecovery.NotCreated(),
                    cause: null),
            RecoveryBundlePreparationState.Prepared => Preparation(
                RouteUpdateRecoveryPreparationState.Failed,
                preparation: null,
                Residual(result.ResidualPath),
                "Route Update recovery preparation omitted its verified final."),
            RecoveryBundlePreparationState.Incomplete => Preparation(
                RouteUpdateRecoveryPreparationState.Incomplete,
                preparation: null,
                Residual(result.ResidualPath),
                result.Cause ?? "Route Update recovery preparation is incomplete."),
            RecoveryBundlePreparationState.Blocked => Preparation(
                RouteUpdateRecoveryPreparationState.Blocked,
                preparation: null,
                Residual(result.ResidualPath),
                result.Cause ?? "Route Update recovery preparation is blocked."),
            RecoveryBundlePreparationState.Cancelled => Cancelled(result.ResidualPath),
            RecoveryBundlePreparationState.NotNeeded => Preparation(
                RouteUpdateRecoveryPreparationState.Failed,
                preparation: null,
                Residual(result.ResidualPath),
                "Route Update recovery preparation unexpectedly reported no need."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The recovery preparation state is not defined."),
        };

    internal static RouteUpdateRecoveryCompletionResult FromDeletion(
        RecoveryBundlePreparation preparation,
        RecoveryBundleDeletionResult deletion)
    {
        if (deletion.State == RecoveryBundleDeletionState.Deleted
            && deletion.Disposition == RecoveryBundleDisposition.Removed)
        {
            return new RouteUpdateRecoveryCompletionResult
            {
                Recovery = RouteUpdateRecovery.Removed(),
                FindingCode = null,
                Cause = null,
            };
        }

        if (deletion.Disposition == RecoveryBundleDisposition.Retained)
        {
            return Retained(
                preparation,
                deletion.Cause ?? "The verified Route Update recovery artifact was retained.");
        }

        return Unknown(
            deletion.Cause ?? "The Route Update recovery artifact disposition is unknown.");
    }

    internal static RouteUpdateRecoveryCompletionResult Retained(
        RecoveryBundlePreparation preparation,
        string cause)
        => new()
        {
            Recovery = RouteUpdateRecovery.Retained(preparation.BundlePath),
            FindingCode = RouteUpdateFindingCode.RecoveryArtifactRetained,
            Cause = cause,
        };

    internal static RouteUpdateRecoveryCompletionResult Unknown(string cause)
        => new()
        {
            Recovery = RouteUpdateRecovery.Unknown(residualPath: null),
            FindingCode = RouteUpdateFindingCode.RecoveryFailed,
            Cause = cause,
        };

    private static RouteUpdateRecoveryPreparationResult Preparation(
        RouteUpdateRecoveryPreparationState state,
        RecoveryBundlePreparation? preparation,
        RouteUpdateRecovery recovery,
        string? cause)
        => new()
        {
            State = state,
            Preparation = preparation,
            Recovery = recovery,
            Cause = cause,
        };

    private static RouteUpdateRecovery Residual(string? residualPath)
        => residualPath is null
            ? RouteUpdateRecovery.NotCreated()
            : RouteUpdateRecovery.Unknown(residualPath);
}
