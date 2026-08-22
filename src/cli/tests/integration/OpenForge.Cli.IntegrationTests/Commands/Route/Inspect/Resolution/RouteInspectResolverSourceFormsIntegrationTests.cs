using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverSourceFormsIntegrationTests
{
    [Theory(DisplayName = "Route inspect resolves the canonical and every compatibility entrypoint form"),
        InlineData(".agents/canonical/_canonical.md", "canonical", nameof(RouteInspectSourceForm.CanonicalEntrypoint), nameof(RouteSourceForm.CanonicalEntrypoint)),
        InlineData(".agents/index/index.md", "index", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(RouteSourceForm.IndexEntrypoint)),
        InlineData(".agents/underscore-index/_index.md", "underscore-index", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(RouteSourceForm.UnderscoreIndexEntrypoint)),
        InlineData(".agents/references/references.md", "references", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(RouteSourceForm.ReferencesEntrypoint)),
        InlineData(".agents/underscore-references/_references.md", "underscore-references", nameof(RouteInspectSourceForm.CompatibilityEntrypoint), nameof(RouteSourceForm.UnderscoreReferencesEntrypoint))]
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
        Assert.Equal(expectedId, result.Identity!.Id);
        Assert.Equal(RouteInspectSourceKind.Entrypoint, result.Identity!.Kind);
        Assert.Equal(Enum.Parse<RouteInspectSourceForm>(expectedInspectForm), result.Identity.Form);
        Assert.Equal(RouteInspectRouteState.Routed, result.Identity.RouteState);
        Assert.Equal(
            Enum.Parse<RouteSourceForm>(expectedSourceForm),
            result.Graph!.Catalogue.FindByPath(path)!.Base.Form);
        Assert.Same(
            result.Graph.Catalogue.FindByPath(path),
            result.Graph.Topology.FindByPath(path)!.Source);
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
        Assert.Equal(id, result.Identity!.Id);
        Assert.Equal(Enum.Parse<RouteInspectSourceKind>(expectedKind), result.Identity.Kind);
        Assert.Equal(
            path.EndsWith("SKILL.md", StringComparison.Ordinal)
                ? RouteInspectSourceForm.Native
                : RouteInspectSourceForm.Markdown,
            result.Identity.Form);
        Assert.Equal(RouteInspectRouteState.Routed, result.Identity.RouteState);
        var node = result.Graph!.Topology.FindByPath(path)!;
        Assert.Equal(".agents/root/_root.md", node.ParentPath);
        Assert.Same(result.Graph.Catalogue.FindByPath(path), node.Source);
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
        Assert.Equal(RouteInspectRouteState.Detached, result.Identity!.RouteState);
        Assert.Empty(result.Graph!.Topology.LoaderRootPaths);
        var node = Assert.IsType<RouteTopologyNode>(result.Graph.Topology.FindByPath(path));
        Assert.Equal(RouteTopologyParentState.None, node.ParentState);
        Assert.Same(result.Graph.Catalogue.FindByPath(path), node.Source);
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
        Assert.Equal(RouteInspectRouteState.NotRouted, result.Identity!.RouteState);
        Assert.Equal(Enum.Parse<RouteInspectSourceKind>(expectedKind), result.Identity.Kind);
        Assert.Equal(Enum.Parse<RouteInspectSourceForm>(expectedForm), result.Identity.Form);
        Assert.Null(result.Graph!.Topology.FindByPath(path));
        var source = result.Graph.Catalogue.FindByPath(path);
        Assert.NotNull(source);
        Assert.Equal(result.Identity.Id, source!.Id);
        Assert.Equal(path, source.CanonicalPath);
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
        var root = result.Graph!.Topology.FindByPath(".agents/root/_root.md")!;
        Assert.DoesNotContain(detachedPath, root.ChildPaths);
        var detached = result.Graph.Topology.FindByPath(detachedPath)!;
        Assert.Equal(RouteTopologyParentState.None, detached.ParentState);
    }
}
