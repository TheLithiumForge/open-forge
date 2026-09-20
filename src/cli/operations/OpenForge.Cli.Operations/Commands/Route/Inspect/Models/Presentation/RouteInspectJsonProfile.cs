namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonProfile
{
    public required RouteInspectJsonReading Reading { get; init; }

    public required RouteInspectJsonMeasurements Measurements { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonTopology> Topology { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonAxioms> Axioms { get; init; }

    public required string Completeness { get; init; }

    public required string Safety { get; init; }
}
