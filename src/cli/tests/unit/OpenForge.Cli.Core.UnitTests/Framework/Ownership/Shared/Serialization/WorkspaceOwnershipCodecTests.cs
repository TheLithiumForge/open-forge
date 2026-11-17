using System.Text;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Ownership.Shared.Serialization;

[Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit")]
public sealed class WorkspaceOwnershipCodecTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Every recorded section round-trips through a write and a read")]
    public void RoundTripsEverySection()
    {
        var written = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            new FrameworkOwnership(
                new OwnedSource("open-forge", "0.1.0"),
                [".agents/loader.md", "AGENTS.md"],
                [new OwnedRegion(".agents/directives/_directives.md", "entries")]),
            [
                new ExtensionOwnership(
                    "planning",
                    "0.1.0",
                    "bundled",
                    ["development"],
                    [".agents/templates/planning/plan.md"],
                    [new OwnedRegion(".agents/templates/planning/_planning.md", "entries")]),
            ],
            [new LibraryOwnership("shared", "libs/shared", ".agents", ["patterns/api.md"])]);

        var read = WorkspaceOwnershipCodec.Read(WorkspaceOwnershipCodec.Write(written));

        var document = Assert.IsType<WorkspaceOwnershipDocument>(read.Document);
        Assert.Null(read.Cause);
        Assert.Equal(written.SchemaVersion, document.SchemaVersion);

        // Records hold ImmutableArray members, whose default equality is by
        // underlying reference rather than by contents, so the sections are
        // compared element by element.
        var framework = Assert.IsType<FrameworkOwnership>(document.Framework);
        Assert.Equal(written.Framework!.Source, framework.Source);
        Assert.Equal(written.Framework.Paths, framework.Paths);
        Assert.Equal(written.Framework.Regions, framework.Regions);

        var extension = Assert.Single(document.Extensions);
        var writtenExtension = Assert.Single(written.Extensions);
        Assert.Equal(writtenExtension.Id, extension.Id);
        Assert.Equal(writtenExtension.Version, extension.Version);
        Assert.Equal(writtenExtension.Source, extension.Source);
        Assert.Equal(writtenExtension.Dependencies, extension.Dependencies);
        Assert.Equal(writtenExtension.Paths, extension.Paths);
        Assert.Equal(writtenExtension.Regions, extension.Regions);

        var library = Assert.Single(document.Libraries);
        var writtenLibrary = Assert.Single(written.Libraries);
        Assert.Equal(writtenLibrary.Id, library.Id);
        Assert.Equal(writtenLibrary.SourceRoot, library.SourceRoot);
        Assert.Equal(writtenLibrary.DestinationRoot, library.DestinationRoot);
        Assert.Equal(writtenLibrary.Paths, library.Paths);
    }

    /// <summary>
    /// The lock is rebuilt best-effort when it cannot be trusted, so a file that
    /// omits a section records nothing for it rather than failing the read.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "An absent or null section records no ownership")]
    [InlineData("{}")]
    [InlineData("""{"framework":null,"extensions":null,"libraries":null}""")]
    [InlineData("""{"extensions":[],"libraries":[]}""")]
    public void ReadsMissingSectionsAsEmpty(string json)
    {
        var read = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceOwnershipDocument>(read.Document);
        Assert.Null(document.Framework);
        Assert.Empty(document.Extensions);
        Assert.Empty(document.Libraries);
    }

    /// <summary>
    /// A key this release has not heard of means a newer one wrote the file.
    /// Refusing it would make the lock a gate, which the state-file decision
    /// removed.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An unknown key is accepted and ignored")]
    public void AcceptsUnknownKeys()
    {
        const string json = """
            {"schemaVersion":1,"signatures":{"framework":"…"},"extensions":[{"id":"planning","notes":"…"}]}
            """;

        var read = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceOwnershipDocument>(read.Document);
        Assert.Null(read.Cause);
        Assert.Equal("planning", Assert.Single(document.Extensions).Id);
    }

    /// <summary>
    /// The version is a fact to report, not a gate. A lock written by a shape this
    /// release does not know still yields the entries it does understand.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "An unrecognised schema version is still read")]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(99)]
    public void ReadsUnrecognisedSchemaVersion(int version)
    {
        var json = $$"""
            {"schemaVersion":{{version}},"extensions":[{"id":"planning","paths":["a.md"]}]}
            """;

        var read = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceOwnershipDocument>(read.Document);
        Assert.Null(read.Cause);
        Assert.Equal(version, document.SchemaVersion);
        Assert.False(document.IsKnownSchemaVersion);
        Assert.Equal(["a.md"], Assert.Single(document.Extensions).Paths);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "A document with no identity to act on is reported rather than guessed at")]
    [InlineData("[]", "must contain a JSON object")]
    [InlineData("\"text\"", "must contain a JSON object")]
    [InlineData("""{"extensions":[{"paths":["a.md"]}]}""", "requires an id")]
    [InlineData("""{"framework":{"paths":["a.md"]}}""", "requires a source id")]
    [InlineData("""{"libraries":[{"id":"shared","paths":[]}]}""", "requires an id, source root")]
    [InlineData("""{"framework":{"source":{"id":"open-forge"},"regions":[{"path":"a.md"}]}}""", "requires a path and region")]
    [InlineData("""{"extensions":[{"id":"planning","paths":[" "]}]}""", "must not record an empty entry")]
    public void ReportsUnintelligibleDocuments(string json, string expectedCause)
    {
        var read = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(read.Document);
        Assert.Contains(expectedCause, read.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Invalid UTF-8 is reported as an encoding fault")]
    public void ReportsInvalidUtf8()
    {
        var read = WorkspaceOwnershipCodec.Read(new byte[] { 0x7B, 0xFF, 0xFE, 0x7D });

        Assert.Null(read.Document);
        Assert.NotNull(read.Cause);
    }

    /// <summary>
    /// The lock is reviewed in a diff, so two runs that owned the same things must
    /// produce the same bytes regardless of the order the receipts arrived in.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Recorded order does not change the written bytes")]
    public void WritesDeterministicallyRegardlessOfOrder()
    {
        var first = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            Framework: null,
            [
                new ExtensionOwnership("planning", "1.0", null, ["b", "a"], ["z.md", "a.md"], []),
                new ExtensionOwnership("development", "1.0", null, [], ["m.md"], []),
            ],
            [
                new LibraryOwnership("second", "s", "d", ["q.md"]),
                new LibraryOwnership("first", "s", "d", ["p.md"]),
            ]);

        var second = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            Framework: null,
            [
                new ExtensionOwnership("development", "1.0", null, [], ["m.md"], []),
                new ExtensionOwnership("planning", "1.0", null, ["a", "b"], ["a.md", "z.md"], []),
            ],
            [
                new LibraryOwnership("first", "s", "d", ["p.md"]),
                new LibraryOwnership("second", "s", "d", ["q.md"]),
            ]);

        Assert.Equal(WorkspaceOwnershipCodec.Write(first), WorkspaceOwnershipCodec.Write(second));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A path recorded twice by one owner is written once")]
    public void CollapsesDuplicatePaths()
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            Framework: null,
            [new ExtensionOwnership("planning", null, null, [], ["a.md", "a.md"], [])],
            []);

        var read = WorkspaceOwnershipCodec.Read(WorkspaceOwnershipCodec.Write(document));

        var extension = Assert.Single(Assert.IsType<WorkspaceOwnershipDocument>(read.Document).Extensions);
        Assert.Equal(["a.md"], extension.Paths);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A null version or source is omitted rather than written as null")]
    public void OmitsAbsentOptionalValues()
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            Framework: null,
            [new ExtensionOwnership("planning", Version: null, Source: null, [], ["a.md"], [])],
            []);

        var json = Encoding.UTF8.GetString(WorkspaceOwnershipCodec.Write(document));

        Assert.DoesNotContain("\"version\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"source\"", json, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A written lock declares its schema and version first")]
    public void WrittenLockDeclaresItself()
    {
        var json = Encoding.UTF8.GetString(
            WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty));

        Assert.Contains(WorkspaceOwnershipDefinitions.SchemaUrl, json, StringComparison.Ordinal);
        Assert.Contains("\"schemaVersion\": 1", json, StringComparison.Ordinal);
        Assert.True(
            json.IndexOf("$schema", StringComparison.Ordinal)
                < json.IndexOf("schemaVersion", StringComparison.Ordinal),
            "The schema declaration belongs at the top, where somebody opening the file sees it.");
    }
}
