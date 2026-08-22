namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonAxioms
{
    public required RouteInspectJsonFact<RouteInspectJsonAxiomsSources> Inherited { get; init; }

    public required RouteInspectJsonFact<string> Local { get; init; }
}

internal sealed class RouteInspectJsonAxiomsSources
{
    public required string[] SourceIds { get; init; }
}
