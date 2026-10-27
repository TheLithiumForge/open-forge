using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Exclude.Models;

internal sealed record RouteExcludeData
{
    public required string Mode { get; init; }
    public required string Target { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TargetKind { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Catalogue { get; init; }
    public string? Before { get; init; }
    public string? After { get; init; }
    public required bool RegisteredBefore { get; init; }
    public required bool RegisteredAfter { get; init; }
    public required bool Verified { get; init; }
}
