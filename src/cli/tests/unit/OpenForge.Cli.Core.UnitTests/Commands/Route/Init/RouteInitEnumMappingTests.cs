using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitEnumMappingTests
{
    [Fact(DisplayName = "Route Init catalogue issue mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void CatalogueIssueMappingNamesEveryValueAndRejectsUndefined()
    {
        var cases = new[]
        {
            (SourceCatalogueIssueCode.RootMissing, false, RouteInitFindingCode.InspectionIncomplete, true),
            (SourceCatalogueIssueCode.RootMissing, true, RouteInitFindingCode.FrameworkInstallRequired, false),
            (SourceCatalogueIssueCode.RootUnsafe, false, RouteInitFindingCode.TargetUnsafe, false),
            (SourceCatalogueIssueCode.RootUnavailable, false, RouteInitFindingCode.InspectionIncomplete, true),
            (SourceCatalogueIssueCode.DirectoryUnavailable, false, RouteInitFindingCode.InspectionIncomplete, true),
            (SourceCatalogueIssueCode.CandidateUnsafe, false, RouteInitFindingCode.TargetUnsafe, false),
            (SourceCatalogueIssueCode.CandidateUnavailable, false, RouteInitFindingCode.InspectionIncomplete, true),
            (SourceCatalogueIssueCode.IdentityUnavailable, false, RouteInitFindingCode.InspectionIncomplete, true),
            (SourceCatalogueIssueCode.IdentityCollision, false, RouteInitFindingCode.IdentityCollision, false),
            (SourceCatalogueIssueCode.PhysicalAlias, false, RouteInitFindingCode.IdentityCollision, false),
            (SourceCatalogueIssueCode.OrphanOverwrite, false, RouteInitFindingCode.IdentityCollision, false),
        };

        foreach (var (code, isFramework, expectedCode, expectedIncomplete) in cases)
        {
            Assert.Equal(
                (expectedCode, expectedIncomplete),
                RouteInitPlanningInspector.ReadCatalogueFinding(code, isFramework));
        }

        var undefined = (SourceCatalogueIssueCode)int.MaxValue;
        AssertUndefined(
            () => RouteInitPlanningInspector.ReadCatalogueFinding(undefined, isFramework: false),
            "code",
            undefined,
            "The source catalogue issue code is not defined.");
    }

    [Fact(DisplayName = "Route Init Framework trust mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FrameworkTrustMappingNamesEveryValueAndRejectsUndefined()
    {
        var cases = new[]
        {
            (RouteInitFrameworkTrustState.Current, RouteInitFindingCode.LifecycleBlocked, false),
            (RouteInitFrameworkTrustState.InstallRequired, RouteInitFindingCode.FrameworkInstallRequired, false),
            (RouteInitFrameworkTrustState.UpdateRequired, RouteInitFindingCode.FrameworkUpdateRequired, false),
            (RouteInitFrameworkTrustState.Incomplete, RouteInitFindingCode.LifecycleUnavailable, true),
            (RouteInitFrameworkTrustState.Blocked, RouteInitFindingCode.LifecycleBlocked, false),
            (RouteInitFrameworkTrustState.Cancelled, RouteInitFindingCode.Interrupted, true),
        };

        foreach (var (state, expectedCode, expectedIncomplete) in cases)
        {
            Assert.Equal(
                (expectedCode, expectedIncomplete),
                RouteInitPlanningInspector.ReadTrustFinding(state));
        }

        var undefined = (RouteInitFrameworkTrustState)int.MaxValue;
        AssertUndefined(
            () => RouteInitPlanningInspector.ReadTrustFinding(undefined),
            "state",
            undefined,
            "The Route Init Framework trust state is not defined.");
    }

    [Fact(DisplayName = "Route Init application validation mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void ApplicationValidationMappingNamesEveryValueAndRejectsUndefined()
    {
        const string cause = "The exact validation cause.";
        Assert.Equal(
            (RouteInitFindingCode.OperationFailed, "Route Init whole-plan revalidation returned incoherent checks."),
            RouteInitApplicationOperation.ReadValidationBoundary(MutationValidationState.Valid, cause));
        Assert.Equal(
            (RouteInitFindingCode.TargetChanged, cause),
            RouteInitApplicationOperation.ReadValidationBoundary(MutationValidationState.Mismatched, cause));
        Assert.Equal(
            (RouteInitFindingCode.TargetUnsafe, cause),
            RouteInitApplicationOperation.ReadValidationBoundary(MutationValidationState.Blocked, cause));
        Assert.Equal(
            (RouteInitFindingCode.InspectionIncomplete, cause),
            RouteInitApplicationOperation.ReadValidationBoundary(MutationValidationState.Failed, cause));
        Assert.Equal(
            (RouteInitFindingCode.Interrupted, "Route Init whole-plan revalidation was interrupted."),
            RouteInitApplicationOperation.ReadValidationBoundary(MutationValidationState.Cancelled, cause));
        Assert.Equal(
            "A planned Route Init target changed before application.",
            RouteInitApplicationOperation.ReadValidationBoundary(
                MutationValidationState.Mismatched,
                cause: null).Cause);

        var undefined = (MutationValidationState)int.MaxValue;
        AssertUndefined(
            () => RouteInitApplicationOperation.ReadValidationBoundary(undefined, cause),
            "state",
            undefined,
            "The mutation validation state is not defined.");
    }

    [Fact(DisplayName = "Route Init recovery preparation finding mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void RecoveryPreparationFindingMappingNamesEveryValueAndRejectsUndefined()
    {
        var cases = new[]
        {
            (RouteInitRecoveryPreparationState.NotRequired, RouteInitFindingCode.OperationFailed),
            (RouteInitRecoveryPreparationState.Prepared, RouteInitFindingCode.OperationFailed),
            (RouteInitRecoveryPreparationState.Incomplete, RouteInitFindingCode.RecoveryUnavailable),
            (RouteInitRecoveryPreparationState.Blocked, RouteInitFindingCode.RecoveryConflict),
            (RouteInitRecoveryPreparationState.Cancelled, RouteInitFindingCode.Interrupted),
            (RouteInitRecoveryPreparationState.Failed, RouteInitFindingCode.OperationFailed),
        };

        foreach (var (state, expected) in cases)
        {
            Assert.Equal(expected, RouteInitApplicationOperation.ReadPreparationFinding(state));
        }

        var undefined = (RouteInitRecoveryPreparationState)int.MaxValue;
        AssertUndefined(
            () => RouteInitApplicationOperation.ReadPreparationFinding(undefined),
            "state",
            undefined,
            "The Route Init recovery preparation state is not defined.");
    }

    [Fact(DisplayName = "Route Init receipt finding mapping names every tuple component and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void ReceiptFindingMappingNamesEveryTupleComponentAndRejectsUndefined()
    {
        foreach (var effect in Enum.GetValues<FilesystemEffectState>())
        {
            foreach (var verification in Enum.GetValues<FilesystemVerificationState>())
            {
                foreach (var reason in ReceiptReasons())
                {
                    Assert.Equal(
                        ExpectedReceiptFinding(effect, verification, reason),
                        RouteInitApplicationOperation.ReadReceiptFinding(effect, verification, reason));
                }
            }
        }

        var undefinedEffect = (FilesystemEffectState)int.MaxValue;
        AssertUndefined(
            () => RouteInitApplicationOperation.ReadReceiptFinding(
                undefinedEffect,
                FilesystemVerificationState.NotStarted,
                reason: null),
            "effect",
            undefinedEffect,
            "The filesystem effect state is not defined.");
        var undefinedVerification = (FilesystemVerificationState)int.MaxValue;
        AssertUndefined(
            () => RouteInitApplicationOperation.ReadReceiptFinding(
                FilesystemEffectState.NotStarted,
                undefinedVerification,
                reason: null),
            "verification",
            undefinedVerification,
            "The filesystem verification state is not defined.");
        var undefinedReason = (FilesystemNotStartedReason)int.MaxValue;
        AssertUndefined(
            () => RouteInitApplicationOperation.ReadReceiptFinding(
                FilesystemEffectState.NotStarted,
                FilesystemVerificationState.NotStarted,
                undefinedReason),
            "reason",
            undefinedReason,
            "The filesystem not-started reason is not defined.");
    }

    [Fact(DisplayName = "Route Init preflight mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void PreflightMappingNamesEveryValueAndRejectsUndefined()
    {
        var cases = new[]
        {
            (MutationValidationState.Valid, RouteInitFindingCode.OperationFailed, false),
            (MutationValidationState.Mismatched, RouteInitFindingCode.TargetChanged, false),
            (MutationValidationState.Blocked, RouteInitFindingCode.TargetUnsafe, false),
            (MutationValidationState.Failed, RouteInitFindingCode.InspectionIncomplete, true),
            (MutationValidationState.Cancelled, RouteInitFindingCode.Interrupted, true),
        };

        foreach (var (state, expectedCode, expectedIncomplete) in cases)
        {
            Assert.Equal(
                (expectedCode, expectedIncomplete),
                RouteInitPlanFinalizer.ReadPreflightFinding(state));
        }

        var undefined = (MutationValidationState)int.MaxValue;
        AssertUndefined(
            () => RouteInitPlanFinalizer.ReadPreflightFinding(undefined),
            "state",
            undefined,
            "The mutation validation state is not defined.");
    }

    [Fact(DisplayName = "Route Init directory residual mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void DirectoryResidualMappingNamesEveryValueAndRejectsUndefined()
    {
        var withoutAfter = DirectoryReceipt(hasAfter: false);
        var withAfter = DirectoryReceipt(hasAfter: true);
        foreach (var outcome in Enum.GetValues<RouteInitEffectOutcome>())
        {
            foreach (var verification in Enum.GetValues<RouteInitVerificationState>())
            {
                Assert.Equal(
                    ExpectedResidual(outcome, verification, hasAfter: false),
                    RouteInitApplicationResultFactory.ReadResidual(withoutAfter, outcome, verification));
                Assert.Equal(
                    ExpectedResidual(outcome, verification, hasAfter: true),
                    RouteInitApplicationResultFactory.ReadResidual(withAfter, outcome, verification));
            }
        }

        AssertResidualUndefined(
            (outcome, verification) => RouteInitApplicationResultFactory.ReadResidual(
                withoutAfter,
                outcome,
                verification));
    }

    [Fact(DisplayName = "Route Init file residual mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FileResidualMappingNamesEveryValueAndRejectsUndefined()
    {
        var withoutAfter = FileReceipt(hasAfter: false);
        var withAfter = FileReceipt(hasAfter: true);
        foreach (var outcome in Enum.GetValues<RouteInitEffectOutcome>())
        {
            foreach (var verification in Enum.GetValues<RouteInitVerificationState>())
            {
                Assert.Equal(
                    ExpectedResidual(outcome, verification, hasAfter: false),
                    RouteInitApplicationResultFactory.ReadResidual(withoutAfter, outcome, verification));
                Assert.Equal(
                    ExpectedResidual(outcome, verification, hasAfter: true),
                    RouteInitApplicationResultFactory.ReadResidual(withAfter, outcome, verification));
            }
        }

        AssertResidualUndefined(
            (outcome, verification) => RouteInitApplicationResultFactory.ReadResidual(
                withoutAfter,
                outcome,
                verification));
    }

    [Fact(DisplayName = "Route Init applied expectation mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void AppliedExpectationMappingNamesEveryValueAndRejectsUndefined()
    {
        const string cause = "The exact expectation cause.";
        Assert.Null(RouteInitAppliedVerifier.ReadExpectationBoundary(
            FileExpectationValidationState.Matched,
            cause));
        foreach (var state in new[]
        {
            FileExpectationValidationState.Mismatched,
            FileExpectationValidationState.Blocked,
            FileExpectationValidationState.Failed,
        })
        {
            var failed = Assert.IsType<RouteInitAppliedVerification>(
                RouteInitAppliedVerifier.ReadExpectationBoundary(state, cause));
            Assert.Equal(RouteInitAppliedVerificationState.Failed, failed.State);
            Assert.Equal(cause, failed.Cause);
        }

        var fallback = Assert.IsType<RouteInitAppliedVerification>(
            RouteInitAppliedVerifier.ReadExpectationBoundary(
                FileExpectationValidationState.Mismatched,
                cause: null));
        Assert.Equal(
            "An applied Route Init postcondition changed before final verification.",
            fallback.Cause);
        var cancelled = Assert.IsType<RouteInitAppliedVerification>(
            RouteInitAppliedVerifier.ReadExpectationBoundary(
                FileExpectationValidationState.Cancelled,
                cause));
        Assert.Equal(RouteInitAppliedVerificationState.Cancelled, cancelled.State);
        Assert.Equal("Final Route Init verification was interrupted.", cancelled.Cause);

        var undefined = (FileExpectationValidationState)int.MaxValue;
        AssertUndefined(
            () => RouteInitAppliedVerifier.ReadExpectationBoundary(undefined, cause),
            "state",
            undefined,
            "The file expectation validation state is not defined.");
    }

    [Fact(DisplayName = "Route Init recovery bundle preparation mapping names every value and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void RecoveryBundlePreparationMappingNamesEveryValueAndRejectsUndefined()
    {
        Assert.Equal(
            RouteInitRecoveryPreparationState.Failed,
            RouteInitRecoveryLifecycle.ReadPreparationState(
                RecoveryBundlePreparationState.NotNeeded,
                hasPreparation: false));
        Assert.Equal(
            RouteInitRecoveryPreparationState.Prepared,
            RouteInitRecoveryLifecycle.ReadPreparationState(
                RecoveryBundlePreparationState.Prepared,
                hasPreparation: true));
        Assert.Equal(
            RouteInitRecoveryPreparationState.Failed,
            RouteInitRecoveryLifecycle.ReadPreparationState(
                RecoveryBundlePreparationState.Prepared,
                hasPreparation: false));
        Assert.Equal(
            RouteInitRecoveryPreparationState.Incomplete,
            RouteInitRecoveryLifecycle.ReadPreparationState(
                RecoveryBundlePreparationState.Incomplete,
                hasPreparation: false));
        Assert.Equal(
            RouteInitRecoveryPreparationState.Blocked,
            RouteInitRecoveryLifecycle.ReadPreparationState(
                RecoveryBundlePreparationState.Blocked,
                hasPreparation: false));
        Assert.Equal(
            RouteInitRecoveryPreparationState.Cancelled,
            RouteInitRecoveryLifecycle.ReadPreparationState(
                RecoveryBundlePreparationState.Cancelled,
                hasPreparation: false));

        var undefined = (RecoveryBundlePreparationState)int.MaxValue;
        AssertUndefined(
            () => RouteInitRecoveryLifecycle.ReadPreparationState(undefined, hasPreparation: false),
            "state",
            undefined,
            "The recovery bundle preparation state is not defined.");
    }

    [Fact(DisplayName = "Route Init recovery deletion mapping names every tuple component and rejects undefined values"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void RecoveryDeletionMappingNamesEveryTupleComponentAndRejectsUndefined()
    {
        foreach (var state in Enum.GetValues<RecoveryBundleDeletionState>())
        {
            foreach (var disposition in Enum.GetValues<RecoveryBundleDisposition>())
            {
                Assert.Equal(
                    ExpectedDeletionOutcome(state, disposition),
                    RouteInitRecoveryLifecycle.ReadDeletionOutcome(state, disposition));
            }
        }

        var undefinedState = (RecoveryBundleDeletionState)int.MaxValue;
        AssertUndefined(
            () => RouteInitRecoveryLifecycle.ReadDeletionOutcome(
                undefinedState,
                RecoveryBundleDisposition.Removed),
            "state",
            undefinedState,
            "The recovery deletion state is not defined.");
        var undefinedDisposition = (RecoveryBundleDisposition)int.MaxValue;
        AssertUndefined(
            () => RouteInitRecoveryLifecycle.ReadDeletionOutcome(
                RecoveryBundleDeletionState.Deleted,
                undefinedDisposition),
            "disposition",
            undefinedDisposition,
            "The recovery bundle disposition is not defined.");
    }

    private static IEnumerable<FilesystemNotStartedReason?> ReceiptReasons()
    {
        yield return null;
        foreach (var reason in Enum.GetValues<FilesystemNotStartedReason>())
        {
            yield return reason;
        }
    }

    private static RouteInitFindingCode ExpectedReceiptFinding(
        FilesystemEffectState effect,
        FilesystemVerificationState verification,
        FilesystemNotStartedReason? reason)
        => (effect, verification, reason) switch
        {
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.Cancelled) => RouteInitFindingCode.Interrupted,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.TargetChanged) => RouteInitFindingCode.TargetChangedDuringApply,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ApplicationFailed) => RouteInitFindingCode.WriteFailed,
            (FilesystemEffectState.NotStarted, FilesystemVerificationState.NotStarted,
                FilesystemNotStartedReason.ContractRejected) => RouteInitFindingCode.OperationFailed,
            (FilesystemEffectState.Applied, FilesystemVerificationState.Failed, null) =>
                RouteInitFindingCode.VerificationFailed,
            (FilesystemEffectState.Unknown, FilesystemVerificationState.NotStarted, null) =>
                RouteInitFindingCode.WriteFailed,
            _ => RouteInitFindingCode.OperationFailed,
        };

    private static RouteInitEffectResidual ExpectedResidual(
        RouteInitEffectOutcome outcome,
        RouteInitVerificationState verification,
        bool hasAfter)
        => outcome switch
        {
            RouteInitEffectOutcome.Planned
                or RouteInitEffectOutcome.NotStarted => RouteInitEffectResidual.None,
            RouteInitEffectOutcome.Verified => verification == RouteInitVerificationState.Verified
                ? RouteInitEffectResidual.None
                : RouteInitEffectResidual.Retained,
            RouteInitEffectOutcome.VerificationFailed => hasAfter
                ? RouteInitEffectResidual.Retained
                : RouteInitEffectResidual.Unknown,
            RouteInitEffectOutcome.CompletionUnknown => RouteInitEffectResidual.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };

    private static (RouteInitRecoveryState State, RouteInitFindingCode? FindingCode) ExpectedDeletionOutcome(
        RecoveryBundleDeletionState state,
        RecoveryBundleDisposition disposition)
        => (state, disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) =>
                (RouteInitRecoveryState.Removed, null),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained) =>
                (RouteInitRecoveryState.Retained, RouteInitFindingCode.RecoveryArtifactRetained),
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained) =>
                (RouteInitRecoveryState.Retained, RouteInitFindingCode.Interrupted),
            _ => (
                RouteInitRecoveryState.Unknown,
                state == RecoveryBundleDeletionState.Cancelled
                    ? RouteInitFindingCode.Interrupted
                    : RouteInitFindingCode.RecoveryFailed),
        };

    private static DirectoryCreationReceipt DirectoryReceipt(bool hasAfter)
    {
        var logicalPath = Path.Combine(RouteInitRedTestData.Workspace().LexicalRoot, ".agents", "route");
        var physicalPath = Path.GetFullPath(logicalPath);
        var creation = RouteInitRedTestData.Directory(logicalPath);
        var before = FileStateSnapshot.Missing(logicalPath);
        return hasAfter
            ? DirectoryCreationReceipt.VerificationFailed(
                creation,
                before,
                physicalPath,
                FileStateSnapshot.File(logicalPath, physicalPath, "unexpected"u8),
                "Verification differed.")
            : DirectoryCreationReceipt.VerificationUnavailable(
                creation,
                before,
                physicalPath,
                "Verification was unavailable.");
    }

    private static FileChangeReceipt FileReceipt(bool hasAfter)
    {
        var logicalPath = Path.Combine(RouteInitRedTestData.Workspace().LexicalRoot, ".agents", "route.md");
        var physicalPath = Path.GetFullPath(logicalPath);
        var before = FileStateSnapshot.File(logicalPath, physicalPath, "before"u8);
        var change = PlannedFileChange.Replace(before.Expectation, "after"u8);
        return hasAfter
            ? FileChangeReceipt.VerificationFailed(
                change,
                before,
                before,
                "Verification differed.")
            : FileChangeReceipt.VerificationUnavailable(
                change,
                before,
                "Verification was unavailable.");
    }

    private static void AssertResidualUndefined(
        Func<RouteInitEffectOutcome, RouteInitVerificationState, RouteInitEffectResidual> read)
    {
        var undefinedOutcome = (RouteInitEffectOutcome)int.MaxValue;
        AssertUndefined(
            () => read(undefinedOutcome, RouteInitVerificationState.Verified),
            "outcome",
            undefinedOutcome,
            "The Route Init effect outcome is not defined.");
        var undefinedVerification = (RouteInitVerificationState)int.MaxValue;
        AssertUndefined(
            () => read(RouteInitEffectOutcome.NotStarted, undefinedVerification),
            "verification",
            undefinedVerification,
            "The Route Init verification state is not defined.");
    }

    private static void AssertUndefined<TEnum>(
        Action action,
        string parameterName,
        TEnum actualValue,
        string message)
        where TEnum : struct, Enum
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(action);
        Assert.Equal(parameterName, exception.ParamName);
        Assert.Equal(actualValue, Assert.IsType<TEnum>(exception.ActualValue));
        Assert.StartsWith(message, exception.Message);
    }
}
