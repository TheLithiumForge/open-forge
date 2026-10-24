using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    internal static byte[]? AddOutsideRoute(ReadOnlyMemory<byte> existing, string directoryPath)
    {
        var settings = ReadSettingsObject(existing);
        var mainRoot = settings[WorkspaceSettingsDefinitions.MainRootProperty]?.GetValue<string>() ?? WorkspaceLayout.DefaultMainRoot;
        var directories = ReadOutsideRouteValues(settings);
        if (directories.Contains(directoryPath, StringComparer.Ordinal)) return null;
        ValidateOutsideRoutes(mainRoot, [.. directories, directoryPath]);

        var routes = settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] as JsonArray ?? [];
        ((IList<JsonNode?>)routes).Add(JsonValue.Create(directoryPath));
        settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] = routes;
        settings[WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl;
        settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion;
        return WriteSettingsObject(settings);
    }

    internal static byte[]? RemoveOutsideRoute(ReadOnlyMemory<byte> existing, string directoryPath)
    {
        var settings = ReadSettingsObject(existing);
        var mainRoot = settings[WorkspaceSettingsDefinitions.MainRootProperty]?.GetValue<string>() ?? WorkspaceLayout.DefaultMainRoot;
        ValidateOutsideRoutes(mainRoot, [directoryPath]);
        if (settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] is not JsonArray routes) return null;
        for (var index = 0; index < routes.Count; index++)
        {
            if (routes[index]?.GetValue<string>() != directoryPath) continue;
            routes.RemoveAt(index);
            return WriteSettingsObject(settings);
        }
        return null;
    }

    private static ImmutableArray<string> ReadOutsideRouteValues(JsonObject settings)
        => settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] is JsonArray routes
            ? [.. routes.Select(route => route?.GetValue<string>() ?? throw new InvalidOperationException("Validated outside routes require strings."))]
            : [];

    private static ImmutableArray<string> ReadOutsideRoutes(JsonElement root, string mainRoot)
    {
        if (!root.TryGetProperty(WorkspaceSettingsDefinitions.OutsideRoutesProperty, out var value)) return [];
        if (value.ValueKind != JsonValueKind.Array)
            throw new ArgumentException($"{WorkspaceSettingsDefinitions.FileName}: \"{WorkspaceSettingsDefinitions.OutsideRoutesProperty}\" must be an array of canonical workspace-relative directories.");

        var directories = ImmutableArray.CreateBuilder<string>();
        foreach (var entry in value.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.String)
                throw new ArgumentException($"{WorkspaceSettingsDefinitions.FileName}: \"{WorkspaceSettingsDefinitions.OutsideRoutesProperty}\" must contain only directory strings.");
            directories.Add(entry.GetString() ?? string.Empty);
        }
        ValidateOutsideRoutes(mainRoot, directories);
        return directories.ToImmutable();
    }

    private static void ValidateOutsideRoutes(string mainRoot, IEnumerable<string> directories)
    {
        try
        {
            _ = WorkspaceOutsideRoutePath.Validate(mainRoot, directories);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException($"{WorkspaceSettingsDefinitions.FileName}: \"{WorkspaceSettingsDefinitions.OutsideRoutesProperty}\" is invalid. {exception.Message}", exception);
        }
    }
}
