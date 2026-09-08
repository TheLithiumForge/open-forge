using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Detach.Shared.Completion;

public sealed class LibraryDetachCompletionTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("removed", (int)CliSemanticStatus.Complete, (int)LibraryRecoveryState.Removed)]
    [InlineData("retained", (int)CliSemanticStatus.Attention, (int)LibraryRecoveryState.Retained)]
    [InlineData("unknown", (int)CliSemanticStatus.Failed, (int)LibraryRecoveryState.Unknown)]
    public void IndependentCleanupFactsReduceVerifiedEffects(string disposition, int status, int recovery)
    {
        var input = LibraryMutationCompletionData.Detach();
        var path = LibraryMutationPlanningData.Absolute("external-recovery.bundle");
        var cleanup = disposition switch
        {
            "removed" => RecoveryBundleDeletionResult.Deleted(),
            "retained" => RecoveryBundleDeletionResult.FailedRetained(path, "Exact bundle remains."),
            _ => RecoveryBundleDeletionResult.FailedUnknown(path, "Final bundle disposition unavailable."),
        };
        input = input with { Execution = input.Execution with { RecoveryCleanup = cleanup } };
        Assert.Null(input.Execution.RecoveryPreparation);
        var result = LibraryDetachCompletion.Complete(input);
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

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, (int)CliSemanticStatus.Interrupted)]
    [InlineData(true, (int)CliSemanticStatus.Failed)]
    public void PostEffectFailureOutranksCancellationWithoutErasingVerifiedPublication(bool unexpectedFailure, int status)
    {
        var input = LibraryMutationCompletionData.Detach();
        input = input with
        {
            Execution = input.Execution with
            {
                Cancellation = new LibraryCancellationFact(LibraryExecutionStage.RecoveryCleanup),
                UnexpectedFailure = unexpectedFailure ? new LibraryUnexpectedFailureFact(LibraryExecutionStage.Verification, "Unexpected verification failure.") : null,
                RecoveryCleanup = RecoveryBundleDeletionResult.CancelledRetained(LibraryMutationPlanningData.Absolute("external-recovery.bundle")),
            }
        };
        var result = LibraryDetachCompletion.Complete(input);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(LibraryRecordPublicationState.Verified, result.Result.Application.RecordPublication.State);
        Assert.True(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Equal(LibraryRecoveryState.Retained, result.Result.Application.Recovery.State);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CancellationBeforeEffectsDoesNotInventRecoveryOrPublication()
    {
        var input = LibraryMutationCompletionData.Detach();
        input = input with
        {
            Execution = LibraryMutationCompletionData.Empty() with
            { Cancellation = new LibraryCancellationFact(LibraryExecutionStage.Preflight) }
        };
        var result = LibraryDetachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(LibraryRecordPublicationState.NotStarted, result.Result.Application.RecordPublication.State);
        Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Empty(result.Result.Application.Residuals);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnknownRecordCompletionRetainsEarlierVerifiedLinkAndUnknownRecordTruth()
    {
        var input = LibraryMutationCompletionData.Detach();
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
        var result = LibraryDetachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(LibraryRecordPublicationState.Unknown, result.Result.Application.RecordPublication.State);
        Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Contains(result.Result.Application.Residuals, residual => residual.Kind == LibraryResidualKind.Record && residual.State == LibraryResidualState.Unknown);
        Assert.Contains(result.Result.Application.Residuals, residual => residual.Kind == LibraryResidualKind.Link && residual.State == LibraryResidualState.Retained);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("not-last"), InlineData("scope-incomplete"), InlineData("source-target")]
    public void VerifiedBytesAloneCannotEstablishACompleteSafeApplication(string missingProof)
    {
        var input = LibraryMutationCompletionData.Detach();
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
        var result = LibraryDetachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(LibraryRecordPublicationState.Verified, result.Result.Application.RecordPublication.State);
        if (missingProof == "not-last")
        {
            Assert.False(result.Result.Application.RecordPublication.PublishedLast);
        }
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, (int)CliSemanticStatus.Incomplete)]
    [InlineData(true, (int)CliSemanticStatus.Blocked)]
    public void KnownPreflightBlockerOutranksUnavailableFactsWithoutEffects(bool blocked, int status)
    {
        var input = LibraryMutationCompletionData.Detach();
        var plan = Assert.IsType<LibraryDetachPlan>(input.Plan);
        var unavailable = new LibraryDetachFinding
        {
            Code = LibraryDetachFindingCode.RecordUnavailable,
            Status = CliSemanticStatus.Incomplete,
            LibraryId = "team-knowledge",
            Path = ".agents/open-forge.libraries.json",
            Cause = "Required record read unavailable.",
        };
        var unsafeTarget = new LibraryDetachFinding
        {
            Code = LibraryDetachFindingCode.MappingBlocked,
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
        var result = LibraryDetachCompletion.Complete(input);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Empty(result.Result.Application.Residuals);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void CompleteDryRunPlanDoesNotInventAttentionOrAppliedEffects()
    {
        var input = LibraryMutationCompletionData.Detach();
        var observations = Assert.IsType<LibraryDetachPlanningInput>(input.Observations);
        var request = input.Request with { Mode = LibraryMode.DryRun };
        observations = observations with { Request = request };
        input = input with
        {
            Request = request,
            Observations = observations,
            Plan = Assert.IsType<LibraryDetachPlan>(input.Plan) with { Input = observations },
            Execution = LibraryMutationCompletionData.Empty(),
        };
        var result = LibraryDetachCompletion.Complete(input);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Null(result.Result.Application.RecordPublication.PublishedLast);
        Assert.Empty(result.Result.Application.Residuals);
    }
}
