using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

[Trait("Feature", "source-identity"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class SourceIdentityLayoutTests
{
    private const string CustomRoot = "docs/forge";
    private const string Category = "guidance";
    private const string Leaf = "review";
    private const string Id = Category + "/" + Leaf;
    private const string Relative = Id + SourceFormClassifier.MarkdownExtension;
    private const string Outside = "project/notes" + SourceFormClassifier.MarkdownExtension;
    private const string SkillId = WorkspaceLayoutNames.SkillsDirectoryName + "/example";
    private const string Skill = WorkspaceLayout.DefaultMainRoot + "/" + SkillId + "/" + SourceFormClassifier.NativeSkillFileName;

    [Theory(DisplayName = "Identity and forms use the selected primary content location")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void PrimaryIdentity(string root)
    {
        var layout = Layout(root);
        var path = root + "/" + Relative;
        Assert.Equal(Id, SourceIdentity.DeriveId(layout, path));
        Assert.Equal(Id, SourceIdentity.DeriveId(layout, SourceOverwritePath.ReadAdjacentPath(path)));
        Assert.True(SourceFormClassifier.TryClassify(layout, layout.LoaderPath, out var loader));
        Assert.Equal(SourceDocumentForm.Loader, loader);
        var entrypoint = root + "/" + Category + "/_" + Category + SourceFormClassifier.MarkdownExtension;
        Assert.Equal(Category, SourceIdentity.DeriveId(layout, entrypoint));
        Assert.True(SourceIdentity.IsRecognizedEntrypointPath(layout, entrypoint));
        Assert.Equal(WorkspaceLayoutNames.SkillsDirectoryName, SourceIdentity.DeriveId(layout, WorkspaceLayout.NativeSkillsPath));
        Assert.Equal(SkillId, SourceIdentity.DeriveId(layout, Skill));
        Assert.True(SourceFormClassifier.TryClassify(layout, Skill, out var skill));
        Assert.Equal(SourceDocumentForm.Skill, skill);
        Assert.Null(SourceIdentity.DeriveId(layout, Outside));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + Outside, SourceWorkspaceRelativePath.ReadReference(layout, Outside));
    }

    [Fact(DisplayName = "Canonical syntax does not grant primary inventory membership")]
    public void SyntaxAndMembershipAreSeparate()
    {
        var layout = Layout(CustomRoot);
        Assert.True(SourceLogicalPath.IsCanonicalSource(Outside));
        Assert.True(SourceLogicalPath.IsCanonicalRoot(CustomRoot));
        Assert.Throws<ArgumentException>(() => new SourceCatalogueRequest(layout, [Outside]));
        var request = new SourceCatalogueRequest(layout, layout.SourceRoots);
        Assert.Same(layout, request.Layout);
        Assert.Same(layout.Workspace, request.Workspace);
        var catalogue = new SourceCatalogue(layout, [], [], [], false);
        Assert.Same(layout, catalogue.Layout);
        Assert.Same(layout.Workspace, catalogue.Workspace);
        Assert.Null(SourceIdentity.DeriveId(layout, SourceLogicalPath.LoaderPath));
        Assert.False(SourceFormClassifier.TryClassify(layout, CustomRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName + "/" + SourceFormClassifier.NativeSkillFileName, out _));
    }

    private static WorkspaceLayout Layout(string root)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), root);
}
