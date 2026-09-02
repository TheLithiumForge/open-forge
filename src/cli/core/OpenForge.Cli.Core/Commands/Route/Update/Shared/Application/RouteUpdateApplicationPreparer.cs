using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed partial class RouteUpdateApplicationPreparer
{
    private readonly RouteUpdatePlanRevalidator _revalidator;
    private readonly RouteUpdateRecoveryPreparer _recoveryPreparer;
    private readonly MutationRevalidator _mutationRevalidator;

    internal RouteUpdateApplicationPreparer(
        RouteUpdatePlanRevalidator revalidator,
        RouteUpdateRecoveryPreparer recoveryPreparer,
        MutationRevalidator mutationRevalidator)
    {
        _revalidator = revalidator;
        _recoveryPreparer = recoveryPreparer;
        _mutationRevalidator = mutationRevalidator;
    }

    internal async ValueTask<RouteUpdateApplicationPreparation> PrepareAsync(
        RouteUpdateApplicationPreparationInput input,
        CancellationToken cancellationToken)
    {
        var revalidation = await _revalidator.RevalidateAsync(
                input.Plan,
                cancellationToken)
            .ConfigureAwait(false);
        if (revalidation.State != RouteUpdatePlanRevalidationState.Exact)
        {
            return RevalidationBoundary(input.Plan, revalidation);
        }

        MutationValidationResult validation;
        try
        {
            validation = await _mutationRevalidator.ValidateAsync(
                    input.Lease,
                    input.Plan.FileChanges,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Boundary(
                input.Plan,
                RouteUpdateApplicationPreparationState.Cancelled,
                RouteUpdateFindingCode.Interrupted,
                "Route Update whole-plan validation was interrupted.");
        }
        catch (Exception)
        {
            return Boundary(
                input.Plan,
                RouteUpdateApplicationPreparationState.Failed,
                RouteUpdateFindingCode.OperationFailed,
                "Route Update whole-plan validation failed unexpectedly.");
        }

        if (!HasExactChecks(input.Plan, validation))
        {
            return ValidationBoundary(input.Plan, validation);
        }

        var recovery = await _recoveryPreparer.PrepareAsync(
                new RouteUpdateRecoveryPreparationInput
                {
                    Plan = input.Plan,
                    OperationId = input.OperationId.ToString("D"),
                },
                cancellationToken)
            .ConfigureAwait(false);
        if (recovery.State is not (RouteUpdateRecoveryPreparationState.NotRequired
            or RouteUpdateRecoveryPreparationState.Prepared))
        {
            return RecoveryBoundary(input.Plan, recovery, validation);
        }

        return new RouteUpdateApplicationPreparation
        {
            State = RouteUpdateApplicationPreparationState.Ready,
            RecoveryPreparation = recovery.Preparation,
            Validation = validation,
            Recovery = recovery.Recovery,
            Finding = null,
        };
    }

    private static bool HasExactChecks(
        RouteUpdatePlan plan,
        MutationValidationResult validation)
    {
        if (validation.State != MutationValidationState.Valid
            || validation.Checks.Count != plan.FileChanges.Length)
        {
            return false;
        }

        for (var index = 0; index < plan.FileChanges.Length; index++)
        {
            var expectation = plan.FileChanges[index].Expectation;
            var check = validation.Checks[index];
            if (check.State != FileExpectationValidationState.Matched
                || check.Expectation != expectation
                || check.Actual?.Expectation != expectation)
            {
                return false;
            }
        }

        return true;
    }
}
