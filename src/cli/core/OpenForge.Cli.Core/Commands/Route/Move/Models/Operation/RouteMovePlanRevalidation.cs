namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal enum RouteMovePlanRevalidationState
{
    Exact,
    Changed,
    Failed,
    Interrupted,
}

internal sealed record RouteMovePlanRevalidation
{
    internal RouteMovePlanRevalidation(
        RouteMovePlanRevalidationState state,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Move plan-revalidation state is not defined.");
        }

        if (state == RouteMovePlanRevalidationState.Exact && cause is not null
            || state != RouteMovePlanRevalidationState.Exact && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "The Route Move plan-revalidation cause must match its state.",
                nameof(cause));
        }

        State = state;
        Cause = cause;
    }

    internal RouteMovePlanRevalidationState State { get; }

    internal string? Cause { get; }
}
