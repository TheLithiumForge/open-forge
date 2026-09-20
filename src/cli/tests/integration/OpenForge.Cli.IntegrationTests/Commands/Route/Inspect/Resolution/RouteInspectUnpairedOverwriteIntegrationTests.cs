using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectUnpairedOverwriteIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Inspect keeps an unpaired overwrite orphaned when candidate IDs collide"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task CollidingIdsCannotInventOverwriteAmbiguity()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        string[] basePaths = [".agents/root/leaf/_leaf.md", ".agents/root/leaf/_index.md"];
        const string overwritePath = ".agents/root/leaf.overwrite.md";
        foreach (var path in basePaths)
        {
            workspace.Write(path, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        }
        workspace.Write(overwritePath, "unpaired overwrite");
        var before = workspace.SnapshotHashes();
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace.Workspace, [SourceLogicalPath.AgentsRoot]), TestContext.Current.CancellationToken);
        var selection = catalogue.SelectAll();
        Assert.Equal(2, selection.Sources.Count(source => source.Identity.AutomaticId == "root/leaf"));
        Assert.All(selection.Sources, source => Assert.Null(source.Overwrite));
        Assert.Contains(selection.Issues, issue => issue.Code == SourceCatalogueIssueCode.EntrypointCompatibilityCollision);

        var result = await RouteInspectSourceProjectionBuilder.ReadAsync(
            selection,
            new SourceDocumentReader(workspace.Workspace),
            TestContext.Current.CancellationToken);

        var fact = Assert.Single(result.ProjectionSet.OverwriteFacts);
        Assert.Equal(RouteOverwriteState.Orphan, fact.State);
        Assert.Equal(overwritePath, fact.CanonicalPath);
        Assert.Equal(basePaths.Order(StringComparer.Ordinal), fact.CandidateBasePaths);
        Assert.All(result.Projections, projection => Assert.Null(projection.OverwriteRead));
        Assert.Equal(overwritePath, Assert.Single(result.ReadResults).Layer.CanonicalPath);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
