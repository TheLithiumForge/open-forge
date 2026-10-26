using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

[Trait("Boundary", "Processing"), Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
public sealed class GeneratedNavigationLayoutTests
{
    private const string CustomRoot = "docs/forge";
    private const string Category = "guide";
    private const string Outside = "project/guide" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "Guide purpose";
    private static readonly string CategoryEntry = Category + "/" + SourceFormClassifier.CanonicalEntrypointFileName(Category);
    private static readonly string SkillsEntry = WorkspaceLayout.NativeSkillsPath + "/" + SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceLayoutNames.SkillsDirectoryName);
    private static readonly string Metadata = $"---\nopen-forge:\n  description: {Description}\n  tags: [{SourceLoadingClosureResolver.LoadNowTag}]\n---\n";

    [Theory(DisplayName = "Navigation roots follow the selected layout and keep native Skills rooted")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void StructuralRootsUseBothContentLocations(string mainRoot)
    {
        var layout = Layout(mainRoot);
        var loader = Source(mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, SourceDocumentForm.Loader);
        var root = Source(mainRoot + "/" + CategoryEntry, SourceDocumentForm.CanonicalEntrypoint);
        var skills = Source(SkillsEntry, SourceDocumentForm.CanonicalEntrypoint);
        var external = Source(Outside, SourceDocumentForm.Markdown);
        var catalogue = new SourceCatalogue(layout, [], [loader, root, skills, external], [], false);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);

        Assert.Same(loader, formation.Loader);
        Assert.Equal(new[] { root.Identity.CanonicalBasePath, SkillsEntry }.Order(StringComparer.Ordinal), formation.Topology.LoaderRootPaths);
        Assert.Empty(formation.Catalogue.Candidates);
        Assert.Same(external, formation.FindSource(Outside));
        Assert.Empty(formation.IntendedTargetCollisions);
    }

    [Fact(DisplayName = "Custom loader projection emits native Skills once and keeps authored external rows")]
    public void NativeSkillsAreGeneratedWithoutDuplicatingAuthoredRows()
    {
        var layout = Layout(CustomRoot);
        var loaderPath = CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var loader = Source(loaderPath, SourceDocumentForm.Loader);
        var root = Source(CustomRoot + "/" + CategoryEntry, SourceDocumentForm.CanonicalEntrypoint);
        var skills = Source(SkillsEntry, SourceDocumentForm.CanonicalEntrypoint);
        var catalogue = new SourceCatalogue(layout, [], [loader, root, skills], [], false);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var skillDestination = "../../" + SkillsEntry;
        var externalDestination = "../../" + Outside;
        var text = $"# Loader\n\n## Entries\n\n- [{Description}]({skillDestination})\n- [{Description}]({externalDestination})\n";
        var parser = new MarkdownDocumentParser();
        var document = parser.Parse(text);
        var facts = new SourceAuthoredMetadataParser().Parse(parser.Parse(Metadata), SourceDocumentForm.CanonicalEntrypoint);
        var request = new GeneratedNavigationProjectionRequest(formation, [new(loader, document)], [new(root, facts), new(skills, facts)])
        {
            PreserveLoaderMembership = true,
        };
        var region = new GeneratedNavigationRegionPlanner().Plan(request, request.Regions[0]);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Equal(SkillsEntry, Assert.Single(region.Entries).CanonicalPath);
        Assert.Equal(skillDestination, region.Entries[0].Destination);
        Assert.Equal(1, region.AuthoredLinkCount);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        var rewritten = parser.Parse(System.Text.Encoding.UTF8.GetString(change.ExpectedDocumentBytes.AsSpan()));
        var destinations = rewritten.Links.Select(link => link.RawDestination).ToArray();
        Assert.Equal([skillDestination, externalDestination], destinations);
        Assert.DoesNotContain(root.Identity.CanonicalBasePath, region.Entries.Select(entry => entry.CanonicalPath));
    }

    private static WorkspaceLayout Layout(string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);

    private static SourceLogicalSource Source(string path, SourceDocumentForm form)
        => new(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, path),
            new SourceLayer(path, Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar)), form, SourceLayerKind.Base));
}
