using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

/// <summary>
/// The declared schema is documentation, not a gate. These prove the property
/// that makes adding a version safe: no value of it, including one from a release
/// that does not exist yet, can stop an older CLI from reading the file.
/// </summary>
[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsSchemaTests
{
    [Fact(DisplayName = "The published frontmatter schema agrees with codec forms and the missing-key default"), Trait("Boundary", "Input")]
    public void FrontmatterSchemaAgreesWithCodec()
    {
        using var schema = ReadPublishedSchema();
        var frontmatter = schema.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.FrontmatterProperty);

        Assert.Equal("string", frontmatter.GetProperty("type").GetString());
        Assert.Equal(["root", "scoped"], frontmatter.GetProperty("enum").EnumerateArray().Select(value => value.GetString()));
        foreach (var value in frontmatter.GetProperty("enum").EnumerateArray())
        {
            var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{"frontmatter":{{value.GetRawText()}}}"""));
            var document = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
            Assert.Equal(value.GetString(), WorkspaceSettingsDefinitions.ReadFrontmatterName(document.Frontmatter));
        }

        var defaultDocument = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read("{}"u8.ToArray()).Document);
        Assert.Equal("scoped", frontmatter.GetProperty("default").GetString());
        Assert.Equal(frontmatter.GetProperty("default").GetString(), WorkspaceSettingsDefinitions.ReadFrontmatterName(defaultDocument.Frontmatter));
        Assert.False(schema.RootElement.TryGetProperty("required", out var required)
            && required.EnumerateArray().Any(value => value.GetString() == WorkspaceSettingsDefinitions.FrontmatterProperty));
        Assert.Equal(1, defaultDocument.SchemaVersion);
    }

    [Theory(DisplayName = "The frontmatter schema excludes the malformed forms rejected by the codec"), Trait("Boundary", "Input")]
    [InlineData("null"), InlineData("1"), InlineData("\"Root\""), InlineData("\"flat\"")]
    public void FrontmatterSchemaExcludesInvalidForms(string value)
    {
        using var schema = ReadPublishedSchema();
        using var candidate = JsonDocument.Parse(value);
        var frontmatter = schema.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.FrontmatterProperty);

        Assert.Equal("string", frontmatter.GetProperty("type").GetString());
        Assert.DoesNotContain(frontmatter.GetProperty("enum").EnumerateArray(), allowed => JsonElement.DeepEquals(allowed, candidate.RootElement));
        Assert.Null(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$"""{"frontmatter":{{value}}}""")).Document);
    }

    private static JsonDocument ReadPublishedSchema()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "schemas", "v1", "open-forge.schema.json");
            if (File.Exists(path))
            {
                return JsonDocument.Parse(File.ReadAllBytes(path));
            }
            directory = directory.Parent;
        }

        throw new InvalidOperationException("The published workspace settings schema was not found above the test artifact.");
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "A declared schema version is read back")]
    public void ReadsDeclaredVersion()
    {
        var read = WorkspaceSettingsCodec.Read(
            Encoding.UTF8.GetBytes("""{"schemaVersion":1,"allowInstallPaths":["docs"]}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Equal(1, document.SchemaVersion);
        Assert.True(document.IsKnownSchemaVersion);
    }

    /// <summary>
    /// A file written before the key existed, or by hand, is not making a mistake.
    /// </summary>
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An absent or null schema version reads as this release's own")]
    [InlineData("{}")]
    [InlineData("""{"schemaVersion":null}""")]
    [InlineData("""{"allowInstallPaths":["docs"]}""")]
    public void DefaultsAbsentVersion(string json)
    {
        var read = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, document.SchemaVersion);
        Assert.True(document.IsKnownSchemaVersion);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An unrecognised schema version is still read")]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4096)]
    public void ReadsUnrecognisedVersion(int version)
    {
        var read = WorkspaceSettingsCodec.Read(
            Encoding.UTF8.GetBytes($$"""{"schemaVersion":{{version}},"allowInstallPaths":["docs"]}"""));

        var document = Assert.IsType<WorkspaceSettingsDocument>(read.Document);
        Assert.Null(read.Cause);
        Assert.Equal(version, document.SchemaVersion);
        Assert.False(document.IsKnownSchemaVersion);
        Assert.Equal(["docs"], document.AllowInstallPaths);
    }

    /// <summary>
    /// A version that is not a number is a typing mistake in a key the CLI acts
    /// on, which is the one case worth telling the author about.
    /// </summary>
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "A schema version that is not a whole number is reported")]
    [InlineData("""{"schemaVersion":"1"}""")]
    [InlineData("""{"schemaVersion":1.5}""")]
    [InlineData("""{"schemaVersion":true}""")]
    public void ReportsNonIntegerVersion(string json)
    {
        var read = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(read.Document);
        Assert.Contains("whole number", read.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A file the CLI creates declares its schema and version")]
    public void CreatedFileDeclaresItself()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(ReadOnlyMemory<byte>.Empty, "docs");

        var json = Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
        Assert.Contains(WorkspaceSettingsDefinitions.SchemaUrl, json, StringComparison.Ordinal);
        Assert.Contains("\"schemaVersion\": 1", json, StringComparison.Ordinal);

        // The declaration must not cost the file its readability.
        var read = WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written));
        Assert.Equal(["docs"], read.Document!.AllowInstallPaths);
    }

    /// <summary>
    /// Adding keys the author did not ask for to a document they own is not this
    /// command's business. A grant adds the grant, and nothing else.
    /// </summary>
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A file the author already wrote is not given a schema or version")]
    public void ExistingFileIsLeftAsWritten()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(
            Encoding.UTF8.GetBytes("""{"allowInstallPaths":["docs"]}"""),
            "tools");

        var json = Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
        Assert.DoesNotContain("$schema", json, StringComparison.Ordinal);
        Assert.DoesNotContain("schemaVersion", json, StringComparison.Ordinal);
        Assert.Equal(["docs", "tools"], WorkspaceSettingsCodec.Read(Assert.IsType<byte[]>(written)).Document!.AllowInstallPaths);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An existing schema and version survive a grant")]
    public void DeclarationSurvivesAGrant()
    {
        var written = WorkspaceSettingsCodec.AddAllowInstallPath(
            Encoding.UTF8.GetBytes(
                """{"$schema":"https://example.invalid/v1.json","schemaVersion":1,"allowInstallPaths":["docs"]}"""),
            "tools");

        var json = Encoding.UTF8.GetString(Assert.IsType<byte[]>(written));
        Assert.Contains("https://example.invalid/v1.json", json, StringComparison.Ordinal);
        Assert.Contains("\"schemaVersion\": 1", json, StringComparison.Ordinal);
    }
}
