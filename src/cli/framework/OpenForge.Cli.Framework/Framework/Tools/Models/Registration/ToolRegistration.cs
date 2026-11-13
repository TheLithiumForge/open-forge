using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Registration;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolRegistration
{
    public required ToolEntryRewrite[] Rewrites { get; init; }
    public required ToolOptionalComponent[] Components { get; init; }
    public required ToolRoleGuidance[] RoleGuidance { get; init; }
    public required ToolLifecycleCommand? Activate { get; init; }
    public required ToolLifecycleCommand? Deactivate { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolOptionalComponent
{
    public required string Id { get; init; }
    public required string Flag { get; init; }
    public required string Description { get; init; }
    public required bool DefaultEnabled { get; init; }
    public required ToolJsonEntryRemoval[] DeclinedEntries { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolJsonEntryRemoval
{
    public required string Host { get; init; }
    public required string File { get; init; }
    public required string Pointer { get; init; }
    public required JsonElement Expected { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool MatchCommand { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool RemoveCreatedEmptyGroup { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonElement>? PlatformOverrides { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? AbsentCommands { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolRoleGuidance([property: JsonRequired] string Role, [property: JsonRequired] string Line);

internal static class ToolComponentFlags
{
    internal const string PromptHook = "prompt-hook";
}
