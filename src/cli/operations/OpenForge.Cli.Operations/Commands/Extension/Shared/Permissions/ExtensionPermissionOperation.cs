using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Commands.Shared.Permissions.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Completion;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal sealed class ExtensionPermissionOperation
{
    private readonly CliPrompt<CliPermissionQuestion, CliPermissionChoice> _permissionPrompt;
    private readonly PhysicalPathResolver _resolver = new();

    internal ExtensionPermissionOperation(
        CliPrompt<CliPermissionQuestion, CliPermissionChoice> permissionPrompt)
    {
        ArgumentNullException.ThrowIfNull(permissionPrompt);
        _permissionPrompt = permissionPrompt;
    }

    internal async ValueTask<ExtensionPermissionStage> DetermineAsync(
        ExtensionPermissionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var explicitGrants = request.Grants
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToImmutableArray();
        if (request.Targets.IsEmpty && explicitGrants.IsEmpty)
        {
            return new(null, WorkspacePermissionResult.NotEvaluated with { Decision = WorkspacePermissionDecision.NotRequired }, null, null, null);
        }

        var observation = await WorkspaceSettingsReader.ReadAsync(_resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        var requirements = request.Targets.Select(target => target.Path).ToImmutableArray();
        var evaluation = WorkspaceAllowList.Evaluate(observation.Document.AllowInstallPaths, requirements);
        var prospectiveAllowList = observation.Document.AllowInstallPaths
            .Concat(explicitGrants)
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        var prospectiveEvaluation = WorkspaceAllowList.Evaluate(prospectiveAllowList, requirements);
        var choice = CliPermissionChoice.Cancel;
        if (!prospectiveEvaluation.Missing.IsEmpty && request.AllowPrompt)
        {
            var missingTargets = request.Targets
                .Where(target => prospectiveEvaluation.Missing.Contains(target.Path))
                .ToArray();
            var response = await _permissionPrompt(
                new CliPermissionQuestion(
                    request.SourceIdentity ?? "Extension",
                    [.. missingTargets.Select(target => new CliPermissionPath(
                        target.Path,
                        IsDirectoryTarget(request.Workspace, target.Path)))]),
                new CliPromptPolicy(Allowed: true),
                cancellationToken).ConfigureAwait(false);
            if (response.State == CliPromptState.Unavailable)
            {
                return PermissionRequired(observation, evaluation);
            }
            if (response.State == CliPromptState.Cancelled)
            {
                return PermissionInterrupted(observation, evaluation);
            }
            choice = response.Value;
            if (choice == CliPermissionChoice.Cancel)
                return PermissionInterrupted(observation, evaluation);
        }
        PlannedFileChange? change = null;
        var grantPaths = explicitGrants;
        if (choice == CliPermissionChoice.Always)
        {
            grantPaths = grantPaths
                .Concat(prospectiveEvaluation.Missing)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToImmutableArray();
        }
        if (!grantPaths.IsEmpty)
        {
            if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
                || observation.Snapshot is null)
            {
                return new(observation, new(evaluation.Required, evaluation.Missing, evaluation.Decision,
                    WorkspacePermissionAction.None, WorkspacePermissionOutcome.NotRequested), null, null,
                    observation.State == WorkspaceSettingsReadState.Unavailable ? ExtensionPermissionFailure.Unavailable : ExtensionPermissionFailure.Invalid);
            }
            change = WorkspaceSettingsChangePlanner.PlanGrant(observation, grantPaths);
        }
        var (action, recovery) = WorkspaceSettingsChangePlanner.ReadActionAndRecovery(observation, change);
        var decision = choice switch
        {
            CliPermissionChoice.Always or CliPermissionChoice.Once
                => WorkspacePermissionDecision.Approved,
            _ when prospectiveEvaluation.Missing.IsEmpty => evaluation.Required.IsEmpty
                ? WorkspacePermissionDecision.NotRequired
                : WorkspacePermissionDecision.Granted,
            _ => evaluation.Decision,
        };
        ExtensionPermissionFailure? failure = decision switch
        {
            WorkspacePermissionDecision.Required => ExtensionPermissionFailure.Required,
            WorkspacePermissionDecision.Declined => ExtensionPermissionFailure.Declined,
            WorkspacePermissionDecision.Granted or WorkspacePermissionDecision.Approved or WorkspacePermissionDecision.NotRequired => (ExtensionPermissionFailure?)null,
            WorkspacePermissionDecision.NotEvaluated => throw new InvalidOperationException("A complete permission evaluation requires a decision."),
            _ => throw new ArgumentOutOfRangeException(nameof(request), evaluation.Decision, "The permission decision is not defined."),
        };
        return new(observation, new(evaluation.Required, evaluation.Missing, decision, action,
            change is null ? WorkspacePermissionOutcome.NotRequested : WorkspacePermissionOutcome.Planned), change, recovery, failure);
    }

    private static ExtensionPermissionStage PermissionRequired(
        WorkspaceSettingsRead observation,
        WorkspacePermissionEvaluation evaluation)
        => new(observation, new(evaluation.Required, evaluation.Missing, WorkspacePermissionDecision.Required,
            WorkspacePermissionAction.None, WorkspacePermissionOutcome.NotRequested), null, null,
            ExtensionPermissionFailure.Required);

    private static ExtensionPermissionStage PermissionInterrupted(
        WorkspaceSettingsRead observation,
        WorkspacePermissionEvaluation evaluation)
        => new(observation, new(evaluation.Required, evaluation.Missing, WorkspacePermissionDecision.Declined,
            WorkspacePermissionAction.None, WorkspacePermissionOutcome.NotRequested), null, null,
            ExtensionPermissionFailure.Interrupted);

    private static bool IsDirectoryTarget(CliWorkspace workspace, string relativePath)
    {
        try
        {
            return Directory.Exists(Path.Combine(workspace.LexicalRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    internal async ValueTask<bool> RevalidateAsync(
        WorkspaceLockLease lease,
        ExtensionPermissionStage stage,
        CancellationToken cancellationToken)
    {
        if (stage.Observation is not { } before)
        {
            return stage.Result.Decision == WorkspacePermissionDecision.NotRequired;
        }
        var current = await WorkspaceSettingsReader.ReadAsync(_resolver, lease.Request.Workspace, cancellationToken).ConfigureAwait(false);
        return before.MatchesObservation(current);
    }

    internal async ValueTask<ExtensionPermissionApplication> ApplyAsync(
        WorkspaceLockLease lease,
        ExtensionPermissionStage stage,
        RecoveryBundlePreparation? recovery,
        CancellationToken cancellationToken)
    {
        if (stage.Change is not { } change)
        {
            return new(stage.Result, null);
        }
        var result = stage.Result with { Outcome = WorkspacePermissionOutcome.NotStarted };
        if (recovery is null)
        {
            return new(result, ExtensionPermissionFailure.WriteFailed);
        }
        var validator = new FileExpectationValidator(_resolver);
        FileExpectationValidationResult check;
        try
        {
            check = await validator.ValidateAsync(lease.Request.Workspace, change.Expectation, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new(result, ExtensionPermissionFailure.Interrupted);
        }
        catch (Exception)
        {
            return new(result, ExtensionPermissionFailure.WriteFailed);
        }
        if (check.State != FileExpectationValidationState.Matched)
        {
            return new(result, check.State == FileExpectationValidationState.Cancelled ? ExtensionPermissionFailure.Interrupted : ExtensionPermissionFailure.Changed);
        }
        var applier = new FileChangeApplier(new MutationRevalidator(validator), validator);
        var receipt = await applier.ApplyAsync(lease, change, check, recovery, cancellationToken).ConfigureAwait(false);
        result = result with { Outcome = WorkspacePermissionReceiptProjection.ReadOutcome(receipt) };
        if (receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled)
        {
            return new(result, ExtensionPermissionFailure.Interrupted);
        }
        return new(result, result.Outcome == WorkspacePermissionOutcome.Verified ? null : ExtensionPermissionFailure.WriteFailed);
    }
}
