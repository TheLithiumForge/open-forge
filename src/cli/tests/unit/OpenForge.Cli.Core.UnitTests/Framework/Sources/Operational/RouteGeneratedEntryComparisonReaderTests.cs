using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

[Trait("Boundary", "Processing"), Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
public sealed class RouteGeneratedEntryComparisonReaderTests
{
    private const string CustomRoot = "handbook/context";
    private const string Description = "Native Skills catalogue";
    private const string PreviousDescription = "Previous catalogue purpose";
    private const string ExternalPath = "project/guide" + SourceFormClassifier.MarkdownExtension;

    public static IEnumerable<object[]> NativeCatalogueLayouts()
    {
        string[] filenames = [SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceLayoutNames.SkillsDirectoryName),
            SourceFormClassifier.IndexFileName, SourceFormClassifier.UnderscoreIndexFileName,
            SourceFormClassifier.ReferencesFileName, SourceFormClassifier.UnderscoreReferencesFileName];
        foreach (var mainRoot in new[] { WorkspaceLayout.DefaultMainRoot, CustomRoot })
            foreach (var filename in filenames)
                foreach (var staleDescription in new[] { false, true })
                    yield return [mainRoot, filename, staleDescription];
    }

    [Theory(DisplayName = "Native Skills row comparison recognizes current rows and stale descriptions across names and layouts")]
    [MemberData(nameof(NativeCatalogueLayouts))]
    public void GeneratedNativeCatalogueRowComparisonRetainsMetadataChecks(string mainRoot, string filename, bool staleDescription)
    {
        var layout = WorkspaceLayout.Create(
            new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        var loader = Source(layout, layout.LoaderPath);
        var skills = Source(layout, WorkspaceLayout.NativeSkillsPath + "/" + filename);
        var catalogue = new SourceCatalogue(layout, [], [loader, skills], [], false);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var destination = Path.GetRelativePath(SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, mainRoot),
            skills.Base.PhysicalPath).Replace('\\', '/');
        var externalDestination = Path.GetRelativePath(SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, mainRoot),
            SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, ExternalPath)).Replace('\\', '/');
        var rowDescription = staleDescription ? PreviousDescription : Description;
        var row = $"- [{rowDescription}]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag}\n- [Project guide]({externalDestination})";
        var parser = new MarkdownDocumentParser();
        var document = parser.Parse(OpenForgeDocumentSeed.GeneratedEntries(row));
        var input = new GeneratedNavigationRegionInput(loader, document);
        var request = new GeneratedNavigationProjectionRequest(formation, [input],
            [new(skills, SourceAuthoredMetadataFacts.Complete(Description, [SourceLoadingClosureResolver.LoadNowTag]))])
        {
            PreserveLoaderMembership = true,
        };

        var region = new GeneratedNavigationRegionPlanner().Plan(request, input);

        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        Assert.Equal(skills.Identity.CanonicalBasePath, Assert.Single(region.Entries).CanonicalPath);
        var change = Assert.IsType<GeneratedNavigationBoundedChange>(region.Change);
        Assert.Equal(!staleDescription, change.IsUnchanged);
        Assert.Equal(1, region.AuthoredLinkCount);
        var generated = parser.Parse(Encoding.UTF8.GetString(change.ExpectedDocumentBytes.AsSpan()));
        Assert.Equal([destination, externalDestination], generated.Links.Select(link => link.RawDestination));
        var comparisons = RouteGeneratedEntryComparisonReader.Read(layout, loader,
            SourceGeneratedEntriesParser.Parse(document), region.Entries, new Utf8SourceMap(document.Source));

        if (staleDescription)
            Assert.Equal(RouteGeneratedEntryComparisonKind.Description, Assert.Single(comparisons).Kind);
        else
            Assert.Empty(comparisons);
    }

    private static SourceLogicalSource Source(WorkspaceLayout layout, string path)
    {
        Assert.True(SourceFormClassifier.TryClassify(layout, path, out var form));
        var id = SourceIdentity.DeriveId(layout, path) ?? throw new InvalidOperationException("The fixture source must have an identity.");
        return new(new SourceLogicalIdentity(id, path),
            new SourceLayer(path, SourceLogicalPath.ToLexicalPath(layout.Workspace.PhysicalRoot, path), form, SourceLayerKind.Base));
    }
}
