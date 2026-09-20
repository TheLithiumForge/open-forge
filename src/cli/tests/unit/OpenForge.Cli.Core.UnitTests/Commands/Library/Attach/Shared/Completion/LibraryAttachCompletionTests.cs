using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Attach.Shared.Completion;

public sealed class LibraryAttachCompletionTests
{

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)RecoveryBundlePreparationState.Incomplete, false, (int)CliSemanticStatus.Incomplete)]
    [InlineData((int)RecoveryBundlePreparationState.Incomplete, true, (int)CliSemanticStatus.Incomplete)]
    [InlineData((int)RecoveryBundlePreparationState.Blocked, false, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)RecoveryBundlePreparationState.Blocked, true, (int)CliSemanticStatus.Blocked)]
    [InlineData((int)RecoveryBundlePreparationState.Cancelled, false, (int)CliSemanticStatus.Interrupted)]
    [InlineData((int)RecoveryBundlePreparationState.Cancelled, true, (int)CliSemanticStatus.Interrupted)]
    public void FailedPreparationRetainsStatusAndUncertainResidual(int state, bool hasResidual, int status)
    {
        var input = LibraryMutationCompletionData.Attach();
        var path = hasResidual ? LibraryMutationPlanningData.Absolute("external-recovery.bundle") : null;
        var preparation = (RecoveryBundlePreparationState)state switch
        {
            RecoveryBundlePreparationState.Incomplete => RecoveryBundlePreparationResult.Incomplete("Preparation unavailable.", path),
            RecoveryBundlePreparationState.Blocked => RecoveryBundlePreparationResult.Blocked("Preparation blocked.", path),
            RecoveryBundlePreparationState.Cancelled => RecoveryBundlePreparationResult.Cancelled(path),
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        input = input with
        {
            Execution = LibraryMutationCompletionData.Empty() with { RecoveryPreparationOutcome = preparation },
        };

        var result = LibraryAttachCompletion.Complete(input);

        Assert.Equal((CliSemanticStatus)status, result.Status);
        var application = result.Result.Application;
        Assert.Equal(
            preparation.State == RecoveryBundlePreparationState.Cancelled ? LibraryApplicationState.Interrupted : LibraryApplicationState.NotStarted,
            application.State);
        Assert.Equal(LibraryVerificationState.NotStarted, application.Verification);
        Assert.Equal(LibraryRecordPublicationState.NotStarted, application.RecordPublication.State);
        Assert.Null(application.RecordPublication.PublishedLast);
        Assert.Equal(LibraryRecoveryState.Unknown, application.Recovery.State);
        Assert.Equal(path, application.Recovery.Path);
        Assert.Contains(result.Result.Findings, finding => finding.Code == (
            preparation.State == RecoveryBundlePreparationState.Cancelled ? LibraryAttachFindingCode.Interrupted : LibraryAttachFindingCode.RecoveryUnavailable));
        if (hasResidual)
        {
            var residual = Assert.Single(application.Residuals);
            Assert.Equal(LibraryResidualKind.Recovery, residual.Kind);
            Assert.Equal(LibraryResidualState.Unknown, residual.State);
            Assert.Equal(path, residual.Path);
        }
        else
        {
            Assert.Empty(application.Residuals);
        }
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("removed", (int)CliSemanticStatus.Complete, (int)LibraryRecoveryState.Removed)]
    [InlineData("retained", (int)CliSemanticStatus.Attention, (int)LibraryRecoveryState.Retained)]
    [InlineData("unknown", (int)CliSemanticStatus.Failed, (int)LibraryRecoveryState.Unknown)]
    public void IndependentCleanupFactsReduceVerifiedEffects(string disposition, int status, int recovery)
    {
        var input = LibraryMutationCompletionData.Attach();
        var path = LibraryMutationPlanningData.Absolute("external-recovery.bundle");
        var cleanup = disposition switch
        {
            "removed" => RecoveryBundleDeletionResult.Deleted(),
            "retained" => RecoveryBundleDeletionResult.FailedRetained(path, "Exact bundle remains."),
            _ => RecoveryBundleDeletionResult.FailedUnknown(path, "Final bundle disposition unavailable."),
        };
        input = input with { Execution = input.Execution with { RecoveryCleanup = cleanup } };
        Assert.Null(input.Execution.RecoveryPreparation);
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal((LibraryRecoveryState)recovery, result.Result.Application.Recovery.State);
        Assert.Equal(LibraryVerificationState.Verified, result.Result.Application.Verification);
        Assert.Equal(LibraryRecordPublicationState.Verified, result.Result.Application.RecordPublication.State);
        Assert.True(result.Result.Application.RecordPublication.PublishedLast);
        if (disposition != "removed")
        {
            Assert.Contains(result.Result.Application.Residuals, residual => residual.Kind == LibraryResidualKind.Recovery
                && residual.State == (disposition == "retained" ? LibraryResidualState.Retained : LibraryResidualState.Unknown));
        }
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, (int)CliSemanticStatus.Interrupted)]
    [InlineData(true, (int)CliSemanticStatus.Failed)]
    public void PostEffectFailureOutranksCancellationWithoutErasingVerifiedPublication(bool unexpectedFailure, int status)
    {
        var input = LibraryMutationCompletionData.Attach();
        input = input with
        {
            Execution = input.Execution with
            {
                Cancellation = new LibraryCancellationFact(LibraryExecutionStage.RecoveryCleanup),
                UnexpectedFailure = unexpectedFailure ? new LibraryUnexpectedFailureFact(LibraryExecutionStage.Verification, "Unexpected verification failure.") : null,
                RecoveryCleanup = RecoveryBundleDeletionResult.CancelledRetained(LibraryMutationPlanningData.Absolute("external-recovery.bundle")),
            }
        };
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(LibraryRecordPublicationState.Verified, result.Result.Application.RecordPublication.State);
        Assert.True(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Equal(LibraryRecoveryState.Retained, result.Result.Application.Recovery.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CancellationBeforeEffectsDoesNotInventRecoveryOrPublication()
    {
        var input = LibraryMutationCompletionData.Attach();
        input = input with
        {
            Execution = LibraryMutationCompletionData.Empty() with
            { Cancellation = new LibraryCancellationFact(LibraryExecutionStage.Preflight) }
        };
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(LibraryRecordPublicationState.NotStarted, result.Result.Application.RecordPublication.State);
        Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Empty(result.Result.Application.Residuals);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnknownRecordCompletionRetainsEarlierVerifiedLinkAndUnknownRecordTruth()
    {
        var input = LibraryMutationCompletionData.Attach();
        var record = Assert.IsType<FileChangeReceipt>(input.Execution.Record);
        input = input with
        {
            Execution = input.Execution with
            {
                Record = FileChangeReceipt.CompletionUnknown(record.Change, record.Before, after: null, "Publication completion unavailable."),
                RecordPublicationOrder = LibraryRecordPublicationOrder.NotObserved,
                UnexpectedFailure = new LibraryUnexpectedFailureFact(LibraryExecutionStage.Application, "Record effect failed."),
                RecoveryCleanup = RecoveryBundleDeletionResult.FailedRetained(LibraryMutationPlanningData.Absolute("external-recovery.bundle"), "Recovery retained."),
            }
        };
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(LibraryRecordPublicationState.Unknown, result.Result.Application.RecordPublication.State);
        Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Contains(result.Result.Application.Residuals, residual => residual.Kind == LibraryResidualKind.Record && residual.State == LibraryResidualState.Unknown);
        Assert.Contains(result.Result.Application.Residuals, residual => residual.Kind == LibraryResidualKind.Link && residual.State == LibraryResidualState.Retained);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("not-last"), InlineData("scope-incomplete"), InlineData("source-target")]
    public void VerifiedBytesAloneCannotEstablishACompleteSafeApplication(string missingProof)
    {
        var input = LibraryMutationCompletionData.Attach();
        var scope = Assert.IsType<LibrarySourceEffectScopeFacts>(input.Execution.SourceEffectScope);
        input = input with
        {
            Execution = missingProof switch
            {
                "not-last" => input.Execution with { RecordPublicationOrder = LibraryRecordPublicationOrder.NotLast },
                "scope-incomplete" => input.Execution with { SourceEffectScope = scope with { IsComplete = false } },
                _ => input.Execution with
                {
                    SourceEffectScope = scope with
                    { AttemptedMutationTargets = scope.AttemptedMutationTargets.Add(CanonicalRelativePath.Create("shared/team-knowledge/.agents/directives/review.md")) }
                },
            }
        };
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(LibraryRecordPublicationState.Verified, result.Result.Application.RecordPublication.State);
        if (missingProof == "not-last")
        {
            Assert.False(result.Result.Application.RecordPublication.PublishedLast);
        }
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, (int)CliSemanticStatus.Incomplete)]
    [InlineData(true, (int)CliSemanticStatus.Blocked)]
    public void KnownPreflightBlockerOutranksUnavailableFactsWithoutEffects(bool blocked, int status)
    {
        var input = LibraryMutationCompletionData.Attach();
        var plan = Assert.IsType<LibraryAttachPlan>(input.Plan);
        var unavailable = new LibraryAttachFinding
        {
            Code = LibraryAttachFindingCode.RecordUnavailable,
            Status = CliSemanticStatus.Incomplete,
            LibraryId = "team-knowledge",
            Path = ".agents/open-forge.lock.json",
            Cause = "Required record read unavailable.",
        };
        var unsafeTarget = new LibraryAttachFinding
        {
            Code = LibraryAttachFindingCode.MappingBlocked,
            Status = CliSemanticStatus.Blocked,
            LibraryId = "team-knowledge",
            Path = ".agents/directives/review.md",
            Cause = "Unsafe observed destination.",
        };
        input = input with
        {
            Plan = plan with
            {
                State = blocked ? LibraryPlanState.Blocked : LibraryPlanState.Incomplete,
                Findings = blocked ? [unavailable, unsafeTarget] : [unavailable]
            },
            Execution = LibraryMutationCompletionData.Empty(),
        };
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Empty(result.Result.Application.Residuals);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompleteDryRunPlanDoesNotInventAttentionOrAppliedEffects()
    {
        var input = LibraryMutationCompletionData.Attach();
        var observations = Assert.IsType<LibraryAttachPlanningInput>(input.Observations);
        var request = input.Request with { Mode = LibraryMode.DryRun };
        observations = observations with { Request = request };
        input = input with
        {
            Request = request,
            Observations = observations,
            Plan = Assert.IsType<LibraryAttachPlan>(input.Plan) with { Input = observations },
            Execution = LibraryMutationCompletionData.Empty(),
        };
        var result = LibraryAttachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Empty(result.Result.Application.Residuals);
    }
}
