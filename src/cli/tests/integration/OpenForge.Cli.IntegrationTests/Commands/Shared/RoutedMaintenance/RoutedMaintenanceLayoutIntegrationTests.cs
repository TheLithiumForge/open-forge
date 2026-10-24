using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;

[Trait("Boundary", "OS"), Trait("Feature", "routed-maintenance"), Trait("Evidence", "Integration")]
public sealed class RoutedMaintenanceLayoutIntegrationTests
{
    private const string FixtureName = "routed-maintenance-layout";
    private const string CustomRoot = "docs/forge";
    private const string Category = WorkspaceCategoryNames.Guidance;
    private const string Team = "team";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string SkillName = "example";
    private const string Body = "# Note\n";
    private static readonly string RootEntry = SourceFormClassifier.CanonicalEntrypointFileName(Category);
    private static readonly string SkillEntry = SourceFormClassifier.CanonicalEntrypointFileName(WorkspaceLayoutNames.SkillsDirectoryName);
    private static readonly string TeamEntry = SourceFormClassifier.CanonicalEntrypointFileName(Team);

    [Theory(DisplayName = "Shared maintenance completes both content locations and scaffolds only below a recognized root")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task CatalogueLayoutDrivesCompletionAndScaffolding(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot);
        var rootPath = mainRoot + "/" + Category + "/" + RootEntry;
        var notePath = mainRoot + "/" + Category + "/" + Team + "/" + Note;
        var skillRoot = WorkspaceLayout.NativeSkillsPath + "/" + SkillEntry;
        var skillPath = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName;
        var skillNote = WorkspaceLayout.NativeSkillsPath + "/" + Note;
        foreach (var path in new[] { rootPath, notePath, skillRoot, skillPath, skillNote }) temporary.CreateFile(path, Body);
        var paths = new PhysicalPathResolver();
        var session = await new SourceReadSessionReader(paths).ReadAsync(layout, TestContext.Current.CancellationToken);
        var before = temporary.SnapshotHashes();
        var intent = await new RoutedMaintenancePlanner(paths).PrepareAsync(new()
        {
            Catalogue = session.Catalogue,
            RootPaths = [rootPath, skillRoot],
            SelectedSources = [.. session.Catalogue.Sources],
            Policy = new() { Settings = WorkspaceSettingsDocument.Empty, Ownership = WorkspaceOwnershipDocument.Empty },
        }, TestContext.Current.CancellationToken);

        Assert.Empty(intent.Limitations);
        Assert.Same(layout, intent.Formation.Catalogue.Layout);
        var scaffold = Assert.Single(intent.Entrypoints);
        Assert.Equal(mainRoot + "/" + Category + "/" + Team + "/" + TeamEntry, scaffold.Source.Identity.CanonicalBasePath);
        Assert.Equal(Category + "/" + Team, scaffold.Source.Identity.AutomaticId);
        Assert.Contains(intent.Metadata, metadata => metadata.Path == notePath);
        Assert.Contains(intent.Metadata, metadata => metadata.Path == skillPath);
        Assert.Contains(intent.Metadata, metadata => metadata.Path == skillNote);
        Assert.Equal(before, temporary.SnapshotHashes());
    }
}
