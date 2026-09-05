namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal enum RouteRemovePlanRevalidationState
{
    Exact,
    Changed,
    Failed,
    Interrupted,
}

internal sealed record RouteRemovePlanRevalidation
{
    internal RouteRemovePlanRevalidation(
        RouteRemovePlanRevalidationState state,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Remove plan-revalidation state is not defined.");
        }

        if (state == RouteRemovePlanRevalidationState.Exact && cause is not null
            || state != RouteRemovePlanRevalidationState.Exact && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "The Route Remove plan-revalidation cause must match its state.",
                nameof(cause));
        }

        State = state;
        Cause = cause;
    }

    internal RouteRemovePlanRevalidationState State { get; }

    internal string? Cause { get; }
}
