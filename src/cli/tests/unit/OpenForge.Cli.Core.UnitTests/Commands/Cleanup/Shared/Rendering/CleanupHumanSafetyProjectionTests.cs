using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup.Shared.Rendering;

public sealed class CleanupHumanSafetyProjectionTests
{
    [Theory(DisplayName = "Both Cleanup human views retain newly observed preserved paths after catalogue drift"),
     InlineData(false), InlineData(true),
     Trait("Feature", "cleanup-c1-human"), Trait("Evidence", "UnitContract")]
    public void NewlyObservedPreservedPathsRemainVisible(bool expanded)
    {
        var view = expanded ? CliView.Expanded : CliView.Compact;
        var workspace = CleanupTestData.Workspace("human-observed");
        var original = CleanupTestData.Candidate(selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-human-observed", "first.zip"));
        var additional = CleanupTestData.Candidate(selectedWorkspace: workspace,
            path: Path.Combine(Path.GetTempPath(), "cleanup-human-observed", "newly-observed.zip"));
        var plan = CleanupTestData.Plan(CleanupTestData.Request(workspace), CleanupTestData.Catalogue(candidates: [original]));
        var builder = new CleanupResultBuilder(plan)
        {
            Revalidation = new CleanupCatalogueComparison
            {
                State = CleanupCatalogueComparisonState.Changed,
                Planned = plan.Catalogue,
                Observed = CleanupTestData.Catalogue(candidates: [original, additional]),
            },
        };
        builder.AddFinding(CleanupFindingCode.CatalogueChangedDuringApply, "The catalogue gained an exact candidate.");
        var result = builder.Build();

        var text = CleanupPresentation.Human(Presentation(result, view));

        Assert.Contains(additional.Path, text, StringComparison.Ordinal);
        Assert.Contains(original.Path, text, StringComparison.Ordinal);
        Assert.Contains("preserved", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Removed and verified", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Both Cleanup human dry-run views state application lease and final validation contingency"),
     InlineData(false), InlineData(true),
     Trait("Feature", "cleanup-c1-human"), Trait("Evidence", "UnitContract")]
    public void PlannedDeletionRetainsApplicationContingency(bool expanded)
    {
        var view = expanded ? CliView.Expanded : CliView.Compact;
        var plan = CleanupTestData.Plan(CleanupTestData.Request(mode: CleanupMode.DryRun));
        var builder = new CleanupResultBuilder(plan)
        {
            Effects = [.. plan.DeletionEntries.Select(entry =>
                CleanupEffect.Create(entry, CleanupEffectOutcome.Planned, CleanupEffectResidual.None))],
        };
        var result = builder.Build();

        var text = CleanupPresentation.Human(Presentation(result, view));

        Assert.Contains("application lease", text, StringComparison.Ordinal);
        Assert.Contains("final validation", text, StringComparison.Ordinal);
        Assert.Contains(plan.DeletionEntries[0].Path, text, StringComparison.Ordinal);
        Assert.Contains("planned", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Removed and verified", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Both Cleanup human views expose candidate-only previews and avoid repeated effect paths"),
        InlineData(false), InlineData(true),
        Trait("Feature", "cleanup-c1-human"), Trait("Evidence", "Unit")]
    public void CandidateOnlyPreviewRemainsVisible(bool expanded)
    {
        var view = expanded ? CliView.Expanded : CliView.Compact;
        var plan = CleanupTestData.Plan(CleanupTestData.Request(mode: CleanupMode.DryRun));
        var builder = new CleanupResultBuilder(plan);
        var candidateOnly = builder.Build();
        var path = plan.DeletionEntries[0].Path;
        var before = CleanupPresentation.Json(Presentation(candidateOnly, view));
        var text = CleanupPresentation.Human(Presentation(candidateOnly, view));
        Assert.Contains(path, text, StringComparison.Ordinal);
        Assert.DoesNotContain("Removed and verified", text, StringComparison.Ordinal);
        Assert.Equal(before, CleanupPresentation.Json(Presentation(candidateOnly, view)));

        builder.Effects = [.. plan.DeletionEntries.Select(entry =>
            CleanupEffect.Create(entry, CleanupEffectOutcome.Planned, CleanupEffectResidual.None))];
        text = CleanupPresentation.Human(Presentation(builder.Build(), view));
        Assert.Equal(1, text.Split(path, StringSplitOptions.None).Length - 1);
    }

    private static CliPresentationRequest<CleanupResult> Presentation(CleanupResult result, CliView view)
        => new(result, new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal));
}
