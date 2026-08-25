using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListSourceProjectionBuilderIntegrationTests
{
    [Fact(DisplayName = "Route List projection builder uses one real OS selection for paired overwrite and body reads")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task PairedRealLayersAreProjectedOnce()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(".agents/root/leaf.md", "base");
        workspace.Write(".agents/root/leaf.overwrite.md", "overwrite");
        var logicalSource = Source(
            workspace,
            ".agents/root/leaf.md",
            "root/leaf",
            withOverwrite: true);
        var before = workspace.SnapshotHashes();

        var result = await new RouteListSourceProjectionBuilder().ReadAsync(
            Selection(logicalSource),
            new SourceDocumentReader(workspace.Workspace),
            TestContext.Current.CancellationToken);

        var projection = Assert.Single(result.Projections);
        var baseRead = Assert.IsType<FileReadResult<string>>(projection.BaseRead.Read);
        var overwrite = Assert.IsType<SourceDocumentReadResult>(projection.OverwriteRead);
        var overwriteRead = Assert.IsType<FileReadResult<string>>(overwrite.Read);
        Assert.Same(logicalSource, projection.LogicalSource);
        Assert.Equal("base", baseRead.Value);
        Assert.Equal("overwrite", overwriteRead.Value);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route List projection builder retains real orphan and collision evidence without Inspect-only ambiguity")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task RealOrphanAndCollisionFactsRemainListEvidence()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(".agents/root/collision.md", "leaf");
        workspace.Write(".agents/root/collision/_collision.md", "entrypoint");
        workspace.Write(".agents/root/orphan.overwrite.md", "orphan");
        var leaf = Source(workspace, ".agents/root/collision.md", "root/collision");
        var entrypoint = Source(
            workspace,
            ".agents/root/collision/_collision.md",
            "root/collision",
            SourceDocumentForm.CanonicalEntrypoint);
        var orphan = new SourceLayer(
            ".agents/root/orphan.overwrite.md",
            workspace.Absolute(".agents/root/orphan.overwrite.md"),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        var before = workspace.SnapshotHashes();

        var result = await new RouteListSourceProjectionBuilder().ReadAsync(
            Selection([leaf, entrypoint], [orphan]),
            new SourceDocumentReader(workspace.Workspace),
            TestContext.Current.CancellationToken);

        Assert.Single(result.ProjectionSet.IdentityCollisions);
        Assert.Contains(result.ReadResults, read =>
            read.Layer.CanonicalPath == ".agents/root/orphan.overwrite.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static SourceCatalogueSelection Selection(params SourceLogicalSource[] sources)
    {
        return Selection(sources, []);
    }

    private static SourceCatalogueSelection Selection(
        IReadOnlyList<SourceLogicalSource> sources,
        IReadOnlyList<SourceLayer> sourceLessLayers)
    {
        var candidates = sources
            .SelectMany(source => new[]
            {
                Candidate(source.Base),
                source.Overwrite is null ? null : Candidate(source.Overwrite),
            })
            .Concat(sourceLessLayers.Select(Candidate))
            .Where(candidate => candidate is not null)
            .Cast<SourceCandidate>()
            .ToArray();
        return new SourceCatalogueSelection(sources, candidates, [], []);
    }

    private static SourceLogicalSource Source(
        RouteListFilesystemIntegrationWorkspace workspace,
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
        var physicalParentPath = Assert.IsType<string>(Path.GetDirectoryName(layer.PhysicalPath));
        return new SourceCandidate(
            layer.CanonicalPath,
            layer.Form,
            layer.CanonicalPath[..^".md".Length],
            PhysicalPathState.Contained,
            layer.PhysicalPath,
            physicalParentPath);
    }
}
