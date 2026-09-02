using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed partial class RouteUpdateApplicationPreparer
{
    private static RouteUpdateApplicationPreparation RevalidationBoundary(
        RouteUpdatePlan plan,
        RouteUpdatePlanRevalidation revalidation)
    {
        return revalidation.State switch
        {
            RouteUpdatePlanRevalidationState.Changed => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Changed,
                RouteUpdateFindingCode.TargetChanged,
                revalidation.Cause ?? "The complete Route Update plan changed before application."),
            RouteUpdatePlanRevalidationState.Cancelled => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Cancelled,
                RouteUpdateFindingCode.Interrupted,
                revalidation.Cause ?? "Route Update plan revalidation was interrupted."),
            RouteUpdatePlanRevalidationState.Failed => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Failed,
                RouteUpdateFindingCode.OperationFailed,
                revalidation.Cause ?? "Route Update plan revalidation failed unexpectedly."),
            RouteUpdatePlanRevalidationState.Exact => throw new ArgumentOutOfRangeException(
                nameof(revalidation),
                revalidation.State,
                "An exact Route Update revalidation has no boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(revalidation),
                revalidation.State,
                "The Route Update revalidation state is not defined."),
        };
    }

    private static RouteUpdateApplicationPreparation ValidationBoundary(
        RouteUpdatePlan plan,
        MutationValidationResult validation)
        => validation.State switch
        {
            MutationValidationState.Mismatched => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Changed,
                RouteUpdateFindingCode.TargetChanged,
                validation.Cause ?? "A planned Route Update target changed before recovery preparation."),
            MutationValidationState.Blocked => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Blocked,
                RouteUpdateFindingCode.TargetUnsafe,
                validation.Cause ?? "A planned Route Update target became unsafe before application."),
            MutationValidationState.Failed => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Incomplete,
                RouteUpdateFindingCode.InspectionIncomplete,
                validation.Cause ?? "A planned Route Update target could not be revalidated completely."),
            MutationValidationState.Cancelled => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Cancelled,
                RouteUpdateFindingCode.Interrupted,
                "Route Update whole-plan validation was interrupted."),
            MutationValidationState.Valid => Boundary(
                plan,
                RouteUpdateApplicationPreparationState.Failed,
                RouteUpdateFindingCode.OperationFailed,
                "Route Update whole-plan validation returned incoherent checks."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(validation),
                validation.State,
                "The mutation validation state is not defined."),
        };

    private static RouteUpdateApplicationPreparation RecoveryBoundary(
        RouteUpdatePlan plan,
        RouteUpdateRecoveryPreparationResult recovery,
        MutationValidationResult validation)
    {
        var mapped = recovery.State switch
        {
            RouteUpdateRecoveryPreparationState.Incomplete => (
                RouteUpdateApplicationPreparationState.Incomplete,
                RouteUpdateFindingCode.RecoveryUnavailable),
            RouteUpdateRecoveryPreparationState.Blocked => (
                RouteUpdateApplicationPreparationState.Blocked,
                RouteUpdateFindingCode.RecoveryConflict),
            RouteUpdateRecoveryPreparationState.Cancelled => (
                RouteUpdateApplicationPreparationState.Cancelled,
                RouteUpdateFindingCode.Interrupted),
            RouteUpdateRecoveryPreparationState.Failed => (
                RouteUpdateApplicationPreparationState.Failed,
                RouteUpdateFindingCode.OperationFailed),
            RouteUpdateRecoveryPreparationState.NotRequired
                or RouteUpdateRecoveryPreparationState.Prepared => throw new ArgumentOutOfRangeException(
                    nameof(recovery),
                    recovery.State,
                    "A completed Route Update recovery preparation has no boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The Route Update recovery preparation state is not defined."),
        };
        return new RouteUpdateApplicationPreparation
        {
            State = mapped.Item1,
            RecoveryPreparation = null,
            Validation = validation,
            Recovery = recovery.Recovery,
            Finding = Finding(
                plan,
                mapped.Item2,
                recovery.Cause ?? "Route Update recovery preparation did not complete."),
        };
    }

    private static RouteUpdateApplicationPreparation Boundary(
        RouteUpdatePlan plan,
        RouteUpdateApplicationPreparationState state,
        RouteUpdateFindingCode code,
        string cause)
        => new()
        {
            State = state,
            RecoveryPreparation = null,
            Validation = null,
            Recovery = RouteUpdateRecovery.NotCreated(),
            Finding = Finding(plan, code, cause),
        };

    private static RouteUpdateFinding Finding(
        RouteUpdatePlan plan,
        RouteUpdateFindingCode code,
        string cause)
        => new(code, cause, plan.Preview.Target.Path ?? plan.Preview.Target.Requested);

}
