using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.List;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Loader;

public sealed class LoaderDestinationResolverIntegrationTests
{
    [Theory(DisplayName = "Route-list Loader resolves an empty exact Entries region without inventing roots"),
        InlineData(""),
        InlineData("- none - No entries - #Empty")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EmptyLoaderProducesResolvedEmptyRootSet(string markerBody)
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader(markerBody);

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Resolved, result.State);
        Assert.Empty(result.SelectedSources);
        Assert.Empty(result.Issues);
    }

    [Fact(DisplayName = "Route-list Loader resolves literal, encoded, Unicode, and encoded-percent root destinations")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task CanonicalAndEncodedRootsResolveWithOneDecode()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [Root](root/_root.md) - #Root\n"
            + "- [Alpha](project%20alpha/_project%20alpha.md) - #Alpha\n"
            + "- [Unicode](%E5%B7%A5%E4%BD%9C/_%E5%B7%A5%E4%BD%9C.md) - #Unicode\n"
            + "- [Percent](encoded%2520name/_encoded%2520name.md) - #Percent");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/project alpha/_project alpha.md", "alpha");
        workspace.Write(".agents/工作/_工作.md", "unicode");
        workspace.Write(".agents/encoded%20name/_encoded%20name.md", "percent");
        var sources = new[]
        {
            workspace.Source("root", ".agents/root/_root.md", RouteListSourceKind.Entrypoint),
            workspace.Source(
                "project alpha",
                ".agents/project alpha/_project alpha.md",
                RouteListSourceKind.Entrypoint),
            workspace.Source("工作", ".agents/工作/_工作.md", RouteListSourceKind.Entrypoint),
            workspace.Source(
                "encoded%20name",
                ".agents/encoded%20name/_encoded%20name.md",
                RouteListSourceKind.Entrypoint),
        };

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(sources));

        Assert.Equal(LoaderDestinationResolutionState.Resolved, result.State);
        Assert.Equal(
            [
                ".agents/encoded%20name/_encoded%20name.md",
                ".agents/project alpha/_project alpha.md",
                ".agents/root/_root.md",
                ".agents/工作/_工作.md",
            ],
            result.SelectedSources.Select(source => source.CanonicalPath));
        Assert.Empty(result.Issues);
    }

    [Fact(DisplayName = "Route-list Loader reports malformed percent encoding as incomplete Loader input")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MalformedPercentIsIncomplete()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root%2/_root.md) - #Root");

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.LoaderMalformed, issue.Code);
        Assert.Equal("root%2/_root.md", issue.Subject);
        Assert.Empty(result.SelectedSources);
    }

    [Fact(DisplayName = "Route-list Loader retains safe roots before a later malformed declaration")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task LaterMalformedDeclarationRetainsEarlierSafeRoots()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [Root](root/_root.md) - #Root\n"
            + "- [Malformed](broken%2/_broken.md) - #Broken");
        workspace.Write(".agents/root/_root.md", "root");
        var source = workspace.Source(
            "root",
            ".agents/root/_root.md",
            RouteListSourceKind.Entrypoint);

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(source));

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        Assert.Same(source, Assert.Single(result.SelectedSources));
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.LoaderMalformed, issue.Code);
        Assert.Equal("broken%2/_broken.md", issue.Subject);
    }

    [Fact(DisplayName = "Route-list Loader retains safe roots before a malformed mixed empty sentinel")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MixedEmptySentinelRetainsEarlierSafeRoots()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [Root](root/_root.md) - #Root\n"
            + "- none - No entries - #Empty");
        workspace.Write(".agents/root/_root.md", "root");
        var source = workspace.Source(
            "root",
            ".agents/root/_root.md",
            RouteListSourceKind.Entrypoint);

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(source));

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        Assert.Same(source, Assert.Single(result.SelectedSources));
        Assert.Equal(RouteListFindingCode.LoaderMalformed, Assert.Single(result.Issues).Code);
    }

    [Fact(DisplayName = "Route-list Loader returns blocked with a retained malformed declaration finding")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnsafeThenMalformedDestinationsReturnOneTypedBlockedOutcome()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [Unsafe](root%2F%2F_root.md) - #Root\n"
            + "- [Malformed](broken%2/_broken.md) - #Broken");

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Blocked, result.State);
        Assert.Equal(
            [CliSemanticStatus.Blocked, CliSemanticStatus.Incomplete],
            result.Issues.Select(issue => issue.Status));
        Assert.Equal(
            [RouteListFindingCode.PhysicalBoundary, RouteListFindingCode.LoaderMalformed],
            result.Issues.Select(issue => issue.Code));
    }

    [Theory(DisplayName = "Route-list Loader blocks unsafe decoded destinations at the physical boundary"),
        InlineData("root%2F%2F_root.md"),
        InlineData("root/%2E/_root.md"),
        InlineData("root/%2E%2E/_root.md"),
        InlineData("root%3Fquery/_root.md"),
        InlineData("root%5C_root.md")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnsafeDecodedDestinationIsBlocked(string destination)
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader($"- [Unsafe]({destination}) - #Unsafe");
        var before = workspace.SnapshotHashes();

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Blocked, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list Loader reports a missing declared root as unavailable incomplete coverage")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MissingRootIsIncomplete()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        const string destination = "missing/_missing.md";
        workspace.WriteLoader($"- [Missing]({destination}) - #Missing");

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.LoaderUnavailable, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
    }

    [Fact(DisplayName = "Route-list Loader reports an existing but unrecognized root as unavailable")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnrecognizedRootIsIncomplete()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        const string destination = "root/_root.md";
        workspace.WriteLoader($"- [Root]({destination}) - #Root");
        workspace.Write(".agents/root/_root.md", "unrecognized by catalogue");

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.LoaderUnavailable, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
    }

    [Fact(DisplayName = "Route-list Loader reports a declared leaf as malformed root input")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task NonEntrypointRootIsIncomplete()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        const string destination = "root/leaf.md";
        workspace.WriteLoader($"- [Leaf]({destination}) - #Leaf");
        workspace.Write(".agents/root/leaf.md", "leaf");
        var leaf = workspace.Source(
            "root/leaf",
            ".agents/root/leaf.md",
            RouteListSourceKind.RoutedLeaf);

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(leaf));

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.LoaderMalformed, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
    }

    [Fact(DisplayName = "Route-list Loader rejects an overwrite companion as a root declaration")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task OverwriteRootIsIncomplete()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        const string destination = "root/_root.overwrite.md";
        workspace.WriteLoader($"- [Overwrite]({destination}) - #Root");
        workspace.Write(".agents/root/_root.md", "base");
        workspace.Write(".agents/root/_root.overwrite.md", "overwrite");
        var source = workspace.Source(
            "root",
            ".agents/root/_root.md",
            RouteListSourceKind.Entrypoint,
            overwritePath: ".agents/root/_root.overwrite.md");

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(source));

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.LoaderMalformed, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
    }

    [Theory(DisplayName = "Route-list Loader rejects orphan overwrite and malformed nested-label root declarations"),
        InlineData("- [Orphan](root/_root.overwrite.md) - #Root", ".agents/root/_root.overwrite.md", "orphan"),
        InlineData("- [forged [Root](root/_root.md) - #Root", ".agents/root/_root.md", "root")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task StructurallyInvalidRootDeclarationsAreIncomplete(
        string declaration,
        string physicalPath,
        string contents)
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader(declaration);
        workspace.Write(physicalPath, contents);

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Incomplete, result.State);
        Assert.Equal(RouteListFindingCode.LoaderMalformed, Assert.Single(result.Issues).Code);
        Assert.Empty(result.SelectedSources);
    }

    [Fact(DisplayName = "Route-list Loader blocks a root whose route identity is ambiguous")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task AmbiguousRootIsBlocked()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        const string destination = "root/_root.md";
        workspace.WriteLoader($"- [Root]({destination}) - #Root");
        workspace.Write(".agents/root/_root.md", "ambiguous");
        var source = workspace.Source(
            "root",
            ".agents/root/_root.md",
            RouteListSourceKind.Entrypoint,
            isRouteAmbiguous: true);

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(source));

        Assert.Equal(LoaderDestinationResolutionState.Blocked, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.RouteAmbiguous, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
    }

    [Fact(DisplayName = "Route-list Loader accepts a contained physical root alias and keeps its logical path")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ContainedRootAliasResolves()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write("contained/_root.md", "contained root");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/root/_root.md",
                workspace.Absolute("contained/_root.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        var source = workspace.Source(
            "root",
            ".agents/root/_root.md",
            RouteListSourceKind.Entrypoint,
            physicalRelativePath: "contained/_root.md");
        var before = workspace.SnapshotHashes();

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue(source));

        Assert.Equal(LoaderDestinationResolutionState.Resolved, result.State);
        Assert.Same(source, Assert.Single(result.SelectedSources));
        Assert.Equal(".agents/root/_root.md", result.SelectedSources[0].CanonicalPath);
        Assert.Empty(result.Issues);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list Loader blocks an external root alias without inspecting outside")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExternalRootAliasIsBlocked()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-loader-external-root");
        const string destination = "root/escape/_escape.md";
        workspace.WriteLoader($"- [Escape]({destination}) - #Escape");
        outside.WriteText("_escape.md", "outside");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(
                ".agents/root/escape",
                outside.Path,
                out _),
            "This integration case requires real symbolic-link support.");
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var result = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(LoaderDestinationResolutionState.Blocked, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(result.SelectedSources);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Route-list Loader blocks the first external transition before a reentry target")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExternalThenReentryRootIsBlockedBeforeReentry()
    {
        using var workspace = RouteListSelectionIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-list-loader-external-reentry");
        const string destination = "root/return/back/_return.md";
        workspace.WriteLoader($"- [Return]({destination}) - #Return");
        workspace.Write("inside/_return.md", "inside");
        Assert.True(
            outside.TryCreateDirectorySymbolicLink(
                "back",
                workspace.Absolute("inside"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(
                ".agents/root/return",
                outside.Path,
                out _),
            "This integration case requires real symbolic-link support.");
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var first = await ResolveLoaderAsync(workspace, workspace.Catalogue());
        var second = await ResolveLoaderAsync(workspace, workspace.Catalogue());

        Assert.Equal(first.State, second.State);
        Assert.Equal(first.Issues.Select(issue => issue.Code), second.Issues.Select(issue => issue.Code));
        Assert.Equal(LoaderDestinationResolutionState.Blocked, first.State);
        var issue = Assert.Single(first.Issues);
        Assert.Equal(RouteListFindingCode.PhysicalBoundary, issue.Code);
        Assert.Equal(destination, issue.Subject);
        Assert.Empty(first.SelectedSources);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    private static async Task<LoaderDestinationResolution> ResolveLoaderAsync(
        RouteListSelectionIntegrationWorkspace workspace,
        RouteSourceCatalogue catalogue)
    {
        return await new LoaderDestinationResolver(new PhysicalPathResolver())
            .ResolveAsync(
                workspace.Workspace,
                catalogue,
                TestContext.Current.CancellationToken);
    }
}
