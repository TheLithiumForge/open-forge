using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Models;

internal sealed record RouteRemoveData
{
    public required string Mode { get; init; }

    public required string Subject { get; init; }

    public required RouteRemoveDataSource Source { get; init; }

    public required IReadOnlyList<string> Removed { get; init; }

    public required IReadOnlyList<RouteRemoveDataDetachedLink> DetachedLinks { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteRemoveDataScan? Scan { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextRows { get; init; } = [];

    [JsonIgnore]
    internal IReadOnlyList<string> TextDetailLines { get; init; } = [];

    [JsonIgnore]
    internal bool ShowNoChanges { get; init; }
}

internal sealed record RouteRemoveDataSource
{
    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteRemoveDataDetachedLink
{
    public required string Path { get; init; }

    public required string Location { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Before { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? After { get; init; }
}

internal sealed record RouteRemoveDataScan
{
    public required int FilesScanned { get; init; }

    public required int Occurrences { get; init; }
}
