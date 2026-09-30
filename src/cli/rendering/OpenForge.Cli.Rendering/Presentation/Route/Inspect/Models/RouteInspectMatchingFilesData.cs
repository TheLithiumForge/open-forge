using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Models;

internal sealed record RouteInspectMatchingFilesData
{
    internal const string AllFilesScope = "all-files";

    public required string? Scope { get; init; }

    public required bool Complete { get; init; }

    public required int? Count { get; init; }

    public required IReadOnlyList<string> Paths { get; init; }

    public required int PathLimit { get; init; }

    public required bool Truncated { get; init; }

    public required string? Reason { get; init; }

    public required string? Note { get; init; }

    [JsonIgnore]
    internal string? ScopeDescription { get; init; }

    [JsonIgnore]
    internal string? Limitation { get; init; }
}
