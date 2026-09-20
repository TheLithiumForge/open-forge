namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonMeasurements
{
    public required RouteInspectJsonFact<RouteInspectJsonMeasurement> OwnSource { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonMeasurement> SelectedClosure { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonMeasurement> TaskStartOverlap { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonMeasurement> SelectionAddition { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonMeasurement> LoadNowDescendants { get; init; }
}

internal sealed class RouteInspectJsonMeasurement
{
    public required long PhysicalFileCount { get; init; }

    public required long UnicodeScalarCount { get; init; }

    public required long Utf8ByteCount { get; init; }

    public required long EstimatedTokens { get; init; }
}
