using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Result;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Update;

internal sealed class UpdateOperation
{
    private const string ConfirmationPrompt = "Apply the complete Update plan? [y/N] ";

    private readonly CliInteractiveSession _interactiveSession;
    private readonly UpdatePlanBuilder _planBuilder;
    private readonly UpdateApplicationOperation _applicationOperation;

    internal UpdateOperation(
        CliInteractiveSession interactiveSession,
        UpdatePlanBuilder planBuilder,
        UpdateApplicationOperation applicationOperation)
    {
        ArgumentNullException.ThrowIfNull(interactiveSession);
        ArgumentNullException.ThrowIfNull(planBuilder);
        ArgumentNullException.ThrowIfNull(applicationOperation);
        _interactiveSession = interactiveSession;
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
            || (execution.Effects.Count == 0 && execution.LifecycleChange is null))
        {
            return resolution.Build.Preview;
        }

        var confirmation = await ConfirmAsync(execution, cancellationToken)
            .ConfigureAwait(false);
        return confirmation
            ?? await _applicationOperation
                .ExecuteAsync(execution, cancellationToken)
                .ConfigureAwait(false);
    }

    private async ValueTask<UpdateResult?> ConfirmAsync(
        UpdatePlanExecution execution,
        CancellationToken cancellationToken)
    {
        var request = execution.Request;
        if (request.Automatic)
        {
            return null;
        }

        if (!request.AllowsInteractiveConfirmation || !_interactiveSession.CanPrompt)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                new UpdateFinding(
                    UpdateFindingCode.ConfirmationRequired,
                    target: null,
                    "Update requires explicit automatic mode when interactive confirmation is unavailable."));
        }

        try
        {
            var response = await _interactiveSession
                .AskAsync(ConfirmationPrompt, cancellationToken)
                .ConfigureAwait(false);
            if (response.IsEndOfInput
                || response.Answer is not { } answer
                || !IsAcceptedAnswer(answer))
            {
                return UpdateResultBuilder.BeforeEffects(
                    execution,
                    new UpdateFinding(
                        UpdateFindingCode.Interrupted,
                        target: null,
                        "Update confirmation was refused or reached end of input."));
            }

            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return UpdateResultBuilder.BeforeEffects(
                execution,
                new UpdateFinding(
                    UpdateFindingCode.Interrupted,
                    target: null,
                    "Update confirmation was interrupted."));
        }
    }

    private static bool IsAcceptedAnswer(string answer)
    {
        var normalized = answer.Trim();
        return string.Equals(normalized, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase);
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
