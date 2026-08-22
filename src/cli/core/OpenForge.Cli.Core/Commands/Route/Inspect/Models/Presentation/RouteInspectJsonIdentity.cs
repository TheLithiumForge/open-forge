namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonIdentity
{
    public required string Id { get; init; }

    public required string Path { get; init; }

    public required string SourceKind { get; init; }

    public required string SourceForm { get; init; }

    public required string RouteState { get; init; }

    public required RouteInspectJsonPhysicalLayer[] PhysicalLayers { get; init; }
}

internal sealed class RouteInspectJsonPhysicalLayer
{
    public required string WorkspaceRelativePath { get; init; }

    public required string PhysicalPath { get; init; }

    public required string Role { get; init; }
}
