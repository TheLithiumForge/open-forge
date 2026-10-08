using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsFrontmatterTests
{
    [Fact(DisplayName = "An omitted frontmatter setting means scoped without a declaration"), Trait("Boundary", "Input")]
    public void MissingMeansScoped()
    {
        var decoded = WorkspaceSettingsCodec.Read("{}"u8.ToArray());

        var document = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
        Assert.Null(decoded.Cause);
        Assert.Null(document.DeclaredFrontmatter);
        Assert.Equal(FrontmatterForm.Scoped, document.Frontmatter);
        Assert.Null(WorkspaceSettingsDocument.Empty.DeclaredFrontmatter);
        Assert.Equal(FrontmatterForm.Scoped, WorkspaceSettingsDocument.Empty.Frontmatter);
    }

    [Theory(DisplayName = "Settings retain each explicitly declared frontmatter form"), Trait("Boundary", "Input")]
    [InlineData("root", (int)FrontmatterForm.Root), InlineData("scoped", (int)FrontmatterForm.Scoped)]
    public void ReadsDeclaredForms(string name, int expected)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{"frontmatter":"{{name}}"}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
        Assert.Null(decoded.Cause);
        Assert.Equal((FrontmatterForm)expected, document.DeclaredFrontmatter);
        Assert.Equal((FrontmatterForm)expected, document.Frontmatter);
    }

    [Theory(DisplayName = "Malformed and unknown frontmatter settings invalidate decoding"), Trait("Boundary", "Input")]
    [InlineData("null", "\"frontmatter\" must be a string.")]
    [InlineData("1", "\"frontmatter\" must be a string.")]
    [InlineData("true", "\"frontmatter\" must be a string.")]
    [InlineData("[]", "\"frontmatter\" must be a string.")]
    [InlineData("{}", "\"frontmatter\" must be a string.")]
    [InlineData("\"Root\"", "\"frontmatter\" must be \"root\" or \"scoped\".")]
    [InlineData("\"flat\"", "\"frontmatter\" must be \"root\" or \"scoped\".")]
    [InlineData("\"SCOPED\"", "\"frontmatter\" must be \"root\" or \"scoped\".")]
    [InlineData("\"\"", "\"frontmatter\" must be \"root\" or \"scoped\".")]
    public void RejectsMalformedOrUnknownForm(string value, string cause)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{"frontmatter":{{value}}}"""));

        Assert.Null(decoded.Document);
        Assert.Equal(cause, decoded.Cause);
    }

    [Theory(DisplayName = "Typed frontmatter names round trip through settings definitions"), Trait("Boundary", "Processing")]
    [InlineData("root", (int)FrontmatterForm.Root), InlineData("scoped", (int)FrontmatterForm.Scoped)]
    public void ConvertsSupportedForms(string name, int expected)
    {
        Assert.True(WorkspaceSettingsDefinitions.TryReadFrontmatter(name, out var form));
        Assert.Equal((FrontmatterForm)expected, form);
        Assert.Equal(name, WorkspaceSettingsDefinitions.ReadFrontmatterName(form));
    }

    [Theory(DisplayName = "Frontmatter names accept only the exact persisted spellings"), Trait("Boundary", "Input")]
    [InlineData(null), InlineData(""), InlineData("Root"), InlineData("flat"), InlineData("SCOPED")]
    public void RejectsUnknownNames(string? name)
        => Assert.False(WorkspaceSettingsDefinitions.TryReadFrontmatter(name, out _));

    [Fact(DisplayName = "Frontmatter mappings cover the complete supported value set"), Trait("Boundary", "Processing")]
    public void CoversEverySupportedForm()
        => Assert.Equal([FrontmatterForm.Scoped, FrontmatterForm.Root], Enum.GetValues<FrontmatterForm>());

    [Fact(DisplayName = "Undefined frontmatter values cannot be named or written"), Trait("Boundary", "Input")]
    public void RejectsUndefinedForm()
    {
        var undefined = (FrontmatterForm)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceSettingsDefinitions.ReadFrontmatterName(undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceSettingsCodec.SetFrontmatter(ReadOnlyMemory<byte>.Empty, undefined));
    }

    [Theory(DisplayName = "Frontmatter edits preserve foreign values and authored property order"), Trait("Boundary", "Output")]
    [InlineData(false), InlineData(true)]
    public void PreservesForeignKeys(bool declared)
    {
        var original = declared
            ? """{"first":{"nested":[1,null,true]},"frontmatter":"scoped","schemaVersion":7,"last":"kept"}"""
            : """{"first":{"nested":[1,null,true]},"schemaVersion":7,"last":"kept"}""";
        var written = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetFrontmatter(Encoding.UTF8.GetBytes(original), FrontmatterForm.Root));
        using var json = JsonDocument.Parse(written);
        using var before = JsonDocument.Parse(original);

        string[] expectedOrder = declared
            ? ["first", "frontmatter", "schemaVersion", "last"]
            : ["first", "schemaVersion", "last", "frontmatter"];
        Assert.Equal(expectedOrder, json.RootElement.EnumerateObject().Select(property => property.Name));
        foreach (var property in before.RootElement.EnumerateObject().Where(property => property.Name != "frontmatter"))
        {
            Assert.True(JsonElement.DeepEquals(property.Value, json.RootElement.GetProperty(property.Name)));
        }
        Assert.Equal(FrontmatterForm.Root, Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(written).Document).DeclaredFrontmatter);
    }

    [Theory(DisplayName = "Setting an already declared frontmatter form is a byte preserving no-op"), Trait("Boundary", "Output")]
    [InlineData("root", (int)FrontmatterForm.Root), InlineData("scoped", (int)FrontmatterForm.Scoped)]
    public void ExplicitSameFormIsNoOp(string name, int form)
    {
        var bytes = Encoding.UTF8.GetBytes($$"""{ /* keep */ "frontmatter":"{{name}}", "foreign":true, }""");
        var before = bytes.ToArray();

        Assert.Null(WorkspaceSettingsCodec.SetFrontmatter(bytes, (FrontmatterForm)form));
        Assert.Equal(before, bytes);
    }

    [Fact(DisplayName = "An explicit scoped request adds a missing frontmatter key"), Trait("Boundary", "Output")]
    public void MissingKeyToExplicitScopedIsAChange()
    {
        var written = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetFrontmatter("{}"u8.ToArray(), FrontmatterForm.Scoped));

        var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(written).Document);
        Assert.Equal(FrontmatterForm.Scoped, document.DeclaredFrontmatter);
        using var json = JsonDocument.Parse(written);
        Assert.Equal(["frontmatter"], json.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Theory(DisplayName = "New frontmatter settings declare the schema and version"), Trait("Boundary", "Output")]
    [InlineData((int)FrontmatterForm.Root), InlineData((int)FrontmatterForm.Scoped)]
    public void NewDocumentDeclaresSchema(int form)
    {
        var written = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetFrontmatter(ReadOnlyMemory<byte>.Empty, (FrontmatterForm)form));
        using var json = JsonDocument.Parse(written);

        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, json.RootElement.GetProperty("$schema").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(["$schema", "schemaVersion", "frontmatter"], json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal((FrontmatterForm)form, Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(written).Document).DeclaredFrontmatter);
    }

    [Theory(DisplayName = "Frontmatter edits refuse an invalid authored settings document"), Trait("Boundary", "Input")]
    [InlineData("[]"), InlineData("null"), InlineData("{broken")]
    [InlineData("""{"frontmatter":null}""")]
    [InlineData("""{"frontmatter":"flat"}""")]
    [InlineData("""{"removedFiles":1}""")]
    public void RefusesInvalidExistingDocument(string json)
        => Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.SetFrontmatter(Encoding.UTF8.GetBytes(json), FrontmatterForm.Root));

    [Fact(DisplayName = "Grant and removal writers preserve the declared frontmatter preference"), Trait("Boundary", "Output")]
    public void ExistingWritersPreserveFrontmatter()
    {
        var bytes = """{"frontmatter":"root","foreign":{"keep":true}}"""u8.ToArray();
        bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddAllowInstallPath(bytes, "docs"));
        bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRemovals(bytes, new WorkspaceRemovalSelection { Files = ["docs/old.md"] }));
        bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.ClearRemovals(bytes, new WorkspaceRemovalSelection { Files = ["docs/old.md"] }));

        var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document);
        Assert.Equal(FrontmatterForm.Root, document.DeclaredFrontmatter);
        Assert.Equal(["docs"], document.AllowInstallPaths);
        Assert.Empty(document.RemovedFiles);
        using var json = JsonDocument.Parse(bytes);
        Assert.Equal(["frontmatter", "foreign", "allowInstallPaths", "removedFiles"], json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.True(json.RootElement.GetProperty("foreign").GetProperty("keep").GetBoolean());
    }
}
