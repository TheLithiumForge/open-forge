namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;

internal enum RouteUpdatePlanRevalidationState
{
    Exact,
    Changed,
    Cancelled,
    Failed,
}

internal sealed record RouteUpdatePlanRevalidation
{
    public required RouteUpdatePlanRevalidationState State { get; init; }

    public string? Cause { get; init; }
}
