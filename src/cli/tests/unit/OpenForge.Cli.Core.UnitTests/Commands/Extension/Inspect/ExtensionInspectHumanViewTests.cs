using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect;

public sealed class ExtensionInspectHumanViewTests
{
    [Theory(DisplayName = "Extension Inspect views preserve unknown counts, exact finding identities and human source locations"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void UnknownCountsAndFindingIdentity(int view)
    {
        var path = $".agents/{new string('x', 600)}.md";
        var result = ExtensionInspectResultBuilder.InvalidStableId(null, "Toolkit", "The ID is not lowercase.");
        result = result with
        {
            Findings = [new()
            {
                Code = ExtensionInspectFindingCode.PathUnavailable, Status = CliSemanticStatus.Incomplete,
                Subject = "selected-source", PackageId = "toolkit", Dependency = "dependency",
                Path = path, Cause = "The file could not be read.", Location = new SourceLocation(12, 4, 600, 10),
                Candidates = [new() { Id = "alternative", Path = "source/alternative" }],
            }],
        };
        var request = new CliPresentationRequest<ExtensionInspectResult>(result, new(CliOutputFormat.Human, (CliView)view, CliVerbosity.Normal));
        var before = ExtensionInspectJsonRenderer.Render(request);
        var text = ExtensionInspectHumanRenderer.Render(request);
        Assert.Contains("unknown intended", text, StringComparison.Ordinal);
        Assert.DoesNotContain("0 intended", text, StringComparison.Ordinal);
        Assert.Contains(path, text, StringComparison.Ordinal);
        Assert.Contains("Line 12, column 4", text, StringComparison.Ordinal);
        Assert.DoesNotContain("600+10", text, StringComparison.Ordinal);
        Assert.Contains("Subject: selected-source", text, StringComparison.Ordinal);
        Assert.Contains("Package: toolkit", text, StringComparison.Ordinal);
        Assert.Contains("Dependency: dependency", text, StringComparison.Ordinal);
        Assert.Contains("alternative: source/alternative", text, StringComparison.Ordinal);
        Assert.Equal(before, ExtensionInspectJsonRenderer.Render(request));
    }

    [Theory(DisplayName = "Compact Extension Inspect summarizes healthy source-only paths while preserving unavailable observations"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void SourceOnlySummaryPreservesUnavailablePaths(int view)
    {
        var result = ExtensionInspectResultBuilder.InvalidStableId(null, "Toolkit", "The ID is not lowercase.");
        result = result with
        {
            Subject = result.Subject with { State = ExtensionInspectSubjectState.Resolved, Id = "toolkit", Candidates = [new() { Id = "toolkit", Path = "source/extension.json" }] },
            Comparison = result.Comparison with
            {
                Mode = ExtensionInspectComparisonMode.AvailableOnly,
                State = ExtensionInspectComparisonState.Complete,
                Paths = [Comparison(".agents/healthy.md"), Comparison(".agents/unavailable.md")],
            },
            PathFacts = result.PathFacts with
            {
                Declared =
                [
                    new() { Path = ".agents/healthy.md", SourcePath = "source/healthy.md", State = ExtensionInspectDeclaredPathState.Available },
                    new() { Path = ".agents/unavailable.md", SourcePath = "source/unavailable.md", State = ExtensionInspectDeclaredPathState.Unavailable },
                ],
            },
        };
        var text = ExtensionInspectHumanRenderer.Render(new(result, new(CliOutputFormat.Human, (CliView)view, CliVerbosity.Normal)));
        Assert.DoesNotContain("no choice made", text, StringComparison.Ordinal);
        Assert.Contains("toolkit: source/extension.json", text, StringComparison.Ordinal);
        Assert.Contains(".agents/unavailable.md", text, StringComparison.Ordinal);
        Assert.Equal(view == (int)CliView.Expanded, text.Contains(".agents/healthy.md", StringComparison.Ordinal));
        Assert.Equal(view == (int)CliView.Compact, text.Contains("Paths summarized: 1.", StringComparison.Ordinal));
    }

    private static ExtensionInspectPathComparison Comparison(string path) => new()
    {
        Path = path,
        Baseline = null,
        Current = null,
        Intended = null,
        Relation = ExtensionInspectPathRelation.NotApplicable,
        BaselineOwners = [],
        CurrentOwners = [],
        IntendedOwners = [],
    };

    [Theory(DisplayName = "Extension Inspect explains membership and local changes without asserting a mutation"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionInspectPathRelation.Retired, "not in the selected package")]
    [InlineData(ExtensionInspectPathRelation.CurrentDiverged, "workspace differs from the installed baseline")]
    [InlineData(ExtensionInspectPathRelation.New, "new in the selected package")]
    public void RelationWording(object value, string expected)
        => Assert.Equal(expected, ExtensionInspectPathsHumanRenderer.Relation((ExtensionInspectPathRelation)value));
}
