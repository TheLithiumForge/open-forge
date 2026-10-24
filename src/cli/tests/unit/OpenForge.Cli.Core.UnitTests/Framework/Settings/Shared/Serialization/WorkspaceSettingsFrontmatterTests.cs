using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsFrontmatterTests
{
    private const string First = "first";
    private const string Last = "last";
    private const string Foreign = "foreign";
    private const string Keep = "keep";
    private const string Docs = "docs";
    private const string OldFile = "docs/old.md";

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
    [InlineData(WorkspaceSettingsDefinitions.FrontmatterRootName, (int)FrontmatterForm.Root), InlineData(WorkspaceSettingsDefinitions.FrontmatterScopedName, (int)FrontmatterForm.Scoped)]
    public void ReadsDeclaredForms(string name, int expected)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":"{{{name}}}"}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
        Assert.Null(decoded.Cause);
        Assert.Equal((FrontmatterForm)expected, document.DeclaredFrontmatter);
        Assert.Equal((FrontmatterForm)expected, document.Frontmatter);
    }

    [Theory(DisplayName = "Malformed and unknown frontmatter settings invalidate decoding"), Trait("Boundary", "Input")]
    [InlineData("null")]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("\"Root\"")]
    [InlineData("\"flat\"")]
    [InlineData("\"SCOPED\"")]
    [InlineData("\"\"")]
    public void RejectsMalformedOrUnknownForm(string value)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":{{{value}}}}"""));

        Assert.Null(decoded.Document);
        Assert.NotNull(decoded.Cause);
    }

    [Theory(DisplayName = "Typed frontmatter names round trip through settings definitions"), Trait("Boundary", "Processing")]
    [InlineData(WorkspaceSettingsDefinitions.FrontmatterRootName, (int)FrontmatterForm.Root), InlineData(WorkspaceSettingsDefinitions.FrontmatterScopedName, (int)FrontmatterForm.Scoped)]
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
            ? $$$"""{"{{{First}}}":{"nested":[1,null,true]},"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":"{{{WorkspaceSettingsDefinitions.FrontmatterScopedName}}}","{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":7,"{{{Last}}}":"kept"}"""
            : $$$"""{"{{{First}}}":{"nested":[1,null,true]},"{{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}}":7,"{{{Last}}}":"kept"}""";
        var written = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetFrontmatter(Encoding.UTF8.GetBytes(original), FrontmatterForm.Root));
        using var json = JsonDocument.Parse(written);
        using var before = JsonDocument.Parse(original);

        string[] expectedOrder = declared
            ? [First, WorkspaceSettingsDefinitions.FrontmatterProperty, WorkspaceSettingsDefinitions.SchemaVersionProperty, Last]
            : [First, WorkspaceSettingsDefinitions.SchemaVersionProperty, Last, WorkspaceSettingsDefinitions.FrontmatterProperty];
        Assert.Equal(expectedOrder, json.RootElement.EnumerateObject().Select(property => property.Name));
        foreach (var property in before.RootElement.EnumerateObject().Where(property => property.Name != WorkspaceSettingsDefinitions.FrontmatterProperty))
        {
            Assert.True(JsonElement.DeepEquals(property.Value, json.RootElement.GetProperty(property.Name)));
        }
        Assert.Equal(FrontmatterForm.Root, Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(written).Document).DeclaredFrontmatter);
    }

    [Theory(DisplayName = "Setting an already declared frontmatter form is a byte preserving no-op"), Trait("Boundary", "Output")]
    [InlineData(WorkspaceSettingsDefinitions.FrontmatterRootName, (int)FrontmatterForm.Root), InlineData(WorkspaceSettingsDefinitions.FrontmatterScopedName, (int)FrontmatterForm.Scoped)]
    public void ExplicitSameFormIsNoOp(string name, int form)
    {
        var bytes = Encoding.UTF8.GetBytes($$$"""{ /* keep */ "{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":"{{{name}}}", "{{{Foreign}}}":true, }""");
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
        Assert.Equal([WorkspaceSettingsDefinitions.FrontmatterProperty], json.RootElement.EnumerateObject().Select(property => property.Name));
    }

    [Theory(DisplayName = "New frontmatter settings declare the schema and version"), Trait("Boundary", "Output")]
    [InlineData((int)FrontmatterForm.Root), InlineData((int)FrontmatterForm.Scoped)]
    public void NewDocumentDeclaresSchema(int form)
    {
        var written = Assert.IsType<byte[]>(WorkspaceSettingsCodec.SetFrontmatter(ReadOnlyMemory<byte>.Empty, (FrontmatterForm)form));
        using var json = JsonDocument.Parse(written);

        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.Equal([WorkspaceSettingsDefinitions.SchemaProperty, WorkspaceSettingsDefinitions.SchemaVersionProperty, WorkspaceSettingsDefinitions.FrontmatterProperty], json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal((FrontmatterForm)form, Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(written).Document).DeclaredFrontmatter);
    }

    [Theory(DisplayName = "Frontmatter edits refuse an invalid authored settings document"), Trait("Boundary", "Input")]
    [InlineData("[]"), InlineData("null"), InlineData("{broken")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":null}""")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":"flat"}""")]
    [InlineData($$$"""{"{{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}}":1}""")]
    public void RefusesInvalidExistingDocument(string json)
        => Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.SetFrontmatter(Encoding.UTF8.GetBytes(json), FrontmatterForm.Root));

    [Fact(DisplayName = "Grant and removal writers preserve the declared frontmatter preference"), Trait("Boundary", "Output")]
    public void ExistingWritersPreserveFrontmatter()
    {
        var bytes = Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":"{{{WorkspaceSettingsDefinitions.FrontmatterRootName}}}","{{{Foreign}}}":{"{{{Keep}}}":true}}""").ToArray();
        bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddAllowInstallPath(bytes, Docs));
        bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRemovals(bytes, new WorkspaceRemovalSelection { Files = [OldFile] }));
        bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.ClearRemovals(bytes, new WorkspaceRemovalSelection { Files = [OldFile] }));

        var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document);
        Assert.Equal(FrontmatterForm.Root, document.DeclaredFrontmatter);
        Assert.Equal([Docs], document.AllowInstallPaths);
        Assert.Empty(document.RemovedFiles);
        using var json = JsonDocument.Parse(bytes);
        Assert.Equal([WorkspaceSettingsDefinitions.FrontmatterProperty, Foreign, WorkspaceSettingsDefinitions.AllowInstallPathsProperty, WorkspaceSettingsDefinitions.RemovedFilesProperty], json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.True(json.RootElement.GetProperty(Foreign).GetProperty(Keep).GetBoolean());
    }
}
