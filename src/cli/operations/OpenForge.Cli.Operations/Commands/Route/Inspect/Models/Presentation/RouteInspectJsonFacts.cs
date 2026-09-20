namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonFact<T>
    where T : class
{
    public required string State { get; init; }

    public required T? Value { get; init; }

    public required string? Reason { get; init; }
}

internal sealed class RouteInspectJsonBooleanFact
{
    public required string State { get; init; }

    public required bool? Value { get; init; }

    public required string? Reason { get; init; }
}
