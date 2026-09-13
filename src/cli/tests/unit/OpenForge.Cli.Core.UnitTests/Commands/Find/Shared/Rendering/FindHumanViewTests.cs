using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Rendering;

public sealed class FindHumanViewTests
{
    [Theory(DisplayName = "Find views retain full finding subjects and do not change JSON"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void FullSubjectAndJson(int viewValue)
    {
        var source = FindPresentationTestData.InvalidResult();
        var original = source.Findings[0];
        var subject = $".agents/{new string('x', 600)}.md";
        var finding = new FindFinding(code: original.Code, status: original.Status, subject: subject,
            cause: original.Cause, selectorRole: original.SelectorRole, selectorOccurrence: original.SelectorOccurrence,
            source: original.Source, layer: original.Layer, path: original.Path, region: original.Region,
            location: original.Location, candidates: original.Candidates);
        var result = new FindResult(status: source.Status, workspace: source.Workspace, universe: source.Universe,
            query: source.Query, presentation: source.Presentation, coverage: source.Coverage,
            findings: [finding], matches: source.Matches, next: source.Next);
        var jsonRequest = FindPresentationTestData.PresentationRequest(result, format: CliOutputFormat.Json);
        var before = FindJsonRenderer.Render(jsonRequest);
        var output = FindHumanRenderer.Render(FindPresentationTestData.PresentationRequest(result, view: (CliView)viewValue));

        Assert.Contains(subject, output, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge find --help", output, StringComparison.Ordinal);
        Assert.Equal(before, FindJsonRenderer.Render(jsonRequest));
    }

    [Fact(DisplayName = "Find expanded places matches before search details and incomplete findings before matches"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void AnswerPrecedesExplanation()
    {
        var output = FindExpandedRenderer.Render(FindPresentationTestData.IncompleteResult(), CliHumanStyle.Plain);
        Assert.Contains("Status: incomplete", output, StringComparison.Ordinal);
        var match = output.IndexOf("Path: .agents/docs.md", StringComparison.Ordinal);
        Assert.True(match >= 0);
        Assert.True(output.IndexOf("[find.", StringComparison.Ordinal) < match);
        Assert.True(match < output.IndexOf("Search details:", StringComparison.Ordinal));
    }
}
