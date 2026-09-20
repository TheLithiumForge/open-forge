using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Operation;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Install;

internal sealed class InstallOperation(
    CliPlanConfirmation<InstallResult, InstallConfirmationFacts> planConfirmation,
    InstallPlanBuilder planBuilder,
    MutationPreflight preflight,
    InstallApplicationOperation applicationOperation)
{
    private readonly CliPlanConfirmation<InstallResult, InstallConfirmationFacts> _planConfirmation =
        planConfirmation ?? throw new ArgumentNullException(nameof(planConfirmation));
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

        var preview = PlanPreview(plan);
        if (request.IsDryRun)
        {
            return preview;
        }

        var interactionBoundary = await ConfirmAsync(plan, preview, cancellationToken)
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
        InstallResult preview,
        CancellationToken cancellationToken)
    {
        if (plan.Request.Automatic)
        {
            return null;
        }

        try
        {
            var response = await _planConfirmation(
                    preview,
                    InstallConfirmationFacts.From(plan),
                    new CliPromptPolicy(plan.Request.AllowsInteractiveConfirmation),
                    cancellationToken)
                .ConfigureAwait(false);

            if (response.State == CliPromptState.Unavailable)
            {
                return PlanResult(
                    plan,
                    InstallFindingCode.ConfirmationRequired,
                    "Install requires explicit automatic mode when interactive confirmation is unavailable.");
            }

            if (response.State == CliPromptState.Answered && response.Value)
            {
                return null;
            }

            return PlanResult(
                plan,
                InstallFindingCode.Interrupted,
                "Install was cancelled. Nothing was changed.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanResult(
                plan,
                InstallFindingCode.Interrupted,
                "Install was cancelled. Nothing was changed.");
        }
    }

    private static InstallResult PlanPreview(InstallPlan plan)
        => new(
            workspace: plan.Request.Workspace,
            input: new InstallBindingInput(
                Force: plan.Request.Force,
                Automatic: plan.Request.Automatic,
                Mode: InstallMode.DryRun),
            findings: [],
            summary: Summary(
                plan.ManagementState,
                plan.DirectoryCreations.Count,
                plan.PlannedFileCount),
            facts: InstallResultFactsFactory.DryRun(plan));

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
                plan.PlannedFileCount),
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
                plan.PlannedFileCount),
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
