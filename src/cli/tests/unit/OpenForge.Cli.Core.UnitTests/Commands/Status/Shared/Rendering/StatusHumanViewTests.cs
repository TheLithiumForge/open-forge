using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status.Shared.Rendering;

public sealed class StatusHumanViewTests
{
    [Theory(DisplayName = "Status groups navigation findings and retains unmatched subjects"), InlineData(false), InlineData(true), Trait("Feature", "status-presentation"), Trait("Evidence", "Unit")]
    public void RepeatedNavigationCausesAndPathsRenderOnce(bool compact)
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Attention);
        var finding = new StatusFinding
        {
            Code = StatusFindingCode.GeneratedNavigationChanged,
            Status = CliSemanticStatus.Attention,
            Subject = ".agents/one.md",
            Cause = "Generated navigation needs updating.",
        };
        var result = seed with
        {
            Findings = [finding, finding with { Subject = ".agents/two.md" }, finding with { Subject = ".agents/unmatched.md" }],
            Facts = seed.Facts with
            {
                Structure = seed.Facts.Structure with
                {
                    GeneratedNavigation = [new(".agents/one.md", OperationalGeneratedNavigationState.Changed), new(".agents/two.md", OperationalGeneratedNavigationState.Changed)],
                },
            },
        };
        var before = RenderJson(result);

        var text = Render(result, compact ? CliView.Compact : CliView.Expanded);

        Assert.Equal(1, text.Split(finding.Cause, StringSplitOptions.None).Length - 1);
        foreach (var subject in new[] { ".agents/one.md", ".agents/two.md", ".agents/unmatched.md" })
        {
            Assert.Equal(1, text.Split(subject, StringSplitOptions.None).Length - 1);
        }

        Assert.Contains("[generated-navigation-changed]", text, StringComparison.Ordinal);
        Assert.Contains("Affected navigation paths are listed under Routes.", text, StringComparison.Ordinal);
        Assert.Equal(before, RenderJson(result));
    }

    [Fact(DisplayName = "Compact Status summarizes current navigation and preserves every other path"), Trait("Feature", "status-presentation"), Trait("Evidence", "Unit")]
    public void NavigationViewsRetainProblemsWithoutRepeatingHealthyPaths()
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Attention);
        var result = seed with
        {
            Facts = seed.Facts with
            {
                Structure = seed.Facts.Structure with
                {
                    GeneratedNavigation =
                    [
                        new(".agents/current.md", OperationalGeneratedNavigationState.Current),
                        new(".agents/changed.md", OperationalGeneratedNavigationState.Changed),
                        new(".agents/missing.md", OperationalGeneratedNavigationState.Missing),
                        new(".agents/unavailable.md", OperationalGeneratedNavigationState.Unavailable),
                        new(".agents/blocked.md", OperationalGeneratedNavigationState.Blocked),
                        new(".agents/not-applicable.md", OperationalGeneratedNavigationState.NotApplicable),
                    ],
                },
            },
        };
        var before = RenderJson(result);
        var compact = Render(result, CliView.Compact);
        var expanded = Render(result, CliView.Expanded);

        Assert.DoesNotContain(".agents/current.md", compact, StringComparison.Ordinal);
        Assert.Contains(".agents/current.md", expanded, StringComparison.Ordinal);
        foreach (var item in result.Facts.Structure.GeneratedNavigation.Skip(1))
        {
            Assert.Contains(item.Path, compact, StringComparison.Ordinal);
            Assert.Contains(item.Path, expanded, StringComparison.Ordinal);
        }

        foreach (var candidate in result.Facts.Recovery.Candidates)
        {
            Assert.Contains(candidate.Path, compact, StringComparison.Ordinal);
            Assert.Contains(candidate.Path, expanded, StringComparison.Ordinal);
        }

        Assert.Equal(before, RenderJson(result));
        Assert.Contains("""
            Generated navigation:
              1 current
              1 need updating
              1 missing
              1 unavailable
              1 blocked
              1 not-applicable
              .agents/changed.md: need updating
              .agents/missing.md: missing
              .agents/unavailable.md: unavailable
              .agents/blocked.md: blocked
              .agents/not-applicable.md: not-applicable
            """, compact.ReplaceLineEndings("\n"), StringComparison.Ordinal);
    }

    private static string Render(StatusResult result, CliView view)
        => StatusHumanRenderer.Render(new CliPresentationRequest<StatusResult>(result, new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal)));

    private static string RenderJson(StatusResult result)
        => StatusJsonRenderer.Render(new CliPresentationRequest<StatusResult>(result, new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
}
