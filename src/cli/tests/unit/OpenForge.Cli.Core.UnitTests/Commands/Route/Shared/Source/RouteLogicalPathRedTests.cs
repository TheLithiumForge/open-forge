using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Source;

public sealed class RouteLogicalPathRedTests
{
    [Theory(DisplayName = "Shared logical paths accept canonical slash-separated source paths"),
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
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CanonicalValidationRejectsUnsafeLogicalPaths(string path, bool expected)
    {
        Assert.Equal(expected, RouteLogicalPath.IsCanonical(path));
    }

    [Theory(DisplayName = "Shared logical paths read exact parent and file segments"),
        InlineData(".agents/root/_root.md", ".agents/root", "_root.md"),
        InlineData(".agents/root/child/grand.md", ".agents/root/child", "grand.md"),
        InlineData(".agents/project alpha/工作%20note.md", ".agents/project alpha", "工作%20note.md")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ParentAndFileConversionPreserveText(
        string logicalPath,
        string expectedParent,
        string expectedFileName)
    {
        Assert.Equal(expectedParent, RouteLogicalPath.ReadParent(logicalPath));
        Assert.Equal(expectedFileName, RouteLogicalPath.ReadFileName(logicalPath));
    }

    [Fact(DisplayName = "Shared logical paths convert canonical paths to one lexical workspace path")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void LexicalConversionUsesTheWorkspaceRoot()
    {
        var workspaceRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-logical-path-unit"));

        var lexical = RouteLogicalPath.ToLexicalPath(
            workspaceRoot,
            ".agents/project alpha/工作%20note.md");

        Assert.Equal(
            Path.Combine(workspaceRoot, ".agents", "project alpha", "工作%20note.md"),
            lexical);
    }

    [Fact(DisplayName = "Shared logical paths reject parent and file conversion outside canonical source paths")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ParentAndFileConversionRejectInvalidInputs()
    {
        Assert.ThrowsAny<ArgumentException>(() => RouteLogicalPath.ReadParent(".agents"));
        Assert.ThrowsAny<ArgumentException>(() => RouteLogicalPath.ReadFileName(".agents"));
        Assert.ThrowsAny<ArgumentException>(() => RouteLogicalPath.ReadParent(".agents/root/../file.md"));
        Assert.ThrowsAny<ArgumentException>(() => RouteLogicalPath.ReadFileName(".agents/root\\file.md"));
    }
}
