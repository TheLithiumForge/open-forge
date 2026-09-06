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
    ExtensionRemoveApplicationOperation application)
{
    private readonly ExtensionRemovePlanner _planner = planner;
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

        if (build.Plan is not { } plan || plan.IsNoOp)
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

        return request.IsDryRun
            ? build.Result
            : await _application.ExecuteAsync(plan, build.Result, cancellationToken)
                .ConfigureAwait(false);
    }
}
