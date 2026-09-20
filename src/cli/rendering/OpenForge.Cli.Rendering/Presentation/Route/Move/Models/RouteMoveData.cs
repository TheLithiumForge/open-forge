using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Models;

internal sealed record RouteMoveData
{
    public required string Mode { get; init; }

    public required string Subject { get; init; }

    public required RouteMoveDataSource Source { get; init; }

    public required RouteMoveDataDestination Destination { get; init; }

    public required IReadOnlyList<RouteMoveDataMoved> Moved { get; init; }

    public required IReadOnlyList<RouteMoveDataRewrittenLink> RewrittenLinks { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteMoveDataScan? Scan { get; init; }

    [JsonIgnore]
    internal IReadOnlyList<string> TextRows { get; init; } = [];
}

internal sealed record RouteMoveDataSource
{
    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteMoveDataDestination
{
    public string? Id { get; init; }

    public string? Path { get; init; }
}

internal sealed record RouteMoveDataMoved
{
    public required string From { get; init; }

    public required string To { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteMoveDataOverwrite? Overwrite { get; init; }
}

internal sealed record RouteMoveDataOverwrite
{
    public required string From { get; init; }

    public required string To { get; init; }
}

internal sealed record RouteMoveDataRewrittenLink
{
    public required string Path { get; init; }

    public required string Location { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteMoveDataDestination? From { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteMoveDataDestination? To { get; init; }
}

internal sealed record RouteMoveDataScan
{
    public required int FilesScanned { get; init; }

    public required int Occurrences { get; init; }
}
