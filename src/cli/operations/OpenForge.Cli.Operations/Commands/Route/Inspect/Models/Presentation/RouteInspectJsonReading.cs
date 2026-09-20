namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonReading
{
    public required RouteInspectJsonBooleanFact TaskStart { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonAutomaticReadings> Automatic { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonLaterReading> Later { get; init; }
}

internal sealed class RouteInspectJsonAutomaticReadings
{
    public required RouteInspectJsonAutomaticReading[] Reasons { get; init; }
}

internal sealed class RouteInspectJsonAutomaticReading
{
    public required string Kind { get; init; }

    public required string? RelatedSourceId { get; init; }

    public required string[] Events { get; init; }
}

internal sealed class RouteInspectJsonLaterReading
{
    public required bool MayBeReadAgain { get; init; }

    public required string[] Occasions { get; init; }
}
