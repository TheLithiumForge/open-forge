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
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Completion;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal sealed class LibraryPermissionOperation
{
    private readonly CliPrompt<CliPermissionQuestion, CliPermissionChoice> _prompt;
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
        var observation = request.SettingsObservation;
        var approvedPaths = observation.Document.AllowInstallPaths.Concat(explicitPaths).ToImmutableArray();
        var approval = LibraryPermissionScopePlanner.Plan(request, approvedPaths);
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
        {
            var settingsFailure = observation.State == WorkspaceSettingsReadState.Unavailable
                ? LibraryPermissionFailure.Unavailable
                : LibraryPermissionFailure.Invalid;
            return CreateStage(observation, approval, change: null, grantChange: null, settingsFailure);
        }

        approval = LibraryPermissionScopePlanner.Plan(request, approvedPaths);
        var explicitChange = explicitPaths.IsEmpty ? null : WorkspaceSettingsChangePlanner.PlanGrant(observation, explicitPaths);
        var explicitRemoval = request.RemovalSelection is null
            ? null
            : WorkspaceSettingsChangePlanner.PlanRemovals(observation, request.RemovalSelection);
        var explicitCombined = ComposeChange(observation, explicitChange, explicitRemoval, explicitPaths, request.RemovalSelection);
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
                return CreateStage(observation, approval, explicitCombined, explicitChange, LibraryPermissionFailure.Interrupted);
            }
        }

        if (promptUnavailable)
        {
            return CreateStage(observation, approval, explicitCombined, explicitChange, LibraryPermissionFailure.Required);
        }

        var grantPaths = explicitPaths
            .Concat(choice == CliPermissionChoice.Always
                ? approval.ApprovedScopes.Select(scope => scope.Path)
                : [])
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        var grantChange = grantPaths.IsEmpty ? null : WorkspaceSettingsChangePlanner.PlanGrant(observation, grantPaths);
        var removalChange = request.RemovalSelection is null
            ? null
            : WorkspaceSettingsChangePlanner.PlanRemovals(observation, request.RemovalSelection);
        var change = ComposeChange(observation, grantChange, removalChange, grantPaths, request.RemovalSelection);
        LibraryPermissionFailure? permissionFailure;
        permissionFailure = approval.Leaves.Decision switch
        {
            WorkspacePermissionDecision.Required => LibraryPermissionFailure.Required,
            WorkspacePermissionDecision.Declined => LibraryPermissionFailure.Declined,
            WorkspacePermissionDecision.Granted or WorkspacePermissionDecision.Approved or WorkspacePermissionDecision.NotRequired => (LibraryPermissionFailure?)null,
            WorkspacePermissionDecision.NotEvaluated => throw new InvalidOperationException("A complete permission evaluation requires a decision."),
            _ => throw new ArgumentOutOfRangeException(nameof(request), approval.Leaves.Decision, "The permission decision is not defined."),
        };
        return CreateStage(observation, approval, change, grantChange, permissionFailure);
    }

    private static CliPermissionQuestion CreateQuestion(
        LibraryPermissionRequest request,
        LibraryPermissionApproval approval)
        => new(
            request.LibraryId.Value,
            [.. approval.ProposedScopes.Select(scope => new CliPermissionPath(
                scope.Path,
                scope.Kind == LibraryPermissionScopeKind.Directory))]);

    internal static async ValueTask<bool> RevalidateAsync(WorkspaceLockLease lease, LibraryPermissionStage stage, CancellationToken cancellationToken)
    {
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            throw new ArgumentException("Library permission revalidation requires a live same-workspace lease.", nameof(lease));
        }
        var current = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), lease.Request.Workspace, cancellationToken).ConfigureAwait(false);
        return stage.Observation.MatchesObservation(current);
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
        WorkspaceSettingsRead observation,
        LibraryPermissionApproval approval,
        PlannedFileChange? change,
        PlannedFileChange? grantChange,
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
            GrantChange = grantChange,
            RecoveryTarget = recovery,
            Failure = failure,
        };
    }

    private static PlannedFileChange? ComposeChange(
        WorkspaceSettingsRead observation,
        PlannedFileChange? grantChange,
        PlannedFileChange? removalChange,
        ImmutableArray<string> grantPaths,
        WorkspaceRemovalSelection? removalSelection)
    {
        if (grantChange is null && removalChange is null)
        {
            return null;
        }
        if (grantChange is null)
        {
            return removalChange;
        }
        if (removalChange is null)
        {
            return grantChange;
        }

        var snapshot = observation.Snapshot
            ?? throw new InvalidOperationException("A combined Library settings change requires its exact original snapshot.");
        ReadOnlyMemory<byte> intended = removalChange.IntendedBytes.ToArray();
        foreach (var path in grantPaths.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (WorkspaceSettingsCodec.AddAllowInstallPath(intended, path) is { } updated)
            {
                intended = updated;
            }
        }
        _ = removalSelection ?? throw new InvalidOperationException("A combined settings change requires its removal selection.");
        return snapshot.Kind switch
        {
            FileExpectationKind.Missing => PlannedFileChange.Create(snapshot.Expectation, intended.ToArray()),
            FileExpectationKind.File when !snapshot.Bytes.AsSpan().SequenceEqual(intended.Span)
                => PlannedFileChange.Replace(snapshot.Expectation, intended.ToArray()),
            FileExpectationKind.File => null,
            _ => throw new InvalidOperationException("A combined Library settings change requires a missing or ordinary settings file."),
        };
    }
}
