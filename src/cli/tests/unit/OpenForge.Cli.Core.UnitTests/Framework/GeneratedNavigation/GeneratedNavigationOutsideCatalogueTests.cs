using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

[Trait("Boundary", "Processing"), Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
public sealed class GeneratedNavigationOutsideCatalogueTests
{
    private const string Route = "docs";
    private const string Scope = Route + "/scope";
    private const string FirstTag = "Alpha";
    private const string AddedTag = "Gamma";
    private const string AuthoredTag = "Authored";
    private const string LeavingDescription = "Project readme";
    private static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Route);
    private static readonly string ScopeAnchor = WorkspaceRoutePath.CanonicalEntrypointPath(Scope);
    private static readonly string Leaf = Route + "/file" + SourceFormClassifier.MarkdownExtension;
    private static readonly string LeavingTarget = "readme" + SourceFormClassifier.MarkdownExtension;
    private static readonly string LeavingDestination = "../" + LeavingTarget;
    private static readonly string LeavingRow = $"- [{LeavingDescription}]({LeavingDestination}) - #{AuthoredTag}";

    [Theory(DisplayName = "An outside catalogue rebuilds its routed children's rows from current metadata, keeps authored links and then stays current")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public void RoutedChildRowsFollowCurrentMetadata(string mainRoot)
    {
        var tree = new OutsideTree(mainRoot);
        var initial = OpenForgeDocumentSeed.GeneratedEntries(LeavingRow);
        var first = tree.Plan(initial, Revision.First);
        var firstText = Expected(first);

        var refreshed = tree.Plan(firstText, Revision.Second);
        var refreshedText = Expected(refreshed);
        Assert.True(Assert.IsType<GeneratedNavigationBoundedChange>(refreshed.Change).RequiresUpdate);
        Assert.Equal(1, refreshed.AuthoredLinkCount);
        var document = new MarkdownDocumentParser().Parse(refreshedText);
        var entries = SourceGeneratedEntriesParser.Parse(document).Entries;
        foreach (var child in new[] { Leaf, ScopeAnchor })
        {
            var row = Assert.Single(entries, entry => entry.Destination == Destination(child));
            Assert.Equal(Description(child, Revision.Second), row.Description);
            Assert.Equal(Tags(Revision.Second), row.Tags);
        }

        var authored = Assert.Single(entries, entry => entry.Destination == LeavingDestination);
        Assert.Equal(LeavingRow, document.Source[authored.Span.Start..authored.Span.End]);
        CommandOutputSnapshot.MatchSnapshot(refreshedText, "refreshed-catalogue");

        var repeated = tree.Plan(refreshedText, Revision.Second);
        Assert.True(Assert.IsType<GeneratedNavigationBoundedChange>(repeated.Change).IsUnchanged);
    }

    [Fact(DisplayName = "A row inside the folder that reaches a routed child through a proven alias is rebuilt, while an aliased link leaving the folder stays authored")]
    public void AliasRowIsRebuilt()
    {
        var alias = Route + "/alias/file" + SourceFormClassifier.MarkdownExtension;
        var tree = new OutsideTree(WorkspaceLayout.DefaultMainRoot, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [alias] = Leaf,
            [LeavingTarget] = Leaf,
        });
        var aliasRow = $"- [{Description(Leaf, Revision.First)}]({Destination(alias)}) - #{FirstTag}";
        var region = tree.Plan(OpenForgeDocumentSeed.GeneratedEntries($"{aliasRow}\n{LeavingRow}"), Revision.Second);

        Assert.Equal(1, region.AuthoredLinkCount);
        var entries = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(Expected(region))).Entries;
        Assert.DoesNotContain(entries, entry => entry.Destination == Destination(alias));
        Assert.Single(entries, entry => entry.Destination == LeavingDestination);
        Assert.Equal(Description(Leaf, Revision.Second), Assert.Single(entries, entry => entry.Destination == Destination(Leaf)).Description);
    }

    private static string Expected(GeneratedNavigationRegion region)
    {
        Assert.Equal(GeneratedNavigationRegionState.Available, region.State);
        return Encoding.UTF8.GetString(Assert.IsType<GeneratedNavigationBoundedChange>(region.Change).ExpectedDocumentBytes.AsSpan());
    }

    private static string Destination(string child) => child[(Route.Length + 1)..];

    private static string Description(string child, Revision revision) => $"{revision} {Path.GetFileNameWithoutExtension(child)} description";

    private static string[] Tags(Revision revision) => revision == Revision.First ? [FirstTag] : [FirstTag, AddedTag];

    private enum Revision
    {
        First,
        Second,
    }

    private sealed class OutsideTree
    {
        private readonly SourceLogicalSource _anchor;
        private readonly SourceLogicalSource[] _children;
        private readonly GeneratedNavigationFormation _formation;

        internal OutsideTree(string mainRoot, IReadOnlyDictionary<string, string>? aliases = null)
        {
            var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
            var layout = WorkspaceLayout.Create(workspace, mainRoot).WithOutsideRoutes([Route]);
            _anchor = Source(layout, Anchor, SourceDocumentForm.CanonicalEntrypoint);
            _children = [Source(layout, Leaf, SourceDocumentForm.Markdown), Source(layout, ScopeAnchor, SourceDocumentForm.CanonicalEntrypoint)];
            SourceLogicalSource[] sources = [Source(layout, layout.LoaderPath, SourceDocumentForm.Loader), _anchor, .. _children];
            _formation = new GeneratedNavigationFormationBuilder().Build(new SourceCatalogue(layout, [], sources, [], false, aliases));
        }

        internal GeneratedNavigationRegion Plan(string catalogue, Revision revision)
        {
            var parser = new MarkdownDocumentParser();
            var metadata = _children.Select(child => new GeneratedNavigationMetadata(child, new SourceAuthoredMetadataParser().Parse(
                parser.Parse(OpenForgeDocumentSeed.MetadataFrontmatter(Description(child.Base.CanonicalPath, revision), Tags(revision))), child.Base.Form)));
            var request = new GeneratedNavigationProjectionRequest(_formation, [new(_anchor, parser.Parse(catalogue))], metadata);
            return new GeneratedNavigationRegionPlanner().Plan(request, request.Regions[0]);
        }

        private static SourceLogicalSource Source(WorkspaceLayout layout, string path, SourceDocumentForm form)
            => new(new SourceLogicalIdentity(SourceIdentity.DeriveId(layout, path) ?? throw new InvalidOperationException(), path),
                new SourceLayer(path, Path.Combine(layout.Workspace.PhysicalRoot, path.Replace('/', Path.DirectorySeparatorChar)), form, SourceLayerKind.Base));
    }
}
