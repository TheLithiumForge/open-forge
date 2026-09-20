using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;

internal enum RouteRemoveAppliedVerificationState
{
    Verified,
    Failed,
    Interrupted,
}

internal sealed record RouteRemoveAppliedVerificationInput
{
    public required RouteRemovePlan Plan { get; init; }

    public required WorkspaceLockLease Lease { get; init; }

    public required RouteRemoveApplicationProgress Progress { get; init; }
}

internal sealed record RouteRemoveAppliedVerification
{
    internal RouteRemoveAppliedVerification(
        RouteRemoveAppliedVerificationState state,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Route Remove applied-verification state is not defined.");
        }

        if (state == RouteRemoveAppliedVerificationState.Verified && cause is not null
            || state != RouteRemoveAppliedVerificationState.Verified && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "The Route Remove applied-verification cause must match its state.",
                nameof(cause));
        }

        State = state;
        Cause = cause;
    }

    internal RouteRemoveAppliedVerificationState State { get; }

    internal string? Cause { get; }
}
