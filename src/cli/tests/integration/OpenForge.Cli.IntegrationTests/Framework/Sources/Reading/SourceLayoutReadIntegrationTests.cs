using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

[Trait("Boundary", "OS"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
public sealed class SourceLayoutReadIntegrationTests
{
    private const string FixtureName = "source-layout-read";
    private const string CustomRoot = "docs/forge";
    private const string LeafId = "guide";
    private const string LeafFile = LeafId + SourceFormClassifier.MarkdownExtension;
    private const string SkillName = "example";
    private const string SkillId = WorkspaceLayoutNames.SkillsDirectoryName + "/" + SkillName;
    private const string SkillPath = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string SkillCatalogue = WorkspaceLayout.NativeSkillsPath + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension;
    private const string Outside = "project/_project" + SourceFormClassifier.MarkdownExtension;
    private const string OutsideLeaf = "project/leaf" + SourceFormClassifier.MarkdownExtension;
    private const string Missing = "project/missing" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "First description";
    private const string OtherDescription = "Second description";
    private const string Condition = "src/**";
    private const string OtherCondition = "tests/**";
    private const string Body = "# Source";
    private const string BlockedDirectory = "blocked";
    private const string OrphanName = "orphan" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Source sessions inventory the selected main root and native Skills without relocating Skills")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task InventoryAndDefaultScopesFollowLayout(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = Layout(temporary, mainRoot);
        var leaf = mainRoot + "/" + LeafFile;
        temporary.CreateFile(leaf, Body);
        temporary.CreateFile(SourceOverwritePath.ReadAdjacentPath(leaf), Body);
        temporary.CreateFile(layout.LoaderPath, Body);
        temporary.CreateFile(SkillPath, Body);
        temporary.CreateFile(SkillCatalogue, Body);

        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, TestContext.Current.CancellationToken);

        Assert.Same(layout, session.Layout);
        Assert.Same(layout.Workspace, session.DocumentReader.Workspace);
        Assert.Empty(session.Catalogue.Issues);
        Assert.Equal(layout.SourceRoots, session.DefaultSelectionScopes.Select(scope => scope.CanonicalDirectoryPath));
        var ordinary = Assert.IsType<SourceLogicalSource>(session.Catalogue.FindByPath(leaf));
        Assert.Equal(LeafId, ordinary.Identity.AutomaticId);
        Assert.NotNull(ordinary.Overwrite);
        var skill = Assert.IsType<SourceLogicalSource>(session.Catalogue.FindByPath(SkillPath));
        Assert.Equal(SkillId, skill.Identity.AutomaticId);
        Assert.Equal(SourceDocumentForm.Skill, skill.Base.Form);
        Assert.Null(skill.IncludedEntry);
        Assert.Equal(SourceDocumentForm.CanonicalEntrypoint, session.Catalogue.FindByPath(SkillCatalogue)?.Base.Form);
    }

    [Fact(DisplayName = "Absent optional Skills do not become a missing main root issue")]
    public async Task MissingOptionalSkillsAreNotMissingMainRoot()
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = Layout(temporary, CustomRoot);
        temporary.CreateFile(layout.LoaderPath, Body);
        var reader = new SourceReadSessionReader(new PhysicalPathResolver());
        var session = await reader.ReadAsync(layout, TestContext.Current.CancellationToken);
        Assert.Empty(session.Catalogue.Issues);
        Assert.Equal(CustomRoot, Assert.Single(session.DefaultSelectionScopes).CanonicalDirectoryPath);

        using var missing = TemporaryWorkspace.Create(FixtureName);
        var missingLayout = Layout(missing, CustomRoot);
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(missingLayout, missingLayout.SourceRoots), TestContext.Current.CancellationToken);
        var issue = Assert.Single(catalogue.Issues);
        Assert.Equal(SourceCatalogueIssueCode.RootMissing, issue.Code);
        Assert.Equal(CustomRoot, issue.AttemptedCanonicalPath);
    }

    [Theory(DisplayName = "Included reading retains ordered rows and traverses targets already present in inventory")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task PreinventoriedIncludesKeepMetadataAndTraversal(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = Layout(temporary, mainRoot);
        var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        temporary.CreateFile(layout.LoaderPath, $"""
            # Loader

            ## Entries

            - [{Description}]({prefix}{Outside}) - #{SourceLoadingClosureResolver.LoadNowTag} - applies to `{Condition}`
            - [{OtherDescription}]({prefix}{Outside}) - #{SourceLoadingClosureResolver.KeepInMindTag} - applies to `{OtherCondition}`
            - [{Description}]({prefix}{Missing}) - #{SourceLoadingClosureResolver.LoadNowTag}
            """);
        temporary.CreateFile(Outside, $"""
            # Project

            ## Entries

            - [{Description}]({Path.GetFileName(OutsideLeaf)}) - #{SourceLoadingClosureResolver.LoadNowTag}
            """);
        temporary.CreateFile(OutsideLeaf, Body);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, TestContext.Current.CancellationToken);
        var existing = new SourceLogicalSource(new SourceLogicalIdentity(SourceWorkspaceRelativePath.LiteralReferencePrefix + Outside, Outside),
            new SourceLayer(Outside, temporary.Combine(Outside), SourceDocumentForm.Markdown, SourceLayerKind.Base));
        var catalogue = new SourceCatalogue(layout, session.Catalogue.Candidates, [.. session.Catalogue.Sources, existing], session.Catalogue.Issues, false);
        var read = await new SourceIncludedEntryReader().ReadEntriesAsync(catalogue, session.DocumentReader, TestContext.Current.CancellationToken);
        var extended = SourceIncludedEntryReader.Extend(catalogue, read.Sources, read.Rows);

        Assert.Same(layout, extended.Layout);
        Assert.Equal(4, extended.IncludedRows.Count);
        Assert.Equal(new[] { Outside, Outside, Missing, OutsideLeaf }, extended.IncludedRows.Select(row => row.Path));
        var first = extended.IncludedRows[0];
        var second = extended.IncludedRows[1];
        Assert.Equal(layout.LoaderPath, first.ParentPath);
        Assert.Equal(Description, first.Entry.Description);
        Assert.Equal(OtherDescription, second.Entry.Description);
        Assert.Equal(SourceLoadingClosureResolver.LoadNowTag, Assert.Single(first.Entry.Tags));
        Assert.Equal(SourceLoadingClosureResolver.KeepInMindTag, Assert.Single(second.Entry.Tags));
        Assert.Equal(Condition, Assert.Single(first.Entry.ApplyTo).Text);
        Assert.Equal(OtherCondition, Assert.Single(second.Entry.ApplyTo).Text);
        Assert.True(first.Entry.Span.Start < second.Entry.Span.Start);
        Assert.Equal(PhysicalPathState.Missing, extended.IncludedRows[2].PhysicalState);
        var target = Assert.IsType<SourceLogicalSource>(extended.FindByPath(Outside));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Outside, target.Identity.AutomaticId);
        Assert.Same(first.Entry, target.IncludedEntry);
        Assert.True(target.IsIncludedCatalogue);
        Assert.Equal(Outside, extended.FindByPath(OutsideLeaf)?.IncludedParentPath);
    }

    private static WorkspaceLayout Layout(TemporaryWorkspace temporary, string root)
        => WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), root);

    [Theory(DisplayName = "Unreadable directories and orphan candidates survive default and exclusion-only selection across both content locations")]
    [InlineData(false), InlineData(true)]
    public async Task BothLocationsRetainUnavailableEvidence(bool exclude)
    {
        using var temporary = TemporaryWorkspace.Create(FixtureName);
        var layout = Layout(temporary, CustomRoot);
        temporary.CreateFile(CustomRoot + "/" + LeafFile, Body);
        var directories = layout.SourceRoots.Select(root => temporary.CreateDirectory(root + "/" + BlockedDirectory)).ToArray();
        foreach (var root in layout.SourceRoots)
            temporary.CreateFile(SourceOverwritePath.ReadAdjacentPath(root + "/" + OrphanName), Body);
        var denials = new List<IDisposable>();
        var modes = new Dictionary<string, UnixFileMode>();
        try
        {
            foreach (var directory in directories)
            {
                if (OperatingSystem.IsWindows())
                    denials.Add(WindowsDirectoryEnumerationDenial.Create(temporary.Path, directory));
                else
                {
                    modes.Add(directory, File.GetUnixFileMode(directory));
                    File.SetUnixFileMode(directory, UnixFileMode.UserExecute);
                }
            }
            var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(layout, TestContext.Current.CancellationToken);
            var result = new SourceUniverseFilterResolver(new SourceReferenceResolver((workspace, path) => session.DocumentReader.PhysicalPaths.ResolveCandidate(
                workspace.LexicalRoot, workspace.PhysicalRoot, temporary.Combine(path)))).Resolve(new SourceUniverseFilterRequest(
                session.Catalogue, session.DefaultSelectionScopes,
                exclude ? [new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, LeafId, 1)] : []));

            Assert.True(result.IsResolved);
            Assert.Equal(2, result.Selection.Issues.Count(issue => issue.Code == SourceCatalogueIssueCode.DirectoryUnavailable));
            Assert.Equal(2, result.Selection.Issues.Count(issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite));
            Assert.Equal(2, result.Selection.Candidates.Count(candidate => candidate.Form == SourceDocumentForm.OverwriteCompanion));
            Assert.Equal(exclude ? 0 : 1, result.Selection.Sources.Count);
        }
        finally
        {
            foreach (var denial in denials) denial.Dispose();
            if (!OperatingSystem.IsWindows())
                foreach (var mode in modes) File.SetUnixFileMode(mode.Key, mode.Value);
        }
    }
}
