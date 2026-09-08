using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class LibraryRepairCompletionTests
{
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
}
