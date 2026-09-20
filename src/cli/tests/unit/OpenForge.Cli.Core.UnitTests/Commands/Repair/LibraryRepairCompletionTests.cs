using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class LibraryRepairCompletionTests
{
    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Lease"), InlineData("Revalidation"), InlineData("ForwardPreparation"), InlineData("Effect")]
    [InlineData("Verification"), InlineData("ForwardCleanup"), InlineData("PostDiagnosis")]
    public void IndependentCancellationFactsRetainStageAndDoNotInventAppliedEffects(string stage)
    {
        var execution = LibraryRepairData.Execution() with
        {
            Cancellation = new RepairLibraryCancellation(Enum.Parse<RepairLibraryExecutionStage>(stage), null),
        };
        var outcome = RepairLibraryRecoveryApplication.Complete(LibraryRepairData.Plan(), execution);
        Assert.Same(execution, outcome.LibraryExecution);
        Assert.Equal(0, outcome.Application.AppliedEffects);
        Assert.Equal(RepairApplicationState.Interrupted, outcome.Application.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("removed", "Removed"), InlineData("retained", "Retained"), InlineData("unknown", "Unknown")]
    public void IndependentForwardCleanupFactsPreserveResidualTruth(string cleanup, string recoveryState)
    {
        var path = Path.GetFullPath("forward-repair.zip");
        var deletion = cleanup switch
        {
            "removed" => RecoveryBundleDeletionResult.Deleted(),
            "retained" => RecoveryBundleDeletionResult.FailedRetained(path, "Independent cleanup failure."),
            _ => RecoveryBundleDeletionResult.FailedUnknown(path, "Independent cleanup uncertainty."),
        };
        var execution = LibraryRepairData.Execution() with { ForwardCleanup = deletion };
        var outcome = RepairLibraryRecoveryApplication.Complete(LibraryRepairData.Plan(), execution);
        Assert.Same(execution, outcome.LibraryExecution);
        Assert.Equal(Enum.Parse<RepairRecoveryState>(recoveryState), outcome.Recovery.State);
        Assert.Equal(0, outcome.Application.AppliedEffects);
        Assert.NotNull(outcome.LibraryExecution);
        Assert.Empty(outcome.LibraryExecution.LibraryReceipts);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, "Interrupted"), InlineData(true, "Failed")]
    public void ResultFormationPreservesFailurePrecedenceOverCancellation(bool failed, string status)
    {
        var plan = LibraryRepairData.Plan();
        var execution = LibraryRepairData.Execution() with
        {
            Cancellation = new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, 0),
            UnexpectedFailure = failed ? new RepairLibraryUnexpectedFailure(RepairLibraryExecutionStage.Effect, 0, "Effect failed.") : null,
        };
        var result = RepairLibraryResultFormation.Build(new RepairResultInput
        {
            Request = plan.Request,
            Plan = plan,
            LibraryExecution = execution,
            Diagnosis = RepairTestData.CompleteCoverage(),
            Findings = [],
            InitialFindings = [],
            Preflight = RepairPreflight.NotRequested,
            Application = RepairApplication.NotRequested,
            Verification = RepairVerification.NotRequested,
            Recovery = RepairRecovery.NotRequired,
            PostDiagnosis = RepairPostDiagnosis.NotRequested,
        });
        Assert.Equal(Enum.Parse<CliSemanticStatus>(status), result.Status);
        Assert.Same(execution, result.LibraryExecution);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false), InlineData(true)]
    public void UndefinedExecutionStagesAreRejected(bool failure)
    {
        var execution = LibraryRepairData.Execution();
        execution = failure
            ? execution with { UnexpectedFailure = new RepairLibraryUnexpectedFailure((RepairLibraryExecutionStage)int.MaxValue, 0, "Invalid stage.") }
            : execution with { Cancellation = new RepairLibraryCancellation((RepairLibraryExecutionStage)int.MaxValue, 0) };
        Assert.Throws<ArgumentException>(() => RepairLibraryRecoveryApplication.Complete(LibraryRepairData.Plan(), execution));
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, true, false, (int)RepairStepOutcome.Interrupted), InlineData(false, false, true, (int)RepairStepOutcome.Failed)]
    [InlineData(false, true, true, (int)RepairStepOutcome.Failed), InlineData(true, true, false, (int)RepairStepOutcome.Interrupted)]
    [InlineData(true, false, true, (int)RepairStepOutcome.Failed), InlineData(true, true, true, (int)RepairStepOutcome.Failed)]
    public void AtomicEffectFailureRetainsActualLibraryStep(bool mixed, bool cancelled, bool failed, int expected)
    {
        var plan = CreateActualPlan(mixed);
        var original = Assert.Single(plan.LibrarySteps);
        Assert.Equal(mixed ? 3 : 1, original.Ordinal);
        Assert.Equal(mixed ? 2 : 0, plan.Steps.Count);
        Assert.Equal(mixed ? 1 : 0, plan.Effects.Count);
        Assert.NotNull(original.Effect);
        if (mixed)
        {
            var effect = Assert.Single(plan.Effects);
            Assert.All(plan.Steps, step => Assert.Same(effect, step.Effect));
            Assert.Equal("new|new", Encoding.UTF8.GetString(effect.IntendedState.Bytes.AsSpan()));
        }

        var atomicOrdinal = mixed ? 1 : 0;
        var execution = LibraryRepairData.Execution() with
        {
            Cancellation = cancelled ? new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, atomicOrdinal) : null,
            UnexpectedFailure = failed ? new RepairLibraryUnexpectedFailure(RepairLibraryExecutionStage.Effect, atomicOrdinal, "Effect failed.") : null,
        };
        var result = FormResult(plan, execution);

        Assert.Same(execution, result.LibraryExecution);
        Assert.Empty(execution.LibraryReceipts);
        Assert.Empty(execution.ReferenceReceipts);
        Assert.Equal(0, result.Application.AppliedEffects);
        Assert.NotNull(result.Plan);
        Assert.All(result.Plan.Steps, step => Assert.Equal(RepairStepOutcome.Planned, step.Outcome));
        var actual = Assert.Single(result.Plan.LibrarySteps);
        Assert.Same(original.Effect, actual.Effect);
        Assert.Equal((RepairStepOutcome)expected, actual.Outcome);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData(false, null), InlineData(true, null), InlineData(true, 0)]
    public void UnrelatedOrAbsentAtomicOrdinalPreservesLibraryStep(bool mixed, int? effectOrdinal)
    {
        var plan = CreateActualPlan(mixed);
        var execution = LibraryRepairData.Execution() with
        {
            Cancellation = new RepairLibraryCancellation(RepairLibraryExecutionStage.Effect, effectOrdinal),
            UnexpectedFailure = new RepairLibraryUnexpectedFailure(RepairLibraryExecutionStage.Effect, effectOrdinal, "Independent effect failure."),
        };

        var result = FormResult(plan, execution);

        Assert.NotNull(result.Plan);
        Assert.Equal(RepairStepOutcome.Planned, Assert.Single(result.Plan.LibrarySteps).Outcome);
        Assert.Empty(execution.LibraryReceipts);
        Assert.Equal(0, result.Application.AppliedEffects);
    }

    private static RepairPlan CreateActualPlan(bool mixed)
    {
        var input = LibraryRepairData.Input(LibraryRepairData.Evidence());
        if (mixed)
        {
            var logicalPath = Path.Combine(input.Request.Workspace.PhysicalRoot, RepairTestData.SourcePath);
            var state = FileStateSnapshot.File(logicalPath, logicalPath, Encoding.UTF8.GetBytes("old|old"));
            input = input with
            {
                References =
                [
                    new RepairProposalInput(RepairTestData.SafeProposal(byteOffset: 4),
                        new RepairOccurrenceState(RepairTestData.SourcePath, RepairTestData.Occurrence(byteOffset: 4), state), "old"),
                    new RepairProposalInput(RepairTestData.SafeProposal(),
                        new RepairOccurrenceState(RepairTestData.SourcePath, RepairTestData.Occurrence(), state), "old"),
                ],
            };
        }

        var plan = RepairLibraryRecoveryPlanner.Build(input);
        Assert.False(plan.IsBlocked);
        return plan;
    }

    private static RepairResult FormResult(RepairPlan plan, RepairLibraryExecution execution)
        => RepairLibraryResultFormation.Build(new RepairResultInput
        {
            Request = plan.Request,
            Plan = plan,
            LibraryExecution = execution,
            Diagnosis = RepairTestData.CompleteCoverage(),
            Findings = [],
            InitialFindings = [],
            Preflight = RepairPreflight.NotRequested,
            Application = RepairApplication.NotRequested,
            Verification = RepairVerification.NotRequested,
            Recovery = RepairRecovery.NotRequired,
            PostDiagnosis = RepairPostDiagnosis.NotRequested,
        });
}
