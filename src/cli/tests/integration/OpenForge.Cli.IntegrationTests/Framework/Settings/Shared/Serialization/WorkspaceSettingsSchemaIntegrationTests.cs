using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.IntegrationTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class WorkspaceSettingsSchemaIntegrationTests
{
    [Fact(DisplayName = "Outside routes have an optional v5 schema and do not alter earlier published shapes"), Trait("Boundary", "Input")]
    public void OutsideRoutesUseNewSchema()
    {
        using var current = ReadPublishedSchema();
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, current.RootElement.GetProperty("$id").GetString());
        var property = current.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.OutsideRoutesProperty);
        Assert.Equal(JsonValueKind.Array, SchemaType(property.GetProperty("type")));
        Assert.Equal(JsonValueKind.String, SchemaType(property.GetProperty("items").GetProperty("type")));
        Assert.True(property.GetProperty("uniqueItems").GetBoolean());
        Assert.True(current.RootElement.GetProperty("additionalProperties").GetBoolean());
        Assert.False(current.RootElement.TryGetProperty("required", out var required)
            && required.EnumerateArray().Any(value => value.GetString() == WorkspaceSettingsDefinitions.OutsideRoutesProperty));
        for (var version = 1; version < WorkspaceSettingsDefinitions.SchemaVersion; version++)
        {
            using var previous = ReadPublishedSchema(version);
            Assert.False(previous.RootElement.GetProperty("properties").TryGetProperty(WorkspaceSettingsDefinitions.OutsideRoutesProperty, out _));
        }
        foreach (var example in current.RootElement.GetProperty("examples").EnumerateArray())
        {
            var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(example.GetRawText())).Document);
            Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, document.SchemaVersion);
            Assert.NotEmpty(document.OutsideRoutes);
        }
    }

    [Fact(DisplayName = "Main root is optional in v4 and absent from published earlier schemas"), Trait("Boundary", "Input")]
    public void MainRootUsesNewSchema()
    {
        using var current = ReadPublishedSchema(WorkspaceSettingsDefinitions.SchemaVersion);
        Assert.Equal(JsonValueKind.String, SchemaType(current.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.MainRootProperty).GetProperty("type")));
        Assert.False(current.RootElement.TryGetProperty("required", out var required)
            && required.EnumerateArray().Any(value => value.GetString() == WorkspaceSettingsDefinitions.MainRootProperty));
        for (var version = 1; version <= 3; version++)
        {
            using var previous = ReadPublishedSchema(version);
            Assert.False(previous.RootElement.GetProperty("properties").TryGetProperty(WorkspaceSettingsDefinitions.MainRootProperty, out _));
        }
    }

    [Fact(DisplayName = "Link scope has a new optional settings schema while published version one stays unchanged"), Trait("Boundary", "Input")]
    public void LinkScopeUsesNewSchema()
    {
        using var current = ReadPublishedSchema(2);
        using var previous = ReadPublishedSchema(1);
        foreach (var key in new[] { WorkspaceSettingsDefinitions.IgnoreLinksFromProperty, WorkspaceSettingsDefinitions.IgnoreLinksToProperty })
        {
            var property = current.RootElement.GetProperty("properties").GetProperty(key);
            Assert.Equal(JsonValueKind.Array, SchemaType(property.GetProperty("type")));
            Assert.Equal(JsonValueKind.String, SchemaType(property.GetProperty("items").GetProperty("type")));
            Assert.False(previous.RootElement.GetProperty("properties").TryGetProperty(key, out _));
        }
        foreach (var example in current.RootElement.GetProperty("examples").EnumerateArray())
            Assert.NotNull(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(example.GetRawText())).Document);
    }

    [Fact(DisplayName = "Included roots have a new optional settings schema while published version two stays unchanged"), Trait("Boundary", "Input")]
    public void RootsUseNewSchema()
    {
        using var current = ReadPublishedSchema();
        using var previous = ReadPublishedSchema(2);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, current.RootElement.GetProperty("$id").GetString());
        var property = current.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.RootsProperty);
        Assert.Equal(JsonValueKind.Array, SchemaType(property.GetProperty("type")));
        Assert.Equal(JsonValueKind.String, SchemaType(property.GetProperty("items").GetProperty("type")));
        Assert.True(property.GetProperty("uniqueItems").GetBoolean());
        Assert.False(previous.RootElement.GetProperty("properties").TryGetProperty(WorkspaceSettingsDefinitions.RootsProperty, out _));
        Assert.True(current.RootElement.GetProperty("additionalProperties").GetBoolean());
        foreach (var example in current.RootElement.GetProperty("examples").EnumerateArray())
        {
            var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(example.GetRawText())).Document);
            Assert.True(document.IsKnownSchemaVersion);
            Assert.NotEmpty(document.Roots);
        }
    }

    [Fact(DisplayName = "The published frontmatter schema agrees with codec forms and the missing-key default"), Trait("Boundary", "Input")]
    public void FrontmatterSchemaAgreesWithCodec()
    {
        using var schema = ReadPublishedSchema();
        var frontmatter = schema.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.FrontmatterProperty);

        Assert.Equal(JsonValueKind.String, SchemaType(frontmatter.GetProperty("type")));
        Assert.Equal([WorkspaceSettingsDefinitions.FrontmatterRootName, WorkspaceSettingsDefinitions.FrontmatterScopedName], frontmatter.GetProperty("enum").EnumerateArray().Select(value => value.GetString()));
        foreach (var value in frontmatter.GetProperty("enum").EnumerateArray())
        {
            var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":{{{value.GetRawText()}}}}"""));
            var document = Assert.IsType<WorkspaceSettingsDocument>(decoded.Document);
            Assert.Equal(value.GetString(), WorkspaceSettingsDefinitions.ReadFrontmatterName(document.Frontmatter));
        }

        var defaultDocument = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read("{}"u8.ToArray()).Document);
        Assert.Equal(WorkspaceSettingsDefinitions.FrontmatterScopedName, frontmatter.GetProperty("default").GetString());
        Assert.Equal(frontmatter.GetProperty("default").GetString(), WorkspaceSettingsDefinitions.ReadFrontmatterName(defaultDocument.Frontmatter));
        Assert.False(schema.RootElement.TryGetProperty("required", out var required)
            && required.EnumerateArray().Any(value => value.GetString() == WorkspaceSettingsDefinitions.FrontmatterProperty));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, defaultDocument.SchemaVersion);
    }

    [Theory(DisplayName = "The frontmatter schema excludes the malformed forms rejected by the codec"), Trait("Boundary", "Input")]
    [InlineData("null"), InlineData("1"), InlineData("\"Root\""), InlineData("\"flat\"")]
    public void FrontmatterSchemaExcludesInvalidForms(string value)
    {
        using var schema = ReadPublishedSchema();
        using var candidate = JsonDocument.Parse(value);
        var frontmatter = schema.RootElement.GetProperty("properties").GetProperty(WorkspaceSettingsDefinitions.FrontmatterProperty);

        Assert.Equal(JsonValueKind.String, SchemaType(frontmatter.GetProperty("type")));
        Assert.DoesNotContain(frontmatter.GetProperty("enum").EnumerateArray(), allowed => JsonElement.DeepEquals(allowed, candidate.RootElement));
        Assert.Null(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes($$$"""{"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":{{{value}}}}""")).Document);
    }

    private static JsonValueKind SchemaType(JsonElement property)
        => Enum.Parse<JsonValueKind>(property.GetString()!, ignoreCase: true);

    private static JsonDocument ReadPublishedSchema(int version = WorkspaceSettingsDefinitions.SchemaVersion)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, WorkspaceSettingsDefinitions.SchemaDirectoryName, $"v{version}", WorkspaceSettingsDefinitions.SchemaFileName);
            if (File.Exists(path))
            {
                return JsonDocument.Parse(File.ReadAllBytes(path));
            }
            directory = directory.Parent;
        }

        throw new InvalidOperationException("The published workspace settings schema was not found above the test artifact.");
    }

}
