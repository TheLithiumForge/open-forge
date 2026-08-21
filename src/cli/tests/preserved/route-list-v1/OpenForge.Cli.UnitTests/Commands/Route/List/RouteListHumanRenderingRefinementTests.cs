using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Commands.Route.List.Rendering;
using OpenForge.Cli.Commands.Route.List.Topology;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.UnitTests.Commands.Route.List;

public sealed class RouteListHumanRenderingRefinementTests
{
    [Fact(DisplayName = "Route-list expanded human output exposes complete selection depth coverage finding hierarchy and provenance facts"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void ExpandedOutputContainsCompleteTypedFacts()
    {
        var output = Render(CliView.Expanded);

        AssertWorkspaceSelection(output, "C:/workspace");
        AssertSelectedSource(output, "root", ".agents/root/_root.md");
        AssertSemanticValueLine(output, "all", "requested", "depth");
        AssertExplainedEffectiveDepth(output, "1");
        AssertCoverageBoundary(output, "incomplete", "metadata read");
        AssertFindingFacts(
            output,
            RouteListFindingCodes.SourceReadFailed,
            "Direct read cause.",
            ".agents/root/broken.md");
        AssertParentFacts(output, "root", ".agents/root/_root.md");
        AssertSemanticValueLine(output, "1", "directchildren");
        AssertProvenanceLine(output, "Loader root", "authored topology");
        AssertProvenanceLine(output, "direct child of root", "authored topology");
    }

    [Fact(DisplayName = "Route-list compact human output keeps selected identity and finding location in dense records"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void CompactOutputRetainsDenseIdentityAndFindings()
    {
        var output = Render(CliView.Compact);
        var lines = output.Split('\n', StringSplitOptions.None);

        AssertSelectedSource(output, "root", ".agents/root/_root.md");
        Assert.Contains(lines, line => line.Contains(RouteListFindingCodes.SourceReadFailed, StringComparison.Ordinal)
            && line.Contains(".agents/root/broken.md", StringComparison.Ordinal));
        var rootRow = FindRowLineIndex(lines, ".agents/root/_root.md", "Root");
        var childRow = FindRowLineIndex(lines, ".agents/root/child.md", "Child");
        Assert.True(rootRow >= 0);
        Assert.True(childRow >= 0);
        Assert.True(rootRow < childRow);
    }

    [Theory(DisplayName = "Route-list human views escape controls preserve parent-before-child order and emit no Markdown framing"),
     InlineData((int)CliView.Compact),
     InlineData((int)CliView.Expanded),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void ViewsAreSafeOrderedPlainText(int viewValue)
    {
        var output = Render((CliView)viewValue);

        Assert.DoesNotContain("\0", output, StringComparison.Ordinal);
        Assert.Contains("Child\\u0007", output, StringComparison.Ordinal);
        Assert.True(
            output.IndexOf(".agents/root/_root.md", StringComparison.Ordinal)
            < output.IndexOf(".agents/root/child.md", StringComparison.Ordinal));
        Assert.DoesNotContain("```", output, StringComparison.Ordinal);
        Assert.DoesNotContain("**", output, StringComparison.Ordinal);
        Assert.DoesNotContain("# ", output, StringComparison.Ordinal);
    }

    private static string Render(CliView view)
    {
        var result = Result();
        return RouteListHumanRenderer.Render(new CliPresentationMessage<RouteListResult>(
            result.Status,
            result,
            new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal)));
    }

    private static void AssertSemanticLine(string output, params string[] semanticFragments)
    {
        Assert.Contains(
            output.Split('\n', StringSplitOptions.None),
            line => HasSemanticFragments(line, semanticFragments));
    }

    private static void AssertSemanticValueLine(
        string output,
        string exactValue,
        params string[] semanticFragments)
    {
        Assert.Contains(
            output.Split('\n', StringSplitOptions.None),
            line => HasSemanticFragments(line, semanticFragments)
                && ContainsStandaloneValue(line, exactValue));
    }

    private static void AssertWorkspaceSelection(string output, string workspacePath)
    {
        AssertSemanticValueLine(output, workspacePath, "workspace");
        Assert.Contains(
            output.Split('\n', StringSplitOptions.None),
            line => HasSemanticFragments(line, "selectedby")
                && (line.Contains("--workspace", StringComparison.Ordinal)
                    || line.Contains("explicitWorkspace", StringComparison.Ordinal)));
    }

    private static void AssertExplainedEffectiveDepth(string output, string exactDepth)
    {
        var lines = output.Split('\n', StringSplitOptions.None);
        var depthLine = Array.FindIndex(
            lines,
            line => HasSemanticFragments(line, "effective", "depth")
                && ContainsStandaloneValue(line, exactDepth));
        Assert.True(depthLine >= 0);

        Assert.Contains(
            lines,
            line => (HasSemanticFragments(line, "explanation")
                    || HasSemanticFragments(line, "depth"))
                && new[] { "root", "child", "descendant", "closure" }
                    .Any(explanation => HasSemanticFragments(line, explanation)));
    }

    private static void AssertCoverageBoundary(
        string output,
        string coverageState,
        string boundary)
    {
        var lines = output.Split('\n', StringSplitOptions.None);
        var coverageLine = Array.FindIndex(
            lines,
            line => HasSemanticFragments(line, "coverage")
                && line.Contains(coverageState, StringComparison.Ordinal));
        Assert.True(coverageLine >= 0);

        Assert.Contains(boundary, output, StringComparison.Ordinal);
    }

    private static void AssertSelectedSource(string output, string sourceId, string sourcePath)
    {
        var lines = output.Split('\n', StringSplitOptions.None);
        if (lines.Any(line => IsSelectionLine(line)
                && ContainsStandaloneValue(line, sourceId)
                && line.Contains(sourcePath, StringComparison.Ordinal)))
        {
            return;
        }

        var selectedId = Array.FindIndex(
            lines,
            line => IsSelectionLine(line)
                && HasSemanticFragments(line, "id")
                && ContainsStandaloneValue(line, sourceId));
        var selectedPath = Array.FindIndex(
            lines,
            line => IsSelectionLine(line)
                && HasSemanticFragments(line, "path")
                && line.Contains(sourcePath, StringComparison.Ordinal));
        if (selectedId >= 0 && selectedPath >= 0)
        {
            return;
        }

        var heading = Array.FindIndex(lines, IsSelectionLine);
        Assert.True(heading >= 0);
        var blockEnd = Math.Min(lines.Length, heading + 4);
        var block = lines[heading..blockEnd];
        Assert.Contains(block, line => HasSemanticFragments(line, "id")
            && ContainsStandaloneValue(line, sourceId));
        Assert.Contains(block, line => HasSemanticFragments(line, "path")
            && line.Contains(sourcePath, StringComparison.Ordinal));
    }

    private static void AssertFindingFacts(
        string output,
        string findingCode,
        string findingMessage,
        string findingPath)
    {
        Assert.Contains(findingCode, output, StringComparison.Ordinal);
        Assert.Contains(findingMessage, output, StringComparison.Ordinal);
        Assert.Contains(findingPath, output, StringComparison.Ordinal);
    }

    private static void AssertParentFacts(string output, string parentId, string parentPath)
    {
        var lines = output.Split('\n', StringSplitOptions.None);
        Assert.Contains(lines, line => HasSemanticFragments(line, "parent")
            && ContainsStandaloneValue(line, parentId));
        Assert.Contains(lines, line => HasSemanticFragments(line, "parent")
            && line.Contains(parentPath, StringComparison.Ordinal));
    }

    private static void AssertProvenanceLine(string output, params string[] exactValues)
    {
        Assert.Contains(
            output.Split('\n', StringSplitOptions.None),
            line => HasSemanticFragments(line, "provenance")
                && exactValues.All(value => line.Contains(value, StringComparison.Ordinal)));
    }

    private static int FindRowLineIndex(
        IReadOnlyList<string> lines,
        string exactPath,
        string exactDescription)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            if (HasSemanticFragments(lines[index], "description")
                && lines[index].Contains(exactPath, StringComparison.Ordinal)
                && lines[index].Contains(exactDescription, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool HasSemanticFragments(string line, params string[] semanticFragments)
    {
        var normalizedLine = NormalizeSemanticText(line);
        return semanticFragments.All(fragment => normalizedLine.Contains(
            NormalizeSemanticText(fragment),
            StringComparison.Ordinal));
    }

    private static bool IsSelectionLine(string line)
    {
        return HasSemanticFragments(line, "select");
    }

    private static bool ContainsStandaloneValue(string line, string value)
    {
        var start = line.IndexOf(value, StringComparison.Ordinal);
        while (start >= 0)
        {
            var beforeIsBoundary = start == 0 || IsValueBoundary(line[start - 1]);
            var end = start + value.Length;
            var afterIsBoundary = end == line.Length || IsValueBoundary(line[end]);
            if (beforeIsBoundary && afterIsBoundary)
            {
                return true;
            }

            start = line.IndexOf(value, start + 1, StringComparison.Ordinal);
        }

        return false;
    }

    private static bool IsValueBoundary(char character)
    {
        return char.IsWhiteSpace(character) || character is '=' or ':' or '(' or ')' or '[' or ']' or ',' or ';' or '"' or '\'';
    }

    private static string NormalizeSemanticText(string value)
    {
        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static RouteListResult Result()
    {
        var root = new RouteListRow(
            "root",
            ".agents/root/_root.md",
            null,
            null,
            0,
            0,
            RouteListRowKind.Entrypoint,
            "Root",
            ["Root"],
            1,
            ["Loader root", "authored topology"]);
        var child = new RouteListRow(
            "root/child",
            ".agents/root/child.md",
            "root",
            ".agents/root/_root.md",
            1,
            1,
            RouteListRowKind.RoutedLeaf,
            "Child\a",
            ["Leaf"],
            null,
            ["direct child of root", "authored topology"]);
        return new RouteListResult(
            RouteListDefinitions.SchemaVersion,
            RouteListDefinitions.ResultCommand,
            CliSemanticStatus.Incomplete,
            new CliWorkspace("C:/workspace", CliWorkspaceSelection.ExplicitWorkspace),
            new RouteListPayload(
                RouteListSelectionFactory.ResolvedSource("root", ".agents/root/_root.md"),
                RouteListDepth.All,
                RouteListDepth.Bounded(1),
                new RouteListCoverage(RouteListCoverageState.Incomplete, "metadata read"),
                [new RouteListFinding(RouteListFindingCodes.SourceReadFailed, "Direct read cause.", ".agents/root/broken.md")],
                [root, child]),
            new CliNextAction(RouteListDefinitions.ListCommandPath, "Repair the selected source and rerun."));
    }
}
