using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
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

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal sealed class LibraryPermissionOperation
{
    private readonly CliPrompt<CliPermissionQuestion, CliPermissionChoice> _prompt;
    private readonly PhysicalPathResolver _resolver = new();

    internal LibraryPermissionOperation(
        CliPrompt<CliPermissionQuestion, CliPermissionChoice> prompt)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        _prompt = prompt;
    }

    internal async ValueTask<LibraryPermissionStage> DetermineAsync(LibraryPermissionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var explicitPaths = request.ExplicitGrantPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        var approval = LibraryPermissionScopePlanner.Plan(request);
        if (approval.Leaves.Required.IsEmpty && explicitPaths.IsEmpty)
        {
            return CreateStage(observation: null, approval, change: null, failure: null);
        }
        var observation = await WorkspaceSettingsReader.ReadAsync(_resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
        {
            var settingsFailure = observation.State == WorkspaceSettingsReadState.Unavailable
                ? LibraryPermissionFailure.Unavailable
                : LibraryPermissionFailure.Invalid;
            return CreateStage(observation, approval, change: null, settingsFailure);
        }

        approval = LibraryPermissionScopePlanner.Plan(
            request,
            [.. observation.Document.AllowInstallPaths.Concat(explicitPaths)]);
        var explicitChange = explicitPaths.IsEmpty
            ? null
            : WorkspaceSettingsChangePlanner.PlanGrant(observation, explicitPaths);
        var choice = CliPermissionChoice.Cancel;
        var promptUnavailable = false;
        if (!approval.Leaves.Missing.IsEmpty && request.AllowPrompt)
        {
            try
            {
                var response = await _prompt(
                    CreateQuestion(request, approval), new CliPromptPolicy(request.AllowPrompt), cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
                if (response.State == CliPromptState.Unavailable)
                {
                    promptUnavailable = true;
                }
                else if (response.State == CliPromptState.Cancelled)
                {
                    approval = approval with
                    {
                        Leaves = approval.Leaves with { Decision = WorkspacePermissionDecision.Declined },
                        ApprovedScopes = [],
                    };
                }
                else
                {
                    choice = response.Value;
                    var accepted = choice != CliPermissionChoice.Cancel;
                    approval = approval with
                    {
                        Leaves = approval.Leaves with { Decision = accepted ? WorkspacePermissionDecision.Approved : WorkspacePermissionDecision.Declined },
                        ApprovedScopes = accepted ? approval.ProposedScopes : [],
                    };
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return CreateStage(observation, approval, explicitChange, LibraryPermissionFailure.Interrupted);
            }
        }

        if (promptUnavailable)
        {
            return CreateStage(observation, approval, explicitChange, LibraryPermissionFailure.Required);
        }

        var grantPaths = explicitPaths
            .Concat(choice == CliPermissionChoice.Always
                ? approval.ApprovedScopes.Select(scope => scope.Path)
                : [])
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        PlannedFileChange? change = null;
        if (!grantPaths.IsEmpty)
        {
            change = WorkspaceSettingsChangePlanner.PlanGrant(observation, grantPaths);
        }
        LibraryPermissionFailure? permissionFailure;
        permissionFailure = approval.Leaves.Decision switch
        {
            WorkspacePermissionDecision.Required => LibraryPermissionFailure.Required,
            WorkspacePermissionDecision.Declined => LibraryPermissionFailure.Declined,
            WorkspacePermissionDecision.Granted or WorkspacePermissionDecision.Approved or WorkspacePermissionDecision.NotRequired => (LibraryPermissionFailure?)null,
            WorkspacePermissionDecision.NotEvaluated => throw new InvalidOperationException("A complete permission evaluation requires a decision."),
            _ => throw new ArgumentOutOfRangeException(nameof(request), approval.Leaves.Decision, "The permission decision is not defined."),
        };
        return CreateStage(observation, approval, change, permissionFailure);
    }

    private static CliPermissionQuestion CreateQuestion(
        LibraryPermissionRequest request,
        LibraryPermissionApproval approval)
        => new(
            request.Library.Id.Value,
            [.. approval.ProposedScopes.Select(scope => new CliPermissionPath(
                scope.Path,
                scope.Kind == LibraryPermissionScopeKind.Directory))]);

    internal static LibraryPermissionStage RefusedWrite() => new()
    {
        Observation = null,
        Approval = null,
        Result = WorkspacePermissionResult.NotEvaluated,
        Change = null,
        RecoveryTarget = null,
        Failure = LibraryPermissionFailure.WriteFailed,
    };

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
        var current = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), lease.Request.Workspace, cancellationToken).ConfigureAwait(false);
        return before.MatchesObservation(current);
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
        if (recovery is null)
        {
            return new(result, Receipt: null, LibraryPermissionFailure.WriteFailed);
        }
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
        WorkspaceSettingsRead? observation,
        LibraryPermissionApproval approval,
        PlannedFileChange? change,
        LibraryPermissionFailure? failure)
    {
        var (action, recovery) = WorkspaceSettingsChangePlanner.ReadActionAndRecovery(observation, change);
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
