using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Document;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record WorkspaceToolSetting
{
    public required string Version { get; init; }
    public required string[] Roles { get; init; }
    public string[]? Hosts { get; init; }
    public string? CodeRoot { get; init; }
}
