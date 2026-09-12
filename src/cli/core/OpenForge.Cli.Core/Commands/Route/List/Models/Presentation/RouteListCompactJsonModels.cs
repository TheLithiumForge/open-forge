namespace OpenForge.Cli.Core.Commands.Route.List.Models.Presentation;

internal sealed class RouteListCompactJsonResult
{
    public required RouteListJsonSelection Selection { get; init; }
    public required RouteListJsonDepth? RequestedDepth { get; init; }
    public required RouteListJsonDepth? EffectiveDepth { get; init; }
    public required RouteListJsonCoverage Coverage { get; init; }
    public required RouteListJsonFinding[] Findings { get; init; }
    public required RouteListCompactJsonRow[] Rows { get; init; }
}

internal sealed class RouteListCompactJsonRow
{
    public required string Id { get; init; }
    public required string Path { get; init; }
    public required string? ParentId { get; init; }
    public required string? ParentPath { get; init; }
    public required int? AbsoluteDepth { get; init; }
    public required int RelativeDepth { get; init; }
    public required string Kind { get; init; }
    public required string Description { get; init; }
    public required string[] Tags { get; init; }
    public required int? DirectChildCount { get; init; }
}
