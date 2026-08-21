using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Selection;

public sealed class RouteListSelectionResolutionFactoryTests
{
    [Fact(DisplayName = "Route-list selection resolution factory forms every outcome state")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FactoryFormsEverySelectionResolutionState()
    {
        var source = Source();
        var resolvedSelection = RouteListSelectionFactory.ResolvedId("memory", source);
        var attemptedId = RouteListSelectionFactory.AttemptedId("missing");
        var loaderRoots = RouteListSelectionFactory.LoaderRoots();
        var invalidIssue = Issue(RouteListFindingCode.UnknownSource, "missing");
        var blockedIssue = new RouteListSelectionIssue(
            RouteListFindingCode.AmbiguousSource,
            "missing",
            "The source ID is ambiguous.",
            [".agents/a.md", ".agents/z.md"]);
        var incompleteIssue = Issue(RouteListFindingCode.LoaderUnavailable, ".agents/loader.md");
        var interruptedIssue = Issue(RouteListFindingCode.Interrupted, ".agents/loader.md");

        var resolved = RouteListSelectionResolutionFactory.Resolved(resolvedSelection, [source]);
        var invalid = RouteListSelectionResolutionFactory.Invalid(attemptedId, invalidIssue);
        var blocked = RouteListSelectionResolutionFactory.Blocked(attemptedId, [], [blockedIssue]);
        var incomplete = RouteListSelectionResolutionFactory.Incomplete(loaderRoots, [source], [incompleteIssue]);
        var interrupted = RouteListSelectionResolutionFactory.Interrupted(loaderRoots, [], [interruptedIssue]);
        var blockedWithIncomplete = RouteListSelectionResolutionFactory.Blocked(
            loaderRoots,
            [source],
            [blockedIssue, incompleteIssue]);
        var interruptedWithBlocked = RouteListSelectionResolutionFactory.Interrupted(
            loaderRoots,
            [source],
            [blockedIssue, incompleteIssue, interruptedIssue]);

        Assert.Equal(RouteListSelectionResolutionState.Resolved, resolved.State);
        Assert.Same(source, Assert.Single(resolved.SelectedSources));
        Assert.Equal(RouteListSelectionResolutionState.Invalid, invalid.State);
        Assert.Same(invalidIssue, Assert.Single(invalid.Issues));
        Assert.Equal(RouteListSelectionResolutionState.Blocked, blocked.State);
        Assert.Equal([".agents/a.md", ".agents/z.md"], Assert.Single(blocked.Issues).CandidatePaths);
        Assert.Equal(RouteListSelectionResolutionState.Incomplete, incomplete.State);
        Assert.Same(source, Assert.Single(incomplete.SelectedSources));
        Assert.Equal(RouteListSelectionResolutionState.Interrupted, interrupted.State);
        Assert.Equal(
            [CliSemanticStatus.Blocked, CliSemanticStatus.Incomplete],
            blockedWithIncomplete.Issues.Select(issue => issue.Status));
        Assert.Equal(
            [CliSemanticStatus.Blocked, CliSemanticStatus.Incomplete, CliSemanticStatus.Interrupted],
            interruptedWithBlocked.Issues.Select(issue => issue.Status));
    }

    [Fact(DisplayName = "Route-list selection resolution factory snapshots sources issues and candidates")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResolutionCollectionsAreImmutableSnapshots()
    {
        var source = Source();
        var candidatePaths = new[] { ".agents/a.md", ".agents/z.md" };
        var ambiguousIssue = new RouteListSelectionIssue(
            RouteListFindingCode.AmbiguousSource,
            "memory",
            "The source ID is ambiguous.",
            candidatePaths);
        var issue = Issue(RouteListFindingCode.LoaderUnavailable, ".agents/loader.md");
        var sources = new[] { source };
        var issues = new[] { issue };
        var result = RouteListSelectionResolutionFactory.Incomplete(
            RouteListSelectionFactory.LoaderRoots(),
            sources,
            issues);

        sources[0] = new RouteListSource(
            "other",
            ".agents/other.md",
            Path.Combine(Path.GetTempPath(), "other.md"),
            RouteListSourceKind.RoutedLeaf);
        issues[0] = Issue(RouteListFindingCode.RouteAmbiguous, "other");
        candidatePaths[0] = ".agents/changed.md";

        Assert.Same(source, Assert.Single(result.SelectedSources));
        var retainedIssue = Assert.Single(result.Issues);
        Assert.Same(issue, retainedIssue);
        Assert.Equal([".agents/a.md", ".agents/z.md"], ambiguousIssue.CandidatePaths);
    }

    [Fact(DisplayName = "Route-list selection resolution factory rejects inconsistent outcomes")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ResolutionRejectsInconsistentState()
    {
        var source = Source();
        var attempted = RouteListSelectionFactory.AttemptedId("memory");
        var resolved = RouteListSelectionFactory.ResolvedId("memory", source);

        Assert.Throws<ArgumentException>(() => RouteListSelectionResolutionFactory.Resolved(attempted, [source]));
        Assert.Throws<ArgumentException>(() => RouteListSelectionResolutionFactory.Resolved(resolved, []));
        Assert.Throws<ArgumentException>(() => RouteListSelectionResolutionFactory.Invalid(
            attempted,
            Issue(RouteListFindingCode.PhysicalBoundary, "memory")));
        Assert.Throws<ArgumentException>(() => RouteListSelectionResolutionFactory.Blocked(
            attempted,
            [],
            [Issue(RouteListFindingCode.UnknownSource, "memory")]));
        Assert.Throws<ArgumentException>(() => RouteListSelectionResolutionFactory.Incomplete(
            RouteListSelectionFactory.LoaderRoots(),
            [],
            []));
        Assert.Throws<ArgumentException>(() => new RouteListSelectionIssue(
            RouteListFindingCode.AmbiguousSource,
            "memory",
            "The source ID is ambiguous."));
    }

    private static RouteListSource Source()
    {
        return new RouteListSource(
            "memory",
            ".agents/memory/_memory.md",
            Path.Combine(Path.GetTempPath(), "memory", "_memory.md"),
            RouteListSourceKind.Entrypoint);
    }

    private static RouteListSelectionIssue Issue(RouteListFindingCode code, string subject)
    {
        return new RouteListSelectionIssue(code, subject, "A bounded selection issue.");
    }
}
