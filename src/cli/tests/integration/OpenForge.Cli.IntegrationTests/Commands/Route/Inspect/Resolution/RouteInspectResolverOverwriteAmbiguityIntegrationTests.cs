using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectResolverOverwriteAmbiguityIntegrationTests
{
    [Theory(DisplayName = "Route inspect blocks either exact base path whose overwrite has ambiguous candidates"),
        InlineData(".agents/root/ambiguous.md"),
        InlineData(".agents/root/ambiguous/_ambiguous.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task ExactBasePathDoesNotRepairAmbiguousOverwriteMeaning(string selectedPath)
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

        Assert.Equal(RouteInspectResolutionState.Blocked, result.State);
        Assert.Equal(RouteInspectReferenceKind.SourcePath, result.Selection.ReferenceKind);
        Assert.Equal(RouteInspectSelectionMethod.ExactPath, result.Selection.SelectionMethod);
        Assert.Equal(selectedPath, result.Selection.RequestedReference);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.AmbiguousOverwrite, issue.Code);
        Assert.Equal(overwritePath, issue.Subject);
        Assert.Equal([basePath, entrypointPath], issue.Paths);
        Assert.Null(result.Identity);
        Assert.Null(result.Graph);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
