using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.MatchingFiles;

[Trait("Feature", "route-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInspectScanDeadlineTests
{
    [Theory(DisplayName = "Matching and final result formation discard all facts on cancellation or deadline expiry")]
    [InlineData(false, false), InlineData(true, false)]
    [InlineData(false, true), InlineData(true, true)]
    public async Task StopsAfterEnumeration(bool cancel, bool materialized)
    {
        using var workspace = TemporaryWorkspace.Create("matching-deadline");
        workspace.CreateFile("a.md");
        workspace.CreateFile("b.md");
        using var cancellation = new CancellationTokenSource();
        var clock = new RouteInspectScanClock();
        var enumerationFinished = false;
        var matchingSteps = 0;
        var observed = false;
        var target = materialized ? RouteInspectScanStage.Materialized : RouteInspectScanStage.Matching;
        var scanner = new RouteInspectMatchingFilesScanner((_, _) =>
        {
            enumerationFinished = true;
            return ValueTask.FromResult(RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.WorkspaceFiles, ["b.md", "a.md"]));
        }, clock, stage =>
        {
            Assert.True(enumerationFinished);
            if (stage == RouteInspectScanStage.Matching)
            {
                matchingSteps++;
            }

            if (stage == target)
            {
                observed = true;
                if (cancel)
                {
                    cancellation.Cancel();
                }
                else
                {
                    clock.Advance(RouteInspectMatchingFiles.ScanDeadline, deliverTimers: false);
                }
            }
        });

        var result = await scanner.ScanAsync(workspace.Path, Applicability(), cancellation.Token);

        Assert.True(observed);
        Assert.Equal(materialized ? 4 : 1, matchingSteps);
        AssertUnavailable(result, cancel, RouteInspectMatchingFilesScope.WorkspaceFiles);
    }

    [Theory(DisplayName = "Ignore parsing and evaluation honor the scan token without a wall-clock race")]
    [InlineData(false, false), InlineData(true, false)]
    [InlineData(false, true), InlineData(true, true)]
    public async Task StopsDuringIgnoreWork(bool cancel, bool evaluating)
    {
        using var cancellation = new CancellationTokenSource();
        var clock = new RouteInspectScanClock();
        var observed = false;
        var scanner = new RouteInspectMatchingFilesScanner((_, token) =>
        {
            if (evaluating)
            {
                var rules = RouteInspectIgnoreRules.Parse("", ["*.log", "!keep.log"], token);
                RouteInspectIgnoreRules.IsIncluded(Interrupt(rules.Rules), "keep.log", false, token);
            }
            else
            {
                RouteInspectIgnoreRules.Parse("", Interrupt(new[] { "*.log", "!keep.log" }), token);
            }

            throw new InvalidOperationException("Ignore work must stop before returning an inventory.");
        }, clock);

        var result = await scanner.ScanAsync("unused", Applicability(), cancellation.Token);

        Assert.True(observed);
        AssertUnavailable(result, cancel, null);

        IEnumerable<T> Interrupt<T>(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                yield return value;
                observed = true;
                if (cancel)
                {
                    cancellation.Cancel();
                }
                else
                {
                    clock.Advance(RouteInspectMatchingFiles.ScanDeadline);
                }
            }
        }
    }

    private static SourceApplicabilityResult Applicability()
    {
        var pattern = ApplyToPatternMatcher.Parse("*.md").Pattern ?? throw new InvalidOperationException();
        var metadata = ApplyToMetadataFacts.Valid([pattern],
            [new ApplyToDeclaration(ApplyToMetadataLocation.Root, new YamlTextSpan(0, 1), new YamlTextSpan(2, 1), [pattern])]);
        return SourceApplicabilityEvaluator.Evaluate([new("parent.md", metadata), new("source.md", metadata)], []);
    }

    private static void AssertUnavailable(RouteInspectMatchingFiles result, bool cancel, RouteInspectMatchingFilesScope? scope)
    {
        Assert.False(result.Complete);
        Assert.Equal(cancel ? RouteInspectMatchingFilesReason.Cancelled : RouteInspectMatchingFilesReason.ScanTimeout, result.Reason);
        Assert.Equal(scope, result.Scope);
        Assert.Null(result.Count);
        Assert.Empty(result.Paths);
        Assert.False(result.Truncated);
        Assert.Null(result.Note);
    }
}
