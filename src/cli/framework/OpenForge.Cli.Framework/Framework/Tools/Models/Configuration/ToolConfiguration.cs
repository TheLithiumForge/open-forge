using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolConfiguration
{
    public required string Template { get; init; }

    public required string CodeRoot { get; init; }

    public required string Documents { get; init; }

    public required Dictionary<string, JsonElement> Settings { get; init; }

    public required ToolConfigFile[] Files { get; init; }

    public required string[] Actions { get; init; }
}
