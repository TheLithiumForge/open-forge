using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Route.List.Models;

[JsonConverter(typeof(OpenForge.Cli.Core.Presentation.Route.List.Shared.Rendering.RouteListDataJsonConverter))]
internal sealed record RouteListData
{
    public RouteListDataSubject? Subject { get; init; }

    public RouteListDataDepth? Depth { get; init; }

    public required IReadOnlyList<RouteListDataRow> Rows { get; init; }

    [JsonIgnore]
    internal bool ShowPaths { get; init; }

    [JsonIgnore]
    internal bool ShowDetails { get; init; }

    [JsonIgnore]
    internal bool ShowTrailer { get; init; }

    [JsonIgnore]
    internal string? Trailer { get; init; }

    [JsonIgnore]
    internal bool SeparateRows { get; init; }
}

internal sealed record RouteListDataSubject
{
    public required string Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record RouteListDataDepth
{
    internal required RouteListPresentationDepth Value { get; init; }
}

internal sealed record RouteListDataRow
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string Description { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public required int RelativeDepth { get; init; }

    [JsonIgnore]
    internal required RouteListPresentationRow Result { get; init; }
}
