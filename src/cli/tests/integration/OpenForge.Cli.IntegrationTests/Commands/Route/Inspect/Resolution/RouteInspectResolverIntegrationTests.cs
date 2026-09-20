using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect resolves an automatic ID and its exact path to the same graph source")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task IdAndExactPathHaveResolutionParity()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        var before = workspace.SnapshotHashes();

        var byId = await workspace.ResolveAsync("root", TestContext.Current.CancellationToken);
        var byPath = await workspace.ResolveAsync("./.agents/root/_root.md", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, byId.State);
        Assert.Equal(RouteInspectResolutionState.Resolved, byPath.State);
        var byIdIdentity = Assert.IsType<RouteInspectIdentity>(byId.Identity);
        var byPathIdentity = Assert.IsType<RouteInspectIdentity>(byPath.Identity);
        Assert.Equal("root", byIdIdentity.Id);
        Assert.Equal(".agents/root/_root.md", byIdIdentity.CanonicalWorkspaceRelativePath);
        Assert.Equal(byIdIdentity.Id, byPathIdentity.Id);
        Assert.Equal(byIdIdentity.CanonicalWorkspaceRelativePath, byPathIdentity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, byId.Selection.SelectionMethod);
        Assert.Equal(RouteInspectSelectionMethod.ExactPath, byPath.Selection.SelectionMethod);
        var byIdGraph = Assert.IsType<RouteInspectGraph>(byId.Graph);
        var projection = Assert.Single(
            byIdGraph.ProjectionSet.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == ".agents/root/_root.md");
        var node = Assert.IsType<SourceRouteNode>(
            byIdGraph.RouteFacts.Topology.FindByPath(".agents/root/_root.md"));
        Assert.Same(
            projection.LogicalSource.Identity,
            node.Identity);
        Assert.Equal([".agents/root/_root.md"], byIdGraph.RouteFacts.Topology.LoaderRootPaths);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect reports a missing exact source as invalid without resolved facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MissingSourceIsInvalid()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();

        var result = await workspace.ResolveAsync(".agents/root/missing.md", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Invalid, result.State);
        Assert.Equal(RouteInspectReferenceKind.SourcePath, result.Selection.ReferenceKind);
        Assert.Equal(".agents/root/missing.md", result.Selection.RequestedReference);
        Assert.False(result.Selection.IsResolved);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
        Assert.Equal(RouteInspectResolutionIssueCode.MissingSource, Assert.Single(result.Issues).Code);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect rejects malformed traversal references before filesystem resolution")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MalformedReferenceIsInvalid()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();

        var result = await workspace.ResolveAsync(".agents/root/../root.md", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Invalid, result.State);
        Assert.Equal(RouteInspectReferenceKind.Invalid, result.Selection.ReferenceKind);
        Assert.Equal(".agents/root/../root.md", result.Selection.RequestedReference);
        Assert.Equal(RouteInspectResolutionIssueCode.InvalidReference, Assert.Single(result.Issues).Code);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect rejects the Loader as a subject rather than exposing a root route")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task LoaderSubjectIsInvalid()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader(string.Empty);

        var result = await workspace.ResolveAsync("loader", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Invalid, result.State);
        Assert.Equal(RouteInspectReferenceKind.SourceId, result.Selection.ReferenceKind);
        Assert.Equal("loader", result.Selection.RequestedReference);
        Assert.Equal(RouteInspectResolutionIssueCode.LoaderSubject, Assert.Single(result.Issues).Code);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect blocks an ambiguous automatic ID with sorted candidate paths")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task AmbiguousIdRetainsSortedCandidates()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        workspace.Write(".agents/root/collision.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        workspace.Write(
            ".agents/root/collision/_collision.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Collision", "Collision"));

        var result = await workspace.ResolveAsync("root/collision", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        Assert.Equal(RouteInspectReferenceKind.SourceId, result.Selection.ReferenceKind);
        Assert.False(result.Selection.IsResolved);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.AmbiguousSource, issue.Code);
        Assert.Equal(
            [".agents/root/collision.md", ".agents/root/collision/_collision.md"],
            issue.Paths);
        Assert.Equal(
            [".agents/root/collision.md", ".agents/root/collision/_collision.md"],
            result.Selection.CandidatePaths);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect exact-path selection resolves one source from a non-unique automatic ID")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ExactPathDisambiguatesOneSourceIdentity()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        workspace.Write(".agents/root/collision.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        workspace.Write(
            ".agents/root/collision/_collision.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Collision", "Collision"));

        var result = await workspace.ResolveAsync(".agents/root/collision.md", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        Assert.Equal(RouteInspectSelectionMethod.ExactPath, result.Selection.SelectionMethod);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root/collision", identity.Id);
        Assert.Equal(".agents/root/collision.md", identity.CanonicalWorkspaceRelativePath);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        var projection = Assert.Single(
            graph.ProjectionSet.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == identity.CanonicalWorkspaceRelativePath);
        var node = Assert.IsType<SourceRouteNode>(
            graph.RouteFacts.Topology.FindByPath(identity.CanonicalWorkspaceRelativePath));
        Assert.Same(
            projection.LogicalSource.Identity,
            node.Identity);
        Assert.Empty(result.Issues);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect gives ID, base-path, and overwrite-path references one logical source")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task BaseAndOverwriteReferencesHaveParity()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        workspace.Write(".agents/root/leaf.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        workspace.Write(".agents/root/leaf.overwrite.md", "overwrite body");

        var byId = await workspace.ResolveAsync("root/leaf", TestContext.Current.CancellationToken);
        var byBase = await workspace.ResolveAsync(".agents/root/leaf.md", TestContext.Current.CancellationToken);
        var byOverwrite = await workspace.ResolveAsync(
            "./.agents/root/leaf.overwrite.md",
            TestContext.Current.CancellationToken);

        Assert.All(new[] { byId, byBase, byOverwrite }, result =>
        {
            Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
            var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
            Assert.Equal("root/leaf", identity.Id);
            Assert.Equal(".agents/root/leaf.md", identity.CanonicalWorkspaceRelativePath);
            Assert.Equal(
                [".agents/root/leaf.md", ".agents/root/leaf.overwrite.md"],
                identity.PhysicalLayers.Select(layer => layer.WorkspaceRelativePath));
        });
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, byId.Selection.SelectionMethod);
        Assert.Equal(RouteInspectSelectionMethod.ExactPath, byBase.Selection.SelectionMethod);
        Assert.Equal(RouteInspectSelectionMethod.ExactPath, byOverwrite.Selection.SelectionMethod);
    }

}
