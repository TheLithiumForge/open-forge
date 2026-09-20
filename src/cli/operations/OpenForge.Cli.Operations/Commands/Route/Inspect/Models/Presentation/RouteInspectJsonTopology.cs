namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;

internal sealed class RouteInspectJsonTopology
{
    public required string RootRoute { get; init; }

    public required string[] RouteChain { get; init; }

    public required string? ParentId { get; init; }

    public required int Depth { get; init; }

    public required RouteInspectJsonFact<RouteInspectJsonTopologyCounts> Counts { get; init; }
}

internal sealed class RouteInspectJsonTopologyCounts
{
    public required int DirectRoutedFileCount { get; init; }

    public required int DirectEntrypointCount { get; init; }

    public required int DescendantRoutedFileCount { get; init; }

    public required int DescendantEntrypointCount { get; init; }
}
