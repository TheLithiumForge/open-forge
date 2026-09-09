using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Models.Identity;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryIdentityTests
{
    [Theory(DisplayName = "Library IDs preserve strict lowercase ASCII segment identity")]
    [InlineData("a"), InlineData("0"), InlineData("team-knowledge"), InlineData("a-1-b")]
    public void AcceptsIds(string value)
        => Assert.Equal(value, LibraryId.Create(value).Value);

    [Fact(DisplayName = "Library ID length accepts 128 characters and rejects 129")]
    public void EnforcesLengthBoundary()
    {
        Assert.Equal(new string('a', 128), LibraryId.Create(new string('a', 128)).Value);
        Assert.Throws<ArgumentException>(() => LibraryId.Create(new string('a', 129)));
    }

    [Theory(DisplayName = "Library IDs reject invalid segments and non-ASCII characters")]
    [InlineData(""), InlineData(" "), InlineData("A"), InlineData("a_b"), InlineData("a--b")]
    [InlineData("-a"), InlineData("a-"), InlineData("é"), InlineData("a/b"), InlineData("a.b")]
    public void RejectsIds(string value)
        => Assert.Throws<ArgumentException>(() => LibraryId.Create(value));

    [Theory(DisplayName = "Library paths reject noncanonical workspace-relative spellings")]
    [InlineData(""), InlineData("/shared"), InlineData("shared/"), InlineData("shared//a")]
    [InlineData("."), InlineData(".."), InlineData("shared/../a"), InlineData("shared/./a"), InlineData("shared\\a")]
    public void RejectsNoncanonicalDirectories(string value)
        => Assert.Throws<ArgumentException>(() => WorkspaceRelativeDirectory.Create(value));

    [Theory(DisplayName = "Library file paths reject noncanonical portable leaf spellings")]
    [InlineData("."), InlineData("../a.md"), InlineData("/a.md"), InlineData(".agents/../a.md")]
    [InlineData(".agents//a.md"), InlineData(".agents/a.md/"), InlineData(".agents\\a.md")]
    public void RejectsNoncanonicalFiles(string value)
    {
        Assert.Throws<ArgumentException>(() => SourceRelativeEligiblePath.Create(value));
        Assert.Throws<ArgumentException>(() => WorkspaceRelativeEligiblePath.Create(value));
    }

    [Theory(DisplayName = "Library mappings retain identical destinations and exact raw relative targets")]
    [InlineData("shared/team", ".agents/a.md", "../shared/team/.agents/a.md")]
    [InlineData("shared/team", ".agents/directives/a.md", "../../shared/team/.agents/directives/a.md")]
    public void DerivesMapping(string root, string path, string rawTarget)
    {
        var mapping = LibraryPathIdentity.Map(WorkspaceRelativeDirectory.Create(root), LibraryDestinationRoot.Create("."), SourceRelativeEligiblePath.Create(path));

        Assert.Equal(path, mapping.SourcePath.Value);
        Assert.Equal(path, mapping.DestinationPath.Value);
        Assert.Equal(rawTarget, mapping.ExpectedRelativeLink.Value);
    }

    [Fact(DisplayName = "Library records reject duplicate and unordered IDs or destinations")]
    public void RejectsDuplicateAndUnorderedIdentity()
    {
        var a = LibraryRecord.Create(LibraryId.Create("a"), WorkspaceRelativeDirectory.Create("one"), LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(".agents/a.md")]);
        var b = LibraryRecord.Create(LibraryId.Create("b"), WorkspaceRelativeDirectory.Create("two"), LibraryDestinationRoot.Create("."), [SourceRelativeEligiblePath.Create(".agents/a.md")]);
        Assert.Throws<ArgumentException>(() => LibrariesRecord.Create([a, a]));
        Assert.Throws<ArgumentException>(() => LibrariesRecord.Create([b, a]));
        Assert.Throws<ArgumentException>(() => LibrariesRecord.Create([a, b]));
        Assert.Throws<ArgumentException>(() => LibraryRecord.Create(a.Id, a.SourceRoot, a.DestinationRoot,
            [SourceRelativeEligiblePath.Create(".agents/z.md"), SourceRelativeEligiblePath.Create(".agents/a.md")]));
        Assert.Throws<ArgumentException>(() => LibraryRecord.Create(a.Id, a.SourceRoot, a.DestinationRoot, [a.Paths[0], a.Paths[0]]));
    }

    [Theory]
    [InlineData("README.md"), InlineData("docs/_docs.md"), InlineData("docs/a.overwrite.md"), InlineData(".agents/a.md")]
    public void AcceptsOrdinarySourceRelativeLeaves(string path)
    {
        Assert.Equal(path, SourceRelativeEligiblePath.Create(path).Value);
        Assert.Equal(path, WorkspaceRelativeEligiblePath.Create(path).Value);
    }

    [Theory]
    [InlineData(".", "README.md", "README.md", "shared/team/README.md")]
    [InlineData("docs", "README.md", "docs/README.md", "../shared/team/README.md")]
    [InlineData(".apm/agents/team", "review.md", ".apm/agents/team/review.md", "../../../shared/team/review.md")]
    [InlineData(".agents/directives", "review.md", ".agents/directives/review.md", "../../shared/team/review.md")]
    [InlineData("shared", "docs/a.md", "shared/docs/a.md", "../team/docs/a.md")]
    public void DerivesMappedLeafFromBothRoots(string destinationRoot, string sourcePath, string destination, string rawTarget)
    {
        var mapping = LibraryPathIdentity.Map(WorkspaceRelativeDirectory.Create("shared/team"),
            LibraryDestinationRoot.Create(destinationRoot), SourceRelativeEligiblePath.Create(sourcePath));

        Assert.Equal(destination, mapping.DestinationPath.Value);
        Assert.Equal(sourcePath, mapping.SourcePath.Value);
        Assert.Equal(rawTarget, mapping.ExpectedRelativeLink.Value);
    }

    [Theory]
    [InlineData("../docs"), InlineData("/docs"), InlineData("docs/"), InlineData("docs//nested")]
    [InlineData("docs/./nested"), InlineData("docs/CON.txt"), InlineData("docs/..")]
    public void DestinationRootRejectsUnsafeSpelling(string value)
        => Assert.Throws<ArgumentException>(() => LibraryDestinationRoot.Create(value));

    [Fact]
    public void WorkspaceRootDestinationDoesNotBroadenSourceRootGrammar()
    {
        Assert.Equal(".", LibraryDestinationRoot.Create(".").Value);
        Assert.Throws<ArgumentException>(() => WorkspaceRelativeDirectory.Create("."));
    }

    [Theory]
    [InlineData("docs", "review.md", ".", "docs/review.md")]
    [InlineData("docs", "review.md", "DOCS", "REVIEW.md")]
    public void DistinctSourceSuffixesCannotOwnTheSamePortableMappedLeaf(
        string firstRoot, string firstPath, string secondRoot, string secondPath)
    {
        var first = LibraryRecord.Create(LibraryId.Create("first"), WorkspaceRelativeDirectory.Create("shared/first"),
            LibraryDestinationRoot.Create(firstRoot), [SourceRelativeEligiblePath.Create(firstPath)]);
        var second = LibraryRecord.Create(LibraryId.Create("second"), WorkspaceRelativeDirectory.Create("shared/second"),
            LibraryDestinationRoot.Create(secondRoot), [SourceRelativeEligiblePath.Create(secondPath)]);

        Assert.Throws<ArgumentException>(() => LibrariesRecord.Create([first, second]));
    }

    [Fact]
    public void LibrariesCanShareRealDestinationParentsWithoutSharingLeaves()
    {
        var first = LibraryRecord.Create(LibraryId.Create("first"), WorkspaceRelativeDirectory.Create("shared/first"),
            LibraryDestinationRoot.Create("docs"), [SourceRelativeEligiblePath.Create("a.md")]);
        var second = LibraryRecord.Create(LibraryId.Create("second"), WorkspaceRelativeDirectory.Create("shared/second"),
            LibraryDestinationRoot.Create("docs"), [SourceRelativeEligiblePath.Create("b.md")]);

        Assert.Equal(2, LibrariesRecord.Create([first, second]).Libraries.Length);
    }
}
