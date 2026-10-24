using System.Text.Json;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    internal static byte[]? SetMainRoot(ReadOnlyMemory<byte> existing, string mainRoot)
    {
        if (!WorkspaceMainRootPath.IsCanonical(mainRoot))
            throw new ArgumentException("The main root must be a canonical workspace content directory.", nameof(mainRoot));

        var settings = ReadSettingsObject(existing);
        var current = settings[WorkspaceSettingsDefinitions.MainRootProperty]?.GetValue<string>() ?? WorkspaceLayout.DefaultMainRoot;
        if (current == mainRoot) return null;

        if (mainRoot == WorkspaceLayout.DefaultMainRoot)
        {
            settings.Remove(WorkspaceSettingsDefinitions.MainRootProperty);
        }
        else
        {
            settings[WorkspaceSettingsDefinitions.MainRootProperty] = mainRoot;
            settings[WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl;
            settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion;
        }

        var bytes = WriteSettingsObject(settings);
        var decoded = Read(bytes);
        if (decoded.Document is null) throw new ArgumentException(decoded.Cause, nameof(mainRoot));
        return bytes;
    }

    private static string? ReadDeclaredMainRoot(JsonElement root)
    {
        if (!root.TryGetProperty(WorkspaceSettingsDefinitions.MainRootProperty, out var value)) return null;
        var mainRoot = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        if (!WorkspaceMainRootPath.IsCanonical(mainRoot))
            throw new ArgumentException($"\"{WorkspaceSettingsDefinitions.MainRootProperty}\" must be a canonical workspace content directory.");
        return mainRoot;
    }
}
