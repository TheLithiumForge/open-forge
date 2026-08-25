using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceLogicalPathRedTests
{
    [Theory(DisplayName = "Neutral logical paths accept canonical slash-separated source paths"),
        InlineData(".agents/loader.md", true),
        InlineData(".agents/project alpha/工作.md", true),
        InlineData(".agents/root/encoded%20name.md", true),
        InlineData(".agents", false),
        InlineData(".agents/", false),
        InlineData("./.agents/root.md", false),
        InlineData(".agents/root//file.md", false),
        InlineData(".agents/root/./file.md", false),
        InlineData(".agents/root/../file.md", false),
        InlineData(".agents/root\\file.md", false),
        InlineData(".agents/root/\0file.md", false)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CanonicalSourceValidationPreservesTheRootDistinction(
        string path,
        bool expected)
    {
        Assert.Equal(expected, SourceLogicalPath.IsCanonicalSource(path));
        Assert.Equal(expected, SourceLogicalPath.IsCanonical(path));
    }

    [Theory(DisplayName = "Neutral logical roots accept the .agents root and canonical descendants"),
        InlineData(".agents", true),
        InlineData(".agents/root", true),
        InlineData(".agents/project alpha", true),
        InlineData(".agents/root/../other", false),
        InlineData(".agents/", false),
        InlineData(".agents/root\\child", false)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CanonicalRootValidationIncludesOnlyDirectoryShapes(
        string path,
        bool expected)
    {
        Assert.Equal(expected, SourceLogicalPath.IsCanonicalRoot(path));
    }

    [Theory(DisplayName = "Neutral logical paths read exact parent and file segments"),
        InlineData(".agents/root/_root.md", ".agents/root", "_root.md"),
        InlineData(".agents/root/child/grand.md", ".agents/root/child", "grand.md"),
        InlineData(".agents/project alpha/工作%20note.md", ".agents/project alpha", "工作%20note.md")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ParentAndFileConversionPreserveText(
        string logicalPath,
        string expectedParent,
        string expectedFileName)
    {
        Assert.Equal(expectedParent, SourceLogicalPath.ReadParent(logicalPath));
        Assert.Equal(expectedFileName, SourceLogicalPath.ReadFileName(logicalPath));
    }

    [Fact(DisplayName = "Neutral logical paths combine canonical segments and convert to one lexical workspace path")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CombinationAndLexicalConversionPreserveTheCanonicalSpelling()
    {
        var workspaceRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "source-logical-path-unit"));

        Assert.Equal(
            ".agents/project alpha/工作%20note.md",
            SourceLogicalPath.Combine(
                ".agents/project alpha",
                "工作%20note.md"));
        Assert.Equal(
            Path.Combine(workspaceRoot, ".agents", "project alpha", "工作%20note.md"),
            SourceLogicalPath.ToLexicalPath(
                workspaceRoot,
                ".agents/project alpha/工作%20note.md"));
        Assert.Equal(
            Path.Combine(workspaceRoot, ".agents"),
            SourceLogicalPath.ToLexicalPath(workspaceRoot, ".agents"));
    }

    [Fact(DisplayName = "Neutral logical path conversion rejects root parents and unsafe source shapes")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ParentAndFileConversionKeepSourceOnlyPreconditions()
    {
        Assert.ThrowsAny<ArgumentException>(() => SourceLogicalPath.ReadParent(".agents"));
        Assert.ThrowsAny<ArgumentException>(() => SourceLogicalPath.ReadFileName(".agents"));
    }
}
