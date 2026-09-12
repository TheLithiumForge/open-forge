using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup.Shared.Application;

public sealed class CleanupDeletionProgressTests
{
    [Fact(DisplayName = "Cleanup stops after a changed candidate and preserves prior deletion and remaining residual facts"),
     Trait("Feature", "cleanup-session-progress"), Trait("Evidence", "UnitContract")]
    public void ChangedCandidateStopsFurtherEffects()
    {
        var plan = Plan();
        var progress = Start(plan);

        progress.Record(RecoveryBundleDeletionResult.BlockedUnknown("The final changed.", plan.Entries[1].Path));

        AssertStopped(progress, plan, CleanupEffectOutcome.CompletionUnknown, CleanupEffectResidual.Unknown, CleanupFindingCode.CandidateChangedDuringApply);
    }

    [Fact(DisplayName = "Cleanup stops after cancellation with prior verified deletion and the remaining candidates retained"),
     Trait("Feature", "cleanup-session-progress"), Trait("Evidence", "UnitContract")]
    public void CancellationStopsFurtherEffects()
    {
        var plan = Plan();
        var progress = Start(plan);

        progress.Record(RecoveryBundleDeletionResult.CancelledRetained(plan.Entries[1].Path));

        AssertStopped(progress, plan, CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained, CleanupFindingCode.Interrupted);
    }

    [Fact(DisplayName = "Cleanup stops after failed absence verification and reports the retained candidate separately from prior success"),
     Trait("Feature", "cleanup-session-progress"), Trait("Evidence", "UnitContract")]
    public void FailedVerificationStopsFurtherEffects()
    {
        var plan = Plan();
        var progress = Start(plan);
        var failure = new FilesystemFailure(FilesystemFailureKind.InputOutput, "The candidate remains.");

        progress.Record(RecoveryBundleDeletionResult.FailedRetained(plan.Entries[1].Path, failure.DirectCause, failure));

        AssertStopped(progress, plan, CleanupEffectOutcome.VerificationFailed, CleanupEffectResidual.Retained, CleanupFindingCode.VerificationFailed);
    }

    private static CleanupDeletionProgress Start(CleanupPlan plan)
    {
        var progress = new CleanupDeletionProgress(plan);
        Assert.Same(plan.Entries[0], progress.Next);
        progress.Record(RecoveryBundleDeletionResult.Deleted());
        Assert.Same(plan.Entries[1], progress.Next);
        return progress;
    }

    private static void AssertStopped(
        CleanupDeletionProgress progress,
        CleanupPlan plan,
        CleanupEffectOutcome outcome,
        CleanupEffectResidual residual,
        CleanupFindingCode finding)
    {
        Assert.Null(progress.Next);
        Assert.Throws<InvalidOperationException>(() => progress.Record(RecoveryBundleDeletionResult.Deleted()));
        var effects = progress.Effects;
        Assert.Equal(3, effects.Length);
        Assert.Equal(CleanupEffectOutcome.Verified, effects[0].Outcome);
        Assert.Equal(CleanupEffectResidual.None, effects[0].Residual);
        Assert.Equal(outcome, effects[1].Outcome);
        Assert.Equal(residual, effects[1].Residual);
        Assert.Equal(CleanupEffectOutcome.NotStarted, effects[2].Outcome);
        Assert.Equal(CleanupEffectResidual.Retained, effects[2].Residual);
        Assert.Equal(plan.Entries, effects.Select(effect => effect.PlanEntry));
        Assert.Equal(finding, Assert.Single(progress.Findings).Code);
        Assert.Equal(plan.Entries[1].Path, Assert.Single(progress.Findings).Subject);
    }

    private static CleanupPlan Plan()
    {
        var workspace = CleanupTestData.Workspace("session-progress");
        var candidates = Enumerable.Range(0, 3).Select(index => CleanupTestData.Candidate(
            selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-progress", $"candidate-{index}.zip"))).ToArray();
        return CleanupTestData.Plan(CleanupTestData.Request(workspace), CleanupTestData.Catalogue(CleanupCatalogueCoverage.Complete, candidates));
    }
}
