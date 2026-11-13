using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolSkill
{
    public required string Template { get; init; }

    public required bool Optional { get; init; }

    public required bool DefaultEnabled { get; init; }
}
