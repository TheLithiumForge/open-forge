using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Status.Models;

internal sealed record StatusDataOutsideRoute
{
    public required string Path { get; init; }

    public required string State { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Listed { get; init; }
}
