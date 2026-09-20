using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectSourceProjectionBuilderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Inspect projection builder uses a real OS reader for exact adjacent layers"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RealExactLayersRemainExplicit()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.Write(".agents/root/leaf.md", "base");
        workspace.Write(".agents/root/leaf.overwrite.md", "overwrite");
        var source = Source(workspace, ".agents/root/leaf.md", "root/leaf", withOverwrite: true);
        var before = workspace.SnapshotHashes();

        var result = await RouteInspectSourceProjectionBuilder.ReadAsync(
            Selection(source),
            new SourceDocumentReader(workspace.Workspace),
            TestContext.Current.CancellationToken);

        var projection = Assert.Single(result.Projections);
        var baseRead = Assert.IsType<FileReadResult<string>>(projection.BaseRead.Read);
        var overwrite = Assert.IsType<SourceDocumentReadResult>(projection.OverwriteRead);
        var overwriteRead = Assert.IsType<FileReadResult<string>>(overwrite.Read);
        Assert.Same(source, projection.LogicalSource);
        Assert.Equal("base", baseRead.Value);
        Assert.Equal("overwrite", overwriteRead.Value);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Inspect preserves real catalogue pairing separately from ID collisions and orphan overwrites"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RealCataloguePairSurvivesCollisionWhileOrphanRemainsExplicit()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        const string basePath = ".agents/root/ambiguous.md";
        const string overwritePath = ".agents/root/ambiguous.overwrite.md";
        const string entrypointPath = ".agents/root/ambiguous/_ambiguous.md";
        const string orphanPath = ".agents/root/nonadjacent.overwrite.md";
        workspace.Write(basePath, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Leaf", "Leaf"));
        workspace.Write(entrypointPath, RouteInspectResolutionIntegrationWorkspace.OpenForgeMetadata("Entrypoint", "Entrypoint"));
        workspace.Write(overwritePath, "paired overwrite");
        workspace.Write(".agents/root/nonadjacent/_nonadjacent.md", "entrypoint");
        workspace.Write(orphanPath, "orphan overwrite");
        var before = workspace.SnapshotHashes();
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace.Workspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);
        var selection = catalogue.SelectAll();
        var paired = Assert.Single(selection.Sources, source => source.Identity.CanonicalBasePath == basePath);
        Assert.Equal(overwritePath, Assert.IsType<SourceLayer>(paired.Overwrite).CanonicalPath);
        Assert.Equal(2, selection.Sources.Count(source => source.Identity.AutomaticId == "root/ambiguous"));
        Assert.Contains(selection.Issues, issue => issue.Code == SourceCatalogueIssueCode.IdentityCollision);

        var result = await RouteInspectSourceProjectionBuilder.ReadAsync(
            selection,
            new SourceDocumentReader(workspace.Workspace),
            TestContext.Current.CancellationToken);

        Assert.Equal(before, workspace.SnapshotHashes());
        var projection = Assert.Single(result.Projections, item => item.LogicalSource.Identity.CanonicalBasePath == basePath);
        var overwrite = Assert.IsType<SourceDocumentReadResult>(projection.OverwriteRead);
        Assert.Equal("paired overwrite", Assert.IsType<FileReadResult<string>>(overwrite.Read).Value);
        Assert.True(Assert.IsType<RouteSource>(projection.Source).Metadata.IsOverwritePresent);
        var colliding = Assert.Single(result.Projections, item => item.LogicalSource.Identity.CanonicalBasePath == entrypointPath);
        Assert.Null(colliding.OverwriteRead);
        Assert.False(Assert.IsType<RouteSource>(colliding.Source).Metadata.IsOverwritePresent);
        Assert.DoesNotContain(result.ProjectionSet.OverwriteFacts, fact => fact.State == RouteOverwriteState.Ambiguous);
        Assert.Contains(result.ProjectionSet.OverwriteFacts, fact => fact.State == RouteOverwriteState.Orphan
            && fact.CanonicalPath == orphanPath
            && fact.CandidateBasePaths.SequenceEqual([".agents/root/nonadjacent/_nonadjacent.md"]));
        Assert.Equal([orphanPath], result.ReadResults.Select(read => read.Layer.CanonicalPath));
    }

    private static SourceCatalogueSelection Selection(
        IReadOnlyList<SourceLogicalSource> sources,
        IReadOnlyList<SourceLayer>? sourceLessLayers = null)
    {
        var candidates = sources
            .SelectMany(source => new[]
            {
                Candidate(source.Base),
                source.Overwrite is null ? null : Candidate(source.Overwrite),
            })
            .Concat((sourceLessLayers ?? []).Select(Candidate))
            .Where(candidate => candidate is not null)
            .Cast<SourceCandidate>()
            .ToArray();
        return new SourceCatalogueSelection(sources, candidates, [], []);
    }

    private static SourceCatalogueSelection Selection(SourceLogicalSource source)
    {
        return Selection([source]);
    }

    private static SourceLogicalSource Source(
        RouteInspectResolutionIntegrationWorkspace workspace,
        string canonicalPath,
        string id,
        SourceDocumentForm form = SourceDocumentForm.Markdown,
        bool withOverwrite = false)
    {
        var baseLayer = new SourceLayer(
            canonicalPath,
            workspace.Absolute(canonicalPath),
            form,
            SourceLayerKind.Base);
        var overwritePath = canonicalPath[..^".md".Length] + ".overwrite.md";
        var overwrite = withOverwrite
            ? new SourceLayer(
                overwritePath,
                workspace.Absolute(overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            baseLayer,
            overwrite);
    }

    private static SourceCandidate Candidate(SourceLayer layer)
    {
        return new SourceCandidate(
            layer.CanonicalPath,
            layer.Form,
            layer.CanonicalPath[..^".md".Length],
            PhysicalPathState.Contained,
            layer.PhysicalPath,
            Path.GetDirectoryName(layer.PhysicalPath)
                ?? throw new InvalidOperationException("The integration layer must have a physical parent."));
    }
}
