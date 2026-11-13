using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using System.Text;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    internal static byte[] SetTools(ReadOnlyMemory<byte> existing, IReadOnlyDictionary<string, WorkspaceToolSetting> tools,
        IReadOnlyDictionary<string, ToolRunSummary> runs)
    {
        var settings = ReadSettingsObject(existing);
        settings[WorkspaceSettingsDefinitions.ToolsProperty] = JsonSerializer.SerializeToNode(
            tools.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToDictionary(), WorkspaceToolsJsonContext.Default.ToolSettings);
        if (runs.Count != 0)
            settings[WorkspaceSettingsDefinitions.ToolRunsProperty] = JsonSerializer.SerializeToNode(
                runs.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToDictionary(), WorkspaceToolsJsonContext.Default.ToolRunSummaries);
        else settings.Remove(WorkspaceSettingsDefinitions.ToolRunsProperty);
        var bytes = WriteSettingsObject(settings);
        var decoded = Read(bytes);
        if (decoded.Document is null) throw new JsonException(decoded.Cause);
        return bytes;
    }

    private static ImmutableDictionary<string, ToolRunSummary> ReadToolRuns(JsonElement root)
    {
        if (root.EnumerateObject().Count(property => property.NameEquals(WorkspaceSettingsDefinitions.ToolRunsProperty)) > 1)
            throw new JsonException("The toolRuns map is declared more than once.");
        if (!root.TryGetProperty(WorkspaceSettingsDefinitions.ToolRunsProperty, out var element)) return ImmutableDictionary<string, ToolRunSummary>.Empty;
        var bytes = Encoding.UTF8.GetBytes(element.GetRawText());
        JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(bytes);
        var runs = JsonSerializer.Deserialize(bytes, WorkspaceToolsJsonContext.Default.ToolRunSummaries) ?? throw new JsonException("The toolRuns map cannot be null.");
        ToolLifecycleValidator.Validate(runs);
        return runs.ToImmutableDictionary(StringComparer.Ordinal);
    }

    private static ImmutableDictionary<string, WorkspaceToolSetting> ReadTools(JsonElement root)
    {
        if (root.EnumerateObject().Count(property => property.NameEquals(WorkspaceSettingsDefinitions.ToolsProperty)) > 1)
            throw new JsonException("The tools map is declared more than once.");
        if (!root.TryGetProperty(WorkspaceSettingsDefinitions.ToolsProperty, out var element)) return ImmutableDictionary<string, WorkspaceToolSetting>.Empty;
        var bytes = System.Text.Encoding.UTF8.GetBytes(element.GetRawText());
        JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(bytes);
        var tools = JsonSerializer.Deserialize(bytes, WorkspaceToolsJsonContext.Default.ToolSettings) ?? throw new JsonException("The tools map cannot be null.");
        var roles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (id, tool) in tools)
        {
            ToolManifestValidator.Require(ToolIdentity.IsToolId(id) && tool is not null && ToolIdentity.IsVersion(tool.Version), "invalid installed tool ID or pin");
            ToolManifestValidator.Distinct(tool.Roles, "tool roles");
            ToolManifestValidator.Require(tool.Roles.All(role => role == ToolVocabulary.CodeNavigationRole && roles.Add(role)), "reserved or duplicate active role");
            if (tool.Hosts is { } hosts)
            {
                ToolManifestValidator.Distinct(hosts, "tool hosts");
                ToolManifestValidator.Require(hosts.All(ToolVocabulary.Hosts.Contains), "unknown tool host");
            }
            if (tool.CodeRoot is { } codeRoot) ToolManifestValidator.Require(codeRoot == "." || ToolIdentity.IsRelativePath(codeRoot), "invalid code root");
        }
        return tools.ToImmutableDictionary(StringComparer.Ordinal);
    }
}
