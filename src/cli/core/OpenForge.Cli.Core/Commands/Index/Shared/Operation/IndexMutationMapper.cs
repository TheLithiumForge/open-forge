using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Operation;

internal static class IndexMutationMapper
{
    internal static IndexFinding CreateFinding(
        IndexFindingCode code,
        string? cause = null,
        IndexLogicalSource? source = null)
        => new(
            code: code,
            sourceOccurrence: null,
            source: source,
            cause: cause ?? ReadOperationCause(code),
            candidates: []);

    internal static IndexFindingCode? ReadLockFindingCode(WorkspaceLockResult result)
    {
        return result.State switch
        {
            WorkspaceLockState.Acquired => null,
            WorkspaceLockState.Failed => ReadLockFailureCode(result.Failure
                ?? throw new InvalidOperationException("A failed workspace lock requires its typed failure.")),
            WorkspaceLockState.Cancelled => IndexFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The workspace lock state is not defined."),
        };
    }

    internal static IndexFindingCode ReadLockFailureCode(FilesystemFailure failure)
    {
        return failure.Kind switch
        {
            FilesystemFailureKind.InvalidPath => IndexFindingCode.WorkspaceUnsafe,
            FilesystemFailureKind.AccessDenied
                or FilesystemFailureKind.InputOutput
                or FilesystemFailureKind.Unsupported => IndexFindingCode.WorkspaceLockUnavailable,
            FilesystemFailureKind.InvalidEncoding
                or FilesystemFailureKind.InvalidSyntax => throw new InvalidOperationException(
                    "A workspace lock cannot classify document content failures."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(failure),
                failure.Kind,
                "The filesystem failure kind is not defined."),
        };
    }

    internal static IndexMutationFindingMapping ReadValidation(IndexValidationMappingInput input)
    {
        var findingCode = input.Validation.State switch
        {
            MutationValidationState.Valid => HasExactValidChecks(input)
                ? (IndexFindingCode?)null
                : IndexFindingCode.OperationFailed,
            MutationValidationState.Mismatched => IndexFindingCode.TargetChanged,
            MutationValidationState.Blocked => IndexFindingCode.TargetUnsafe,
            MutationValidationState.Failed => IndexFindingCode.ProjectionIncomplete,
            MutationValidationState.Cancelled => IndexFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                input.Validation.State,
                "The mutation validation state is not defined."),
        };
        return new IndexMutationFindingMapping(
            findingCode: findingCode,
            source: findingCode is null ? null : ReadDecisiveValidationSource(input),
            cause: findingCode is null
                ? null
                : ReadValidationCause(findingCode.Value));
    }

    internal static IndexPreparationMapping ReadPreparation(RecoveryBundlePreparationResult result)
    {
        return result.State switch
        {
            RecoveryBundlePreparationState.Prepared => IndexPreparationMapping.Prepared(
                result.Preparation
                    ?? throw new InvalidOperationException("A prepared recovery result requires its preparation.")),
            RecoveryBundlePreparationState.NotNeeded => IndexPreparationMapping.Rejected(
                ReadNonpreparedRecovery(result.ResidualPath),
                IndexFindingCode.OperationFailed),
            RecoveryBundlePreparationState.Incomplete => IndexPreparationMapping.Rejected(
                ReadNonpreparedRecovery(result.ResidualPath),
                IndexFindingCode.RecoveryUnavailable),
            RecoveryBundlePreparationState.Blocked => IndexPreparationMapping.Rejected(
                ReadNonpreparedRecovery(result.ResidualPath),
                IndexFindingCode.RecoveryConflict),
            RecoveryBundlePreparationState.Cancelled => IndexPreparationMapping.Rejected(
                ReadNonpreparedRecovery(result.ResidualPath),
                IndexFindingCode.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The recovery bundle preparation state is not defined."),
        };
    }

    internal static IndexReceiptMapping ReadReceipt(FileChangeReceipt receipt)
    {
        return (receipt.EffectState, receipt.VerificationState, receipt.NotStartedReason) switch
        {
            (FilesystemEffectState.Applied, FilesystemVerificationState.Verified, null) => new IndexReceiptMapping(
                outcome: IndexRegionOutcome.Verified,
                findingCode: null,
                shouldContinue: true),
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.TargetChanged) => RejectedReceipt(
                    IndexRegionOutcome.NotStarted,
                    IndexFindingCode.TargetChangedDuringApply),
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.Cancelled) => RejectedReceipt(
                    IndexRegionOutcome.NotStarted,
                    IndexFindingCode.Interrupted),
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ApplicationFailed) => RejectedReceipt(
                    IndexRegionOutcome.NotStarted,
                    IndexFindingCode.WriteFailed),
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ContractRejected) => RejectedReceipt(
                    IndexRegionOutcome.NotStarted,
                    IndexFindingCode.OperationFailed),
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed, null) => RejectedReceipt(
                IndexRegionOutcome.Applied,
                IndexFindingCode.VerificationFailed),
            (FilesystemEffectState.Unknown, FilesystemVerificationState.NotStarted, null) => RejectedReceipt(
                IndexRegionOutcome.Unknown,
                IndexFindingCode.WriteFailed),
            _ => throw new InvalidOperationException(
                "The file-change receipt does not describe a supported coherent state."),
        };
    }

    internal static IndexDeletionMapping ReadDeletion(RecoveryBundleDeletionResult result)
    {
        return (result.State, result.Disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Removed, residualPath: null),
                findingCode: null),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Retained, result.ResidualPath),
                findingCode: IndexFindingCode.RecoveryArtifactRetained),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Unknown) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Unknown, result.ResidualPath),
                findingCode: IndexFindingCode.RecoveryFailed),
            (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Retained) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Retained, result.ResidualPath),
                findingCode: IndexFindingCode.RecoveryFailed),
            (RecoveryBundleDeletionState.Blocked, RecoveryBundleDisposition.Unknown) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Unknown, result.ResidualPath),
                findingCode: IndexFindingCode.RecoveryFailed),
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Retained, result.ResidualPath),
                findingCode: IndexFindingCode.Interrupted),
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Unknown) => new IndexDeletionMapping(
                recovery: new IndexRecovery(IndexRecoveryState.Unknown, result.ResidualPath),
                findingCode: IndexFindingCode.Interrupted),
            _ => throw new InvalidOperationException(
                "The recovery deletion result does not describe a supported coherent state."),
        };
    }

    internal static IndexMutationFindingMapping ReadFreshVerification(IndexFreshVerificationInput input)
    {
        var stateFinding = ReadProjectionReadState(input.Fresh.State);
        if (stateFinding is not null)
        {
            var finding = input.Fresh.Findings.FirstOrDefault();
            return new IndexMutationFindingMapping(
                findingCode: stateFinding,
                source: finding?.Source,
                cause: finding?.Cause);
        }

        if (input.Fresh.Projection is not { } freshProjection
            || input.Fresh.Findings.Count != 0)
        {
            var finding = input.Fresh.Findings.FirstOrDefault(candidate =>
                    candidate.Code == IndexFindingCode.Interrupted)
                ?? input.Fresh.Findings.FirstOrDefault();
            var findingCode = finding?.Code == IndexFindingCode.Interrupted
                ? IndexFindingCode.Interrupted
                : IndexFindingCode.VerificationFailed;
            return new IndexMutationFindingMapping(
                findingCode: findingCode,
                source: finding?.Source,
                cause: finding?.Cause);
        }

        return ReadFreshProjection(input.Original, freshProjection);
    }

    internal static IndexFindingCode? ReadProjectionReadState(IndexProjectionReadState state)
        => state switch
        {
            IndexProjectionReadState.Cancelled => IndexFindingCode.Interrupted,
            IndexProjectionReadState.SelectionIncomplete => IndexFindingCode.VerificationFailed,
            IndexProjectionReadState.Projected => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Index projection-read state is not defined."),
        };

    private static IndexReceiptMapping RejectedReceipt(
        IndexRegionOutcome outcome,
        IndexFindingCode findingCode)
        => new(
            outcome: outcome,
            findingCode: findingCode,
            shouldContinue: false);

    private static IndexRecovery ReadNonpreparedRecovery(string? residualPath)
        => residualPath is null
            ? new IndexRecovery(IndexRecoveryState.NotCreated, residualPath: null)
            : new IndexRecovery(IndexRecoveryState.Unknown, residualPath);

    private static bool HasExactValidChecks(IndexValidationMappingInput input)
    {
        if (input.Validation.Checks.Count != input.Plan.Updates.Count)
        {
            return false;
        }

        for (var index = 0; index < input.Plan.Updates.Count; index++)
        {
            var change = input.Plan.Updates[index];
            var check = input.Validation.Checks[index];
            if (check.State != FileExpectationValidationState.Matched
                || check.Expectation != change.Expectation
                || check.Actual is not { } actual
                || actual.Expectation != change.Expectation
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    check.PhysicalPath,
                    change.Expectation.PhysicalPath))
            {
                return false;
            }
        }

        return true;
    }

    private static IndexLogicalSource? ReadDecisiveValidationSource(IndexValidationMappingInput input)
    {
        var decisive = input.Validation.Checks.FirstOrDefault(check =>
            check.State != FileExpectationValidationState.Matched);
        if (decisive is null)
        {
            return null;
        }

        var updateIndex = -1;
        for (var index = 0; index < input.Plan.Updates.Count; index++)
        {
            if (input.Plan.Updates[index].Expectation == decisive.Expectation)
            {
                updateIndex = index;
                break;
            }
        }

        if (updateIndex < 0)
        {
            return null;
        }

        return input.Plan.Regions
            .Where(region => region.Action == IndexRegionAction.Update)
            .ElementAt(updateIndex)
            .Source;
    }

    private static string ReadValidationCause(IndexFindingCode code)
        => code switch
        {
            IndexFindingCode.OperationFailed =>
                "Whole-plan revalidation returned incoherent target checks.",
            IndexFindingCode.TargetChanged =>
                "A planned target changed before recovery preparation.",
            IndexFindingCode.TargetUnsafe =>
                "A planned target could not be revalidated safely.",
            IndexFindingCode.ProjectionIncomplete =>
                "A planned target could not be revalidated completely.",
            IndexFindingCode.Interrupted =>
                "Whole-plan revalidation was interrupted.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The finding code is not a whole-plan validation outcome."),
        };

    private static IndexMutationFindingMapping ReadFreshProjection(
        IndexPlan original,
        IndexProjectionFormation fresh)
    {
        var originalSelection = original.Input.Projection.Selection;
        var freshSelection = fresh.Selection;
        if (!SelectionsEqual(originalSelection.Selection, freshSelection.Selection))
        {
            return FreshFailure(
                source: null,
                cause: "The normalized Index selection changed during application verification.");
        }

        if (originalSelection.Targets.Count != freshSelection.Targets.Count)
        {
            return FreshFailure(
                source: null,
                cause: "The selected Index target closure changed during application verification.");
        }

        for (var index = 0; index < originalSelection.Targets.Count; index++)
        {
            var expected = originalSelection.Targets[index];
            var actual = freshSelection.Targets[index];
            if (!string.Equals(
                    expected.Identity.CanonicalBasePath,
                    actual.Identity.CanonicalBasePath,
                    StringComparison.Ordinal)
                || !string.Equals(
                    expected.Identity.AutomaticId,
                    actual.Identity.AutomaticId,
                    StringComparison.Ordinal)
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    expected.Base.PhysicalPath,
                    actual.Base.PhysicalPath))
            {
                return FreshFailure(
                    source: original.Input.Projection.Regions[index].Source,
                    cause: "A selected Index target changed its exact identity during application verification.");
            }
        }

        return ReadFreshExpectedBodies(original, fresh);
    }

    private static bool SelectionsEqual(
        IndexSelection expected,
        IndexSelection actual)
    {
        if (expected.Origin != actual.Origin
            || expected.Scope != actual.Scope
            || expected.Sources.Count != actual.Sources.Count)
        {
            return false;
        }

        return expected.Sources.Zip(actual.Sources).All(pair =>
            string.Equals(pair.First.Id, pair.Second.Id, StringComparison.Ordinal)
            && string.Equals(pair.First.Path, pair.Second.Path, StringComparison.Ordinal)
            && pair.First.Scope == pair.Second.Scope);
    }

    private static IndexMutationFindingMapping ReadFreshExpectedBodies(
        IndexPlan original,
        IndexProjectionFormation fresh)
    {
        foreach (var projected in original.Input.Projection.Regions)
        {
            var freshRegion = fresh.Regions.SingleOrDefault(candidate => string.Equals(
                candidate.Source.Path,
                projected.Source.Path,
                StringComparison.Ordinal));
            var freshChange = freshRegion?.Region.Change;
            var originalChange = projected.Region.Change;
            if (freshRegion is null
                || originalChange is null
                || freshRegion.Region.State != GeneratedNavigationRegionState.Available
                || freshChange is null
                || freshChange.Kind != GeneratedNavigationChangeKind.Unchanged
                || !string.Equals(
                    freshChange.ExpectedBody,
                    originalChange.ExpectedBody,
                    StringComparison.Ordinal))
            {
                return FreshFailure(
                    source: projected.Source,
                    cause: "An Index target did not retain its exact expected generated body during application verification.");
            }
        }

        return new IndexMutationFindingMapping(
            findingCode: null,
            source: null,
            cause: null);
    }

    private static IndexMutationFindingMapping FreshFailure(
        IndexLogicalSource? source,
        string cause)
        => new(
            findingCode: IndexFindingCode.VerificationFailed,
            source: source,
            cause: cause);

    private static string ReadOperationCause(IndexFindingCode code)
        => code switch
        {
            IndexFindingCode.WorkspaceUnsafe =>
                "The selected workspace lock path is unsafe.",
            IndexFindingCode.WorkspaceLockUnavailable =>
                "The selected workspace lock could not be acquired.",
            IndexFindingCode.TargetChanged =>
                "A planned target changed before recovery preparation.",
            IndexFindingCode.TargetUnsafe =>
                "A planned target could not be revalidated safely.",
            IndexFindingCode.ProjectionIncomplete =>
                "A planned target could not be revalidated completely.",
            IndexFindingCode.RecoveryUnavailable =>
                "The recovery bundle could not be prepared completely.",
            IndexFindingCode.RecoveryConflict =>
                "The deterministic recovery bundle path conflicts with an existing artifact.",
            IndexFindingCode.TargetChangedDuringApply =>
                "A target changed after recovery preparation and before its effect.",
            IndexFindingCode.WriteFailed =>
                "A target effect failed or its completion could not be proved.",
            IndexFindingCode.VerificationFailed =>
                "The completed Index operation could not be verified exactly.",
            IndexFindingCode.RecoveryFailed =>
                "The recovery bundle could not be removed with a proved disposition.",
            IndexFindingCode.OperationFailed =>
                "The Index operation failed at an internal contract boundary.",
            IndexFindingCode.Interrupted =>
                "The Index operation was interrupted before completion.",
            IndexFindingCode.RecoveryArtifactRetained =>
                "The verified Index changes completed, but the recovery artifact remains.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The finding code is not an Index operation event."),
        };
}
