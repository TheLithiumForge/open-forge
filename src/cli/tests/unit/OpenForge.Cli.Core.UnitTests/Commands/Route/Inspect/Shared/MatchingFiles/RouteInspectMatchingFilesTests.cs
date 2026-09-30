using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.MatchingFiles;

[Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteInspectMatchingFilesTests
{
    [Theory(DisplayName = "Skipped ignore lines retain complete counts and compose the zero-match note")]
    [InlineData(false), InlineData(true)]
    public void SkippedIgnoreNote(bool zero)
    {
        var result = RouteInspectMatchingFiles.Success(RouteInspectMatchingFilesScope.WorkspaceFiles,
            zero ? [] : ["docs/a.md"], 2);
        Assert.True(result.Complete);
        Assert.Null(result.Reason);
        Assert.Equal(zero ? 0 : 1, result.Count);
        Assert.Equal((zero ? RouteInspectMatchingFiles.NoMatchesNote + " " : "") + "Skipped unsupported .gitignore lines: 2.", result.Note);
    }

    [Theory(DisplayName = "Absent conditions return the complete unmeasured all-files answer without enumeration")]
    [InlineData(false), InlineData(true)]
    public async Task NoConditionsFence(bool includeAbsentDeclaration)
    {
        SourceApplyToCondition[] conditions = includeAbsentDeclaration
            ? [new("source.md", ApplyToMetadataFacts.Absent)]
            : [];
        var calls = 0;
        var scanner = new RouteInspectMatchingFilesScanner((_, _) =>
        {
            calls++;
            throw new InvalidOperationException("Must not enumerate.");
        });
        var result = await scanner.ScanAsync("unused", SourceApplicabilityEvaluator.Evaluate(conditions, []), TestContext.Current.CancellationToken);
        Assert.Equal(0, calls);
        AssertAllFiles(result);
    }

    [Theory(DisplayName = "Match-all patterns fence enumeration while restrictive patterns request it")]
    [InlineData("**", true), InlineData("**/*", true), InlineData("**/**", true)]
    [InlineData("*/**", true), InlineData("{**,src/**}", true)]
    [InlineData("*", false), InlineData("*/*", false), InlineData("**/*/*", false), InlineData("src/**", false)]
    public async Task RecognizesMatchAllPatterns(string text, bool matchAll)
    {
        var calls = 0;
        var scanner = new RouteInspectMatchingFilesScanner((_, _) =>
        {
            calls++;
            return ValueTask.FromResult(RouteInspectFileEnumeration.Unavailable(
                RouteInspectMatchingFilesScope.GitTrackedAndUntracked, RouteInspectMatchingFilesReason.GitUnavailable));
        });
        var pattern = ApplyToPatternMatcher.Parse(text).Pattern ?? throw new InvalidOperationException("Invalid test pattern.");
        var conditions = new[] { Condition(pattern) };
        var applicability = SourceApplicabilityEvaluator.Evaluate(conditions, []);
        var result = await scanner.ScanAsync("unused", applicability, TestContext.Current.CancellationToken);
        Assert.Equal(matchAll ? 0 : 1, calls);
        Assert.Equal(conditions, applicability.Conditions);
        if (matchAll)
        {
            AssertAllFiles(result);
        }
        else
        {
            Assert.Equal(RouteInspectMatchingFilesReason.GitUnavailable, result.Reason);
        }
    }

    [Fact(DisplayName = "Match-all recognition falls back to segments when expanded alternatives are absent")]
    public async Task UsesSegmentsWithoutAlternatives()
    {
        var scanner = new RouteInspectMatchingFilesScanner((_, _) => throw new InvalidOperationException("Must not enumerate."));
        var conditions = new[] { Condition(new ApplyToPattern("**/*", ["**", "*"])) };
        var result = await scanner.ScanAsync("unused", SourceApplicabilityEvaluator.Evaluate(conditions, []), TestContext.Current.CancellationToken);
        AssertAllFiles(result);
    }

    private static SourceApplyToCondition Condition(ApplyToPattern pattern)
        => new("source.md", ApplyToMetadataFacts.Valid([pattern],
            [new ApplyToDeclaration(ApplyToMetadataLocation.Root, new YamlTextSpan(0, 1), new YamlTextSpan(2, 1), [pattern])]));

    private static void AssertAllFiles(RouteInspectMatchingFiles result)
    {
        Assert.Equal(RouteInspectMatchingFilesScope.AllFiles, result.Scope);
        Assert.True(result.Complete);
        Assert.Null(result.Count);
        Assert.Empty(result.Paths);
        Assert.Equal(100, result.PathLimit);
        Assert.False(result.Truncated);
        Assert.Null(result.Reason);
        Assert.Equal("No effective applyTo restriction. Every file applies. No scan was run.", result.Note);
    }

    [Theory(DisplayName = "Matching files counts distinct ordinal paths before applying the fixed cap")]
    [InlineData(0), InlineData(100), InlineData(143)]
    public void CountsBeforeCapping(int count)
    {
        var paths = Enumerable.Range(0, count).Select(index => $"docs/{index:D3}.md").Reverse().ToArray();
        var result = RouteInspectMatchingFiles.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, paths.Concat(paths));

        Assert.True(result.Complete);
        Assert.Equal(count, result.Count);
        Assert.Equal(100, result.PathLimit);
        Assert.Equal(count > 100, result.Truncated);
        Assert.Equal(paths.Order(StringComparer.Ordinal).Take(100), result.Paths);
        Assert.Null(result.Reason);
        Assert.Equal(count == 0, result.Note is not null);
    }

    [Theory(DisplayName = "Result formation checks cancellation while counting beyond the cap and after the last path")]
    [InlineData(false), InlineData(true)]
    public void ResultFormationCancellation(bool afterLastPath)
    {
        using var cancellation = new CancellationTokenSource();
        var reached = 0;
        Assert.Throws<OperationCanceledException>(() => RouteInspectMatchingFiles.Materialize(
            RouteInspectMatchingFilesScope.WorkspaceFiles, Paths(), cancellationToken: cancellation.Token));
        Assert.Equal(RouteInspectMatchingFiles.MaximumPaths + 1, reached);

        IEnumerable<string> Paths()
        {
            for (var index = 0; index <= RouteInspectMatchingFiles.MaximumPaths; index++)
            {
                reached++;
                yield return $"docs/{index:D3}.md";
            }

            cancellation.Cancel();
            if (!afterLastPath)
            {
                yield return "docs/000.md";
                throw new InvalidOperationException("Cancelled result formation must not continue through duplicates.");
            }
        }
    }

    [Theory(DisplayName = "Unavailable matching files never expose a partial count or path set")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    [InlineData(4), InlineData(5), InlineData(6), InlineData(7)]
    public void UnavailableDiscardsPartialFacts(int value)
    {
        var reason = (RouteInspectMatchingFilesReason)value;
        var result = RouteInspectMatchingFiles.Unavailable(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, reason);
        Assert.False(result.Complete);
        Assert.Null(result.Count);
        Assert.Empty(result.Paths);
        Assert.False(result.Truncated);
        Assert.Null(result.Note);
        Assert.Equal(reason, result.Reason);
        var inventory = RouteInspectFileEnumeration.Unavailable(result.Scope, reason);
        Assert.Empty(inventory.Paths);
        Assert.Equal(reason, inventory.Failure);
    }

    [Fact(DisplayName = "An unresolved condition chain prevents enumeration")]
    public async Task MissingChainDoesNotEnumerate()
    {
        var scanner = new RouteInspectMatchingFilesScanner((_, _) => throw new InvalidOperationException("Must not enumerate."));
        var result = await scanner.ScanAsync("unused", null, TestContext.Current.CancellationToken);
        Assert.Equal(RouteInspectMatchingFilesReason.ConditionUnavailable, result.Reason);
        Assert.Null(result.Scope);
    }
}
