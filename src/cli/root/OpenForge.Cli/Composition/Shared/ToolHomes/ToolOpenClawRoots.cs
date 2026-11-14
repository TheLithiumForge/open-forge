using System.Text.Json;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

internal static class ToolOpenClawRoots
{
    internal const string DefaultProfile = "default";
    internal const string ConfigurationFile = "openclaw.json";
    internal const string AgentsProperty = "agents";
    internal const string DefaultsProperty = "defaults";
    internal const string WorkspaceProperty = "workspace";
    internal const string IncludeProperty = "$include";

    internal static string Configuration(ToolHomeEnvironment environment, string home)
    {
        var internalHome = InternalHome(environment, home);
        if (environment.Nonempty(ToolHomeEnvironment.OpenClawConfig) is { } custom) return Expand(custom, internalHome);
        var state = environment.Nonempty(ToolHomeEnvironment.OpenClawState) is { } stateValue
            ? Expand(stateValue, internalHome)
            : ToolHomePaths.Join(internalHome, $".openclaw{ProfileSuffix(environment)}");
        return ToolHomePaths.Join(state, ConfigurationFile);
    }

    internal static string Workspace(ToolHomeEnvironment environment, string home, string configuration)
    {
        var internalHome = InternalHome(environment, home);
        if (ToolClientConfigurationReader.Read(configuration) is { } text)
        {
            using var json = JsonDocument.Parse(text, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new IOException("The native OpenClaw configuration requires an object.");
            if (Member(root, AgentsProperty) is { } agents && Member(agents, DefaultsProperty) is { } defaults
                && Member(defaults, WorkspaceProperty) is { } workspace)
            {
                if (workspace.ValueKind != JsonValueKind.String || workspace.GetString() is not { Length: > 0 } value)
                    throw new IOException("The native OpenClaw workspace requires a nonempty path string.");
                return Expand(value, internalHome);
            }
            if (Member(root, IncludeProperty) is not null)
                throw new IOException("The native OpenClaw workspace is unavailable through an include.");
        }
        if (environment.Nonempty(ToolHomeEnvironment.OpenClawWorkspace) is { } custom) return Expand(custom, internalHome);
        return ToolHomePaths.Join(internalHome, $".openclaw/workspace{ProfileSuffix(environment)}");
    }

    private static string InternalHome(ToolHomeEnvironment environment, string home)
        => environment.Nonempty(ToolHomeEnvironment.OpenClawHome) is { } value ? Expand(value, home) : home;

    private static string Expand(string value, string home) => ToolHomeEnvironment.Expand(value, home)
        ?? throw new IOException("The native OpenClaw path must be absolute or home-relative.");

    private static string ProfileSuffix(ToolHomeEnvironment environment)
    {
        var profile = environment.Nonempty(ToolHomeEnvironment.OpenClawProfile);
        if (profile is null or DefaultProfile) return string.Empty;
        return $"-{ToolHomeEnvironment.Profile(profile)}";
    }

    private static JsonElement? Member(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object) throw new IOException("The native OpenClaw path member requires an object.");
        JsonElement? found = null;
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name != name) continue;
            if (found is not null) throw new IOException("The native OpenClaw path member is duplicated.");
            found = property.Value;
        }
        return found;
    }
}
