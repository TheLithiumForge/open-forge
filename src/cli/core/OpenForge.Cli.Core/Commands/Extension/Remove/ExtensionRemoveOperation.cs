using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove;

internal sealed class ExtensionRemoveOperation(
    ExtensionRemovePlanner planner,
    MutationPreflight preflight,
    ExtensionPermissionOperation permissions,
    ExtensionRemoveApplicationOperation application)
{
    private readonly ExtensionRemovePlanner _planner = planner;
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly MutationPreflight _preflight = preflight;
    private readonly ExtensionRemoveApplicationOperation _application = application;

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
                request.Prune,
                request.Automatic,
                new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.Interrupted,
                    "Extension Remove planning was interrupted."));
        }
        catch (Exception)
        {
            return ExtensionRemoveResult.Empty(
                request.Workspace,
                request.Mode,
                request.Prune,
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
        MutationValidationResult preflight;
        try
        {
            preflight = await _preflight.ValidateAsync(
                request.Workspace,
                changes,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(
                plan,
                build.Result,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove preflight was interrupted.");
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
                .SelectMany(path => path.SelectedOwnerIds.Select(id =>
                    new ExtensionPermissionTarget(new WorkspacePermissionRequirement(new ExtensionPermissionSubject(id), path.Path),
                        path.Action == ExtensionRemovePathAction.Delete ? ExtensionPermissionEffect.Delete : ExtensionPermissionEffect.ReleaseOwnership)));
            permission = await _permissions.DetermineAsync(new(request.Workspace, [.. required], SourceIdentity: null,
                AllowPrompt: !request.IsDryRun && !request.Automatic && request.AllowInteraction), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(plan, build.Result, ExtensionRemoveFindingCode.Interrupted,
                "Extension permission approval was interrupted.");
        }
        catch (Exception)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(plan, build.Result, ExtensionRemoveFindingCode.OperationFailed,
                "Extension permission determination failed unexpectedly.");
        }
        if (permission.Failure is { } failure)
        {
            return ExtensionRemoveResultFactory.PlanBoundary(plan, build.Result, ExtensionRemoveDefinitions.ReadPermissionFinding(failure),
                "The requested Extension destinations require valid consumer permission.") with
            { Permissions = permission.Result };
        }
        var execution = new ExtensionRemoveExecutionPlan(plan, permission);
        var planned = build.Result with { Permissions = permission.Result };
        if (request.IsDryRun || execution.IsNoOp)
        {
            return planned;
        }
        return await _application.ExecuteAsync(execution, planned, cancellationToken).ConfigureAwait(false);
    }
}
