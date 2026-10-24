using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

[Trait("Feature", "source-reading"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class SourceOutsideReadSessionIntegrationTests
{
    private const string CustomRoot = "context/forge";
    private const string Team = "docs/team";
    private const string Notes = "notes";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string Linked = "included/linked" + SourceFormClassifier.MarkdownExtension;
    private const string Sibling = "included/sibling" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Read sessions scan declared inventory roots and retain their ordered explicit selection scopes")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(CustomRoot)]
    public async Task InventoryRootsAndScopes(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create("outside-read-session-scopes");
        var layout = Layout(temporary, mainRoot).WithOutsideRoutes([Notes, Team]);
        foreach (var root in layout.InventoryRoots) temporary.CreateDirectory(root);
        byte[] bytes = [1, 2, 3];
        var paths = new[] { layout.SourcePath("rules/" + Note), Notes + "/" + Note, Team + "/" + Note, "elsewhere/" + Note };
        foreach (var path in paths) temporary.CreateFile(path, bytes);

        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, TestContext.Current.CancellationToken);

        Assert.Same(layout, session.Layout);
        Assert.Same(layout.Workspace, session.DocumentReader.Workspace);
        Assert.Equal(layout.InventoryRoots, session.DefaultSelectionScopes.Select(scope => scope.CanonicalDirectoryPath));
        Assert.Equal(layout.InventoryRoots.Select(root => temporary.Combine(root)), session.DefaultSelectionScopes.Select(scope => scope.PhysicalDirectoryPath));
        Assert.Equal(3, session.Catalogue.Sources.Count);
        foreach (var outside in new[] { Notes + "/" + Note, Team + "/" + Note })
        {
            var source = Assert.IsType<SourceLogicalSource>(session.Catalogue.FindByPath(outside));
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + outside, source.Identity.AutomaticId);
            Assert.Null(source.IncludedEntry);
            Assert.False(source.IsIncludedCatalogue);
        }
        Assert.Null(session.Catalogue.FindByPath("elsewhere/" + Note));
        Assert.Empty(session.Catalogue.IncludedRows);
        foreach (var path in paths) Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(path)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Theory(DisplayName = "File includes remain disjoint from registered scans and do not scan siblings or reread settings")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(CustomRoot)]
    public async Task IncludesDoNotBecomeScanRoots(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create("outside-read-session-includes");
        var layout = Layout(temporary, mainRoot).WithOutsideRoutes([Team]);
        var linkedDestination = Path.GetRelativePath(temporary.Combine(mainRoot), temporary.Combine(Linked)).Replace(Path.DirectorySeparatorChar, '/');
        var loader = Encoding.UTF8.GetBytes($"# Demo\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n- [Linked note]({linkedDestination})\n");
        temporary.CreateFile(layout.LoaderPath, loader);
        byte[] bytes = [0x41, 0x42, 0x0A];
        temporary.CreateFile(Linked, bytes);
        temporary.CreateFile(Sibling, bytes);
        temporary.CreateFile(Team + "/" + Note, bytes);
        byte[] invalidSettings = [0xFF, 0xFE];
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath, invalidSettings);
        var reader = new SourceReadSessionReader(new PhysicalPathResolver());

        var inventory = await reader.ReadAsync(layout, TestContext.Current.CancellationToken);
        Assert.Null(inventory.Catalogue.FindByPath(Linked));
        Assert.Null(inventory.Catalogue.FindByPath(Sibling));
        Assert.NotNull(inventory.Catalogue.FindByPath(Team + "/" + Note));
        var combined = await reader.ReadIncludedAsync(layout, TestContext.Current.CancellationToken);
        var included = Assert.IsType<SourceLogicalSource>(combined.Catalogue.FindByPath(Linked));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Linked, included.Identity.AutomaticId);
        Assert.Null(combined.Catalogue.FindCandidateByPath(Linked));
        Assert.Null(combined.Catalogue.FindByPath(Sibling));
        Assert.NotNull(combined.Catalogue.FindCandidateByPath(Team + "/" + Note));
        Assert.Single(combined.Catalogue.IncludedRows);
        Assert.Equal(inventory.DefaultSelectionScopes.Select(scope => (scope.CanonicalDirectoryPath, scope.PhysicalDirectoryPath)),
            combined.DefaultSelectionScopes.Select(scope => (scope.CanonicalDirectoryPath, scope.PhysicalDirectoryPath)));
        Assert.DoesNotContain(combined.DefaultSelectionScopes, scope => scope.CanonicalDirectoryPath == SourceLogicalPath.ReadParent(Linked));
        Assert.Equal(loader, File.ReadAllBytes(temporary.Combine(layout.LoaderPath)));
        Assert.Equal(invalidSettings, File.ReadAllBytes(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        foreach (var path in new[] { Linked, Sibling, Team + "/" + Note }) Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(path)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Fact(DisplayName = "Missing registered roots remain diagnosable and are not replaced by another scan root")]
    public async Task MissingRoot()
    {
        using var temporary = TemporaryWorkspace.Create("outside-read-session-missing");
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Team, Notes]);
        temporary.CreateDirectory(layout.MainRoot);
        byte[] bytes = [4, 5, 6];
        temporary.CreateFile(Notes + "/" + Note, bytes);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, TestContext.Current.CancellationToken);
        Assert.Same(layout, session.Layout);
        Assert.Equal([layout.MainRoot, Notes], session.DefaultSelectionScopes.Select(scope => scope.CanonicalDirectoryPath));
        Assert.Contains(session.Catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootMissing && issue.AttemptedCanonicalPath == Team);
        Assert.DoesNotContain(session.Catalogue.Issues, issue => issue.AttemptedCanonicalPath == WorkspaceLayout.NativeSkillsPath);
        Assert.NotNull(session.Catalogue.FindByPath(Notes + "/" + Note));
        Assert.False(Directory.Exists(temporary.Combine(Team)));
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(Notes + "/" + Note)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Fact(DisplayName = "Pre-cancelled outside sessions retain cancellation without creating missing roots or runtime state")]
    public async Task Cancellation()
    {
        using var temporary = TemporaryWorkspace.Create("outside-read-session-cancelled");
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Team]);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, cancellation.Token);
        Assert.True(session.Catalogue.IsCancelled);
        Assert.Empty(session.Catalogue.Sources);
        Assert.Empty(session.DefaultSelectionScopes);
        Assert.Empty(session.Catalogue.Issues);
        foreach (var root in layout.InventoryRoots) Assert.False(Directory.Exists(temporary.Combine(root)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    private static WorkspaceLayout Layout(TemporaryWorkspace temporary, string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot);
}
