using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationProspectiveFormationIntegrationTests
{
    [Fact(DisplayName = "Prospective generated navigation projects an intended new source through real current files without writes")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task IntendedSourceProjectsAgainstRealCurrentWorkspaceWithoutWrites()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-formation-intended");
        workspace.Write(
            SourceLogicalPath.LoaderPath,
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "stale",
                Prefix = "# Loader",
            }));
        workspace.Write(
            ".agents/root/_root.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Root",
                tags: ["Root"],
                body: OpenForgeDocumentSeed.GeneratedEntries(entries: "stale")));
        var before = workspace.SnapshotHashes();
        var catalogue = await ReadCatalogueAsync(workspace);
        var root = Assert.IsType<SourceLogicalSource>(catalogue.FindByPath(".agents/root/_root.md"));
        var addedPath = ".agents/root/added.md";
        var added = new SourceLogicalSource(
            identity: new SourceLogicalIdentity(
                SourceIdentity.DeriveId(addedPath)
                    ?? throw new InvalidOperationException("The intended integration source requires an automatic ID."),
                addedPath),
            @base: new SourceLayer(
                canonicalPath: addedPath,
                physicalPath: workspace.Absolute(addedPath),
                form: SourceDocumentForm.Markdown,
                kind: SourceLayerKind.Base));
        var formation = new GeneratedNavigationFormationBuilder().Build(
            catalogue,
            [.. catalogue.Sources, added]);
        var document = await ReadDocumentAsync(workspace, root);
        var request = new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(root, document)],
            metadata:
            [
                new GeneratedNavigationMetadata(
                    added,
                    SourceAuthoredMetadataFacts.Complete("Added", ["Docs"])),
            ]);

        var projection = new GeneratedNavigationProjector().Project(request);

        var region = Assert.Single(projection.Regions);
        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Equal("\n- [Added](added.md) - #Docs\n", region.ExpectedBody);
        Assert.Same(added, Assert.Single(region.Entries).Source);
        Assert.Null(catalogue.FindCandidateByPath(addedPath));
        Assert.Empty(formation.IntendedTargetCollisions);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<SourceCatalogue> ReadCatalogueAsync(
        SourceIntegrationWorkspace workspace)
    {
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace.Workspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);
        Assert.False(catalogue.IsCancelled);
        return catalogue;
    }

    private static async Task<MarkdownDocumentFacts> ReadDocumentAsync(
        SourceIntegrationWorkspace workspace,
        SourceLogicalSource source)
    {
        var result = await new SourceDocumentReader(workspace.Workspace).ReadAsync(
            source.Base,
            TestContext.Current.CancellationToken);
        var read = Assert.IsType<FileReadResult<string>>(result.Read);
        Assert.Equal(FileReadState.Complete, read.State);
        return new MarkdownDocumentParser().Parse(Assert.IsType<string>(read.Value));
    }
}
