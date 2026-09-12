using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationIntegrationTests
{
    [Fact(DisplayName = "Generated navigation reads a real rooted tree and projects Loader and entrypoint regions without writes")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task RootedTreeProjectsLoaderAndEntrypointRegionsWithoutWrites()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-rooted");
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
        workspace.Write(
            ".agents/root/child.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Child",
                tags: ["Docs"],
                body: "# Child\n"));
        workspace.Write(
            ".agents/root/tool/SKILL.md",
            OpenForgeDocumentSeed.Skill(
                name: "tool",
                description: "Native tool"));

        var formation = await ReadFormationAsync(workspace);
        var loader = RequireSource(formation, SourceLogicalPath.LoaderPath);
        var root = RequireSource(formation, ".agents/root/_root.md");
        var child = RequireSource(formation, ".agents/root/child.md");
        var skill = RequireSource(formation, ".agents/root/tool/SKILL.md");
        var reader = new SourceDocumentReader(workspace.Workspace);
        var loaderDocument = await ReadDocumentAsync(reader, loader);
        var rootDocument = await ReadDocumentAsync(reader, root);
        var rootFacts = await ReadMetadataAsync(reader, root);
        var childFacts = await ReadMetadataAsync(reader, child);
        var skillFacts = await ReadMetadataAsync(reader, skill);
        var request = new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions:
            [
                new GeneratedNavigationRegionInput(loader, loaderDocument),
                new GeneratedNavigationRegionInput(root, rootDocument),
            ],
            metadata: [rootFacts, childFacts, skillFacts]);
        var before = workspace.SnapshotHashes();

        var projection = new GeneratedNavigationProjector().Project(request);

        Assert.True(projection.IsComplete);
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, ".agents/root/_root.md"],
            projection.Regions.Select(region => region.CanonicalPath));
        Assert.Equal(
            "\n- [Root](root/_root.md) - #Root\n",
            projection.Regions[0].ExpectedBody);
        Assert.Equal(
            "\n- [Child](child.md) - #Docs\n- [Native tool](tool/SKILL.md) - #Skill\n",
            projection.Regions[1].ExpectedBody);
        Assert.Equal(["Skill"], projection.Regions[1].Entries[1].Tags);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Generated navigation maintains a detached CRLF entrypoint with aliases and an attached overwrite")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task DetachedTreePreservesCrLfAndPhysicalIdentity()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-detached");
        workspace.Write(
            ".agents/detached/_detached.md",
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "- [Alias](a.md) - #Alias\r\n- [Other](b.md) - #Other",
                LineEnding = "\r\n",
                Prefix = "# Detached",
            }));
        workspace.Write(
            "shared/child.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Alias",
                tags: ["Alias"],
                body: "# Child\n"));
        workspace.Write(
            ".agents/detached/b.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Other",
                tags: ["Other"],
                body: "# Child\n"));
        workspace.Write(".agents/detached/a.overwrite.md", "authored overwrite\r\n");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/detached/a.md",
                workspace.Absolute("shared/child.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/detached/z.md",
                workspace.Absolute("shared/child.md"),
                out _),
            "This integration case requires real symbolic-link support.");

        var formation = await ReadFormationAsync(workspace);
        var parent = RequireSource(formation, ".agents/detached/_detached.md");
        var firstAlias = RequireSource(formation, ".agents/detached/a.md");
        var secondAlias = RequireSource(formation, ".agents/detached/z.md");
        var other = RequireSource(formation, ".agents/detached/b.md");
        var reader = new SourceDocumentReader(workspace.Workspace);
        var parentDocument = await ReadDocumentAsync(reader, parent);
        var firstFacts = await ReadMetadataAsync(reader, firstAlias);
        var secondFacts = await ReadMetadataAsync(reader, secondAlias);
        var otherFacts = await ReadMetadataAsync(reader, other);
        var request = new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(parent, parentDocument)],
            metadata: [firstFacts, secondFacts, otherFacts]);
        var before = workspace.SnapshotHashes();

        var projection = new GeneratedNavigationProjector().Project(request);

        var region = Assert.Single(projection.Regions);
        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Equal(
            ["a.md", "b.md"],
            region.Entries.Select(entry => entry.Destination));
        Assert.Equal(
            "\r\n- [Alias](a.md) - #Alias\r\n- [Other](b.md) - #Other\r\n",
            region.ExpectedBody);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        Assert.True(change.IsUnchanged);
        Assert.NotNull(firstAlias.Overwrite);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Generated navigation keeps malformed markers and strict invalid UTF-8 as unavailable real-file facts")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Integration")]
    public async Task MalformedAndInvalidFilesNeverFormChanges()
    {
        using var workspace = SourceIntegrationWorkspace.Create("generated-navigation-failures");
        workspace.Write(
            ".agents/malformed/_malformed.md",
            "# Malformed\n\n## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "stale\n"
            + "<!-- open-forge:generated-index:end -->\n"
            + "<!-- open-forge:generated-index:end -->\n");
        workspace.Write(".agents/invalid/_invalid.md", [0xC3, 0x28]);
        var formation = await ReadFormationAsync(workspace);
        var malformed = RequireSource(formation, ".agents/malformed/_malformed.md");
        var invalid = RequireSource(formation, ".agents/invalid/_invalid.md");
        var reader = new SourceDocumentReader(workspace.Workspace);
        var malformedDocument = await ReadDocumentAsync(reader, malformed);
        var invalidReadResult = await reader.ReadAsync(
            invalid.Base,
            TestContext.Current.CancellationToken);
        var invalidRead = Assert.IsType<FileReadResult<string>>(invalidReadResult.Read);
        Assert.Equal(FileReadState.InvalidEncoding, invalidRead.State);
        var request = new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions:
            [
                new GeneratedNavigationRegionInput(malformed, malformedDocument),
                new GeneratedNavigationRegionInput(
                    invalid,
                    invalidRead.Failure?.DirectCause
                        ?? "The source read failed strict UTF-8 validation."),
            ],
            metadata: []);
        var before = workspace.SnapshotHashes();

        var projection = new GeneratedNavigationProjector().Project(request);

        Assert.Equal(2, projection.Regions.Count);
        Assert.All(
            projection.Regions,
            region =>
            {
                Assert.Equal(GeneratedNavigationRegionState.Unavailable, region.State);
                Assert.Empty(region.Entries);
                Assert.Null(region.Change);
                Assert.False(string.IsNullOrWhiteSpace(region.Cause));
            });
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<MarkdownDocumentFacts> ReadDocumentAsync(
        SourceDocumentReader reader,
        SourceLogicalSource source)
    {
        var result = await reader.ReadAsync(
            source.Base,
            TestContext.Current.CancellationToken);
        var read = Assert.IsType<FileReadResult<string>>(result.Read);
        Assert.Equal(FileReadState.Complete, read.State);
        return new MarkdownDocumentParser().Parse(
            Assert.IsType<string>(read.Value));
    }

    private static async Task<GeneratedNavigationMetadata> ReadMetadataAsync(
        SourceDocumentReader reader,
        SourceLogicalSource source)
    {
        var document = await ReadDocumentAsync(reader, source);
        var facts = new SourceAuthoredMetadataParser().Parse(document, source.Base.Form);
        Assert.Equal(SourceAuthoredMetadataState.Complete, facts.State);
        return new GeneratedNavigationMetadata(source, facts);
    }

    private static async Task<GeneratedNavigationFormation> ReadFormationAsync(
        SourceIntegrationWorkspace workspace)
    {
        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace.Workspace, [SourceLogicalPath.AgentsRoot]),
            TestContext.Current.CancellationToken);
        Assert.False(catalogue.IsCancelled);
        return new GeneratedNavigationFormationBuilder().Build(catalogue);
    }

    private static SourceLogicalSource RequireSource(
        GeneratedNavigationFormation formation,
        string canonicalPath)
    {
        var source = formation.FindSource(canonicalPath);
        Assert.NotNull(source);
        return source;
    }

}
