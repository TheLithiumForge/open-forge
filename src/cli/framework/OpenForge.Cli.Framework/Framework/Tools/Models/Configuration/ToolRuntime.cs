using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolRuntime
{
    public required string Kind { get; init; }

    public required string? Version { get; init; }

    public required string Notes { get; init; }

    public required string[] ExtraDownloads { get; init; }

    public required string InstallScripts { get; init; }
}
