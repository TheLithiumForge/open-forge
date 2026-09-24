using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed class RouteRemoveOwnershipPublisher(
    FileChangeApplier applier,
    FileExpectationValidator expectationValidator)
{
    private readonly FileChangeApplier _applier = applier;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteRemoveApplicationProgress> PublishAsync(
        RouteRemovePlan plan,
        WorkspaceLockLease lease,
        RecoveryBundlePreparation preparation,
        RouteRemoveApplicationProgress progress,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(progress);

        var change = plan.Projection.OwnershipChange;
        if (change is null)
        {
            return progress with { OwnershipOutcome = RouteRemovePersistenceOutcome.Unchanged };
        }

        if (!lease.IsHeldFor(plan.Request.Workspace))
        {
            return Boundary(
                progress,
                receipt: null,
                RouteRemovePersistenceOutcome.NotStarted,
                new RouteRemoveFinding(
                    RouteRemoveFindingCode.OperationFailed,
                    CliSemanticStatus.Failed,
                    plan.Projection.Ownership.LogicalPath,
                    "Route Remove ownership publication requires the selected workspace lease."));
        }

        try
        {
            var check = await _expectationValidator.ValidateAsync(
                plan.Request.Workspace,
                change.Expectation,
                cancellationToken).ConfigureAwait(false);
            if (check.State != FileExpectationValidationState.Matched)
            {
                var reason = check.State switch
                {
                    FileExpectationValidationState.Mismatched => FilesystemNotStartedReason.TargetChanged,
                    FileExpectationValidationState.Cancelled => FilesystemNotStartedReason.Cancelled,
                    FileExpectationValidationState.Failed => FilesystemNotStartedReason.ApplicationFailed,
                    FileExpectationValidationState.Blocked => FilesystemNotStartedReason.ContractRejected,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(check), check.State, "The ownership validation state is not defined."),
                };
                var receipt = FileChangeReceipt.NotStarted(
                    change,
                    plan.Projection.Ownership.Snapshot
                        ?? throw new InvalidOperationException("A planned ownership publication requires its prior snapshot."),
                    reason,
                    check.Cause ?? "The ownership lock changed before the planned release could be published.");
                var validationFailure = reason switch
                {
                    FilesystemNotStartedReason.TargetChanged => (
                        Outcome: RouteRemovePersistenceOutcome.NotStarted,
                        Code: RouteRemoveFindingCode.TargetChangedDuringApply,
                        Status: CliSemanticStatus.Blocked),
                    FilesystemNotStartedReason.Cancelled => (
                        Outcome: RouteRemovePersistenceOutcome.NotStarted,
                        Code: RouteRemoveFindingCode.Interrupted,
                        Status: CliSemanticStatus.Interrupted),
                    FilesystemNotStartedReason.ApplicationFailed => (
                        Outcome: RouteRemovePersistenceOutcome.Failed,
                        Code: RouteRemoveFindingCode.WriteFailed,
                        Status: CliSemanticStatus.Failed),
                    FilesystemNotStartedReason.ContractRejected => (
                        Outcome: RouteRemovePersistenceOutcome.Failed,
                        Code: RouteRemoveFindingCode.WriteFailed,
                        Status: CliSemanticStatus.Failed),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(reason), reason, "The ownership publication failure reason is not defined."),
                };
                return Boundary(
                    progress,
                    new RouteRemoveFileChangeReceipt(receipt),
                    validationFailure.Outcome,
                    new RouteRemoveFinding(
                        validationFailure.Code,
                        validationFailure.Status,
                        plan.Projection.Ownership.LogicalPath,
                        receipt.Cause ?? "The ownership lock changed before publication."));
            }

            var applied = await _applier.ApplyAsync(
                lease,
                change,
                check,
                preparation,
                cancellationToken).ConfigureAwait(false);
            var outcome = ReadOutcome(applied);
            if (outcome == RouteRemovePersistenceOutcome.Applied)
            {
                return progress with
                {
                    OwnershipReceipt = new RouteRemoveFileChangeReceipt(applied),
                    OwnershipOutcome = outcome,
                };
            }

            return Boundary(
                progress,
                new RouteRemoveFileChangeReceipt(applied),
                outcome,
                new RouteRemoveFinding(
                    ReadCode(applied),
                    ReadStatus(applied),
                    plan.Projection.Ownership.LogicalPath,
                    applied.Cause ?? "The ownership release did not complete and verify."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Boundary(
                progress,
                receipt: null,
                RouteRemovePersistenceOutcome.NotStarted,
                new RouteRemoveFinding(
                    RouteRemoveFindingCode.Interrupted,
                    CliSemanticStatus.Interrupted,
                    plan.Projection.Ownership.LogicalPath,
                    "Route Remove ownership publication was interrupted before completion."));
        }
        catch (Exception)
        {
            var before = plan.Projection.Ownership.Snapshot
                ?? throw new InvalidOperationException("A planned ownership publication requires its prior snapshot.");
            const string cause = "Route Remove ownership publication failed unexpectedly after it was attempted.";
            var unknown = FileChangeReceipt.CompletionUnknown(
                change,
                before,
                after: null,
                cause);
            return Boundary(
                progress,
                new RouteRemoveFileChangeReceipt(unknown),
                RouteRemovePersistenceOutcome.Unknown,
                new RouteRemoveFinding(
                    RouteRemoveFindingCode.WriteFailed,
                    CliSemanticStatus.Failed,
                    plan.Projection.Ownership.LogicalPath,
                    unknown.Cause ?? cause));
        }
    }

    private static RouteRemoveApplicationProgress Boundary(
        RouteRemoveApplicationProgress progress,
        RouteRemoveFileChangeReceipt? receipt,
        RouteRemovePersistenceOutcome outcome,
        RouteRemoveFinding finding)
        => progress with
        {
            OwnershipReceipt = receipt,
            OwnershipOutcome = outcome,
            Verification = finding.Status == CliSemanticStatus.Interrupted
                ? RouteRemoveVerificationState.Unknown
                : RouteRemoveVerificationState.Failed,
            Findings =
            [
                .. progress.Findings,
                finding,
            ],
        };

    private static RouteRemovePersistenceOutcome ReadOutcome(FileChangeReceipt receipt)
        => (receipt.EffectState, receipt.VerificationState) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified) => RouteRemovePersistenceOutcome.Applied,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed) => RouteRemovePersistenceOutcome.Failed,
            (FilesystemEffectState.Unknown, _) => RouteRemovePersistenceOutcome.Unknown,
            (FilesystemEffectState.NotStarted, _) => RouteRemovePersistenceOutcome.NotStarted,
            _ => RouteRemovePersistenceOutcome.Unknown,
        };

    private static RouteRemoveFindingCode ReadCode(FileChangeReceipt receipt)
        => receipt.NotStartedReason switch
        {
            FilesystemNotStartedReason.TargetChanged => RouteRemoveFindingCode.TargetChangedDuringApply,
            FilesystemNotStartedReason.Cancelled => RouteRemoveFindingCode.Interrupted,
            FilesystemNotStartedReason.ApplicationFailed => RouteRemoveFindingCode.WriteFailed,
            _ when receipt.EffectState == FilesystemEffectState.Unknown => RouteRemoveFindingCode.WriteFailed,
            _ => RouteRemoveFindingCode.VerificationFailed,
        };

    private static CliSemanticStatus ReadStatus(FileChangeReceipt receipt)
        => ReadCode(receipt) switch
        {
            RouteRemoveFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            RouteRemoveFindingCode.TargetChangedDuringApply => CliSemanticStatus.Blocked,
            _ => CliSemanticStatus.Failed,
        };
}
