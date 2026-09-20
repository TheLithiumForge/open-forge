namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonObservation
{
    public required string Code { get; init; }

    public required string Subject { get; init; }

    public required string Message { get; init; }

    public required string[] Paths { get; init; }
}

internal sealed class RouteInspectJsonCondition
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string Subject { get; init; }

    public required string Message { get; init; }

    public required string[] Paths { get; init; }
}
