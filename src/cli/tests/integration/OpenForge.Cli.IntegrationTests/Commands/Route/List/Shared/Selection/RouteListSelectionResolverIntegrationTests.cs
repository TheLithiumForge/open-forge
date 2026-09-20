using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.IntegrationTests.Commands.Route.List;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Selection;

public sealed class RouteListSelectionResolverIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection maps an omitted source to the resolved Loader-root selection")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task OmittedSourceMapsLoaderResolutionToLoaderRoots()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader("");

        var result = await ResolveAsync(workspace, null);

        Assert.Equal(RouteListSelectionResolutionState.Resolved, result.State);
        Assert.Equal(RouteListSelectionKind.LoaderRoots, result.Selection.Kind);
        Assert.Empty(result.SelectedSources);
        Assert.Empty(result.Issues);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route-list selection resolves an ID and its exact path to the same physical source"),
        InlineData("root", ".agents/root/_root.md"),
        InlineData("./.agents/root/_root.md", ".agents/root/_root.md")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task IdAndExactPathHaveParity(string firstReference, string expectedCanonicalPath)
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.Write(".agents/root/_root.md", "root");
        var source = workspace.Source(
            "root",
            expectedCanonicalPath,
            RouteListSourceKind.Entrypoint);
        var result = await ResolveAsync(workspace, firstReference);

        Assert.Equal(RouteListSelectionResolutionState.Resolved, result.State);
        AssertSelectedSource(result, source);
        Assert.Equal("root", result.Selection.ResolvedId);
        Assert.Equal(expectedCanonicalPath, result.Selection.ResolvedPath);
        Assert.Empty(result.Issues);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection preserves an unknown ID as attempted identity")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnknownIdRetainsOnlyAttemptedId()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();

        var result = await ResolveAsync(
            workspace,
            "unknown/source");

        Assert.Equal(RouteListSelectionResolutionState.Invalid, result.State);
        Assert.Equal(RouteListSelectionKind.SourceId, result.Selection.Kind);
        Assert.Equal("unknown/source", result.Selection.AttemptedId);
        Assert.Null(result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Null(result.Selection.ResolvedPath);
        Assert.Empty(result.SelectedSources);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.UnknownSource, issue.Code);
        Assert.Equal("unknown/source", issue.Subject);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection preserves a missing exact path as attempted canonical identity")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MissingExactPathRetainsOnlyAttemptedPath()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        const string expectedPath = ".agents/root/missing.md";

        var result = await ResolveAsync(
            workspace,
            "./.agents/root/missing.md");

        Assert.Equal(RouteListSelectionResolutionState.Invalid, result.State);
        Assert.Equal(RouteListSelectionKind.SourcePath, result.Selection.Kind);
        Assert.Null(result.Selection.AttemptedId);
        Assert.Equal(expectedPath, result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Null(result.Selection.ResolvedPath);
        Assert.Empty(result.SelectedSources);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.UnknownSource, issue.Code);
        Assert.Equal(expectedPath, issue.Subject);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route-list selection rejects malformed ID and path grammar while retaining interpreted input"),
        InlineData("root/../child", "root/../child", null),
        InlineData("./.agents/root/../child.md", null, ".agents/root/../child.md")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MalformedReferenceRetainsItsInterpretedAttempt(
        string sourceReference,
        string? attemptedId,
        string? attemptedPath)
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();

        var result = await ResolveAsync(
            workspace,
            sourceReference);

        Assert.Equal(RouteListSelectionResolutionState.Invalid, result.State);
        Assert.Equal(attemptedId, result.Selection.AttemptedId);
        Assert.Equal(attemptedPath, result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Null(result.Selection.ResolvedPath);
        Assert.Empty(result.SelectedSources);
        Assert.Equal(RouteListFindingCode.InvalidSourceReference, Assert.Single(result.Issues).Code);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection blocks an ID collision with every ordinal candidate")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task AmbiguousIdRetainsSortedCandidates()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.Write(".agents/root/collision.md", "leaf");
        workspace.Write(".agents/root/collision/_collision.md", "entrypoint");
        var leaf = workspace.Source("root/collision", ".agents/root/collision.md");
        var entrypoint = workspace.Source(
            "root/collision",
            ".agents/root/collision/_collision.md",
            RouteListSourceKind.Entrypoint);

        var result = await ResolveAsync(
            workspace,
            "root/collision");

        Assert.Equal(RouteListSelectionResolutionState.Blocked, result.State);
        Assert.Equal("root/collision", result.Selection.AttemptedId);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Empty(result.SelectedSources);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.AmbiguousSource, issue.Code);
        Assert.Equal(
            [leaf.CanonicalPath, entrypoint.CanonicalPath],
            issue.CandidatePaths);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection resolves base ID, base path, and overwrite path to one source")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task BaseAndOverwriteReferencesHaveParity()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Guidance](guidance/_guidance.md) - #Guidance");
        workspace.Write(".agents/guidance/_guidance.md", "guidance");
        workspace.Write(
            ".agents/guidance/style.md",
            "---\nopen-forge:\n  description: Style guidance\n  tags: [Route]\n---\nbase");
        workspace.Write(".agents/guidance/style.overwrite.md", "overwrite");
        var source = workspace.Source(
            "guidance/style",
            ".agents/guidance/style.md",
            RouteListSourceKind.RoutedLeaf,
            overwritePath: ".agents/guidance/style.overwrite.md");

        var idResult = await ResolveAsync(workspace, "guidance/style");
        var baseResult = await ResolveAsync(workspace, ".agents/guidance/style.md");
        var overwriteResult = await ResolveAsync(
            workspace,
            "./.agents/guidance/style.overwrite.md");

        AssertResolvedLogicalSource(idResult, source, "guidance/style", null);
        AssertResolvedLogicalSource(baseResult, source, null, ".agents/guidance/style.md");
        AssertResolvedLogicalSource(
            overwriteResult,
            source,
            null,
            ".agents/guidance/style.overwrite.md");
        Assert.Equal(".agents/guidance/style.md", overwriteResult.Selection.ResolvedPath);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection rejects an orphan overwrite even when catalogued as an alias")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task OrphanOverwriteIsInvalid()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.Write(".agents/guidance/style.overwrite.md", "orphan overwrite");
        var result = await ResolveAsync(
            workspace,
            ".agents/guidance/style.overwrite.md");

        Assert.Equal(RouteListSelectionResolutionState.Invalid, result.State);
        Assert.Equal(".agents/guidance/style.overwrite.md", result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Equal(RouteListFindingCode.UnsupportedSource, Assert.Single(result.Issues).Code);
        Assert.Empty(result.SelectedSources);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection accepts an explicitly selected detached entrypoint")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task DetachedEntrypointResolvesWithoutLoaderExposure()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.Write(".agents/detached/_detached.md", "detached");
        var source = workspace.Source(
            "detached",
            ".agents/detached/_detached.md",
            RouteListSourceKind.Entrypoint);

        var result = await ResolveAsync(
            workspace,
            ".agents/detached/_detached.md");

        Assert.Equal(RouteListSelectionResolutionState.Resolved, result.State);
        AssertSelectedSource(result, source);
        Assert.Equal("detached", result.Selection.ResolvedId);
        Assert.Equal(".agents/detached/_detached.md", result.Selection.ResolvedPath);
        Assert.Empty(result.Issues);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection rejects the Loader as an explicit subject")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExplicitLoaderIsInvalid()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader("");
        var loader = workspace.Source(
            "loader",
            ".agents/loader.md",
            RouteListSourceKind.Loader);

        var result = await ResolveAsync(workspace, loader.Id);

        Assert.Equal(RouteListSelectionResolutionState.Invalid, result.State);
        Assert.Equal(loader.Id, result.Selection.AttemptedId);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Null(result.Selection.ResolvedPath);
        Assert.Empty(result.SelectedSources);
        Assert.Equal(RouteListFindingCode.LoaderSubject, Assert.Single(result.Issues).Code);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection rejects an unrouted source")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnroutedSourceIsInvalid()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.Write(".agents/flat.md", "unrouted");
        var source = workspace.Source(
            "flat",
            ".agents/flat.md",
            RouteListSourceKind.Unrouted);

        var result = await ResolveAsync(
            workspace,
            ".agents/flat.md");

        Assert.Equal(RouteListSelectionResolutionState.Invalid, result.State);
        Assert.Equal(source.CanonicalPath, result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Equal(RouteListFindingCode.UnsupportedSource, Assert.Single(result.Issues).Code);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection blocks a structurally ambiguous route")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task AmbiguousRouteIsBlocked()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/ambiguous/_ambiguous.md", "ambiguous");
        workspace.Write(".agents/ambiguous/index.md", "ambiguous compatibility entrypoint");
        var source = workspace.Source(
            "ambiguous",
            ".agents/ambiguous/_ambiguous.md",
            RouteListSourceKind.Entrypoint,
            isRouteAmbiguous: true);

        var result = await ResolveAsync(
            workspace,
            ".agents/ambiguous/_ambiguous.md");

        Assert.Equal(RouteListSelectionResolutionState.Blocked, result.State);
        Assert.Equal(source.CanonicalPath, result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Empty(result.SelectedSources);
        Assert.Equal(RouteListFindingCode.RouteAmbiguous, Assert.Single(result.Issues).Code);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection accepts a contained physical file alias under its logical path")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ContainedFileAliasResolvesWithLogicalIdentity()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.Write("contained/root.md", "contained");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/root/_root.md",
                workspace.Absolute("contained/root.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        var source = workspace.Source(
            "root",
            ".agents/root/_root.md",
            RouteListSourceKind.Entrypoint,
            physicalRelativePath: "contained/root.md");
        var before = workspace.SnapshotHashes();

        var result = await ResolveAsync(
            workspace,
            ".agents/root/_root.md");

        Assert.Equal(RouteListSelectionResolutionState.Resolved, result.State);
        AssertSelectedSource(result, source);
        Assert.Equal(".agents/root/_root.md", result.Selection.ResolvedPath);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection blocks an external physical alias without reading outside")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExternalFileAliasIsBlocked()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-selection-external-file");
        var outsideFile = outside.CreateFile("outside.md", "outside");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/root/escape.md",
                outsideFile,
                out _),
            "This integration case requires real symbolic-link support.");
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var result = await ResolveAsync(
            workspace,
            ".agents/root/escape.md");

        Assert.Equal(RouteListSelectionResolutionState.Blocked, result.State);
        Assert.Equal(".agents/root/escape.md", result.Selection.AttemptedPath);
        Assert.Null(result.Selection.ResolvedId);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, Assert.Single(result.Issues).Code);
        Assert.Empty(result.SelectedSources);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route-list selection blocks the first external transition before a later reentry")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExternalThenReentryIsBlockedAtFirstTransition()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-selection-external-reentry");
        workspace.Write("inside/_back.md", "inside");
        var insideDirectory = workspace.Absolute("inside");
        Assert.True(
            outside.TryCreateDirectorySymbolicLink(
                "back",
                insideDirectory,
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(
                ".agents/root/return",
                outside.Path,
                out _),
            "This integration case requires real symbolic-link support.");
        const string logicalPath = ".agents/root/return/back/_back.md";
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var first = await ResolveAsync(workspace, logicalPath);
        var second = await ResolveAsync(workspace, logicalPath);

        AssertResolutionEquivalent(first, second);
        Assert.Equal(RouteListSelectionResolutionState.Blocked, first.State);
        Assert.Equal(logicalPath, first.Selection.AttemptedPath);
        Assert.Null(first.Selection.ResolvedId);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, Assert.Single(first.Issues).Code);
        Assert.Empty(first.SelectedSources);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    private static async Task<RouteListSelectionResolution> ResolveAsync(
        RouteListSelectionIntegrationWorkspace workspace,
        string? sourceReference)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundary = await workspace.BoundaryAsync(cancellationToken);
        var result = await new RouteListSelectionResolver(new PhysicalPathResolver())
            .ResolveAsync(
                workspace.RouteRequest(sourceReference),
                boundary.Catalogue,
                boundary.ProjectionSet,
                boundary.RouteFacts,
                cancellationToken);
        Assert.All(result.SelectedSources, selected =>
            Assert.Same(selected, boundary.ProjectionSet.FindByPath(selected.CanonicalPath)));
        return result;
    }

    private static void AssertResolvedLogicalSource(
        RouteListSelectionResolution result,
        RouteSource source,
        string? attemptedId,
        string? attemptedPath)
    {
        Assert.Equal(RouteListSelectionResolutionState.Resolved, result.State);
        AssertSelectedSource(result, source);
        Assert.Equal(attemptedId, result.Selection.AttemptedId);
        Assert.Equal(attemptedPath, result.Selection.AttemptedPath);
        Assert.Equal(source.Id, result.Selection.ResolvedId);
        Assert.Equal(source.CanonicalPath, result.Selection.ResolvedPath);
        Assert.Empty(result.Issues);
    }

    private static void AssertSelectedSource(
        RouteListSelectionResolution result,
        RouteSource expected)
    {
        var selected = Assert.Single(result.SelectedSources);
        Assert.Equal(expected.Id, selected.Id);
        Assert.Equal(expected.CanonicalPath, selected.CanonicalPath);
    }

    private static void AssertResolutionEquivalent(
        RouteListSelectionResolution first,
        RouteListSelectionResolution second)
    {
        Assert.Equal(first.State, second.State);
        Assert.Equal(first.Selection.Kind, second.Selection.Kind);
        Assert.Equal(first.Selection.AttemptedId, second.Selection.AttemptedId);
        Assert.Equal(first.Selection.AttemptedPath, second.Selection.AttemptedPath);
        Assert.Equal(first.Selection.ResolvedId, second.Selection.ResolvedId);
        Assert.Equal(first.Selection.ResolvedPath, second.Selection.ResolvedPath);
        Assert.Equal(
            first.SelectedSources.Select(source => source.CanonicalPath),
            second.SelectedSources.Select(source => source.CanonicalPath));
        Assert.Equal(
            first.SelectedSources.Select(source => source.Id),
            second.SelectedSources.Select(source => source.Id));
        Assert.Equal(first.Issues.Count, second.Issues.Count);
        for (var index = 0; index < first.Issues.Count; index++)
        {
            var firstIssue = first.Issues[index];
            var secondIssue = second.Issues[index];
            Assert.Equal(firstIssue.Code, secondIssue.Code);
            Assert.Equal(firstIssue.Subject, secondIssue.Subject);
            Assert.Equal(firstIssue.Cause, secondIssue.Cause);
            Assert.Equal(firstIssue.CandidatePaths, secondIssue.CandidatePaths);
        }
    }
}
