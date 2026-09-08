using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Completion;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;

internal sealed class ExtensionPermissionOperation(CliInteractiveSession interaction)
{
    private readonly CliInteractiveSession _interaction = interaction;
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<ExtensionPermissionStage> DetermineAsync(
        ExtensionPermissionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.Targets.IsEmpty)
        {
            return new(null, WorkspacePermissionResult.NotEvaluated with { Decision = WorkspacePermissionDecision.NotRequired }, null, null, null);
        }

        var observation = await WorkspacePermissionReader.ReadAsync(_resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        var failure = observation.State switch
        {
            WorkspacePermissionReadState.Missing or WorkspacePermissionReadState.Complete => (ExtensionPermissionFailure?)null,
            WorkspacePermissionReadState.Invalid or WorkspacePermissionReadState.Blocked => ExtensionPermissionFailure.Invalid,
            WorkspacePermissionReadState.Unavailable => ExtensionPermissionFailure.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(request), observation.State, "The permission observation state is not defined."),
        };
        if (failure is not null)
        {
            return new(observation, WorkspacePermissionResult.NotEvaluated with
            {
                Required = [.. request.Targets.Select(target => target.Requirement).Distinct()
                    .OrderBy(requirement => requirement.Subject.Id, StringComparer.Ordinal).ThenBy(requirement => requirement.Path, StringComparer.Ordinal)],
            }, null, null, failure);
        }

        var document = observation.State == WorkspacePermissionReadState.Missing
            ? WorkspacePermissionDocument.Empty
            : observation.Document ?? throw new InvalidOperationException("A complete permission observation requires its document.");
        var evaluation = WorkspacePermissionEvaluator.Evaluate(document, [.. request.Targets.Select(target => target.Requirement)]);
        if (!evaluation.Missing.IsEmpty && request.AllowPrompt && _interaction.CanPrompt)
        {
            var targets = request.Targets.Distinct().ToDictionary(target => target.Requirement);
            var paths = string.Join('\n', evaluation.Missing.Select(requirement =>
                $"  {requirement.Subject.Id}: {requirement.Path} ({ReadEffectName(targets[requirement].Effect)})"));
            var source = request.SourceIdentity is { } identity ? $"Source: {identity}\n" : string.Empty;
            CliInteractiveResponse response;
            try
            {
                response = await _interaction.AskAsync(
                    $"{source}Allow these Extension destinations and remember this permission in this workspace?\n{paths}\n[y/N] ", cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return new(observation, new(evaluation.Required, evaluation.Missing, evaluation.Decision,
                    WorkspacePermissionAction.None, WorkspacePermissionOutcome.NotRequested), null, null, ExtensionPermissionFailure.Interrupted);
            }
            var answer = response.Answer?.Trim();
            var approved = string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
            evaluation = evaluation with { Decision = approved ? WorkspacePermissionDecision.Approved : WorkspacePermissionDecision.Declined };
        }

        var change = WorkspacePermissionChangePlanner.Plan(new(observation, evaluation));
        var action = change?.Kind switch
        {
            null => WorkspacePermissionAction.None,
            PlannedFileChangeKind.Create => WorkspacePermissionAction.Create,
            PlannedFileChangeKind.Replace => WorkspacePermissionAction.Replace,
            _ => throw new InvalidOperationException("Permission approval can only create or replace its consumer document."),
        };
        RecoveryBundleTarget? recovery = null;
        if (change is not null && observation.Snapshot is { } before)
        {
            recovery = change.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(change, before)
                : RecoveryBundleTarget.Create(change, before);
        }
        failure = evaluation.Decision switch
        {
            WorkspacePermissionDecision.Required => ExtensionPermissionFailure.Required,
            WorkspacePermissionDecision.Declined => ExtensionPermissionFailure.Declined,
            WorkspacePermissionDecision.Granted or WorkspacePermissionDecision.Approved or WorkspacePermissionDecision.NotRequired => (ExtensionPermissionFailure?)null,
            WorkspacePermissionDecision.NotEvaluated => throw new InvalidOperationException("A complete permission evaluation requires a decision."),
            _ => throw new ArgumentOutOfRangeException(nameof(request), evaluation.Decision, "The permission decision is not defined."),
        };
        return new(observation, new(evaluation.Required, evaluation.Missing, evaluation.Decision, action,
            change is null ? WorkspacePermissionOutcome.NotRequested : WorkspacePermissionOutcome.Planned), change, recovery, failure);
    }

    internal static string ReadEffectName(ExtensionPermissionEffect effect)
        => effect switch
        {
            ExtensionPermissionEffect.Copy => "copy",
            ExtensionPermissionEffect.Delete => "delete",
            ExtensionPermissionEffect.ReleaseOwnership => "release ownership",
            ExtensionPermissionEffect.Preserve => "preserve content",
            _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, "The permission effect is not defined."),
        };

    internal async ValueTask<bool> RevalidateAsync(
        WorkspaceLockLease lease,
        ExtensionPermissionStage stage,
        CancellationToken cancellationToken)
    {
        if (stage.Observation is not { } before)
        {
            return stage.Result.Decision == WorkspacePermissionDecision.NotRequired;
        }
        var current = await WorkspacePermissionReader.ReadAsync(_resolver, lease.Request.Workspace, cancellationToken).ConfigureAwait(false);
        return before.State == current.State
            && before.Snapshot is { } expected && current.Snapshot is { } actual
            && expected.Expectation == actual.Expectation
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());
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
