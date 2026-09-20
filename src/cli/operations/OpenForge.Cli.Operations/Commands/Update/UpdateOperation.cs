using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Update;

internal sealed class UpdateOperation
{
    private readonly CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts> _planConfirmation;
    private readonly UpdatePlanBuilder _planBuilder;
    private readonly UpdateApplicationOperation _applicationOperation;

    internal UpdateOperation(
        CliPlanConfirmation<UpdateResult, UpdateConfirmationFacts> planConfirmation,
        UpdatePlanBuilder planBuilder,
        UpdateApplicationOperation applicationOperation)
    {
        ArgumentNullException.ThrowIfNull(planConfirmation);
        ArgumentNullException.ThrowIfNull(planBuilder);
        ArgumentNullException.ThrowIfNull(applicationOperation);
        _planConfirmation = planConfirmation;
        _planBuilder = planBuilder;
        _applicationOperation = applicationOperation;
    }

    internal async ValueTask<UpdateResult> ExecuteAsync(
        UpdateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        UpdatePlanResolution resolution;
        try
        {
            resolution = await _planBuilder
                .BuildExecutionAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanningBoundary(
                request,
                UpdateFindingCode.Interrupted,
                "Update planning was interrupted.");
        }
        catch (Exception)
        {
            return PlanningBoundary(
                request,
                UpdateFindingCode.OperationFailed,
                "Update planning failed unexpectedly.");
        }

        if (resolution.Execution is not { } execution
            || request.IsDryRun
            || (execution.Effects.Count == 0
                && execution.OwnershipChange is null))
        {
            return resolution.Build.Preview;
        }

        var preview = execution.Build.Preview.ForPlanReview();
        var confirmation = await ConfirmAsync(execution, preview, cancellationToken)
            .ConfigureAwait(false);
        return confirmation
            ?? await _applicationOperation
                .ExecuteAsync(execution, cancellationToken)
                .ConfigureAwait(false);
    }

    private async ValueTask<UpdateResult?> ConfirmAsync(
        UpdatePlanExecution execution,
        UpdateResult preview,
        CancellationToken cancellationToken)
    {
        var request = execution.Request;
        if (request.Automatic)
        {
            return null;
        }

        try
        {
            var response = await _planConfirmation(
                    preview,
                    UpdateConfirmationFacts.From(execution),
                    new CliPromptPolicy(request.AllowsInteractiveConfirmation),
                    cancellationToken)
                .ConfigureAwait(false);

            if (response.State == CliPromptState.Unavailable)
            {
                return UpdateResultBuilder.BeforeEffects(
                    execution,
                    new UpdateFinding(
                        UpdateFindingCode.ConfirmationRequired,
                        target: null,
                        "Update requires explicit automatic mode when interactive confirmation is unavailable."));
            }

            if (response.State == CliPromptState.Answered && response.Value)
            {
                return null;
            }

            return UpdateResultBuilder.BeforeEffects(
                execution,
                new UpdateFinding(
                    UpdateFindingCode.Interrupted,
                    target: null,
                    "Update was cancelled. Nothing was changed."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                new UpdateFinding(
                    UpdateFindingCode.Interrupted,
                    target: null,
                    "Update was cancelled. Nothing was changed."));
        }
    }

    private static UpdateResult PlanningBoundary(
        UpdateRequest request,
        UpdateFindingCode code,
        string cause)
        => UpdatePlanResultFactory.Boundary(
            request,
            source: null,
            comparisons: [],
            new UpdateFinding(code, target: null, cause),
            UpdatePlanResultFactory.UnstartedLifecycle(
                UpdateLifecycleTrust.NotRequested,
                UpdateLifecycleCoverage.NotRequested));
}
