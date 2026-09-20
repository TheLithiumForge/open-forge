using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup.Shared.Application;

public sealed class CleanupUnattemptedEffectsTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Cleanup cannot claim planned candidates remain when no current catalogue was established"), Trait("Feature", "cleanup-unattempted-effects"), Trait("Evidence", "UnitContract")]
    public void MissingCurrentCatalogueDoesNotClaimRetention()
    {
        var result = new CleanupResultBuilder(Plan());

        result.PreserveUnattempted(observed: null);

        Assert.Equal(2, result.Effects.Length);
        Assert.All(result.Effects, effect =>
        {
            Assert.Equal(CleanupEffectOutcome.CompletionUnknown, effect.Outcome);
            Assert.Equal(CleanupEffectResidual.Unknown, effect.Residual);
            Assert.Contains("No deletion was attempted", effect.Cause, StringComparison.Ordinal);
        });
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Cleanup distinguishes an observed retained candidate from a stale planned path missing at session opening"),
     Trait("Feature", "cleanup-unattempted-effects"), Trait("Evidence", "UnitContract")]
    public void CurrentObservationSeparatesRetainedAndMissingPlannedPaths()
    {
        var plan = Plan();
        var result = new CleanupResultBuilder(plan);
        var observed = RecoveryBundleCatalogueResult.Available([plan.Entries[0].Candidate.Snapshot]);

        result.PreserveUnattempted(observed);

        Assert.Equal(2, result.Effects.Length);
        Assert.Same(plan.Entries[0], result.Effects[0].PlanEntry);
        Assert.Equal(CleanupEffectOutcome.NotStarted, result.Effects[0].Outcome);
        Assert.Equal(CleanupEffectResidual.Retained, result.Effects[0].Residual);
        Assert.Same(plan.Entries[1], result.Effects[1].PlanEntry);
        Assert.Equal(CleanupEffectOutcome.CompletionUnknown, result.Effects[1].Outcome);
        Assert.Equal(CleanupEffectResidual.Unknown, result.Effects[1].Residual);
        Assert.Contains("No deletion was attempted", result.Effects[1].Cause, StringComparison.Ordinal);
        Assert.DoesNotContain(result.Effects, effect => effect.Outcome == CleanupEffectOutcome.Verified);
    }

    private static CleanupPlan Plan()
    {
        var workspace = CleanupTestData.Workspace("unattempted-effects");
        var first = CleanupTestData.Candidate(selectedWorkspace: workspace, path: Path.Combine(Path.GetTempPath(), "cleanup-unattempted", "first.zip"));
        var second = CleanupTestData.Candidate(selectedWorkspace: workspace, path: Path.Combine(Path.GetTempPath(), "cleanup-unattempted", "second.zip"));
        return CleanupTestData.Plan(CleanupTestData.Request(workspace), CleanupTestData.Catalogue(CleanupCatalogueCoverage.Complete, first, second));
    }
}
