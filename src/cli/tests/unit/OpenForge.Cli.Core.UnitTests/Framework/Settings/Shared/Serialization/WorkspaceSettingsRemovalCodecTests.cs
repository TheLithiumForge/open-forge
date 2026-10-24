using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsRemovalCodecTests
{
    private const string First = "first";
    private const string Future = "future";
    private const string Keep = "keep";
    private const string Nested = "nested";
    private const string Templates = "templates";
    private const string Alpha = "alpha";
    private const string Zebra = "zebra";
    private const string FileA = "docs/a.md";
    private const string FileZ = "docs/z.md";
    private const string OldDirectory = "docs/old";
    private const string NewDirectory = "docs/new-a";
    private const string OldDirectoryZ = "docs/old-z";
    private const string BaseExtension = "base";
    private const string CoreExtension = "core";
    private const string PlanningExtension = "planning";
    private const string LibraryA = "team-a";
    private const string LibraryB = "team-b";
    private const string LibraryBase = "team-base";
    private const string Obsolete = "docs/obsolete";

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Removal lists append sorted unique values and preserve unknown authored keys")]
    public void AppendsSortedUniqueValuesAndPreservesUnknownKeys()
    {
        const string original = $$$"""
            {
              // comments follow the settings round-trip behavior
              "{{{First}}}": {"{{{Keep}}}": true},
              "{{{WorkspaceSettingsDefinitions.RemovedCategoriesProperty}}}": ["{{{Templates}}}"],
              "{{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}}": ["{{{WorkspaceLayoutNames.AgentHostFileName}}}"],
              "{{{WorkspaceSettingsDefinitions.RemovedDirectoriesProperty}}}": ["{{{OldDirectory}}}"],
              "{{{WorkspaceSettingsDefinitions.RemovedExtensionsProperty}}}": ["{{{BaseExtension}}}"],
              "{{{Future}}}": {"{{{Nested}}}": [1, 2, {"{{{Keep}}}": true}]}
            }
            """;
        var selection = new WorkspaceRemovalSelection
        {
            Categories = [Zebra, Alpha, Zebra, Templates],
            Files = [FileZ, FileA, WorkspaceLayoutNames.AgentHostFileName],
            Directories = [OldDirectoryZ, NewDirectory, OldDirectory],
            Extensions = [PlanningExtension, CoreExtension, PlanningExtension],
            Libraries = [LibraryB, LibraryA, LibraryBase],
        };

        var written = WorkspaceSettingsCodec.AddRemovals(Encoding.UTF8.GetBytes(original), selection);
        var bytes = Assert.IsType<byte[]>(written);
        using var json = JsonDocument.Parse(bytes);

        Assert.Equal(
            [First, WorkspaceSettingsDefinitions.RemovedCategoriesProperty, WorkspaceSettingsDefinitions.RemovedFilesProperty, WorkspaceSettingsDefinitions.RemovedDirectoriesProperty, WorkspaceSettingsDefinitions.RemovedExtensionsProperty, Future, WorkspaceSettingsDefinitions.RemovedLibrariesProperty],
            json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal([Templates, Alpha, Zebra], Strings(json, WorkspaceSettingsDefinitions.RemovedCategoriesProperty));
        Assert.Equal([WorkspaceLayoutNames.AgentHostFileName, FileA, FileZ], Strings(json, WorkspaceSettingsDefinitions.RemovedFilesProperty));
        Assert.Equal([OldDirectory, NewDirectory, OldDirectoryZ], Strings(json, WorkspaceSettingsDefinitions.RemovedDirectoriesProperty));
        Assert.Equal([BaseExtension, CoreExtension, PlanningExtension], Strings(json, WorkspaceSettingsDefinitions.RemovedExtensionsProperty));
        Assert.Equal([LibraryA, LibraryB, LibraryBase], Strings(json, WorkspaceSettingsDefinitions.RemovedLibrariesProperty));
        Assert.True(json.RootElement.GetProperty(First).GetProperty(Keep).GetBoolean());
        Assert.True(json.RootElement.GetProperty(Future).GetProperty(Nested)[2].GetProperty(Keep).GetBoolean());

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
            new WorkspaceRemovalSelection { Directories = [Obsolete] });

        using var json = JsonDocument.Parse(Assert.IsType<byte[]>(written));
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaUrl,
            json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaProperty).GetString());
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        Assert.Equal([Obsolete], Strings(json, WorkspaceSettingsDefinitions.RemovedDirectoriesProperty));
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
            "directory" => new WorkspaceRemovalSelection { Directories = [WorkspaceLayout.DefaultMainRoot] },
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
