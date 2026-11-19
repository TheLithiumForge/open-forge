using System.Text.Json.Nodes;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    /// <summary>The settings with <paramref name="id"/> added to <c>tools</c>, or null when it's already there.</summary>
    internal static byte[]? AddTool(ReadOnlyMemory<byte> existing, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var settings = ReadSettingsObject(existing);
        var tools = ToolArray(settings) ?? [];
        if (tools.Any(value => value?.GetValue<string>() == id)) return null;
        // The generic JsonArray.Add<T> isn't AOT-safe. The list interface takes a built node.
        ((IList<JsonNode?>)tools).Add(JsonValue.Create(id));
        settings[WorkspaceSettingsDefinitions.ToolsProperty] = tools;
        // The tools list exists from schema v6, so a file that gains it declares that shape.
        settings[WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl;
        settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion;
        return WriteSettingsObject(settings);
    }

    /// <summary>The settings without <paramref name="id"/> in <c>tools</c>, or null when it isn't there.</summary>
    internal static byte[]? RemoveTool(ReadOnlyMemory<byte> existing, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var settings = ReadSettingsObject(existing);
        if (ToolArray(settings) is not { } tools) return null;
        var removed = false;
        for (var index = tools.Count - 1; index >= 0; index--)
        {
            if (tools[index]?.GetValue<string>() != id) continue;
            tools.RemoveAt(index);
            removed = true;
        }
        if (!removed) return null;
        if (tools.Count == 0) settings.Remove(WorkspaceSettingsDefinitions.ToolsProperty);
        return WriteSettingsObject(settings);
    }

    private static JsonArray? ToolArray(JsonObject settings)
    {
        if (!settings.ContainsKey(WorkspaceSettingsDefinitions.ToolsProperty)) return null;
        return settings[WorkspaceSettingsDefinitions.ToolsProperty] as JsonArray
            ?? throw new ArgumentException($"\"{WorkspaceSettingsDefinitions.ToolsProperty}\" must be an array of strings.");
    }
}
