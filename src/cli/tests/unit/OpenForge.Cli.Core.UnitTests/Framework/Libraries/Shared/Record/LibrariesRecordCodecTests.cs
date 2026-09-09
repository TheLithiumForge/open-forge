using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Shared.Record;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibrariesRecordCodecTests
{
    private const string ValidRecord = """
        {"schemaVersion":1,"libraries":[{"id":"team","sourceRoot":"shared/team","destinationRoot":".","paths":[".agents/directives/a.md",".agents/notes/z.txt"]}]}
        """;

    [Theory(DisplayName = "Strict Library decoding accepts only the complete schema-v1 record")]
    [InlineData("{\"schemaVersion\":1,\"libraries\":[]}", 0)]
    [InlineData(ValidRecord, 1)]
    public void ReadsExactRecord(string json, int count)
    {
        var result = LibrariesRecordCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Equal(LibrariesRecordReadState.Complete, result.State);
        var record = Assert.IsType<LibrariesRecord>(result.Record);
        Assert.Equal(1, record.SchemaVersion);
        Assert.Equal(count, record.Libraries.Length);
        Assert.Null(result.Cause);
        if (count == 1)
        {
            var library = Assert.Single(record.Libraries);
            Assert.Equal("team", library.Id.Value);
            Assert.Equal("shared/team", library.SourceRoot.Value);
            Assert.Equal([".agents/directives/a.md", ".agents/notes/z.txt"], library.Paths.Select(path => path.Value));
        }
    }

    public static TheoryData<string> MalformedRecords
    {
        get
        {
            TheoryData<string> records =
            [
                "", "null", "[]", "{}", "{", "{\"schemaVersion\":1}", "{\"libraries\":[]}",
                "{\"schemaVersion\":1,\"libraries\":null}", "{\"schemaVersion\":1,\"libraries\":{}}",
                "{\"schemaVersion\":1,\"libraries\":[null]}", "{\"schemaVersion\":1,\"libraries\":[{}]}",
            ];
            foreach (var version in new[] { "null", "0", "2", "1.0", "true", "\"1\"" })
            {
                records.Add(ValidRecord.Replace("\"schemaVersion\":1", $"\"schemaVersion\":{version}", StringComparison.Ordinal));
            }

            foreach (var extra in new[] { "\"schemaVersion\":1,", "\"libraries\":[],", "\"extra\":0," })
            {
                records.Add(ValidRecord.Insert(1, extra));
            }

            foreach (var property in new[] { "\"id\":\"team\"", "\"sourceRoot\":\"shared/team\"", "\"destinationRoot\":\".\"", "\"paths\":[\".agents/directives/a.md\",\".agents/notes/z.txt\"]" })
            {
                records.Add(ValidRecord.Replace(property, $"{property},{property}", StringComparison.Ordinal));
                records.Add(ValidRecord.Replace(property, $"{property},\"extra\":0", StringComparison.Ordinal));
                var key = property[..property.IndexOf(':')];
                foreach (var invalid in new[] { "null", "true", "17", "{}" })
                {
                    records.Add(ValidRecord.Replace(property, $"{key}:{invalid}", StringComparison.Ordinal));
                }
            }

            records.Add("{\"schemaVersion\":1,\"libraries\":[{\"sourceRoot\":\"shared/team\",\"destinationRoot\":\".\",\"paths\":[]}]}");
            records.Add("{\"schemaVersion\":1,\"libraries\":[{\"id\":\"team\",\"paths\":[]}]}");
            records.Add("{\"schemaVersion\":1,\"libraries\":[{\"id\":\"team\",\"sourceRoot\":\"shared/team\"}]}");
            foreach (var invalidPaths in new[] { "[null]", "[1]", "[true]", "[{}]", "\"path\"", "[\".agents/z.md\",\".agents/a.md\"]", "[\".agents/a.md\",\".agents/a.md\"]" })
            {
                records.Add(ValidRecord.Replace("[\".agents/directives/a.md\",\".agents/notes/z.txt\"]", invalidPaths, StringComparison.Ordinal));
            }

            foreach (var invalidId in new[] { "TEAM", "a--b", "a_b", "-a", "a-", "é", "" })
            {
                records.Add(ValidRecord.Replace("\"team\"", $"\"{invalidId}\"", StringComparison.Ordinal));
            }

            foreach (var source in new[] { "../team", "shared/../team", "/team", "shared//team", "shared/./team", "shared/team/" })
            {
                records.Add(ValidRecord.Replace("shared/team", source, StringComparison.Ordinal));
            }

            foreach (var path in new[]
            {
                "../a.md", ".", ".agents/../a.md", ".agents//a.md", ".agents/a.md/",
                ".agents/loader.md", ".agents/open-forge.libraries.json", ".agents/directives/_directives.md", ".agents/directives/a.overwrite.md",
            })
            {
                records.Add(ValidRecord.Replace(".agents/directives/a.md", path, StringComparison.Ordinal));
            }

            foreach (var ids in new[] { new[] { "z", "a" }, ["a", "a"] })
            {
                records.Add($$"""
                    {"schemaVersion":1,"libraries":[
                     {"id":"{{ids[0]}}","sourceRoot":"one","destinationRoot":".","paths":[]},
                     {"id":"{{ids[1]}}","sourceRoot":"two","destinationRoot":".","paths":[]}]}
                    """);
            }

            records.Add("""
                {"schemaVersion":1,"libraries":[
                 {"id":"a","sourceRoot":"one","destinationRoot":".","paths":[".agents/a.md"]},
                 {"id":"b","sourceRoot":"two","destinationRoot":".","paths":[".agents/a.md"]}]}
                """);
            return records;
        }
    }

    [Theory(DisplayName = "Malformed Library records never grant partial record authority"), MemberData(nameof(MalformedRecords))]
    public void RejectsMalformedRecord(string json)
    {
        var result = LibrariesRecordCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Equal(LibrariesRecordReadState.Malformed, result.State);
        Assert.Null(result.Record);
        Assert.False(string.IsNullOrWhiteSpace(result.Cause));
    }

    [Fact(DisplayName = "Library writer emits exactly schema-v1 fields and deterministic ordinal identities")]
    public void WritesExactDeterministicSchema()
    {
        var record = LibrariesRecord.Create([
            LibraryRecord.Create(LibraryId.Create("a"), WorkspaceRelativeDirectory.Create("shared/a"), LibraryDestinationRoot.Create("."),
                [SourceRelativeEligiblePath.Create(".agents/a.md"), SourceRelativeEligiblePath.Create(".agents/z.md")]),
            LibraryRecord.Create(LibraryId.Create("b"), WorkspaceRelativeDirectory.Create("shared/b"), LibraryDestinationRoot.Create("."), []),
        ]);

        var bytes = LibrariesRecordCodec.Write(record);

        Assert.Equal(bytes, LibrariesRecordCodec.Write(record));
        using var json = JsonDocument.Parse(bytes);
        Assert.Equal(["schemaVersion", "libraries"], json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        var libraries = json.RootElement.GetProperty("libraries").EnumerateArray().ToArray();
        Assert.Equal(["a", "b"], libraries.Select(library => library.GetProperty("id").GetString()));
        Assert.All(libraries, library => Assert.Equal(["id", "sourceRoot", "destinationRoot", "paths"], library.EnumerateObject().Select(property => property.Name)));
        Assert.Equal("shared/a", libraries[0].GetProperty("sourceRoot").GetString());
        Assert.Equal([".agents/a.md", ".agents/z.md"], libraries[0].GetProperty("paths").EnumerateArray().Select(path => path.GetString()));
        Assert.Empty(libraries[1].GetProperty("paths").EnumerateArray());
    }

    [Fact]
    public void MappedRecordKeepsSourceSuffixSeparateFromDestinationRoot()
    {
        const string json = """{"schemaVersion":1,"libraries":[{"id":"team","sourceRoot":"shared/team","destinationRoot":"docs","paths":["README.md","nested/_nested.md"]}]}""";

        var read = LibrariesRecordCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Equal(LibrariesRecordReadState.Complete, read.State);
        var library = Assert.Single(Assert.IsType<LibrariesRecord>(read.Record).Libraries);
        Assert.Equal("docs", library.DestinationRoot.Value);
        Assert.Equal(["README.md", "nested/_nested.md"], library.Paths.Select(value => value.Value));
    }

    [Fact]
    public void MissingDestinationRootCannotReadAsImplicitLegacyRoot()
    {
        const string json = """{"schemaVersion":1,"libraries":[{"id":"team","sourceRoot":"shared/team","paths":[]}]}""";

        Assert.Equal(LibrariesRecordReadState.Malformed, LibrariesRecordCodec.Read(Encoding.UTF8.GetBytes(json)).State);
    }
}
