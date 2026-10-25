using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.RouteScaffolding;

[Trait("Feature", "route-scaffolding"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteScaffoldOutsideTests
{
    private const string Route = "docs/team";
    private const string Other = "reference";
    private const string CustomRoot = "knowledge/main";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Selected outside scaffolding restores its anchor and stops there with exact catalogue identities")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void MissingAnchorAndBoundedChain(string mainRoot)
    {
        var layout = Layout(mainRoot);
        var builder = new RouteScaffoldSourceBuilder();
        var note = builder.Create(layout, Route + "/nested/deep/" + Note);
        var unrelated = builder.Create(layout, Other + "/nested/" + Note);
        var catalogue = new SourceCatalogue(layout, [], [note, unrelated], [], isCancelled: false);
        var request = new RouteScaffoldDiscoveryRequest
        {
            Catalogue = catalogue,
            RecognizedRootPaths = [WorkspaceRoutePath.CanonicalEntrypointPath(Route)],
            EligibleSources = [note, unrelated],
            Settings = WorkspaceSettingsDocument.Empty,
        };

        var discovery = new RouteScaffoldChainDiscovery().Discover(request);

        Assert.Empty(discovery.Boundaries);
        var expected = new[] { Route, Route + "/nested", Route + "/nested/deep" }.Select(WorkspaceRoutePath.CanonicalEntrypointPath).ToArray();
        Assert.Equal(expected, discovery.Gaps.Select(gap => gap.CanonicalPath));
        Assert.Equal(expected.Select(path => SourceWorkspaceRelativePath.LiteralReferencePrefix + path), discovery.Gaps.Select(gap => gap.FolderId));
        Assert.Equal(expected[0], RouteScaffoldSourceBuilder.CanonicalPath(layout, SourceWorkspaceRelativePath.LiteralReferencePrefix + Route));
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request with { RecognizedRootPaths = [] }).Gaps);
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request with
        {
            RecognizedRootPaths = [WorkspaceRoutePath.CanonicalEntrypointPath(Route.ToUpperInvariant())],
        }).Gaps);
        Assert.Throws<ArgumentException>(() => RouteScaffoldSourceBuilder.CanonicalPath(layout, SourceWorkspaceRelativePath.LiteralReferencePrefix + "unregistered"));
    }

    [Theory(DisplayName = "Outside category names and nested state folders keep generic metadata defaults")]
    [InlineData(WorkspaceCategoryNames.Directives), InlineData(WorkspaceCategoryNames.Memory), InlineData(WorkspaceLayoutNames.SkillsDirectoryName)]
    public void OutsideDefaultsAreGeneric(string category)
    {
        var layout = Layout(CustomRoot).WithOutsideRoutes([category]);
        var file = Facts(category + "/scope/working/" + SourceFormClassifier.NativeSkillFileName) with
        {
            LoaderRootEntrypointPath = WorkspaceRoutePath.CanonicalEntrypointPath(category),
            RouteEntrypointPaths = [WorkspaceRoutePath.CanonicalEntrypointPath(category + "/scope/working")],
        };
        Assert.Equal(RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Workspace), RoutedMetadataDefaults.Tags(layout, file));
        var context = new RoutedMetadataPolicyContext { Layout = layout, Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty };
        Assert.Equal(RoutedMetadataEligibility.OutsideRoot, new RoutedMetadataCompletionPolicy().Evaluate(file, context));
        Assert.False(layout.IsImplicitContentDestination(file.CanonicalPath));
    }

    [Fact(DisplayName = "Outside compatibility entrypoints and removed anchors preserve chain refusal rules")]
    public void CompatibilityAndRemoval()
    {
        var layout = Layout(CustomRoot);
        var builder = new RouteScaffoldSourceBuilder();
        var compatibility = builder.Create(layout, Route + "/" + SourceFormClassifier.IndexFileName);
        var note = builder.Create(layout, Route + "/nested/" + Note);
        var request = new RouteScaffoldDiscoveryRequest
        {
            Catalogue = new SourceCatalogue(layout, [], [compatibility, note], [], isCancelled: false),
            RecognizedRootPaths = [compatibility.Identity.CanonicalBasePath],
            EligibleSources = [note],
            Settings = WorkspaceSettingsDocument.Empty,
        };
        Assert.Equal(WorkspaceRoutePath.CanonicalEntrypointPath(Route + "/nested"), Assert.Single(new RouteScaffoldChainDiscovery().Discover(request).Gaps).CanonicalPath);
        var canonical = builder.Create(layout, WorkspaceRoutePath.CanonicalEntrypointPath(Route));
        var ambiguous = request with { Catalogue = new SourceCatalogue(layout, [], [compatibility, canonical, note], [], isCancelled: false) };
        Assert.Contains(new RouteScaffoldChainDiscovery().Discover(ambiguous).Boundaries,
            boundary => boundary.Kind == RouteScaffoldBoundaryKind.EntrypointAmbiguous && boundary.Path == Route);
        Assert.Empty(new RouteScaffoldChainDiscovery().Discover(request with
        {
            Settings = WorkspaceSettingsDocument.Empty with { RemovedDirectories = [Route] },
        }).Gaps);
    }

    private static RoutedMetadataFileFacts Facts(string path) => new()
    {
        CanonicalPath = path,
        LoaderRootEntrypointPath = WorkspaceRoutePath.CanonicalEntrypointPath(Route),
        RouteEntrypointPaths = [WorkspaceRoutePath.CanonicalEntrypointPath(Route + "/memory/scope/working")],
        IsSelected = true,
        IsReadable = true,
        IsSafeOrdinaryLeaf = true,
        IsGeneratedFile = false,
        HasManagedAuthoredRegions = false,
    };

    private static WorkspaceLayout Layout(string mainRoot)
    {
        var root = Path.GetFullPath(nameof(RouteScaffoldOutsideTests));
        return WorkspaceLayout.Create(new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot).WithOutsideRoutes([Route, Other]);
    }
}
