using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.RouteScaffolding;

[Trait("Boundary", "Processing"), Trait("Feature", "route-scaffolding"), Trait("Evidence", "Unit")]
public sealed class RouteScaffoldLayoutTests
{
    private const string CustomRoot = "docs/forge";
    private const string Category = WorkspaceCategoryNames.Guidance;
    private const string Team = "team";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string FrameworkId = "fixture-framework";
    private static readonly string CategoryEntry = SourceFormClassifier.CanonicalEntrypointFileName(Category);
    private static readonly string TeamEntry = SourceFormClassifier.CanonicalEntrypointFileName(Team);

    [Theory(DisplayName = "Scaffolding and adoption stop at the selected category rather than workspace depth")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void NestedRootsProduceTheSameSourceCoordinates(string mainRoot)
    {
        var layout = Layout(mainRoot);
        var directory = mainRoot + "/" + Category;
        var root = Source(directory + "/" + CategoryEntry, Category, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(directory + "/" + Team + "/" + Note, Category + "/" + Team + "/" + Path.GetFileNameWithoutExtension(Note), SourceDocumentForm.Markdown);
        var catalogue = new SourceCatalogue(layout, [], [root, leaf], [], false);
        var discovery = new RouteScaffoldChainDiscovery().Discover(new()
        {
            Catalogue = catalogue,
            RecognizedRootPaths = [root.Identity.CanonicalBasePath],
            EligibleSources = [leaf],
            Settings = WorkspaceSettingsDocument.Empty,
        });
        var gap = Assert.Single(discovery.Gaps);
        var expectedPath = directory + "/" + Team + "/" + TeamEntry;
        Assert.Equal(Category + "/" + Team, gap.FolderId);
        Assert.Equal(expectedPath, gap.CanonicalPath);
        var intended = new RouteScaffoldSourceBuilder().Create(layout, gap.CanonicalPath);
        Assert.Equal(Category + "/" + Team, intended.Identity.AutomaticId);
        Assert.Equal(expectedPath, RouteScaffoldSourceBuilder.CanonicalPath(layout, Category + "/" + Team));
        var replaced = new RouteScaffoldSourceBuilder().Replace(catalogue, [intended]);
        Assert.Same(layout, replaced.Catalogue.Layout);
        Assert.Same(catalogue, replaced.Catalogue);
        Assert.Empty(replaced.Topology.LoaderRootPaths);

        var adoption = WorkspaceAdoptionTopologyPlanner.Plan(layout, [leaf], [root], [], WorkspaceOwnershipRead.Absent(Physical(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName)));
        Assert.Null(adoption.Cause);
        Assert.Equal([expectedPath], adoption.CreatedEntrypointPaths);
        Assert.Equal([leaf.Identity.CanonicalBasePath], adoption.EligibleSourcePaths);
    }

    [Fact(DisplayName = "Metadata completion uses real layout categories and honors category exclusions")]
    public void MetadataEligibilityUsesLayout()
    {
        var layout = Layout(CustomRoot);
        var rootPath = CustomRoot + "/" + Category + "/" + CategoryEntry;
        var file = new RoutedMetadataFileFacts
        {
            CanonicalPath = CustomRoot + "/" + Category + "/" + Note,
            LoaderRootEntrypointPath = rootPath,
            RouteEntrypointPaths = [rootPath],
            IsSelected = true,
            IsReadable = true,
            IsSafeOrdinaryLeaf = true,
            IsGeneratedFile = false,
            HasManagedAuthoredRegions = false,
        };
        var policy = new RoutedMetadataPolicyContext { Layout = layout, Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty };
        var completion = new RoutedMetadataCompletionPolicy();
        Assert.Equal(RoutedMetadataEligibility.Eligible, completion.Evaluate(file, policy));
        Assert.Equal(RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Guidance), RoutedMetadataDefaults.Tags(policy.Roots, file));
        Assert.Equal(RoutedMetadataEligibility.SettingsExcluded, completion.Evaluate(file, policy with
        {
            Settings = policy.Settings with { RemovedCategories = [Category] },
        }));
        Assert.Equal(RoutedMetadataEligibility.UnsupportedForm, completion.Evaluate(file with
        {
            CanonicalPath = WorkspaceLayout.DefaultMainRoot + "/" + Category + "/" + Note,
        }, policy));
    }

    [Fact(DisplayName = "Persisted sharing records are structural and catalogue membership is layout specific")]
    public void SharingRoundtripAndMembershipAreSeparate()
    {
        var directory = CustomRoot + "/" + Category + "/" + Team;
        var entrypoint = directory + "/" + TeamEntry;
        var framework = new FrameworkOwnership(new(FrameworkId, null), [], []) { GitIgnoredRoutes = [new(directory, entrypoint)] };
        var serialized = WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with { Framework = framework });
        var decoded = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(serialized).Document);
        Assert.Equal(framework.GitIgnoredRoutes, Assert.IsType<FrameworkOwnership>(decoded.Framework).GitIgnoredRoutes);
        Assert.Empty(decoded.ManagedPaths());
        var sharing = new SourceSharing(framework.GitIgnoredRoutes);
        var source = Source(entrypoint, Category + "/" + Team, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(directory + "/" + Note, Category + "/" + Team + "/" + Path.GetFileNameWithoutExtension(Note), SourceDocumentForm.Markdown);
        var catalogue = new SourceCatalogue(Layout(CustomRoot), [], [source, leaf], [], false);
        var projected = sharing.Project(catalogue);
        Assert.Same(source, Assert.Single(projected.Sources));
        Assert.Same(catalogue.Layout, projected.Layout);
        var other = new SourceCatalogue(Layout(WorkspaceLayout.DefaultMainRoot), [], [], [], false);
        Assert.Equal(entrypoint, sharing.FindUnavailableEntrypoint(other));
        Assert.Throws<ArgumentException>(() => sharing.Project(other));
    }

    private static WorkspaceLayout Layout(string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);

    private static SourceLogicalSource Source(string path, string id, SourceDocumentForm form)
        => new(new SourceLogicalIdentity(id, path), new SourceLayer(path, Physical(path), form, SourceLayerKind.Base));

    private static string Physical(string path) => Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
}
