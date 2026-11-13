using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
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
