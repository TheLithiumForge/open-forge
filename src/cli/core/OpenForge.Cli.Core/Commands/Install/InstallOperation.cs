using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Install;

internal sealed class InstallOperation(
    CliInteractiveSession interactiveSession,
    InstallPlanBuilder planBuilder,
    MutationPreflight preflight,
    InstallApplicationOperation applicationOperation)
{
    private const string ConfirmationPrompt = "Apply this Install plan? [y/N] ";

    private readonly CliInteractiveSession _interactiveSession = interactiveSession;
    private readonly InstallPlanBuilder _planBuilder = planBuilder;
    private readonly MutationPreflight _preflight = preflight;
    private readonly InstallApplicationOperation _applicationOperation = applicationOperation;

    internal async ValueTask<InstallResult> ExecuteAsync(
        InstallRequest request,
        CancellationToken cancellationToken)
    {
        InstallPlanBuild build;
        try
        {
            build = await _planBuilder.BuildAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return InitialResult(
                request,
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "Install planning was interrupted.");
        }
        catch (Exception)
        {
            return InitialResult(
                request,
                InstallManagementState.Blocked,
                InstallFindingCode.OperationFailed,
                "Install planning failed unexpectedly.");
        }

        if (build.Plan is not { } plan)
        {
            return InstallResult.Create(
                request,
                build.Findings,
                Summary(
                    build.ManagementState,
                    plannedDirectories: 0,
                    plannedFiles: 0),
                InstallResultFactsFactory.FromBuild(build));
        }

        if (!plan.IsComplete)
        {
            return InstallResult.Create(
                request,
                plan.Findings,
                Summary(
                    plan.ManagementState,
                    plannedDirectories: 0,
                    plannedFiles: 0),
                InstallResultFactsFactory.PlanBoundary(plan));
        }

        if (plan.IsNoOp)
        {
            return InstallResult.Create(
                request,
                findings: [],
                Summary(
                    plan.ManagementState,
                    plannedDirectories: 0,
                    plannedFiles: 0),
                InstallResultFactsFactory.NoOp(plan));
        }

        MutationValidationResult validation;
        try
        {
            validation = await _preflight.ValidateAsync(
                    request.Workspace,
                    plan.DirectoryCreations,
                    plan.FileChanges,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanResult(
                plan,
                InstallFindingCode.Interrupted,
                "Install preflight was interrupted.");
        }
        catch (Exception)
        {
            return PlanResult(
                plan,
                InstallFindingCode.OperationFailed,
                "Install preflight failed unexpectedly.");
        }

        if (validation.State != MutationValidationState.Valid)
        {
            return PlanResult(
                plan,
                validation.State == MutationValidationState.Cancelled
                    ? InstallFindingCode.Interrupted
                    : InstallFindingCode.TargetUnsafe,
                validation.Cause
                    ?? "The complete Install plan is stale, unavailable, or unsafe.");
        }

        if (request.IsDryRun)
        {
            return InstallResult.Create(
                request,
                findings: [],
                Summary(
                    plan.ManagementState,
                    plan.DirectoryCreations.Count,
                    plan.FileChanges.Count),
                InstallResultFactsFactory.DryRun(plan));
        }

        var interactionBoundary = await ConfirmAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        if (interactionBoundary is not null)
        {
            return interactionBoundary;
        }

        InstallApplicationOutcome outcome;
        try
        {
            outcome = await _applicationOperation.ExecuteAsync(
                    plan,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ApplicationUnknownResult(
                plan,
                InstallFindingCode.Interrupted,
                "Install application was interrupted.");
        }
        catch (Exception)
        {
            return ApplicationUnknownResult(
                plan,
                InstallFindingCode.OperationFailed,
                "Install application failed unexpectedly.");
        }

        return InstallResult.Create(
            request,
            outcome.Findings,
            outcome.Summary,
            outcome.Facts);
    }

    private async ValueTask<InstallResult?> ConfirmAsync(
        InstallPlan plan,
        CancellationToken cancellationToken)
    {
        if (plan.Request.Automatic)
        {
            return null;
        }

        if (!plan.Request.AllowsInteractiveConfirmation
            || !_interactiveSession.CanPrompt)
        {
            return PlanResult(
                plan,
                InstallFindingCode.ConfirmationRequired,
                "Install requires explicit automatic mode when interactive confirmation is unavailable.");
        }

        try
        {
            var response = await _interactiveSession.AskAsync(
                    ConfirmationPrompt,
                    cancellationToken)
                .ConfigureAwait(false);
            if (response.IsEndOfInput
                || response.Answer is not { } answer
                || !IsAcceptedAnswer(answer))
            {
                return PlanResult(
                    plan,
                    InstallFindingCode.Interrupted,
                    "Install confirmation was refused or reached end of input.");
            }

            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanResult(
                plan,
                InstallFindingCode.Interrupted,
                "Install confirmation was interrupted.");
        }
    }

    private static bool IsAcceptedAnswer(string answer)
    {
        var normalized = answer.Trim();
        return string.Equals(normalized, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static InstallResult InitialResult(
        InstallRequest request,
        InstallManagementState state,
        InstallFindingCode code,
        string cause)
        => InstallResult.Create(
            request,
            [new InstallFinding(code, cause)],
            Summary(state, plannedDirectories: 0, plannedFiles: 0));

    private static InstallResult PlanResult(
        InstallPlan plan,
        InstallFindingCode code,
        string cause)
        => InstallResult.Create(
            plan.Request,
            [new InstallFinding(code, cause)],
            Summary(
                plan.ManagementState,
                plan.DirectoryCreations.Count,
                plan.FileChanges.Count),
            InstallResultFactsFactory.PlanBoundary(plan));

    private static InstallResult ApplicationUnknownResult(
        InstallPlan plan,
        InstallFindingCode code,
        string cause)
        => InstallResult.Create(
            plan.Request,
            [new InstallFinding(code, cause)],
            Summary(
                plan.ManagementState,
                plan.DirectoryCreations.Count,
                plan.FileChanges.Count),
            InstallResultFactsFactory.ApplicationUnknown(plan));

    private static InstallOperationSummary Summary(
        InstallManagementState managementState,
        int plannedDirectories,
        int plannedFiles)
        => new()
        {
            ManagementState = managementState,
            PlannedDirectoryCount = plannedDirectories,
            PlannedFileCount = plannedFiles,
            AppliedDirectoryCount = 0,
            AppliedTargetFileCount = 0,
            LifecyclePublished = false,
            RecoveryState = InstallRecoveryState.NotRequired,
            RecoveryResidualPath = null,
        };
}
