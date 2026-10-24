using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

[Trait("Boundary", "Processing"), Trait("Feature", "source-identity"), Trait("Evidence", "Unit")]
public sealed class SourcePrimaryPathTests
{
    private const string CustomRoot = "docs/forge";
    private const string Name = "topic";
    private const string TextExtension = ".txt";
    private const string ExternalRoot = "external";

    [Theory(DisplayName = "Primary Markdown checks distinguish native, overwrite and literal external paths")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void ClassifiesPrimaryPathsWithoutPhysicalWorkspaceAccess(string mainRoot)
    {
        var roots = mainRoot == WorkspaceLayout.DefaultMainRoot
            ? SourceContentRoots.Default
            : new SourceContentRoots(WorkspaceLayout.Create(SourceInventoryTestData.Workspace(), mainRoot));
        var markdown = mainRoot + "/" + Name + SourceFormClassifier.MarkdownExtension;
        var overwrite = SourceOverwritePath.ReadAdjacentPath(markdown);
        var text = mainRoot + "/" + Name + TextExtension;
        var external = ExternalRoot + "/" + Name + SourceFormClassifier.MarkdownExtension;
        var skill = WorkspaceLayout.NativeSkillsPath + "/" + Name + "/" + SourceFormClassifier.NativeSkillFileName;

        Assert.True(SourceLogicalPath.IsPrimaryMarkdownSource(roots, markdown));
        Assert.False(SourceLogicalPath.IsPrimaryOverwriteSource(roots, markdown));
        Assert.True(SourceLogicalPath.IsPrimaryMarkdownSource(roots, overwrite));
        Assert.True(SourceLogicalPath.IsPrimaryOverwriteSource(roots, overwrite));
        Assert.True(SourceLogicalPath.IsPrimaryMarkdownSource(roots, skill));
        Assert.False(SourceLogicalPath.IsPrimaryMarkdownSource(roots, text));
        Assert.True(SourceLogicalPath.IsPrimaryNonMarkdownSource(roots, text));
        Assert.False(SourceLogicalPath.IsPrimaryNonMarkdownSource(roots, external));
        Assert.False(SourceLogicalPath.IsPrimaryMarkdownSource(roots, external));
        Assert.False(SourceLogicalPath.IsPrimaryOverwriteSource(roots, SourceOverwritePath.ReadAdjacentPath(external)));
        Assert.False(SourceLogicalPath.IsPrimaryMarkdownSource(roots, null));
        Assert.False(SourceLogicalPath.IsPrimaryOverwriteSource(roots, null));
    }
}
