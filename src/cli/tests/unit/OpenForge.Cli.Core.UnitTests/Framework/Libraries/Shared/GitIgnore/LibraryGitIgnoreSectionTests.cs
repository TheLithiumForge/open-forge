using System.Text;
using OpenForge.Cli.Core.Framework.Libraries.Shared.GitIgnore;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Shared.GitIgnore;

[Trait("Feature", "library-git-ignore"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class LibraryGitIgnoreSectionTests
{
    [Theory(DisplayName = "Library ignore rules anchor and escape exact leaves")]
    [InlineData("a.md", "/a.md")]
    [InlineData("shared/a[1]*?.md", "/shared/a\\[1\\]\\*\\?.md")]
    [InlineData("notes/a #!.md ", "/notes/a\\ \\#\\!.md\\ ")]
    public void ExactLeafPattern(string path, string expected)
        => Assert.Equal(expected, LibraryGitIgnoreSection.Pattern(path));

    [Fact(DisplayName = "Library section preserves BOM authored bytes Install section and sibling rules")]
    public void PreservesAuthoredBytesAndInstall()
    {
        var authored = "\uFEFF# notes\r\n/shared/a.md\n# BEGIN OPEN FORGE INSTALL\r\n/.agents/memory/working/\r\n# END OPEN FORGE INSTALL\r\n";
        var actual = LibraryGitIgnoreSection.Rewrite(Encoding.UTF8.GetBytes(authored), [], ["shared/a.md", "root.md"]);
        Assert.Equal(authored + "# BEGIN OPEN FORGE LIBRARIES\r\n/root.md\r\n/shared/a.md\r\n# END OPEN FORGE LIBRARIES\r\n", Encoding.UTF8.GetString(actual));
    }

    [Fact(DisplayName = "Library section removes retired claims while preserving unrelated text exactly")]
    public void ReleasesOnlyOwnedRules()
    {
        var original = "prefix\n# BEGIN OPEN FORGE LIBRARIES\n/a.md\n/b.md\n# END OPEN FORGE LIBRARIES\nsuffix\r\n";
        Assert.Equal("prefix\n# BEGIN OPEN FORGE LIBRARIES\n/b.md\n# END OPEN FORGE LIBRARIES\nsuffix\r\n",
            Encoding.UTF8.GetString(LibraryGitIgnoreSection.Rewrite(Encoding.UTF8.GetBytes(original), ["a.md", "b.md"], ["b.md"])));
        Assert.Equal("prefix\nsuffix\r\n",
            Encoding.UTF8.GetString(LibraryGitIgnoreSection.Rewrite(Encoding.UTF8.GetBytes(original), ["a.md", "b.md"], [])));
    }

    [Theory(DisplayName = "Ambiguous duplicate and unsupported Library owned rules block reconciliation")]
    [InlineData("# BEGIN OPEN FORGE LIBRARIES\n/a.md\n")]
    [InlineData("# END OPEN FORGE LIBRARIES\n")]
    [InlineData("# BEGIN OPEN FORGE LIBRARIES\n/unknown.md\n# END OPEN FORGE LIBRARIES\n")]
    [InlineData("# BEGIN OPEN FORGE LIBRARIES\n/a.md\n/a.md\n# END OPEN FORGE LIBRARIES\n")]
    [InlineData("# BEGIN OPEN FORGE LIBRARIES\n# user note\n# END OPEN FORGE LIBRARIES\n")]
    [InlineData("# BEGIN OPEN FORGE LIBRARIES\n/a.md\n\n# END OPEN FORGE LIBRARIES\n")]
    [InlineData("# BEGIN OPEN FORGE LIBRARIES\n/a.md\n# END OPEN FORGE LIBRARIES\n# BEGIN OPEN FORGE LIBRARIES\n# END OPEN FORGE LIBRARIES\n")]
    public void RefusesUnknownOwnedContent(string text)
        => Assert.Throws<InvalidDataException>(() => LibraryGitIgnoreSection.Rewrite(Encoding.UTF8.GetBytes(text), ["a.md"], []));

    [Fact(DisplayName = "Opt-out without owned rules retains exact ignore bytes")]
    public void OptOutNoChange()
    {
        var bytes = Encoding.UTF8.GetBytes("# authored\r\n/shared/*\r\n");
        Assert.Equal(bytes, LibraryGitIgnoreSection.Rewrite(bytes, [], []));
    }
}
