using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.MatchingFiles;

[Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteInspectIgnoreRulesTests
{
    [Theory(DisplayName = "Ignore parsing stops on cancellation between lines and after the final line")]
    [InlineData(false), InlineData(true)]
    public void ParsingCancellation(bool afterLastLine)
    {
        using var cancellation = new CancellationTokenSource();
        var reached = false;
        Assert.Throws<OperationCanceledException>(() => RouteInspectIgnoreRules.Parse("", Lines(), cancellation.Token));
        Assert.True(reached);

        IEnumerable<string> Lines()
        {
            yield return "*.log";
            reached = true;
            cancellation.Cancel();
            if (!afterLastLine)
            {
                yield return "#comment";
                throw new InvalidOperationException("Cancelled parsing must not continue through comments.");
            }
        }
    }

    [Theory(DisplayName = "Ignore evaluation stops on cancellation between rules and after the final rule")]
    [InlineData(false), InlineData(true)]
    public void EvaluationCancellation(bool afterLastRule)
    {
        using var cancellation = new CancellationTokenSource();
        var parsed = RouteInspectIgnoreRules.Parse("", ["*.log", "!keep.log"], cancellation.Token);
        var reached = false;
        Assert.Throws<OperationCanceledException>(() => RouteInspectIgnoreRules.IsIncluded(Rules(), "keep.log", false, cancellation.Token));
        Assert.True(reached);

        IEnumerable<RouteInspectIgnoreRule> Rules()
        {
            yield return parsed.Rules[0];
            reached = true;
            cancellation.Cancel();
            if (!afterLastRule)
            {
                yield return parsed.Rules[1];
                throw new InvalidOperationException("Cancelled evaluation must not continue through rules.");
            }
        }
    }

    [Theory(DisplayName = "Fallback ignore translation preserves literal syntax and shared wildcard semantics")]
    [InlineData("*.log", "nested/a.log", false, false)]
    [InlineData("/a.log", "nested/a.log", false, true)]
    [InlineData("docs/", "docs", false, true)]
    [InlineData("docs/", "docs", true, false)]
    [InlineData("docs/**", "docs", true, true)]
    [InlineData("docs/**", "docs/a.md", false, false)]
    [InlineData("{a,b}.md", "{a,b}.md", false, false)]
    [InlineData("{a,b}.md", "a.md", false, true)]
    [InlineData("a,b.md", "a,b.md", false, false)]
    [InlineData("\\#file", "#file", false, false)]
    [InlineData("\\!file", "!file", false, false)]
    [InlineData("a\\*.md", "a*.md", false, false)]
    [InlineData("a\\*.md", "abc.md", false, true)]
    [InlineData("a\\?.md", "a?.md", false, false)]
    [InlineData("a\\?.md", "ab.md", false, true)]
    [InlineData("\\[a\\]", "[a]", false, false)]
    [InlineData(" leading", " leading", false, false)]
    [InlineData("trailing  ", "trailing", false, false)]
    [InlineData("trailing\\ ", "trailing ", false, false)]
    [InlineData("[!a-c]?.md", "z1.md", false, false)]
    [InlineData("[!!]{a,b}", "x{a,b}", false, false)]
    [InlineData("*.MD", "a.md", false, true)]
    public void Translation(string line, string path, bool directory, bool included)
    {
        var parsed = RouteInspectIgnoreRules.Parse("", [line], TestContext.Current.CancellationToken);
        Assert.Equal(0, parsed.SkippedLines);
        Assert.Single(parsed.Rules);
        Assert.Equal(included, RouteInspectIgnoreRules.IsIncluded(parsed.Rules, path, directory, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Fallback ignore rules use literal base directories and last matching ancestor or local rule")]
    public void Precedence()
    {
        var token = TestContext.Current.CancellationToken;
        var parent = RouteInspectIgnoreRules.Parse("", ["docs/**", "!docs/keep.md"], token);
        Assert.True(RouteInspectIgnoreRules.IsIncluded(parent.Rules, "docs", true, token));
        Assert.True(RouteInspectIgnoreRules.IsIncluded(parent.Rules, "docs/keep.md", false, token));
        Assert.False(RouteInspectIgnoreRules.IsIncluded(parent.Rules, "docs/drop.md", false, token));
        var local = RouteInspectIgnoreRules.Parse("docs", ["!drop.md", "keep.md"], token);
        var combined = parent.Rules.AddRange(local.Rules);
        Assert.True(RouteInspectIgnoreRules.IsIncluded(combined, "docs/drop.md", false, token));
        Assert.False(RouteInspectIgnoreRules.IsIncluded(combined, "docs/keep.md", false, token));
        var literal = RouteInspectIgnoreRules.Parse("[docs]", ["*.md"], token);
        Assert.False(RouteInspectIgnoreRules.IsIncluded(literal.Rules, "[docs]/a.md", false, token));
        Assert.True(RouteInspectIgnoreRules.IsIncluded(literal.Rules, "d/a.md", false, token));
        var restoredDirectory = RouteInspectIgnoreRules.Parse("", ["docs/", "!docs/"], token);
        Assert.True(RouteInspectIgnoreRules.IsIncluded(restoredDirectory.Rules, "docs", true, token));
    }

    [Theory(DisplayName = "Unsupported fallback ignore lines are counted without reinterpretation")]
    [InlineData("[[:alpha:]]"), InlineData("[[.a.]]"), InlineData("[[=a=]]")]
    [InlineData("[a\\b]"), InlineData("a\\/b"), InlineData("a\\\\b"), InlineData("dangling\\")]
    [InlineData("[unfinished"), InlineData("../outside"), InlineData("!")]
    public void Unsupported(string line)
    {
        var parsed = RouteInspectIgnoreRules.Parse("", ["", "#comment", "   ", line], TestContext.Current.CancellationToken);
        Assert.Equal(1, parsed.SkippedLines);
        Assert.Empty(parsed.Rules);
        Assert.True(RouteInspectIgnoreRules.IsIncluded(parsed.Rules, "anything.md", false, TestContext.Current.CancellationToken));
    }
}
