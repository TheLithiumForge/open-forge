namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;

internal enum RouteCreatePlanRevalidationState
{
    Exact,
    Changed,
    Cancelled,
    Failed,
}

internal sealed record RouteCreatePlanRevalidation
{
    public required RouteCreatePlanRevalidationState State { get; init; }

    public string? Cause { get; init; }
}
