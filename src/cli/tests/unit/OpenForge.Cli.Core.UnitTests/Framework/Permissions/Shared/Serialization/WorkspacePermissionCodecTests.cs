using System.Text;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Permissions.Shared.Serialization;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionCodecTests
{
    private const string EmptyDocument = """{"schemaVersion":1,"extensions":[],"libraries":[]}""";

    [Fact]
    public void ReadsBothSubjectKindsWithoutRequiringAuthoredSortOrder()
    {
        const string json = """
            {"libraries":[{"paths":["z.txt","a.txt"],"sourceRoot":"shared/team","id":"team"}],
             "extensions":[{"id":"z","paths":[".apm/z.md",".apm/a.md"]},{"id":"a","paths":[]}],"schemaVersion":1}
            """;

        var result = WorkspacePermissionCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspacePermissionDocument>(result.Document);
        Assert.Null(result.Cause);
        Assert.Equal(["a", "z"], document.Extensions.Select(value => value.Id).Order(StringComparer.Ordinal));
        var library = Assert.Single(document.Libraries);
        Assert.Equal("shared/team", library.SourceRoot);
        Assert.Equal(["a.txt", "z.txt"], library.Paths.Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":null,\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":2,\"extensions\":[],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1.0,\"extensions\":[],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"schemaVersion\":1,\"extensions\":[],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[],\"libraries\":[],\"extra\":true}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[{\"id\":\"team\",\"id\":\"team\",\"paths\":[]}],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[{\"id\":\"team\",\"paths\":[],\"sourceRoot\":\"x\"}],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[],\"libraries\":[{\"id\":\"team\",\"paths\":[]}]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[{\"id\":\"team\",\"paths\":[]},{\"id\":\"team\",\"paths\":[]}],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[{\"id\":\"team--x\",\"paths\":[]}],\"libraries\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"extensions\":[],\"libraries\":[],}")]
    public void RejectsIncompleteOrAmbiguousDocument(string json)
    {
        var result = WorkspacePermissionCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(result.Document);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Theory]
    [InlineData(".agents/a.md")]
    [InlineData(".AGENTS/a.md")]
    [InlineData("../a.md")]
    [InlineData("/a.md")]
    [InlineData("a//b.md")]
    [InlineData("a/./b.md")]
    [InlineData("a/*.md")]
    [InlineData("a/")]
    [InlineData("a/CON.txt")]
    [InlineData("a/file. ")]
    [InlineData("a/e\u0301.md")]
    public void RejectsNoncanonicalOrInternalGrantPath(string path)
    {
        var json = $$"""{"schemaVersion":1,"extensions":[{"id":"team","paths":["{{path}}"]}],"libraries":[]}""";

        var result = WorkspacePermissionCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(result.Document);
        Assert.NotNull(result.Cause);
    }

    [Theory]
    [InlineData("\"a.md\",\"A.md\"")]
    [InlineData("\"a.md\",\"a.md\"")]
    public void RejectsPortableDuplicatesWithinOneGrant(string paths)
    {
        var json = $$"""{"schemaVersion":1,"extensions":[{"id":"team","paths":[{{paths}}]}],"libraries":[]}""";

        Assert.Null(WorkspacePermissionCodec.Read(Encoding.UTF8.GetBytes(json)).Document);
    }

    [Fact]
    public void WritesCanonicalDocumentAgainstIndependentLiteralOracle()
    {
        var document = new WorkspacePermissionDocument(
            Extensions: [new("z", ["z.txt", "a.txt"]), new("a", [])],
            Libraries: [new("team", "shared/team", [".apm/agents/reviewer.md"])]);
        const string expected = """
            {
              "schemaVersion": 1,
              "extensions": [
                {
                  "id": "a",
                  "paths": []
                },
                {
                  "id": "z",
                  "paths": [
                    "a.txt",
                    "z.txt"
                  ]
                }
              ],
              "libraries": [
                {
                  "id": "team",
                  "sourceRoot": "shared/team",
                  "paths": [
                    ".apm/agents/reviewer.md"
                  ]
                }
              ]
            }
            """;

        Assert.Equal(expected + "\n", Encoding.UTF8.GetString(WorkspacePermissionCodec.Write(document)));
    }

    [Fact]
    public void ReadsExactEmptyDocument()
    {
        var document = Assert.IsType<WorkspacePermissionDocument>(
            WorkspacePermissionCodec.Read(Encoding.UTF8.GetBytes(EmptyDocument)).Document);

        Assert.Empty(document.Extensions);
        Assert.Empty(document.Libraries);
    }

    [Fact]
    public void RejectsInvalidUtf8InsideAnOtherwiseValidPermissionString()
    {
        var prefix = Encoding.UTF8.GetBytes("{\"schemaVersion\":1,\"extensions\":[{\"id\":\"team\",\"paths\":[\"");
        var suffix = Encoding.UTF8.GetBytes(".txt\"]}],\"libraries\":[]}");
        byte[] bytes = [.. prefix, 0xff, .. suffix];

        Assert.Null(WorkspacePermissionCodec.Read(bytes).Document);
    }
}
