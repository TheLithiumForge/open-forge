using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexProjectionIntegrationTests
{
    private const string RootPath = ".agents/root/_root.md";

    [Fact(DisplayName = "Index projection uses base metadata, ignores overwrite metadata, and preserves workspace bytes")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RealProjectionUsesBaseMetadataAndPreservesWorkspaceBytes()
    {
        using var workspace = TemporaryWorkspace.Create("index-projection-base");
        workspace.WriteText(RootPath, OpenForgeDocumentSeed.GeneratedEntries(entries: "stale"));
        workspace.WriteText(
            ".agents/root/child.md",
            OpenForgeDocumentSeed.Metadata("Base child", ["Docs"], "# Child\n"));
        workspace.WriteText(
            ".agents/root/child.overwrite.md",
            OpenForgeDocumentSeed.Metadata("Overwrite child", ["Wrong"], "# Overwrite\n"));
        var context = await CreateContextAsync(workspace);
        var before = workspace.SnapshotHashes();

        var result = await new IndexProjectionBuilder().BuildAsync(
            context,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsComplete);
        var region = Assert.Single(result.Projection.Regions);
        var projectedRegion = Assert.Single(result.Regions);
        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Same(region, projectedRegion.Region);
        Assert.Null(projectedRegion.BeforeEntryCount);
        Assert.Equal(1, projectedRegion.ExpectedEntryCount);
        Assert.Equal("- [Base child](child.md) - #Docs", Assert.Single(region.Entries).Line);
        Assert.DoesNotContain("Overwrite", region.ExpectedBody, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Index projection counts a wholly parseable generated Entries interior on real files")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RealProjectionRetainsTheAuthoritativeBeforeEntryCount()
    {
        using var workspace = TemporaryWorkspace.Create("index-projection-entry-count");
        workspace.WriteText(
            RootPath,
            OpenForgeDocumentSeed.GeneratedEntries(
                "- [Old alpha](old-alpha.md) - #Docs\n- [Old beta](old-beta.md) - #Docs"));
        workspace.WriteText(
            ".agents/root/child.md",
            OpenForgeDocumentSeed.Metadata("Child", ["Docs"], "# Child\n"));
        var context = await CreateContextAsync(workspace);

        var result = await new IndexProjectionBuilder().BuildAsync(
            context,
            TestContext.Current.CancellationToken);

        var region = Assert.Single(result.Regions);
        Assert.Equal(2, region.BeforeEntryCount);
        Assert.Equal(1, region.ExpectedEntryCount);
    }

    [Fact(DisplayName = "Index projection distinguishes missing metadata from invalidly encoded metadata on real files")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RealMetadataFailuresKeepMissingAndUnsafeMeaningsDistinct()
    {
        using var missingWorkspace = TemporaryWorkspace.Create("index-projection-missing-metadata");
        missingWorkspace.WriteText(RootPath, OpenForgeDocumentSeed.GeneratedEntries(entries: "stale"));
        missingWorkspace.WriteText(".agents/root/child.md", "# Child\n");
        var missingContext = await CreateContextAsync(missingWorkspace);

        var missing = await new IndexProjectionBuilder().BuildAsync(
            missingContext,
            TestContext.Current.CancellationToken);

        Assert.Equal(IndexFindingCode.MetadataIncomplete, Assert.Single(missing.Findings).Code);

        using var invalidWorkspace = TemporaryWorkspace.Create("index-projection-invalid-metadata");
        invalidWorkspace.WriteText(RootPath, OpenForgeDocumentSeed.GeneratedEntries(entries: "stale"));
        invalidWorkspace.WriteText(
            ".agents/root/child.md",
            OpenForgeDocumentSeed.Metadata("Child", ["Docs"], "# Child\n"));
        var invalidContext = await CreateContextAsync(invalidWorkspace);
        invalidWorkspace.ReplaceBytes(".agents/root/child.md", [0xC3, 0x28]);
        var before = invalidWorkspace.SnapshotHashes();

        var invalid = await new IndexProjectionBuilder().BuildAsync(
            invalidContext,
            TestContext.Current.CancellationToken);

        Assert.Equal(IndexFindingCode.MetadataUnsafe, Assert.Single(invalid.Findings).Code);
        Assert.Equal(before, invalidWorkspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Index projection rejects a missing generated region without changing real workspace bytes")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task MissingGeneratedRegionIsUnsafeAndReadOnly()
    {
        using var workspace = TemporaryWorkspace.Create("index-projection-missing-region");
        workspace.WriteText(RootPath, "# Root\n");
        workspace.WriteText(
            ".agents/root/child.md",
            OpenForgeDocumentSeed.Metadata("Child", ["Docs"], "# Child\n"));
        var context = await CreateContextAsync(workspace);
        var before = workspace.SnapshotHashes();

        var result = await new IndexProjectionBuilder().BuildAsync(
            context,
            TestContext.Current.CancellationToken);

        Assert.Equal(IndexFindingCode.GeneratedRegionUnsafe, Assert.Single(result.Findings).Code);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<IndexProjectionContext> CreateContextAsync(TemporaryWorkspace workspace)
    {
        var cliWorkspace = new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(cliWorkspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var selection = CreateSelection(cliWorkspace, formation);
        Assert.True(selection.IsComplete);
        return new IndexProjectionContext
        {
            Formation = formation,
            Selection = selection,
            Reader = new SourceDocumentReader(cliWorkspace),
        };
    }

    private static IndexSelectionResolution CreateSelection(
        CliWorkspace workspace,
        GeneratedNavigationFormation formation)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var referenceResolver = new SourceReferenceResolver((currentWorkspace, canonicalPath) =>
            physicalPathResolver.ResolveCandidate(
                currentWorkspace.LexicalRoot,
                currentWorkspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(currentWorkspace.LexicalRoot, canonicalPath)));
        return new IndexSelectionResolver(referenceResolver).Resolve(
            new IndexRequest(workspace, [RootPath], IndexMode.Apply),
            formation);
    }
}
