using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverSourceFormsIntegrationTests
{
    [Theory(DisplayName = "Route inspect resolves the canonical and every compatibility entrypoint form"),
        InlineData(".agents/canonical/_canonical.md", "canonical", nameof(RouteInspectSourceForm.CanonicalEntrypoint), nameof(SourceDocumentForm.CanonicalEntrypoint)),
        InlineData(".agents/index/index.md", "index", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(SourceDocumentForm.IndexEntrypoint)),
        InlineData(".agents/underscore-index/_index.md", "underscore-index", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(SourceDocumentForm.UnderscoreIndexEntrypoint)),
        InlineData(".agents/references/references.md", "references", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(SourceDocumentForm.ReferencesEntrypoint)),
        InlineData(".agents/underscore-references/_references.md", "underscore-references", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(SourceDocumentForm.UnderscoreReferencesEntrypoint))]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task EntrypointFormsRetainIntrinsicIdentity(
        string path,
        string expectedId,
        string expectedInspectForm,
        string expectedSourceForm)
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader($"- [Route]({path[".agents/".Length..]}) - #Route");
        workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Route", "Route"));

        var result = await workspace.ResolveAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(expectedId, identity.Id);
        Assert.Equal(RouteInspectSourceKind.Entrypoint, identity.Kind);
        Assert.Equal(Enum.Parse<RouteInspectSourceForm>(expectedInspectForm), identity.Form);
        Assert.Equal(RouteInspectRouteState.Routed, identity.RouteState);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        var projection = Assert.Single(
            graph.ProjectionSet.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == path);
        var node = Assert.IsType<SourceRouteNode>(graph.RouteFacts.Topology.FindByPath(path));
        Assert.Equal(
            Enum.Parse<SourceDocumentForm>(expectedSourceForm),
            projection.LogicalSource.Base.Form);
        Assert.Same(
            projection.LogicalSource.Identity,
            node.Identity);
        Assert.Empty(result.Issues);
    }

    [Theory(DisplayName = "Route inspect resolves routed Markdown and native Skill sources with authored parent topology"),
        InlineData(".agents/root/leaf.md", "root/leaf", nameof(RouteInspectSourceKind.Markdown)),
        InlineData(".agents/root/native/SKILL.md", "root/native", nameof(RouteInspectSourceKind.Native))]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RoutedLeafAndNativeIdentityRetainTopology(
        string path,
        string id,
        string expectedKind)
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        if (path.EndsWith("SKILL.md", StringComparison.Ordinal))
        {
            workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.SkillMetadata("native", "Native"));
        }
        else
        {
            workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        }

        var result = await workspace.ResolveAsync(id, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(id, identity.Id);
        Assert.Equal(Enum.Parse<RouteInspectSourceKind>(expectedKind), identity.Kind);
        Assert.Equal(
            path.EndsWith("SKILL.md", StringComparison.Ordinal)
                ? RouteInspectSourceForm.Native
                : RouteInspectSourceForm.Markdown,
            identity.Form);
        Assert.Equal(RouteInspectRouteState.Routed, identity.RouteState);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        var node = Assert.IsType<SourceRouteNode>(graph.RouteFacts.Topology.FindByPath(path));
        Assert.Equal(SourceRouteParentState.Resolved, node.ParentState);
        Assert.Equal([".agents/root/_root.md"], node.ParentPaths);
        var projection = Assert.Single(
            graph.ProjectionSet.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == path);
        Assert.Same(projection.LogicalSource.Identity, node.Identity);
    }

    [Fact(DisplayName = "Route inspect resolves a detached entrypoint without fabricating Loader roots")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task DetachedEntrypointRetainsLocalTopologyOnly()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader(string.Empty);
        const string path = ".agents/detached/_detached.md";
        workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Detached", "Detached"));

        var result = await workspace.ResolveAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(RouteInspectRouteState.Detached, identity.RouteState);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        Assert.Empty(graph.RouteFacts.Topology.LoaderRootPaths);
        var node = Assert.IsType<SourceRouteNode>(graph.RouteFacts.Topology.FindByPath(path));
        Assert.Equal(SourceRouteParentState.None, node.ParentState);
        var projection = Assert.Single(
            graph.ProjectionSet.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == path);
        Assert.Same(projection.LogicalSource.Identity, node.Identity);
    }

    [Theory(DisplayName = "Route inspect reports known supported unrouted Markdown and native sources without route facts"),
        InlineData(".agents/flat.md", nameof(RouteInspectSourceKind.Markdown), nameof(RouteInspectSourceForm.Markdown)),
        InlineData(".agents/native/SKILL.md", nameof(RouteInspectSourceKind.Native), nameof(RouteInspectSourceForm.Native))]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task SupportedUnroutedSourcesRemainKnownButNotRouted(
        string path,
        string expectedKind,
        string expectedForm)
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader(string.Empty);
        if (path.EndsWith("SKILL.md", StringComparison.Ordinal))
        {
            workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.SkillMetadata("native", "Native"));
        }
        else
        {
            workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Flat", "Flat"));
        }

        var result = await workspace.ResolveAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal(RouteInspectRouteState.NotRouted, identity.RouteState);
        Assert.Equal(Enum.Parse<RouteInspectSourceKind>(expectedKind), identity.Kind);
        Assert.Equal(Enum.Parse<RouteInspectSourceForm>(expectedForm), identity.Form);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        Assert.Null(graph.RouteFacts.Topology.FindByPath(path));
        var projection = Assert.Single(
            graph.ProjectionSet.Projections,
            projection => projection.LogicalSource.Identity.CanonicalBasePath == path);
        Assert.Equal(identity.Id, projection.LogicalSource.Identity.AutomaticId);
        Assert.Equal(path, projection.LogicalSource.Identity.CanonicalBasePath);
    }

    [Fact(DisplayName = "Route inspect derives authored topology without treating generated Entries as graph edges")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task GeneratedNavigationDoesNotDefineAuthoredParentage()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(
            ".agents/root/_root.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root")
            + """
                # Root

                ## Entries

                <!-- open-forge:generated-index:start -->
                - [Detached](../detached/_detached.md) - #Detached
                <!-- open-forge:generated-index:end -->
                """);
        const string detachedPath = ".agents/detached/_detached.md";
        workspace.Write(
            detachedPath,
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Detached", "Detached"));

        var result = await workspace.ResolveAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        var root = Assert.IsType<SourceRouteNode>(
            graph.RouteFacts.Topology.FindByPath(".agents/root/_root.md"));
        Assert.DoesNotContain(detachedPath, root.ChildPaths);
        var detached = Assert.IsType<SourceRouteNode>(graph.RouteFacts.Topology.FindByPath(detachedPath));
        Assert.Equal(SourceRouteParentState.None, detached.ParentState);
    }
}
