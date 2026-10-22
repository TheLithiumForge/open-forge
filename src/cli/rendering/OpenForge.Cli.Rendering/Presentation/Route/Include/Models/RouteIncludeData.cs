namespace OpenForge.Cli.Core.Presentation.Route.Include.Models;

internal sealed record RouteIncludeData
{
    public required string Mode { get; init; }
    public required string Target { get; init; }
    public string? Before { get; init; }
    public string? After { get; init; }
    public required bool RegisteredBefore { get; init; }
    public required bool RegisteredAfter { get; init; }
    public required bool Verified { get; init; }
}
