using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Completion;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal sealed class LibraryPermissionOperation(CliInteractiveSession interaction)
{
    private readonly CliInteractiveSession _interaction = interaction;
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<LibraryPermissionStage> DetermineAsync(LibraryPermissionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var approval = LibraryPermissionScopePlanner.Plan(request, WorkspacePermissionDocument.Empty);
        if (approval.Leaves.Required.IsEmpty)
        {
            return CreateStage(observation: null, approval, change: null, failure: null);
        }
        var observation = await WorkspacePermissionReader.ReadAsync(_resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        var failure = observation.State switch
        {
            WorkspacePermissionReadState.Missing or WorkspacePermissionReadState.Complete => (LibraryPermissionFailure?)null,
            WorkspacePermissionReadState.Invalid or WorkspacePermissionReadState.Blocked => LibraryPermissionFailure.Invalid,
            WorkspacePermissionReadState.Unavailable => LibraryPermissionFailure.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(request), observation.State, "The permission observation state is not defined."),
        };
        if (failure is not null)
        {
            return CreateStage(observation, approval with
            {
                Leaves = approval.Leaves with { Decision = WorkspacePermissionDecision.NotEvaluated },
                ProposedScopes = [],
            }, change: null, failure);
        }
        var document = WorkspacePermissionDocument.Empty;
        if (observation.State == WorkspacePermissionReadState.Complete)
        {
            document = observation.Document ?? throw new InvalidOperationException("A complete permission observation requires its document.");
        }
        approval = LibraryPermissionScopePlanner.Plan(request, document);
        if (!approval.Leaves.Missing.IsEmpty && request.AllowPrompt && _interaction.CanPrompt)
        {
            try
            {
                var response = await _interaction.AskAsync(LibraryPermissionPresentation.RenderPrompt(request, approval), cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                var answer = response.Answer?.Trim();
                var accepted = string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
                approval = approval with
                {
                    Leaves = approval.Leaves with { Decision = accepted ? WorkspacePermissionDecision.Approved : WorkspacePermissionDecision.Declined },
                    ApprovedScopes = accepted ? approval.ProposedScopes : [],
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return CreateStage(observation, approval, change: null, LibraryPermissionFailure.Interrupted);
            }
        }
        PlannedFileChange? change = null;
        if (approval.Leaves.Decision == WorkspacePermissionDecision.Approved)
        {
            change = WorkspacePermissionChangePlanner.PlanLibrary(new(observation, new()
            {
                Subject = new LibraryPermissionSubject(request.Library.Id.Value, request.Library.SourceRoot.Value),
                ApprovedScopes = approval.ApprovedScopes,
                PreviousSourceRoot = approval.Rebinding?.PreviousSourceRoot,
            }));
        }
        failure = approval.Leaves.Decision switch
        {
            WorkspacePermissionDecision.Required => LibraryPermissionFailure.Required,
            WorkspacePermissionDecision.Declined => LibraryPermissionFailure.Declined,
            WorkspacePermissionDecision.Granted or WorkspacePermissionDecision.Approved or WorkspacePermissionDecision.NotRequired => (LibraryPermissionFailure?)null,
            WorkspacePermissionDecision.NotEvaluated => throw new InvalidOperationException("A complete permission evaluation requires a decision."),
            _ => throw new ArgumentOutOfRangeException(nameof(request), approval.Leaves.Decision, "The permission decision is not defined."),
        };
        return CreateStage(observation, approval, change, failure);
    }

    internal static async ValueTask<bool> RevalidateAsync(WorkspaceLockLease lease, LibraryPermissionStage stage, CancellationToken cancellationToken)
    {
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            throw new ArgumentException("Library permission revalidation requires a live same-workspace lease.", nameof(lease));
        }
        if (stage.Observation is not { } before)
        {
            return stage.Result.Decision == WorkspacePermissionDecision.NotRequired;
        }
        var current = await WorkspacePermissionReader.ReadAsync(new PhysicalPathResolver(), lease.Request.Workspace, cancellationToken).ConfigureAwait(false);
        return before.State == current.State
            && before.Snapshot is { } expected && current.Snapshot is { } actual
            && expected.Expectation == actual.Expectation
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());
    }

    internal static async ValueTask<LibraryPermissionApplication> ApplyAsync(
        WorkspaceLockLease lease,
        LibraryPermissionStage stage,
        RecoveryBundlePreparation? recovery,
        CancellationToken cancellationToken)
    {
        if (stage.Change is not { } change)
        {
            return new(stage.Result, Receipt: null, stage.Failure);
        }
        var result = stage.Result with { Outcome = WorkspacePermissionOutcome.NotStarted };
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        FileExpectationValidationResult check;
        try
        {
            check = await validator.ValidateAsync(lease.Request.Workspace, change.Expectation, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new(result, Receipt: null, LibraryPermissionFailure.Interrupted);
        }
        catch (Exception)
        {
            return new(result, Receipt: null, LibraryPermissionFailure.WriteFailed);
        }
        if (check.State != FileExpectationValidationState.Matched)
        {
            return new(result, Receipt: null,
                check.State == FileExpectationValidationState.Cancelled ? LibraryPermissionFailure.Interrupted : LibraryPermissionFailure.Changed);
        }
        var applier = new FileChangeApplier(new MutationRevalidator(validator), validator);
        var receipt = await applier.ApplyAsync(lease, change, check, recovery, cancellationToken).ConfigureAwait(false);
        result = result with { Outcome = WorkspacePermissionReceiptProjection.ReadOutcome(receipt) };
        if (receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled)
        {
            return new(result, receipt, LibraryPermissionFailure.Interrupted);
        }
        return new(result, receipt, result.Outcome == WorkspacePermissionOutcome.Verified ? null : LibraryPermissionFailure.WriteFailed);
    }


    private static LibraryPermissionStage CreateStage(
        WorkspacePermissionRead? observation,
        LibraryPermissionApproval approval,
        PlannedFileChange? change,
        LibraryPermissionFailure? failure)
    {
        var action = change?.Kind switch
        {
            null => WorkspacePermissionAction.None,
            PlannedFileChangeKind.Create => WorkspacePermissionAction.Create,
            PlannedFileChangeKind.Replace => WorkspacePermissionAction.Replace,
            _ => throw new InvalidOperationException("Permission approval can only create or replace its consumer document."),
        };
        RecoveryBundleTarget? recovery = null;
        if (change is not null && observation?.Snapshot is { } before)
        {
            recovery = change.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(change, before)
                : RecoveryBundleTarget.Create(change, before);
        }
        return new()
        {
            Observation = observation,
            Approval = approval,
            Result = new(approval.Leaves.Required, approval.Leaves.Missing, approval.Leaves.Decision, action,
                change is null ? WorkspacePermissionOutcome.NotRequested : WorkspacePermissionOutcome.Planned),
            Change = change,
            RecoveryTarget = recovery,
            Failure = failure,
        };
    }
}
