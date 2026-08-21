using OpenForge.Cli.EndToEndTests.Process;

namespace OpenForge.Cli.EndToEndTests.Commands.Route.List;

public sealed class RouteListHumanRefinementProcessTests
{
    [Fact(DisplayName = "Published route-list expanded human journey exposes complete workspace selection depth hierarchy and provenance facts"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task ExpandedCompleteJourneyContainsPublicFacts()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var before = workspace.Snapshot();

        var result = await RunCliAsync(
            "route", "list", "root", "--workspace", workspace.Path, "--depth=1", "--view=expanded");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stderr);
        AssertWorkspaceSelection(result.Stdout, workspace.Path);
        AssertSelectedSource(result.Stdout, "root", ".agents/root/_root.md");
        AssertSemanticValueLine(result.Stdout, "1", "requested", "depth");
        AssertExplainedEffectiveDepth(result.Stdout, "1");
        AssertSemanticValueLine(result.Stdout, "complete", "coverage");
        AssertParentFacts(result.Stdout, "root", ".agents/root/_root.md");
        AssertSemanticValueLine(result.Stdout, "1", "directchildren");
        AssertProvenanceLine(result.Stdout, "Loader root", "authored topology");
        AssertProvenanceLine(result.Stdout, "direct child of root", "authored topology");
        Assert.DoesNotContain("```", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Published route-list compact incomplete journey retains selected root and finding location without Markdown"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task CompactIncompleteJourneyContainsPublicFacts()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        workspace.WriteBytes(".agents/root/broken.md", [0xFF, 0xFE]);
        var before = workspace.Snapshot();

        var result = await RunCliAsync(
            "route", "list", "root", "--workspace", workspace.Path, "--depth=all", "--view=compact");

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Stderr);
        AssertSelectedSource(result.Stdout, "root", ".agents/root/_root.md");
        var findingLine = Assert.Single(
            result.Stdout.Split('\n', StringSplitOptions.None),
            line => HasSemanticFragments(
                line,
                "source-read-failed",
                ".agents/root/broken.md"));
        Assert.NotEmpty(findingLine);
        var lines = result.Stdout.Split('\n', StringSplitOptions.None);
        var rootRow = FindRowLineIndex(lines, ".agents/root/_root.md", "Root");
        var childRow = FindRowLineIndex(lines, ".agents/root/child.md", "Child");
        Assert.True(rootRow >= 0);
        Assert.True(childRow >= 0);
        Assert.True(rootRow < childRow);
        Assert.DoesNotContain("```", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("**", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    private static Task<CliProcessResult> RunCliAsync(params string[] arguments)
    {
        var environment = CliEndToEndEnvironment.ReadRequired();
        return CliProcessRunner.RunAsync(
            new CliProcessRequest(environment.ExecutablePath, arguments),
            TestContext.Current.CancellationToken);
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
        AssertSemanticValueLine(output, Escape(workspacePath), "workspace");
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

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal);
    }

    private static string NormalizeSemanticText(string value)
    {
        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}
