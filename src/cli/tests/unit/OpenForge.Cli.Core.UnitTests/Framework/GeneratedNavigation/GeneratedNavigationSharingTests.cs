using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

[Trait("Feature", "workspace-route-sharing"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class GeneratedNavigationSharingTests
{
    [Fact(DisplayName = "All shared region planners omit private rows while retaining explicitly supplied region targets")]
    public void SharedChildrenAreFilteredInDirectAndWholeProjection()
    {
        var parent = GeneratedNavigationTestData.Source(".agents/memory/_memory.md", SourceDocumentForm.CanonicalEntrypoint);
        var shared = GeneratedNavigationTestData.Source(".agents/memory/working/_working.md", SourceDocumentForm.CanonicalEntrypoint);
        var nested = GeneratedNavigationTestData.Source(".agents/memory/working/nested/_nested.md", SourceDocumentForm.CanonicalEntrypoint);
        var note = GeneratedNavigationTestData.Source(".agents/memory/working/nested/note.md", SourceDocumentForm.Markdown);
        var sources = new[] { parent, shared, nested, note };
        var formation = new GeneratedNavigationFormationBuilder().Build(GeneratedNavigationTestData.Catalogue(sources));
        var parser = new MarkdownDocumentParser();
        var regions = new[] { parent, shared, nested }.Select(source => new GeneratedNavigationRegionInput(source,
            parser.Parse(OpenForgeDocumentSeed.GeneratedEntries("- stale")))).ToArray();
        var metadata = new[] { new GeneratedNavigationMetadata(shared, new SourceAuthoredMetadataParser().Parse(
            parser.Parse(OpenForgeDocumentSeed.Metadata("Working", ["Memory"], "# Working\n")), shared.Base.Form)) };
        var request = new GeneratedNavigationProjectionRequest(formation, regions, metadata,
            new SourceSharing([new(".agents/memory/working", shared.Identity.CanonicalBasePath)]));
        var projection = new GeneratedNavigationProjector().Project(request);
        Assert.True(projection.IsComplete);
        Assert.Equal(3, projection.Regions.Count);
        var parentRegion = projection.Regions.Single(region => region.CanonicalPath == parent.Identity.CanonicalBasePath);
        Assert.Equal(shared.Identity.CanonicalBasePath, Assert.Single(parentRegion.Entries).CanonicalPath);
        Assert.All(projection.Regions.Where(region => region.CanonicalPath != parent.Identity.CanonicalBasePath), region => Assert.Empty(region.Entries));
        var direct = new GeneratedNavigationRegionPlanner().Plan(request, regions[1]);
        Assert.Equal(GeneratedNavigationRegionState.Available, direct.State);
        Assert.Empty(direct.Entries);
    }
}
