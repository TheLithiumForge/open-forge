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
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexProjectionIntegrationTests
{
    private const string RootPath = ".agents/root/_root.md";

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Index projection uses base metadata, ignores overwrite metadata, and preserves workspace bytes")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RealProjectionUsesBaseMetadataAndPreservesWorkspaceBytes()
    {
        using var workspace = TemporaryWorkspace.Create("index-projection-base");
        workspace.WriteText(RootPath, OpenForgeDocumentSeed.GeneratedEntries(entries: "- stale"));
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

    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Index projection keeps ordinary missing metadata optional and invalid encoding unsafe on real files")]
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

        Assert.True(missing.IsComplete);
        Assert.Equal(IndexFindingCode.MetadataOptional, Assert.Single(missing.Findings).Code);
        Assert.Equal("- [root/child](child.md)", Assert.Single(Assert.Single(missing.Projection.Regions).Entries).Line);

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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Index projection independently skips readable malformed native Skill but refuses malformed-only")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task ReadableMalformedNativeSkillRequiresAnIndependentUpdate()
    {
        using var independentWorkspace = TemporaryWorkspace.Create("index-projection-native-skill-independent");
        WriteTarget(
            independentWorkspace,
            targetPath: ".agents/alpha/_alpha.md",
            childPath: ".agents/alpha/child.md",
            description: "Alpha Child",
            tag: "Alpha");
        independentWorkspace.WriteText(
            ".agents/beta/_beta.md",
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "stale",
                Prefix = "# Beta",
            }));
        independentWorkspace.WriteText(
            ".agents/beta/native/SKILL.md",
            "---\nname: beta-native\ndescription: [\n---\n# Native\n");
        var independentBefore = independentWorkspace.SnapshotHashes();
        var independent = await new IndexProjectionBuilder().BuildAsync(
            await CreateContextAsync(
                independentWorkspace,
                ".agents/alpha/_alpha.md",
                ".agents/beta/_beta.md"),
            TestContext.Current.CancellationToken);

        Assert.False(independent.IsComplete);
        Assert.True(independent.IsExecutable);
        var skipped = Assert.Single(independent.Findings);
        Assert.Equal(IndexFindingCode.MetadataSkipped, skipped.Code);
        Assert.Equal(".agents/beta/_beta.md", skipped.Details?.ParentPath);
        Assert.Equal(".agents/beta/native/SKILL.md", skipped.Source?.Path);
        Assert.Collection(
            independent.Regions,
            alpha =>
            {
                Assert.Equal(".agents/alpha/_alpha.md", alpha.Source.Path);
                Assert.Equal(GeneratedNavigationRegionState.Available, alpha.Region.State);
                Assert.Equal(GeneratedNavigationChangeKind.Update, alpha.Region.Change?.Kind);
                Assert.Equal(
                    "- [Alpha Child](child.md) - #Alpha",
                    Assert.Single(alpha.Region.Entries).Line);
            },
            beta =>
            {
                Assert.Equal(".agents/beta/_beta.md", beta.Source.Path);
                Assert.Equal(GeneratedNavigationRegionState.Unavailable, beta.Region.State);
            });
        Assert.Equal(independentBefore, independentWorkspace.SnapshotHashes());

        independentWorkspace.ReplaceText(".agents/beta/native/SKILL.md", "# Native without required metadata\n");
        var missingBefore = independentWorkspace.SnapshotHashes();
        var missing = await new IndexProjectionBuilder().BuildAsync(
            await CreateContextAsync(independentWorkspace, ".agents/alpha/_alpha.md", ".agents/beta/_beta.md"),
            TestContext.Current.CancellationToken);
        Assert.False(missing.IsExecutable);
        Assert.Contains(missing.Findings, finding => finding.Code == IndexFindingCode.MetadataIncomplete);
        Assert.Equal(missingBefore, independentWorkspace.SnapshotHashes());

        using var strictWorkspace = TemporaryWorkspace.Create("index-projection-native-skill-strict");
        strictWorkspace.WriteText(
            RootPath,
            OpenForgeDocumentSeed.GeneratedEntries(entries: "stale"));
        strictWorkspace.WriteText(
            ".agents/root/native/SKILL.md",
            "---\nname: root-native\ndescription: [\n---\n# Native\n");
        var strictBefore = strictWorkspace.SnapshotHashes();
        var strict = await new IndexProjectionBuilder().BuildAsync(
            await CreateContextAsync(strictWorkspace),
            TestContext.Current.CancellationToken);

        Assert.False(strict.IsComplete);
        Assert.False(strict.IsExecutable);
        Assert.Equal(IndexFindingCode.MetadataUnsafe, Assert.Single(strict.Findings).Code);
        Assert.Equal(strictBefore, strictWorkspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
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

    private static async Task<IndexProjectionContext> CreateContextAsync(
        TemporaryWorkspace workspace,
        params string[] sourcePaths)
    {
        var cliWorkspace = new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(cliWorkspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var selection = CreateSelection(
            cliWorkspace,
            formation,
            sourcePaths.Length == 0 ? [RootPath] : sourcePaths);
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
        GeneratedNavigationFormation formation,
        IReadOnlyList<string> sourcePaths)
    {
        var physicalPathResolver = new PhysicalPathResolver();
        var referenceResolver = new SourceReferenceResolver((currentWorkspace, canonicalPath) =>
            physicalPathResolver.ResolveCandidate(
                currentWorkspace.LexicalRoot,
                currentWorkspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(currentWorkspace.LexicalRoot, canonicalPath)));
        return new IndexSelectionResolver(referenceResolver).Resolve(
            new IndexRequest(workspace, sourcePaths, IndexMode.Apply),
            formation);
    }

    private static void WriteTarget(
        TemporaryWorkspace workspace,
        string targetPath,
        string childPath,
        string description,
        string tag)
    {
        workspace.WriteText(
            targetPath,
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "stale",
                Prefix = $"# {description}",
            }));
        workspace.WriteText(
            childPath,
            OpenForgeDocumentSeed.Metadata(
                description,
                [tag],
                $"# {description}\n"));
    }
}
