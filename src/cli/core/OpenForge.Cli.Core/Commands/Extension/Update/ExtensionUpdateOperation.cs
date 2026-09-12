using OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update;

internal sealed class ExtensionUpdateOperation(
    ExtensionUpdatePlanner planner,
    MutationPreflight preflight,
    ExtensionPermissionOperation permissions,
    ExtensionUpdateApplicationOperation application)
{
    private readonly ExtensionUpdatePlanner _planner = planner;
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly MutationPreflight _preflight = preflight;
    private readonly ExtensionUpdateApplicationOperation _application = application;

    internal async ValueTask<ExtensionUpdateResult> ExecuteAsync(
        ExtensionUpdateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.All && request.RequestedIds.Count > 0)
        {
            return ExtensionUpdateResult.Empty(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Prune,
                request.Automatic,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.InvalidInput,
                    "Explicit Extension IDs and --all cannot be combined."));
        }

        ExtensionUpdatePlanBuild build;
        try
        {
            build = await _planner.BuildAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionUpdateResult.Empty(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Prune,
                request.Automatic,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.Interrupted,
                    "Extension Update planning was interrupted."));
        }
        catch (Exception)
        {
            return ExtensionUpdateResult.Empty(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Prune,
                request.Automatic,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.OperationFailed,
                    "Extension Update planning failed unexpectedly."));
        }

        if (build.Plan is not { } plan)
        {
            return build.Result;
        }

        MutationValidationResult preflight;
        try
        {
            preflight = await _preflight.ValidateAsync(
                request.Workspace,
                plan.DirectoryCreations,
                plan.AllFileChanges,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanBoundary(
                plan,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update preflight was interrupted.");
        }
        catch (Exception)
        {
            return PlanBoundary(
                plan,
                ExtensionUpdateFindingCode.OperationFailed,
                "Extension Update preflight failed unexpectedly.");
        }

        if (preflight.State != MutationValidationState.Valid)
        {
            return PlanBoundary(
                plan,
                preflight.State == MutationValidationState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : ExtensionUpdateFindingCode.TargetUnsafe,
                preflight.Cause
                    ?? "The complete Extension Update plan is stale, unavailable, or unsafe.");
        }

        ExtensionPermissionStage permission;
        try
        {
            var selectedIds = plan.Packages.Select(package => package.Id).ToHashSet(StringComparer.Ordinal);
            var requirements = plan.Packages.SelectMany(package => package.Payload
                .Where(file => file.TargetPath is { } path && !ExtensionDestinationPolicy.IsImplicit(path))
                .Select(file => new WorkspacePermissionRequirement(new ExtensionPermissionSubject(package.Id),
                    file.TargetPath ?? throw new InvalidOperationException("A planned Extension target requires its path."))))
                .Concat(plan.CurrentLifecycle.Packages.Where(package => selectedIds.Contains(package.Id))
                    .SelectMany(package => package.Paths.Where(path => !ExtensionDestinationPolicy.IsImplicit(path))
                        .Select(path => new WorkspacePermissionRequirement(new ExtensionPermissionSubject(package.Id), path))));
            var targets = requirements.Select(requirement => new ExtensionPermissionTarget(requirement, ReadPermissionEffect(plan, requirement)));
            permission = await _permissions.DetermineAsync(new(request.Workspace, [.. targets], plan.SourceRead.Identity,
                request.Mode == ExtensionUpdateMode.Apply && !request.Automatic && request.AllowInteraction), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanBoundary(plan, ExtensionUpdateFindingCode.Interrupted, "Extension permission approval was interrupted.");
        }
        catch (Exception)
        {
            return PlanBoundary(plan, ExtensionUpdateFindingCode.OperationFailed, "Extension permission determination failed unexpectedly.");
        }
        if (permission.Failure is { } failure)
        {
            return PlanBoundary(plan, ExtensionUpdateDefinitions.ReadPermissionFinding(failure),
                "The requested Extension destinations require valid consumer permission.") with
            { Permissions = permission.Result };
        }
        var execution = new ExtensionUpdateExecutionPlan(plan, permission);
        if (request.Mode == ExtensionUpdateMode.DryRun || execution.IsNoOp)
        {
            return build.Result with { Permissions = permission.Result };
        }
        var outcome = await _application.ExecuteAsync(execution, cancellationToken).ConfigureAwait(false);
        return Application(plan, outcome) with { Permissions = outcome.Permissions };
    }

    private static ExtensionPermissionEffect ReadPermissionEffect(ExtensionUpdatePlan plan, WorkspacePermissionRequirement requirement)
    {
        var change = plan.Effects.FirstOrDefault(effect => effect.Result.Path == requirement.Path)?.FileChange;
        if (change is not null)
        {
            return change.Kind == PlannedFileChangeKind.Delete ? ExtensionPermissionEffect.Delete : ExtensionPermissionEffect.Copy;
        }
        var retained = plan.IntendedLifecycle.Packages.Any(package => package.Id == requirement.Subject.Id && package.Paths.Contains(requirement.Path));
        return retained ? ExtensionPermissionEffect.Preserve : ExtensionPermissionEffect.ReleaseOwnership;
    }

    private static ExtensionUpdateResult PlanBoundary(
        ExtensionUpdatePlan plan,
        ExtensionUpdateFindingCode code,
        string cause)
        => ExtensionUpdateResultFormationFactory.Create(
            plan.Request,
            ApplicationFacts(
                plan,
                effects: [],
                plan.Result.Lifecycle,
                plan.RequiresRecovery
                    ? new ExtensionUpdateRecovery(
                        ExtensionUpdateRecoveryState.NotCreated,
                        [],
                        residualPath: null)
                    : plan.Result.Recovery,
                new ExtensionUpdateVerification(
                    ExtensionUpdateVerificationState.NotRequested,
                    ExtensionUpdateVerificationState.NotRequested,
                    ExtensionUpdateVerificationState.NotRequested)),
            [.. plan.Result.Findings, new ExtensionUpdateFinding(code, cause)]);

    private static ExtensionUpdateResult Application(
        ExtensionUpdatePlan plan,
        ExtensionUpdateApplicationOutcome outcome)
        => ExtensionUpdateResultFormationFactory.Create(
            plan.Request,
            ApplicationFacts(
                plan,
                outcome.Effects,
                outcome.Lifecycle,
                outcome.Recovery,
                outcome.Verification),
            outcome.Finding is null
                ? plan.Result.Findings
                : [.. plan.Result.Findings, outcome.Finding]);

    private static ExtensionUpdateResultFacts ApplicationFacts(
        ExtensionUpdatePlan plan,
        IReadOnlyList<Models.Effects.ExtensionUpdateEffect> effects,
        ExtensionUpdateLifecycle lifecycle,
        ExtensionUpdateRecovery recovery,
        ExtensionUpdateVerification verification)
        => new()
        {
            Selection = plan.Result.Selection,
            Source = plan.Result.Source,
            Packages = plan.Result.Packages,
            Comparisons = plan.Result.Comparisons,
            GeneratedNavigation = plan.Result.GeneratedNavigation,
            Effects = effects,
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
        };
}
