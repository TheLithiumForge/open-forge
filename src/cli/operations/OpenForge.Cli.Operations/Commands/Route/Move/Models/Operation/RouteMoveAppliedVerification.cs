using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;

internal enum RouteMoveAppliedVerificationState
{
    Verified,
    Failed,
    Interrupted,
}

internal sealed record RouteMoveAppliedVerificationInput
{
    public required RouteMovePlan Plan { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required RouteMoveApplicationProgress Progress { get; init; }
}

internal sealed record RouteMoveAppliedVerification
{
    internal RouteMoveAppliedVerification(
        RouteMoveAppliedVerificationState state,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Move applied-verification state is not defined.");
        }

        if (state == RouteMoveAppliedVerificationState.Verified && cause is not null
            || state != RouteMoveAppliedVerificationState.Verified && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "The Route Move applied-verification cause must match its state.",
                nameof(cause));
        }

        State = state;
        Cause = cause;
    }

    internal RouteMoveAppliedVerificationState State { get; }

    internal string? Cause { get; }
}
