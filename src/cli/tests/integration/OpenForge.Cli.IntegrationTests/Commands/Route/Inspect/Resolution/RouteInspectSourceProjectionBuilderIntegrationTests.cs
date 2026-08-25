using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

public sealed class RouteInspectSourceProjectionBuilderIntegrationTests
{
    [Fact(DisplayName = "Route Inspect projection builder uses a real OS reader for exact adjacent layers")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RealExactLayersRemainExplicit()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.Write(".agents/root/leaf.md", "base");
        workspace.Write(".agents/root/leaf.overwrite.md", "overwrite");
        var source = Source(workspace, ".agents/root/leaf.md", "root/leaf", withOverwrite: true);
        var before = workspace.SnapshotHashes();

        var result = await new RouteInspectSourceProjectionBuilder().ReadAsync(
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

    [Fact(DisplayName = "Route Inspect projection builder reads every ambiguous and orphan overwrite as a source-less real OS result")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RealAmbiguousAndNonAdjacentOverwriteFactsRemainBlockedEvidence()
    {
        using var workspace = RouteInspectResolutionIntegrationWorkspace.Create();
        workspace.Write(".agents/root/ambiguous.md", "leaf");
        workspace.Write(".agents/root/ambiguous/_ambiguous.md", "entrypoint");
        workspace.Write(".agents/root/ambiguous.overwrite.md", "ambiguous overwrite");
        workspace.Write(".agents/root/nonadjacent/_nonadjacent.md", "entrypoint");
        workspace.Write(".agents/root/nonadjacent.overwrite.md", "orphan overwrite");
        var first = Source(workspace, ".agents/root/ambiguous.md", "root/ambiguous");
        var second = Source(
            workspace,
            ".agents/root/ambiguous/_ambiguous.md",
            "root/ambiguous",
            SourceDocumentForm.CanonicalEntrypoint);
        var nonadjacentBase = Source(
            workspace,
            ".agents/root/nonadjacent/_nonadjacent.md",
            "root/nonadjacent",
            SourceDocumentForm.CanonicalEntrypoint);
        var ambiguous = new SourceLayer(
            ".agents/root/ambiguous.overwrite.md",
            workspace.Absolute(".agents/root/ambiguous.overwrite.md"),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        var nonadjacent = new SourceLayer(
            ".agents/root/nonadjacent.overwrite.md",
            workspace.Absolute(".agents/root/nonadjacent.overwrite.md"),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        var before = workspace.SnapshotHashes();

        var result = await new RouteInspectSourceProjectionBuilder().ReadAsync(
            Selection([first, second, nonadjacentBase], [ambiguous, nonadjacent]),
            new SourceDocumentReader(workspace.Workspace),
            TestContext.Current.CancellationToken);

        var ambiguousProjections = result.Projections
            .Where(projection => projection.LogicalSource.Identity.AutomaticId == "root/ambiguous")
            .ToArray();
        Assert.Equal(2, ambiguousProjections.Length);
        Assert.All(ambiguousProjections, projection =>
        {
            Assert.Null(projection.OverwriteRead);
            Assert.False(Assert.IsType<OpenForge.Cli.Core.Commands.Route.Shared.Models.Source.RouteSource>(
                projection.Source).Metadata.IsOverwritePresent);
        });
        Assert.Contains(
            result.ProjectionSet.OverwriteFacts,
            fact => fact.State == OpenForge.Cli.Core.Commands.Route.Shared.Models.Source.RouteOverwriteState.Ambiguous
                && fact.CanonicalPath == ".agents/root/ambiguous.overwrite.md"
                && fact.CandidateBasePaths.SequenceEqual(
                    [".agents/root/ambiguous.md", ".agents/root/ambiguous/_ambiguous.md"]));
        Assert.Contains(
            result.ProjectionSet.OverwriteFacts,
            fact => fact.State == OpenForge.Cli.Core.Commands.Route.Shared.Models.Source.RouteOverwriteState.Orphan
                && fact.CanonicalPath == ".agents/root/nonadjacent.overwrite.md"
                && fact.CandidateBasePaths.SequenceEqual(
                    [".agents/root/nonadjacent/_nonadjacent.md"]));
        Assert.Equal(
            [".agents/root/ambiguous.overwrite.md", ".agents/root/nonadjacent.overwrite.md"],
            result.ReadResults.Select(read => read.Layer.CanonicalPath));
        Assert.Equal(before, workspace.SnapshotHashes());
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
