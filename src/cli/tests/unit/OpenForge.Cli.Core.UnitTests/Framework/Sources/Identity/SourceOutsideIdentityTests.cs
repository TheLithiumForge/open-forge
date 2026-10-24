using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

[Trait("Feature", "source-identity"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class SourceOutsideIdentityTests
{
    private const string Team = "docs/team";
    private const string CustomRoot = "context/forge";
    private const string Nested = Team + "/nested";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;

    public static TheoryData<string, string> OutsideForms => new()
    {
        { WorkspaceRoutePath.CanonicalEntrypointPath(Team), nameof(SourceDocumentForm.CanonicalEntrypoint) },
        { WorkspaceRoutePath.CanonicalEntrypointPath(Nested), nameof(SourceDocumentForm.CanonicalEntrypoint) },
        { Team + "/" + SourceFormClassifier.IndexFileName, nameof(SourceDocumentForm.IndexEntrypoint) },
        { Team + "/" + SourceFormClassifier.UnderscoreIndexFileName, nameof(SourceDocumentForm.UnderscoreIndexEntrypoint) },
        { Nested + "/" + SourceFormClassifier.ReferencesFileName, nameof(SourceDocumentForm.ReferencesEntrypoint) },
        { Nested + "/" + SourceFormClassifier.UnderscoreReferencesFileName, nameof(SourceDocumentForm.UnderscoreReferencesEntrypoint) },
        { Team + "/" + Note, nameof(SourceDocumentForm.Markdown) },
        { Team + "/" + SourceFormClassifier.NativeSkillFileName, nameof(SourceDocumentForm.Markdown) },
        { Nested + "/" + SourceFormClassifier.NativeSkillFileName, nameof(SourceDocumentForm.Markdown) },
        { Team + "/" + WorkspaceLayoutNames.LoaderFileName, nameof(SourceDocumentForm.Markdown) },
    };

    [Theory(DisplayName = "Outside intrinsic forms retain literal identities and adjacent overwrites in either primary layout")]
    [MemberData(nameof(OutsideForms))]
    public void FormsAndLiteralIdentities(string path, string formName)
    {
        var expectedForm = Enum.Parse<SourceDocumentForm>(formName);
        foreach (var mainRoot in new[] { WorkspaceLayout.DefaultMainRoot, CustomRoot })
        {
            var unregistered = Layout(mainRoot);
            var layout = unregistered.WithOutsideRoutes([Team]);
            var literalId = SourceWorkspaceRelativePath.ReadReference(unregistered, path);
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, literalId);
            Assert.Equal(literalId, SourceIdentity.DeriveId(layout, path));
            Assert.Equal(literalId, SourceWorkspaceRelativePath.ReadReference(layout, path));
            Assert.Equal(literalId, SourceWorkspaceRelativePath.ReadReference(layout.WithOutsideRoutes([]), path));
            Assert.True(SourceFormClassifier.TryClassify(layout, path, out var form));
            Assert.Equal(expectedForm, form);
            Assert.True(SourceFormClassifier.Matches(layout, path, expectedForm));
            Assert.Equal(SourceFormClassifier.IsEntrypoint(expectedForm), SourceIdentity.IsRecognizedEntrypointPath(layout, path));
            Assert.False(SourceFormClassifier.IsEntryDocument(layout, path));
            var overwrite = SourceOverwritePath.ReadAdjacentPath(path);
            Assert.Equal(literalId, SourceIdentity.DeriveId(layout, overwrite));
            Assert.True(SourceFormClassifier.Matches(layout, overwrite, SourceDocumentForm.OverwriteCompanion));
            Assert.False(layout.IsImplicitContentDestination(path));
            Assert.False(layout.TryGetSourceRelativePath(path, out _));

            var primary = new SourceContentRoots(layout);
            Assert.False(SourceLogicalPath.IsPrimarySource(primary, path));
            Assert.False(SourceLogicalPath.IsPrimaryMarkdownSource(primary, path));
            Assert.False(SourceFormClassifier.TryClassify(primary, path, out _));
            Assert.False(SourceIdentity.IsRecognizedEntrypointPath(primary, path));
            Assert.Null(SourceIdentity.DeriveId(primary, path));
        }
    }

    [Theory(DisplayName = "Outside discovery retains lexical source names that portable writes reject")]
    [InlineData("draft?/note" + SourceFormClassifier.MarkdownExtension)]
    [InlineData("draft:notes" + SourceFormClassifier.MarkdownExtension)]
    [InlineData("cafe\u0301" + SourceFormClassifier.MarkdownExtension)]
    [InlineData("NUL/" + Note)]
    public void LexicalReadNames(string relative)
    {
        var path = Team + "/" + relative;
        var layout = Layout(CustomRoot).WithOutsideRoutes([Team]);
        Assert.True(SourceLogicalPath.IsCanonicalSource(path));
        Assert.False(PortableWorkspacePath.TryNormalize(path, out _));
        Assert.True(SourceLogicalPath.IsOutsideSource(layout, path));
        Assert.True(SourceLogicalPath.IsInventoryPath(layout, path));
        Assert.False(SourceLogicalPath.IsPrimarySource(layout, path));
        Assert.True(SourceFormClassifier.Matches(layout, path, SourceDocumentForm.Markdown));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, SourceIdentity.DeriveId(layout, path));
    }

    public static TheoryData<string?> OutsideNonmembers => new()
    {
        (string?)null, string.Empty, ".", "./" + Team + "/" + Note, Team + "/", Team + "/../" + Note,
        Team + "/nested\\" + Note, Team + "/bad\0" + Note, Team.ToUpperInvariant() + "/" + Note,
        Team + "-old/" + Note, "docs/" + Note, "elsewhere/" + Note,
    };

    [Theory(DisplayName = "Outside membership rejects noncanonical paths case aliases and prefix lookalikes")]
    [MemberData(nameof(OutsideNonmembers))]
    public void OrdinalMembership(string? path)
    {
        var layout = Layout(CustomRoot).WithOutsideRoutes([Team]);
        Assert.False(SourceLogicalPath.IsOutsideSource(layout, path));
        Assert.False(SourceLogicalPath.IsInventoryPath(layout, path));
        Assert.False(SourceFormClassifier.TryClassify(layout, path, out _));
        Assert.Null(SourceIdentity.DeriveId(layout, path));
    }

    [Theory(DisplayName = "Catalogue requests admit declared inventory roots and lexical descendants without widening primary coordinates")]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(CustomRoot)]
    public void RequestAdmission(string mainRoot)
    {
        var layout = Layout(mainRoot).WithOutsideRoutes([Team]);
        var roots = new[] { mainRoot, WorkspaceLayout.NativeSkillsPath, Team, Nested, Team + "/draft?" };
        var request = new SourceCatalogueRequest(layout, roots);
        Assert.Equal(roots.Order(StringComparer.Ordinal), request.LogicalRoots);
        Assert.Equal(layout.InventoryRoots.Order(StringComparer.Ordinal), new SourceCatalogueRequest(layout, layout.InventoryRoots).LogicalRoots);
        Assert.True(SourceLogicalPath.IsOutsideSource(layout, Team));
        Assert.False(SourceLogicalPath.IsPrimarySource(layout, Team));
        foreach (var path in new[] { "docs", Team + "-old", Team.ToUpperInvariant(), "./" + Team, "elsewhere" })
            Assert.Throws<ArgumentException>(() => new SourceCatalogueRequest(layout, [path]));
        Assert.Throws<ArgumentException>(() => new SourceCatalogueRequest(layout, []));
        if (!layout.IsDefault)
        {
            Assert.Throws<ArgumentException>(() => new SourceCatalogueRequest(layout, [WorkspaceLayout.DefaultMainRoot]));
            Assert.Throws<ArgumentException>(() => new SourceCatalogueRequest(layout, [mainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName]));
        }

        var primaryPath = layout.SourcePath("rules/" + Note);
        Assert.Equal("rules/note", SourceIdentity.DeriveId(layout, primaryPath));
        var skillPath = WorkspaceLayout.NativeSkillsPath + "/tool/" + SourceFormClassifier.NativeSkillFileName;
        Assert.Equal(WorkspaceLayoutNames.SkillsDirectoryName + "/tool", SourceIdentity.DeriveId(layout, skillPath));
        Assert.True(SourceFormClassifier.Matches(layout, skillPath, SourceDocumentForm.Skill));
        Assert.True(SourceFormClassifier.Matches(layout, layout.LoaderPath, SourceDocumentForm.Loader));
        Assert.Equal([WorkspaceLayout.DefaultMainRoot], SourceContentRoots.PackageCoordinates.SourceRoots);
        Assert.Null(SourceIdentity.DeriveId(SourceContentRoots.PackageCoordinates, Team + "/" + Note));
    }

    private static WorkspaceLayout Layout(string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
}
