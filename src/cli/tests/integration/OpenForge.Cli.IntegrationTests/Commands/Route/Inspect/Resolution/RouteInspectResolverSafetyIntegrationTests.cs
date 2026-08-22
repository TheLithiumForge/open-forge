using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverSafetyIntegrationTests
{
    [Fact(DisplayName = "Route inspect blocks an orphan overwrite instead of promoting it to a source")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task OrphanOverwriteIsBlocked()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        const string path = ".agents/root/leaf.overwrite.md";
        workspace.Write(path, "orphan overwrite");

        var result = await workspace.ResolveAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        Assert.Equal(RouteInspectReferenceKind.SourcePath, result.Selection.ReferenceKind);
        Assert.Equal(path, result.Selection.RequestedReference);
        Assert.Equal(RouteInspectResolutionIssueCode.OrphanOverwrite, Assert.Single(result.Issues).Code);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
    }

    [Fact(DisplayName = "Route inspect blocks an overwrite whose automatic identity has ambiguous base candidates")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task AmbiguousOverwriteIsBlocked()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        workspace.Write(".agents/root/ambiguous.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        workspace.Write(
            ".agents/root/ambiguous/_ambiguous.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Entrypoint", "Entrypoint"));
        const string overwritePath = ".agents/root/ambiguous.overwrite.md";
        workspace.Write(overwritePath, "ambiguous overwrite");

        var result = await workspace.ResolveAsync(overwritePath, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.AmbiguousOverwrite, issue.Code);
        Assert.Equal(
            [".agents/root/ambiguous.md", ".agents/root/ambiguous/_ambiguous.md"],
            issue.Paths);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
    }

    [Fact(DisplayName = "Route inspect blocks an unsafe contained-link transition without mutating either location")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ExternalContainedLinkIsBlockedAtTheBoundary()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-inspect-resolution-external");
        var outsideFile = outside.CreateFile("outside.md", "outside");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/root/escape.md", outsideFile, out var linkPath),
            "This integration case requires real symbolic-link support.");
        Assert.NotNull(linkPath);
        var originalLinkTarget = new FileInfo(linkPath).LinkTarget;
        Assert.NotNull(originalLinkTarget);
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var result = await workspace.ResolveAsync(".agents/root/escape.md", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        Assert.Equal(RouteInspectResolutionIssueCode.UnsafeSource, Assert.Single(result.Issues).Code);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
        var retainedLink = new FileInfo(linkPath);
        Assert.True((retainedLink.Attributes & FileAttributes.ReparsePoint) != 0);
        Assert.Equal(originalLinkTarget, retainedLink.LinkTarget);
    }

    [Fact(DisplayName = "Route inspect blocks a safe subject when the Loader crosses an unsafe physical boundary")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnsafeLoaderBlocksRouteResolution()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        using var outside = TemporaryWorkspace.Create("route-inspect-resolution-external-loader");
        var outsideLoader = outside.CreateFile("loader.md", "outside Loader");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/loader.md", outsideLoader, out var loaderLinkPath),
            "This integration case requires real symbolic-link support.");
        Assert.NotNull(loaderLinkPath);
        var originalLinkTarget = new FileInfo(loaderLinkPath).LinkTarget;
        Assert.NotNull(originalLinkTarget);
        workspace.Write(
            ".agents/root/_root.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var result = await workspace.ResolveAsync(
            ".agents/root/_root.md",
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.UnsafeSource, issue.Code);
        Assert.Equal(".agents/loader.md", issue.Subject);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
        var retainedLink = new FileInfo(loaderLinkPath);
        Assert.True((retainedLink.Attributes & FileAttributes.ReparsePoint) != 0);
        Assert.Equal(originalLinkTarget, retainedLink.LinkTarget);
    }

    [Fact(DisplayName = "Route inspect retains an incomplete result when a required route ancestor is unreadable")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task UnreadableRequiredAncestorIsIncomplete()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", [0xC3, 0x28]);
        workspace.Write(
            ".agents/root/leaf.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        var before = workspace.SnapshotHashes();

        var result = await workspace.ResolveAsync("root/leaf", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Incomplete, result.State);
        Assert.Equal(RouteInspectSelectionMethod.AutomaticId, result.Selection.SelectionMethod);
        Assert.Equal("root/leaf", result.Selection.RequestedReference);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root/leaf", identity.Id);
        Assert.Equal(".agents/root/leaf.md", identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteInspectRouteState.Routed, identity.RouteState);
        var graph = Assert.IsType<RouteInspectGraph>(result.Graph);
        Assert.Same(
            graph.Catalogue.FindByPath(identity.CanonicalWorkspaceRelativePath),
            graph.Topology.FindByPath(identity.CanonicalWorkspaceRelativePath)!.Source);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.ReadUnavailable, issue.Code);
        Assert.Equal(".agents/root/_root.md", issue.Subject);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route inspect returns an interrupted resolution when its caller is cancelled")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task CancellationFormsAnInterruptedResolution()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        cancellation.Cancel();

        var result = await workspace.ResolveAsync("root", cancellation.Token);

        Assert.Equal(RouteInspectResolutionState.Interrupted, result.State);
        Assert.Equal(RouteInspectResolutionIssueCode.Interrupted, Assert.Single(result.Issues).Code);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
    }

    [Fact(DisplayName = "Route inspect leaves bytes unchanged for one resolved read-only journey")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ResolvedJourneyDoesNotWrite()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        var before = workspace.SnapshotHashes();

        var result = await workspace.ResolveAsync("root", TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        Assert.Empty(result.Issues);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route inspect leaves bytes unchanged for one blocked overwrite journey")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task BlockedJourneyDoesNotWrite()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        const string path = ".agents/root/leaf.overwrite.md";
        workspace.Write(path, "orphan overwrite");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ResolveAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        Assert.Equal(RouteInspectResolutionIssueCode.OrphanOverwrite, Assert.Single(result.Issues).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

}
