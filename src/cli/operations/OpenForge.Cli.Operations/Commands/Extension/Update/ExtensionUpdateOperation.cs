using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update;

internal sealed class ExtensionUpdateOperation(
    ExtensionUpdatePlanner planner,
    MutationPreflight preflight,
    ExtensionPermissionOperation permissions,
    ExtensionUpdateApplicationOperation application,
    ExtensionUpdateInteraction interaction)
{
    private readonly ExtensionUpdatePlanner _planner = planner;
    private readonly ExtensionPermissionOperation _permissions = permissions;
    private readonly MutationPreflight _preflight = preflight;
    private readonly ExtensionUpdateApplicationOperation _application = application;
    private readonly ExtensionUpdateInteraction _interaction = interaction
        ?? throw new ArgumentNullException(nameof(interaction));

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
                    "Extension update was cancelled. Nothing was changed."));
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
                "Extension update was cancelled. Nothing was changed.");
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
                .Select(file => file.TargetPath ?? throw new InvalidOperationException("A planned Extension target requires its path.")))
                .Concat(plan.Ownership.Document.Extensions.Where(package => selectedIds.Contains(package.Id))
                    .SelectMany(package => package.Paths.Where(path => !ExtensionDestinationPolicy.IsImplicit(path))))
                .Distinct(StringComparer.Ordinal);
            var targets = requirements.Select(requirement => new ExtensionPermissionTarget(requirement, ReadPermissionEffect(plan, requirement)));
            permission = await _permissions.DetermineAsync(new(request.Workspace, [.. targets], plan.SourceRead.Identity,
                request.Mode == ExtensionUpdateMode.Apply && !request.Automatic && request.AllowInteraction,
                request.AllowPath), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PlanBoundary(plan, ExtensionUpdateFindingCode.Interrupted,
                "Extension update was cancelled. Nothing was changed.");
        }
        catch (Exception)
        {
            return PlanBoundary(plan, ExtensionUpdateFindingCode.OperationFailed, "Extension permission determination failed unexpectedly.");
        }
        if (permission.Failure is { } failure)
        {
            var cause = failure == ExtensionPermissionFailure.Interrupted
                ? "Extension update was cancelled. Nothing was changed."
                : "The requested Extension destinations require shared permission in .agents/open-forge.json; use --allow-path <path> to record a grant.";
            return PlanBoundary(plan, ExtensionUpdateDefinitions.ReadPermissionFinding(failure),
                cause) with
            { Permissions = permission.Result };
        }
        var execution = new ExtensionUpdateExecutionPlan(plan, permission);
        if (request.Mode == ExtensionUpdateMode.DryRun || execution.IsNoOp)
        {
            return build.Result with { Permissions = permission.Result };
        }

        if (!request.Automatic)
        {
            if (!request.AllowInteraction)
            {
                return PlanBoundary(plan, ExtensionUpdateFindingCode.ConfirmationRequired,
                    "Extension update needs confirmation, and this session cannot ask.",
                    permission.Result);
            }

            var preview = ExtensionUpdateResultFormationFactory.Create(
                PreviewRequest(plan.Request),
                ApplicationFacts(
                    plan,
                    effects: plan.Result.Effects,
                    plan.Result.Lifecycle,
                    plan.Result.Recovery,
                    plan.Result.Verification,
                    permission.Result),
                plan.Result.Findings);
            var deletionCount = plan.Effects.Count(effect =>
                effect.FileChange?.Kind == PlannedFileChangeKind.Delete);
            var question = request.Prune
                ? _interaction.PruneQuestion(deletionCount)
                : _interaction.ApplyQuestion;
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
                return PlanBoundary(plan, ExtensionUpdateFindingCode.Interrupted,
                    "Extension update was cancelled. Nothing was changed.",
                    permission.Result);
            }
            if (approval.State == CliPromptState.Unavailable)
            {
                return PlanBoundary(plan, ExtensionUpdateFindingCode.ConfirmationRequired,
                    "Extension update needs confirmation, and this session cannot ask.",
                    permission.Result);
            }
            if (approval.State != CliPromptState.Answered || !approval.Value)
            {
                return PlanBoundary(plan, ExtensionUpdateFindingCode.Interrupted,
                    "Extension update was cancelled. Nothing was changed.",
                    permission.Result);
            }
        }
        var outcome = await _application.ExecuteAsync(execution, cancellationToken).ConfigureAwait(false);
        return Application(plan, outcome) with { Permissions = outcome.Permissions };
    }

    private static ExtensionUpdateRequest PreviewRequest(ExtensionUpdateRequest request)
        => new(
            request.Workspace,
            ExtensionUpdateMode.DryRun,
            request.RequestedIds,
            request.All,
            request.SourcePath,
            request.Force,
            request.Prune,
            request.Automatic,
            request.AllowInteraction,
            request.AllowPath);

    private static ExtensionPermissionEffect ReadPermissionEffect(ExtensionUpdatePlan plan, string path)
    {
        var change = plan.Effects.FirstOrDefault(effect => effect.Result.Path == path)?.FileChange;
        if (change is not null)
        {
            return change.Kind == PlannedFileChangeKind.Delete ? ExtensionPermissionEffect.Delete : ExtensionPermissionEffect.Copy;
        }
        var retained = plan.IntendedOwnership.Any(package => package.Paths.Contains(path));
        return retained ? ExtensionPermissionEffect.Preserve : ExtensionPermissionEffect.ReleaseOwnership;
    }

    private static ExtensionUpdateResult PlanBoundary(
        ExtensionUpdatePlan plan,
        ExtensionUpdateFindingCode code,
        string cause,
        WorkspacePermissionResult? permissions = null)
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
                    ExtensionUpdateVerificationState.NotRequested),
                permissions),
            [.. plan.Result.Findings, new ExtensionUpdateFinding(code, cause)]);

    private static ExtensionUpdateResult Application(
        ExtensionUpdatePlan plan,
        ExtensionUpdateApplicationOutcome outcome)
    {
        var result = ExtensionUpdateResultFormationFactory.Create(
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
        if (result.Next is null
            && outcome.Verification.Targets == ExtensionUpdateVerificationState.Verified
            && outcome.Verification.Topology == ExtensionUpdateVerificationState.Verified
            && outcome.Recovery.State == ExtensionUpdateRecoveryState.Retained
            && outcome.Recovery.ResidualPath is { } bundlePath)
        {
            var gitPath = Path.Combine(plan.Request.Workspace.LexicalRoot, ".git");
            var hasGitDirectory = Directory.Exists(gitPath)
                && (File.GetAttributes(gitPath) & FileAttributes.ReparsePoint) == 0;
            return result with
            {
                Next = new CliNextAction(hasGitDirectory ? "git diff" : "open-forge doctor",
                    hasGitDirectory
                        ? $"Review the changes with git diff. Previous content remains in {bundlePath}."
                        : $"Review previous content in the recovery bundle at {bundlePath}."),
            };
        }
        return result;
    }

    private static ExtensionUpdateResultFacts ApplicationFacts(
        ExtensionUpdatePlan plan,
        IReadOnlyList<Models.Effects.ExtensionUpdateEffect> effects,
        ExtensionUpdateLifecycle lifecycle,
        ExtensionUpdateRecovery recovery,
        ExtensionUpdateVerification verification,
        WorkspacePermissionResult? permissions = null)
        => new()
        {
            Selection = plan.Result.Selection,
            Source = plan.Result.Source,
            Packages = plan.Result.Packages,
            Comparisons = plan.Result.Comparisons,
            GeneratedNavigation = plan.Result.GeneratedNavigation,
            Effects = effects,
            Permissions = permissions ?? plan.Result.Permissions,
            Lifecycle = lifecycle,
            Recovery = recovery,
            Verification = verification,
        };
}
