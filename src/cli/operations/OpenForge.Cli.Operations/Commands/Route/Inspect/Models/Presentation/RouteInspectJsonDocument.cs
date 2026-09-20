namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RouteInspectJsonWorkspace? Workspace { get; init; }

    public required RouteInspectJsonResult Result { get; init; }

    public required RouteInspectJsonNext? Next { get; init; }
}

internal sealed class RouteInspectJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class RouteInspectJsonResult
{
    public required RouteInspectJsonSelection Selection { get; init; }

    public required RouteInspectJsonIdentity? Identity { get; init; }

    public required RouteInspectJsonProfile? Profile { get; init; }

    public required RouteInspectJsonObservation[] Observations { get; init; }

    public required RouteInspectJsonCondition[] Conditions { get; init; }
}

internal sealed class RouteInspectJsonSelection
{
    public required string ReferenceKind { get; init; }

    public required string SelectionMethod { get; init; }

    public required string? RequestedReference { get; init; }

    public required string[] CandidatePaths { get; init; }
}

internal sealed class RouteInspectJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
