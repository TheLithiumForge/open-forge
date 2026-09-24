using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsRemovalCodecTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Removal lists append sorted unique values and preserve unknown authored keys")]
    public void AppendsSortedUniqueValuesAndPreservesUnknownKeys()
    {
        const string original = """
            {
              // comments follow the settings round-trip behavior
              "first": {"keep": true},
              "removedCategories": ["templates"],
              "removedFiles": ["AGENTS.md"],
              "removedDirectories": ["docs/old"],
              "removedExtensions": ["base"],
              "future": {"nested": [1, 2, {"keep": true}]}
            }
            """;
        var selection = new WorkspaceRemovalSelection
        {
            Categories = ["zebra", "alpha", "zebra", "templates"],
            Files = ["docs/z.md", "docs/a.md", "AGENTS.md"],
            Directories = ["docs/old-z", "docs/new-a", "docs/old"],
            Extensions = ["planning", "core", "planning"],
            Libraries = ["team-b", "team-a", "team-base"],
        };

        var written = WorkspaceSettingsCodec.AddRemovals(Encoding.UTF8.GetBytes(original), selection);
        var bytes = Assert.IsType<byte[]>(written);
        var text = Encoding.UTF8.GetString(bytes);
        using var json = JsonDocument.Parse(bytes);

        Assert.DoesNotContain("comments follow", text, StringComparison.Ordinal);
        Assert.Equal(
            ["first", "removedCategories", "removedFiles", "removedDirectories", "removedExtensions", "future", "removedLibraries"],
            json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["templates", "alpha", "zebra"], Strings(json, "removedCategories"));
        Assert.Equal(["AGENTS.md", "docs/a.md", "docs/z.md"], Strings(json, "removedFiles"));
        Assert.Equal(["docs/old", "docs/new-a", "docs/old-z"], Strings(json, "removedDirectories"));
        Assert.Equal(["base", "core", "planning"], Strings(json, "removedExtensions"));
        Assert.Equal(["team-a", "team-b", "team-base"], Strings(json, "removedLibraries"));
        Assert.True(json.RootElement.GetProperty("first").GetProperty("keep").GetBoolean());
        Assert.True(json.RootElement.GetProperty("future").GetProperty("nested")[2].GetProperty("keep").GetBoolean());

        var decoded = WorkspaceSettingsCodec.Read(bytes);
        Assert.NotNull(decoded.Document);
        Assert.Null(WorkspaceSettingsCodec.AddRemovals(bytes, selection));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An absent settings file is created with the selected removal list")]
    public void CreatesRemovalSettingsFromNothing()
    {
        var written = WorkspaceSettingsCodec.AddRemovals(
            ReadOnlyMemory<byte>.Empty,
            new WorkspaceRemovalSelection { Directories = ["docs/obsolete"] });

        using var json = JsonDocument.Parse(Assert.IsType<byte[]>(written));
        Assert.Equal("https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v1/open-forge.schema.json",
            json.RootElement.GetProperty("$schema").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(["docs/obsolete"], Strings(json, "removedDirectories"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "An empty removal selection makes no authored change")]
    public void EmptySelectionIsUnchanged()
        => Assert.Null(WorkspaceSettingsCodec.AddRemovals(
            Encoding.UTF8.GetBytes("not JSON"),
            new WorkspaceRemovalSelection()));

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Removal authoring rejects invalid selected paths and IDs")]
    [InlineData("file")]
    [InlineData("directory")]
    [InlineData("extension")]
    [InlineData("library")]
    [InlineData("category")]
    public void RejectsInvalidSelection(string kind)
    {
        var selection = kind switch
        {
            "file" => new WorkspaceRemovalSelection { Files = ["../docs/file.md"] },
            "directory" => new WorkspaceRemovalSelection { Directories = [".agents"] },
            "extension" => new WorkspaceRemovalSelection { Extensions = ["Planning"] },
            "library" => new WorkspaceRemovalSelection { Libraries = ["team--knowledge"] },
            "category" => new WorkspaceRemovalSelection { Categories = ["nested/category"] },
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        Assert.Throws<ArgumentException>(
            () => WorkspaceSettingsCodec.AddRemovals(ReadOnlyMemory<byte>.Empty, selection));
    }

    private static string[] Strings(JsonDocument json, string property)
        => json.RootElement.GetProperty(property)
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();
}
