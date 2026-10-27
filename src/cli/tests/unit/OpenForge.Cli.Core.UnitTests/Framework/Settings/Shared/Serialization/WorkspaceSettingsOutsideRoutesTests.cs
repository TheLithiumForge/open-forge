using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class WorkspaceSettingsOutsideRoutesTests
{
    private const string MainRoot = "context/forge";
    private const string Team = "docs/team";
    private const string Notes = "notes";
    private const string FutureProperty = "future";
    private const string FutureValue = "retained";
    private const string ExistingSchema = "https://example.invalid/authored.json";

    [Fact(DisplayName = "Missing outside routes decode as empty independently of file roots")]
    public void AbsentMeansEmpty()
    {
        var document = Decode("{}"u8.ToArray());
        Assert.Empty(document.OutsideRoutes);
        Assert.Empty(WorkspaceSettingsDocument.Empty.OutsideRoutes);
        var file = WorkspaceRoutePath.CanonicalEntrypointPath(Team);
        var registered = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRoot(ReadOnlyMemory<byte>.Empty, file));
        Assert.Equal([file], Decode(registered).Roots);
        Assert.Empty(Decode(registered).OutsideRoutes);
    }

    [Theory(DisplayName = "Outside routes decode in order with lower higher current and absent schema declarations")]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(WorkspaceSettingsDefinitions.SchemaVersion)]
    [InlineData(WorkspaceSettingsDefinitions.SchemaVersion + 1)]
    public void TolerantVersions(int? version)
    {
        var settings = Settings(Team, Notes);
        if (version is not null) settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = version.Value;
        var document = Decode(Bytes(settings));
        Assert.Equal([Team, Notes], document.OutsideRoutes);
        Assert.Equal(version ?? WorkspaceSettingsDefinitions.SchemaVersion, document.SchemaVersion);
        Assert.Equal(version is null or >= 1 and <= WorkspaceSettingsDefinitions.SchemaVersion, document.IsKnownSchemaVersion);
        Assert.Empty(document.Roots);
    }

    [Theory(DisplayName = "Outside routes reject malformed property and entry shapes")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("1")]
    [InlineData("\"docs/team\"")]
    [InlineData("{}")]
    [InlineData("[null]")]
    [InlineData("[1]")]
    [InlineData("[{}]")]
    public void WrongShape(string value)
    {
        var bytes = Encoding.UTF8.GetBytes($$"""{"{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":{{value}}}""");
        var read = WorkspaceSettingsCodec.Read(bytes);
        Assert.Null(read.Document);
        Assert.NotNull(read.Cause);
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.AddOutsideRoute(bytes, Notes));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.RemoveOutsideRoute(bytes, Notes));
    }

    public static TheoryData<string> InvalidDirectories => new()
    {
        string.Empty, ".", "./" + Team, Team + "/", "docs//team", "docs/../team", "docs\\team", "/docs", "C:/docs", "docs/NUL",
        MainRoot, MainRoot.ToUpperInvariant() + "/notes", "context", "CONTEXT",
        WorkspaceLayout.DefaultMainRoot, WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/notes",
        WorkspaceRuntimePaths.DirectoryName, WorkspaceRuntimePaths.DirectoryName.ToUpperInvariant() + "/notes",
        WorkspaceLayoutNames.GitMetadataDirectoryName, Notes + "/" + WorkspaceLayoutNames.GitMetadataDirectoryName.ToUpperInvariant(),
    };

    [Theory(DisplayName = "Decode and mutations share canonical outside directory validation")]
    [MemberData(nameof(InvalidDirectories))]
    public void InvalidDirectory(string directory)
    {
        Assert.Null(WorkspaceSettingsCodec.Read(Bytes(Settings(directory))).Document);
        var existing = Bytes(Settings());
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.AddOutsideRoute(existing, directory));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.RemoveOutsideRoute(existing, directory));
    }

    [Theory(DisplayName = "Decode rejects symmetric outside overlaps including portable case aliases")]
    [InlineData(Team, Team)]
    [InlineData(Team, "DOCS/TEAM")]
    [InlineData(Team, "docs")]
    [InlineData(Team, Team + "/notes")]
    [InlineData(Team, "DOCS/TEAM/notes")]
    public void SymmetricOverlap(string first, string second)
    {
        Assert.Null(WorkspaceSettingsCodec.Read(Bytes(Settings(first, second))).Document);
        Assert.Null(WorkspaceSettingsCodec.Read(Bytes(Settings(second, first))).Document);
        if (first == second)
        {
            Assert.Null(WorkspaceSettingsCodec.AddOutsideRoute(Bytes(Settings(first)), second));
            return;
        }
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.AddOutsideRoute(Bytes(Settings(first)), second));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.AddOutsideRoute(Bytes(Settings(second)), first));
    }

    [Fact(DisplayName = "Adding outside routes declares v5 preserves authored order and is idempotent")]
    public void AddDeclaresFeature()
    {
        var settings = Settings();
        settings[WorkspaceSettingsDefinitions.SchemaProperty] = ExistingSchema;
        settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = 1;
        var beforeKeys = settings.Select(property => property.Key).ToArray();
        var original = Bytes(settings);
        var preserved = original.ToArray();
        var first = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddOutsideRoute(original, Team));
        Assert.Equal(preserved, original);
        using var json = JsonDocument.Parse(first);
        Assert.Equal(beforeKeys, json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(FutureValue, json.RootElement.GetProperty(FutureProperty).GetString());
        var firstCopy = first.ToArray();
        Assert.Null(WorkspaceSettingsCodec.AddOutsideRoute(first, Team));
        Assert.Equal(firstCopy, first);
        var second = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddOutsideRoute(first, Notes));
        Assert.Equal([Team, Notes], Decode(second).OutsideRoutes);
        var created = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddOutsideRoute(ReadOnlyMemory<byte>.Empty, Team));
        Assert.Equal([Team], Decode(created).OutsideRoutes);
        using var createdJson = JsonDocument.Parse(created);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, createdJson.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, Decode(created).SchemaVersion);
    }

    [Theory(DisplayName = "Removing outside routes retains declarations unknown values order and the last empty array")]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(WorkspaceSettingsDefinitions.SchemaVersion)]
    [InlineData(WorkspaceSettingsDefinitions.SchemaVersion + 1)]
    public void RemovePreservesDeclarations(int? version)
    {
        var settings = Settings(Team, Notes);
        if (version is not null) settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = version.Value;
        settings[WorkspaceSettingsDefinitions.SchemaProperty] = ExistingSchema;
        var keys = settings.Select(property => property.Key).ToArray();
        var original = Bytes(settings);
        var originalCopy = original.ToArray();
        Assert.Null(WorkspaceSettingsCodec.RemoveOutsideRoute(original, "missing"));
        Assert.Equal(originalCopy, original);
        var first = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveOutsideRoute(original, Team));
        Assert.Equal([Notes], Decode(first).OutsideRoutes);
        var last = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveOutsideRoute(first, Notes));
        Assert.Empty(Decode(last).OutsideRoutes);
        using var json = JsonDocument.Parse(last);
        Assert.Equal(JsonValueKind.Array, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.OutsideRoutesProperty).ValueKind);
        Assert.Equal(keys, json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(FutureValue, json.RootElement.GetProperty(FutureProperty).GetString());
        Assert.Equal(ExistingSchema, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(version is not null, json.RootElement.TryGetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty, out var declaration));
        if (version is not null) Assert.Equal(version.Value, declaration.GetInt32());
        var lastCopy = last.ToArray();
        Assert.Null(WorkspaceSettingsCodec.RemoveOutsideRoute(last, Notes));
        Assert.Equal(lastCopy, last);
        Assert.Null(WorkspaceSettingsCodec.RemoveOutsideRoute(ReadOnlyMemory<byte>.Empty, Team));
        Assert.Null(WorkspaceSettingsCodec.RemoveOutsideRoute("{}"u8.ToArray(), Team));
    }

    [Fact(DisplayName = "File registrations remain independent from outside registration and unrelated rewrites retain authored declarations")]
    public void IndependentFileRootsAndRewrites()
    {
        var file = WorkspaceRoutePath.CanonicalEntrypointPath(Team);
        var settings = Settings(Team);
        settings[WorkspaceSettingsDefinitions.SchemaProperty] = ExistingSchema;
        settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = 1;
        var addedFile = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRoot(Bytes(settings), file));
        Assert.Equal([file], Decode(addedFile).Roots);
        Assert.Equal([Team], Decode(addedFile).OutsideRoutes);
        var removedOutside = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveOutsideRoute(addedFile, Team));
        Assert.Equal([file], Decode(removedOutside).Roots);
        var removedFile = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveRoot(addedFile, file));
        Assert.Equal([Team], Decode(removedFile).OutsideRoutes);
        Assert.Empty(Decode(removedFile).Roots);
        var frontmatter = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetFrontmatter(addedFile, FrontmatterForm.Root));
        var granted = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddAllowInstallPath(addedFile, "tools"));
        foreach (var rewritten in new[] { addedFile, removedFile, frontmatter, granted })
        {
            using var json = JsonDocument.Parse(rewritten);
            Assert.Equal(ExistingSchema, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
            Assert.Equal(1, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
            Assert.Equal(FutureValue, json.RootElement.GetProperty(FutureProperty).GetString());
            Assert.Equal([Team], Decode(rewritten).OutsideRoutes);
            Assert.Equal(settings.Select(property => property.Key), json.RootElement.EnumerateObject().Take(settings.Count).Select(property => property.Name));
        }
    }

    [Fact(DisplayName = "Changing the main root cannot create an overlap with an outside route")]
    public void ChangedMainRootStillValidatesOutsideRoutes()
    {
        var original = Bytes(Settings(Team));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.SetMainRoot(original, Team));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.SetMainRoot(original, "docs"));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.SetMainRoot(original, Team + "/forge"));
    }

    private static JsonObject Settings(params string[] directories)
        => new()
        {
            [FutureProperty] = FutureValue,
            [WorkspaceSettingsDefinitions.MainRootProperty] = MainRoot,
            [WorkspaceSettingsDefinitions.OutsideRoutesProperty] = new JsonArray(directories.Select(directory => (JsonNode?)JsonValue.Create(directory)).ToArray()),
        };

    private static byte[] Bytes(JsonObject settings) => Encoding.UTF8.GetBytes(settings.ToJsonString());

    private static WorkspaceSettingsDocument Decode(byte[] bytes)
    {
        var read = WorkspaceSettingsCodec.Read(bytes);
        Assert.Null(read.Cause);
        return Assert.IsType<WorkspaceSettingsDocument>(read.Document);
    }
}
