using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolLifecycle
{
    public required ToolLifecycleCommand Install { get; init; }
    public required ToolLifecycleCommand Uninstall { get; init; }
    public required ToolLifecycleCommand IndexCleanup { get; init; }
    public required string[] IgnorePaths { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolLifecycleCommand
{
    public required string[] Args { get; init; }
    public required string Scope { get; init; }
    public required string[] WorkspaceOutputs { get; init; }
    public required int TimeoutSeconds { get; init; }
}
