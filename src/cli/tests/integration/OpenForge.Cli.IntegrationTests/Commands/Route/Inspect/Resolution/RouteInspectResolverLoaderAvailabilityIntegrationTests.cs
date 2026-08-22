using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverLoaderAvailabilityIntegrationTests
{
    [Fact(DisplayName = "Route inspect keeps route state unresolved when the Loader is unreadable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnreadableLoaderLeavesRouteMeaningUnresolved()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.Write(".agents/loader.md", [0xC3, 0x28]);
        workspace.Write(
            ".agents/root/_root.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        var before = workspace.SnapshotHashes();

        var result = await workspace.ResolveAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Incomplete, result.State);
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, result.Selection.SelectionMethod);
        Assert.Equal("root", result.Selection.RequestedReference);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root", identity.Id);
        Assert.Equal(".agents/root/_root.md", identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteInspectRouteState.Unresolved, identity.RouteState);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        Assert.Same(
            graph.Catalogue.FindByPath(identity.CanonicalWorkspaceRelativePath),
            graph.Topology.FindByPath(identity.CanonicalWorkspaceRelativePath)!.Source);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.ReadUnavailable, issue.Code);
        Assert.Equal(".agents/loader.md", issue.Subject);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route inspect keeps route state unresolved when Loader Entries are malformed")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task MalformedLoaderLeavesRouteMeaningUnresolved()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [forged [Root](root/_root.md) - #Root");
        workspace.Write(
            ".agents/root/_root.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        var before = workspace.SnapshotHashes();

        var result = await workspace.ResolveAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Incomplete, result.State);
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, result.Selection.SelectionMethod);
        Assert.Equal("root", result.Selection.RequestedReference);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root", identity.Id);
        Assert.Equal(".agents/root/_root.md", identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteInspectRouteState.Unresolved, identity.RouteState);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        Assert.Same(
            graph.Catalogue.FindByPath(identity.CanonicalWorkspaceRelativePath),
            graph.Topology.FindByPath(identity.CanonicalWorkspaceRelativePath)!.Source);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.IncompleteRoute, issue.Code);
        Assert.Equal(".agents/loader.md", issue.Subject);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
