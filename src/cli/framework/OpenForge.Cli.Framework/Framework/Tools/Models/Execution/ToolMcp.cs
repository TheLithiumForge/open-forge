using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Execution;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolMcp
{
    public required string Transport { get; init; }

    public required string Command { get; init; }

    public required string[] Args { get; init; }

    public required string Cwd { get; init; }

    public required Dictionary<string, string> Environment { get; init; }

    public required Dictionary<string, ToolHostTemplate> Hosts { get; init; }
}
