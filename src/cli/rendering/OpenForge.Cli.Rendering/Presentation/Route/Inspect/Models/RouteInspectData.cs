using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Models;

internal sealed record RouteInspectData
{
    public string? Id { get; init; }

    public string? Path { get; init; }

    public string? Kind { get; init; }

    public string? EntrypointForm { get; init; }

    public string? OverwritePath { get; init; }

    public RouteInspectDataBelongs? Belongs { get; init; }

    public RouteInspectDataRead? Read { get; init; }

    public RouteInspectDataSize? Size { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteInspectDataAxioms? Axioms { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Tags { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteInspectDataSelected? Selected { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RouteInspectDataSelection? Selection { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<RouteInspectDataLayer>? Layers { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StatusReason { get; init; }

    [JsonIgnore]
    internal RouteInspectIdentity? Identity { get; init; }

    [JsonIgnore]
    internal RouteInspectProfile? Profile { get; init; }
}

internal sealed record RouteInspectDataBelongs
{
    public IReadOnlyList<string>? RouteChain { get; init; }

    public string? Parent { get; init; }

    public RouteInspectDataCounts? DirectChildren { get; init; }

    public RouteInspectDataCounts? Descendants { get; init; }
}

internal sealed record RouteInspectDataRead
{
    public bool? AtStart { get; init; }

    public IReadOnlyList<string>? AutomaticallyWhen { get; init; }

    public bool? MayReadAgain { get; init; }
}

internal sealed record RouteInspectDataSize
{
    public RouteInspectDataMeasurement? Own { get; init; }

    public RouteInspectDataMeasurement? Adds { get; init; }

    public RouteInspectDataMeasurement? LoadNow { get; init; }
}

internal sealed record RouteInspectDataMeasurement
{
    public long Files { get; init; }

    public long Bytes { get; init; }

    public long Tokens { get; init; }
}

internal sealed record RouteInspectDataCounts
{
    public int Files { get; init; }

    public int Entrypoints { get; init; }
}

internal sealed record RouteInspectDataAxioms
{
    public IReadOnlyList<string>? InheritedFrom { get; init; }

    public string? Local { get; init; }
}

internal sealed record RouteInspectDataSelected
{
    public RouteInspectDataMeasurement? Closure { get; init; }

    public RouteInspectDataMeasurement? StartupOverlap { get; init; }
}

internal sealed record RouteInspectDataSelection
{
    public required string Kind { get; init; }

    public required string Method { get; init; }

    public string? Requested { get; init; }
}

internal sealed record RouteInspectDataLayer
{
    public required string Path { get; init; }

    public required string Kind { get; init; }
}
