using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove;

internal sealed class ExtensionRemoveOperation(
    ExtensionRemovePlanner planner,
    MutationPreflight preflight,
    ExtensionPermissionOperation permissions,
    ExtensionRemoveApplicationOperation application,
    ExtensionRemoveInteraction interaction)
{
    private readonly ExtensionRemovePlanner _planner = planner;
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly MutationPreflight _preflight = preflight;
    private readonly ExtensionRemoveApplicationOperation _application = application;
    private readonly ExtensionRemoveInteraction _interaction = interaction
        ?? throw new ArgumentNullException(nameof(interaction));

    internal async ValueTask<ExtensionRemoveResult> ExecuteAsync(
        ExtensionRemoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ExtensionRemovePlanBuild build;
        try
        {
            build = await _planner.BuildAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionRemoveResult.Empty(
                request.Workspace,
                request.Mode,
                request.Automatic,
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.Interrupted,
                    "Extension remove was cancelled. Nothing was changed."));
        }
        catch (Exception)
        {
            return ExtensionRemoveResult.Empty(
                request.Workspace,
                request.Mode,
                request.Automatic,
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.OperationFailed,
                    "Extension Remove planning failed unexpectedly."));
        }

        if (build.Plan is not { } plan)
        {
            return build.Result;
        }

        var changes = ExtensionRemoveApplicationOperation.ReadChanges(plan);
        var directories = ExtensionRemoveApplicationOperation.ReadDirectoryCreations(plan);
        MutationValidationResult preflight;
        try
        {
            preflight = directories.Count == 0
                ? await _preflight.ValidateAsync(
                    request.Workspace,
                    changes,
                    cancellationToken).ConfigureAwait(false)
                : await _preflight.ValidateAsync(
                    request.Workspace,
                    directories,
                    changes,
                    cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(
                plan,
                build.Result,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension remove was cancelled. Nothing was changed.");
        }
        catch (Exception)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(
                plan,
                build.Result,
                ExtensionRemoveFindingCode.OperationFailed,
                "Extension Remove preflight failed unexpectedly.");
        }

        if (preflight.State != MutationValidationState.Valid)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(
                plan,
                build.Result,
                preflight.State == MutationValidationState.Cancelled
                    ? ExtensionRemoveFindingCode.Interrupted
                    : ExtensionRemoveFindingCode.TargetUnsafe,
                preflight.Cause
                    ?? "The complete Extension Remove plan is stale, unavailable, or unsafe.");
        }

        ExtensionPermissionStage permission;
        try
        {
            var required = plan.Planning.Decisions.Select(decision => decision.Path)
                .Where(path => !ExtensionDestinationPolicy.IsImplicit(path.Path))
                .Select(path => new ExtensionPermissionTarget(path.Path,
                    path.Action == ExtensionRemovePathAction.Delete ? ExtensionPermissionEffect.Delete : ExtensionPermissionEffect.ReleaseOwnership));
            permission = await _permissions.DetermineAsync(new(request.Workspace, [.. required], SourceIdentity: null,
                AllowPrompt: !request.IsDryRun && !request.Automatic && request.AllowInteraction,
                ExplicitGrantPaths: request.AllowPath), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(plan, build.Result, ExtensionRemoveFindingCode.Interrupted,
                "Extension remove was cancelled. Nothing was changed.");
        }
        catch (Exception)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(plan, build.Result, ExtensionRemoveFindingCode.OperationFailed,
                "Extension permission determination failed unexpectedly.");
        }
        if (permission.Failure is { } failure)
        {
            var cause = failure == ExtensionPermissionFailure.Interrupted
                ? "Extension remove was cancelled. Nothing was changed."
                : "The requested Extension destinations require shared permission in .agents/open-forge.json; use --allow-path <path> to record a grant.";
            return ExtensionRemoveResultFactory.PlanBoundary(plan, build.Result, ExtensionRemoveDefinitions.ReadPermissionFinding(failure),
                cause) with
            { Permissions = permission.Result };
        }
        if (permission.Observation is { } permissionObservation
            && !plan.SettingsObservation.MatchesObservation(permissionObservation))
        {
            return ExtensionRemoveResultFactory.PlanBoundary(
                plan,
                build.Result with { Permissions = permission.Result },
                ExtensionRemoveFindingCode.TargetChanged,
                "Workspace settings changed after Extension Remove planning.");
        }

        var execution = ExtensionRemoveExecutionPlanComposer.Compose(plan, permission);
        var planned = build.Result with { Permissions = permission.Result };
        if (request.IsDryRun || execution.IsNoOp)
        {
            return planned;
        }

        if (!request.Automatic)
        {
            if (!request.AllowInteraction)
            {
                return ExtensionRemoveResultFactory.PlanBoundary(plan, planned,
                    ExtensionRemoveFindingCode.ConfirmationRequired,
                    "Extension remove needs confirmation, and this session cannot ask.");
            }

            var preview = PreviewRequest(request, planned);
            var deleteCount = plan.Effects.Count(effect =>
                effect.FileChange?.Kind == OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.PlannedFileChangeKind.Delete);
            var question = _interaction.DeleteQuestion(deleteCount);
            CliPromptReply<bool> approval;
            try
            {
                approval = await _interaction.Apply(
                    preview,
                    question,
                    new CliPromptPolicy(!request.Automatic && request.AllowInteraction),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return ExtensionRemoveResultFactory.PlanBoundary(plan, planned,
                    ExtensionRemoveFindingCode.Interrupted,
                    "Extension remove was cancelled. Nothing was changed.");
            }
            if (approval.State == CliPromptState.Unavailable)
            {
                return ExtensionRemoveResultFactory.PlanBoundary(plan, planned,
                    ExtensionRemoveFindingCode.ConfirmationRequired,
                    "Extension remove needs confirmation, and this session cannot ask.");
            }
            if (approval.State != CliPromptState.Answered || !approval.Value)
            {
                return ExtensionRemoveResultFactory.PlanBoundary(plan, planned,
                    ExtensionRemoveFindingCode.Interrupted,
                    "Extension remove was cancelled. Nothing was changed.");
            }
        }
        return await _application.ExecuteAsync(execution, planned, cancellationToken).ConfigureAwait(false);
    }

    private static ExtensionRemoveResult PreviewRequest(
        ExtensionRemoveRequest request,
        ExtensionRemoveResult planned)
        => new ExtensionRemoveResult(new ExtensionRemoveResultFormation
        {
            Workspace = request.Workspace,
            Mode = ExtensionRemoveMode.DryRun,
            Automatic = request.Automatic,
            Facts = new ExtensionRemoveResultFacts
            {
                Selection = planned.Selection,
                Dependencies = planned.Dependencies,
                Paths = planned.Paths,
                GeneratedNavigation = planned.GeneratedNavigation,
                Effects = planned.Effects,
                Permissions = planned.Permissions,
                Lifecycle = planned.Lifecycle,
                Recovery = planned.Recovery,
                Verification = planned.Verification,
                PackageSourceUnchanged = planned.PackageSourceUnchanged,
            },
            Findings = planned.Findings,
        });
}
