using System.Text;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Shared.Observation;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRegistrationReaderTests
{
    [Trait("Boundary", "Input")]
    [Fact]
    public void CanonicalizesUnorderedLockRegistrationsWithoutChangingMappings()
    {
        var input = Document(
            new("z", "shared/z", "other", ["b.md", "a.md"]),
            new("a", "shared/a", "docs", ["nested/_nested.md", "README.md"]));
        var result = LibraryRegistrationReader.ReadRegistrations(input);
        Assert.Equal(["a", "z"], result.Libraries.Select(library => library.Id.Value));
        var first = result.Libraries[0];
        Assert.Equal("docs", first.DestinationRoot.Value);
        Assert.Equal(["README.md", "nested/_nested.md"], first.Paths.Select(path => path.Value));
    }

    [Trait("Boundary", "Input")]
    [Theory]
    [InlineData("TEAM"), InlineData("a--b"), InlineData("a_b"), InlineData("-a"), InlineData("a-")]
    public void InvalidIdsProvideInformationWithoutPartialRegistrations(string id)
        => AssertUnknown(Document(new LibraryOwnership(id, "shared/team", "docs", ["a.md"])));

    [Trait("Boundary", "Input")]
    [Theory]
    [InlineData("../team"), InlineData("shared/../team"), InlineData("/team"), InlineData("shared//team")]
    public void InvalidSourceRootsCannotSupplyDeletionMappings(string source)
        => AssertUnknown(Document(new LibraryOwnership("team", source, "docs", ["a.md"])));

    [Trait("Boundary", "Input")]
    [Theory]
    [InlineData("../a.md"), InlineData(".agents/../a.md"), InlineData(".agents//a.md")]
    public void InvalidPathsCannotSupplyDeletionMappings(string path)
        => AssertUnknown(Document(new LibraryOwnership("team", "shared/team", "docs", [path])));

    [Trait("Boundary", "Input")]
    [Theory]
    [InlineData("a", "a", "docs", "other")]
    [InlineData("a", "b", "docs", "docs")]
    [InlineData("a", "b", "Docs", "docs")]
    public void DuplicateIdentityOrMappedDestinationDeclinesTheWholeSection(string firstId, string secondId, string firstRoot, string secondRoot)
        => AssertUnknown(Document(
            new(firstId, "one", firstRoot, ["a.md"]),
            new(secondId, "two", secondRoot, ["a.md"])));

    [Trait("Boundary", "Input")]
    [Fact]
    public void DistinctMappedDestinationsMayShareTheSameSourceSuffix()
    {
        var result = LibraryRegistrationReader.ReadRegistrations(Document(
            new("a", "one", "docs", ["a.md"]), new("b", "two", "other", ["a.md"])));
        Assert.Equal(["docs", "other"], result.Libraries.Select(library => library.DestinationRoot.Value));
    }

    [Trait("Boundary", "Input")]
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"schemaVersion\":999,\"libraries\":null,\"future\":true}")]
    public void UsesTheForgivingLockCodecWithoutRecreatingStrictLegacySchema(string json)
    {
        var decoded = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(json));
        var document = Assert.IsType<WorkspaceOwnershipDocument>(decoded.Document);
        Assert.Empty(LibraryRegistrationReader.ReadRegistrations(document).Libraries);
    }

    private static WorkspaceOwnershipDocument Document(params LibraryOwnership[] libraries)
        => new(1, null, [], [.. libraries]);

    private static void AssertUnknown(WorkspaceOwnershipDocument document)
    {
        var result = LibraryRegistrationReader.Read(new(WorkspaceOwnershipReadState.Complete,
            document, Path.GetFullPath(".agents/open-forge.lock.json"), null, null));
        Assert.Equal(LibraryRegistrationReadState.Malformed, result.State);
        Assert.Null(result.Record);
        Assert.False(string.IsNullOrWhiteSpace(result.OwnershipObservation));
    }
}
