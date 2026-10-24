using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Sources;

[Trait("Feature", "framework-payload-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class FrameworkPayloadLayoutTests
{
    private const string CustomRoot = "docs/context";
    private const string Category = "notes";
    private const string Child = "working";
    private const string Scope = "team";
    private const string Skill = "sample";
    private const string Resource = "data.bin";
    private const string Entrypoint = "_" + Category + SourceFormClassifier.MarkdownExtension;
    private const string ChildEntrypoint = "_" + Child + SourceFormClassifier.MarkdownExtension;
    private const string RelativeBase = Category + "/" + Entrypoint;
    private const string RelativeChild = Category + "/" + Child + "/" + ChildEntrypoint;
    private const string PackageBase = WorkspaceLayout.DefaultMainRoot + "/" + RelativeBase;
    private const string InstalledBase = CustomRoot + "/" + RelativeBase;
    private const string PackageChild = WorkspaceLayout.DefaultMainRoot + "/" + RelativeChild;
    private const string InstalledChild = CustomRoot + "/" + RelativeChild;
    private const string SkillDirectory = WorkspaceLayout.NativeSkillsPath + "/" + Skill;
    private const string SkillPath = SkillDirectory + "/" + SourceFormClassifier.NativeSkillFileName;

    [Fact(DisplayName = "Package paths stay in package coordinates while projection maps bases and overwrites")]
    public void ProjectsMappedSourcesWithoutChangingPackageAssets()
    {
        var layout = Layout();
        var overwritePath = SourceOverwritePath.ReadAdjacentPath(PackageBase);
        var payload = Payload(Asset(PackageBase), Asset(overwritePath), Asset(SkillPath));
        var sources = new EmbeddedFrameworkSourceProjector().Project(layout, payload);
        Assert.Equal(new[] { SkillPath, CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName, InstalledBase }.Order(StringComparer.Ordinal),
            sources.Select(source => source.Identity.CanonicalBasePath));
        var projected = Assert.Single(sources, source => source.Identity.CanonicalBasePath == InstalledBase);
        Assert.Equal(Category, projected.Identity.AutomaticId);
        Assert.Equal(SourceDocumentForm.CanonicalEntrypoint, projected.Base.Form);
        Assert.Equal(SourceOverwritePath.ReadAdjacentPath(InstalledBase), projected.Overwrite?.CanonicalPath);
        Assert.Equal(Path.Combine(layout.Workspace.PhysicalRoot, CustomRoot.Replace('/', Path.DirectorySeparatorChar), Category, Entrypoint), projected.Base.PhysicalPath);
        Assert.Equal(PackageBase, payload.Find(PackageBase)?.Path);
        Assert.Null(payload.Find(InstalledBase));
        Assert.Throws<ArgumentException>(() => Asset(InstalledBase));
    }

    [Fact(DisplayName = "Single payload projection has the same mapped identity as catalogue projection")]
    public void SingleProjectionUsesSelectedLayout()
    {
        var source = FrameworkPayloadSourceProjection.Create(Layout(), Asset(PackageBase));
        Assert.Equal(InstalledBase, source.Identity.CanonicalBasePath);
        Assert.Equal(Category, source.Identity.AutomaticId);
        Assert.Equal(InstalledBase, source.Base.CanonicalPath);
    }

    [Fact(DisplayName = "Reverse alignment finds mapped exact assets and scoped entrypoints in package coordinates")]
    public void AlignmentReturnsOriginalPackageAsset()
    {
        var asset = Asset(PackageChild);
        var payload = Payload(Asset(PackageBase), asset, Asset(SkillPath));
        var layout = Layout();
        Assert.Same(asset, FrameworkSourceAlignment.ReadAsset(layout, InstalledChild, payload));
        Assert.Same(asset, FrameworkSourceAlignment.ReadAsset(layout, CustomRoot + "/" + Category + "/" + Scope + "/" + Child + "/" + ChildEntrypoint, payload));
        Assert.Null(FrameworkSourceAlignment.ReadAsset(layout, PackageChild, payload));
        Assert.Same(payload.Find(SkillPath), FrameworkSourceAlignment.ReadAsset(layout, SkillPath, payload));
    }

    [Fact(DisplayName = "Payload exclusions compare mapped destinations and preserve fixed Skill resource suppression")]
    public void SelectionUsesDestinationCoordinates()
    {
        var layout = Layout();
        var settings = WorkspaceSettingsDocument.Empty with { RemovedFiles = [InstalledBase, SkillPath] };
        Assert.False(FrameworkPayloadSelection.IncludesPath(layout, PackageBase, settings));
        Assert.False(FrameworkPayloadSelection.IncludesPath(layout, SkillDirectory + "/" + Resource, settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(layout, PackageChild, settings));
        Assert.True(FrameworkPayloadSelection.IncludesPath(layout, PackageBase,
            WorkspaceSettingsDocument.Empty with { RemovedFiles = [PackageBase] }));
        Assert.False(FrameworkPayloadSelection.IncludesPath(layout, PackageChild,
            WorkspaceSettingsDocument.Empty with { RemovedDirectories = [CustomRoot + "/" + Category] }));
        Assert.False(FrameworkPayloadSelection.IncludesPath(layout, PackageChild,
            WorkspaceSettingsDocument.Empty with { RemovedCategories = [Category] }));
        Assert.False(FrameworkPayloadSelection.IncludesPath(layout, SkillPath,
            WorkspaceSettingsDocument.Empty with { RemovedCategories = [WorkspaceLayoutNames.SkillsDirectoryName] }));
    }

    [Fact(DisplayName = "Custom projection still rejects an orphan package overwrite")]
    public void OrphanOverwriteRemainsInvalid()
        => Assert.Throws<InvalidDataException>(() => new EmbeddedFrameworkSourceProjector().Project(
            Layout(), Payload(Asset(SourceOverwritePath.ReadAdjacentPath(PackageBase)))));

    [Fact(DisplayName = "Projection detects collisions after mapping fixed native Skill destinations")]
    public void DetectMappedCollision()
    {
        var differentlyCasedSkill = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName.ToUpperInvariant()
            + "/" + Skill + "/" + SourceFormClassifier.NativeSkillFileName;
        var payload = Payload(Asset(SkillPath), Asset(differentlyCasedSkill));
        Assert.Throws<ArgumentException>(() => new EmbeddedFrameworkSourceProjector().Project(Layout(), payload));
    }

    [Fact(DisplayName = "Missing payload ancestors are reported at installed destinations and existing ancestors satisfy them")]
    public void RequiredAncestorsUseMappedPaths()
    {
        var layout = Layout();
        var payload = Payload(Asset(PackageBase), Asset(PackageChild));
        var observed = new SourceCatalogue(layout, [], [], [], isCancelled: false);
        var empty = new GeneratedNavigationFormationBuilder().Build(observed);
        var removedLoader = WorkspaceSettingsDocument.Empty with { RemovedFiles = [CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName] };
        Assert.Equal(CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
            FrameworkPayloadSelection.FindMissingRequiredAncestor(layout, payload, removedLoader, empty));
        var removedParent = WorkspaceSettingsDocument.Empty with { RemovedFiles = [InstalledBase] };
        Assert.Equal(InstalledBase, FrameworkPayloadSelection.FindMissingRequiredAncestor(layout, payload, removedParent, empty));
        var existing = new GeneratedNavigationFormationBuilder().Build(observed, [FrameworkPayloadSourceProjection.Create(layout, Asset(PackageBase))]);
        Assert.Null(FrameworkPayloadSelection.FindMissingRequiredAncestor(layout, payload, removedParent, existing));
    }

    private static FrameworkPayloadAsset Asset(string path) => FrameworkPayloadAsset.Create(path, []);

    private static FrameworkPayload Payload(params FrameworkPayloadAsset[] assets)
        => FrameworkPayload.Create([Asset(FrameworkPayloadAsset.RootAgentPath), Asset(FrameworkPayloadAsset.RootClaudePath), Asset(FrameworkPayloadAsset.LoaderPath), .. assets]);

    private static WorkspaceLayout Layout()
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), CustomRoot);
}
