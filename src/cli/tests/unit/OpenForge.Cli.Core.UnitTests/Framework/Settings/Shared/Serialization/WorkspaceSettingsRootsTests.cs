using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

/// <summary>
/// The registry of files that route include brought in. It records paths only, because the loader's
/// Entries decide what loads.
/// </summary>
[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsRootsTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Included roots are read in their authored order")]
    public void ReadsRootsInOrder()
    {
        var document = Read("""{"roots":["docs/team/_team.md","CONTRIBUTING.md",".cursor/rules/testing.mdc"]}""");

        Assert.Equal(["docs/team/_team.md", "CONTRIBUTING.md", ".cursor/rules/testing.mdc"], document.Roots);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Absent or null roots read as an empty registry")]
    [InlineData("{}")]
    [InlineData("""{"roots":null}""")]
    [InlineData("""{"roots":[]}""")]
    public void AbsentRootsAreEmpty(string json)
        => Assert.Empty(Read(json).Roots);

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Roots reject paths that are not canonical files outside .agents")]
    [InlineData("""{"roots":"CONTRIBUTING.md"}""")]
    [InlineData("""{"roots":[1]}""")]
    [InlineData("""{"roots":[""]}""")]
    [InlineData("""{"roots":["./CONTRIBUTING.md"]}""")]
    [InlineData("""{"roots":["docs\\guide.md"]}""")]
    [InlineData("""{"roots":["../outside.md"]}""")]
    [InlineData("""{"roots":["/etc/hosts"]}""")]
    [InlineData("""{"roots":[".agents/patterns/_patterns.md"]}""")]
    [InlineData("""{"roots":[".Agents/notes.md"]}""")]
    [InlineData("""{"roots":[".agents"]}""")]
    [InlineData("""{"roots":[".git/config"]}""")]
    [InlineData("""{"roots":["CONTRIBUTING.md","CONTRIBUTING.md"]}""")]
    public void RejectsInvalidRoots(string json)
    {
        var decoded = WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json));

        Assert.Null(decoded.Document);
        Assert.Contains(WorkspaceSettingsDefinitions.RootsProperty, decoded.Cause, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Adding a root appends it and keeps every other authored key and declaration")]
    public void AddingAppendsAndPreservesKeys()
    {
        const string original = """
            {
              "$schema": "https://example.invalid/v2.json",
              "schemaVersion": 2,
              "future": {"keep": true},
              "roots": ["CONTRIBUTING.md"]
            }
            """;

        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRoot(Encoding.UTF8.GetBytes(original), "docs/team/_team.md"));
        using var json = JsonDocument.Parse(bytes);

        Assert.Equal("https://example.invalid/v2.json", json.RootElement.GetProperty("$schema").GetString());
        Assert.Equal(2, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.True(json.RootElement.GetProperty("future").GetProperty("keep").GetBoolean());
        Assert.Equal(
            ["CONTRIBUTING.md", "docs/team/_team.md"],
            json.RootElement.GetProperty("roots").EnumerateArray().Select(entry => entry.GetString()));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Adding a root to a missing settings file declares this release's schema")]
    public void AddingToMissingFileDeclaresSchema()
    {
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.AddRoot(ReadOnlyMemory<byte>.Empty, "CONTRIBUTING.md"));
        using var json = JsonDocument.Parse(bytes);

        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl, json.RootElement.GetProperty("$schema").GetString());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(["CONTRIBUTING.md"], Read(Encoding.UTF8.GetString(bytes)).Roots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Adding a registered root and removing an unregistered one change nothing")]
    public void RepeatedChangesAreNoOps()
    {
        var existing = Encoding.UTF8.GetBytes("""{"roots":["CONTRIBUTING.md"]}""");

        Assert.Null(WorkspaceSettingsCodec.AddRoot(existing, "CONTRIBUTING.md"));
        Assert.Null(WorkspaceSettingsCodec.RemoveRoot(existing, "docs/guide.md"));
        Assert.Null(WorkspaceSettingsCodec.RemoveRoot(Encoding.UTF8.GetBytes("{}"), "docs/guide.md"));
        Assert.Null(WorkspaceSettingsCodec.RemoveRoot(ReadOnlyMemory<byte>.Empty, "docs/guide.md"));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Removing the last root leaves an empty list and the other keys")]
    public void RemovingLastRootLeavesEmptyList()
    {
        var bytes = Assert.IsType<byte[]>(WorkspaceSettingsCodec.RemoveRoot(
            Encoding.UTF8.GetBytes("""{"frontmatter":"root","roots":["CONTRIBUTING.md"]}"""),
            "CONTRIBUTING.md"));
        using var json = JsonDocument.Parse(bytes);

        Assert.Equal("root", json.RootElement.GetProperty("frontmatter").GetString());
        Assert.Equal(0, json.RootElement.GetProperty("roots").GetArrayLength());
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Adding or removing an invalid root path is refused")]
    [InlineData(".agents/patterns/_patterns.md")]
    [InlineData("./CONTRIBUTING.md")]
    [InlineData("../outside.md")]
    [InlineData(".agents/open-forge.json")]
    public void RefusesInvalidRootChanges(string path)
    {
        var existing = Encoding.UTF8.GetBytes("{}");

        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.AddRoot(existing, path));
        Assert.Throws<ArgumentException>(() => WorkspaceSettingsCodec.RemoveRoot(existing, path));
    }

    private static WorkspaceSettingsDocument Read(string json)
        => Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(json)).Document);
}
