using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverOverwritePairIntegrationTests
{
    [Theory(DisplayName = "Route inspect preserves exact catalogue pairs despite an automatic ID collision"),
        InlineData(".agents/root/ambiguous.md"),
        InlineData(".agents/root/ambiguous.overwrite.md"),
        InlineData(".agents/root/ambiguous/_ambiguous.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public static async Task ExactPathsPreserveCataloguePairDespiteAutomaticIdCollision(string selectedPath)
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(
            ".agents/root/_root.md",
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Root", "Root"));
        const string basePath = ".agents/root/ambiguous.md";
        workspace.Write(
            basePath,
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        const string entrypointPath = ".agents/root/ambiguous/_ambiguous.md";
        workspace.Write(
            entrypointPath,
            RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Entrypoint", "Entrypoint"));
        const string overwritePath = ".agents/root/ambiguous.overwrite.md";
        workspace.Write(overwritePath, "ambiguous overwrite");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ResolveAsync(selectedPath, TestContext.Current.CancellationToken);

        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteInspectResolutionState.Resolved, result.State);
        Assert.Equal(RouteInspectReferenceKind.SourcePath, result.Selection.ReferenceKind);
        Assert.Equal(RouteInspectSelectionMethod.ExactPath, result.Selection.SelectionMethod);
        Assert.Equal(selectedPath, result.Selection.RequestedReference);
        Assert.Empty(result.Issues);
        var identity = Assert.IsType<RouteInspectIdentity>(result.Identity);
        Assert.Equal("root/ambiguous", identity.Id);
        Assert.Equal(selectedPath == entrypointPath ? entrypointPath : basePath, identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(
            selectedPath == entrypointPath ? [entrypointPath] : new[] { basePath, overwritePath },
            identity.PhysicalLayers.Select(layer => layer.WorkspaceRelativePath));
        Assert.NotNull(result.Graph);
    }
}
