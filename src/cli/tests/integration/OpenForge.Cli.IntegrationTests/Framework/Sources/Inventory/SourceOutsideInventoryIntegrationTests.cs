using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Inventory;

[Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class SourceOutsideInventoryIntegrationTests
{
    private const string Team = "docs/team";
    private const string Nested = Team + "/nested";
    private const string CustomRoot = "context/forge";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Registered inventory retains every outside intrinsic form exact identity and overwrite without reading bodies")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(CustomRoot)]
    public async Task OutsideFormsAndPairing(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create("outside-source-forms");
        var layout = Layout(temporary, mainRoot).WithOutsideRoutes([Team]);
        var outside = new (string Path, SourceDocumentForm Form)[]
        {
            (WorkspaceRoutePath.CanonicalEntrypointPath(Team), SourceDocumentForm.CanonicalEntrypoint),
            (WorkspaceRoutePath.CanonicalEntrypointPath(Nested), SourceDocumentForm.CanonicalEntrypoint),
            (Team + "/" + SourceFormClassifier.IndexFileName, SourceDocumentForm.IndexEntrypoint),
            (Team + "/" + SourceFormClassifier.UnderscoreIndexFileName, SourceDocumentForm.UnderscoreIndexEntrypoint),
            (Nested + "/" + SourceFormClassifier.ReferencesFileName, SourceDocumentForm.ReferencesEntrypoint),
            (Nested + "/" + SourceFormClassifier.UnderscoreReferencesFileName, SourceDocumentForm.UnderscoreReferencesEntrypoint),
            (Team + "/" + Note, SourceDocumentForm.Markdown),
            (Team + "/" + SourceFormClassifier.NativeSkillFileName, SourceDocumentForm.Markdown),
            (Team + "/" + WorkspaceLayoutNames.LoaderFileName, SourceDocumentForm.Markdown),
            (Team + "/cafe\u0301" + SourceFormClassifier.MarkdownExtension, SourceDocumentForm.Markdown),
        };
        var primary = layout.SourcePath("rules/" + Note);
        var skill = WorkspaceLayout.NativeSkillsPath + "/tool/" + SourceFormClassifier.NativeSkillFileName;
        var overwrite = SourceOverwritePath.ReadAdjacentPath(Team + "/" + Note);
        var orphan = SourceOverwritePath.ReadAdjacentPath(Team + "/orphan" + SourceFormClassifier.MarkdownExtension);
        var paths = outside.Select(item => item.Path).Concat([layout.LoaderPath, primary, skill, overwrite, orphan, Team + "/ignored.txt", Team + "-old/" + Note]).ToArray();
        byte[] bytes = [0xFF, 0xFE];
        foreach (var path in paths) temporary.CreateFile(path, bytes);

        var catalogue = await new SourceCatalogueReader().ReadAsync(new(layout, layout.InventoryRoots), TestContext.Current.CancellationToken);

        Assert.Same(layout, catalogue.Layout);
        Assert.False(catalogue.IsCancelled);
        foreach (var (path, form) in outside)
        {
            var source = Assert.IsType<SourceLogicalSource>(catalogue.FindByPath(path));
            Assert.Equal(form, source.Base.Form);
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, source.Identity.AutomaticId);
            Assert.Null(source.IncludedEntry);
            Assert.False(source.IsIncludedCatalogue);
            Assert.False(layout.IsImplicitContentDestination(path));
        }
        var paired = Assert.IsType<SourceLogicalSource>(catalogue.FindByPath(Team + "/" + Note));
        Assert.Same(paired, catalogue.FindByPath(overwrite));
        Assert.Equal(overwrite, Assert.IsType<SourceLayer>(paired.Overwrite).CanonicalPath);
        Assert.Equal(paired.Identity.AutomaticId, catalogue.FindCandidateByPath(overwrite)?.AutomaticId);
        Assert.Null(catalogue.FindByPath(orphan));
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite && issue.AttemptedCanonicalPath == orphan);
        Assert.Equal("rules/note", catalogue.FindByPath(primary)?.Identity.AutomaticId);
        Assert.Equal(WorkspaceLayoutNames.SkillsDirectoryName + "/tool", catalogue.FindByPath(skill)?.Identity.AutomaticId);
        Assert.Equal(SourceDocumentForm.Loader, catalogue.FindByPath(layout.LoaderPath)?.Base.Form);
        Assert.Null(catalogue.FindCandidateByPath(Team + "/ignored.txt"));
        Assert.Null(catalogue.FindCandidateByPath(Team + "-old/" + Note));
        Assert.Empty(catalogue.IncludedRows);
        Assert.DoesNotContain(catalogue.Issues, issue => issue.Code is SourceCatalogueIssueCode.IdentityCollision or SourceCatalogueIssueCode.EntrypointCompatibilityCollision);
        foreach (var path in paths) Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(path)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Fact(DisplayName = "Outside discovery keeps physical alias facts and blocks external cyclic re-entry and runtime paths")]
    public async Task PhysicalBoundaries()
    {
        using var temporary = TemporaryWorkspace.Create("outside-source-boundaries");
        using var external = TemporaryWorkspace.Create("outside-source-external");
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Team]);
        var safe = Team + "/" + Note;
        var runtime = WorkspaceRuntimePaths.DirectoryName + "/" + Note;
        byte[] bytes = [1, 3, 5, 7];
        var safePhysical = temporary.CreateFile(safe, bytes);
        var externalPhysical = external.CreateFile(Note, bytes);
        temporary.CreateFile(runtime, bytes);
        Assert.True(temporary.TryCreateFileSymbolicLink(Team + "/alias" + SourceFormClassifier.MarkdownExtension, safePhysical, out _));
        Assert.True(temporary.TryCreateFileSymbolicLink(Team + "/external" + SourceFormClassifier.MarkdownExtension, externalPhysical, out _));
        Assert.True(temporary.TryCreateDirectorySymbolicLink(Team + "/cycle", temporary.Combine(Team), out _));
        Assert.True(external.TryCreateDirectorySymbolicLink("back", temporary.Combine(Team), out _));
        Assert.True(temporary.TryCreateDirectorySymbolicLink(Team + "/reentry", external.Path, out _));
        Assert.True(temporary.TryCreateDirectorySymbolicLink(Team + "/runtime", temporary.Combine(WorkspaceRuntimePaths.DirectoryName), out _));

        var catalogue = await new SourceCatalogueReader().ReadAsync(new(layout, [Team]), TestContext.Current.CancellationToken);

        Assert.NotNull(catalogue.FindByPath(safe));
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.PhysicalAlias && issue.RelatedPaths.Contains(safe));
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.CandidateUnsafe && issue.AttemptedCanonicalPath == Team + "/cycle");
        var externalPath = Team + "/external" + SourceFormClassifier.MarkdownExtension;
        Assert.Equal(PhysicalPathState.External, catalogue.FindCandidateByPath(externalPath)?.PhysicalState);
        Assert.Null(catalogue.FindByPath(externalPath));
        Assert.DoesNotContain(catalogue.Candidates, candidate => candidate.CanonicalPath.StartsWith(Team + "/reentry/", StringComparison.Ordinal));
        Assert.DoesNotContain(catalogue.Candidates, candidate => candidate.CanonicalPath.StartsWith(Team + "/runtime/", StringComparison.Ordinal));
        Assert.Equal(bytes, File.ReadAllBytes(safePhysical));
        Assert.Equal(bytes, File.ReadAllBytes(externalPhysical));
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(runtime)));
    }

    [Fact(DisplayName = "Outside roots retain independent missing unavailable and unsafe root issues")]
    public async Task RootOutcomes()
    {
        using var temporary = TemporaryWorkspace.Create("outside-source-root-outcomes");
        using var external = TemporaryWorkspace.Create("outside-source-root-external");
        const string missing = "docs/missing";
        const string fileRoot = "docs/file";
        const string unsafeRoot = "docs/external";
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Team, missing, fileRoot, unsafeRoot]);
        byte[] bytes = [2, 4, 6];
        temporary.CreateFile(Team + "/" + Note, bytes);
        temporary.CreateFile(fileRoot, bytes);
        Assert.True(temporary.TryCreateDirectorySymbolicLink(unsafeRoot, external.Path, out _));

        var catalogue = await new SourceCatalogueReader().ReadAsync(new(layout, [Team, missing, fileRoot, unsafeRoot]), TestContext.Current.CancellationToken);

        Assert.NotNull(catalogue.FindByPath(Team + "/" + Note));
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootMissing && issue.AttemptedCanonicalPath == missing);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootUnavailable && issue.AttemptedCanonicalPath == fileRoot);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootUnsafe && issue.AttemptedCanonicalPath == unsafeRoot);
        Assert.False(Directory.Exists(temporary.Combine(missing)));
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(fileRoot)));
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(Team + "/" + Note)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    [Fact(DisplayName = "Repeated overlapping outside scan requests are deterministic and cancellation creates no state")]
    public async Task DeterminismAndCancellation()
    {
        using var temporary = TemporaryWorkspace.Create("outside-source-repeat");
        var layout = Layout(temporary, CustomRoot).WithOutsideRoutes([Team]);
        byte[] bytes = [9, 8, 7];
        temporary.CreateFile(Team + "/" + Note, bytes);
        temporary.CreateFile(Nested + "/" + Note, bytes);
        var request = new SourceCatalogueRequest(layout, [Nested, Team, Team]);
        var reader = new SourceCatalogueReader();
        var first = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        var second = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(2, first.Sources.Count);
        Assert.Equal(first.Candidates.Select(candidate => candidate.CanonicalPath), second.Candidates.Select(candidate => candidate.CanonicalPath));
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var cancelled = await reader.ReadAsync(new(layout, [Team + "/missing"]), cancellation.Token);
        Assert.True(cancelled.IsCancelled);
        Assert.Empty(cancelled.Candidates);
        Assert.Empty(cancelled.Issues);
        Assert.False(Directory.Exists(temporary.Combine(Team + "/missing")));
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(Team + "/" + Note)));
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(Nested + "/" + Note)));
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    private static WorkspaceLayout Layout(TemporaryWorkspace temporary, string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot);
}
