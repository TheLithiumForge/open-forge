using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Serialization;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class WorkspaceSettingsToolsTests
{
    [Fact(DisplayName = "Adding a tool to empty settings writes the tools list with the current schema")]
    public void AddsToEmptySettings()
    {
        var bytes = WorkspaceSettingsCodec.AddTool(ReadOnlyMemory<byte>.Empty, CodeGraphEntry.Id)!;

        var document = WorkspaceSettingsCodec.Read(bytes).Document!;
        Assert.Equal([CodeGraphEntry.Id], document.Tools);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, document.SchemaVersion);
    }

    [Fact(DisplayName = "Adding a listed tool changes nothing, and other keys survive adding and removing")]
    public void KeepsOtherKeys()
    {
        var settings = new JsonObject
        {
            [WorkspaceSettingsDefinitions.AllowInstallPathsProperty] = new JsonArray(JsonValue.Create(WorkspaceSettingsDefinitions.MemoryArchivePath)),
        };
        var added = WorkspaceSettingsCodec.AddTool(System.Text.Encoding.UTF8.GetBytes(settings.ToJsonString()), CodebaseMemoryEntry.Id)!;

        Assert.Null(WorkspaceSettingsCodec.AddTool(added, CodebaseMemoryEntry.Id));
        var removed = WorkspaceSettingsCodec.RemoveTool(added, CodebaseMemoryEntry.Id)!;
        var document = WorkspaceSettingsCodec.Read(removed).Document!;
        Assert.Empty(document.Tools);
        Assert.Equal([WorkspaceSettingsDefinitions.MemoryArchivePath], document.AllowInstallPaths);
        Assert.False(JsonNode.Parse(removed)!.AsObject().ContainsKey(WorkspaceSettingsDefinitions.ToolsProperty));
        Assert.Null(WorkspaceSettingsCodec.RemoveTool(removed, CodebaseMemoryEntry.Id));
    }
}
